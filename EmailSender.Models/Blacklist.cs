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
