using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpBot.Models.Api;

namespace UpBot.Services.Apis
{
    public class ExchangeApiClass
    {
        /// <summary>
        /// 자산
        /// </summary>
        public AssetClass Asset { get; } = new();

        /// <summary>
        /// 주문
        /// </summary>
        public OrderClass Order { get; } = new();

        /// <summary>
        /// 출금
        /// </summary>
        public WithdrawalClass Withdrawal { get; } = new();

        /// <summary>
        /// 입금
        /// </summary>
        public DepositClass Deposit { get; } = new();

        /// <summary>
        /// 서비스 정보
        /// </summary>
        public ServiceClass Service { get; } = new();

        public class AssetClass : ApiBase
        {
            /// <summary>
            /// 계정이 보유하고 있는 자산 목록과 잔고를 조회합니다.
            /// </summary>
            /// <returns></returns>
            public async Task<List<Account>?> GetAccountsAsync()
            {
                var url = "https://api.upbit.com/v1/accounts";
                return await Api.GetAsync<List<Account>>(url);
            }
        }

        public class OrderClass : ApiBase
        {
            /// <summary>
            /// 지정한 페어의 주문 가능 정보를 조회합니다.
            /// </summary>
            /// <param name="market">조회하고자 하는 페어(거래쌍)</param>
            /// <returns></returns>
            public async Task<OrdersChance?> GetOrdersChanceAsync(string market)
            {
                var url = "https://api.upbit.com/v1/orders/chance";
                var param = new Dictionary<string, object>
                {
                    { "market", market }
                };
                return await Api.GetAsync<OrdersChance>(url, param);
            }
        }

        public class WithdrawalClass : ApiBase
        {

        }

        public class DepositClass : ApiBase
        {
        }

        public class ServiceClass : ApiBase
        {
        }
    }
}
