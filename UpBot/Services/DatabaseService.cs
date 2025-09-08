using Microsoft.Data.Sqlite;
using UpBot.Core;
using UpBot.Models;

namespace UpBot.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString = "Data Source=UpBot.db";

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

        private void CreateTables()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS TradeHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CoinName TEXT,
                    BuyPrice REAL,
                    SellPrice REAL,
                    ProfitAmount REAL,
                    ProfitPercent REAL,
                    TradeDate TEXT
                )";
            cmd.ExecuteNonQuery();
        }

        // 거래내역 저장, 조회 등 메서드 추가
    }
}