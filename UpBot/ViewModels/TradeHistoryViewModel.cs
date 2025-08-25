using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UpBot.Models;

namespace UpBot.ViewModels
{
    public class TradeHistoryViewModel : BaseViewModel
    {
        public ObservableCollection<TradeHistory> TradeHistories { get; set; } = new();
        public DateTime? SelectedDate { get; set; }
        public decimal TotalProfitAmount { get; set; }
        public decimal TotalProfitPercent { get; set; }
        public ICommand SearchCommand { get; }

        public TradeHistoryViewModel()
        {
            SearchCommand = new RelayCommand(Search);

            // 예시 데이터
            TradeHistories.Add(new TradeHistory
            {
                CoinName = "BTC",
                BuyPrice = 900000,
                SellPrice = 1000000,
                Quantity = 0.5m,
                ProfitAmount = 50000,
                ProfitPercent = 5.5m,
                TradeDate = DateTime.Today
            });
            TradeHistories.Add(new TradeHistory
            {
                CoinName = "ETH",
                BuyPrice = 250000,
                SellPrice = 300000,
                Quantity = 2.0m,
                ProfitAmount = 100000,
                ProfitPercent = 8.0m,
                TradeDate = DateTime.Today
            });

            UpdateProfit();
        }

        private void Search()
        {
            // DB에서 거래내역 조회 후 TradeHistories, TotalProfitAmount, TotalProfitPercent 갱신
            UpdateProfit();
        }

        private void UpdateProfit()
        {
            TotalProfitAmount = 0;
            TotalProfitPercent = 0;
            foreach (var t in TradeHistories)
            {
                TotalProfitAmount += t.ProfitAmount;
                TotalProfitPercent += t.ProfitPercent;
            }
            OnPropertyChanged(nameof(TotalProfitAmount));
            OnPropertyChanged(nameof(TotalProfitPercent));
        }
    }
}