using EmailSender.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Identity.Client;
using MimeKit;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmailSender.Models;

namespace EmailSender.Core.Helpers
{
    /// <summary>
    /// Outlook / Hotmail OAuth2 授权 + SMTP XOAUTH2 发送服务。
    /// 使用 MSAL（Microsoft.Identity.Client）完成 Loopback 授权。
    ///
    /// 前置条件（一次性操作）：
    ///   1. 免费注册 Azure：https://azure.microsoft.com/free
    ///   2. 进入 portal.azure.com → Microsoft Entra ID → 应用注册 → 新注册
    ///      · 名称：随意，如 WaimaoTong
    ///      · 账户类型：任何组织目录中的账户和个人 Microsoft 账户
    ///      · 重定向 URI：平台=公共客户端/本机，URI=http://localhost
    ///   3. 注册完成后复制「应用程序（客户端）ID」
    ///   4. 在程序目录创建 outlook_client.json：
    ///      { "client_id": "你的-client-id" }
    ///   （Azure 免费版不收费，仅需信用卡验证身份）
    ///  
    ///  【我的账号】  tradechina_1997@hotmail.com
    ///  【显示名称】: waimaotong
    ///  【应用程序(客户端) ID】 : 3309f306-d709-4183-8480-dfbef1a0dac9
    ///  
    /// </summary>
    public static class OutlookOAuthService
    {
        private static readonly string[] Scopes =
        {
            "https://outlook.office.com/SMTP.Send",
            "offline_access",            // 获取 refresh_token，长期有效
            "openid", "email", "profile" // 获取用户邮箱地址
        };

        // outlook_client.json 路径（程序目录）
        private static string ClientConfigPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "outlook_client.json");

        // token 缓存目录
        private static string TokenCacheDir =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OutlookTokens");

        // ── 读取 Client ID ────────────────────────────────────

        private static string ReadClientId()
        {
            if (!File.Exists(ClientConfigPath))
                throw new FileNotFoundException(
                    $"未找到 outlook_client.json，请将文件放到程序目录：\n{ClientConfigPath}\n\n"
                  + "获取步骤（免费）：\n"
                  + "1. 注册 Azure 免费账号：azure.microsoft.com/free\n"
                  + "2. portal.azure.com → Microsoft Entra ID → 应用注册 → 新注册\n"
                  + "3. 账户类型选「任何组织目录中的账户和个人 Microsoft 账户」\n"
                  + "4. 重定向 URI：平台=公共客户端/本机，URI=http://localhost\n"
                  + "5. 复制「应用程序（客户端）ID」\n"
                  + "6. 创建文件内容：{ \"client_id\": \"粘贴ID\" }");

            var json = Newtonsoft.Json.Linq.JObject.Parse(
                File.ReadAllText(ClientConfigPath, System.Text.Encoding.UTF8));
            var id = json["client_id"]?.ToString();
            if (string.IsNullOrWhiteSpace(id))
                throw new InvalidOperationException("outlook_client.json 中缺少 client_id 字段");
            return id;
        }

        // ── 构建 MSAL 公共客户端 ──────────────────────────────

        private static IPublicClientApplication BuildApp(string accountKey)
        {
            var tokenPath = Path.Combine(TokenCacheDir, accountKey + ".json");
            Directory.CreateDirectory(TokenCacheDir);

            var app = PublicClientApplicationBuilder
                .Create(ReadClientId())
                // common = 同时支持个人账号 + 组织账号
                .WithAuthority("https://login.microsoftonline.com/common")
                .WithRedirectUri("http://localhost")   // Loopback，与 Azure 注册一致
                .Build();

            // 文件 token 缓存：程序关闭后 token 依然有效，无需每次重新授权
            app.UserTokenCache.SetBeforeAccess(args =>
            {
                if (File.Exists(tokenPath))
                    args.TokenCache.DeserializeMsalV3(File.ReadAllBytes(tokenPath));
            });
            app.UserTokenCache.SetAfterAccess(args =>
            {
                if (args.HasStateChanged)
                    File.WriteAllBytes(tokenPath, args.TokenCache.SerializeMsalV3());
            });

            return app;
        }

