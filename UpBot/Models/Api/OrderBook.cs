namespace UpBot.Models.Api;

/// <summary>
/// 거래쌍의 호가 정보를 담는 클래스입니다.
/// </summary>
public class OrderBook : ApiResponseBase
{
    /// <summary>
    /// 페어(거래쌍)의 코드입니다.
    /// 예시: "KRW-BTC"
    /// </summary>
    public string market { get; set; }

    /// <summary>
    /// 조회 요청 시각의 타임스탬프(ms)입니다.
    /// </summary>
    public long timestamp { get; set; }

    /// <summary>
    /// 현재 호가의 전체 매도 잔량 합계입니다.
    /// </summary>
    public double total_ask_size { get; set; }

    /// <summary>
    /// 현재 호가의 전체 매수 잔량 합계입니다.
    /// </summary>
    public double total_bid_size { get; set; }

    /// <summary>
    /// 호가 정보 리스트입니다. 1호가부터 30호가까지의 정보를 담고 있습니다.
    /// </summary>
    public List<OrderBookUnit> orderbook_units { get; set; }
}

/// <summary>
/// 개별 호가 정보를 담는 클래스입니다.
/// </summary>
public class OrderBookUnit
{
    /// <summary>
    /// 매도 호가입니다.
    /// </summary>
    public double ask_price { get; set; }

    /// <summary>
    /// 매수 호가입니다.
    /// </summary>
    public double bid_price { get; set; }

    /// <summary>
    /// 매도 잔량입니다.
    /// </summary>
    public double ask_size { get; set; }

    /// <summary>
    /// 매수 잔량입니다.
    /// </summary>
    public double bid_size { get; set; }

    /// <summary>
    /// 해당 호가가 적용된 가격 단위입니다. 기본값은 0입니다.
    /// </summary>
    public double level { get; set; }
}

public 