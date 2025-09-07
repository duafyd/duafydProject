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
    public int StockCount => 1;

    private readonly ApiService _api;
    private readonly AppDataService _appData;

    private readonly Timer _buyTimer;
    private bool _isBuying;

    private readonly Timer _sellTimer;
    private bool _isSelling;

    public decimal CashBalance { get; private set; }
    public decimal TotalBalance { get; private set; }

    public List<Market> Markets { get; private set; } = new();

    public Bot()
    {
        _api = App.ServiceProvider.GetRequiredService<ApiService>();
        _appData = App.ServiceProvider.GetRequiredService<AppDataService>();

        _buyTimer = new Timer(TimeSpan.FromMinutes(5));
        _buyTimer.Elapsed += async (s, e) => await CheckBuyAsync();

        _sellTimer = new Timer(TimeSpan.FromSeconds(2)); // 2초
        _sellTimer.Elapsed += async (s, e) => await CheckSellAsync();
    }

    public void Start()
    {
        _buyTimer.Start();
        CheckBuyAsync(); // 즉시 실행

        _sellTimer.Start();
        CheckSellAsync(); // 즉시 실행

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

            if (CashBalance < 5000)
                return;

            Logger.Debug("Checking buy conditions...");

            // 매수 조건 확인 및 매수 로직 구현          
            if (Markets.Count == 0)
            {
                // 마켓 전체 가져오기
                var markets = await _api.QuotationApi.TradingPairs.GetMarketsAsync();
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
            var tickers = await _api.QuotationApi.Ticker.GetTickersAsync(marketsStr);

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
                Logger.Info($"Checking should buy {t.market}");

                var candles = await _api.QuotationApi.OHLCV.GetCandlesMinutesAsync(
                    market: t.market,
                    count: 200,
                    unit: CandleMinuteUnitType.Minute5);

                if (candles == null || candles.Count < 21)
                {
                    Logger.Warn($"{t.market}: 캔들 데이터 부족");
                    continue;
                }

                var isBuySignal = ShouldBuy(candles);
                if (isBuySignal)
                {
                    Logger.Info($"Buy Signal Detected for {t.market} at Price {t.trade_price}");
                    // TODO: 실제 매수 로직 구현 필요                    

                    // 주문가능 정보 조회
                    var orderChange = await _api.ExchangeApi.Order.GetOrdersChanceAsync("KRW-BTC");
                    //// 시장가로 매수
                    //var order = await _api.ExchangeApi.Order.PostOrderAsync(new Dictionary<string, object>
                    //{
                    //    { "market", t.market },
                    //    { "side", "bid" },
                    //    { "volume", null },
                    //    { "price", Math.Floor(CashBalance * 0.9995m) }, // 수수료 고려
                    //    { "ord_type", "price" } // 시장가 매수
                    //});


                    break;
                }

                // 요청 제한 회피   
                if (t != top20.Last())
                    await Task.Delay(200);
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
            // 마켓 전체 가져오기
            var markets = await _api.QuotationApi.TradingPairs.GetMarketsAsync();
            if (markets != null)
                Markets = markets;
        }

        // 매도 조건 확인 및 매도 로직 구현  
        try
        {
            // 보유 자산 조회
            var account = await _api.ExchangeApi.Asset.GetAccountsAsync();
            if (account == null || account.Count == 0)
            {
                Logger.Warn("보유 자산 없음");
                return;
            }

            var list = new List<TradeHistory>();

            CashBalance = account?.FirstOrDefault(a => a.currency == "KRW")?.BalanceDecimal ?? 0;
            foreach (var acc in account!.Where(a => a.currency != "KRW" && a.BalanceDecimal > 0))
            {
                try
                {
                    var (currentPrice, profit, profitPercent) = await GetCurrentPrice(acc);
                    if (currentPrice >= 5000)
                    {
                        list.Add(new TradeHistory
                        {
                            CoinName = acc.currency,
                            BuyPrice = acc.AvgBuyPriceDecimal.ToString(),
                            Quantity = acc.BalanceDecimal.ToString("#,#"),
                            ProfitAmount = profit.ToString("C"),
                            ProfitPercent = profitPercent.ToString("F2") + "%",
                        });

                        if (profitPercent >= 1.5m || profitPercent <= -0.5m)
                        {
                            Logger.Info($"Sell Signal Detected for {acc.currency} at Price {currentPrice}, Profit: {profit} ({profitPercent:F2}%)");
                        }
                    }
                }
                catch { }
            }

            _appData.CoinStatus = new List<TradeHistory>(list);

        }
        catch (Exception ex)
        {
            // 예외 처리
            Logger.Error("Error in CheckSellAsync", ex);
        }
        finally
        {
            _isSelling = false;
            Logger.Debug("Finished checking sell conditions.");
        }
    }

    /// <summary>
    /// 단타 매수 타이밍 감지
    /// </summary>
    /// <param name="candles"></param>
    /// <returns></returns>
    public bool ShouldBuy(List<CandleMinute> candles)
    {
        int rsiPeriod = 14;
        int emaPeriod = 20;

        List<decimal> closes = new List<decimal>();
        foreach (var c in candles)
            closes.Add(c.trade_price);

        var rsi = TechnicalIndicators.RSI(closes.GetRange(closes.Count - (rsiPeriod + 1), rsiPeriod + 1), rsiPeriod);
        var ema = TechnicalIndicators.EMA(closes.GetRange(closes.Count - emaPeriod, emaPeriod), emaPeriod);

        var lastCandle = candles[candles.Count - 1];
        var prevRsi = TechnicalIndicators.RSI(closes.GetRange(closes.Count - (rsiPeriod + 2), rsiPeriod + 1), rsiPeriod);

        // 조건 1: EMA20 지지 + 직전봉은 EMA 아래, 현재봉은 EMA 위 (골든크로스)
        bool emaSupport = closes[closes.Count - 2] < ema && lastCandle.trade_price > ema;

        // 조건 2: RSI 35 미만 & 직전봉 대비 상승 반전 (완화)
        bool rsiOversold = rsi < 35 && rsi > prevRsi;

        // 조건 3: 거래량 급증 (최근 5봉 평균의 1.5배 이상, 완화)
        decimal avgVolume = 0;
        for (int i = candles.Count - 6; i < candles.Count - 1; i++) avgVolume += candles[i].candle_acc_trade_volume;
        avgVolume /= 5;
        bool highVolume = lastCandle.candle_acc_trade_volume > avgVolume * 1.5m;

        // 조건 4: 반전 캔들 (Hammer 예시, 그대로 유지)
        decimal body = Math.Abs(lastCandle.trade_price - lastCandle.opening_price);
        decimal lowerShadow = lastCandle.opening_price > lastCandle.trade_price
                              ? lastCandle.trade_price - lastCandle.low_price
                              : lastCandle.opening_price - lastCandle.low_price;
        bool hammer = lowerShadow > body * 2;

        // 4개 중 3개 이상 만족하면 매수
        int trueCount = 0;
        if (emaSupport) trueCount++;
        if (rsiOversold) trueCount++;
        if (highVolume) trueCount++;
        if (hammer) trueCount++;

        return trueCount >= 3;
    }

    /// <summary>
    /// 보유 코인 가격 구하기
    /// </summary>
    /// <param name="market">마켓 쌍</param>
    /// <param name="balance">보유 수량</param>
    /// <param name="avg_buy_price"></param>
    /// <returns></returns>
    public async Task<(decimal currentPrice, decimal profit, decimal profitPercent)> GetCurrentPrice(Account account)
    {
        if (account.currency == "KRW")
            return (0, 0, 0);

        var market = $"{account.unit_currency}-{account.currency}";
        var ticker = await _api.QuotationApi.Ticker.GetTickersAsync(market);

        decimal currentPrice = ticker?.FirstOrDefault()?.trade_price ?? 0;
        decimal evaluation = account.BalanceDecimal * currentPrice;
        decimal profit = evaluation - (account.BalanceDecimal * account.AvgBuyPriceDecimal);
        decimal profitPercent = profit / (account.BalanceDecimal * account.AvgBuyPriceDecimal) * 100;

        return (evaluation, profit, profitPercent);
    }
}
