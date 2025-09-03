using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpBot.Core;

public class TechnicalIndicators
{
    public static decimal EMA(List<decimal> prices, int period)
    {
        decimal multiplier = 2m / (period + 1);
        decimal ema = prices[0];
        for (int i = 1; i < prices.Count; i++)
        {
            ema = ((prices[i] - ema) * multiplier) + ema;
        }
        return ema;
    }

    public static decimal RSI(List<decimal> closes, int period)
    {
        decimal gain = 0, loss = 0;
        for (int i = 1; i <= period; i++)
        {
            decimal change = closes[i] - closes[i - 1];
            if (change > 0) gain += change;
            else loss -= change;
        }
        decimal avgGain = gain / period;
        decimal avgLoss = loss / period;
        if (avgLoss == 0) return 100;
        decimal rs = avgGain / avgLoss;
        return 100 - (100 / (1 + rs));
    }
}