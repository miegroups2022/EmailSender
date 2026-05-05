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
