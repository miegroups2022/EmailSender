import os

base = "EmailSender.Core"
for d in ["ApiClients", "Services", "Helpers", "Properties"]:
    os.makedirs(f"{base}/{d}", exist_ok=True)

files = {}

# ── EmailSender.Core.csproj ────────────────────────────────────
files["EmailSender.Core.csproj"] = """\
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props" />
  <PropertyGroup>
    <ProjectGuid>{33333333-3333-3333-3333-333333333333}</ProjectGuid>
    <OutputType>Library</OutputType>
    <RootNamespace>EmailSender.Core</RootNamespace>
    <AssemblyName>EmailSender.Core</AssemblyName>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="ApiClients\\MeetbyApiClient.cs" />
    <Compile Include="ApiClients\\SendCloudApiClient.cs" />
    <Compile Include="Services\\AuthService.cs" />
    <Compile Include="Services\\TemplateService.cs" />
    <Compile Include="Services\\EmailListService.cs" />
    <Compile Include="Services\\SendTaskService.cs" />
    <Compile Include="Services\\SendEngineService.cs" />
    <Compile Include="Services\\ResultFetchService.cs" />
    <Compile Include="Services\\BlacklistService.cs" />
    <Compile Include="Services\\AiAnalysisService.cs" />
    <Compile Include="Helpers\\EmailValidator.cs" />
    <Compile Include="Helpers\\TemplateRenderer.cs" />
    <Compile Include="Helpers\\OAuthHelper.cs" />
    <Compile Include="Properties\\AssemblyInfo.cs" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\\EmailSender.Models\\EmailSender.Models.csproj">
      <Project>{11111111-1111-1111-1111-111111111111}</Project>
      <Name>EmailSender.Models</Name>
    </ProjectReference>
    <ProjectReference Include="..\\EmailSender.Data\\EmailSender.Data.csproj">
      <Project>{22222222-2222-2222-2222-222222222222}</Project>
      <Name>EmailSender.Data</Name>
    </ProjectReference>
  </ItemGroup>
  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Net.Http" />
    <Reference Include="System.Web" />
    <Reference Include="Newtonsoft.Json">
      <HintPath>..\\packages\\Newtonsoft.Json.13.0.3\\lib\\net45\\Newtonsoft.Json.dll</HintPath>
    </Reference>
  </ItemGroup>
  <Import Project="$(MSBuildToolsPath)\\Microsoft.CSharp.targets" />
</Project>
"""

# ── Properties/AssemblyInfo.cs ─────────────────────────────────
files["Properties/AssemblyInfo.cs"] = """\
using System.Reflection;
[assembly: AssemblyTitle("EmailSender.Core")]
[assembly: AssemblyDescription("Business logic layer for EmailSender")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
"""

# ══════════════════════════════════════════════════════════════
# ApiClients
# ══════════════════════════════════════════════════════════════

# ── ApiClients/MeetbyApiClient.cs ──────────────────────────────
files["ApiClients/MeetbyApiClient.cs"] = """\
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
        public async Task<List<EmailAddress>> GetEmailListMembersAsync(
            int listId, string listName,
            IProgress<(int current, int total)> progress = null)
        {
            var result  = new List<EmailAddress>();
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
                    result.Add(new EmailAddress
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
"""

# ── ApiClients/SendCloudApiClient.cs ───────────────────────────
files["ApiClients/SendCloudApiClient.cs"] = """\
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
"""

# ══════════════════════════════════════════════════════════════
# Services
# ══════════════════════════════════════════════════════════════

