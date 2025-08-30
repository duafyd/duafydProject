using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UpBot.Models.Api;

namespace UpBot.Services.Apis;

public class QuotationApiClass
{
    public TradingPairsClass TradingPairs { get; set; } = new();

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

    public class OHLCVClass
    {
    }

    public class TradeClass
    {
    }

    public class TickerClass
    {
    }

    public class OrderbookClass
    {
    }
}
