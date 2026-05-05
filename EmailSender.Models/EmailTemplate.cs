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
