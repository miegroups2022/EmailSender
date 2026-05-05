using System.Drawing;
using System.Windows.Forms;

namespace EmailSender.UI.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel  panelNav;
        private Panel  panelContent;
        private Panel  panelHeader;
        private Label  lblAppTitle;
        private Label  lblUser;
        private Button btnLogout;
        private Button btnNavTask;
        private Button btnNavTemplate;
        private Button btnNavList;
        private Button btnNavBlacklist;
        private Button btnNavSettings;
        private Label  lblNavStats;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Text          = "EmailSender v1.0";
            this.Size          = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize   = new Size(900, 600);
            this.Font          = new Font("微软雅黑", 9f);
            this.BackColor     = Color.White;

            // ── Header ─────────────────────────────────────────
            panelHeader = new Panel {
                Dock      = DockStyle.Top,
                Height    = 48,
                BackColor = Color.FromArgb(16, 37, 63),
            };
            lblAppTitle = new Label {
                Text      = "📧  Email Sender",
                Font      = new Font("微软雅黑", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                Location  = new Point(15, 10),
                Size      = new Size(220, 28),
            };
            lblUser = new Label {
                Text      = "",
                ForeColor = Color.FromArgb(180, 210, 240),
                Location  = new Point(700, 15),
                Size      = new Size(280, 20),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
            };
            btnLogout = new Button {
                Text      = "退出登录",
                Location  = new Point(1000, 10),
                Size      = new Size(80, 28),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(80, 80, 80),
                Cursor    = Cursors.Hand,
                Font      = new Font("微软雅黑", 8.5f),
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += btnLogout_Click;
            panelHeader.Controls.AddRange(new Control[] { lblAppTitle, lblUser, btnLogout });

            // ── Nav ────────────────────────────────────────────
            panelNav = new Panel {
                Dock      = DockStyle.Left,
                Width     = 150,
                BackColor = Color.FromArgb(31, 73, 125),
            };

            Button MakeNavBtn(string text, int top) => new Button {
                Text      = text,
                Location  = new Point(0, top),
                Size      = new Size(150, 46),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(31, 73, 125),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding   = new Padding(18, 0, 0, 0),
                Font      = new Font("微软雅黑", 9.5f),
                Cursor    = Cursors.Hand,
            };

            btnNavTask      = MakeNavBtn("▶  发送任务",  10);
            btnNavTemplate  = MakeNavBtn("✉  邮件模版",  56);
            btnNavList      = MakeNavBtn("📋  邮件列表", 102);
            btnNavBlacklist = MakeNavBtn("🚫  黑名单",   148);
            btnNavSettings  = MakeNavBtn("⚙  系统配置",  194);

            foreach (var b in new[]{btnNavTask,btnNavTemplate,
                                    btnNavList,btnNavBlacklist,btnNavSettings})
                b.FlatAppearance.BorderSize = 0;

            btnNavTask.Click      += btnNavTask_Click;
            btnNavTemplate.Click  += btnNavTemplate_Click;
            btnNavList.Click      += btnNavList_Click;
            btnNavBlacklist.Click += btnNavBlacklist_Click;
            btnNavSettings.Click  += btnNavSettings_Click;

            lblNavStats = new Label {
                Text      = "",
                ForeColor = Color.FromArgb(160, 200, 240),
                Location  = new Point(5, 580),
                Size      = new Size(140, 60),
                Font      = new Font("微软雅黑", 8.5f),
            };

            panelNav.Controls.AddRange(new Control[] {
                btnNavTask, btnNavTemplate, btnNavList,
                btnNavBlacklist, btnNavSettings, lblNavStats
            });

            // ── Content ────────────────────────────────────────
            panelContent = new Panel {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding   = new Padding(12),
            };

            this.Controls.Add(panelContent);
            this.Controls.Add(panelNav);
            this.Controls.Add(panelHeader);
        }
    }
}