# ── Services/AuthService.cs ────────────────────────────────────
files["Services/AuthService.cs"] = """\
using System;
using System.Threading.Tasks;
using EmailSender.Core.ApiClients;
using EmailSender.Data.Repositories;

namespace EmailSender.Core.Services
{
    /// <summary>
    /// 登录认证服务
    /// 负责：meetby登录、Token持久化、自动续期
    /// </summary>
    public class AuthService
    {
        private readonly MeetbyApiClient      _api;
        private readonly AppConfigRepository  _config;

        public string CurrentToken    { get; private set; }
        public string CurrentUsername { get; private set; }
        public bool   IsLoggedIn      => !string.IsNullOrEmpty(CurrentToken);

        public AuthService(MeetbyApiClient api, AppConfigRepository config)
        {
            _api    = api;
            _config = config;
        }

        /// <summary>登录并持久化 Token</summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                CurrentToken    = await _api.LoginAsync(username, password);
                CurrentUsername = username;

                // 持久化
                _config.Set("MeetbyToken",    CurrentToken);
                _config.Set("MeetbyUsername", username);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"登录失败：{ex.Message}");
            }
        }

        /// <summary>从本地配置恢复 Token（启动时调用）</summary>
        public bool TryRestoreSession()
        {
            var token    = _config.Get("MeetbyToken");
            var username = _config.Get("MeetbyUsername");
            if (string.IsNullOrEmpty(token)) return false;

            CurrentToken    = token;
            CurrentUsername = username;
            _api.SetToken(token);
            return true;
        }

        /// <summary>退出登录，清除本地 Token</summary>
        public void Logout()
        {
            CurrentToken    = null;
            CurrentUsername = null;
            _config.Delete("MeetbyToken");
            _config.Delete("MeetbyUsername");
        }
    }
}
"""

# ── Services/TemplateService.cs ────────────────────────────────
files["Services/TemplateService.cs"] = """\
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmailSender.Core.ApiClients;
using EmailSender.Data.Repositories;
using EmailSender.Models;

namespace EmailSender.Core.Services
{
    /// <summary>
    /// 模版服务
    /// 负责：从 meetby 同步模版、推送到 SendCloud、触发 AI 分析
    /// </summary>
    public class TemplateService
    {
        private readonly MeetbyApiClient        _meetby;
        private readonly SendCloudApiClient     _sendCloud;
        private readonly EmailTemplateRepository _repo;
        private readonly AiAnalysisService      _ai;

        public TemplateService(
            MeetbyApiClient meetby,
            SendCloudApiClient sendCloud,
            EmailTemplateRepository repo,
            AiAnalysisService ai)
        {
            _meetby    = meetby;
            _sendCloud = sendCloud;
            _repo      = repo;
            _ai        = ai;
        }

        /// <summary>从 meetby 拉取模版并同步到本地数据库</summary>
        public async Task<List<EmailTemplate>> SyncFromMeetbyAsync()
        {
            var remoteList = await _meetby.GetTemplatesAsync();
            var synced     = new List<EmailTemplate>();

            foreach (var remote in remoteList)
            {
                var all      = _repo.GetAll();
                var existing = all.Find(t => t.MeetbyTemplateId == remote.MeetbyTemplateId);

                if (existing == null)
                {
                    remote.Id = _repo.Add(remote);
                    synced.Add(remote);
                }
                else
                {
                    // 更新内容但保留本地 AI 分析和 SendCloud 同步状态
                    existing.Name      = remote.Name;
                    existing.Subject   = remote.Subject;
                    existing.HtmlBody  = remote.HtmlBody;
                    existing.TextBody  = remote.TextBody;
                    existing.FromName  = remote.FromName;
                    existing.FromEmail = remote.FromEmail;
                    existing.NeedResync = true;
                    _repo.Update(existing);
                    synced.Add(existing);
                }
            }
            return synced;
        }

        /// <summary>将指定模版推送到 SendCloud</summary>
        public async Task SyncToSendCloudAsync(int templateId)
        {
            var template = _repo.GetById(templateId)
                ?? throw new Exception($"模版 {templateId} 不存在");

            var sendCloudId = await _sendCloud.UploadTemplateAsync(template);
            _repo.MarkSynced(templateId, sendCloudId);
        }

        /// <summary>对模版进行 AI 反垃圾分析并保存结果</summary>
        public async Task AnalyzeWithAiAsync(int templateId)
        {
            var template = _repo.GetById(templateId)
                ?? throw new Exception($"模版 {templateId} 不存在");

            var result = await _ai.AnalyzeAsync(template.Subject, template.HtmlBody);

            template.AiScore       = result.Score;
            template.AiIssues      = result.IssuesJson;
            template.AiSuggestions = result.SuggestionsJson;
            template.AiAnalyzedAt  = DateTime.Now;
            _repo.Update(template);
        }

        public List<EmailTemplate> GetAll()   => _repo.GetAll();
        public EmailTemplate GetById(int id)  => _repo.GetById(id);

        public void SaveLocal(EmailTemplate t)
        {
            if (t.Id == 0) _repo.Add(t);
            else           _repo.Update(t);
        }

        public void Delete(int id) => _repo.Delete(id);
    }
}
"""

