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
        }
    }

    public event EventHandler CoinStatusChanged;

    public decimal? CashBalance { get; set; }
    public decimal TotalBalance => GetTotalBalance();

    public AppDataService()
    {
    }

    public void SetCoinStatus(List<TradeHistory>? list)
    {
        if (list == null || list.Count == 0)
            CoinStatus.Clear();
        else
            CoinStatus = list.ToList();

        if (CoinStatusChanged != null)
            CoinStatusChanged(this, EventArgs.Empty);
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
