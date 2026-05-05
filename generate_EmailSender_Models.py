import os

base = "EmailSender.Models"
os.makedirs(base, exist_ok=True)

files = {}

# ── EmailSender.Models.csproj ──────────────────────────────────
files["EmailSender.Models.csproj"] = """\
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props" />
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProjectGuid>{11111111-1111-1111-1111-111111111111}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>EmailSender.Models</RootNamespace>
    <AssemblyName>EmailSender.Models</AssemblyName>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="AppConfig.cs" />
    <Compile Include="Blacklist.cs" />
    <Compile Include="EmailAddress.cs" />
    <Compile Include="EmailTemplate.cs" />
    <Compile Include="Enums.cs" />
    <Compile Include="SendRecord.cs" />
    <Compile Include="SenderAccount.cs" />
    <Compile Include="SendTask.cs" />
    <Compile Include="Properties\\AssemblyInfo.cs" />
  </ItemGroup>
  <Import Project="$(MSBuildToolsPath)\\Microsoft.CSharp.targets" />
</Project>
"""

# ── Properties/AssemblyInfo.cs ─────────────────────────────────
os.makedirs(f"{base}/Properties", exist_ok=True)
files["Properties/AssemblyInfo.cs"] = """\
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("EmailSender.Models")]
[assembly: AssemblyDescription("Data model layer for EmailSender")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: ComVisible(false)]
"""

# ── Enums.cs ───────────────────────────────────────────────────
files["Enums.cs"] = """\
namespace EmailSender.Models
{
    /// <summary>发送任务状态</summary>
    public enum TaskStatus
    {
        Pending  = 0,   // 待执行
        Running  = 1,   // 执行中
        Paused   = 2,   // 已暂停
        Done     = 3,   // 已完成
        Failed   = 4    // 失败
    }

    /// <summary>发送记录状态</summary>
    public enum SendStatus
    {
        Pending   = 0,  // 待发送
        Sent      = 1,  // 已提交
        Delivered = 2,  // 已送达
        Opened    = 3,  // 已打开
        Clicked   = 4,  // 已点击
        Bounced   = 5,  // 退信
        Failed    = 6,  // 发送失败
        Spam      = 7   // 垃圾邮件举报
    }

    /// <summary>发送通道</summary>
    public enum SendChannel
    {
        SendCloud = 0,
        Gmail     = 1,
        Hotmail   = 2,
        SMTP      = 3
    }

    /// <summary>账户类型</summary>
    public enum AccountType
    {
        SendCloud = 0,
        Gmail     = 1,
        Hotmail   = 2,
        SMTP      = 3
    }

    /// <summary>邮件地址验证状态</summary>
    public enum VerifyStatus
    {
        Unknown  = 0,
        Valid    = 1,
        Invalid  = 2,
        CatchAll = 3,
        SpamTrap = 4
    }

    /// <summary>黑名单类型</summary>
    public enum BlacklistType
    {
        Manual       = 0,   // 手动添加
        HardBounce   = 1,   // 硬退信
        SpamReport   = 2,   // 垃圾举报
        Unsubscribe  = 3,   // 退订
        RepeatedFail = 4,   // 多次失败
        Invalid      = 5    // 无效地址
    }
}
"""

# ── AppConfig.cs ───────────────────────────────────────────────
files["AppConfig.cs"] = """\
using System;

namespace EmailSender.Models
{
    /// <summary>
    /// 应用配置键值对，存储在 SQLite config 表中
    /// </summary>
    public class AppConfig
    {
        public int    Id        { get; set; }
        public string Key       { get; set; }
        public string Value     { get; set; }
        public string Remark    { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
"""

