namespace UpBot.Models.Api;

/// <summary>
/// 호가 정책 조회
/// </summary>
public class OrderBookInstruments : ApiResponseBase
{
    /// <summary>
    /// 페어(거래쌍)의 코드
    /// 예시: "KRW-BTC"
    /// </summary>
    public string market { get; set; }

    /// <summary>
    /// 해당 페어의 마켓 통화 코드(ex. KRW, BTC, USDT)
    /// </summary>
    public string quote_currency { get; set; }

    /// <summary>
    /// 해당 페어에 적용되는 호가 단위
    /// </summary>
    public string tick_size { get; set; }

    /// <summary>
    /// 해당 페어에서 지원하는 호가 모아보기 단위.<br/>
    /// 0: 기본 호가단위<br/>
    /// 호가 모아보기 기능은 원화마켓(KRW)에서만 지원합니다.<br/>
    /// (BTC, USDT 마켓의 경우 0만 존재)
    /// </summary>
    public int[] supported_levels { get; set; }
}
