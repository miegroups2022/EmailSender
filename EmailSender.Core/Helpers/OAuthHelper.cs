using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using EmailSender.Data.Repositories;
using EmailSender.Models;

namespace EmailSender.Core.Helpers
{
    /// <summary>
    /// OAuth 2.0 授权助手
    /// 支持：Gmail (Google OAuth 2.0) / Hotmail (Microsoft OAuth 2.0)
    /// 流程：GenerateAuthUrl → 浏览器授权 → 本地回调监听 → 换Token → 自动刷新
    /// </summary>
    public class OAuthHelper
    {
        private readonly HttpClient              _http = new HttpClient();
        private readonly SenderAccountRepository _accountRepo;
        private readonly AppConfigRepository     _configRepo;

        private const string GoogleAuthUrl  = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string GoogleTokenUrl = "https://oauth2.googleapis.com/token";
        private const string GoogleScope    = "https://mail.google.com/";

        private const string MsAuthUrl      = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
        private const string MsTokenUrl     = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
        private const string MsScope        = "https://outlook.office.com/SMTP.Send offline_access";

        private const int    CallbackPort   = 9988;
        private const string RedirectUri    = "http://localhost:9988/callback";

        // ✅ 新增：构造函数
        public OAuthHelper(SenderAccountRepository accountRepo, AppConfigRepository configRepo)
        {
            _accountRepo = accountRepo;
            _configRepo = configRepo;
        }

        // ── 1. 生成授权 URL ────────────────────────────────────

        public string StartGmailAuth(int accountId)
        {
            var clientId = _configRepo.Get("GoogleClientId")
                ?? throw new Exception("未配置 Google Client ID，请在系统配置中填写");

            var url = GoogleAuthUrl
                + "?client_id="     + Uri.EscapeDataString(clientId)
                + "&redirect_uri="  + Uri.EscapeDataString(RedirectUri)
                + "&response_type=code"
                + "&scope="         + Uri.EscapeDataString(GoogleScope)
                + "&access_type=offline"
                + "&prompt=consent"
                + "&state=gmail_"   + accountId;

            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            return url;
        }

        public string StartHotmailAuth(int accountId)
        {
            var clientId = _configRepo.Get("MicrosoftClientId")
                ?? throw new Exception("未配置 Microsoft Client ID，请在系统配置中填写");

            var url = MsAuthUrl
                + "?client_id="     + Uri.EscapeDataString(clientId)
                + "&redirect_uri="  + Uri.EscapeDataString(RedirectUri)
                + "&response_type=code"
                + "&scope="         + Uri.EscapeDataString(MsScope)
                + "&state=hotmail_" + accountId;

            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            return url;
        }

        // ── 2. 本地监听回调 ────────────────────────────────────

        public async Task<(string code, string state)> WaitForCallbackAsync(
            int timeoutSeconds = 120)
        {
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add("http://localhost:" + CallbackPort + "/callback/");
                listener.Start();

                var contextTask = listener.GetContextAsync();
                var timeoutTask = Task.Delay(timeoutSeconds * 1000);
                var completed   = await Task.WhenAny(contextTask, timeoutTask);

                if (completed == timeoutTask)
                {
                    listener.Stop();
                    throw new TimeoutException("OAuth 授权超时，请重试");
                }

                var context = await contextTask;
                var query   = context.Request.QueryString;
                var code    = query["code"]  ?? "";
                var state   = query["state"] ?? "";
                var error   = query["error"] ?? "";

                var html = string.IsNullOrEmpty(error)
                    ? "<html><body><h2 style='color:green'>授权成功！请返回 EmailSender 继续操作。</h2></body></html>"
                    : "<html><body><h2 style='color:red'>授权失败：" + error + "</h2></body></html>";

                var buffer = Encoding.UTF8.GetBytes(html);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.ContentType     = "text/html; charset=utf-8";
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                context.Response.Close();
                listener.Stop();

                if (!string.IsNullOrEmpty(error))
                    throw new Exception("OAuth 授权被拒绝：" + error);

                return (code, state);
            }
        }

        // ── 3. 授权码换 Token ──────────────────────────────────

