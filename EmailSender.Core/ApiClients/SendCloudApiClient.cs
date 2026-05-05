using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EmailSender.Models;

namespace EmailSender.Core.ApiClients
{
    /// <summary>
    /// SendCloud API 客户端
    /// 负责：模版上传/更新、触发发送、查询发送日志
    /// </summary>
    public class SendCloudApiClient
    {
        private readonly HttpClient _http;
        private readonly string _apiUrl;
        private readonly string _apiUser;
        private readonly string _apiKey;

        public SendCloudApiClient(string apiUrl, string apiUser, string apiKey)
        {
            _apiUrl  = apiUrl.TrimEnd('/');
            _apiUser = apiUser;
            _apiKey  = apiKey;
            _http    = new HttpClient();
            _http.Timeout = TimeSpan.FromSeconds(30);
        }

        // ── 签名 ────────────────────────────────────────────────
        private string Sign(SortedDictionary<string, string> param)
        {
            var sb = new StringBuilder();
            sb.Append(_apiKey).Append('&');
            foreach (var kv in param)
                sb.Append(kv.Key).Append('=').Append(kv.Value).Append('&');
            sb.Append(_apiKey);
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        // ── 模版管理 ────────────────────────────────────────────
        /// <summary>上传或更新模版到 SendCloud</summary>
        public async Task<string> UploadTemplateAsync(EmailTemplate template)
        {
            var param = new SortedDictionary<string, string>
            {
                ["apiUser"]       = _apiUser,
                ["templateName"]  = template.Name,
                ["subject"]       = template.Subject,
                ["html"]          = template.HtmlBody,
                ["templateType"]  = "1",  // 1=触发类
            };
            param["signature"] = Sign(param);

            // 判断是新建还是更新
            var endpoint = string.IsNullOrEmpty(template.SendCloudTemplateId)
                ? $"{_apiUrl}/template/add"
                : $"{_apiUrl}/template/update";

            if (!string.IsNullOrEmpty(template.SendCloudTemplateId))
                param["invokeId"] = template.SendCloudTemplateId;

            var content = new FormUrlEncodedContent(param);
            var resp    = await _http.PostAsync(endpoint, content);
            var body    = await resp.Content.ReadAsStringAsync();

            var json = JObject.Parse(body);
            if (json["result"]?.Value<bool>() != true)
                throw new Exception($"上传模版失败: {json["message"]}");

            return json["info"]?["data"]?["invokeId"]?.ToString()
                   ?? template.SendCloudTemplateId;
        }

        // ── 发送 ────────────────────────────────────────────────
        /// <summary>使用模版发送单封邮件，返回 msgId</summary>
        public async Task<string> SendByTemplateAsync(
            string toEmail, string toName,
            string templateId, string fromEmail, string fromName,
            Dictionary<string, string> vars = null)
        {
            var xsmtpapi = new
            {
                to   = new[] { toEmail },
                sub  = vars != null
                       ? BuildSubstitutions(new[] { toEmail }, vars)
                       : null
            };

            var param = new SortedDictionary<string, string>
            {
                ["apiUser"]    = _apiUser,
                ["from"]       = fromEmail,
                ["fromName"]   = fromName,
                ["to"]         = toEmail,
                ["subject"]    = "%subject%",
                ["templateId"] = templateId,
                ["xsmtpapi"]   = JsonConvert.SerializeObject(xsmtpapi),
            };
            param["signature"] = Sign(param);

            var content = new FormUrlEncodedContent(param);
            var resp    = await _http.PostAsync($"{_apiUrl}/mail/sendtemplate", content);
            var body    = await resp.Content.ReadAsStringAsync();

            var json = JObject.Parse(body);
            if (json["result"]?.Value<bool>() != true)
                throw new Exception($"SendCloud发送失败: {json["message"]}");

            return json["info"]?["emailIdList"]?[0]?.ToString() ?? "";
        }

        private Dictionary<string, string[]> BuildSubstitutions(
            string[] emails, Dictionary<string, string> vars)
        {
            var sub = new Dictionary<string, string[]>();
            foreach (var kv in vars)
                sub[$"%{kv.Key}%"] = new[] { kv.Value };
            return sub;
        }

        // ── 查询日志 ────────────────────────────────────────────
        /// <summary>批量查询发送日志，返回 msgId -> status 映射</summary>
        public async Task<Dictionary<string, string>> QuerySendLogAsync(
            string startDate, string endDate, int days = 3)
        {
            var param = new SortedDictionary<string, string>
            {
                ["apiUser"]   = _apiUser,
                ["startDate"] = startDate,
                ["endDate"]   = endDate,
                ["days"]      = days.ToString(),
            };
            param["signature"] = Sign(param);

            var sb = new StringBuilder($"{_apiUrl}/log/list?");
            foreach (var kv in param)
                sb.Append($"{kv.Key}={Uri.EscapeDataString(kv.Value)}&");

            var resp = await _http.GetAsync(sb.ToString().TrimEnd('&'));
            var body = await resp.Content.ReadAsStringAsync();

            var result = new Dictionary<string, string>();
            var json   = JObject.Parse(body);
            var list   = json["info"]?["dataList"] as JArray ?? new JArray();

            foreach (var item in list)
            {
                var msgId  = item["emailId"]?.ToString();
                var status = item["status"]?.ToString();
                if (!string.IsNullOrEmpty(msgId))
                    result[msgId] = status ?? "unknown";
            }
            return result;
        }
    }
}