# ── Services/EmailListService.cs ───────────────────────────────
files["Services/EmailListService.cs"] = """\
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
        private readonly MeetbyApiClient         _meetby;
        private readonly EmailAddressRepository  _repo;
        private readonly BlacklistRepository     _blacklist;
        private readonly AppConfigRepository     _config;
        private readonly HttpClient              _http = new HttpClient();

        public EmailListService(
            MeetbyApiClient meetby,
            EmailAddressRepository repo,
            BlacklistRepository blacklist,
            AppConfigRepository config)
        {
            _meetby    = meetby;
            _repo      = repo;
            _blacklist  = blacklist;
            _config    = config;
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
        public List<EmailAddress> GetFiltered(
            int listId,
            bool excludeBlacklist   = true,
            int  maxFailCount       = 3,
            bool excludeSent        = false,
            bool excludeThisWeek    = false,
            int? excludeTaskId      = null)
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
            var apiKey  = _config.Get("ZeroBounceApiKey");
            var apiUrl  = _config.Get("ZeroBounceApiUrl")
                          ?? "https://api.zerobounce.net/v2";
            var result  = new Dictionary<string, VerifyStatus>();

            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("ZeroBounce API Key 未配置");

            // 批量验证，每批 100 个
            int batchSize = 100;
            for (int i = 0; i < emails.Count; i += batchSize)
            {
                var batch = emails.Skip(i).Take(batchSize).ToList();
                var emailList = string.Join(",", batch.Select(e =>
                    $"{{\"email_address\":\"{e}\"}}"));

                var url  = $"{apiUrl}/validatebatch";
                var body = $"{{\"email_batch\":[{emailList}],\"api_key\":\"{apiKey}\"}}";
                var content = new System.Net.Http.StringContent(
                    body, System.Text.Encoding.UTF8, "application/json");

                var resp     = await _http.PostAsync(url, content);
                var respBody = await resp.Content.ReadAsStringAsync();
                var json     = JObject.Parse(respBody);
                var items    = json["email_batch"] as JArray ?? new JArray();

                foreach (var item in items)
                {
                    var email  = item["address"]?.ToString();
                    var status = item["status"]?.ToString();
                    if (string.IsNullOrEmpty(email)) continue;

                    result[email] = status switch
                    {
                        "valid"    => VerifyStatus.Valid,
                        "invalid"  => VerifyStatus.Invalid,
                        "catch-all"=> VerifyStatus.CatchAll,
                        "spamtrap" => VerifyStatus.SpamTrap,
                        _          => VerifyStatus.Unknown
                    };
                }

                progress?.Report((Math.Min(i + batchSize, emails.Count), emails.Count));
                await Task.Delay(500); // 避免频率限制
            }

            // 将无效地址标记
            foreach (var kv in result.Where(x => x.Value == VerifyStatus.Invalid))
                _repo.UpdateFailCount(kv.Key, 999); // 标记为无效

            return result;
        }
    }
}
"""

