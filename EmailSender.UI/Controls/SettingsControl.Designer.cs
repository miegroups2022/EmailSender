using EmailSender.UI.Common;
using System.Drawing;
using System.Windows.Forms;
using EmailSender.Models;

namespace EmailSender.UI.Controls
{
    partial class SettingsControl
    {
        private System.ComponentModel.IContainer components = null;
        private TabControl   tabSettings;
        private TabPage      tabApi;
        private TabPage      tabAi;
        private TabPage      tabVerify;
        private TabPage      tabOAuth;
        private TabPage      tabAccounts;
        private Button       btnSave;

        // Tab1 控件
        private TextBox txtMeetbyUrl, txtSendCloudUrl, txtSendCloudUser, txtSendCloudKey;
        // Tab2 控件
        private TextBox txtAiUrl, txtAiKey, txtAiModel;
        // Tab3 控件
        private TextBox txtZbUrl, txtZbKey;
        // Tab4 控件
        private TextBox txtGoogleClientId, txtGoogleClientSecret;
        private TextBox txtMsClientId, txtMsClientSecret;
        // Tab5 账户管理
        private DataGridView dgvAccounts;
        private TextBox  txtAccountName, txtSmtpHost, txtSmtpPort;
        private TextBox  txtSmtpUser, txtSmtpPass, txtSmtpFrom, txtSmtpFromName;
        private TextBox  txtOAuthEmail, txtApiUser;
        private ComboBox cmbAccountType;
        private CheckBox chkSsl;
        private Panel    panelSmtp, panelOAuth, panelApiUser;
        private Button   btnAddAccount, btnDeleteAccount, btnOAuthGmail, btnOAuthHotmail;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private Label MakeLbl(string text) =>
            new Label { Text=text, AutoSize=true,
                        Font=new Font("微软雅黑",9f),
                        Padding=new Padding(0,6,8,0) };

        private TextBox MakeTxt(int width=280, bool pwd=false) =>
            new TextBox { Width=width, Height=26,
                          Font=new Font("微软雅黑",9f),
                          UseSystemPasswordChar=pwd };

        private TableLayoutPanel MakeForm(int rows) {
            var t = new TableLayoutPanel {
                Dock=DockStyle.Top, AutoSize=true,
                ColumnCount=2, Padding=new Padding(15,12,15,0)
            };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  100));
            for (int i=0;i<rows;i++) t.RowStyles.Add(new RowStyle(SizeType.Absolute,36));
            return t;
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.Font = new Font("微软雅黑",9f);

            // ── Tab1: API 配置 ──────────────────────────────────
            txtMeetbyUrl    = MakeTxt(320);
            txtSendCloudUrl = MakeTxt(320);
            txtSendCloudUser= MakeTxt(240);
            txtSendCloudKey = MakeTxt(240, true);

            var formApi = MakeForm(4);
            formApi.Controls.Add(MakeLbl("meetby 服务器地址："),   0,0);
            formApi.Controls.Add(txtMeetbyUrl,                     1,0);
            formApi.Controls.Add(MakeLbl("SendCloud API URL："),   0,1);
            formApi.Controls.Add(txtSendCloudUrl,                  1,1);
            formApi.Controls.Add(MakeLbl("SendCloud ApiUser："),   0,2);
            formApi.Controls.Add(txtSendCloudUser,                 1,2);
            formApi.Controls.Add(MakeLbl("SendCloud ApiKey："),    0,3);
            formApi.Controls.Add(txtSendCloudKey,                  1,3);

            tabApi = new TabPage("📡  API 配置");
            tabApi.Controls.Add(formApi);

            // ── Tab2: AI 配置 ───────────────────────────────────
            txtAiUrl   = MakeTxt(320);
            txtAiKey   = MakeTxt(320, true);
            txtAiModel = MakeTxt(200);

            var formAi = MakeForm(3);
            formAi.Controls.Add(MakeLbl("AI API 地址："),  0,0);
            formAi.Controls.Add(txtAiUrl,                  1,0);
            formAi.Controls.Add(MakeLbl("AI API Key："),   0,1);
            formAi.Controls.Add(txtAiKey,                  1,1);
            formAi.Controls.Add(MakeLbl("模型名称："),      0,2);
            formAi.Controls.Add(txtAiModel,                1,2);

            tabAi = new TabPage("🤖  AI 分析");
            tabAi.Controls.Add(formAi);

