import os

base = "EmailSender.Core/Helpers"
os.makedirs(base, exist_ok=True)

files = {}

# ══════════════════════════════════════════════════════════════
# Helpers/EmailValidator.cs
# ══════════════════════════════════════════════════════════════
files["EmailValidator.cs"] = """\
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EmailSender.Core.Helpers
{
    /// <summary>
    /// 本地邮件地址格式验证（纯本地，无需网络）
    /// 支持：格式校验、无效域名过滤、批量过滤、域名提取、地址标准化
    /// </summary>
    public static class EmailValidator
    {
        // RFC 5322 简化版正则（注意：C# 字符串中 \\- 是正则的 \-）
        private static readonly Regex _regex = new Regex(
            @"^[a-zA-Z0-9._%+\\-]+@[a-zA-Z0-9.\\-]+\\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>常见无效/临时邮件域名黑名单</summary>
        private static readonly HashSet<string> _invalidDomains =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "example.com", "example.org", "example.net",
            "test.com",    "test.org",    "test.net",
            "localhost",   "invalid.com", "nowhere.com",
            "mailinator.com",   "guerrillamail.com", "tempmail.com",
            "throwam.com",      "yopmail.com",       "sharklasers.com",
            "spam4.me",         "trashmail.com",     "dispostable.com",
            "maildrop.cc",      "mailnull.com",      "spamgourmet.com",
            "trashmail.at",     "trashmail.io",      "fakeinbox.com",
            "spambox.us",       "mytrashmail.com",   "discard.email",
        };

        public static bool IsValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            email = email.Trim();
            if (email.Length > 254) return false;
            if (!_regex.IsMatch(email)) return false;
            var domain = ExtractDomain(email);
            if (string.IsNullOrEmpty(domain))     return false;
            if (_invalidDomains.Contains(domain)) return false;
            var local = email.Split('@')[0];
            if (local.StartsWith(".") || local.EndsWith(".")) return false;
            if (local.Contains(".."))                          return false;
            return true;
        }

        public static List<string> FilterInvalid(IEnumerable<string> emails)
            => emails.Where(e => !IsValid(e)).ToList();

        public static List<string> FilterValid(IEnumerable<string> emails)
            => emails.Where(IsValid).ToList();

        public static string ExtractDomain(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "";
            var idx = email.IndexOf('@');
            if (idx < 0 || idx == email.Length - 1) return "";
            return email.Substring(idx + 1).Trim().ToLower();
        }

        public static string Normalize(string email)
            => email?.Trim().ToLower() ?? "";

        public static List<string> NormalizeAndDedup(IEnumerable<string> emails)
        {
            var seen   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<string>();
            foreach (var e in emails)
            {
                var norm = Normalize(e);
                if (!string.IsNullOrEmpty(norm) && seen.Add(norm))
                    result.Add(norm);
            }
            return result;
        }

        public static Dictionary<string, int> GetDomainDistribution(
            IEnumerable<string> emails, int topN = 20)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in emails)
            {
                var domain = ExtractDomain(e);
                if (string.IsNullOrEmpty(domain)) continue;
                dict[domain] = dict.ContainsKey(domain) ? dict[domain] + 1 : 1;
            }
            return dict
                .OrderByDescending(kv => kv.Value)
                .Take(topN)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public static bool IsFreeMailDomain(string email)
        {
            var domain = ExtractDomain(email);
            var free   = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "gmail.com","googlemail.com",
                "hotmail.com","hotmail.co.uk","outlook.com","live.com","msn.com",
                "yahoo.com","yahoo.co.uk","yahoo.co.jp","yahoo.fr","yahoo.de",
                "qq.com","163.com","126.com","sina.com","sohu.com",
                "icloud.com","me.com","mac.com",
                "protonmail.com","proton.me",
            };
            return free.Contains(domain);
        }
    }
}
"""

