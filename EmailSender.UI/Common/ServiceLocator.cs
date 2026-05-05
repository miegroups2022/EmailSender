using EmailSender.Core.ApiClients;
using EmailSender.Core.Helpers;
using EmailSender.Core.Services;
using EmailSender.Data.Repositories;
using EmailSender.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EmailSender.UI.Common
{
    /// <summary>
    /// Meetby 服务器 API 客户端   当前上下文中不存在名称“ServiceLocator”
    /// USE_MOCK = true  → 读取本地 mock/*.json 文件
    /// USE_MOCK = false → 请求真实服务器 http://ems.meetby.net
    /// </summary>
    public class ServiceLocator
    {

        // ══════════════════════════════════════════════════════
        // ⭐ 唯一开关：改这里切换 Mock / 真实接口
        // ══════════════════════════════════════════════════════
        private const bool USE_MOCK = true;

        private readonly string _baseUrl;
        private readonly HttpClient _http;
        private string _token;

        // mock 文件目录
        private static string MockDir =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mock");


        // 仓储层 -- 在 EmailSender.Date 下面，做数据操作的
        public static AppConfigRepository configRepository { get; private set; }
        public static BlacklistRepository blacklistRepository { get; private set; }
        public static EmailAddressRepository emailAddressRepository { get; private set; }
        public static EmailTemplateRepository emailTemplateRepository { get; private set; }
        public static SenderAccountRepository senderAccountRepository { get; private set; }
        public static SendRecordRepository sendRecordRepository { get; private set; }
        public static SendTaskRepository sendTaskRepository { get; private set; }
        

        // API 客户端 --- 在 EmailSender.Core 下面的 ApiClents 文件夹里面 ，有2个
        public static MeetbyApiClient meetbyApiClient { get; private set; }
        public static SendCloudApiClient sendCloudApiClient { get; private set; }

        // 服务层  --- 在 EmailSender.Core 下面的 Services 文件夹里面 ，有好几个个
        public static AuthService authService { get; private set; }
        public static EmailListService emailListService { get; private set; }
        public static SendEngineService sendEngineService { get; private set; }
        public static SendTaskService sendTaskService { get; private set; }
        public static TemplateService templateService { get; private set; }
        public static MockApiService mockApiService { get; private set; }
        public static AiAnalysisService aiAnalysisService { get; private set; }
        public static ResultFetchService resultFetchService { get; private set; }
        public static BlacklistService blacklistService { get; private set; }
       


        // 辅助工具
        public static OAuthHelper oAuthHelper { get; private set; }
        public static TemplateRenderer templateRenderer { get; private set; }
        

        public ServiceLocator(string baseUrl)
        {
            _baseUrl = baseUrl?.TrimEnd('/');
            _http = new HttpClient();
            _http.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>初始化所有服务</summary>
        public static void Initialize(string dbPath = "emailsender.db")
        {
            // 1. 初始化仓储
            configRepository = new AppConfigRepository();
            senderAccountRepository = new SenderAccountRepository();
            emailAddressRepository = new EmailAddressRepository();
            sendTaskRepository = new SendTaskRepository();
           

            // 2. 初始化 API 客户端（你的 Mock 逻辑在这里）
            meetbyApiClient = new MeetbyApiClient("http://ems.meetby.net");
            sendCloudApiClient = new SendCloudApiClient("http://ems.meetby.net","user","key");

            // 3. 初始化服务层

            mockApiService = new MockApiService();
            // ⚠️ 注意：authService 需要 meetbyApiClient，这里需要适配
            authService = new AuthService(meetbyApiClient, configRepository);
            
            emailListService = new EmailListService(meetbyApiClient,emailAddressRepository,blacklistRepository, configRepository);
            templateService = new TemplateService(meetbyApiClient,sendCloudApiClient,emailTemplateRepository,aiAnalysisService);
            
            sendEngineService = new SendEngineService(sendCloudApiClient, senderAccountRepository, sendRecordRepository, emailAddressRepository, blacklistRepository, templateRenderer);
            sendTaskService = new SendTaskService(sendTaskRepository,emailListService,sendEngineService);

            resultFetchService = new ResultFetchService( sendCloudApiClient, sendRecordRepository, sendTaskRepository, blacklistRepository);

            blacklistService = new BlacklistService(blacklistRepository);


         // 4. 初始化辅助工具
         oAuthHelper = new OAuthHelper(senderAccountRepository, configRepository);
            templateRenderer = new TemplateRenderer();

        }
    

        // ── 认证 ──────────────────────────────────────────────

        /// <summary>登录，成功后自动保存 Token</summary>
        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            if (USE_MOCK)
            {
                await Delay(400);
                // admin/admin 直接通过
                if (username == "admin" && password == "admin")
                {
                    var json = ReadMock("auth_login.json");
                    var data = JObject.Parse(json)["data"];
                    _token = data["token"].ToString();
                    return new LoginResult
                    {
                        Success = true,
                        Token = _token,
                        UserId = (int)data["user"]["id"],
                        Nickname = data["user"]["nickname"].ToString(),
                        Email = data["user"]["email"].ToString(),
                        ExpireAt = DateTime.Now.AddDays(7)
                    };
                }
                return new LoginResult
                {
                    Success = false,
                    ErrorMsg = "用户名或密码错误（测试账号：admin / admin）"
                };
            }

            // ── 真实接口 ──
            var body = JsonConvert.SerializeObject(new { username, password });
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync($"{_baseUrl}/api/v1/auth/login", content);
            var respStr = await resp.Content.ReadAsStringAsync();
            var obj = JObject.Parse(respStr);

            if ((int)obj["code"] == 0)
            {
                var data = obj["data"];
                _token = data["token"].ToString();
                _http.DefaultRequestHeaders.Remove("Authorization");
                _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
                return new LoginResult
                {
                    Success = true,
                    Token = _token,
                    UserId = (int)data["user"]["id"],
                    Nickname = data["user"]["nickname"].ToString(),
                    Email = data["user"]["email"].ToString(),
                    ExpireAt = DateTime.Parse(data["expires_at"].ToString())
                };
            }
            return new LoginResult
            {
                Success = false,
                ErrorMsg = obj["message"]?.ToString()
            };
        }

        public void Logout() => _token = null;

        public bool IsLoggedIn => !string.IsNullOrEmpty(_token);

        // ── 模板接口 ──────────────────────────────────────────

        public async Task<ApiPageResult<EmailTemplateDto>> GetTemplatesAsync(
            int page = 1, int pageSize = 20, string keyword = "")
        {
            if (USE_MOCK)
            {
                await Delay(300);
                var json = ReadMock("templates.json");
                var data = JObject.Parse(json)["data"];
                var list = data["list"].ToObject<List<EmailTemplateDto>>();
                if (!string.IsNullOrWhiteSpace(keyword))
                    list = list.FindAll(t =>
                        t.Name.Contains(keyword) ||
                        t.Subject.Contains(keyword));
                return new ApiPageResult<EmailTemplateDto>
                {
                    Success = true,
                    Total = list.Count,
                    List = list
                };
            }

            // ── 真实接口 ──
            var url = $"{_baseUrl}/api/v1/templates?page={page}&page_size={pageSize}&keyword={keyword}";
            var resp = await _http.GetStringAsync(url);
            var obj = JObject.Parse(resp);
            return new ApiPageResult<EmailTemplateDto>
            {
                Success = (int)obj["code"] == 0,
                Total = (int)obj["data"]["total"],
                List = obj["data"]["list"].ToObject<List<EmailTemplateDto>>()
            };
        }

        public async Task<SaveResult> SaveTemplateAsync(EmailTemplateDto template)
        {
            if (USE_MOCK)
            {
                await Delay(400);
                if (string.IsNullOrWhiteSpace(template.Name))
                    return new SaveResult { Success = false, ErrorMsg = "模板名称不能为空" };
                return new SaveResult
                {
                    Success = true,
                    Id = template.Id == 0 ? new Random().Next(100, 999) : template.Id,
                    Message = template.Id == 0 ? "模板创建成功" : "模板更新成功"
                };
            }

            // ── 真实接口 ──
            var body = JsonConvert.SerializeObject(template);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            HttpResponseMessage resp;
            if (template.Id == 0)
                resp = await _http.PostAsync($"{_baseUrl}/api/v1/templates", content);
            else
                resp = await _http.PutAsync($"{_baseUrl}/api/v1/templates/{template.Id}", content);
            var obj = JObject.Parse(await resp.Content.ReadAsStringAsync());
            return new SaveResult
            {
                Success = (int)obj["code"] == 0,
                Message = obj["message"]?.ToString(),
                ErrorMsg = (int)obj["code"] != 0 ? obj["message"]?.ToString() : null
            };
        }

        public async Task<SaveResult> DeleteTemplateAsync(int id)
        {
            if (USE_MOCK)
            {
                await Delay(200);
                return new SaveResult { Success = true, Message = "删除成功" };
            }
            var resp = await _http.DeleteAsync($"{_baseUrl}/api/v1/templates/{id}");
            var obj = JObject.Parse(await resp.Content.ReadAsStringAsync());
            return new SaveResult { Success = (int)obj["code"] == 0 };
        }

        // ── 收件人接口 ────────────────────────────────────────

        public async Task<List<RecipientGroupDto>> GetGroupsAsync()
        {
            if (USE_MOCK)
            {
                await Delay(200);
                var json = ReadMock("recipients.json");
                return JObject.Parse(json)["data"]["groups"]
                              .ToObject<List<RecipientGroupDto>>();
            }
            var resp = await _http.GetStringAsync($"{_baseUrl}/api/v1/recipient-groups");
            return JObject.Parse(resp)["data"].ToObject<List<RecipientGroupDto>>();
        }

        public async Task<ApiPageResult<EmailRecipientDto>> GetRecipientsAsync(
            int groupId = 0, int page = 1, int pageSize = 50)
        {
            if (USE_MOCK)
            {
                await Delay(300);
                var json = ReadMock("recipients.json");
                var list = JObject.Parse(json)["data"]["list"]
                                  .ToObject<List<EmailRecipientDto>>();
                if (groupId > 0)
                    list = list.FindAll(r => r.GroupId == groupId);
                return new ApiPageResult<EmailRecipientDto>
                {
                    Success = true,
                    Total = list.Count,
                    List = list
                };
            }
            var url = $"{_baseUrl}/api/v1/recipients?group_id={groupId}&page={page}&page_size={pageSize}";
            var resp = await _http.GetStringAsync(url);
            var obj = JObject.Parse(resp);
            return new ApiPageResult<EmailRecipientDto>
            {
                Success = (int)obj["code"] == 0,
                Total = (int)obj["data"]["total"],
                List = obj["data"]["list"].ToObject<List<EmailRecipientDto>>()
            };
        }

        // ── 发送任务接口 ──────────────────────────────────────

        public async Task<SaveResult> SubmitSendTaskAsync(
            int templateId, int groupId, string senderAccount)
        {
            if (USE_MOCK)
            {
                await Delay(600);
                return new SaveResult
                {
                    Success = true,
                    Id = new Random().Next(1000, 9999),
                    Message = $"发送任务已提交！预计开始时间：{DateTime.Now.AddSeconds(5):HH:mm:ss}"
                };
            }
            var body = JsonConvert.SerializeObject(new { template_id = templateId, group_id = groupId, sender_account = senderAccount });
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync($"{_baseUrl}/api/v1/send-tasks", content);
            var obj = JObject.Parse(await resp.Content.ReadAsStringAsync());
            return new SaveResult
            {
                Success = (int)obj["code"] == 0,
                Message = obj["message"]?.ToString()
            };
        }

        // ── 统计接口 ──────────────────────────────────────────

        public async Task<SendStatsDto> GetStatsAsync()
        {
            if (USE_MOCK)
            {
                await Delay(200);
                return new SendStatsDto
                {
                    TotalSent = 1280,
                    TotalSuccess = 1245,
                    TotalFailed = 35,
                    TodaySent = 86,
                    LastSendTime = DateTime.Now.AddHours(-2)
                };
            }
            var resp = await _http.GetStringAsync($"{_baseUrl}/api/v1/stats/overview");
            return JObject.Parse(resp)["data"].ToObject<SendStatsDto>();
        }

        // ── 私有辅助 ──────────────────────────────────────────

        private static string ReadMock(string fileName)
        {
            var path = Path.Combine(MockDir, fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException(
                    $"Mock文件不存在: {path}\n" +
                    $"请将 mock/ 目录放到程序运行目录下");
            return File.ReadAllText(path, Encoding.UTF8);
        }

        private static Task Delay(int ms) => Task.Delay(ms);
    }

    // ── DTO 模型 ──────────────────────────────────────────────

    public class LoginResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public DateTime ExpireAt { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class ApiPageResult<T>
    {
        public bool Success { get; set; }
        public int Total { get; set; }
        public List<T> List { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class SaveResult
    {
        public bool Success { get; set; }
        public int Id { get; set; }
        public string Message { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class EmailTemplateDto
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("subject")] public string Subject { get; set; }
        [JsonProperty("html_body")] public string HtmlBody { get; set; }
        [JsonProperty("variables")] public List<string> Variables { get; set; }
        [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }
    }

    public class EmailRecipientDto
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("company")] public string Company { get; set; }
        [JsonProperty("country")] public string Country { get; set; }
        [JsonProperty("group_id")] public int GroupId { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
    }

    public class RecipientGroupDto
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("count")] public int Count { get; set; }
    }

    public class SendStatsDto
    {
        [JsonProperty("total_sent")] public int TotalSent { get; set; }
        [JsonProperty("total_success")] public int TotalSuccess { get; set; }
        [JsonProperty("total_failed")] public int TotalFailed { get; set; }
        [JsonProperty("today_sent")] public int TodaySent { get; set; }
        [JsonProperty("last_send_time")] public DateTime LastSendTime { get; set; }
    }
}