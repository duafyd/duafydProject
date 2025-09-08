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

    public OrdersChanceMarket market { get; set; } = new();

    /// <summary>
    /// 호가 자산 계좌 정보
    /// </summary>
    public OrdersChanceAccount bid_account { get; set; } = new();

    /// <summary>
    /// 기준 자산 계좌 정보
    /// </summary>
    public OrdersChanceAccount ask_account { get; set; } = new();

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

        /// <summary>
        /// 지원하는 매수 주문 유형
        /// </summary>
        public string[] bid_types { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 지원하는 매도 주문 유형
        /// </summary>
        public string[] ask_types { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 매수 제약 조건
        /// </summary>
        public OrdersChanceMarketCondition bid { get; set; } = new ();

        /// <summary>
        /// 매도 제약 조건
        /// </summary>
        public OrdersChanceMarketCondition ask { get; set; } = new ();

        /// <summary>
        /// 최대 주문 가능 금액
        /// </summary>
        public string max_total { get; set; }

        /// <summary>
        /// 페어 운영 상태
        /// </summary>
        public string state { get; set; } = string.Empty;

        public class OrdersChanceMarketCondition
        {
            /// <summary>
            /// 디지털 자산 구매에 사용되는 통화(KRW,BTC,USDT)
            /// </summary>
            public string currency { get; set; } = string.Empty;

            /// <summary>
            /// 매수 시 최소 주믄 금액(결제 화폐 기준)
            /// </summary>
            public string min_total { get; set; } = string.Empty;
        }
    }

    public class OrdersChanceAccount
    {
        /// <summary>
        /// 조회하고자 하는 통화 코드
        /// </summary>
        public string currency { get; set; } = string.Empty;

        /// <summary>
        /// 주문 가능 수량 또는 금액<br/>
        /// 디지털 자산의 경우 수량, 법정 통화의 경우 금액입니다.
        /// </summary>
        public string balance { get; set; } = string.Empty;

        /// <summary>
        /// 출금이나 주문 등에 잠겨 있는 금액
        /// </summary>
        public string locked { get; set; } = string.Empty;

        /// <summary>
        /// 매수 평균가
        /// </summary>
        public string avg_buy_price { get; set; } = string.Empty;

        /// <summary>
        /// 매수 평균가 수정 여부
        /// </summary>
        public bool avg_buy_price_modified { get; set; }

        /// <summary>
        /// 평균가 기준 통화<br/>
        /// "avg_buy_price"가 기준하는 단위입니다.
        /// </summary>
        public string unit_currency { get; set; } = string.Empty;
    }
}

