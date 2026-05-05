using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using EmailSender.Core.Services;
using EmailSender.Models;
using EmailSender.Core;

namespace EmailSender.UI.Forms
{
    public partial class AccountEditDialog : Form
    {
        public SenderAccount Account    { get; private set; }
        public string OutlookTempKey   { get; private set; }  // 授权后的临时 token key，入库拿到 Id 后重命名

        private bool _isNew;

        public AccountEditDialog(SenderAccount existing = null)
        {
            _isNew = existing == null;
            Account = existing ?? new SenderAccount
            {
                Provider    = (int)MailProvider.Custom,
                AuthMode    = (int)AuthType.SmtpPassword,
                SmtpPort    = 465,
                SmtpUseSsl      = true,
                DailyLimit  = 200,
                IntervalMin = 30,
                IntervalMax = 90,
                NeedVpn     = false
            };

            InitializeComponent();
            PopulateProviderCombo();

            if (!_isNew)
            {
                this.Text = "编辑邮箱账号";
                FillFields();
            }
            else
            {
                cmbProvider.SelectedIndex = 0;
            }
        }

        private void PopulateProviderCombo()
        {
            cmbProvider.Items.Clear();
            foreach (MailProvider p in Enum.GetValues<MailProvider>())
                cmbProvider.Items.Add(MailProviderConfigs.Configs[p].DisplayName);
        }

        private void cmbProvider_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProvider.SelectedIndex < 0) return;
            var provider = (MailProvider)cmbProvider.SelectedIndex;
            var cfg = MailProviderConfigs.Configs[provider];

            txtHost.Text         = cfg.SmtpHost;
            nudPort.Value        = cfg.SmtpPort > 0 ? cfg.SmtpPort : 465;
            chkSsl.Checked       = cfg.UseSsl;
            nudDailyLimit.Value  = cfg.DailyLimit;
            nudIntervalMin.Value = cfg.IntervalMinSec;
            nudIntervalMax.Value = cfg.IntervalMaxSec;
            chkVpn.Checked       = cfg.NeedVpn;

            bool isOAuth = cfg.AuthType == AuthType.OAuth2;
            pnlSmtp.Visible   = !isOAuth;
            pnlOAuth.Visible  = isOAuth;
            lblGuide.Text     = cfg.SetupGuide;
            pnlVpnTip.Visible = cfg.NeedVpn;

