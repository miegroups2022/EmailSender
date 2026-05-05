using EmailSender.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EmailSender.Models;          // ← EmailRecipient 在这里

namespace EmailSender.Core.Services
{
    /// <summary>
    /// 从本地 mock/ 目录读取 JSON 文件模拟服务器接口。
    /// 真实接口就绪后，将此类替换为 RealApiService.cs 即可。
    /// 
    /// mock 文件放置位置（程序运行目录下）：
    ///   mock/auth_login.json
    ///   mock/templates.json
    ///   mock/recipients.json
    /// </summary>
    public class MockApiService : IApiService
    {
        // ── 当前登录 Token ────────────────────────────────────────────
        private string _token;
        private UserInfo _currentUser;

        // ── mock 文件目录（相对于程序运行目录）────────────────────────
        private static string MockDir =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mock");

        // ── 1. 登录 ───────────────────────────────────────────────────

        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            await SimulateDelay(400);

            // 硬编码：admin/admin 直接通过
            if (username == "admin" && password == "admin")
            {
                var json = ReadMockFile("auth_login.json");
                var obj = JObject.Parse(json);
                var data = obj["data"];

                _token = data["token"].ToString();
                _currentUser = new UserInfo
                {
                    Id = (int)data["user"]["id"],
                    Username = data["user"]["username"].ToString(),
                    Nickname = data["user"]["nickname"].ToString(),
                    Email = data["user"]["email"].ToString()
                };

                return new LoginResult
                {
                    Success = true,
                    Token = _token,
                    User = _currentUser,
                    ExpireAt = DateTime.Now.AddDays(7)
                };
            }

            return new LoginResult
            {
                Success = false,
                ErrorMsg = "用户名或密码错误（测试账号：admin / admin）"
            };
        }

        public void Logout() => _token = null;

        public bool IsLoggedIn => !string.IsNullOrEmpty(_token);

        // ── 2. 模板接口 ───────────────────────────────────────────────

        public async Task<ApiListResult<EmailTemplate>> GetTemplatesAsync(
            int page = 1, int pageSize = 20, string keyword = "")
        {
            await SimulateDelay(300);
            EnsureLoggedIn();

            var json = ReadMockFile("templates.json");
            var obj = JObject.Parse(json);
            var list = obj["data"]["list"].ToObject<List<EmailTemplate>>();

            // 本地模拟关键字过滤
            if (!string.IsNullOrWhiteSpace(keyword))
                list = list.FindAll(t =>
                    t.Name.Contains(keyword) || t.Subject.Contains(keyword));

            return new ApiListResult<EmailTemplate>
            {
                Success = true,
                Total = list.Count,
                List = list
            };
        }

        public async Task<EmailTemplate> GetTemplateAsync(int id)
        {
            await SimulateDelay(200);
            EnsureLoggedIn();

            var result = await GetTemplatesAsync();
            return result.List.Find(t => t.Id == id);
        }

        public async Task<SaveResult> SaveTemplateAsync(EmailTemplate template)
        {
            await SimulateDelay(400);
            EnsureLoggedIn();

            if (string.IsNullOrWhiteSpace(template.Name))
                return new SaveResult { Success = false, ErrorMsg = "模板名称不能为空" };
            if (string.IsNullOrWhiteSpace(template.Subject))
                return new SaveResult { Success = false, ErrorMsg = "邮件主题不能为空" };

            // 模拟保存成功（实际不写文件，内存操作）
            return new SaveResult
            {
                Success = true,
                Id = template.Id == 0 ? new Random().Next(100, 999) : template.Id,
                Message = template.Id == 0 ? "模板创建成功" : "模板更新成功"
            };
        }

        public async Task<SaveResult> DeleteTemplateAsync(int id)
        {
            await SimulateDelay(200);
            EnsureLoggedIn();
            return new SaveResult { Success = true, Message = "删除成功" };
        }

        // ── 3. 收件人接口 ─────────────────────────────────────────────

        public async Task<ApiListResult<RecipientGroup>> GetGroupsAsync()
        {
            await SimulateDelay(200);
            EnsureLoggedIn();

            var json = ReadMockFile("recipients.json");
            var obj = JObject.Parse(json);
            var groups = obj["data"]["groups"].ToObject<List<RecipientGroup>>();

            return new ApiListResult<RecipientGroup>
            {
                Success = true,
                Total = groups.Count,
                List = groups
            };
        }

