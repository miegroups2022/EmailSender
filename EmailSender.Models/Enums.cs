namespace EmailSender.Models
{
    /// <summary>发送任务状态</summary>
    public enum SendTaskStatus
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
