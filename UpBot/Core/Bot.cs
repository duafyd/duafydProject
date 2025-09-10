using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using UpBot.Core;
using UpBot.Models;
using UpBot.Models.Api;
using Timer = System.Timers.Timer;

namespace UpBot.Services;

public class Bot
{
    /// <summary>
    /// 관리 종목 수
    /// </summary>
    public int StockCount => 1;

    /// <summary>
    /// 목표 수익률(%) - 5분봉용
    /// </summary>
    public decimal ProfitTargetPercent => 1.0m;

    /// <summary>
    /// 손절 수익률(%) - 5분봉용
    /// </summary>
    public decimal StopLossPercent => -0.8m;

    /// <summary>
    /// 트레일링 시작 수익률(%) - 이 이상 이익에서만 드로우다운 감시
    /// </summary>
    public decimal TrailStartPercent => 1.0m;

    /// <summary>
    /// 트레일링 드로우다운 폭(%) - 최대이익에서 이만큼 반납 시 매도
    /// </summary>
    public decimal TrailDrawdownPercent => 0.6m;

    /// <summary>
    /// 매매 후 동일 종목 쿨다운 시간
    /// </summary>
    public TimeSpan CooldownAfterTrade => TimeSpan.FromMinutes(30);

    private readonly ApiService Api;
    private readonly AppDataService AppData;
    private readonly DatabaseService Database;

    private readonly Timer _buyTimer;
    private bool _isBuying;

    private readonly Timer _sellTimer;
    private bool _isSelling;

    public decimal TotalBalance { get; private set; }

    public List<Market> Markets { get; private set; } = new();

    // 봉 마감 체크용(마켓+단위 -> 마지막 처리한 봉 KST 시각)
    private readonly Dictionary<string, string> _lastProcessedCandleKst = new();

    // 트레일링용(마켓 -> 관측된 최대 수익률)
    private readonly Dictionary<string, decimal> _peakProfitPercent = new();

    // 쿨다운(마켓 -> 쿨다운 만료 시각 UTC)
    private readonly Dictionary<string, DateTime> _cooldowns = new();

    public Bot()
    {
        Api = App.ServiceProvider.GetRequiredService<ApiService>();
        AppData = App.ServiceProvider.GetRequiredService<AppDataService>();
        Database = App.ServiceProvider.GetRequiredService<DatabaseService>();

        _buyTimer = new Timer(TimeSpan.FromMinutes(1));
        _buyTimer.Elapsed += async (s, e) => await CheckBuyAsync();

        _sellTimer = new Timer(TimeSpan.FromSeconds(2)); // 2초
        _sellTimer.Elapsed += async (s, e) => await CheckSellAsync();
    }

    public async void Start()
    {
        _sellTimer.Start();
        await CheckSellAsync(); // 즉시 실행

        _buyTimer.Start();
        await CheckBuyAsync(); // 즉시 실행        

        Logger.Info("Bot started.");
    }

    public void Stop()
    {
        _buyTimer.Stop();
        _sellTimer.Stop();

        Logger.Info("Bot stopped.");
    }