# ── Services/SendTaskService.cs ────────────────────────────────
files["Services/SendTaskService.cs"] = """\
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EmailSender.Data.Repositories;
using EmailSender.Models;

namespace EmailSender.Core.Services
{
    /// <summary>
    /// 发送任务服务
    /// 负责：任务创建、调度、启动/暂停/取消
    /// </summary>
    public class SendTaskService
    {
        private readonly SendTaskRepository    _taskRepo;
        private readonly EmailListService      _listService;
        private readonly SendEngineService     _engine;

        // 当前运行中的任务取消令牌
        private readonly Dictionary<int, CancellationTokenSource> _runningTasks
            = new Dictionary<int, CancellationTokenSource>();

        public SendTaskService(
            SendTaskRepository taskRepo,
            EmailListService listService,
            SendEngineService engine)
        {
            _taskRepo    = taskRepo;
            _listService = listService;
            _engine      = engine;
        }

        /// <summary>创建新任务</summary>
        public int CreateTask(SendTask task)
        {
            task.Status    = TaskStatus.Pending;
            task.CreatedAt = DateTime.Now;
            task.UpdatedAt = DateTime.Now;
            task.Id        = _taskRepo.Add(task);
            return task.Id;
        }

        /// <summary>启动任务（异步，不阻塞UI）</summary>
        public async Task StartTaskAsync(
            int taskId,
            IProgress<SendProgressInfo> progress = null)
        {
            var task = _taskRepo.GetById(taskId)
                ?? throw new Exception($"任务 {taskId} 不存在");

            if (task.Status == TaskStatus.Running)
                throw new Exception("任务已在运行中");

            // 获取过滤后的邮件列表
            var emails = _listService.GetFiltered(
                task.ListId,
                excludeBlacklist: true,
                maxFailCount: task.RetryMax,
                excludeTaskId: taskId);

            task.TotalCount = emails.Count;
            _taskRepo.UpdateStatus(taskId, TaskStatus.Running);

            var cts = new CancellationTokenSource();
            _runningTasks[taskId] = cts;

            try
            {
                await _engine.ExecuteAsync(task, emails, progress, cts.Token);
                _taskRepo.UpdateStatus(taskId, TaskStatus.Done);
            }
            catch (OperationCanceledException)
            {
                _taskRepo.UpdateStatus(taskId, TaskStatus.Paused);
            }
            catch (Exception)
            {
                _taskRepo.UpdateStatus(taskId, TaskStatus.Failed);
                throw;
            }
            finally
            {
                _runningTasks.Remove(taskId);
            }
        }

        /// <summary>暂停/取消任务</summary>
        public void CancelTask(int taskId)
        {
            if (_runningTasks.TryGetValue(taskId, out var cts))
                cts.Cancel();
        }

        public List<SendTask> GetAll()         => _taskRepo.GetAll();
        public SendTask GetById(int id)        => _taskRepo.GetById(id);
        public void Delete(int id)             => _taskRepo.Delete(id);
    }

    /// <summary>发送进度信息</summary>
    public class SendProgressInfo
    {
        public int    Total     { get; set; }
        public int    Sent      { get; set; }
        public int    Success   { get; set; }
        public int    Failed    { get; set; }
        public string CurrentEmail { get; set; }
        public double Percent   => Total > 0 ? (double)Sent / Total * 100 : 0;
    }
}
"""

