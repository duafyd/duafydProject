using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpBot.Core;

public class TechnicalIndicators
{
    //public static decimal EMA(List<decimal> prices, int period)
    //{
    //    decimal multiplier = 2m / (period + 1);
    //    decimal ema = prices[0];
    //    for (int i = 1; i < prices.Count; i++)
    //    {
    //        ema = ((prices[i] - ema) * multiplier) + ema;
    //    }
    //    return ema;
    //}

    public static List<decimal> EMA(List<decimal> prices, int period)
    {
        var result = new List<decimal>();

        if (prices == null || prices.Count < period)
            return result;

        // multiplier = 2 / (기간 + 1)
        decimal multiplier = 2m / (period + 1);

        // 초기값: 단순이동평균(SMA)
        decimal sma = prices.Take(period).Average();
        result.Add(sma);

        // 나머지 EMA 계산
        decimal prevEma = sma;
        for (int i = period; i < prices.Count; i++)
        {
            decimal ema = ((prices[i] - prevEma) * multiplier) + prevEma;
            result.Add(ema);
            prevEma = ema;
        }

        return result;
    }

    //public static decimal RSI(List<decimal> closes, int period)
    //{
    //    decimal gain = 0, loss = 0;
    //    for (int i = 1; i <= period; i++)
    //    {
    //        decimal change = closes[i] - closes[i - 1];
    //        if (change > 0) gain += change;
    //        else loss -= change;
    //    }
    //    decimal avgGain = gain / period;
    //    decimal avgLoss = loss / period;
    //    if (avgLoss == 0) return 100;
    //    decimal rs = avgGain / avgLoss;
    //    return 100 - (100 / (1 + rs));
    //}

    public static List<decimal> RSI(List<decimal> prices, int period)
    {
        var result = new List<decimal>();

        if (prices == null || prices.Count <= period)
            return result;

        List<decimal> gains = new List<decimal>();
        List<decimal> losses = new List<decimal>();

        // 첫 period 구간 평균 구하기
        for (int i = 1; i <= period; i++)
        {
            decimal change = prices[i] - prices[i - 1];
            if (change >= 0)
            {
                gains.Add(change);
                losses.Add(0);
            }
            else
            {
                gains.Add(0);
                losses.Add(Math.Abs(change));
            }
        }

        decimal avgGain = gains.Average();
        decimal avgLoss = losses.Average();

        // 첫 RSI 값
        decimal rs = avgLoss == 0 ? 0 : avgGain / avgLoss;
        decimal rsi = avgLoss == 0 ? 100 : 100 - (100 / (1 + rs));
        result.Add(rsi);

        // 나머지 RSI 값
        for (int i = period + 1; i < prices.Count; i++)
        {
            decimal change = prices[i] - prices[i - 1];

            decimal gain = change > 0 ? change : 0;
            decimal loss = change < 0 ? Math.Abs(change) : 0;

            avgGain = ((avgGain * (period - 1)) + gain) / period;
            avgLoss = ((avgLoss * (period - 1)) + loss) / period;

            rs = avgLoss == 0 ? 0 : avgGain / avgLoss;
            rsi = avgLoss == 0 ? 100 : 100 - (100 / (1 + rs));

            result.Add(rsi);
        }

        return result;
    }
}