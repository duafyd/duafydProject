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
    }

    public class OrderClass : ApiBase
    {
        
    }
}
