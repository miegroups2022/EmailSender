using System;

namespace EmailSender.Models
{
    /// <summary>
    /// 邮件地址 + 联系人信息
    /// </summary>
    public class EmailRecipient
    {
        public int        Id           { get; set; }
        public int        ListId       { get; set; }   // 所属列表ID（meetby listId）
        public int GroupId { get; set; }   // ✅ 新增：分组ID
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
