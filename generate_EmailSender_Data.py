import os

base = "EmailSender.Data"
os.makedirs(base, exist_ok=True)
os.makedirs(f"{base}/Repositories", exist_ok=True)
os.makedirs(f"{base}/Properties", exist_ok=True)

files = {}

# ── csproj ─────────────────────────────────────────────────────
files["EmailSender.Data.csproj"] = """\
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props" />
  <PropertyGroup>
    <ProjectGuid>{22222222-2222-2222-2222-222222222222}</ProjectGuid>
    <OutputType>Library</OutputType>
    <RootNamespace>EmailSender.Data</RootNamespace>
    <AssemblyName>EmailSender.Data</AssemblyName>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="DatabaseHelper.cs" />
    <Compile Include="MySqlHelper.cs" />
    <Compile Include="Repositories\\AppConfigRepository.cs" />
    <Compile Include="Repositories\\BlacklistRepository.cs" />
    <Compile Include="Repositories\\EmailAddressRepository.cs" />
    <Compile Include="Repositories\\EmailTemplateRepository.cs" />
    <Compile Include="Repositories\\SendRecordRepository.cs" />
    <Compile Include="Repositories\\SenderAccountRepository.cs" />
    <Compile Include="Repositories\\SendTaskRepository.cs" />
    <Compile Include="Properties\\AssemblyInfo.cs" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\\EmailSender.Models\\EmailSender.Models.csproj">
      <Project>{11111111-1111-1111-1111-111111111111}</Project>
      <Name>EmailSender.Models</Name>
    </ProjectReference>
  </ItemGroup>
  <ItemGroup>
    <Reference Include="System.Data" />
    <Reference Include="Dapper">
      <HintPath>..\\packages\\Dapper.2.1.35\\lib\\net461\\Dapper.dll</HintPath>
    </Reference>
    <Reference Include="System.Data.SQLite">
      <HintPath>..\\packages\\System.Data.SQLite.Core.1.0.118.0\\lib\\net46\\System.Data.SQLite.dll</HintPath>
    </Reference>
    <Reference Include="MySql.Data">
      <HintPath>..\\packages\\MySql.Data.8.3.0\\lib\\net48\\MySql.Data.dll</HintPath>
    </Reference>
  </ItemGroup>
  <Import Project="$(MSBuildToolsPath)\\Microsoft.CSharp.targets" />
</Project>
"""

# ── Properties/AssemblyInfo.cs ─────────────────────────────────
files["Properties/AssemblyInfo.cs"] = """\
using System.Reflection;
[assembly: AssemblyTitle("EmailSender.Data")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
"""

# ── DatabaseHelper.cs ──────────────────────────────────────────
files["DatabaseHelper.cs"] = """\
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
"""

# ── MySqlHelper.cs ─────────────────────────────────────────────
files["MySqlHelper.cs"] = """\
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
"""

# ── Repositories/AppConfigRepository.cs ───────────────────────
files["Repositories/AppConfigRepository.cs"] = """\
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
"""

# ── Repositories/EmailAddressRepository.cs ────────────────────
files["Repositories/EmailAddressRepository.cs"] = """\
using System;
using System.Collections.Generic;
using Dapper;
using EmailSender.Models;

namespace EmailSender.Data.Repositories
{
    public class EmailAddressRepository
    {
        public int Add(EmailAddress item)
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

        public void AddBatch(IEnumerable<EmailAddress> items)
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

        public EmailAddress GetById(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.QueryFirstOrDefault<EmailAddress>(
                    "SELECT * FROM EmailAddress WHERE Id=@Id", new { Id = id });
            }
        }

        public List<EmailAddress> GetByListId(int listId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                return conn.Query<EmailAddress>(
                    "SELECT * FROM EmailAddress WHERE ListId=@ListId AND IsValid=1",
                    new { ListId = listId }).AsList();
            }
        }

        /// <summary>获取过滤后的发送列表（排除黑名单/已发/失败超限）</summary>
        public List<EmailAddress> GetFilteredForTask(
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
                return conn.Query<EmailAddress>(sql,
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
"""

# ── Repositories/EmailTemplateRepository.cs ───────────────────
files["Repositories/EmailTemplateRepository.cs"] = """\
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
"""

# ── Repositories/SendTaskRepository.cs ────────────────────────
files["Repositories/SendTaskRepository.cs"] = """\
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

        public void UpdateStatus(int id, TaskStatus status)
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
"""

# ── Repositories/SendRecordRepository.cs ──────────────────────
files["Repositories/SendRecordRepository.cs"] = """\
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
"""

# ── Repositories/SenderAccountRepository.cs ───────────────────
files["Repositories/SenderAccountRepository.cs"] = """\
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
                         OAuthEmail,OAuthToken,OAuthRefreshToken,TokenExpiresAt,
                         SmtpHost,SmtpPort,SmtpUseSsl,SmtpUser,SmtpPassword,
                         SmtpFromEmail,SmtpFromName,IsActive,IsPaused,DailyLimit,
                         CreatedAt,UpdatedAt)
                    VALUES
                        (@Name,@AccountType,@ApiUser,@ApiKey,
                         @OAuthEmail,@OAuthToken,@OAuthRefreshToken,@TokenExpiresAt,
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
                        OAuthEmail=@OAuthEmail, OAuthToken=@OAuthToken,
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
"""

# ── Repositories/BlacklistRepository.cs ───────────────────────
files["Repositories/BlacklistRepository.cs"] = """\
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
"""

# ── 写入所有文件 ───────────────────────────────────────────────
for rel_path, content in files.items():
    full_path = os.path.join(base, rel_path)
    os.makedirs(os.path.dirname(full_path), exist_ok=True)
    with open(full_path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"  ✅ 已生成: {full_path}")

print(f"\n🎉 EmailSender.Data 项目文件生成完毕！共 {len(files)} 个文件")
print(f"📁 输出目录: ./{base}/")

