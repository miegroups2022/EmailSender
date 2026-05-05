using System;
using System.Windows.Forms;
using EmailSender.UI.Common;

namespace EmailSender.UI.Forms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                UIHelper.Warn("请输入用户名和密码");
                return;
            }

            btnLogin.Enabled = false;
            lblStatus.Text   = "正在登录...";

            try
            {
                await ServiceLocator.AuthService.LoginAsync(username, password);
                Hide();
                var main = new MainForm();
                main.FormClosed += (s, args) => Close();
                main.Show();
            }
            catch (Exception ex)
            {
                UIHelper.Error($"登录失败：{ex.Message}");
                lblStatus.Text = "登录失败，请重试";
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnLogin_Click(sender, e);
        }
    }
}
