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