            // ── Tab3: ZeroBounce ────────────────────────────────
            txtZbUrl = MakeTxt(320);
            txtZbKey = MakeTxt(320, true);

            var formZb = MakeForm(2);
            formZb.Controls.Add(MakeLbl("ZeroBounce URL："), 0,0);
            formZb.Controls.Add(txtZbUrl,                    1,0);
            formZb.Controls.Add(MakeLbl("ZeroBounce Key："), 0,1);
            formZb.Controls.Add(txtZbKey,                    1,1);

            tabVerify = new TabPage("✉  邮件验证");
            tabVerify.Controls.Add(formZb);

            // ── Tab4: OAuth ─────────────────────────────────────
            txtGoogleClientId     = MakeTxt(320);
            txtGoogleClientSecret = MakeTxt(320, true);
            txtMsClientId         = MakeTxt(320);
            txtMsClientSecret     = MakeTxt(320, true);

            var formOAuth = MakeForm(4);
            formOAuth.Controls.Add(MakeLbl("Google Client ID："),     0,0);
            formOAuth.Controls.Add(txtGoogleClientId,                 1,0);
            formOAuth.Controls.Add(MakeLbl("Google Client Secret："), 0,1);
            formOAuth.Controls.Add(txtGoogleClientSecret,             1,1);
            formOAuth.Controls.Add(MakeLbl("Microsoft Client ID："),  0,2);
            formOAuth.Controls.Add(txtMsClientId,                     1,2);
            formOAuth.Controls.Add(MakeLbl("Microsoft Secret："),     0,3);
            formOAuth.Controls.Add(txtMsClientSecret,                 1,3);

            tabOAuth = new TabPage("🔑  OAuth 配置");
            tabOAuth.Controls.Add(formOAuth);

            // ── Tab5: 发送账户 ──────────────────────────────────
            dgvAccounts = new DataGridView {
                Dock=DockStyle.Top, Height=160
            };
            UIHelper.StyleGrid(dgvAccounts);

            cmbAccountType = new ComboBox {
                Width=140, DropDownStyle=ComboBoxStyle.DropDownList,
                Font=new Font("微软雅黑",9f)
            };
            cmbAccountType.Items.AddRange(new object[]{
                "SendCloud","Gmail","Hotmail","SMTP"
            });
            cmbAccountType.SelectedIndex = 0;
            cmbAccountType.SelectedIndexChanged += cmbAccountType_SelectedIndexChanged;

            txtAccountName  = MakeTxt(160);
            txtSmtpHost     = MakeTxt(200);
            txtSmtpPort     = MakeTxt(60);  txtSmtpPort.Text="587";
            txtSmtpUser     = MakeTxt(200);
            txtSmtpPass     = MakeTxt(200, true);
            txtSmtpFrom     = MakeTxt(200);
            txtSmtpFromName = MakeTxt(160);
            txtOAuthEmail   = MakeTxt(220);
            txtApiUser      = MakeTxt(220);
            chkSsl          = new CheckBox { Text="使用SSL", Checked=true, AutoSize=true };

            // SMTP 面板
            panelSmtp = new Panel { Dock=DockStyle.Top, AutoSize=true, Visible=false };
            var formSmtp = MakeForm(5);
            formSmtp.Controls.Add(MakeLbl("SMTP 服务器："),   0,0); formSmtp.Controls.Add(txtSmtpHost,    1,0);
            formSmtp.Controls.Add(MakeLbl("端口："),           0,1); formSmtp.Controls.Add(txtSmtpPort,    1,1);
            formSmtp.Controls.Add(MakeLbl("账号："),           0,2); formSmtp.Controls.Add(txtSmtpUser,    1,2);
            formSmtp.Controls.Add(MakeLbl("密码："),           0,3); formSmtp.Controls.Add(txtSmtpPass,    1,3);
            formSmtp.Controls.Add(MakeLbl("发件人邮箱："),     0,4); formSmtp.Controls.Add(txtSmtpFrom,    1,4);
            panelSmtp.Controls.Add(formSmtp);

            // OAuth 面板
            panelOAuth = new Panel { Dock=DockStyle.Top, AutoSize=true, Visible=false };
            var formOA = MakeForm(1);
            formOA.Controls.Add(MakeLbl("OAuth 邮箱："), 0,0);
            formOA.Controls.Add(txtOAuthEmail,           1,0);

