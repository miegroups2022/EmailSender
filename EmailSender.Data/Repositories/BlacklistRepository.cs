using System.Collections.Generic;
using Dapper;
using EmailSender.Models;

namespace EmailSender.Data.Repositories
{
    public class BlacklistRepository
    {
        public void Add(Blacklist b)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute(@"
                    INSERT OR IGNORE INTO Blacklist
                        (Email,Type,Reason,TaskId,SendRecordId,
                         FailCount,TotalSent,CreatedAt)
                    VALUES
                        (@Email,@Type,@Reason,@TaskId,@SendRecordId,
                         @FailCount,@TotalSent,datetime('now','localtime'))", b);
            }
        }

        public void AddBatch(IEnumerable<Blacklist> items)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    conn.Execute(@"
                        INSERT OR IGNORE INTO Blacklist
                            (Email,Type,Reason,TaskId,SendRecordId,
                             FailCount,TotalSent,CreatedAt)
                        VALUES
                            (@Email,@Type,@Reason,@TaskId,@SendRecordId,
                             @FailCount,@TotalSent,datetime('now','localtime'))",
                        items, tx);
                    tx.Commit();
                }
            }
        }

        public bool Contains(string email)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Blacklist WHERE Email=@Email",
                    new { Email = email }) > 0;
            }
        }

        public List<Blacklist> GetAll()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.Query<Blacklist>(
                    "SELECT * FROM Blacklist ORDER BY CreatedAt DESC").AsList();
            }
        }

        public void Delete(string email)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute("DELETE FROM Blacklist WHERE Email=@Email",
                    new { Email = email });
            }
        }

        public int GetCount()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Blacklist");
            }
        }
    }
}
