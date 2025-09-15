using ControlzEx.Standard;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using UpBot.Core;
using UpBot.Models;
using UpBot.Models.Api;
using Timer = System.Timers.Timer;

namespace UpBot.Services;

public class Bot
{
    public int StockCount => AppData.CashBalance.HasValue && AppData.CashBalance.Value >= 30000 ? 2 : 1;
    public TimeSpan CooldownAfterTrade => TimeSpan.FromMinutes(60);

    private readonly ApiService Api;
    private readonly AppDataService AppData;
    private readonly DatabaseService Database;

    private readonly Timer _buyTimer;
    private bool _isBuying;

    private readonly Timer _sellTimer;
    private bool _isSelling;

    public decimal TotalBalance { get; private set; }
    public List<Market> Markets { get; private set; } = new();

    private readonly Dictionary<string, string> _lastProcessedCandleKst = new();
    private readonly Dictionary<string, DateTime> _cooldowns = new();
    private readonly Dictionary<string, int> _buyStage = new(); // 0=없음,1=1차,2=2차,3=3차

    public Bot()
    {
        Api = App.ServiceProvider.GetRequiredService<ApiService>();
        AppData = App.ServiceProvider.GetRequiredService<AppDataService>();
        Database = App.ServiceProvider.GetRequiredService<DatabaseService>();

        _buyTimer = new Timer(TimeSpan.FromMinutes(5));
        _buyTimer.Elapsed += async (s, e) => await CheckBuyAsync();

        _sellTimer = new Timer(TimeSpan.FromSeconds(5));
        _sellTimer.Elapsed += async (s, e) => await CheckSellAsync();
    }

    public async void Start()
    {
        _sellTimer.Start();
        await CheckSellAsync();

        _buyTimer.Start();
        await CheckBuyAsync();

        Logger.Info("Swing Bot started.");
    }

    public void Stop()
    {
        _buyTimer.Stop();
        _sellTimer.Stop();
        Logger.Info("Bot stopped.");
    }

