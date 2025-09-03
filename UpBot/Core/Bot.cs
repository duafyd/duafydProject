using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpBot.Core;
using UpBot.Models.Api;

namespace UpBot.Services;

public class Bot
{
    /// <summary>
    /// 단타 매수 타이밍 감지
    /// </summary>
    /// <param name="candles"></param>
    /// <returns></returns>
    public static bool ShouldBuy(List<CandleMinute> candles)
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
