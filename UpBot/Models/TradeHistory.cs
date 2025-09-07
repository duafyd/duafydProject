using CommunityToolkit.Mvvm.ComponentModel;

namespace UpBot.Models
{
    public partial class TradeHistory : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string _coinName;

        [ObservableProperty]
        private string _buyPrice;

        [ObservableProperty]
        private string _sellPrice;

        [ObservableProperty]
        private string _quantity;

        [ObservableProperty]
        private string _profitAmount;

        [ObservableProperty]
        private string _profitPercent;

        [ObservableProperty]
        private DateTime _tradeDate;
    }
}