# ── Services/SendEngineService.cs ──────────────────────────────
files["Services/SendEngineService.cs"] = """\
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using EmailSender.Core.ApiClients;
using EmailSender.Core.Helpers;
using EmailSender.Data.Repositories;
using EmailSender.Models;

namespace EmailSender.Core.Services
{
    /// <summary>
    /// 核心发送引擎
    /// 支持：SendCloud / Gmail / Hotmail / SMTP 四种通道
    /// 特性：多线程并发、发送间隔、失败重试、进度上报、取消支持
    /// </summary>
    public class SendEngineService
    {
        private readonly SendCloudApiClient      _sendCloud;
        private readonly SenderAccountRepository _accountRepo;
        private readonly SendRecordRepository    _recordRepo;
        private readonly EmailAddressRepository  _addrRepo;
        private readonly BlacklistRepository     _blacklist;
        private readonly TemplateRenderer        _renderer;

        public SendEngineService(
            SendCloudApiClient sendCloud,
            SenderAccountRepository accountRepo,
            SendRecordRepository recordRepo,
            EmailAddressRepository addrRepo,
            BlacklistRepository blacklist,
            TemplateRenderer renderer)
        {
            _sendCloud   = sendCloud;
            _accountRepo = accountRepo;
            _recordRepo  = recordRepo;
            _addrRepo    = addrRepo;
            _blacklist   = blacklist;
            _renderer    = renderer;
        }

        /// <summary>执行发送任务</summary>
        public async Task ExecuteAsync(
            SendTask task,
            List<EmailAddress> emails,
            IProgress<SendProgressInfo> progress,
            CancellationToken ct)
        {
            var account   = _accountRepo.GetById(task.AccountId)
                ?? throw new Exception($"发送账户 {task.AccountId} 不存在");
            var semaphore = new SemaphoreSlim(task.ThreadCount, task.ThreadCount);
            var taskList  = new List<Task>();
            var info      = new SendProgressInfo { Total = emails.Count };
            var lockObj   = new object();

            foreach (var email in emails)
            {
                ct.ThrowIfCancellationRequested();
                await semaphore.WaitAsync(ct);

                var emailCopy = email; // 闭包捕获
                var t = Task.Run(async () =>
                {
                    try
                    {
                        await SendOneAsync(task, account, emailCopy, ct);
                        lock (lockObj) { info.Success++; info.Sent++; }
                        _addrRepo.UpdateLastSentAt(emailCopy.Email);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        lock (lockObj) { info.Failed++; info.Sent++; }
                        // 记录失败，更新失败次数
                        _addrRepo.UpdateFailCount(emailCopy.Email,
                            emailCopy.FailCount + 1);
                        // 失败次数超限自动加黑名单
                        if (emailCopy.FailCount + 1 >= task.RetryMax)
                            _blacklist.Add(new Blacklist
                            {
                                Email  = emailCopy.Email,
                                Type   = BlacklistType.RepeatedFail,
                                Reason = ex.Message,
                                TaskId = task.Id
                            });
                    }
                    finally
                    {
                        lock (lockObj) { info.CurrentEmail = emailCopy.Email; }
                        progress?.Report(info);
                        semaphore.Release();
                    }
                }, ct);

                taskList.Add(t);
                await Task.Delay(task.IntervalSeconds * 1000, ct);
            }

            await Task.WhenAll(taskList);

            // 更新任务统计
            _recordRepo.GetCountByStatus(task.Id, SendStatus.Sent);
        }

        private async Task SendOneAsync(
            SendTask task, SenderAccount account,
            EmailAddress addr, CancellationToken ct)
        {
            var record = new SendRecord
            {
                TaskId         = task.Id,
                EmailAddressId = addr.Id,
                Email          = addr.Email,
                SendStatus     = SendStatus.Pending,
                SentAt         = DateTime.Now,
            };

            try
            {
                switch (task.Channel)
                {
                    case SendChannel.SendCloud:
                        record.SendCloudMsgId = await SendViaSendCloudAsync(
                            task, account, addr);
                        break;
                    case SendChannel.Gmail:
                    case SendChannel.Hotmail:
                        await SendViaOAuthSmtpAsync(task, account, addr);
                        break;
                    case SendChannel.SMTP:
                        await SendViaSmtpAsync(task, account, addr);
                        break;
                }
                record.SendStatus = SendStatus.Sent;
            }
            catch (Exception ex)
            {
                record.SendStatus    = SendStatus.Failed;
                record.ErrorMessage  = ex.Message;
                throw;
            }
            finally
            {
                _recordRepo.Add(record);
            }
        }

        private async Task<string> SendViaSendCloudAsync(
            SendTask task, SenderAccount account, EmailAddress addr)
        {
            var vars = new Dictionary<string, string>
            {
                ["first_name"] = addr.FirstName ?? "",
                ["last_name"]  = addr.LastName  ?? "",
                ["company"]    = addr.Company   ?? "",
                ["email"]      = addr.Email,
            };
            return await _sendCloud.SendByTemplateAsync(
                addr.Email, $"{addr.FirstName} {addr.LastName}".Trim(),
                account.ApiUser, account.SmtpFromEmail ?? account.OAuthEmail,
                account.SmtpFromName ?? account.Name, vars);
        }

        private async Task SendViaOAuthSmtpAsync(
            SendTask task, SenderAccount account, EmailAddress addr)
        {
            // OAuth Token 刷新由 OAuthHelper 处理
            var smtpHost = task.Channel == SendChannel.Gmail
                ? "smtp.gmail.com" : "smtp.office365.com";
            var smtpPort = 587;

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl             = true;
                client.UseDefaultCredentials = false;
                client.Credentials           = new NetworkCredential(
                    account.OAuthEmail, account.OAuthToken);

                var msg = BuildMailMessage(task, account, addr);
                await client.SendMailAsync(msg);
            }
        }

        private async Task SendViaSmtpAsync(
            SendTask task, SenderAccount account, EmailAddress addr)
        {
            using (var client = new SmtpClient(account.SmtpHost, account.SmtpPort))
            {
                client.EnableSsl             = account.SmtpUseSsl;
                client.UseDefaultCredentials = false;
                client.Credentials           = new NetworkCredential(
                    account.SmtpUser, account.SmtpPassword);

                var msg = BuildMailMessage(task, account, addr);
                await client.SendMailAsync(msg);
            }
        }

        private MailMessage BuildMailMessage(
            SendTask task, SenderAccount account, EmailAddress addr)
        {
            // 从 Data 层获取模版内容（由调用方注入，此处简化）
            var html = _renderer.Render("{{html_body}}", addr);
            var msg  = new MailMessage
            {
                From       = new MailAddress(
                    account.SmtpFromEmail ?? account.OAuthEmail,
                    account.SmtpFromName  ?? account.Name),
                Subject    = _renderer.Render("{{subject}}", addr),
                Body       = html,
                IsBodyHtml = true,
            };
            msg.To.Add(new MailAddress(addr.Email,
                $"{addr.FirstName} {addr.LastName}".Trim()));
            return msg;
        }
    }
}
"""

