using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using EmailSender.Core.ApiClients;
using EmailSender.Data.Repositories;
using EmailSender.Models;

namespace EmailSender.Core.Services
{
    /// <summary>
    /// 邮件列表服务
    /// 负责：下载列表成员、清洗过滤、ZeroBounce验证、域名统计
    /// </summary>
    public class EmailListService
    {
        private readonly MeetbyApiClient _meetby;
        private readonly EmailAddressRepository _repo;
        private readonly BlacklistRepository _blacklist;
        private readonly AppConfigRepository _config;
        private readonly HttpClient _http = new HttpClient();

        public EmailListService(
            MeetbyApiClient meetby,
            EmailAddressRepository repo,
            BlacklistRepository blacklist,
            AppConfigRepository config)
        {
            _meetby = meetby;
            _repo = repo;
            _blacklist = blacklist;
            _config = config;
        }

        /// <summary>从 meetby 下载列表成员并存入本地数据库</summary>
        public async Task<int> DownloadAndSaveAsync(
            int listId, string listName,
            IProgress<(int current, int total)> progress = null)
        {
            var members = await _meetby.GetEmailListMembersAsync(
                listId, listName, progress);

            _repo.AddBatch(members);
            return members.Count;
        }

        /// <summary>获取过滤后的发送地址列表</summary>
        public List<EmailRecipient> GetFiltered(
            int listId,
            bool excludeBlacklist = true,
            int maxFailCount = 3,
            bool excludeSent = false,
            bool excludeThisWeek = false,
            int? excludeTaskId = null)
        {
            return _repo.GetFilteredForTask(
                listId, maxFailCount, excludeTaskId, excludeThisWeek);
        }

        /// <summary>获取域名分布统计</summary>
        public Dictionary<string, int> GetDomainStats(int listId)
            => _repo.GetDomainStats(listId);

        /// <summary>获取列表总数</summary>
        public int GetCount(int listId) => _repo.GetCount(listId);

        /// <summary>使用 ZeroBounce API 批量验证邮件地址</summary>
        public async Task<Dictionary<string, VerifyStatus>> VerifyWithZeroBounceAsync(
            List<string> emails,
            IProgress<(int current, int total)> progress = null)
        {
            var apiKey = _config.Get("ZeroBounceApiKey");
            var apiUrl = _config.Get("ZeroBounceApiUrl") ?? "https://api.zerobounce.net/v2";
            var result = new Dictionary<string, VerifyStatus>();

            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("ZeroBounce API Key 未配置");

            int batchSize = 100;
            for (int i = 0; i < emails.Count; i += batchSize)
            {
                var batch = emails.Skip(i).Take(batchSize).ToList();

                // 用 JObject/JArray 构建 JSON，避免手拼字符串的转义问题
                var emailArray = new JArray();
                foreach (var e in batch)
                {
                    emailArray.Add(new JObject { ["email_address"] = e });
                }

                var requestBody = new JObject
                {
                    ["email_batch"] = emailArray,
                    ["api_key"] = apiKey
                };

                var url = $"{apiUrl}/validatebatch";
                var content = new System.Net.Http.StringContent(
                    requestBody.ToString(), System.Text.Encoding.UTF8, "application/json");

                var resp = await _http.PostAsync(url, content);
                var respBody = await resp.Content.ReadAsStringAsync();
                var json = JObject.Parse(respBody);
                var items = json["email_batch"] as JArray ?? new JArray();

                foreach (var item in items)
                {
                    var email = item["address"]?.ToString();
                    var status = item["status"]?.ToString();
                    if (string.IsNullOrEmpty(email)) continue;

                    result[email] = status switch
                    {
                        "valid" => VerifyStatus.Valid,
                        "invalid" => VerifyStatus.Invalid,
                        "catch-all" => VerifyStatus.CatchAll,
                        "spamtrap" => VerifyStatus.SpamTrap,
                        _ => VerifyStatus.Unknown
                    };
                }

                progress?.Report((Math.Min(i + batchSize, emails.Count), emails.Count));
                await Task.Delay(500);
            }

            // 将无效地址标记失败次数为 999
            foreach (var kv in result.Where(x => x.Value == VerifyStatus.Invalid))
                _repo.UpdateFailCount(kv.Key, 999);

            return result;
        }
    }
}