# ══════════════════════════════════════════════════════════════
# Helpers/TemplateRenderer.cs
# ══════════════════════════════════════════════════════════════
files["TemplateRenderer.cs"] = """\
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using EmailSender.Models;

namespace EmailSender.Core.Helpers
{
    /// <summary>
    /// 邮件模版变量替换引擎
    /// 支持格式：
    ///   {{first_name}}  —— 双花括号格式（本地 SMTP 发送用）
    ///   %first_name%    —— 百分号格式（SendCloud xsmtpapi 变量格式）
    /// 内置变量：first_name / last_name / full_name / company / email / domain
    /// </summary>
    public class TemplateRenderer
    {
        private string _subject;
        private string _htmlBody;
        private string _textBody;

        public void LoadTemplate(string subject, string htmlBody, string textBody = null)
        {
            _subject  = subject  ?? "";
            _htmlBody = htmlBody ?? "";
            _textBody = textBody ?? "";
        }

        public string RenderSubject(EmailAddress addr)
            => Render(_subject, BuildVars(addr));

        public string RenderHtmlBody(EmailAddress addr)
            => Render(_htmlBody, BuildVars(addr));

        public string RenderTextBody(EmailAddress addr)
            => Render(_textBody, BuildVars(addr));

        public string Render(string template, EmailAddress addr)
            => Render(template, BuildVars(addr));

        public string Render(string template, Dictionary<string, string> vars)
        {
            if (string.IsNullOrEmpty(template)) return template ?? "";
            var sb = new StringBuilder(template);
            foreach (var kv in vars)
            {
                var val = kv.Value ?? "";
                sb.Replace("{{" + kv.Key + "}}", val);
                sb.Replace("%" + kv.Key + "%",   val);
            }
            return sb.ToString();
        }

        // 提取 {{var}} 格式变量 —— 注意 C# verbatim string 里写 \\{\\{ 即可
        public static List<string> ExtractDoubleBraceVars(string template)
        {
            var result  = new List<string>();
            var matches = Regex.Matches(template ?? "", @"\\{\\{(\\w+)\\}\\}");
            foreach (Match m in matches)
            {
                var name = m.Groups[1].Value;
                if (!result.Contains(name)) result.Add(name);
            }
            return result;
        }

        // 提取 %var% 格式变量
        public static List<string> ExtractPercentVars(string template)
        {
            var result  = new List<string>();
            var matches = Regex.Matches(template ?? "", @"%(\\w+)%");
            foreach (Match m in matches)
            {
                var name = m.Groups[1].Value;
                if (!result.Contains(name)) result.Add(name);
            }
            return result;
        }

        public static List<string> ExtractAllVars(string template)
        {
            var result = ExtractDoubleBraceVars(template);
            foreach (var v in ExtractPercentVars(template))
                if (!result.Contains(v)) result.Add(v);
            return result;
        }

        public string RenderPreview(string template)
        {
            var sample = new Dictionary<string, string>
            {
                ["first_name"] = "张",
                ["last_name"]  = "三",
                ["full_name"]  = "张三",
                ["company"]    = "示例公司",
                ["email"]      = "zhangsan@example.com",
                ["domain"]     = "example.com",
            };
            return Render(template, sample);
        }

        private Dictionary<string, string> BuildVars(EmailAddress addr)
        {
            var firstName = addr?.FirstName ?? "";
            var lastName  = addr?.LastName  ?? "";
            var fullName  = (firstName + " " + lastName).Trim();
            return new Dictionary<string, string>
            {
                ["first_name"] = firstName,
                ["last_name"]  = lastName,
                ["full_name"]  = fullName,
                ["name"]       = fullName,
                ["company"]    = addr?.Company ?? "",
                ["email"]      = addr?.Email   ?? "",
                ["domain"]     = addr?.Domain  ?? "",
            };
        }
    }
}
"""

# ══════════════════════════════════════════════════════════════
# Helpers/OAuthHelper.cs
# ══════════════════════════════════════════════════════════════
files["OAuthHelper.cs"] = """\
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

        public OAuthHelper(SenderAccountRepository accountRepo,
                           AppConfigRepository configRepo)
        {
            _accountRepo = accountRepo;
            _configRepo  = configRepo;
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

            _accountRepo.Update(account);
        }
    }
}
"""

# ── 写入所有文件 ───────────────────────────────────────────────
for filename, content in files.items():
    full_path = os.path.join(base, filename)
    with open(full_path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"  ✅ 已生成: {full_path}")

print(f"\n🎉 EmailSender.Core/Helpers 生成完毕！共 {len(files)} 个文件")
print(f"📁 输出目录: ./{base}/")
print()
print("文件清单：")
for f in files:
    print(f"  📄 {f}")


