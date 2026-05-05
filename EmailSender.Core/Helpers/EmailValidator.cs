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
        // RFC 5322 简化版正则（注意：C# 字符串中 \- 是正则的 \-）
        private static readonly Regex _regex = new Regex(
            @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
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
