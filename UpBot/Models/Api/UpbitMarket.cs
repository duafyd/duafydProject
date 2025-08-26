namespace UpBot.Models.Api
{
    public class UpbitMarket : UpbitApiResponseBase
    {
        /// <summary>
        /// 마켓 코드 (예: KRW-BTC)
        /// </summary>
        public string market { get; set; }

        /// <summary>
        /// 한글명 (예: 비트코인)
        /// </summary>
        public string korean_name { get; set; }

        /// <summary>
        /// 영문명 (예: Bitcoin)
        /// </summary>
        public string english_name { get; set; }

        public market_event
    }

    public class MarketEvent
            {
        public bool warning { get; set; }
        public string code { get; set; }
        public string market { get; set; }
        public DateTime created_at { get; set; }
    }
}