    private async Task CheckBuyAsync()
    {
        try
        {
            if (_isBuying)
                return;

            _isBuying = true;

            if (!AppData.CashBalance.HasValue)
            {
                return;
            }

            if (AppData.CashBalance < 5000)
            {
                Logger.Warn($"잔고부족({AppData.CashBalance.Value.ToString("C")})");
                return;
            }

            Logger.Debug("Checking buy conditions...");

            // 매수 조건 확인 및 매수 로직 구현          
            if (Markets.Count == 0)
            {
                // 마켓 전체 가져오기
                var markets = await Api.QuotationApi.TradingPairs.GetMarketsAsync();
                if (markets != null)
                    Markets = markets;
            }

            // 유의종목 제외한 KRW 마켓만 필터링
            var goodMarkets = Markets
                //.Where(m => m.market.StartsWith("KRW-") && m.IsSafe)
                .Where(m => m.market.StartsWith("KRW-"))
                .Select(m => m.market)
                .ToList();

            var marketsStr = string.Join(",", goodMarkets);
            // ticker 데이터 가져오기(거래대금)
            var tickers = await Api.QuotationApi.Ticker.GetTickersAsync(marketsStr);

            // 거래대금 상위 20개
            var top20 = tickers?
                .OrderByDescending(t => t.acc_trade_price_24h)
                .Take(20)
                .ToList();

            // 로그 기록            
            var top20Dict = top20?.ToDictionary(t => t.market, t => t.acc_trade_price_24h);
            var top20Json = JsonSerializer.Serialize(top20Dict);
            Logger.Info($"Top 20 Markets by 24h Trade Price: {top20Json}");

            // 상위 20개 종목에 대해 매수 조건 확인
            foreach (var t in top20)
            {
                // 테더 스킵
                if (t.market == "KRW-USDT")
                    continue;

                // 쿨다운 체크
                if (_cooldowns.TryGetValue(t.market, out var untilUtc) && DateTime.UtcNow < untilUtc)
                {
                    Logger.Debug($"Cooldown active for {t.market} until {untilUtc:u}");
                    continue;
                }

                // 요청 제한 회피
                await Task.Delay(100);

                Logger.Info($"Checking should buy {t.market}");

                var unit = CandleMinuteUnitType.Minute5; // 5분봉
                var candles = await Api.QuotationApi.OHLCV.GetCandlesMinutesAsync(
                    market: t.market,
                    count: 200,
                    unit: unit);

                // ShouldBuy는 최소 80봉 필요
                if (candles == null || candles.Count < 80)
                {
                    Logger.Warn($"{t.market}: 캔들 데이터 부족");
                    continue;
                }

                // 봉 마감 시점 체크(새 봉 생성 시에만 평가)
                var firstKst = DateTime.Parse(candles.First().candle_date_time_kst);
                var lastKst = DateTime.Parse(candles.Last().candle_date_time_kst);
                var latestCandle = firstKst >= lastKst ? candles.First() : candles.Last();

                var keyClose = $"{t.market}:{(int)unit}";
                var latestKstStr = latestCandle.candle_date_time_kst;

                if (_lastProcessedCandleKst.TryGetValue(keyClose, out var prevKst) && prevKst == latestKstStr)
                {
                    Logger.Debug($"Skip {t.market} - no new closed candle. last={latestKstStr}");
                    continue;
                }

                // 중복 처리 방지: 같은 봉에서는 재평가하지 않도록 즉시 마킹
                _lastProcessedCandleKst[keyClose] = latestKstStr;

                var isBuySignal = ShouldBuy(candles);
                if (isBuySignal)
                {
                    try
                    {
                        var coinName = GetCoinName(t.market);
                        Logger.Info($"매수 타이밍 확인 for {t.market}/{coinName} at Price {t.trade_price}");
                        // 주문가능 정보 조회
                        var orderChange = await Api.ExchangeApi.Order.GetOrdersChanceAsync(t.market);
                        if (orderChange == null)
                        {
                            Logger.Error($"주문 가능 정보 조회 실패 for {t.market}");
                            continue;
                        }

                        var investAmount = AppData.TotalBalance / StockCount; // 종목당 투자금액
                        var buyAmount = Math.Min(investAmount, AppData.CashBalance.Value);
                        buyAmount = Math.Floor(buyAmount / (1 + decimal.Parse(orderChange.bid_fee))); // 수수료 고려

                        if (buyAmount < 5000)
                        {
                            Logger.Warn($"매수 금액 + 수수료 부족({buyAmount.ToString("C")}) for {t.market}");
                            continue;
                        }

                        // 시장가로 매수
                        var order = await Api.ExchangeApi.Order.PostMarketBuyAsync(t.market, buyAmount.ToString());
                        if (order == null)
                        {
                            Logger.Error($"매수 실패 for {t.market}");
                            continue;
                        }

                        var buyInfo = new TradeHistory
                        {
                            Id = order.uuid,
                            Side = order.side,
                            CoinName = coinName,
                            BuyPrice = order.price,
                            Volume = order.executed_volume,
                        };

                        Database.SaveTradeHistory(buyInfo);

                        // 매수 후 쿨다운 및 트레일링 상태 초기화
                        _cooldowns[t.market] = DateTime.UtcNow + CooldownAfterTrade;
                        _peakProfitPercent[t.market] = 0m;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error buying {t.market}", ex);
                    }
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            // 예외 처리
            Logger.Error("Error in CheckBuyAsync", ex);
        }
        finally
        {
            _isBuying = false;
            Logger.Debug("Finished checking buy conditions.");
        }
    }

    private async Task CheckSellAsync()
    {
        if (_isSelling)
            return;

        _isSelling = true;

        Logger.Debug("Checking sell conditions...");

        if (Markets.Count == 0)
        {
            var markets = await Api.QuotationApi.TradingPairs.GetMarketsAsync();
            if (markets != null)
                Markets = markets;
        }

        try
        {
            var account = await Api.ExchangeApi.Asset.GetAccountsAsync();
            if (account == null || account.Count == 0)
            {
                Logger.Warn("보유 자산 없음");
                return;
            }

            var list = new List<TradeHistory>();

            AppData.CashBalance = account?.FirstOrDefault(a => a.currency == "KRW")?.BalanceDecimal ?? 0;
            foreach (var acc in account!.Where(a => a.currency != "KRW" && a.BalanceDecimal > 0))
            {
                try
                {
                    var (currentPrice, profit, profitPercent) = await GetCurrentPrice(acc);

                    // 최소 매도 가능 금액(약 5,000원) 충족 여부는 '평가금액'으로 판단
                    var evaluation = acc.BalanceDecimal * currentPrice;
                    if (evaluation >= 5000)
                    {
                        var market = $"{acc.unit_currency}-{acc.currency}";
                        var coinName = GetCoinName(market);
                        list.Add(new TradeHistory
                        {
                            CoinName = coinName,
                            BuyPrice = acc.AvgBuyPriceDecimal.ToString("C"),
                            Volume = acc.BalanceDecimal.ToString("G29"),
                            ProfitAmount = profit.ToString("C"),
                            ProfitPercent = profitPercent.ToString("F2") + "%",
                        });

                        // 트레일링: 최대 수익률 갱신 및 드로우다운 체크
                        if (_peakProfitPercent.TryGetValue(market, out var peak))
                        {
                            if (profitPercent > peak)
                            {
                                _peakProfitPercent[market] = profitPercent;
                                peak = profitPercent;
                            }
                        }
                        else
                        {
                            _peakProfitPercent[market] = profitPercent;
                            peak = profitPercent;
                        }

                        bool takeProfit = profitPercent >= ProfitTargetPercent;
                        bool stopLoss = profitPercent <= StopLossPercent;
                        bool trailing = peak >= TrailStartPercent && (peak - profitPercent) >= TrailDrawdownPercent;

                        if (takeProfit || stopLoss || trailing)
                        {
                            var reason = takeProfit ? "TP" : (stopLoss ? "SL" : "TRAIL");
                            Logger.Info($"Sell Signal({reason}) for {acc.currency} at Price {currentPrice}, Profit: {profit} ({profitPercent:F2}%), Peak:{peak:F2}%");

                            var sell = await Api.ExchangeApi.Order.PostMarketSellAsync(market, acc.balance);
                            if (sell != null)
                            {
                                Logger.Info($"매도 완료 for {acc.currency} at Price {currentPrice}, Profit: {profit} ({profitPercent:F2}%)");

                                var sellInfo = new TradeHistory
                                {
                                    Id = sell.uuid,
                                    Side = sell.side,
                                    CoinName = coinName,
                                    BuyPrice = acc.AvgBuyPriceDecimal.ToString("C"),
                                    SellPrice = sell.price,
                                    Volume = sell.executed_volume,
                                    ProfitAmount = profit.ToString("C"),
                                    ProfitPercent = profitPercent.ToString("F2") + "%",
                                };

                                Database.SaveTradeHistory(sellInfo);

                                // 매도 후 쿨다운 및 트레일링 상태 초기화
                                _cooldowns[market] = DateTime.UtcNow + CooldownAfterTrade;
                                _peakProfitPercent.Remove(market);
                            }
                            else
                            {
                                Logger.Error($"매도 실패 for {acc.currency}");
                            }
                        }
                    }
                }
                catch { }
            }

            AppData.CoinStatus = new List<TradeHistory>(list);

        }
        catch (Exception ex)
        {
            Logger.Error("Error in CheckSellAsync", ex);
        }
        finally
        {
            _isSelling = false;
            Logger.Debug("Finished checking sell conditions.");
        }
    }

    /// <summary>
    /// 단타 매수 타이밍 감지(함정 회피형)
    /// - 추세 질(EMA20>EMA60, EMA20 상승) + 건전한 돌파(몸통 비율, EMA 이격 제한)
    /// - RSI 50~68 범위의 모멘텀
    /// - 거래량 스파이크, 적정 ATR 범위
    /// </summary>
    public bool ShouldBuy(List<CandleMinute> candles)
    {
        if (candles == null || candles.Count < 80) return false;

        var closes = candles.Select(c => c.trade_price).ToList();

        var ema20Series = TechnicalIndicators.EMA(closes, 20);
        var ema60Series = TechnicalIndicators.EMA(closes, 60);
        if (ema20Series.Count < 2 || ema60Series.Count == 0) return false;

        var ema20 = ema20Series[^1];
        var ema20Prev = ema20Series[^2];
        var ema60 = ema60Series[^1];

        var last = candles[^1];
        var prev = candles[^2];

        // RSI
        var rsiSeries = TechnicalIndicators.RSI(closes, 14);
        if (rsiSeries.Count == 0) return false;
        var rsi = rsiSeries[^1];

        // 변동성(ATR)
        var atr = ComputeAtr(candles, 14);
        if (atr <= 0) return false;
        var atrPct = atr / (last.trade_price == 0 ? 1 : last.trade_price) * 100m;

        // 거래량 평균(최근 10봉)
        decimal avgVol10 = 0m;
        for (int i = candles.Count - 11; i < candles.Count - 1; i++)
            avgVol10 += candles[i].candle_acc_trade_volume;
        avgVol10 /= 10m;

        // 조건
        bool trendUp = ema20 > ema60 && ema20 > ema20Prev; // 상승 추세 + 기울기 양수
        bool breakout = last.trade_price > prev.high_price; // 직전 고점 돌파
        decimal body = Math.Abs(last.trade_price - last.opening_price);
        decimal range = Math.Max(0.00000001m, last.high_price - last.low_price);
        bool strongBody = (body / range) >= 0.6m; // 몸통 비율
        bool notOverextended = Math.Abs(last.trade_price - ema20) / last.trade_price <= 0.01m; // EMA20 이격 ≤ 1%
        bool rsiOk = rsi >= 50m && rsi <= 68m; // 과열 추격 방지
        bool volOk = last.candle_acc_trade_volume >= (avgVol10 * 1.5m); // 거래량 스파이크
        bool atrOk = atrPct >= 0.2m && atrOkUpper(atrPct); // 과소/과대 변동 구간 배제

        Logger.Debug($"BUYCHK trendUp:{trendUp}, breakout:{breakout}, strongBody:{strongBody}, nearEMA20:{notOverextended}, RSI:{rsi:F2}, volOk:{volOk}, ATR%:{atrPct:F2}");

        return trendUp && breakout && strongBody && notOverextended && rsiOk && volOk && atrOk;

        static bool atrOkUpper(decimal atrPctVal) => atrPctVal <= 3.0m;
    }

    /// <summary>
    /// 보유 코인 가격/손익 계산
    /// </summary>
    public async Task<(decimal currentPrice, decimal profit, decimal profitPercent)> GetCurrentPrice(Account account)
    {
        if (account.currency == "KRW")
            return (0, 0, 0);
        
        var market = $"{account.unit_currency}-{account.currency}";
        var ticker = await Api.QuotationApi.Ticker.GetTickersAsync(market);

        decimal currentPrice = ticker?.FirstOrDefault()?.trade_price ?? 0;
        decimal cost = account.BalanceDecimal * account.AvgBuyPriceDecimal;
        if (account.AvgBuyPriceDecimal <= 0 || account.BalanceDecimal <= 0)
            return (currentPrice, 0, 0);

        decimal evaluation = account.BalanceDecimal * currentPrice;
        decimal profit = evaluation - cost;
        decimal profitPercent = cost == 0 ? 0 : profit / cost * 100;

        // BUGFIX: 기존 코드가 evaluation을 currentPrice로 반환하던 문제 수정
        return (currentPrice, profit, profitPercent);
    }

    private decimal ComputeAtr(List<CandleMinute> candles, int period)
    {
        if (candles == null || candles.Count <= period) return 0m;

        decimal sumTr = 0m;
        for (int i = candles.Count - period; i < candles.Count; i++)
        {
            var cur = candles[i];
            var prevClose = candles[i - 1].trade_price;
            decimal tr1 = cur.high_price - cur.low_price;
            decimal tr2 = Math.Abs(cur.high_price - prevClose);
            decimal tr3 = Math.Abs(cur.low_price - prevClose);
            decimal tr = Math.Max(tr1, Math.Max(tr2, tr3));
            sumTr += tr;
        }
        return sumTr / period;
    }

    private string GetCoinName(string market)
    {
        return Markets.FirstOrDefault(m => m.market == market)?.korean_name ?? market;
    }
}