            bool isCustom = provider == MailProvider.Custom;
            txtHost.ReadOnly = !isCustom && !string.IsNullOrEmpty(cfg.SmtpHost);
            nudPort.Enabled  = isCustom || cfg.SmtpPort == 0;
        }

        private void FillFields()
        {
            txtAccountName.Text       = Account.AccountName;
            cmbProvider.SelectedIndex = Account.Provider;
            txtHost.Text              = Account.SmtpHost;
            nudPort.Value             = Account.SmtpPort > 0 ? Account.SmtpPort : 465;
            chkSsl.Checked            = Account.SmtpUseSsl;
            txtUser.Text              = Account.SmtpUser;
            txtPass.Text              = Account.SmtpPassword;
            txtSenderName.Text        = Account.SmtpFromName;
            txtSenderEmail.Text       = Account.SmtpFromEmail;
            nudDailyLimit.Value       = Account.DailyLimit;
            nudIntervalMin.Value      = Account.IntervalMin;
            nudIntervalMax.Value      = Account.IntervalMax;
            chkVpn.Checked            = Account.NeedVpn;
        }

        // ── OAuth2 Loopback 授权 ──
        private async void btnOAuthLogin_Click(object sender, EventArgs e)
        {
            if (cmbProvider.SelectedIndex < 0) return;
            var provider = (MailProvider)cmbProvider.SelectedIndex;

            // 目前只支持 Gmail OAuth2；Outlook 走真正的 MSAL OAuth2
            if (provider == MailProvider.Gmail)
            {
                await DoGmailOAuth();
            }
            else if (provider == MailProvider.Outlook)
            {
                await DoOutlookOAuth();
            }
        }

        private async Task DoOutlookOAuth()
        {
            var configPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "outlook_client.json");

            if (!System.IO.File.Exists(configPath))
            {
                var r = MessageBox.Show(
                    "首次使用 Outlook OAuth2 需要完成一次性配置：\n\n"
                  + "1. 免费注册 Azure：azure.microsoft.com/free\n"
                  + "   （仅需信用卡验证身份，不扣费）\n\n"
                  + "2. portal.azure.com → Microsoft Entra ID\n"
                  + "   → 应用注册 → 新注册\n"
                  + "   · 名称：WaimaoTong（随意）\n"
                  + "   · 账户类型：任何组织目录中的账户和个人 Microsoft 账户\n"
                  + "   · 重定向 URI：平台=公共客户端/本机，URI=http://localhost\n\n"
                  + "3. 注册完成后复制「应用程序（客户端）ID」\n\n"
                  + "4. 在程序目录创建文件 outlook_client.json，内容：\n"
                  + "   { \"client_id\": \"粘贴你的ID\" }\n\n"
                  + "点击「去注册」打开 Azure 门户，完成后重新点击授权按钮。",
                    "需要 Azure 配置", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                if (r == DialogResult.OK)
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo(
                            "https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/CreateApplicationBlade")
                        { UseShellExecute = true });
                return;
            }

            btnOAuthLogin.Enabled = false;
            btnOAuthLogin.Text    = "授权中，请在浏览器完成操作...";
            lblGuide.Text         = "已打开浏览器，请登录微软账号并点击「接受」，完成后本窗口将自动更新。";

            var (ok, email, tempKey, tokenJson, error) =
                await WaimaoTong.Services.OutlookOAuthService.AuthorizeAsync();

            if (ok)
            {
                OutlookTempKey         = tempKey;   // 保存临时 key，入库后由调用方重命名
                Account.OAuthTokenJson = tokenJson;
                Account.SmtpUser       = email;
                Account.SmtpFromEmail    = email;
                Account.SmtpHost       = "smtp-mail.outlook.com";  // 直接写入 Account
                Account.SmtpPort       = 587;
                Account.SmtpUseSsl         = false;  // STARTTLS

                txtUser.Text        = email;
                txtSenderEmail.Text = email;
                if (string.IsNullOrWhiteSpace(txtSenderName.Text))
                    txtSenderName.Text = email.Split('@')[0];
                if (string.IsNullOrWhiteSpace(txtAccountName.Text))
                    txtAccountName.Text = $"Outlook - {email}";

                pnlOAuth.Visible  = false;
                pnlSmtp.Visible   = true;
                txtHost.Text      = Account.SmtpHost;
                nudPort.Value     = Account.SmtpPort;
                chkSsl.Checked    = Account.SmtpUseSsl;
                txtHost.ReadOnly  = true;
                nudPort.Enabled   = false;

                lblGuide.Text      = $"✅ 授权成功！已获取 {email} 的访问权限。";
                lblGuide.ForeColor = System.Drawing.Color.FromArgb(76, 175, 80);
                MessageBox.Show($"Outlook 授权成功！\n授权账号：{email}",
                    "授权成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblGuide.Text      = $"❌ 授权失败：{error}";
                lblGuide.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"授权失败：\n{error}", "授权失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            btnOAuthLogin.Enabled = true;
            btnOAuthLogin.Text    = "📋 查看申请步骤 & 打开申请页面";
        }

        private async Task DoGmailOAuth()
        {
            // 检查 credentials.json 是否存在
            var credPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "credentials.json");
            if (!System.IO.File.Exists(credPath))
            {
                MessageBox.Show(
                    "未找到 credentials.json，请先完成以下步骤：\n\n"
                  + "1. 打开 console.cloud.google.com\n"
                  + "2. 创建项目 → 启用 Gmail API\n"
                  + "3. 凭据 → 创建 OAuth2 凭据 → 类型选「桌面应用（Desktop app）」\n"
                  + "4. 下载 JSON 文件，改名为 credentials.json\n"
                  + $"5. 放到程序目录：{AppDomain.CurrentDomain.BaseDirectory}",
                    "缺少 credentials.json", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnOAuthLogin.Enabled = false;
            btnOAuthLogin.Text    = "授权中，请在浏览器完成操作...";
            lblGuide.Text         = "已打开浏览器，请登录 Google 账号并点击「允许」，完成后本窗口将自动更新。";

            var accountKey = $"acc_new_{DateTime.Now.Ticks}";
            var (ok, email, tokenJson, error) =
                await WaimaoTong.Services.GmailOAuthService.AuthorizeAsync(accountKey);

            if (ok)
            {
                Account.OAuthTokenJson = tokenJson;
                Account.SmtpUser       = email;
                Account.SmtpFromEmail    = email;
                Account.SmtpHost       = "smtp.gmail.com";  // 直接写入 Account
                Account.SmtpPort       = 465;
                Account.SmtpUseSsl         = true;

                txtUser.Text        = email;
                txtSenderEmail.Text = email;
                if (string.IsNullOrWhiteSpace(txtSenderName.Text))
                    txtSenderName.Text = email.Split('@')[0];
                if (string.IsNullOrWhiteSpace(txtAccountName.Text))
                    txtAccountName.Text = $"Gmail - {email}";

                txtHost.Text     = Account.SmtpHost;
                nudPort.Value    = Account.SmtpPort;
                chkSsl.Checked   = Account.SmtpUseSsl;
                txtHost.ReadOnly = true;
                nudPort.Enabled  = false;
                pnlOAuth.Visible = false;
                pnlSmtp.Visible  = true;

                lblGuide.Text     = $"✅ 授权成功！已获取 {email} 的访问权限。";
                lblGuide.ForeColor = System.Drawing.Color.FromArgb(76, 175, 80);
                MessageBox.Show($"Gmail 授权成功！\n授权账号：{email}\n\nToken 已保存，无需重复授权。",
                    "授权成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblGuide.Text      = $"❌ 授权失败：{error}";
                lblGuide.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"授权失败：\n{error}", "授权失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            btnOAuthLogin.Enabled = true;
            btnOAuthLogin.Text    = "📋 查看申请步骤 & 打开申请页面";
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            if (!CollectFormValues()) return;
            btnTest.Enabled = false;
            btnTest.Text    = "测试中...";

            // Outlook OAuth 账号未入库时（Id==0），用临时 key 测试
            var provider = (MailProvider)Account.Provider;
            if (Account.IsOAuthAuthorized
                && provider == MailProvider.Outlook
                && Account.Id == 0
                && !string.IsNullOrEmpty(OutlookTempKey))
            {
                await OutlookOAuthService.TestAsync(Account, msg =>
                {
                    this.BeginInvoke((Action)(() =>
                    {
                        MessageBox.Show(msg, "SMTP 测试结果",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        btnTest.Enabled = true;
                        btnTest.Text    = "测试连接";
                    }));
                }, accountKey: OutlookTempKey);
                return;
            }

            await EmailService.TestSmtpAsync(Account, msg =>
            {
                this.BeginInvoke((Action)(() =>
                {
                    MessageBox.Show(msg, "SMTP 测试结果",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnTest.Enabled = true;
                    btnTest.Text    = "测试连接";
                }));
            });
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!CollectFormValues()) return;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private bool CollectFormValues()
        {
            if (cmbProvider.SelectedIndex < 0) return false;
            var provider = (MailProvider)cmbProvider.SelectedIndex;
            var cfg      = MailProviderConfigs.Configs[provider];
            bool isOAuth = cfg.AuthType == AuthType.OAuth2;

            if (string.IsNullOrWhiteSpace(txtAccountName.Text))
            {
                MessageBox.Show("请填写账号别名", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // OAuth 账号：必须已完成授权
            if (isOAuth && string.IsNullOrEmpty(Account.OAuthTokenJson))
            {
                MessageBox.Show("请先点击授权按钮完成 Gmail 授权，再保存。",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // SMTP 账号：检查用户名和服务器
            if (!isOAuth && string.IsNullOrWhiteSpace(txtUser.Text))
            {
                MessageBox.Show("请填写用户名/邮箱！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!isOAuth && string.IsNullOrWhiteSpace(txtHost.Text))
            {
                MessageBox.Show("SMTP服务器地址不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            Account.AccountName = txtAccountName.Text.Trim();
            Account.Provider    = (int)provider;
            Account.AuthMode    = (int)cfg.AuthType;
            Account.DailyLimit  = (int)nudDailyLimit.Value;
            Account.IntervalMin = (int)nudIntervalMin.Value;
            Account.IntervalMax = (int)nudIntervalMax.Value;
            Account.NeedVpn     = chkVpn.Checked;

            if (isOAuth)
            {
                // OAuth 账号：SmtpUser/SmtpFromEmail 在授权时已填入 Account，不从输入框读取
                // SmtpFromName 允许用户手动补充
                if (!string.IsNullOrWhiteSpace(txtSenderName.Text))
                    Account.SmtpFromName = txtSenderName.Text.Trim();
                if (string.IsNullOrWhiteSpace(Account.SmtpFromName))
                    Account.SmtpFromName = Account.SmtpFromEmail;
            }
            else
            {
                // SMTP 账号：从输入框读取全部字段
                Account.SmtpHost    = txtHost.Text.Trim();
                Account.SmtpPort    = (int)nudPort.Value;
                Account.SmtpUseSsl      = chkSsl.Checked;
                Account.SmtpUser    = txtUser.Text.Trim();
                Account.SmtpPassword    = txtPass.Text;
                Account.SmtpFromName  = txtSenderName.Text.Trim();
                Account.SmtpFromEmail = txtSenderEmail.Text.Trim();

                if (string.IsNullOrWhiteSpace(Account.SmtpFromEmail) && Account.SmtpUser.Contains('@'))
                    Account.SmtpFromEmail = Account.SmtpUser;
            }

            return true;
        }
    }
}
