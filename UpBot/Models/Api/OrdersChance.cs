using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpBot.Models.Api;

/// <summary>
/// 주문 가능 정보 조회 응답 모델
/// </summary>
public class OrdersChance : ApiResponseBase
{
    /// <summary>
    /// 매수 시 수수료율
    /// </summary>
    public string bid_fee { get; set; } = string.Empty;

    /// <summary>
    /// 매도 시 수수료율
    /// </summary>
    public string ask_fee { get; set; } = string.Empty;

    /// <summary>
    /// 매수 maker 주문 수수료 비율
    /// </summary>
    public string maker_bid_fee { get; set; } = string.Empty;

    /// <summary>
    /// 매도 maker 주문 수수료 비율
    /// </summary>
    public string maker_ask_fee { get; set; } = string.Empty;

    public class OrdersChanceMarket
    {
        /// <summary>
        /// 페어(거래쌍)의 코드
        /// </summary>
        public string id { get; set; } = string.Empty;

        /// <summary>
        /// 페어 코드((기준 자산)/(디지털 자산 구매에 사용되는 통화 - KRW,BTC,USDT))
        /// </summary>
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// 지원하는 주문 유형
        /// </summary>
        public string[] order_types { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 지원하는 주문 방향(매수/매도)
        /// </summary>
        public string[] order_sides { get; set; } = Array.Empty<string>();
    }
}

