using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UpBot.Models.Api;

namespace UpBot.Services.Apis;

public class QuotationApiClass
{
    /// <summary>
    /// 업비트에서 지원하는 모든 페어 목록을 조회합니다.
    /// </summary>
    public TradingPairsClass TradingPairs { get; set; } = new();

    /// <summary>
    /// 캔들 목록을 조회합니다.
    /// </summary>
    public OHLCVClass OHLCV { get; set; } = new();

    /// <summary>
    /// 업비트에서 지원하는 모든 페어 목록을 조회합니다.
    /// </summary>
    public class TradingPairsClass : ApiBase
    {
        /// <summary>
        /// 업비트에서 지원하는 모든 페어 목록을 조회합니다.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Market>?> GetMarketsAsync()
        {
            var url = "https://api.upbit.com/v1/market/all";
            var param = new Dictionary<string, object>
            {
                { "is_details", true }
            };
            return await Api.GetAsync<List<Market>>(url, param);
        }
    }

    /// <summary>
    /// 캔들 목록을 조회합니다.
    /// </summary>
    public class OHLCVClass : ApiBase
    {
        public async Task<List<CandleSecond>?> GetCandlesSecondsAsync(string market, DateTime to, int count = 1)
        {
            return await GetCandlesAsync<CandleSecond>(CandleType.Second, market, to, count);
        }

        public async Task<List<CandleMinute>?> GetCandlesMinutesAsync(string market, DateTime to, int count = 1, CandleMinuteUnitType unit = CandleMinuteUnitType.Minute1)
        {
            return await GetCandlesAsync<CandleMinute>(CandleType.Minute, market, to, count, unit);
        }

        public async Task<List<CandleDay>?> GetCandlesDaysAsync(string market, DateTime to, int count = 1)
        {
            return await GetCandlesAsync<CandleDay>(CandleType.Day, market, to, count);
        }

        public async Task<List<CandleWeek>?> GetCandlesWeeksAsync(string market, DateTime to, int count = 1)
        {
            return await GetCandlesAsync<CandleWeek>(CandleType.Week, market, to, count);
        }

        public async Task<List<CandleMonth>?> GetCandlesMonthsAsync(string market, DateTime to, int count = 1)
        {
            return await GetCandlesAsync<CandleMonth>(CandleType.Month, market, to, count);
        }

        public async Task<List<CandleYear>?> GetCandlesYearsAsync(string market, DateTime to, int count = 1)
        {
            return await GetCandlesAsync<CandleYear>(CandleType.Year, market, to, count);
        }


        private async Task<List<T>?> GetCandlesAsync<T>(CandleType candleType, string market, DateTime to, int count = 1, CandleMinuteUnitType unit = CandleMinuteUnitType.None)
        {
            string url = candleType switch
            {
                CandleType.Second => $"https://api.upbit.com/v1/candles/seconds",
                CandleType.Minute => $"https://api.upbit.com/v1/candles/minutes/{(int)unit}",
                CandleType.Day => $"https://api.upbit.com/v1/candles/days",
                CandleType.Week => $"https://api.upbit.com/v1/candles/weeks",
                CandleType.Month => $"https://api.upbit.com/v1/candles/months",
                CandleType.Year => $"https://api.upbit.com/v1/candles/years",
                _ => throw new ArgumentOutOfRangeException(nameof(candleType))
            };

            // 조회 캔들 제한 (1~200)
            if (count > 200)
                count = 200;
            else if (count < 1)
                count = 1;

            var param = new Dictionary<string, object>
            {
                { "market", market },
                { "to", to.ToString("yyyy-MM-ddTHH:mm:ssK") },
                { "count", count }
            };

            if (candleType == CandleType.Day)
            {
                param.Add("converting_price_unit", "KRW");
            }

            return await Api.GetAsync<List<T>>(url, param);
        }
    }

    /// <summary>
    /// 지정한 페어의 최근 체결 목록을 조회합니다.
    /// </summary>
    public class TradeClass : ApiBase
    {
        /// <summary>
        /// 지정한 페어의 최근 체결 목록을 조회합니다.
        /// </summary>
        /// <param name="market">조회하고자 하는 페어</param>
        /// <param name="count">체결 내역의 개수 1~500</param>
        /// <param name="corsor">Pagination을 위한 조회 범위 지정용 커서<br/>
        ///                      응답에 포함된 sequential_id를 입력하면 이어서 조회 가능</param>
        /// <param name="days_ago">조회 대상 일자와 요청 시점과의 일 단위 offsetparam>
        /// <returns></returns>        
        public async Task<List<Ticks>?> GetTicksAsync(string market, int count = 1, string corsor = "", int days_ago = 0)
        {
            var url = "https://api.upbit.com/v1/trades/ticks";
            var param = new Dictionary<string, object>
            {
                { "market", market },
                { "count", count },
            };
            
            if(!string.IsNullOrEmpty(corsor))
                param.Add("cursor", corsor);

            if (days_ago > 0)
                param.Add("days_ago", days_ago);

            return await Api.GetAsync<List<Ticks>>(url, param);
        }
    }

    public class TickerClass : ApiBase
    {
        /// <summary>
        /// 지정한 페어의 현재가를 조회합니다.
        /// </summary>
        /// <param name="markets"></param>
        /// <returns></returns>
        public async Task<List<Ticker>?> GetTickersAsync(string markets)
        {
            var url = "https://api.upbit.com/v1/ticker";
            var param = new Dictionary<string, object>
            {
                { "markets", markets }
            };
            return await Api.GetAsync<List<Ticker>>(url, param);
        }

        /// <summary>
        /// 지정한 마켓(호가 자산) 내 모든 페어들의 현재가 정보를 조회합니다.
        /// </summary>
        /// <param name="quote_currencies">마켓의 통화 코드</param>
        /// <returns></returns>
        public async Task<List<Ticker>?> GetTickerAllAsync(string quote_currencies)
        {
            var url = "https://api.upbit.com/v1/ticker/all";
            var param = new Dictionary<string, object>
            {
                { "quote_currencies", quote_currencies }
            };
            return await Api.GetAsync<List<Ticker>>(url, param);
        }
    }

    public class OrderbookClass : ApiBase
    {
        /// <summary>
        /// 지정한 종목들의 실시간 호가(Orderbook) 정보를 조회합니다.
        /// </summary>
        /// <param name="markets">조회하고자 하는 페어(거래쌍) 목록</param>
        /// <param level="markets">조회하고자 하는 페어(거래쌍) 목록</param>
        /// <returns></returns>
        public async Task<List<OrderBook>?> GetOrderBooksAsync(string markets, string level = "0", int count = 30)
        {
            var url = "https://api.upbit.com/v1/orderbook";
            var param = new Dictionary<string, object>
            {
                { "markets", markets },
                { "level", level },
                { "count", count },
            };
            return await Api.GetAsync<List<OrderBook>>(url, param);
        }

        /// <summary>
        /// 지정한 페어들의 호가 단위(tick_size)와 호가 모아보기 단위(supported_levels) 정보를 조회합니다.
        /// </summary>
        /// <returns></returns>
        public async Task<List<OrderBookInstruments>?> GetOrderBookInstruments(string markets)
        {
            var url = "https://api.upbit.com/v1/orderbook/instruments";
            var param = new Dictionary<string, object>
            {
                { "markets", markets },                
            };
            return await Api.GetAsync<List<OrderBookInstruments>>(url, param);
        }
    }
}
