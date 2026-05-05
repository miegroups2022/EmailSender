using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace EmailSender.Data
{
    public static class DatabaseHelper
    {
        private static string _dbPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "emailsender.db");

        public static string ConnectionString =>
            $"Data Source={_dbPath};Version=3;Foreign Keys=True;";

        /// <summary>初始化数据库，创建所有表</summary>
        public static void Initialize()
        {
            if (!File.Exists(_dbPath))
                SQLiteConnection.CreateFile(_dbPath);

            using (var conn = GetConnection())
            {
                conn.Open();
                CreateTables(conn);
            }
        }

        public static SQLiteConnection GetConnection()
            => new SQLiteConnection(ConnectionString);

        private static void CreateTables(IDbConnection conn)
        {
            var sql = @"
-- 配置表
CREATE TABLE IF NOT EXISTS AppConfig (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    Key       TEXT NOT NULL UNIQUE,
    Value     TEXT,
    Remark    TEXT,
    UpdatedAt TEXT DEFAULT (datetime('now','localtime'))
);

-- 邮件地址表
CREATE TABLE IF NOT EXISTS EmailAddress (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    ListId        INTEGER NOT NULL,
    ListName      TEXT,
    Email         TEXT NOT NULL,
    FirstName     TEXT,
    LastName      TEXT,
    Company       TEXT,
    Domain        TEXT,
    VerifyStatus  INTEGER DEFAULT 0,
    VerifiedAt    TEXT,
    IsValid       INTEGER DEFAULT 1,
    FailCount     INTEGER DEFAULT 0,
    LastSentAt    TEXT,
    TotalSent     INTEGER DEFAULT 0,
    CreatedAt     TEXT DEFAULT (datetime('now','localtime')),
    UpdatedAt     TEXT DEFAULT (datetime('now','localtime'))
);
CREATE UNIQUE INDEX IF NOT EXISTS idx_email_listid ON EmailAddress(Email, ListId);

-- 邮件模版表
CREATE TABLE IF NOT EXISTS EmailTemplate (
    Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
    MeetbyTemplateId    TEXT,
    SendCloudTemplateId TEXT,
    Name                TEXT NOT NULL,
    Subject             TEXT,
    HtmlBody            TEXT,
    TextBody            TEXT,
    FromName            TEXT,
    FromEmail           TEXT,
    NeedResync          INTEGER DEFAULT 0,
    SyncedToSendCloud   INTEGER DEFAULT 0,
    SyncedAt            TEXT,
    AiScore             INTEGER,
    AiIssues            TEXT,
    AiSuggestions       TEXT,
    AiAnalyzedAt        TEXT,
    CreatedAt           TEXT DEFAULT (datetime('now','localtime')),
    UpdatedAt           TEXT DEFAULT (datetime('now','localtime'))
);

-- 发送账户表
CREATE TABLE IF NOT EXISTS SenderAccount (
    Id                INTEGER PRIMARY KEY AUTOINCREMENT,
    Name              TEXT NOT NULL,
    AccountType       INTEGER NOT NULL,
    ApiUser           TEXT,
    ApiKey            TEXT,
    OAuthEmail        TEXT,
    OAuthToken        TEXT,
    OAuthRefreshToken TEXT,
    TokenExpiresAt    TEXT,
    SmtpHost          TEXT,
    SmtpPort          INTEGER DEFAULT 587,
    SmtpUseSsl        INTEGER DEFAULT 1,
    SmtpUser          TEXT,
    SmtpPassword      TEXT,
    SmtpFromEmail     TEXT,
    SmtpFromName      TEXT,
    IsActive          INTEGER DEFAULT 1,
    IsPaused          INTEGER DEFAULT 0,
    PauseReason       TEXT,
    TotalSent         INTEGER DEFAULT 0,
    TodaySent         INTEGER DEFAULT 0,
    DailyLimit        INTEGER DEFAULT 0,
    LastSentAt        TEXT,
    CreatedAt         TEXT DEFAULT (datetime('now','localtime')),
    UpdatedAt         TEXT DEFAULT (datetime('now','localtime'))
);

-- 发送任务表
CREATE TABLE IF NOT EXISTS SendTask (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Name            TEXT NOT NULL,
    TemplateId      INTEGER NOT NULL,
    TemplateName    TEXT,
    ListId          INTEGER NOT NULL,
    ListName        TEXT,
    AccountId       INTEGER NOT NULL,
    AccountName     TEXT,
    Channel         INTEGER DEFAULT 0,
    ScheduledAt     TEXT,
    IntervalSeconds INTEGER DEFAULT 5,
    ThreadCount     INTEGER DEFAULT 3,
    RetryMax        INTEGER DEFAULT 3,
    BatchSize       INTEGER DEFAULT 50,
    FilterConfig    TEXT,
    Status          INTEGER DEFAULT 0,
    StartedAt       TEXT,
    FinishedAt      TEXT,
    TotalCount      INTEGER DEFAULT 0,
    SuccessCount    INTEGER DEFAULT 0,
    FailCount       INTEGER DEFAULT 0,
    OpenCount       INTEGER DEFAULT 0,
    ClickCount      INTEGER DEFAULT 0,
    BounceCount     INTEGER DEFAULT 0,
    CreatedAt       TEXT DEFAULT (datetime('now','localtime')),
    UpdatedAt       TEXT DEFAULT (datetime('now','localtime'))
);

-- 发送记录表
CREATE TABLE IF NOT EXISTS SendRecord (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    TaskId          INTEGER NOT NULL,
    EmailAddressId  INTEGER,
    Email           TEXT NOT NULL,
    SendStatus      INTEGER DEFAULT 0,
    SendCloudMsgId  TEXT,
    ErrorMessage    TEXT,
    RetryCount      INTEGER DEFAULT 0,
    ResultFetched   INTEGER DEFAULT 0,
    FetchedAt       TEXT,
    SentAt          TEXT,
    DeliveredAt     TEXT,
    OpenedAt        TEXT,
    ClickedAt       TEXT,
    BouncedAt       TEXT,
    CreatedAt       TEXT DEFAULT (datetime('now','localtime'))
);
CREATE INDEX IF NOT EXISTS idx_sendrecord_taskid ON SendRecord(TaskId);
CREATE INDEX IF NOT EXISTS idx_sendrecord_fetched ON SendRecord(ResultFetched);

-- 黑名单表
CREATE TABLE IF NOT EXISTS Blacklist (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    Email         TEXT NOT NULL UNIQUE,
    Type          INTEGER DEFAULT 0,
    Reason        TEXT,
    TaskId        INTEGER,
    SendRecordId  INTEGER,
    FailCount     INTEGER DEFAULT 0,
    TotalSent     INTEGER DEFAULT 0,
    LastSentAt    TEXT,
    CreatedAt     TEXT DEFAULT (datetime('now','localtime'))
);
CREATE UNIQUE INDEX IF NOT EXISTS idx_blacklist_email ON Blacklist(Email);
";
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
