using System;
using System.Collections.Generic;
using Dapper;
using EmailSender.Models;

namespace EmailSender.Data.Repositories
{
    public class EmailAddressRepository
    {
        public int Add(EmailRecipient item)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>(@"
                    INSERT OR IGNORE INTO EmailAddress
                        (ListId,ListName,Email,FirstName,LastName,Company,Domain,
                         VerifyStatus,IsValid,FailCount,TotalSent,CreatedAt,UpdatedAt)
                    VALUES
                        (@ListId,@ListName,@Email,@FirstName,@LastName,@Company,@Domain,
                         @VerifyStatus,@IsValid,@FailCount,@TotalSent,
                         datetime('now','localtime'),datetime('now','localtime'));
                    SELECT last_insert_rowid();", item);
            }
        }

        public void AddBatch(IEnumerable<EmailRecipient> items)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    conn.Execute(@"
                        INSERT OR IGNORE INTO EmailAddress
                            (ListId,ListName,Email,FirstName,LastName,Company,Domain,
                             VerifyStatus,IsValid,FailCount,TotalSent,CreatedAt,UpdatedAt)
                        VALUES
                            (@ListId,@ListName,@Email,@FirstName,@LastName,@Company,@Domain,
                             @VerifyStatus,@IsValid,@FailCount,@TotalSent,
                             datetime('now','localtime'),datetime('now','localtime'))",
                        items, tx);
                    tx.Commit();
                }
            }
        }

        public EmailRecipient GetById(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.QueryFirstOrDefault<EmailRecipient>(
                    "SELECT * FROM EmailAddress WHERE Id=@Id", new { Id = id });
            }
        }

        public List<EmailRecipient> GetByListId(int listId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.Query<EmailRecipient>(
                    "SELECT * FROM EmailAddress WHERE ListId=@ListId AND IsValid=1",
                    new { ListId = listId }).AsList();
            }
        }

        /// <summary>获取过滤后的发送列表（排除黑名单/已发/失败超限）</summary>
        public List<EmailRecipient> GetFilteredForTask(
            int listId, int maxFailCount, int? taskId, bool excludeThisWeek)
        {
            var sql = @"
                SELECT a.* FROM EmailAddress a
                WHERE a.ListId = @ListId
                  AND a.IsValid = 1
                  AND a.FailCount <= @MaxFailCount
                  AND a.Email NOT IN (SELECT Email FROM Blacklist)";

            if (taskId.HasValue)
                sql += " AND a.Email NOT IN (SELECT Email FROM SendRecord WHERE TaskId=@TaskId)";

            if (excludeThisWeek)
                sql += " AND (a.LastSentAt IS NULL OR a.LastSentAt < date('now','-7 days'))";

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.Query<EmailRecipient>(sql,
                    new { ListId = listId, MaxFailCount = maxFailCount, TaskId = taskId })
                    .AsList();
            }
        }

        public void UpdateFailCount(string email, int failCount)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute(@"
                    UPDATE EmailAddress SET
                        FailCount=@FailCount,
                        UpdatedAt=datetime('now','localtime')
                    WHERE Email=@Email",
                    new { Email = email, FailCount = failCount });
            }
        }

        public void UpdateLastSentAt(string email)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute(@"
                    UPDATE EmailAddress SET
                        LastSentAt=datetime('now','localtime'),
                        TotalSent=TotalSent+1,
                        UpdatedAt=datetime('now','localtime')
                    WHERE Email=@Email", new { Email = email });
            }
        }

        public Dictionary<string, int> GetDomainStats(int listId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var rows = conn.Query(
                    @"SELECT Domain, COUNT(*) as Cnt
                      FROM EmailAddress WHERE ListId=@ListId AND IsValid=1
                      GROUP BY Domain ORDER BY Cnt DESC LIMIT 20",
                    new { ListId = listId });
                var dict = new Dictionary<string, int>();
                foreach (var r in rows)
                    dict[r.Domain] = (int)r.Cnt;
                return dict;
            }
        }

        public int GetCount(int listId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM EmailAddress WHERE ListId=@ListId AND IsValid=1",
                    new { ListId = listId });
            }
        }
    }
}