    // ==============================
    // 매수 체크 (분할매수 + 슬롯 제한)
    // ==============================
    private async Task CheckBuyAsync()
    {
        if (_isBuying) return;
        _isBuying = true;

        Logger.Info("********* 매수 검사 시작");

        try
        {
            if (!AppData.CashBalance.HasValue || AppData.CashBalance < 5000) return;

            if (Markets.Count == 0)
            {
                var marketsLoaded = await Api.QuotationApi.TradingPairs.GetMarketsAsync();
                if (marketsLoaded == null || marketsLoaded.Count == 0) return;
                Markets = marketsLoaded;
            }

            var accounts = await Api.ExchangeApi.Asset.GetAccountsAsync();
            var heldMarkets = new HashSet<string>(accounts?
                .Where(a => a.currency != "KRW" && a.BalanceDecimal > 0 && a.AvgBuyPriceDecimal > 0)
                .Select(a => $"{a.unit_currency}-{a.currency}") ?? new List<string>());

            int slotsLeft = StockCount - heldMarkets.Count;
            if (slotsLeft <= 0)
            {
                Logger.Info($"BUYCHK RETURN: 슬롯 다 찼음.({StockCount}/{heldMarkets.Count})");
                return; // 슬롯 다 찼음
            }

            var goodMarkets = Markets.Where(m => m.market.StartsWith("KRW-")).Select(m => m.market).ToList();
            var tickers = await Api.QuotationApi.Ticker.GetTickersAsync(string.Join(",", goodMarkets));
            if (tickers == null || tickers.Count == 0) return;

            var top = tickers
                .OrderByDescending(t => t.acc_trade_price_24h)
                .Take(10)
                .ToList();

            foreach (var t in top)
            {
                if (heldMarkets.Contains(t.market)) continue;
                if (_cooldowns.TryGetValue(t.market, out var until) && DateTime.UtcNow < until) continue;
                if (slotsLeft <= 0) break; // 슬롯 다 찼으면 종료

                await Task.Delay(200);

                var candles15m = await Api.QuotationApi.OHLCV.GetCandlesMinutesAsync(
                    market: t.market,
                    count: 200,
                    unit: CandleMinuteUnitType.Minute15);
                if (candles15m == null) continue;

                var latestKst = candles15m.Last().candle_date_time_kst;
                var key = $"{t.market}:15";
                if (_lastProcessedCandleKst.TryGetValue(key, out var prev) && prev == latestKst) continue;
                _lastProcessedCandleKst[key] = latestKst;

                int stage = _buyStage.ContainsKey(t.market) ? _buyStage[t.market] : 0;
                int newStage = await ShouldBuySwingAsync(t.market, candles15m, stage);
                if (newStage <= stage) continue;

                // 종목당 최대 투자금
                var maxInvest = AppData.TotalBalance / StockCount;

                // 단계별 투자 비율 (슬롯 개수에 따라)
                decimal ratio = 0m;
                if (StockCount == 1)
                {
                    ratio = newStage switch
                    {
                        1 => 0.5m, // 50%
                        2 => 0.3m, // 30%
                        3 => 0.2m, // 20%
                        _ => 0m
                    };
                }
                else
                {
                    ratio = newStage switch
                    {
                        1 => 0.4m, // 40%
                        2 => 0.3m, // 30%
                        3 => 0.3m, // 30%
                        _ => 0m
                    };
                }

                var buyAmount = maxInvest * ratio;
                if (buyAmount < 5000) continue;

                var orderChance = await Api.ExchangeApi.Order.GetOrdersChanceAsync(t.market);
                if (orderChance == null) continue;

                if (!decimal.TryParse(orderChance.bid_fee, out var feeRate)) feeRate = 0m;
                buyAmount = Math.Floor(buyAmount / (1 + feeRate));

                var order = await Api.ExchangeApi.Order.PostMarketBuyAsync(t.market, buyAmount.ToString());
                if (order == null) continue;

                await Database.SaveTradeHistory(new TradeHistory
                {
                    Market = t.market,
                    CoinName = GetCoinName(t.market),
                    BuyPrice = order.price,
                    Volume = order.volume,
                    TradeDate = DateTime.UtcNow.ToString("yyyyMMdd"),
                }, true);

                _buyStage[t.market] = newStage;
                Logger.Info($"매수 단계 {newStage} 완료 {t.market} (금액:{buyAmount})");

                heldMarkets.Add(t.market);
                slotsLeft--;
                if (slotsLeft <= 0) break; // 필요한 만큼만 매수
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Error in CheckBuyAsync", ex);
        }
        finally
        {
            _isBuying = false;
            Logger.Info("********* 매수 검사 종료");
        }
    }

    // ==============================
    // 매도 체크 (볼린저 익절 / MA25 손절)
    // ==============================
    private async Task CheckSellAsync()
    {
        if (_isSelling) return;
        _isSelling = true;

        try
        {
            var account = await Api.ExchangeApi.Asset.GetAccountsAsync();
            if (account == null || account.Count == 0) return;

            // 현금 잔고
            AppData.CashBalance = account.FirstOrDefault(a => a.currency == "KRW")?.BalanceDecimal ?? 0;

            var coins = account
                .Where(a => a.currency != "KRW" && a.BalanceDecimal > 0 && a.AvgBuyPriceDecimal > 0)
                .ToList();

            // ===== TotalBalance 갱신 =====
            decimal coinEval = 0m;
            foreach (var acc in coins)
            {
                var ticker = await Api.QuotationApi.Ticker.GetTickersAsync($"{acc.unit_currency}-{acc.currency}");
                var price = ticker?.FirstOrDefault()?.trade_price ?? 0m;
                coinEval += acc.BalanceDecimal * price;
            }
            TotalBalance = AppData.CashBalance.Value + coinEval;

            if (coins.Count == 0)
            {
                Logger.Debug("SELLCHK RETURN: 보유 코인 없음.");

                AppData.SetCoinStatus(null);

                return;
            }

            var list = new List<TradeHistory>();
            foreach(var coin in coins)
            {
                var market = $"{coin.unit_currency}-{coin.currency}";
                var coinName = GetCoinName(market);
                var (currentPrice, profit, profitPercent) = await GetCurrentPrice(coin);

                list.Add(new TradeHistory
                {
                    Market = market,
                    CoinName = coinName,
                    BuyPrice = coin.AvgBuyPriceDecimal.ToString("C"),
                    Volume = coin.BalanceDecimal.ToString("G29"),
                    ProfitAmount = profit.ToString("C"),
                    ProfitPercent = profitPercent.ToString("F2") + "%",
                });

                var history = new TradeHistory()
                {
                    Market = $"{coin.unit_currency}-{coin.currency}",
                    BuyPrice = coin.avg_buy_price,
                    Volume = coin.balance,

                };
                if (history != null)
                    list.Add(history);
            }

            foreach (var acc in coins)
            {
                try
                {
                    var market = $"{acc.unit_currency}-{acc.currency}";
                    var coinName = GetCoinName(market);

                    var candles15m = await Api.QuotationApi.OHLCV.GetCandlesMinutesAsync(
                        market: market,
                        count: 50,
                        unit: CandleMinuteUnitType.Minute15);
                    if (candles15m == null || candles15m.Count < 30) continue;

                    var closes = candles15m.Select(c => c.trade_price).ToList();
                    var ma25 = MovingAverage(closes, 25);
                    var (bbUpper, _) = BollingerBands(closes, 20, 2);

                    var last = candles15m[^1];

                    bool sellSignal = false;
                    string reason = "";

                    if (last.trade_price > bbUpper)
                    {
                        sellSignal = true;
                        reason = "BOLLINGER BREAKOUT";
                    }

                    if (last.trade_price < ma25)
                    {
                        sellSignal = true;
                        reason = "MA25 BREAKDOWN";
                    }

                    if (sellSignal)
                    {
                        Logger.Info($"SELL SIGNAL {reason} {market}: Close={last.trade_price}");

                        var sell = await Api.ExchangeApi.Order.PostMarketSellAsync(market, acc.balance);
                        if (sell != null)
                        {
                            Logger.Info($"SELL EXECUTED {market}: 이유={reason}");
                            var (currentPrice, profit, profitPercent) = await GetCurrentPrice(acc);

                            await Database.SaveTradeHistory(new TradeHistory
                            {
                                Market = market,
                                CoinName = coinName,
                                SellPrice = sell.price,
                                ProfitAmount = profit.ToString("C"),
                                ProfitPercent = profitPercent.ToString("F2") + "%",
                                SellTradeDate = DateTime.UtcNow.ToString("yyyyMMdd"),
                            }, false);

                            _cooldowns[market] = DateTime.UtcNow + CooldownAfterTrade;
                            _buyStage[market] = 0; // 초기화
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("SELLCHK 예외 발생", ex);
                }
            }

            Logger.Debug($"SELLCHK 완료: TotalBalance={TotalBalance:F0}, Cash={AppData.CashBalance}, Coins={coins.Count}");
        }
        catch (Exception ex)
        {
            Logger.Error("Error in CheckSellAsync", ex);
        }
        finally
        {
            _isSelling = false;
        }
    }


    // ==============================
    // 매수 조건 (정배열 전환 + 거래량 필터)
    // ==============================
    private async Task<int> ShouldBuySwingAsync(string market, List<CandleMinute> candles15m, int currentStage)
    {
        if (candles15m.Count < 30) return currentStage;

        var closes = candles15m.Select(c => c.trade_price).ToList();
        var ma5 = MovingAverage(closes, 5);
        var ma10 = MovingAverage(closes, 10);
        var ma15 = MovingAverage(closes, 15);
        var ma25 = MovingAverage(closes, 25);

        var last = candles15m[^1];
        var prevCloses = closes.Take(closes.Count - 1).ToList();

        var prevMa5 = MovingAverage(prevCloses, 5);
        var prevMa10 = MovingAverage(prevCloses, 10);
        var prevMa15 = MovingAverage(prevCloses, 15);
        var prevMa25 = MovingAverage(prevCloses, 25);

        bool isNowGolden = (ma5 > ma10 && ma10 > ma15 && ma15 > ma25);
        bool wasGolden = (prevMa5 > prevMa10 && prevMa10 > prevMa15 && prevMa15 > prevMa25);

        // 거래량 필터
        var avgVol5 = candles15m.SkipLast(1).TakeLast(5).Average(c => c.candle_acc_trade_volume);
        bool volBoost = last.candle_acc_trade_volume > avgVol5;

        // 1️⃣ 정배열 전환 순간 + MA5 터치
        if (currentStage == 0 && !wasGolden && isNowGolden)
        {
            if (last.low_price <= ma5 && last.trade_price >= ma5 && volBoost)
            {
                Logger.Debug($"BUYCHK PASS {market}: 역배→정배 전환 + MA5 + 거래량 증가");
                return 1;
            }
        }

        // 2️⃣ 추가매수
        if (currentStage == 1 && last.low_price <= ma15 && last.trade_price >= ma15)
            return 2;
        if (currentStage == 2 && last.low_price <= ma10 && last.trade_price >= ma10)
            return 3;

        return currentStage;
    }


    // ==============================
    // 보조 함수
    // ==============================
    private decimal MovingAverage(List<decimal> prices, int period)
    {
        if (prices.Count < period) return 0m;
        return prices.Skip(prices.Count - period).Average();
    }

    private (decimal upper, decimal lower) BollingerBands(List<decimal> prices, int period, int k)
    {
        if (prices.Count < period) return (0m, 0m);
        var subset = prices.Skip(prices.Count - period).ToList();
        decimal sma = subset.Average();
        decimal variance = subset.Sum(p => (p - sma) * (p - sma)) / period;
        decimal stdDev = (decimal)Math.Sqrt((double)variance);
        return (sma + k * stdDev, sma - k * stdDev);
    }

    private string GetCoinName(string market)
        => Markets.FirstOrDefault(m => m.market == market)?.korean_name ?? market;

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
}
