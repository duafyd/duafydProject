using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace UpBot.Models
{
    public class TradeHistory : ObservableObject
    {
        private string _toBeKey;
        [PrimaryKey]
        public string ToBeKey
        {
            get => _toBeKey ?? (_toBeKey = $"{Market}-{TradeDate}");
            set => _toBeKey = value;
        }

        private string _market = string.Empty;
        public string Market
        {
            get => _market;
            set => SetProperty(ref _market, value);
        }

        private string _coinName = string.Empty;
        /// <summary>
        /// 코인이름
        /// </summary>
        public string CoinName
        {
            get => _coinName;
            set => SetProperty(ref _coinName, value);
        }

        private string _buyPrice = string.Empty;
        /// <summary>
        /// 매수 단가
        /// </summary>
        public string BuyPrice
        {
            get => _buyPrice;
            set => SetProperty(ref _buyPrice, value);
        }

        private string _sellPrice = string.Empty;
        /// <summary>
        /// 매도단가
        /// </summary>
        public string SellPrice
        {
            get => _sellPrice;
            set => SetProperty(ref _sellPrice, value);
        }

        private string _volume = string.Empty;
        /// <summary>
        /// 주문 수량
        /// </summary>
        public string Volume
        {
            get => _volume;
            set => SetProperty(ref _volume, value);
        }

        private string _profitAmount = string.Empty;
        /// <summary>
        /// 손익 금액
        /// </summary>
        public string ProfitAmount
        {
            get => _profitAmount;
            set => SetProperty(ref _profitAmount, value);
        }

        private string _profitPercent = string.Empty;
        /// <summary>
        /// 손익 퍼센트
        /// </summary>
        public string ProfitPercent
        {
            get => _profitPercent;
            set => SetProperty(ref _profitPercent, value);
        }

        private string _buyTradeDate = "";
        /// <summary>
        /// 매수일자
        /// </summary>
        public string TradeDate
        {
            get => _buyTradeDate;
            set => SetProperty(ref _buyTradeDate, value);
        }

        private string _sellTradeDate = "";
        /// <summary>
        /// 매도일자
        /// </summary>
        public string SellTradeDate
        {
            get => _sellTradeDate;
            set => SetProperty(ref _sellTradeDate, value);
        }
    }
}