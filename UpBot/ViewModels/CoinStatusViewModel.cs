using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using UpBot.Models;
using UpBot.Services;
using Timer = System.Timers.Timer;

namespace UpBot.ViewModels
{
    public partial class CoinStatusViewModel : BaseViewModel
    {
        private readonly ApiService _api;

        private readonly Timer _buyTimer;
        private bool _isBuying;

        private readonly Timer _sellTimer;
        public bool _isSelling;

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

            _buyTimer = new Timer(TimeSpan.FromMinutes(5));
            _buyTimer.Elapsed += async (s, e) => await CheckBuyAsync();

            _sellTimer = new Timer(TimeSpan.FromSeconds(5)); // 5초
            _sellTimer.Elapsed += async (s, e) => await CheckSellAsync();

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

        //[RelayCommand]
        //private void SellAll()
        //{
        //    // 일괄 매도 로직
        //}

        private async Task CheckBuyAsync()
        {
            if (_isBuying)
                return;

            _isBuying = true;

            // 매수 조건 확인 및 매수 로직 구현  
            try
            {
                // 마켓 전체 가져오기
                var markets = await _api.QuotationApi.TradingPairs.GetMarketsAsync();
                // 유의종목 제외한 KRW 마켓만 필터링
                var goodMarkets = markets?
                    .Where(m => m.market.StartsWith("KRW-") && m.IsSafe)
                    .Select(m => m.market)
                    .ToList();

                var marketsStr = string.Join(",", goodMarkets);
                // ticker 데이터 가져오기(거래대금)
                var tickers = await _api.QuotationApi.Ticker.GetTickersAsync(marketsStr);

                // 거래대금 상위 20개
                var top20 = tickers?
                    .OrderByDescending(t => t.acc_trade_price_24h)
                    .Take(20)
                    .ToList();

                // 로그 기록
                foreach (var t in top20)
                {
                    Console.WriteLine($"{t.market} : {t.acc_trade_price_24h}");
                }

                // 상위 20개 종목에 대해 매수 조건 확인
                foreach (var t in top20)
                {
                                        
                }
            }
            catch (Exception ex)
            {
                // 예외 처리
            }
            finally
            {
                _isBuying = false;
            }
        }

        private async Task CheckSellAsync()
        {
            if (_isSelling)
                return;

            _isSelling = true;

            // 매도 조건 확인 및 매도 로직 구현  
            try
            {
            }
            catch (Exception ex)
            {
                // 예외 처리
            }
            finally
            {
                _isSelling = false;
            }
        }
    }
}