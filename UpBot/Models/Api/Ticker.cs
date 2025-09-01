namespace UpBot.Models.Api;

/// <summary>
/// 페어의 현재가 조회
/// </summary>
public class Ticker : ApiResponseBase
{
    /// <summary>
    /// 페어(거래쌍)의 코드
    /// [예시] "KRW-BTC"
    /// </summary>
    public string market { get; set; }

    /// <summary>
    /// 최근 체결 일자 (UTC 기준)
    /// [형식] yyyyMMdd
    /// </summary>
    public string trade_date { get; set; }

    /// <summary>
    /// 최근 체결 시각 (UTC 기준)
    /// [형식] HHmmss
    /// </summary>
    public string trade_time { get; set; }

    /// <summary>
    /// 최근 체결 일자 (KST 기준)
    /// [형식] yyyyMMdd
    /// </summary>
    public string trade_date_kst { get; set; }

    /// <summary>
    /// 최근 체결 시각 (KST 기준)
    /// [형식] HHmmss
    /// </summary>
    public string trade_time_kst { get; set; }

    /// <summary>
    /// 체결 시각의 밀리초단위 타임스탬프
    /// </summary>
    public long trade_timestamp { get; set; }

    /// <summary>
    /// 시가. 해당 페어의 첫 거래 가격입니다.
    /// </summary>
    public double opening_price { get; set; }

    /// <summary>
    /// 고가. 해당 페어의 최고 거래 가격입니다.
    /// </summary>
    public double high_price { get; set; }

    /// <summary>
    /// 저가. 해당 페어의 최저 거래 가격입니다.
    /// </summary>
    public double low_price { get; set; }

    /// <summary>
    /// 종가. 해당 페어의 현재 가격입니다.
    /// </summary>
    public double trade_price { get; set; }

    /// <summary>
    /// 전일 종가 (UTC 0시 기준)
    /// </summary>
    public double prev_closing_price { get; set; }

    /// <summary>
    /// 가격 변동 상태
    /// Allowed: EVEN, RISE, FALL
    /// EVEN: 보합, RISE: 상승, FALL: 하락
    /// </summary>
    public string change { get; set; }

    /// <summary>
    /// 전일 종가 대비 가격 변화(절대값)
    /// "trade_price" - "prev_closing_price"로 계산됩니다.
    /// </summary>
    public double change_price { get; set; }

    /// <summary>
    /// 전일 종가 대비 가격 변화 (절대값)
    /// ("trade_price" - "prev_closing_price") ÷ "prev_closing_price" 으로 계산됩니다.
    /// </summary>
    public double change_rate { get; set; }

    /// <summary>
    /// 전일 종가 대비 가격 변화.<br/>
    /// "trade_price" - "prev_closing_price"로 계산되며, 현재 종가가 전일 종가보다 얼마나 상승 또는 하락했는지를 나타냅니다.<br/>
    /// 양수(+): 현재 종가가 전일 종가보다 상승한 경우<br/>
    /// 음수(-): 현재 종가가 전일 종가보다 하락한 경우
    /// </summary>
    public double signed_change_price { get; set; }

    /// <summary>
    /// 전일 종가 대비 가격 변화율<br/>
    /// ("trade_price" - "prev_closing_price") ÷ "prev_closing_price" 으로 계산됩니다.<br/>
    /// 양수(+): 가격 상승, 음수(-): 가격 하락<br/>
    /// [예시] 0.015 = 1.5% 상승
    /// </summary>
    public double signed_change_rate { get; set; }

    /// <summary>
    /// 최근 거래 수량
    /// </summary>
    public double trade_volume { get; set; }

    /// <summary>
    /// 누적 거래 금액 (UTC 0시 기준)
    /// </summary>
    public double acc_trade_price { get; set; }

    /// <summary>
    /// 24시간 누적 거래 금액
    /// </summary>
    public double acc_trade_price_24h { get; set; }

    /// <summary>
    /// 누적 거래량 (UTC 0시 기준)
    /// </summary>
    public double acc_trade_volume { get; set; }

    /// <summary>
    /// 24시간 누적 거래량
    /// </summary>
    public double acc_trade_volume_24h { get; set; }

    /// <summary>
    /// 52주 신고가
    /// </summary>
    public double highest_52_week_price { get; set; }

    /// <summary>
    /// 52주 신고가 달성일
    /// [형식] yyyy-MM-dd
    /// </summary>
    public string highest_52_week_date { get; set; }

    /// <summary>
    /// 52주 신저가
    /// </summary>
    public double lowest_52_week_price { get; set; }

    /// <summary>
    /// 52주 신저가 달성일
    /// [형식] yyyy-MM-dd
    /// </summary>
    public string lowest_52_week_date { get; set; }

    /// <summary>
    /// 현재가 정보가 반영된 시각의 타임스탬프(ms)
    /// </summary>
    public long timestamp { get; set; }
}
