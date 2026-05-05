using System.Collections.Generic;
using Dapper;
using EmailSender.Models;

namespace EmailSender.Data.Repositories
{
    public class AppConfigRepository
    {
        public string Get(string key)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.QueryFirstOrDefault<string>(
                    "SELECT Value FROM AppConfig WHERE Key=@Key", new { Key = key });
            }
        }

        public void Set(string key, string value, string remark = null)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute(@"
                    INSERT INTO AppConfig(Key,Value,Remark,UpdatedAt)
                    VALUES(@Key,@Value,@Remark,datetime('now','localtime'))
                    ON CONFLICT(Key) DO UPDATE SET
                        Value=excluded.Value,
                        Remark=excluded.Remark,
                        UpdatedAt=excluded.UpdatedAt",
                    new { Key = key, Value = value, Remark = remark });
            }
        }

        public List<AppConfig> GetAll()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.Query<AppConfig>(
                    "SELECT * FROM AppConfig ORDER BY Key").AsList();
            }
        }

        public void Delete(string key)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute("DELETE FROM AppConfig WHERE Key=@Key", new { Key = key });
            }
        }
    }
}
