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
