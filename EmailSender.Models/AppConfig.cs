using System;

namespace EmailSender.Models
{
    /// <summary>
    /// 应用配置键值对，存储在 SQLite config 表中
    /// </summary>
    public class AppConfig
    {
        public int    Id        { get; set; }
        public string Key       { get; set; }
        public string Value     { get; set; }
        public string Remark    { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