        public async Task<ApiListResult<EmailRecipient>> GetRecipientsAsync(
            int groupId = 0, int page = 1, int pageSize = 50)
        {
            await SimulateDelay(300);
            EnsureLoggedIn();

            var json = ReadMockFile("recipients.json");
            var obj = JObject.Parse(json);
            var list = obj["data"]["list"].ToObject<List<EmailRecipient>>();

            if (groupId > 0)
                list = list.FindAll(r => r.GroupId == groupId);

            return new ApiListResult<EmailRecipient>
            {
                Success = true,
                Total = list.Count,
                List = list
            };
        }

        // ── 4. 发送任务接口 ───────────────────────────────────────────

        public async Task<SaveResult> SubmitSendTaskAsync(SendTaskRequest request)
        {
            await SimulateDelay(600);
            EnsureLoggedIn();

            if (request.TemplateId <= 0)
                return new SaveResult { Success = false, ErrorMsg = "请选择发送模板" };
            if (request.RecipientCount <= 0)
                return new SaveResult { Success = false, ErrorMsg = "收件人列表为空" };

            return new SaveResult
            {
                Success = true,
                Id = new Random().Next(1000, 9999),
                Message = $"✅ 发送任务已提交！\n" +
                          $"收件人：{request.RecipientCount} 人\n" +
                          $"预计开始：{DateTime.Now.AddSeconds(5):HH:mm:ss}"
            };
        }

        // ── 5. 统计接口 ───────────────────────────────────────────────

        public async Task<SendStats> GetStatsAsync()
        {
            await SimulateDelay(200);
            EnsureLoggedIn();

            return new SendStats
            {
                TotalSent = 1280,
                TotalSuccess = 1245,
                TotalFailed = 35,
                TodaySent = 86,
                LastSendTime = DateTime.Now.AddHours(-2)
            };
        }

        // ── 私有辅助 ──────────────────────────────────────────────────

        private static string ReadMockFile(string fileName)
        {
            var path = Path.Combine(MockDir, fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException(
                    $"Mock 数据文件不存在：{path}\n" +
                    $"请将 mock/ 目录复制到程序运行目录：\n{MockDir}");
            return File.ReadAllText(path, System.Text.Encoding.UTF8);
        }

        private void EnsureLoggedIn()
        {
            if (!IsLoggedIn)
                throw new UnauthorizedAccessException("请先登录");
        }

        private static Task SimulateDelay(int ms) =>
            Task.Delay(ms); // 模拟真实网络延迟
    }

    // ── 接口契约（方便将来替换为真实实现）────────────────────────────

    public interface IApiService
    {
        Task<LoginResult> LoginAsync(string username, string password);
        void Logout();
        bool IsLoggedIn { get; }
        Task<ApiListResult<EmailTemplate>> GetTemplatesAsync(int page = 1, int pageSize = 20, string keyword = "");
        Task<EmailTemplate> GetTemplateAsync(int id);
        Task<SaveResult> SaveTemplateAsync(EmailTemplate template);
        Task<SaveResult> DeleteTemplateAsync(int id);
        Task<ApiListResult<RecipientGroup>> GetGroupsAsync();
        Task<ApiListResult<EmailRecipient>> GetRecipientsAsync(int groupId = 0, int page = 1, int pageSize = 50);
        Task<SaveResult> SubmitSendTaskAsync(SendTaskRequest request);
        Task<SendStats> GetStatsAsync();
    }

    // ── DTO 模型 ──────────────────────────────────────────────────────

    public class LoginResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public UserInfo User { get; set; }
        public DateTime ExpireAt { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
    }

    public class ApiListResult<T>
    {
        public bool Success { get; set; }
        public int Total { get; set; }
        public List<T> List { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class RecipientGroup
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("count")] public int Count { get; set; }
    }

    public class SaveResult
    {
        public bool Success { get; set; }
        public int Id { get; set; }
        public string Message { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class SendTaskRequest
    {
        public int TemplateId { get; set; }
        public int GroupId { get; set; }
        public int RecipientCount { get; set; }
        public string SenderAccount { get; set; }
    }

    public class SendStats
    {
        public int TotalSent { get; set; }
        public int TotalSuccess { get; set; }
        public int TotalFailed { get; set; }
        public int TodaySent { get; set; }
        public DateTime LastSendTime { get; set; }
    }
}