# ── EmailAddress.cs ────────────────────────────────────────────
files["EmailAddress.cs"] = """\
using System;

namespace EmailSender.Models
{
    /// <summary>
    /// 邮件地址 + 联系人信息
    /// </summary>
    public class EmailAddress
    {
        public int        Id           { get; set; }
        public int        ListId       { get; set; }   // 所属列表ID（meetby listId）
        public string     ListName     { get; set; }   // 列表名称（冗余，方便显示）
        public string     Email        { get; set; }
        public string     FirstName    { get; set; }
        public string     LastName     { get; set; }
        public string     Company      { get; set; }
        public string     Domain       { get; set; }   // 自动从Email提取

        // 验证状态
        public VerifyStatus VerifyStatus { get; set; } = VerifyStatus.Unknown;
        public DateTime?  VerifiedAt   { get; set; }

        // 发送统计
        public bool       IsValid      { get; set; } = true;
        public int        FailCount    { get; set; } = 0;
        public DateTime?  LastSentAt   { get; set; }
        public int        TotalSent    { get; set; } = 0;

        // 元数据
        public DateTime   CreatedAt    { get; set; } = DateTime.Now;
        public DateTime   UpdatedAt    { get; set; } = DateTime.Now;
    }
}
"""

# ── EmailTemplate.cs ───────────────────────────────────────────
files["EmailTemplate.cs"] = """\
using System;

namespace EmailSender.Models
{
    /// <summary>
    /// 邮件模版（从 meetby 同步，可推送到 SendCloud）
    /// </summary>
    public class EmailTemplate
    {
        public int      Id                  { get; set; }
        public string   MeetbyTemplateId    { get; set; }  // meetby 平台模版ID
        public string   SendCloudTemplateId { get; set; }  // SendCloud 模版ID
        public string   Name                { get; set; }  // 模版名称
        public string   Subject             { get; set; }  // 邮件主题
        public string   HtmlBody            { get; set; }  // HTML 内容
        public string   TextBody            { get; set; }  // 纯文本内容（可选）
        public string   FromName            { get; set; }  // 发件人名称
        public string   FromEmail           { get; set; }  // 发件人地址

        // 同步状态
        public bool     NeedResync          { get; set; } = false;
        public bool     SyncedToSendCloud   { get; set; } = false;
        public DateTime? SyncedAt           { get; set; }

        // AI 分析结果
        public int?     AiScore             { get; set; }  // 0-100 反垃圾评分
        public string   AiIssues            { get; set; }  // JSON 问题列表
        public string   AiSuggestions       { get; set; }  // JSON 建议列表
        public DateTime? AiAnalyzedAt       { get; set; }

        // 元数据
        public DateTime CreatedAt           { get; set; } = DateTime.Now;
        public DateTime UpdatedAt           { get; set; } = DateTime.Now;
    }
}
"""

# ── SendTask.cs ────────────────────────────────────────────────
files["SendTask.cs"] = """\
using System;

namespace EmailSender.Models
{
    /// <summary>
    /// 发送任务
    /// </summary>
    public class SendTask
    {
        public int         Id              { get; set; }
        public string      Name            { get; set; }   // 任务名称

        // 关联
        public int         TemplateId      { get; set; }   // EmailTemplate.Id
        public string      TemplateName    { get; set; }   // 冗余显示
        public int         ListId          { get; set; }   // 邮件列表ID
        public string      ListName        { get; set; }   // 冗余显示
        public int         AccountId       { get; set; }   // SenderAccount.Id
        public string      AccountName     { get; set; }   // 冗余显示

        // 发送配置
        public SendChannel Channel         { get; set; } = SendChannel.SendCloud;
        public DateTime?   ScheduledAt     { get; set; }   // null = 立即发送
        public int         IntervalSeconds { get; set; } = 5;
        public int         ThreadCount     { get; set; } = 3;
        public int         RetryMax        { get; set; } = 3;
        public int         BatchSize       { get; set; } = 50;

        // 过滤条件（JSON存储）
        public string      FilterConfig    { get; set; }

        // 状态
        public TaskStatus  Status          { get; set; } = TaskStatus.Pending;
        public DateTime?   StartedAt       { get; set; }
        public DateTime?   FinishedAt      { get; set; }

        // 统计
        public int         TotalCount      { get; set; } = 0;
        public int         SuccessCount    { get; set; } = 0;
        public int         FailCount       { get; set; } = 0;
        public int         OpenCount       { get; set; } = 0;
        public int         ClickCount      { get; set; } = 0;
        public int         BounceCount     { get; set; } = 0;

        // 计算属性
        public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;
        public double OpenRate    => SuccessCount > 0 ? (double)OpenCount / SuccessCount * 100 : 0;

        // 元数据
        public DateTime    CreatedAt       { get; set; } = DateTime.Now;
        public DateTime    UpdatedAt       { get; set; } = DateTime.Now;
    }
}
"""