# ── Services/ResultFetchService.cs ─────────────────────────────
files["Services/ResultFetchService.cs"] = """\
using System;
using System.Linq;
using System.Threading.Tasks;
using EmailSender.Core.ApiClients;
using EmailSender.Data.Repositories;
using EmailSender.Models;

namespace EmailSender.Core.Services
{
    /// <summary>
    /// 发送结果拉取服务
    /// 负责：主动查询 SendCloud 发送日志，更新本地发送记录和任务统计
    /// </summary>
    public class ResultFetchService
    {
        private readonly SendCloudApiClient   _sendCloud;
        private readonly SendRecordRepository _recordRepo;
        private readonly SendTaskRepository   _taskRepo;
        private readonly BlacklistRepository  _blacklist;

        public ResultFetchService(
            SendCloudApiClient sendCloud,
            SendRecordRepository recordRepo,
            SendTaskRepository taskRepo,
            BlacklistRepository blacklist)
        {
            _sendCloud  = sendCloud;
            _recordRepo = recordRepo;
            _taskRepo   = taskRepo;
            _blacklist  = blacklist;
        }

        /// <summary>拉取指定任务的未处理发送结果</summary>
        public async Task<int> FetchPendingResultsAsync(
            int taskId,
            IProgress<(int current, int total)> progress = null)
        {
            var pending = _recordRepo.GetPendingFetch(taskId, 200);
            if (pending.Count == 0) return 0;

            // 查询最近3天的日志
            var startDate = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
            var endDate   = DateTime.Now.ToString("yyyy-MM-dd");
            var logMap    = await _sendCloud.QuerySendLogAsync(startDate, endDate);

            int updated = 0;
            for (int i = 0; i < pending.Count; i++)
            {
                var record = pending[i];
                if (string.IsNullOrEmpty(record.SendCloudMsgId)) continue;

                if (logMap.TryGetValue(record.SendCloudMsgId, out var statusStr))
                {
                    var status = ParseSendCloudStatus(statusStr);
                    _recordRepo.MarkFetched(record.Id, status);

                    // 硬退信自动加黑名单
                    if (status == SendStatus.Bounced)
                        _blacklist.Add(new Blacklist
                        {
                            Email  = record.Email,
                            Type   = BlacklistType.HardBounce,
                            Reason = "SendCloud HardBounce",
                            TaskId = taskId,
                            SendRecordId = record.Id
                        });

                    // 垃圾举报自动加黑名单
                    if (status == SendStatus.Spam)
                        _blacklist.Add(new Blacklist
                        {
                            Email  = record.Email,
                            Type   = BlacklistType.SpamReport,
                            Reason = "SendCloud SpamReport",
                            TaskId = taskId,
                            SendRecordId = record.Id
                        });

                    updated++;
                }
                progress?.Report((i + 1, pending.Count));
            }

            // 重新统计并更新任务
            await RefreshTaskStatsAsync(taskId);
            return updated;
        }

        private async Task RefreshTaskStatsAsync(int taskId)
        {
            var success = _recordRepo.GetCountByStatus(taskId, SendStatus.Delivered)
                        + _recordRepo.GetCountByStatus(taskId, SendStatus.Opened)
                        + _recordRepo.GetCountByStatus(taskId, SendStatus.Clicked);
            var fail    = _recordRepo.GetCountByStatus(taskId, SendStatus.Bounced)
                        + _recordRepo.GetCountByStatus(taskId, SendStatus.Failed);
            var open    = _recordRepo.GetCountByStatus(taskId, SendStatus.Opened);
            var click   = _recordRepo.GetCountByStatus(taskId, SendStatus.Clicked);
            var bounce  = _recordRepo.GetCountByStatus(taskId, SendStatus.Bounced);

            _taskRepo.UpdateStats(taskId, success, fail, open, click, bounce);
            await Task.CompletedTask;
        }

        private SendStatus ParseSendCloudStatus(string s)
        {
            return s?.ToLower() switch
            {
                "delivered"   => SendStatus.Delivered,
                "opened"      => SendStatus.Opened,
                "clicked"     => SendStatus.Clicked,
                "bounced"     => SendStatus.Bounced,
                "spam"        => SendStatus.Spam,
                "unsubscribe" => SendStatus.Spam,
                "failed"      => SendStatus.Failed,
                _             => SendStatus.Sent
            };
        }
    }
}
"""

