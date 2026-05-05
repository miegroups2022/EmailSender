using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EmailSender.Models;
using EmailSender.UI.Common;

namespace EmailSender.UI.Controls
{
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();
            LoadAllSettings();
        }

        // ── 加载所有配置 ───────────────────────────────────────
        private void LoadAllSettings()
        {
            var cfg = ServiceLocator.AppConfigRepo;

            // Tab1：meetby / SendCloud
            txtMeetbyUrl.Text      = cfg.Get("MeetbyBaseUrl")      ?? "http://ems.meetby.net";
            txtSendCloudUrl.Text   = cfg.Get("SendCloudApiUrl")    ?? "https://api.sendcloud.net/apiv2";
            txtSendCloudUser.Text  = cfg.Get("SendCloudApiUser")   ?? "";
            txtSendCloudKey.Text   = cfg.Get("SendCloudApiKey")    ?? "";

            // Tab2：AI 配置
            txtAiUrl.Text          = cfg.Get("AiApiUrl")           ?? "https://api.deepseek.com/v1";
            txtAiKey.Text          = cfg.Get("AiApiKey")           ?? "";
            txtAiModel.Text        = cfg.Get("AiModel")            ?? "deepseek-chat";

            // Tab3：ZeroBounce
            txtZbUrl.Text          = cfg.Get("ZeroBounceApiUrl")   ?? "https://api.zerobounce.net/v2";
            txtZbKey.Text          = cfg.Get("ZeroBounceApiKey")   ?? "";

            // Tab4：OAuth
            txtGoogleClientId.Text     = cfg.Get("GoogleClientId")         ?? "";
            txtGoogleClientSecret.Text = cfg.Get("GoogleClientSecret")     ?? "";
            txtMsClientId.Text         = cfg.Get("MicrosoftClientId")      ?? "";
            txtMsClientSecret.Text     = cfg.Get("MicrosoftClientSecret")  ?? "";

            // 发送账户列表
            LoadAccounts();
        }

        private void LoadAccounts()
        {
            dgvAccounts.DataSource = null;
            dgvAccounts.DataSource = ServiceLocator.SenderAccountRepo.GetAll();
        }

        // ── 保存 ───────────────────────────────────────────────
        private void btnSave_Click(object sender, EventArgs e)
        {
            var cfg = ServiceLocator.AppConfigRepo;

            cfg.Set("MeetbyBaseUrl",       txtMeetbyUrl.Text.Trim());
            cfg.Set("SendCloudApiUrl",     txtSendCloudUrl.Text.Trim());
            cfg.Set("SendCloudApiUser",    txtSendCloudUser.Text.Trim());
            cfg.Set("SendCloudApiKey",     txtSendCloudKey.Text.Trim());
            cfg.Set("AiApiUrl",            txtAiUrl.Text.Trim());
            cfg.Set("AiApiKey",            txtAiKey.Text.Trim());
            cfg.Set("AiModel",             txtAiModel.Text.Trim());
            cfg.Set("ZeroBounceApiUrl",    txtZbUrl.Text.Trim());
            cfg.Set("ZeroBounceApiKey",    txtZbKey.Text.Trim());
            cfg.Set("GoogleClientId",      txtGoogleClientId.Text.Trim());
            cfg.Set("GoogleClientSecret",  txtGoogleClientSecret.Text.Trim());
            cfg.Set("MicrosoftClientId",   txtMsClientId.Text.Trim());
            cfg.Set("MicrosoftClientSecret", txtMsClientSecret.Text.Trim());

            // 重新初始化 SendCloud 客户端
            ServiceLocator.ReloadSendCloud();

            UIHelper.Info("配置保存成功！");
        }

        // ── 发送账户管理 ───────────────────────────────────────
        private void btnAddAccount_Click(object sender, EventArgs e)
        {
            var name = txtAccountName.Text.Trim();
            var type = (AccountType)cmbAccountType.SelectedIndex;
            if (string.IsNullOrEmpty(name))
            { UIHelper.Warn("请输入账户名称"); return; }

            var account = new SenderAccount
            {
                Name        = name,
                AccountType = type,
                SmtpHost    = txtSmtpHost.Text.Trim(),
                SmtpPort    = int.TryParse(txtSmtpPort.Text, out var p) ? p : 587,
                SmtpUser    = txtSmtpUser.Text.Trim(),
                SmtpPassword= txtSmtpPass.Text.Trim(),
                SmtpFromEmail=txtSmtpFrom.Text.Trim(),
                SmtpFromName = txtSmtpFromName.Text.Trim(),
                SmtpUseSsl  = chkSsl.Checked,
                OAuthEmail  = txtOAuthEmail.Text.Trim(),
                ApiUser     = txtApiUser.Text.Trim(),
            };
            ServiceLocator.SenderAccountRepo.Add(account);
            LoadAccounts();
            UIHelper.Info($"账户「{name}」添加成功！");
        }

        private void btnDeleteAccount_Click(object sender, EventArgs e)
        {
            if (dgvAccounts.CurrentRow?.DataBoundItem is not SenderAccount acc) return;
            if (!UIHelper.Confirm($"确定删除账户「{acc.Name}」？")) return;
            ServiceLocator.SenderAccountRepo.Delete(acc.Id);
            LoadAccounts();
        }

        private void btnOAuthGmail_Click(object sender, EventArgs e)
        {
            if (dgvAccounts.CurrentRow?.DataBoundItem is not SenderAccount acc) return;
            try
            {
                ServiceLocator.oAuthHelper.StartGmailAuth(acc.Id);
                UIHelper.Info("浏览器已打开，请完成授权后回到此窗口");
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); }
        }

        private void btnOAuthHotmail_Click(object sender, EventArgs e)
        {
            if (dgvAccounts.CurrentRow?.DataBoundItem is not SenderAccount acc) return;
            try
            {
                ServiceLocator.oAuthHelper.StartHotmailAuth(acc.Id);
                UIHelper.Info("浏览器已打开，请完成授权后回到此窗口");
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); }
        }

        private void cmbAccountType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var type = (AccountType)cmbAccountType.SelectedIndex;
            panelSmtp.Visible   = type == AccountType.SMTP;
            panelOAuth.Visible  = type == AccountType.Gmail || type == AccountType.Hotmail;
            panelApiUser.Visible= type == AccountType.SendCloud;
        }
    }
}
