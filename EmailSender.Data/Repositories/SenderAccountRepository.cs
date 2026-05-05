using System.Collections.Generic;
using Dapper;
using EmailSender.Models;

namespace EmailSender.Data.Repositories
{
    public class SenderAccountRepository
    {
        public int Add(SenderAccount a)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>(@"
                    INSERT INTO SenderAccount
                        (Name,AccountType,ApiUser,ApiKey,
                         OAuthEmail,OAuthTokenJson,OAuthRefreshToken,TokenExpiresAt,
                         SmtpHost,SmtpPort,SmtpUseSsl,SmtpUser,SmtpPassword,
                         SmtpFromEmail,SmtpFromName,IsActive,IsPaused,DailyLimit,
                         CreatedAt,UpdatedAt)
                    VALUES
                        (@Name,@AccountType,@ApiUser,@ApiKey,
                         @OAuthEmail,@OAuthTokenJson,@OAuthRefreshToken,@TokenExpiresAt,
                         @SmtpHost,@SmtpPort,@SmtpUseSsl,@SmtpUser,@SmtpPassword,
                         @SmtpFromEmail,@SmtpFromName,@IsActive,@IsPaused,@DailyLimit,
                         datetime('now','localtime'),datetime('now','localtime'));
                    SELECT last_insert_rowid();", a);
            }
        }

        public void Update(SenderAccount a)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute(@"
                    UPDATE SenderAccount SET
                        Name=@Name, ApiUser=@ApiUser, ApiKey=@ApiKey,
                        OAuthEmail=@OAuthEmail, OAuthTokenJson=@OAuthTokenJson,
                        OAuthRefreshToken=@OAuthRefreshToken,
                        TokenExpiresAt=@TokenExpiresAt,
                        SmtpHost=@SmtpHost, SmtpPort=@SmtpPort,
                        SmtpUseSsl=@SmtpUseSsl, SmtpUser=@SmtpUser,
                        SmtpPassword=@SmtpPassword,
                        SmtpFromEmail=@SmtpFromEmail, SmtpFromName=@SmtpFromName,
                        IsActive=@IsActive, IsPaused=@IsPaused,
                        PauseReason=@PauseReason, DailyLimit=@DailyLimit,
                        UpdatedAt=datetime('now','localtime')
                    WHERE Id=@Id", a);
            }
        }

        public SenderAccount GetById(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.QueryFirstOrDefault<SenderAccount>(
                    "SELECT * FROM SenderAccount WHERE Id=@Id", new { Id = id });
            }
        }

        public List<SenderAccount> GetAll()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.Query<SenderAccount>(
                    "SELECT * FROM SenderAccount ORDER BY Name").AsList();
            }
        }

        public List<SenderAccount> GetActive()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.Query<SenderAccount>(
                    "SELECT * FROM SenderAccount WHERE IsActive=1 AND IsPaused=0")
                    .AsList();
            }
        }

        public void IncrementSentCount(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute(@"
                    UPDATE SenderAccount SET
                        TotalSent=TotalSent+1, TodaySent=TodaySent+1,
                        LastSentAt=datetime('now','localtime'),
                        UpdatedAt=datetime('now','localtime')
                    WHERE Id=@Id", new { Id = id });
            }
        }

        public void Delete(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                conn.Execute("DELETE FROM SenderAccount WHERE Id=@Id", new { Id = id });
            }
        }
    }
}
