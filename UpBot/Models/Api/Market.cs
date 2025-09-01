namespace UpBot.Models.Api;

public class Market : ApiResponseBase
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

    public MarketEvent? market_event { get; set; }
}

public class MarketEvent
{
    /// <summary>
    /// 유의 종목 여부.<br/>
    /// 업비트의 시장경보 시스템에 따라 해당 페어가 유의 종목으로 지정되었는지 여부를 나타냅니다.<br/>
    /// </summary>
    public bool warning { get; set; }

    /// <summary>
    /// 주의 종목 여부.<br/>
    /// 주의 종목으로 지정된 경우, 아래의 세부 경보 유형 중 하나 이상에 해당될 수 있습니다.<br/>
    /// </summary>
    public Caution? caution { get; set; }
    public class Caution
    {
        /// <summary>
        /// 가격 급등락 경보
        /// </summary>
        public bool PRICE_FLUCTUATIONS { get; set; }

        /// <summary>
        /// 거래량 급증 경보
        /// </summary>
        public bool TRADING_VOLUME_SOARING { get; set; }

        /// <summary>
        /// 입금량 급증 경보
        /// </summary>
        public bool DEPOSIT_AMOUNT_SOARING { get; set; }

        /// <summary>
        /// 국내외 가격 차이 경보
        /// </summary>
        public bool GLOBAL_PRICE_DIFFERENCES { get; set; }

        /// <summary>
        /// 소수 계정 집중 거래 경보
        /// </summary>
        public bool CONCENTRATION_OF_SMALL_ACCOUNTS { get; set; }
    }

}