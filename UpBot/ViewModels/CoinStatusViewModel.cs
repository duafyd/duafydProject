using System.Collections.ObjectModel;
using System.Windows.Input;
using UpBot.Models;

namespace UpBot.ViewModels
{
    public class CoinStatusViewModel : BaseViewModel
    {
        public ObservableCollection<TradeHistory> TradeHistories { get; set; } = new();
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand SellAllCommand { get; }

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
            StartCommand = new RelayCommand(Start);
            StopCommand = new RelayCommand(Stop);
            SellAllCommand = new RelayCommand(SellAll);       

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

        private void Start()
        {
            IsRunning = true;
        }
        private void Stop()
        {
            IsRunning = false;
        }
        private void SellAll()
        {
            // 일괄 매도 로직
        }
    }
}