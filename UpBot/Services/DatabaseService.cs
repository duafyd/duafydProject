using ControlzEx.Standard;
using Microsoft.Data.Sqlite;
using SQLite;
using System.IO;
using System.Windows.Controls;
using UpBot.Core;
using UpBot.Models;
using UpBot.Models.Api;

namespace UpBot.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString = "Data Source=UpBot.db";
        private static SQLiteAsyncConnection _db;

        public DatabaseService()
        {

        }

        public void Init()
        {
            try
            {
                CreateTables();
            }
            catch (Exception ex)
            {
                Logger.Error("DB 초기화 에러", ex);
            }
        }

        private async void CreateTables()
        {
            try
            {
                SQLitePCL.Batteries_V2.Init();

                var path = Path.Combine(@"D:\Upbot\", "UpBot.db");
                var db = new SQLiteAsyncConnection(path);
                await db.CreateTableAsync<TradeHistory>();

                _db = db;
            }
            catch (Exception ex)
            {
                Logger.Error("테이블 생성 에러", ex);
            }
        }
        public async Task SaveTradeHistory(TradeHistory history, bool isBuy)
        {
            try
            {
                if (isBuy)
                {
                    var query = $"""
                        SELECT *
                        FROM TradeHistory
                        WHERE Market = '{history.Market}'
                          AND (SellTradeDate IS NULL OR TRIM(SellTradeDate) = '')
                        ORDER BY TradeDate DESC
                        LIMIT 1;
                        """;

                    var existing = await _db.QueryAsync<TradeHistory>(query);
                    if (existing == null || existing.Count == 0)
                    {
                        // 매수 인서트
                        await _db.InsertAsync(history);
                    }
                    else
                    {
                        var price = (decimal.Parse(history.BuyPrice) + decimal.Parse(history.Volume)) / 2;
                        var volume = (decimal.Parse(existing[0].Volume) + decimal.Parse(history.Volume)).ToString();
                        
                        existing[0].BuyPrice = price.ToString();
                        existing[0].Volume = volume;

                        await _db.UpdateAsync(existing[0]);
                    }
                }
                else
                {
                    var query = $"""
                        SELECT *
                        FROM TradeHistory
                        WHERE Market = '{history.Market}'
                          AND (SellTradeDate IS NULL OR TRIM(SellTradeDate) = '')
                        ORDER BY TradeDate DESC
                        LIMIT 1;
                        """;

                    // 매도 업데이트
                    var existing = await _db.QueryAsync<TradeHistory>(query);

                    if (existing != null)
                    {
                        existing[0].SellPrice = history.SellPrice;
                        existing[0].SellTradeDate = history.SellTradeDate;
                        existing[0].ProfitAmount = history.ProfitAmount;
                        existing[0].ProfitPercent = history.ProfitPercent;

                        await _db.UpdateAsync(existing[0]);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error("거래내역 저장 에러", ex);
            }
        }
    }
}