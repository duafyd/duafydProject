using Microsoft.Extensions.DependencyInjection;
using UpBot.Core;
using UpBot.Models.Api;
using Timer = System.Timers.Timer;

namespace UpBot.Services;

public class Bot
{
    private readonly ApiService _api;

    private readonly Timer _buyTimer;
    private bool _isBuying;

    private readonly Timer _sellTimer;
    public bool _isSelling;

    public Bot()
    {
        _api = App.ServiceProvider.GetRequiredService<ApiService>();

        _buyTimer = new Timer(TimeSpan.FromMinutes(5));
        _buyTimer.Elapsed += async (s, e) => await CheckBuyAsync();

        _sellTimer = new Timer(TimeSpan.FromSeconds(5)); // 5초
        _sellTimer.Elapsed += async (s, e) => await CheckSellAsync();
    }

    private async Task CheckBuyAsync()
    {
        if (_isBuying)
            return;

        _isBuying = true;

        // 매수 조건 확인 및 매수 로직 구현  
        try
        {
            // 마켓 전체 가져오기
            var markets = await _api.QuotationApi.TradingPairs.GetMarketsAsync();
            // 유의종목 제외한 KRW 마켓만 필터링
            var goodMarkets = markets?
                .Where(m => m.market.StartsWith("KRW-") && m.IsSafe)
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
            var top20Json = System.Text.Json.JsonSerializer.Serialize(top20Dict);
            Logger.Info($"Top 20 Markets by 24h Trade Price: {top20Json}");            

            // 상위 20개 종목에 대해 매수 조건 확인
            foreach (var t in top20)
            {
                var candles = await _api.QuotationApi.OHLCV.GetCandlesMinutesAsync(
                    market: t.market,
                    count: 200,
                    unit: CandleMinuteUnitType.Minute5);
            }
        }
        catch (Exception ex)
        {
            // 예외 처리
        }
        finally
        {
            _isBuying = false;
        }
    }

    private async Task CheckSellAsync()
    {
        if (_isSelling)
            return;

        _isSelling = true;

        // 매도 조건 확인 및 매도 로직 구현  
        try
        {
        }
        catch (Exception ex)
        {
            // 예외 처리
        }
        finally
        {
            _isSelling = false;
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

        // 조건 1: EMA20 지지
        bool emaSupport = lastCandle.trade_price > ema;

        // 조건 2: RSI 과매도
        bool rsiOversold = rsi < 30;

        // 조건 3: 거래량 증가 (최근 5봉 평균 대비 2배 이상)
        decimal avgVolume = 0;
        for (int i = candles.Count - 6; i < candles.Count - 1; i++) avgVolume += candles[i].candle_acc_trade_volume;
        avgVolume /= 5;
        bool highVolume = lastCandle.candle_acc_trade_volume > avgVolume * 2;

        // 조건 4: 반전 캔들 (Hammer 예시)
        decimal body = Math.Abs(lastCandle.trade_price - lastCandle.opening_price);
        decimal lowerShadow = lastCandle.opening_price > lastCandle.trade_price
                              ? lastCandle.trade_price - lastCandle.low_price
                              : lastCandle.opening_price - lastCandle.low_price;
        bool hammer = lowerShadow > body * 2;

        // 모든 조건 만족하면 매수
        return emaSupport && rsiOversold && highVolume && hammer;
    }
}
