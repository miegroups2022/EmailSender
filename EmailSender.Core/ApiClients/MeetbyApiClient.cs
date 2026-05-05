using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EmailSender.Models;

namespace EmailSender.Core.ApiClients
{
    /// <summary>
    /// meetby EDM 平台 API 客户端
    /// 负责：登录获取Token、拉取模版列表、拉取邮件列表及成员
    /// </summary>
    public class MeetbyApiClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private string _token;

        public MeetbyApiClient(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _http = new HttpClient();
            _http.Timeout = TimeSpan.FromSeconds(30);
        }

        // ── 登录 ────────────────────────────────────────────────
        /// <summary>登录并返回 Token，失败抛出异常</summary>
        public async Task<string> LoginAsync(string username, string password)
        {
            var payload = JsonConvert.SerializeObject(new
            {
                username = username,
                password = password
            });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync($"{_baseUrl}/api/auth/login", content);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"登录失败 [{resp.StatusCode}]: {body}");

            var json = JObject.Parse(body);
            _token = json["data"]?["token"]?.ToString()
                     ?? throw new Exception("响应中未找到 token 字段");

            // 设置后续请求的 Authorization 头
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            return _token;
        }

        public void SetToken(string token)
        {
            _token = token;
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // ── 模版 ────────────────────────────────────────────────
        /// <summary>获取所有邮件模版</summary>
        public async Task<List<EmailTemplate>> GetTemplatesAsync()
        {
            var resp = await _http.GetAsync($"{_baseUrl}/api/templates");
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new Exception($"获取模版失败: {body}");

            var json  = JObject.Parse(body);
            var items = json["data"] as JArray ?? new JArray();
            var result = new List<EmailTemplate>();

            foreach (var item in items)
            {
                result.Add(new EmailTemplate
                {
                    MeetbyTemplateId = item["id"]?.ToString(),
                    Name             = item["name"]?.ToString(),
                    Subject          = item["subject"]?.ToString(),
                    HtmlBody         = item["html_body"]?.ToString(),
                    TextBody         = item["text_body"]?.ToString(),
                    FromName         = item["from_name"]?.ToString(),
                    FromEmail        = item["from_email"]?.ToString(),
                });
            }
            return result;
        }

        // ── 邮件列表 ────────────────────────────────────────────
        /// <summary>获取所有邮件列表（只含列表元信息，不含成员）</summary>
        public async Task<List<(int ListId, string ListName, int MemberCount)>>
            GetEmailListsAsync()
        {
            var resp = await _http.GetAsync($"{_baseUrl}/api/lists");
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new Exception($"获取列表失败: {body}");

            var json  = JObject.Parse(body);
            var items = json["data"] as JArray ?? new JArray();
            var result = new List<(int, string, int)>();

            foreach (var item in items)
            {
                result.Add((
                    item["id"]?.Value<int>() ?? 0,
                    item["name"]?.ToString() ?? "",
                    item["member_count"]?.Value<int>() ?? 0
                ));
            }
            return result;
        }

        /// <summary>分页拉取指定列表的所有成员邮件地址</summary>
        public async Task<List<EmailRecipient>> GetEmailListMembersAsync(
            int listId, string listName,
            IProgress<(int current, int total)> progress = null)
        {
            var result  = new List<EmailRecipient>();
            int page    = 1;
            int perPage = 500;
            int total   = 0;

            while (true)
            {
                var url  = $"{_baseUrl}/api/lists/{listId}/members" +
                           $"?page={page}&per_page={perPage}";
                var resp = await _http.GetAsync(url);
                var body = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode) break;

                var json  = JObject.Parse(body);
                total     = json["total"]?.Value<int>() ?? 0;
                var items = json["data"] as JArray ?? new JArray();
                if (items.Count == 0) break;

                foreach (var item in items)
                {
                    var email = item["email"]?.ToString() ?? "";
                    result.Add(new EmailRecipient
                    {
                        ListId    = listId,
                        ListName  = listName,
                        Email     = email,
                        FirstName = item["first_name"]?.ToString(),
                        LastName  = item["last_name"]?.ToString(),
                        Company   = item["company"]?.ToString(),
                        Domain    = email.Contains("@")
                                    ? email.Split('@')[1].ToLower() : "",
                    });
                }

                progress?.Report((result.Count, total));
                if (result.Count >= total) break;
                page++;
            }
            return result;
        }
    }
}