# ── Services/BlacklistService.cs ───────────────────────────────
files["Services/BlacklistService.cs"] = """\
using System.Collections.Generic;
using System.IO;
using System.Text;
using EmailSender.Data.Repositories;
using EmailSender.Models;

namespace EmailSender.Core.Services
{
    /// <summary>
    /// 黑名单管理服务
    /// 负责：手动添加/删除、批量导入、导出、过滤检查
    /// </summary>
    public class BlacklistService
    {
        private readonly BlacklistRepository _repo;

        public BlacklistService(BlacklistRepository repo)
        {
            _repo = repo;
        }

        public void AddManual(string email, string reason = null)
        {
            _repo.Add(new Blacklist
            {
                Email  = email.Trim().ToLower(),
                Type   = BlacklistType.Manual,
                Reason = reason
            });
        }

        public void Remove(string email) => _repo.Delete(email);

        public bool IsBlacklisted(string email)
            => _repo.Contains(email.Trim().ToLower());

        public List<Blacklist> GetAll() => _repo.GetAll();

        public int GetCount() => _repo.GetCount();

        /// <summary>从文本文件批量导入（每行一个邮件地址）</summary>
        public int ImportFromFile(string filePath)
        {
            int count = 0;
            foreach (var line in File.ReadAllLines(filePath, Encoding.UTF8))
            {
                var email = line.Trim().ToLower();
                if (string.IsNullOrEmpty(email) || !email.Contains("@")) continue;
                _repo.Add(new Blacklist
                {
                    Email  = email,
                    Type   = BlacklistType.Manual,
                    Reason = "批量导入"
                });
                count++;
            }
            return count;
        }

        /// <summary>导出黑名单到文本文件</summary>
        public void ExportToFile(string filePath)
        {
            var list  = _repo.GetAll();
            var lines = new List<string>();
            foreach (var b in list)
                lines.Add($"{b.Email}\t{b.Type}\t{b.Reason}\t{b.CreatedAt}");
            File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }
    }
}
"""

