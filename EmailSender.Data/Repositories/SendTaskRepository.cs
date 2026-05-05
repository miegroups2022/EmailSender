using System.Collections.Generic;
using Dapper;
using EmailSender.Models;

namespace EmailSender.Data.Repositories
{
    public class SendTaskRepository
    {
        public int Add(SendTask t)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>(@"
                    INSERT INTO SendTask
                        (Name,TemplateId,TemplateName,ListId,ListName,
                         AccountId,AccountName,Channel,ScheduledAt,
                         IntervalSeconds,ThreadCount,RetryMax,BatchSize,
                         FilterConfig,Status,TotalCount,CreatedAt,UpdatedAt)
                    VALUES
                        (@Name,@TemplateId,@TemplateName,@ListId,@ListName,
                         @AccountId,@AccountName,@Channel,@ScheduledAt,
                         @IntervalSeconds,@ThreadCount,@RetryMax,@BatchSize,
                         @FilterConfig,@Status,@TotalCount,
                         datetime('now','localtime'),datetime('now','localtime'));
                    SELECT last_insert_rowid();", t);
            }
        }

        public void UpdateStatus(int id, SendTaskStatus status)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute(@"
                    UPDATE SendTask SET Status=@Status,
                        UpdatedAt=datetime('now','localtime')
                    WHERE Id=@Id", new { Id = id, Status = status });
            }
        }

        public void UpdateStats(int id, int success, int fail, int open, int click, int bounce)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute(@"
                    UPDATE SendTask SET
                        SuccessCount=@Success, FailCount=@Fail,
                        OpenCount=@Open, ClickCount=@Click, BounceCount=@Bounce,
                        UpdatedAt=datetime('now','localtime')
                    WHERE Id=@Id",
                    new { Id=id, Success=success, Fail=fail,
                          Open=open, Click=click, Bounce=bounce });
            }
        }

        public SendTask GetById(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.QueryFirstOrDefault<SendTask>(
                    "SELECT * FROM SendTask WHERE Id=@Id", new { Id = id });
            }
        }

        public List<SendTask> GetAll()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.Query<SendTask>(
                    "SELECT * FROM SendTask ORDER BY CreatedAt DESC").AsList();
            }
        }

        public void Delete(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute("DELETE FROM SendTask WHERE Id=@Id", new { Id = id });
            }
        }
    }
}
