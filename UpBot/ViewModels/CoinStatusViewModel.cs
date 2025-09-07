using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using UpBot.Models;
using UpBot.Services;

namespace UpBot.ViewModels
{
    public partial class CoinStatusViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<TradeHistory> _tradeHistories;

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (SetProperty(ref _isRunning, value))
                {
                    if (value)
                        Bot.Start();
                    else
                        Bot.Stop();
                }
            }
        }

        public CoinStatusViewModel()
        {
            AppData.CoinStatusChanged += AppData_CoinStatusChanged;
        }

        private void AppData_CoinStatusChanged(object? sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                TradeHistories = new ObservableCollection<TradeHistory>(AppData.CoinStatus);
            });
        }

        [RelayCommand]
        private async Task Start()
        {
            IsRunning = true;
        }

        [RelayCommand]
        private void Stop()
        {
            IsRunning = false;
        }

        //[RelayCommand]
        //private void SellAll()
        //{
        //    // 일괄 매도 로직
        //}        
    }
}