            btnOAuthGmail   = new Button { Text="授权 Gmail",   AutoSize=true, FlatStyle=FlatStyle.Flat, Cursor=Cursors.Hand };
            btnOAuthHotmail = new Button { Text="授权 Hotmail", AutoSize=true, FlatStyle=FlatStyle.Flat, Cursor=Cursors.Hand };
            btnOAuthGmail.Click   += btnOAuthGmail_Click;
            btnOAuthHotmail.Click += btnOAuthHotmail_Click;
            var flowOA = new FlowLayoutPanel { Dock=DockStyle.Top, AutoSize=true, Padding=new Padding(15,5,0,0) };
            flowOA.Controls.AddRange(new Control[]{ btnOAuthGmail, btnOAuthHotmail });
            panelOAuth.Controls.Add(flowOA);
            panelOAuth.Controls.Add(formOA);

            // SendCloud ApiUser 面板
            panelApiUser = new Panel { Dock=DockStyle.Top, AutoSize=true, Visible=true };
            var formSC = MakeForm(1);
            formSC.Controls.Add(MakeLbl("SendCloud ApiUser："), 0,0);
            formSC.Controls.Add(txtApiUser,                     1,0);
            panelApiUser.Controls.Add(formSC);

            // 账户基本信息
            var formAccBase = MakeForm(2);
            formAccBase.Controls.Add(MakeLbl("账户名称："), 0,0); formAccBase.Controls.Add(txtAccountName,  1,0);
            formAccBase.Controls.Add(MakeLbl("账户类型："), 0,1); formAccBase.Controls.Add(cmbAccountType,  1,1);

            btnAddAccount    = new Button { Text="＋ 添加账户", AutoSize=true, BackColor=Color.FromArgb(31,73,125), ForeColor=Color.White, FlatStyle=FlatStyle.Flat, Cursor=Cursors.Hand };
            btnDeleteAccount = new Button { Text="✕ 删除账户", AutoSize=true, BackColor=Color.FromArgb(196,43,28),  ForeColor=Color.White, FlatStyle=FlatStyle.Flat, Cursor=Cursors.Hand };
            btnAddAccount.FlatAppearance.BorderSize    = 0;
            btnDeleteAccount.FlatAppearance.BorderSize = 0;
            btnAddAccount.Click    += btnAddAccount_Click;
            btnDeleteAccount.Click += btnDeleteAccount_Click;

            var flowAccBtn = new FlowLayoutPanel { Dock=DockStyle.Top, AutoSize=true, Padding=new Padding(15,8,0,0) };
            flowAccBtn.Controls.AddRange(new Control[]{ btnAddAccount, btnDeleteAccount });

            var panelAccForm = new Panel { Dock=DockStyle.Fill, AutoScroll=true };
            panelAccForm.Controls.Add(panelSmtp);
            panelAccForm.Controls.Add(panelOAuth);
            panelAccForm.Controls.Add(panelApiUser);
            panelAccForm.Controls.Add(formAccBase);

            var splitAcc = new SplitContainer {
                Dock=DockStyle.Fill, Orientation=Orientation.Horizontal,
                SplitterDistance=180
            };
            splitAcc.Panel1.Controls.Add(dgvAccounts);
            splitAcc.Panel2.Controls.Add(flowAccBtn);
            splitAcc.Panel2.Controls.Add(panelAccForm);

            tabAccounts = new TabPage("👤  发送账户");
            tabAccounts.Controls.Add(splitAcc);

            // ── TabControl 组装 ─────────────────────────────────
            tabSettings = new TabControl { Dock=DockStyle.Fill };
            tabSettings.TabPages.AddRange(new TabPage[]{
                tabApi, tabAi, tabVerify, tabOAuth, tabAccounts
            });

            // ── 底部保存按钮 ────────────────────────────────────
            var panelBottom = new Panel {
                Dock=DockStyle.Bottom, Height=50,
                BackColor=Color.FromArgb(245,247,250),
                Padding=new Padding(15,8,15,8)
            };
            btnSave = new Button {
                Text="💾  保存所有配置",
                Size=new Size(160,34),
                Dock=DockStyle.Right,
                BackColor=Color.FromArgb(31,73,125),
                ForeColor=Color.White,
                FlatStyle=FlatStyle.Flat,
                Font=new Font("微软雅黑",10f,FontStyle.Bold),
                Cursor=Cursors.Hand,
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;
            panelBottom.Controls.Add(btnSave);

            this.Controls.Add(tabSettings);
            this.Controls.Add(panelBottom);
        }
    }
}
