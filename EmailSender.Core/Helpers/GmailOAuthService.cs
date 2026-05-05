
using MimeKit;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EmailSender.Models;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;              // ✅ 添加这行
using Google.Apis.Gmail.v1.Data;         // ✅ 添加这行

using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace EmailSender.Core.Helpers
{
    /// <summary>
    /// Gmail OAuth2 Loopback 授权 + 发送服务。
    /// credentials.json 放在程序目录，从 Google Cloud Console 下载
    /// （凭证类型选"桌面应用 Desktop app"）。
    /// </summary>
    public static class GmailOAuthService
    {
        private static readonly string[] Scopes =
        {
            GmailService.Scope.GmailSend,
            "https://www.googleapis.com/auth/userinfo.email"  // 读取授权账号的邮箱地址
        };

        // credentials.json 路径（程序目录）
        private static string CredentialsPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.json");

        // token 存储目录（程序目录下 GmailTokens\{accountId}）
        private static string TokenFolder(string accountKey) =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GmailTokens", accountKey);

        /// <summary>
        /// 发起 Loopback OAuth2 授权流程，成功后返回用户邮箱地址。
        /// 会自动打开系统浏览器，用户授权后程序捕获回调。
        /// </summary>
        public static async Task<(bool ok, string email, string tokenJson, string error)>
            AuthorizeAsync(string accountKey = "default")
        {
            if (!File.Exists(CredentialsPath))
                return (false, null, null,
                    $"未找到 credentials.json，请将文件放到程序目录：\n{CredentialsPath}\n\n"
                  + "获取方法：\n"
                  + "1. 打开 console.cloud.google.com\n"
                  + "2. 创建项目 → 启用 Gmail API\n"
                  + "3. 凭据 → 创建 OAuth2 凭据 → 类型选「桌面应用」\n"
                  + "4. 下载 JSON 文件，改名为 credentials.json 放到程序目录");

            try
            {
                UserCredential credential;
                var tokenDir = TokenFolder(accountKey);

                // 彻底删除旧 token 目录，确保用完整 scope 重新授权
                // （FileDataStore.DeleteAsync 的 key 编码规则与文件名不一致，不可靠）
                if (Directory.Exists(tokenDir))
                    Directory.Delete(tokenDir, recursive: true);

                var store = new FileDataStore(tokenDir, fullPath: true);
                using var stream = new FileStream(CredentialsPath, FileMode.Open, FileAccess.Read);
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    accountKey,
                    CancellationToken.None,
                    store);

                // 确保 AccessToken 有效（刚授权时可能尚未填充）
                if (string.IsNullOrEmpty(credential.Token.AccessToken) || credential.Token.IsStale)
                    await credential.RefreshTokenAsync(CancellationToken.None);

                // 获取授权账号的邮箱地址（用 access_token 直接调 Google userinfo 端点）
                using var httpClient = new System.Net.Http.HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", credential.Token.AccessToken);
                var response    = await httpClient.GetAsync(
                    "https://www.googleapis.com/oauth2/v3/userinfo");
                var json        = await response.Content.ReadAsStringAsync();
                var userinfoObj = Newtonsoft.Json.Linq.JObject.Parse(json);

                // userinfo 端点返回 "email" 字段；若因 scope 不足返回空，回退到 id_token 解码
                string emailAddress = userinfoObj["email"]?.ToString();
                if (string.IsNullOrEmpty(emailAddress))
                {
                    // 从 id_token JWT payload 里解码 email（无需签名验证，只读 payload）
                    var idToken = credential.Token.IdToken;
                    if (!string.IsNullOrEmpty(idToken))
                    {
                        var parts   = idToken.Split('.');
                        if (parts.Length >= 2)
                        {
                            // Base64Url → Base64
                            var payload = parts[1];
                            payload = payload.Replace('-', '+').Replace('_', '/');
                            switch (payload.Length % 4)
                            {
                                case 2: payload += "=="; break;
                                case 3: payload += "=";  break;
                            }
                            var decoded = System.Text.Encoding.UTF8.GetString(
                                Convert.FromBase64String(payload));
                            var jwtObj  = Newtonsoft.Json.Linq.JObject.Parse(decoded);
                            emailAddress = jwtObj["email"]?.ToString() ?? "";
                        }
                    }
                }

                // 序列化 token 以便存入数据库
                string tokenJson = JsonConvert.SerializeObject(credential.Token);
                return (true, emailAddress ?? "", tokenJson, null);
            }
            catch (Exception ex)
            {
                return (false, null, null, ex.Message);
            }
        }

        /// <summary>
        /// 用已保存的 token 重建 UserCredential，若 token 过期会自动续期。
        /// </summary>
        public static async Task<UserCredential> GetCredentialAsync(
            SenderAccount account, string accountKey = null)
        {
            accountKey ??= $"acc_{account.Id}";

            if (!File.Exists(CredentialsPath))
                throw new FileNotFoundException(
                    $"未找到 credentials.json，路径：{CredentialsPath}");

            // 如果本地 token 文件夹不存在但数据库里有 tokenJson，先还原到文件
            var tokenDir = TokenFolder(accountKey);
            var tokenFile = Path.Combine(tokenDir, "Google.Apis.Auth.OAuth2.Responses.TokenResponse-" + accountKey);
            if (!File.Exists(tokenFile) && !string.IsNullOrEmpty(account.OAuthTokenJson))
            {
                Directory.CreateDirectory(tokenDir);
                File.WriteAllText(tokenFile, account.OAuthTokenJson, Encoding.UTF8);
            }

            using var stream = new FileStream(CredentialsPath, FileMode.Open, FileAccess.Read);
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                accountKey,
                CancellationToken.None,
                new FileDataStore(tokenDir, fullPath: true));

            // 若 AccessToken 已过期，自动用 RefreshToken 刷新
            if (credential.Token.IsStale)
                await credential.RefreshTokenAsync(CancellationToken.None);

            return credential;
        }

        /// <summary>
        /// 使用 Gmail API 发送一封邮件。
        /// </summary>
        public static async Task SendAsync(
            SenderAccount account,
            string toEmail,
            string toName,
            string subject,
            string htmlBody)
        {
            var credential = await GetCredentialAsync(account, $"acc_{account.Id}");
            var service    = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName       = "WaimaoTong"
            });

            // 用 MailMessageBuilder 构建：占位符替换 + 纯文本备用正文 + Reply-To
            var msg = MailMessageBuilder.Build(
                account.SmtpFromName, account.SmtpFromEmail,
                toName, toEmail, subject, htmlBody);

            using var ms = new MemoryStream();
            await msg.WriteToAsync(ms);
            string raw = Convert.ToBase64String(ms.ToArray())
                .Replace('+', '-').Replace('/', '_').Replace("=", "");

            var gmailMsg = new Google.Apis.Gmail.v1.Data.Message { Raw = raw };
            await service.Users.Messages.Send(gmailMsg, "me").ExecuteAsync();
        }

        /// <summary>
        /// 测试 Gmail OAuth 账号（发一封测试邮件给自己）。
        /// </summary>
        public static async Task TestAsync(SenderAccount account, Action<string> callback)
        {
            try
            {
                await SendAsync(account,
                    account.SmtpFromEmail, account.SmtpFromName,
                    "外贸通 Gmail OAuth2 测试邮件",
                    "<p>Gmail OAuth2 配置测试成功！</p>");
                callback("✅ 测试成功！Gmail OAuth2 配置正确，邮件已发送到您的邮箱。");
            }
            catch (Exception ex)
            {
                callback($"❌ 测试失败：{ex.Message}");
            }
        }
    }
}
