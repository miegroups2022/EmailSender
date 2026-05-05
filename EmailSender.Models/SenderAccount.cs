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
        public int         DailyLimit      { get; set; } = 0;  // 每日最大发信量// 0=不限制
        public DateTime?   LastSentAt      { get; set; }

        public DateTime    CreatedAt       { get; set; } = DateTime.Now;
        public DateTime    UpdatedAt       { get; set; } = DateTime.Now;

    
        public int IntervalMin { get; set; }       // 发送间隔最小秒
        public int IntervalMax { get; set; }       // 发送间隔最大秒
        public bool NeedVpn { get; set; }       // 是否需要VPN
    }
}