# ── SendRecord.cs ──────────────────────────────────────────────
files["SendRecord.cs"] = """\
using System;

namespace EmailSender.Models
{
    /// <summary>
    /// 每封邮件的发送记录
    /// </summary>
    public class SendRecord
    {
        public int        Id                { get; set; }
        public int        TaskId            { get; set; }   // SendTask.Id
        public int        EmailAddressId    { get; set; }   // EmailAddress.Id
        public string     Email             { get; set; }   // 冗余，方便查询

        // 发送结果
        public SendStatus SendStatus        { get; set; } = SendStatus.Pending;
        public string     SendCloudMsgId    { get; set; }   // SendCloud 返回的消息ID
        public string     ErrorMessage      { get; set; }   // 失败原因
        public int        RetryCount        { get; set; } = 0;

        // 结果拉取
        public bool       ResultFetched     { get; set; } = false;
        public DateTime?  FetchedAt         { get; set; }

        // 时间
        public DateTime?  SentAt            { get; set; }
        public DateTime?  DeliveredAt       { get; set; }
        public DateTime?  OpenedAt          { get; set; }
        public DateTime?  ClickedAt         { get; set; }
        public DateTime?  BouncedAt         { get; set; }

        public DateTime   CreatedAt         { get; set; } = DateTime.Now;
    }
}
"""

# ── SenderAccount.cs ───────────────────────────────────────────
files["SenderAccount.cs"] = """\
using System;

namespace EmailSender.Models
{
    /// <summary>
    /// 发送账户（SendCloud / Gmail / Hotmail / SMTP）
    /// </summary>
    public class SenderAccount
    {
        public int         Id              { get; set; }
        public string      Name            { get; set; }   // 账户别名
        public AccountType AccountType     { get; set; }

        // SendCloud
        public string      ApiUser         { get; set; }
        public string      ApiKey          { get; set; }

        // Gmail / Hotmail OAuth
        public string      OAuthEmail      { get; set; }
        public string      OAuthToken      { get; set; }   // 加密存储
        public string      OAuthRefreshToken { get; set; } // 加密存储
        public DateTime?   TokenExpiresAt  { get; set; }

        // SMTP
        public string      SmtpHost        { get; set; }
        public int         SmtpPort        { get; set; } = 587;
        public bool        SmtpUseSsl      { get; set; } = true;
        public string      SmtpUser        { get; set; }
        public string      SmtpPassword    { get; set; }  // 加密存储
        public string      SmtpFromEmail   { get; set; }
        public string      SmtpFromName    { get; set; }

        // 状态
        public bool        IsActive        { get; set; } = true;
        public bool        IsPaused        { get; set; } = false;
        public string      PauseReason     { get; set; }

        // 统计
        public int         TotalSent       { get; set; } = 0;
        public int         TodaySent       { get; set; } = 0;
        public int         DailyLimit      { get; set; } = 0;  // 0=不限制
        public DateTime?   LastSentAt      { get; set; }

        public DateTime    CreatedAt       { get; set; } = DateTime.Now;
        public DateTime    UpdatedAt       { get; set; } = DateTime.Now;
    }
}
"""

# ── Blacklist.cs ───────────────────────────────────────────────
files["Blacklist.cs"] = """\
using System;

namespace EmailSender.Models
{
    /// <summary>
    /// 黑名单（自动或手动加入）
    /// </summary>
    public class Blacklist
    {
        public int           Id          { get; set; }
        public string        Email       { get; set; }
        public BlacklistType Type        { get; set; }
        public string        Reason      { get; set; }   // 备注原因

        // 关联任务（自动加入时记录来源）
        public int?          TaskId      { get; set; }
        public int?          SendRecordId { get; set; }

        // 统计
        public int           FailCount   { get; set; } = 0;
        public int           TotalSent   { get; set; } = 0;
        public DateTime?     LastSentAt  { get; set; }

        public DateTime      CreatedAt   { get; set; } = DateTime.Now;
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

print(f"\n🎉 EmailSender.Models 项目文件生成完毕！共 {len(files)} 个文件")
print(f"📁 输出目录: ./{base}/")