using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using UpBot.Models;
using UpBot.Models.Api;

namespace UpBot.Services;
public class AppDataService : ObservableObject
{
    private List<TradeHistory> _coinStatus = new();
    public List<TradeHistory> CoinStatus
    {
        get => _coinStatus;
        set
        {
            SetProperty(ref _coinStatus, value);
            if (CoinStatusChanged != null)
                CoinStatusChanged(this, EventArgs.Empty);
        }
    }

    public event EventHandler CoinStatusChanged;

    public decimal? CashBalance { get; set; }
    public decimal TotalBalance => GetTotalBalance();

    public AppDataService()
    {
    }

    private decimal GetTotalBalance()
    {
        if (!CashBalance.HasValue)
            return 0m;

        decimal total = CashBalance.Value;
        foreach (var coin in CoinStatus)
        {
            try
            {
                var amount = decimal.Parse(coin.ProfitAmount) * decimal.Parse(coin.Volume);
                total += amount;
            }
            catch { }
        }
        return total;
    }
}
