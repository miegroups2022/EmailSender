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

        // 提取 {{var}} 格式变量 —— 注意 C# verbatim string 里写 \{\{ 即可
        public static List<string> ExtractDoubleBraceVars(string template)
        {
            var result  = new List<string>();
            var matches = Regex.Matches(template ?? "", @"\{\{(\w+)\}\}");
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
            var matches = Regex.Matches(template ?? "", @"%(\w+)%");
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
