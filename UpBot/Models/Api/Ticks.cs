namespace UpBot.Models.Api;

/// <summary>
/// 지정한 페어의 최근 체결 목록을 조회합니다.
/// </summary>
public class Ticks : ApiResponseBase
{
    /// <summary>
    /// 페어(거래쌍)의 코드
    /// [예시] "KRW-BTC"
    /// </summary>
    public string market { get; set; }

    /// <summary>
    /// 체결 일자 (UTC 기준)
    /// [형식] yyyy-MM-dd
    /// </summary>
    public string trade_date_utc { get; set; }

    /// <summary>
    /// 체결 시각 (UTC 기준)
    /// [형식] HH:mm:ss
    /// </summary>
    public string trade_time_utc { get; set; }

    /// <summary>
    /// 체결 시각의 밀리초단위 타임스탬프
    /// </summary>
    public decimal timestamp { get; set; }

    /// <summary>
    /// 최근 체결 가격
    /// </summary>
    public decimal trade_price { get; set; }

    /// <summary>
    /// 최근 거래 수량
    /// </summary>
    public decimal trade_volume { get; set; }

    /// <summary>
    /// 전일 종가 (UTC 0시 기준)
    /// </summary>
    public decimal prev_closing_price { get; set; }

    /// <summary>
    /// 전일 종가 대비 가격 변화.
    /// "trade_price" - "prev_closing_price"로 계산되며, 현재 종가가 전일 종가보다 얼마나 상승 또는 하락했는지를 나타냅니다.
    /// 양수(+): 현재 종가가 전일 종가보다 상승한 경우
    /// 음수(-): 현재 종가가 전일 종가보다 하락한 경우
    /// </summary>
    public decimal change_price { get; set; }

    /// <summary>   
    /// 매수/매도 주문 구분
    /// Allowed: ASK, BID
    /// </summary>
    public string ask_bid { get; set; }

    /// <summary>
    /// 체결의 유일 식별자.
    /// 해당 필드는 체결 순서를 보장하지 않습니다.
    /// </summary>
    public int sequential_id { get; set; }
}
