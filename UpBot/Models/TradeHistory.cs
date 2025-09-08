using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace UpBot.Models
{
    public class TradeHistory : ObservableObject
    {
        [PrimaryKey]
        public string Id { get; set; }

        private string _coinName;
        /// <summary>
        /// 코인이름
        /// </summary>
        public string CoinName
        {
            get => _coinName;
            set => SetProperty(ref _coinName, value);
        }

        private string _side;
        /// <summary>
        /// 주문 방향(매수/매도)
        /// Allowed: ask, bid
        /// </summary>
        public string Side
        {
            get => _side;
            set => SetProperty(ref _side, value);
        }

        private string _buyPrice;
        /// <summary>
        /// 매수 단가
        /// </summary>
        public string BuyPrice
        {
            get => _buyPrice;
            set => SetProperty(ref _buyPrice, value);
        }

        private string _sellPrice;
        /// <summary>
        /// 매도단가
        /// </summary>
        public string SellPrice
        {
            get => _sellPrice;
            set => SetProperty(ref _sellPrice, value);
        }

        private string _volume;
        /// <summary>
        /// 주문 수량
        /// </summary>
        public string Volume
        {
            get => _volume;
            set => SetProperty(ref _volume, value);
        }

        private string _profitAmount;
        /// <summary>
        /// 손익 금액
        /// </summary>
        public string ProfitAmount
        {
            get => _profitAmount;
            set => SetProperty(ref _profitAmount, value);
        }

        private string _profitPercent;
        /// <summary>
        /// 손익 퍼센트
        /// </summary>
        public string ProfitPercent
        {
            get => _profitPercent;
            set => SetProperty(ref _profitPercent, value);
        }

        private DateTime _tradeDate;
        /// <summary>
        /// 거래일자
        /// </summary>
        public DateTime TradeDate
        {
            get => _tradeDate;
            set => SetProperty(ref _tradeDate, value);
        }
    }
}