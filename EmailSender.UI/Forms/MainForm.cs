using System;
using System.Drawing;
using System.Windows.Forms;
using EmailSender.UI.Common;
using EmailSender.UI.Controls;

namespace EmailSender.UI.Forms
{
    public partial class MainForm : Form
    {
        private UserControl _currentControl;

        public MainForm()
        {
            InitializeComponent();
            LoadControl(new SendTaskControl());
            HighlightNav(btnNavTask);
            UpdateStatusBar();
        }

        private void LoadControl(UserControl ctrl)
        {
            _currentControl?.Dispose();
            _currentControl      = ctrl;
            ctrl.Dock            = DockStyle.Fill;
            panelContent.Controls.Clear();
            panelContent.Controls.Add(ctrl);
        }

        private void HighlightNav(Button active)
        {
            var navBtns = new[] { btnNavTask, btnNavTemplate,
                                  btnNavList, btnNavBlacklist, btnNavSettings };
            foreach (var b in navBtns)
            {
                b.BackColor = Color.FromArgb(31, 73, 125);
                b.ForeColor = Color.White;
            }
            active.BackColor = Color.FromArgb(46, 116, 181);
            active.ForeColor = Color.White;
        }

        private void UpdateStatusBar()
        {
            lblUser.Text = $"登录：{ServiceLocator.AuthService.CurrentUsername}";
        }

        // ── 导航按钮事件 ──────────────────────────────────────
        private void btnNavTask_Click(object s, EventArgs e)
        {
            LoadControl(new SendTaskControl());
            HighlightNav(btnNavTask);
        }
        private void btnNavTemplate_Click(object s, EventArgs e)
        {
            LoadControl(new TemplateControl());
            HighlightNav(btnNavTemplate);
        }
        private void btnNavList_Click(object s, EventArgs e)
        {
            LoadControl(new EmailListControl());
            HighlightNav(btnNavList);
        }
        private void btnNavBlacklist_Click(object s, EventArgs e)
        {
            LoadControl(new BlacklistControl());
            HighlightNav(btnNavBlacklist);
        }
        private void btnNavSettings_Click(object s, EventArgs e)
        {
            LoadControl(new SettingsControl());
            HighlightNav(btnNavSettings);
        }

        private void btnLogout_Click(object s, EventArgs e)
        {
            if (!UIHelper.Confirm("确定退出登录？")) return;
            ServiceLocator.AuthService.Logout();
            var login = new LoginForm();
            login.Show();
            Close();
        }
    }
}