# ── Services/AiAnalysisService.cs ─────────────────────────────
files["Services/AiAnalysisService.cs"] = """\
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EmailSender.Data.Repositories;

namespace EmailSender.Core.Services
{
    /// <summary>
    /// AI 反垃圾分析服务
    /// 支持：DeepSeek / OpenAI 兼容接口
    /// 返回：评分(0-100)、问题列表、优化建议
    /// </summary>
    public class AiAnalysisService
    {
        private readonly HttpClient          _http = new HttpClient();
        private readonly AppConfigRepository _config;

        public AiAnalysisService(AppConfigRepository config)
        {
            _config = config;
            _http.Timeout = TimeSpan.FromSeconds(60);
        }

        /// <summary>分析邮件模版的反垃圾风险</summary>
        public async Task<AiAnalysisResult> AnalyzeAsync(string subject, string htmlBody)
        {
            var apiUrl = _config.Get("AiApiUrl")   ?? "https://api.deepseek.com/v1";
            var apiKey = _config.Get("AiApiKey")   ?? "";
            var model  = _config.Get("AiModel")    ?? "deepseek-chat";

            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("AI API Key 未配置，请在系统配置中填写");

            var prompt = BuildPrompt(subject, htmlBody);

            var requestBody = new
            {
                model    = model,
                messages = new[]
                {
                    new { role = "system", content = "你是一个专业的邮件反垃圾分析专家，请用JSON格式返回分析结果。" },
                    new { role = "user",   content = prompt }
                },
                temperature     = 0.3,
                response_format = new { type = "json_object" }
            };

            var json    = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var resp = await _http.PostAsync($"{apiUrl}/chat/completions", content);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"AI API 调用失败 [{resp.StatusCode}]: {body}");

            return ParseResult(body);
        }

        private string BuildPrompt(string subject, string htmlBody)
        {
            // 截取前3000字符避免超出token限制
            var bodyPreview = htmlBody?.Length > 3000
                ? htmlBody.Substring(0, 3000) + "..." : htmlBody;

            return $@"请分析以下邮件的反垃圾邮件风险，返回JSON格式：
{{
  ""score"": 85,
  ""level"": ""低风险"",
  ""issues"": [""问题1"", ""问题2""],
  ""suggestions"": [""建议1"", ""建议2""],
  ""spam_words"": [""触发词1"", ""触发词2""]
}}

score说明：0=极高风险，100=极低风险（越高越好）

邮件主题：{subject}

邮件内容（HTML）：
{bodyPreview}";
        }

        private AiAnalysisResult ParseResult(string responseBody)
        {
            try
            {
                var root    = JObject.Parse(responseBody);
                var content = root["choices"]?[0]?["message"]?["content"]?.ToString()
                    ?? "{}";
                var data    = JObject.Parse(content);

                return new AiAnalysisResult
                {
                    Score           = data["score"]?.Value<int>() ?? 50,
                    Level           = data["level"]?.ToString() ?? "未知",
                    IssuesJson      = data["issues"]?.ToString() ?? "[]",
                    SuggestionsJson = data["suggestions"]?.ToString() ?? "[]",
                    SpamWordsJson   = data["spam_words"]?.ToString() ?? "[]",
                };
            }
            catch
            {
                return new AiAnalysisResult
                {
                    Score           = 50,
                    Level           = "解析失败",
                    IssuesJson      = "[]",
                    SuggestionsJson = "[]",
                    SpamWordsJson   = "[]",
                };
            }
        }
    }

    public class AiAnalysisResult
    {
        public int    Score           { get; set; }
        public string Level           { get; set; }
        public string IssuesJson      { get; set; }
        public string SuggestionsJson { get; set; }
        public string SpamWordsJson   { get; set; }
    }
}
"""
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


# ══════════════════════════════════════════════════════════════
# Helpers（直接引用独立脚本生成的文件内容，此处留空占位）
# 说明：Helpers 三个文件已由 generate_EmailSender_Core_Helpers.py 单独生成
#       如需合并为一个脚本，可将 Helpers 内容粘贴到此处
# ══════════════════════════════════════════════════════════════

# ── Helpers/EmailValidator.cs（占位，已由独立脚本生成）─────────
# files["Helpers/EmailValidator.cs"] = "..."

# ── Helpers/TemplateRenderer.cs（占位，已由独立脚本生成）────────
# files["Helpers/TemplateRenderer.cs"] = "..."

# ── Helpers/OAuthHelper.cs（占位，已由独立脚本生成）─────────────
# files["Helpers/OAuthHelper.cs"] = "..."

# ══════════════════════════════════════════════════════════════
# 写入所有文件到磁盘
# ══════════════════════════════════════════════════════════════
for filename, content in files.items():
    full_path = os.path.join(base, filename)
    # 确保子目录存在
    os.makedirs(os.path.dirname(full_path), exist_ok=True)
    with open(full_path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"  ✅ 已生成: {full_path}")

print(f"\n🎉 EmailSender.Core 生成完毕！共 {len(files)} 个文件")
print(f"📁 输出目录: ./{base}/")
print()
print("文件清单：")
for fname in files:
    print(f"  📄 {fname}")
