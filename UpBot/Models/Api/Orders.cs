namespace UpBot.Models.Api;

/// <summary>
/// 주문 응답 모델
/// </summary>
public class Orders : ApiResponseBase
{
    /// <summary>
    /// 페어(거래쌍)의 코드
    /// [예시] "KRW-BTC"
    /// </summary>
    public string market { get; set; }

    /// <summary>
    /// 주문의 유일 식별자
    /// </summary>
    public string uuid { get; set; }

    /// <summary>
    /// 주문 방향(매수/매도)
    /// Allowed: ask, bid
    /// </summary>
    public string side { get; set; }

    /// <summary>
    /// 주문 유형
    /// Allowed: limit, price, market, best
    /// </summary>
    public string ord_type { get; set; }

    /// <summary>
    /// 주문 단가 또는 총액
    /// 지정가 주문의 경우 단가, 시장가 매수 주문의 경우 매수 총액
    /// </summary>
    public string price { get; set; }

    /// <summary>
    /// 주문 상태
    /// Allowed: wait(체결 대기), watch(예약 주문 대기), done(체결 완료), cancel(주문 취소)
    /// </summary>
    public string state { get; set; }

    /// <summary>
    /// 주문 생성 시각 (KST 기준)
    /// [형식] yyyy-MM-ddTHH:mm:ss+09:00
    /// </summary>
    public string created_at { get; set; }

    /// <summary>
    /// 주문 요청 수량
    /// </summary>
    public string volume { get; set; }

    /// <summary>
    /// 체결 후 남은 주문 양
    /// </summary>
    public string remaining_volume { get; set; }

    /// <summary>
    /// 체결된 양
    /// </summary>
    public string executed_volume { get; set; }

    /// <summary>
    /// 수수료로 예약된 비용
    /// </summary>
    public string reserved_fee { get; set; }

    /// <summary>
    /// 남은 수수료
    /// </summary>
    public string remaining_fee { get; set; }

    /// <summary>
    /// 사용된 수수료
    /// </summary>
    public string paid_fee { get; set; }

    /// <summary>
    /// 거래에 사용 중인 비용
    /// </summary>
    public string locked { get; set; }

    /// <summary>
    /// 해당 주문에 대한 체결 건수
    /// </summary>
    public int trades_count { get; set; }

    /// <summary>
    /// 주문 체결 옵션
    /// Allowed: fok, ioc, post_only
    /// </summary>
    public string time_in_force { get; set; }

    /// <summary>
    /// 주문 생성시 클라이언트가 지정한 주문 식별자
    /// identifier 필드는 2024년 10월 18일 이후 생성된 주문에 대해서만 제공
    /// </summary>
    public string identifier { get; set; }

    /// <summary>
    /// 자전거래 체결 방지(Self-Match Prevention) 모드
    /// Allowed: reduce, cancel_maker, cancel_taker
    /// </summary>
    public string smp_type { get; set; }

    /// <summary>
    /// 자전거래 방지로 인해 취소된 수량
    /// 동일 사용자의 주문 간 체결이 발생하지 않도록 설정(SMP)에 따라 취소된 주문 수량
    /// </summary>
    public string prevented_volume { get; set; }

    /// <summary>
    /// 자전거래 방지로 인해 해제된 자산
    /// 자전거래 체결 방지 설정으로 인해 취소된 주문의 잔여 자산
    /// 매수 주문의 경우: 취소된 금액
    /// 매도 주문의 경우: 취소된 수량
    /// </summary>
    public string prevented_locked { get; set; }
}
