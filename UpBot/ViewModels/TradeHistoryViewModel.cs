using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using UpBot.Models;

namespace UpBot.ViewModels
{
    public partial class TradeHistoryViewModel : ViewModelBase
    {
        public ObservableCollection<TradeHistory> TradeHistories { get; set; } = new();
        public DateTime? SelectedDate { get; set; }
        public decimal TotalProfitAmount { get; set; }
        public decimal TotalProfitPercent { get; set; }   

        public TradeHistoryViewModel()
        {                     // 예시 데이터
           
        }

        [RelayCommand]
        private void Search()
        {
       
        }

    }
}