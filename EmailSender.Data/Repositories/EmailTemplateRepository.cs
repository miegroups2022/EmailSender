using System;
using System.Collections.Generic;
using Dapper;
using EmailSender.Models;

namespace EmailSender.Data.Repositories
{
    public class EmailTemplateRepository
    {
        public int Add(EmailTemplate t)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>(@"
                    INSERT INTO EmailTemplate
                        (MeetbyTemplateId,SendCloudTemplateId,Name,Subject,
                         HtmlBody,TextBody,FromName,FromEmail,
                         NeedResync,SyncedToSendCloud,CreatedAt,UpdatedAt)
                    VALUES
                        (@MeetbyTemplateId,@SendCloudTemplateId,@Name,@Subject,
                         @HtmlBody,@TextBody,@FromName,@FromEmail,
                         @NeedResync,@SyncedToSendCloud,
                         datetime('now','localtime'),datetime('now','localtime'));
                    SELECT last_insert_rowid();", t);
            }
        }

        public void Update(EmailTemplate t)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute(@"
                    UPDATE EmailTemplate SET
                        Name=@Name, Subject=@Subject,
                        HtmlBody=@HtmlBody, TextBody=@TextBody,
                        FromName=@FromName, FromEmail=@FromEmail,
                        NeedResync=@NeedResync,
                        SyncedToSendCloud=@SyncedToSendCloud,
                        SyncedAt=@SyncedAt,
                        AiScore=@AiScore, AiIssues=@AiIssues,
                        AiSuggestions=@AiSuggestions, AiAnalyzedAt=@AiAnalyzedAt,
                        UpdatedAt=datetime('now','localtime')
                    WHERE Id=@Id", t);
            }
        }

        public EmailTemplate GetById(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.QueryFirstOrDefault<EmailTemplate>(
                    "SELECT * FROM EmailTemplate WHERE Id=@Id", new { Id = id });
            }
        }

        public List<EmailTemplate> GetAll()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.Query<EmailTemplate>(
                    "SELECT * FROM EmailTemplate ORDER BY UpdatedAt DESC").AsList();
            }
        }

        public void Delete(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute("DELETE FROM EmailTemplate WHERE Id=@Id", new { Id = id });
            }
        }

        public void MarkNeedResync(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute(@"
                    UPDATE EmailTemplate SET NeedResync=1,
                        UpdatedAt=datetime('now','localtime')
                    WHERE Id=@Id", new { Id = id });
            }
        }

        public void MarkSynced(int id, string sendCloudTemplateId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute(@"
                    UPDATE EmailTemplate SET
                        SyncedToSendCloud=1, NeedResync=0,
                        SendCloudTemplateId=@SendCloudTemplateId,
                        SyncedAt=datetime('now','localtime'),
                        UpdatedAt=datetime('now','localtime')
                    WHERE Id=@Id",
                    new { Id = id, SendCloudTemplateId = sendCloudTemplateId });
            }
        }
    }
}
