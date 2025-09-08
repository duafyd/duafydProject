using Microsoft.Data.Sqlite;
using SQLite;
using System.IO;
using System.Windows.Controls;
using UpBot.Core;
using UpBot.Models;

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
            //try
            //{
            //    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UpBot.db");
            //    var db = new SQLiteAsyncConnection(path);                
            //    await db.CreateTableAsync<TradeHistory>();
                
            //    _db = db;
            //}
            //catch (Exception ex)
            //{
            //    Logger.Error("테이블 생성 에러", ex);
            //}
        }

        public void SaveTradeHistory(TradeHistory history)
        {
            //try
            //{
            //    using var conn = new SqliteConnection(_connectionString);
            //    conn.Open();
            //    var cmd = conn.CreateCommand();
            //    cmd.CommandText = @"
            //    INSERT INTO TradeHistory (CoinName, BuyPrice, SellPrice, ProfitAmount, ProfitPercent, TradeDate)
            //    VALUES ($coinName, $buyPrice, $sellPrice, $profitAmount, $profitPercent, $tradeDate)";
            //    cmd.Parameters.AddWithValue("$coinName", history.CoinName);
            //    cmd.Parameters.AddWithValue("$buyPrice", decimal.Parse(history.BuyPrice));
            //    cmd.Parameters.AddWithValue("$sellPrice", decimal.Parse(history.SellPrice));
            //    cmd.Parameters.AddWithValue("$profitAmount", decimal.Parse(history.ProfitAmount));
            //    cmd.Parameters.AddWithValue("$profitPercent", decimal.Parse(history.ProfitPercent));
            //    cmd.Parameters.AddWithValue("$tradeDate", history.TradeDate.ToString("o"));
            //    cmd.ExecuteNonQuery();
            //}
            //catch(Exception ex)
            //{
            //    Logger.Error("거래내역 저장 에러", ex);
            //}
        }

        // 거래내역 저장, 조회 등 메서드 추가
    }
}