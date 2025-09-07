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
    public string balance { get; set; }

    public decimal BalanceDecimal => decimal.TryParse(balance, out var val) ? val : 0m;

    /// <summary>
    /// 출금이나 주문 등에 잠겨 있는 잔액
    /// </summary>
    public string locked { get; set; }

    /// <summary>
    /// 매수 평균가
    /// </summary>
    public string avg_buy_price { get; set; }

    public decimal AvgBuyPriceDecimal => decimal.TryParse(avg_buy_price, out var val) ? val : 0m;

    /// <summary>
    /// 매수 평균가 수정 여부
    /// </summary>
    public bool avg_buy_price_modified { get; set; }

    /// <summary>
    /// 평균가 기준 통화
    /// </summary>
    public string unit_currency { get; set; }
}
