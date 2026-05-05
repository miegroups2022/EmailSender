using System.Collections.Generic;

namespace EmailSender.Models
{
    public enum MailProvider
    {
        Mail163   = 0,
        QQ        = 1,
        Mail126   = 2,
        Outlook   = 3,
        Gmail     = 4,
        Yahoo     = 5,
        ICloud    = 6,
        ProtonMail = 7,
        Custom    = 8
    }

    public enum AuthType
    {
        SmtpPassword = 0,
        OAuth2       = 1
    }

    public class MailProviderConfig
    {
        public string  DisplayName     { get; set; }
        public string  SmtpHost        { get; set; }
        public int     SmtpPort        { get; set; }
        public bool    UseSsl          { get; set; }
        public AuthType AuthType       { get; set; }
        public int     DailyLimit      { get; set; }
        public int     MaxBccCount     { get; set; }
        public int     IntervalMinSec  { get; set; }
        public int     IntervalMaxSec  { get; set; }
        public bool    NeedVpn         { get; set; }
        public string  SetupGuide      { get; set; }
    }

    public static class MailProviderConfigs
    {
        public static readonly Dictionary<MailProvider, MailProviderConfig> Configs = new()
        {
            [MailProvider.Mail163] = new()
            {
                DisplayName    = "163邮箱",
                SmtpHost       = "smtp.163.com",
                SmtpPort       = 465,
                UseSsl         = true,
                AuthType       = AuthType.SmtpPassword,
                DailyLimit     = 200,
                MaxBccCount    = 30,
                IntervalMinSec = 60,
                IntervalMaxSec = 120,
                NeedVpn        = false,
                SetupGuide     = "登录163网页版 → 设置 → POP3/SMTP → 开启SMTP → 获取授权码（非登录密码）"
            },
            [MailProvider.QQ] = new()
            {
                DisplayName    = "QQ邮箱",
                SmtpHost       = "smtp.qq.com",
                SmtpPort       = 465,
                UseSsl         = true,
                AuthType       = AuthType.SmtpPassword,
                DailyLimit     = 200,
                MaxBccCount    = 30,
                IntervalMinSec = 60,
                IntervalMaxSec = 120,
                NeedVpn        = false,
                SetupGuide     = "登录QQ邮箱网页版 → 设置 → 账户 → 开启SMTP服务 → 获取授权码"
            },
            [MailProvider.Mail126] = new()
            {
                DisplayName    = "126邮箱",
                SmtpHost       = "smtp.126.com",
                SmtpPort       = 465,
                UseSsl         = true,
                AuthType       = AuthType.SmtpPassword,
                DailyLimit     = 200,
                MaxBccCount    = 30,
                IntervalMinSec = 60,
                IntervalMaxSec = 120,
                NeedVpn        = false,
                SetupGuide     = "登录126网页版 → 设置 → POP3/SMTP → 开启SMTP → 获取授权码"
            },
            [MailProvider.Outlook] = new()
            {
                DisplayName    = "Outlook / Hotmail",
                SmtpHost       = "smtp-mail.outlook.com",  // 官方正确地址
                SmtpPort       = 587,
                UseSsl         = false,  // 587 用 STARTTLS，不是 SslOnConnect
                AuthType       = AuthType.OAuth2,
                DailyLimit     = 300,
                MaxBccCount    = 50,
                IntervalMinSec = 90,
                IntervalMaxSec = 180,
                NeedVpn        = true,
                SetupGuide     = "需先开启VPN，点击「微软OAuth2授权」，在弹出浏览器中完成授权"
            },
            [MailProvider.Gmail] = new()
            {
                DisplayName    = "Gmail",
                SmtpHost       = "smtp.gmail.com",
                SmtpPort       = 465,
                UseSsl         = true,
                AuthType       = AuthType.OAuth2,
                DailyLimit     = 500,
                MaxBccCount    = 50,
                IntervalMinSec = 90,
                IntervalMaxSec = 180,
                NeedVpn        = true,
                SetupGuide     = "需先开启VPN，点击「Google授权登录」，在弹出浏览器中完成OAuth2授权"
            },
            [MailProvider.Yahoo] = new()
            {
                DisplayName    = "Yahoo Mail",
                SmtpHost       = "smtp.mail.yahoo.com",
                SmtpPort       = 587,
                UseSsl         = true,
                AuthType       = AuthType.SmtpPassword,
                DailyLimit     = 500,
                MaxBccCount    = 50,
                IntervalMinSec = 60,
                IntervalMaxSec = 120,
                NeedVpn        = true,
                SetupGuide     = "需要VPN。登录 Yahoo → 账户安全 → 生成应用密码 → 将16位密码填入密码栏"
            },
            [MailProvider.ICloud] = new()
            {
                DisplayName    = "iCloud Mail",
                SmtpHost       = "smtp.mail.me.com",
                SmtpPort       = 587,
                UseSsl         = true,
                AuthType       = AuthType.SmtpPassword,
                DailyLimit     = 200,
                MaxBccCount    = 50,
                IntervalMinSec = 60,
                IntervalMaxSec = 120,
                NeedVpn        = true,
                SetupGuide     = "需要VPN。前往 appleid.apple.com → 登录 → 应用专用密码 → 生成密码 → 填入密码栏。用户名填完整 @icloud.com 地址"
            },
            [MailProvider.ProtonMail] = new()
            {
                DisplayName    = "ProtonMail (Bridge)",
                SmtpHost       = "127.0.0.1",
                SmtpPort       = 1025,
                UseSsl         = false,
                AuthType       = AuthType.SmtpPassword,
                DailyLimit     = 150,
                MaxBccCount    = 50,
                IntervalMinSec = 60,
                IntervalMaxSec = 120,
                NeedVpn        = false,
                SetupGuide     = "需先在本机安装并运行 Proton Mail Bridge（proton.me/mail/bridge）。Bridge 运行后会在本机开启 SMTP 端口 1025，用户名和密码在 Bridge 界面查看"
            },
            [MailProvider.Custom] = new()
            {
                DisplayName    = "企业自定义邮箱",
                SmtpHost       = "",
                SmtpPort       = 465,
                UseSsl         = true,
                AuthType       = AuthType.SmtpPassword,
                DailyLimit     = 500,
                MaxBccCount    = 50,
                IntervalMinSec = 30,
                IntervalMaxSec = 90,
                NeedVpn        = false,
                SetupGuide     = "手动填写SMTP服务器地址、端口和账号密码"
            }
        };
    }
}
