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
                lines.Add($"{b.Email}	{b.Type}	{b.Reason}	{b.CreatedAt}");
            File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }
    }
}
