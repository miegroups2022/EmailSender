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
