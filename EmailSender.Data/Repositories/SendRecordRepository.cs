using System;
using System.Collections.Generic;
using Dapper;
using EmailSender.Models;

namespace EmailSender.Data.Repositories
{
    public class SendRecordRepository
    {
        public int Add(SendRecord r)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>(@"
                    INSERT INTO SendRecord
                        (TaskId,EmailAddressId,Email,SendStatus,
                         SendCloudMsgId,ErrorMessage,RetryCount,
                         ResultFetched,SentAt,CreatedAt)
                    VALUES
                        (@TaskId,@EmailAddressId,@Email,@SendStatus,
                         @SendCloudMsgId,@ErrorMessage,@RetryCount,
                         @ResultFetched,@SentAt,datetime('now','localtime'));
                    SELECT last_insert_rowid();", r);
            }
        }

        public void AddBatch(IEnumerable<SendRecord> records)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    conn.Execute(@"
                        INSERT INTO SendRecord
                            (TaskId,EmailAddressId,Email,SendStatus,
                             SendCloudMsgId,ErrorMessage,RetryCount,
                             ResultFetched,SentAt,CreatedAt)
                        VALUES
                            (@TaskId,@EmailAddressId,@Email,@SendStatus,
                             @SendCloudMsgId,@ErrorMessage,@RetryCount,
                             @ResultFetched,@SentAt,datetime('now','localtime'))",
                        records, tx);
                    tx.Commit();
                }
            }
        }

        public List<SendRecord> GetPendingFetch(int taskId, int limit = 100)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.Query<SendRecord>(@"
                    SELECT * FROM SendRecord
                    WHERE TaskId=@TaskId AND ResultFetched=0
                      AND SendCloudMsgId IS NOT NULL
                    LIMIT @Limit",
                    new { TaskId = taskId, Limit = limit }).AsList();
            }
        }

        public void UpdateStatus(int id, SendStatus status, string errorMsg = null)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute(@"
                    UPDATE SendRecord SET
                        SendStatus=@Status, ErrorMessage=@ErrorMsg
                    WHERE Id=@Id",
                    new { Id = id, Status = status, ErrorMsg = errorMsg });
            }
        }

        public void MarkFetched(int id, SendStatus status)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute(@"
                    UPDATE SendRecord SET
                        SendStatus=@Status,
                        ResultFetched=1,
                        FetchedAt=datetime('now','localtime')
                    WHERE Id=@Id",
                    new { Id = id, Status = status });
            }
        }

        public List<SendRecord> GetByTaskId(int taskId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.Query<SendRecord>(
                    "SELECT * FROM SendRecord WHERE TaskId=@TaskId",
                    new { TaskId = taskId }).AsList();
            }
        }

        public int GetCountByStatus(int taskId, SendStatus status)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>(@"
                    SELECT COUNT(*) FROM SendRecord
                    WHERE TaskId=@TaskId AND SendStatus=@Status",
                    new { TaskId = taskId, Status = status });
            }
        }
    }
}
