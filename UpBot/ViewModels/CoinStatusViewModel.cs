using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using UpBot.Models;
using UpBot.Services;

namespace UpBot.ViewModels
{
    public partial class CoinStatusViewModel : BaseViewModel
    {
        private readonly ApiService _api;

        public ObservableCollection<TradeHistory> TradeHistories { get; set; } = new();    

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        public CoinStatusViewModel()
        {
            _api = App.ServiceProvider.GetRequiredService<ApiService>();        

            TradeHistories.Add(new TradeHistory
            {
                CoinName = "BTC",
                BuyPrice = 900000,
                SellPrice = 1000000,
                Quantity = 0.5m,
                ProfitAmount = 50000,
                ProfitPercent = 5.5m,
                TradeDate = System.DateTime.Today
            });
            TradeHistories.Add(new TradeHistory
            {
                CoinName = "ETH",
                BuyPrice = 250000,
                SellPrice = 300000,
                Quantity = 2.0m,
                ProfitAmount = 100000,
                ProfitPercent = 8.0m,
                TradeDate = System.DateTime.Today
            });
        }

        [RelayCommand]
        private async Task Start()
        {
            IsRunning = true;

            var list = await _api.QuotationApi.TradingPairs.GetMarketsAsync();
        }

        [RelayCommand]
        private void Stop()
        {
            IsRunning = false;
        }

        [RelayCommand]
        private void SellAll()
        {
            // 일괄 매도 로직
        }
    }
}