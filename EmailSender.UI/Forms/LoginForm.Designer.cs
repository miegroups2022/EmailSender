using System.Drawing;
using System.Windows.Forms;

namespace EmailSender.UI.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label      lblTitle;
        private Label      lblUsername;
        private Label      lblPassword;
        private TextBox    txtUsername;
        private TextBox    txtPassword;
        private Button     btnLogin;
        private Label      lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Text            = "EmailSender — 登录";
            this.Size            = new Size(380, 300);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = Color.White;
            this.Font            = new Font("微软雅黑", 9f);

            lblTitle = new Label {
                Text      = "📧 Email Sender",
                Font      = new Font("微软雅黑", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 73, 125),
                Location  = new Point(60, 25),
                Size      = new Size(260, 35),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            };

            lblUsername = new Label { Text="用户名：", Location=new Point(50,85),  Size=new Size(65,22) };
            lblPassword = new Label { Text="密  码：", Location=new Point(50,120), Size=new Size(65,22) };

            txtUsername = new TextBox { Location=new Point(120,82),  Size=new Size(190,24) };
            txtPassword = new TextBox { Location=new Point(120,117), Size=new Size(190,24), UseSystemPasswordChar=true };
            txtPassword.KeyDown += txtPassword_KeyDown;

            btnLogin = new Button {
                Text      = "登  录",
                Location  = new Point(90, 165),
                Size      = new Size(200, 36),
                BackColor = Color.FromArgb(31, 73, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("微软雅黑", 10f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += btnLogin_Click;

            lblStatus = new Label {
                Text      = "",
                Location  = new Point(50, 215),
                Size      = new Size(280, 22),
                ForeColor = Color.Gray,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            };

            this.Controls.AddRange(new Control[] {
                lblTitle, lblUsername, lblPassword,
                txtUsername, txtPassword, btnLogin, lblStatus
            });
        }
    }
}
