namespace UpBot.Models.Api;

public class Account : ApiResponseBase
{
    /// <summary>
    /// 조회하고자 하는 통화 코드
    /// </summary>
    public string currency { get; set; } = string.Empty;

    /// <summary>
    /// 주문 가능 수량 또는 금액
    /// </summary>
    public decimal balance { get; set; }

    /// <summary>
    /// 출금이나 주문 등에 잠겨 있는 잔액
    /// </summary>
    public decimal locked { get; set; }

    /// <summary>
    /// 매수 평균가
    /// </summary>
    public decimal avg_buy_price { get; set; }
    
    /// <summary>
    /// 매수 평균가 수정 여부
    /// </summary>
    public decimal avg_buy_price_modified { get; set; }

    /// <summary>
    /// 평균가 기준 통화
    /// </summary>
    public decimal unit_currency { get; set; }
}