        // ── 授权入口（Loopback）────────────────────────────────

        public static async Task<(bool ok, string email, string tempKey, string tokenJson, string error)>
            AuthorizeAsync()
        {
            // 用时间戳生成临时 key；保存账号入库得到 Id 后调用 RenameTokenCache 改为正式 key
            string tempKey = $"tmp_{DateTime.Now.Ticks}";
            try
            {
                var app = BuildApp(tempKey);

                // 删除旧 token，确保弹出账号选择页面
                foreach (var a in await app.GetAccountsAsync())
                    await app.RemoveAsync(a);

                var result = await app
                    .AcquireTokenInteractive(Scopes)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync(CancellationToken.None);

                string email     = result.Account.Username;
                string tokenJson = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    AccessToken = result.AccessToken,
                    ExpiresOn   = result.ExpiresOn,
                    Account     = result.Account.Username
                });
                return (true, email, tempKey, tokenJson, null);
            }
            catch (Exception ex)
            {
                return (false, null, null, null, ex.Message);
            }
        }

        /// <summary>
        /// 账号入库拿到 Id 后调用，将临时 token 文件重命名为正式的 acc_{id}.json。
        /// </summary>
        public static void RenameTokenCache(string tempKey, int accountId)
        {
            var dir      = TokenCacheDir;
            var oldPath  = Path.Combine(dir, tempKey + ".json");
            var newPath  = Path.Combine(dir, $"acc_{accountId}.json");
            if (File.Exists(oldPath))
            {
                if (File.Exists(newPath)) File.Delete(newPath);
                File.Move(oldPath, newPath);
            }
        }

        // ── 静默获取 token（自动 refresh_token 续期）───────────

        public static async Task<AuthenticationResult> GetTokenAsync(
            SenderAccount account, string accountKey = null)
        {
            accountKey ??= $"acc_{account.Id}";
            var app      = BuildApp(accountKey);
            var accounts = await app.GetAccountsAsync();
            var msalAcc  = accounts.FirstOrDefault(
                a => string.Equals(a.Username, account.SmtpUser,
                                   StringComparison.OrdinalIgnoreCase));

            if (msalAcc == null)
                throw new InvalidOperationException(
                    $"未找到 {account.SmtpUser} 的 Outlook token，请重新点击授权按钮。");

            try
            {
                // 优先静默续期（不打开浏览器）
                return await app
                    .AcquireTokenSilent(Scopes, msalAcc)
                    .ExecuteAsync(CancellationToken.None);
            }
            catch (MsalUiRequiredException)
            {
                // refresh_token 也过期了（约90天），需要重新交互授权
                throw new InvalidOperationException(
                    $"{account.SmtpUser} 的授权已过期，请重新点击授权按钮。");
            }
        }

        // ── 发送邮件（SMTP + XOAUTH2）────────────────────────

        public static async Task SendAsync(
            SenderAccount account,
            string toEmail,
            string toName,
            string subject,
            string htmlBody,
            string accountKey = null)
        {
            var token  = await GetTokenAsync(account, accountKey);
            var oauth2 = new SaslMechanismOAuth2(account.OAuthEmail, token.AccessToken);

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp-mail.outlook.com", 587,
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(oauth2);

            // 用 MailMessageBuilder 构建：占位符替换 + 纯文本备用正文 + Reply-To
            var msg = MailMessageBuilder.Build(
                account.SmtpFromName, account.SmtpFromEmail,
                toName, toEmail, subject, htmlBody);

            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }

        // ── 测试（发给自己）──────────────────────────────────

        public static async Task TestAsync(SenderAccount account, Action<string> callback,
            string accountKey = null)
        {
            try
            {
                await SendAsync(account,
                    account.SmtpFromName, account.SmtpFromEmail,
                    "外贸通 Outlook OAuth2 测试邮件",
                    "<p>Outlook OAuth2 配置测试成功！</p>",
                    accountKey);
                callback("✅ 测试成功！Outlook OAuth2 配置正确，邮件已发送到您的邮箱。");
            }
            catch (Exception ex)
            {
                callback($"❌ 测试失败：{ex.Message}");
            }
        }
    }
}