        public async Task ExchangeGmailCodeAsync(int accountId, string code)
        {
            var clientId     = _configRepo.Get("GoogleClientId")     ?? "";
            var clientSecret = _configRepo.Get("GoogleClientSecret") ?? "";
            var tokenData    = await PostTokenAsync(GoogleTokenUrl, new Dictionary<string, string>
            {
                ["code"]          = code,
                ["client_id"]     = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"]  = RedirectUri,
                ["grant_type"]    = "authorization_code",
            });
            SaveTokenToAccount(accountId, tokenData);
        }

        public async Task ExchangeHotmailCodeAsync(int accountId, string code)
        {
            var clientId     = _configRepo.Get("MicrosoftClientId")     ?? "";
            var clientSecret = _configRepo.Get("MicrosoftClientSecret") ?? "";
            var tokenData    = await PostTokenAsync(MsTokenUrl, new Dictionary<string, string>
            {
                ["code"]          = code,
                ["client_id"]     = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"]  = RedirectUri,
                ["grant_type"]    = "authorization_code",
                ["scope"]         = MsScope,
            });
            SaveTokenToAccount(accountId, tokenData);
        }

        // ── 4. 自动刷新 Token ──────────────────────────────────

        public async Task<string> EnsureValidTokenAsync(SenderAccount account)
        {
            // Token 还有 5 分钟以上有效期，直接返回
            if (account.TokenExpiresAt.HasValue &&
                account.TokenExpiresAt.Value > DateTime.Now.AddMinutes(5))
                return account.OAuthToken;

            if (string.IsNullOrEmpty(account.OAuthRefreshToken))
                throw new Exception("账户 [" + account.Name + "] 的 Refresh Token 为空，请重新授权");

            return await RefreshTokenAsync(account);
        }

        public async Task<string> RefreshTokenAsync(SenderAccount account)
        {
            string tokenUrl, clientId, clientSecret;

            if (account.AccountType == AccountType.Gmail)
            {
                tokenUrl     = GoogleTokenUrl;
                clientId     = _configRepo.Get("GoogleClientId")     ?? "";
                clientSecret = _configRepo.Get("GoogleClientSecret") ?? "";
            }
            else
            {
                tokenUrl     = MsTokenUrl;
                clientId     = _configRepo.Get("MicrosoftClientId")     ?? "";
                clientSecret = _configRepo.Get("MicrosoftClientSecret") ?? "";
            }

            var tokenData = await PostTokenAsync(tokenUrl, new Dictionary<string, string>
            {
                ["refresh_token"] = account.OAuthRefreshToken,
                ["client_id"]     = clientId,
                ["client_secret"] = clientSecret,
                ["grant_type"]    = "refresh_token",
            });

            SaveTokenToAccount(account.Id, tokenData);
            return tokenData.ContainsKey("access_token") ? tokenData["access_token"] : "";
        }

        // ── 私有方法 ───────────────────────────────────────────

        private async Task<Dictionary<string, string>> PostTokenAsync(
            string url, Dictionary<string, string> param)
        {
            var content  = new FormUrlEncodedContent(param);
            var resp     = await _http.PostAsync(url, content);
            var body     = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception("Token 请求失败 [" + resp.StatusCode + "]: " + body);

            var json   = JObject.Parse(body);
            var result = new Dictionary<string, string>();
            foreach (var prop in json.Properties())
                result[prop.Name] = prop.Value?.ToString() ?? "";
            return result;
        }

        private void SaveTokenToAccount(int accountId,
            Dictionary<string, string> tokenData)
        {
            var account = _accountRepo.GetById(accountId)
                ?? throw new Exception("账户 " + accountId + " 不存在");

            if (tokenData.ContainsKey("access_token"))
                account.OAuthToken = tokenData["access_token"];

            if (tokenData.ContainsKey("refresh_token"))
                account.OAuthRefreshToken = tokenData["refresh_token"];

            if (tokenData.TryGetValue("expires_in", out var expiresIn)
                && int.TryParse(expiresIn, out var seconds))
                account.TokenExpiresAt = DateTime.Now.AddSeconds(seconds);

            // ✅ 新增：标记为已授权
            account.IsOAuthAuthorized = true;

            _accountRepo.Update(account);
        }
    }
}
