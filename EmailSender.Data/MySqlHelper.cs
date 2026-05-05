using System;
using MySql.Data.MySqlClient;

namespace EmailSender.Data
{
    /// <summary>
    /// MySQL 连接助手（可选，用于 Webhook 回调数据存储）
    /// </summary>
    public static class MySqlHelper
    {
        private static string _connectionString;

        public static void Configure(string host, int port,
            string db, string user, string password)
        {
            _connectionString =
                $"Server={host};Port={port};Database={db};" +
                $"Uid={user};Pwd={password};CharSet=utf8mb4;";
        }

        public static MySqlConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException(
                    "MySqlHelper 未配置，请先调用 Configure()");
            return new MySqlConnection(_connectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch { return false; }
        }
    }
}
