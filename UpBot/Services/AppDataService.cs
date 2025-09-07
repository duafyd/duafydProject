using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using UpBot.Models;

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

    public AppDataService()
    {
    }
}
