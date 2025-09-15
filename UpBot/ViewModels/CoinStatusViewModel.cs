using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using UpBot.Core;
using UpBot.Models;
using UpBot.Services;

namespace UpBot.ViewModels
{
    public partial class CoinStatusViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<TradeHistory> _tradeHistories;

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

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
                try
                {
                    if(AppData.CoinStatus == null || AppData.CoinStatus.Count == 0)
                    {
                        TradeHistories?.Clear();                     
                    }
                    else
                    {
                        TradeHistories = new ObservableCollection<TradeHistory>(AppData.CoinStatus);
                    }                        
                    OnPropertyChanged(nameof(TradeHistories));
                    //StatusMessage = $"Total Balance: {AppData.TotalBalance}";
                }
                catch (Exception ex) { Logger.Error(ex.Message, ex); }
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