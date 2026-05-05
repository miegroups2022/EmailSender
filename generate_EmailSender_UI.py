import os

base = "EmailSender.UI"
for d in ["Properties", "Forms", "Controls", "WizardSteps", "Common", "Resources"]:
    os.makedirs(f"{base}/{d}", exist_ok=True)

files = {}

# ══════════════════════════════════════════════════════════════
# csproj
# ══════════════════════════════════════════════════════════════
files["EmailSender.UI.csproj"] = """\
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props" />
  <PropertyGroup>
    <ProjectGuid>{44444444-4444-4444-4444-444444444444}</ProjectGuid>
    <OutputType>WinExe</OutputType>
    <RootNamespace>EmailSender.UI</RootNamespace>
    <AssemblyName>EmailSender</AssemblyName>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
    <ApplicationIcon>Resources\\app.ico</ApplicationIcon>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Program.cs" />
    <Compile Include="Common\\ServiceLocator.cs" />
    <Compile Include="Common\\UIHelper.cs" />
    <Compile Include="Forms\\LoginForm.cs" />
    <Compile Include="Forms\\LoginForm.Designer.cs" />
    <Compile Include="Forms\\MainForm.cs" />
    <Compile Include="Forms\\MainForm.Designer.cs" />
    <Compile Include="Forms\\WizardForm.cs" />
    <Compile Include="Forms\\WizardForm.Designer.cs" />
    <Compile Include="Controls\\SendTaskControl.cs" />
    <Compile Include="Controls\\SendTaskControl.Designer.cs" />
    <Compile Include="Controls\\TemplateControl.cs" />
    <Compile Include="Controls\\TemplateControl.Designer.cs" />
    <Compile Include="Controls\\EmailListControl.cs" />
    <Compile Include="Controls\\EmailListControl.Designer.cs" />
    <Compile Include="Controls\\BlacklistControl.cs" />
    <Compile Include="Controls\\BlacklistControl.Designer.cs" />
    <Compile Include="Controls\\SettingsControl.cs" />
    <Compile Include="Controls\\SettingsControl.Designer.cs" />
    <Compile Include="WizardSteps\\Step1TemplatePanel.cs" />
    <Compile Include="WizardSteps\\Step1TemplatePanel.Designer.cs" />
    <Compile Include="WizardSteps\\Step2ListPanel.cs" />
    <Compile Include="WizardSteps\\Step2ListPanel.Designer.cs" />
    <Compile Include="WizardSteps\\Step3SettingsPanel.cs" />
    <Compile Include="WizardSteps\\Step3SettingsPanel.Designer.cs" />
    <Compile Include="Properties\\AssemblyInfo.cs" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\\EmailSender.Models\\EmailSender.Models.csproj">
      <Project>{11111111-1111-1111-1111-111111111111}</Project>
      <Name>EmailSender.Models</Name>
    </ProjectReference>
    <ProjectReference Include="..\\EmailSender.Data\\EmailSender.Data.csproj">
      <Project>{22222222-2222-2222-2222-222222222222}</Project>
      <Name>EmailSender.Data</Name>
    </ProjectReference>
    <ProjectReference Include="..\\EmailSender.Core\\EmailSender.Core.csproj">
      <Project>{33333333-3333-3333-3333-333333333333}</Project>
      <Name>EmailSender.Core</Name>
    </ProjectReference>
  </ItemGroup>
  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Windows.Forms" />
    <Reference Include="System.Drawing" />
    <Reference Include="System.Data" />
    <Reference Include="Newtonsoft.Json">
      <HintPath>..\\packages\\Newtonsoft.Json.13.0.3\\lib\\net45\\Newtonsoft.Json.dll</HintPath>
    </Reference>
  </ItemGroup>
  <Import Project="$(MSBuildToolsPath)\\Microsoft.CSharp.targets" />
</Project>
"""

# ══════════════════════════════════════════════════════════════
# Properties
# ══════════════════════════════════════════════════════════════
files["Properties/AssemblyInfo.cs"] = """\
using System.Reflection;
using System.Runtime.InteropServices;
[assembly: AssemblyTitle("EmailSender")]
[assembly: AssemblyDescription("EDM Email Sender Client")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: ComVisible(false)]
"""

# ══════════════════════════════════════════════════════════════
# App.config
# ══════════════════════════════════════════════════════════════
files["App.config"] = """\
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
  </startup>
  <appSettings>
    <!-- meetby 服务器 -->
    <add key="MeetbyBaseUrl"       value="http://ems.meetby.net" />
    <!-- SendCloud -->
    <add key="SendCloudApiUrl"     value="https://api.sendcloud.net/apiv2" />
    <!-- ZeroBounce 邮件验证 -->
    <add key="ZeroBounceApiUrl"    value="https://api.zerobounce.net/v2" />
    <!-- AI 分析 -->
    <add key="AiApiUrl"            value="https://api.deepseek.com/v1" />
    <add key="AiModel"             value="deepseek-chat" />
    <!-- 本地数据库 -->
    <add key="DbFileName"          value="emailsender.db" />
    <!-- 发送默认参数 -->
    <add key="DefaultBatchSize"    value="50" />
    <add key="DefaultInterval"     value="5" />
    <add key="DefaultThreadCount"  value="3" />
    <add key="DefaultRetryMax"     value="3" />
  </appSettings>
</configuration>
"""

# ══════════════════════════════════════════════════════════════
# Program.cs
# ══════════════════════════════════════════════════════════════
files["Program.cs"] = """\
using System;
using System.Windows.Forms;
using EmailSender.Data;
using EmailSender.UI.Common;
using EmailSender.UI.Forms;

namespace EmailSender.UI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // 初始化数据库
                DatabaseHelper.Initialize();

                // 初始化服务容器
                ServiceLocator.Initialize();

                // 尝试恢复上次登录会话
                var auth = ServiceLocator.AuthService;
                if (auth.TryRestoreSession())
                {
                    Application.Run(new MainForm());
                }
                else
                {
                    Application.Run(new LoginForm());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"程序启动失败：{ex.Message}\\n\\n{ex.StackTrace}",
                    "启动错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
"""

# ══════════════════════════════════════════════════════════════
# Common/ServiceLocator.cs
# ══════════════════════════════════════════════════════════════
files["Common/ServiceLocator.cs"] = """\
using System.Configuration;
using EmailSender.Core.ApiClients;
using EmailSender.Core.Helpers;
using EmailSender.Core.Services;
using EmailSender.Data.Repositories;

namespace EmailSender.UI.Common
{
    /// <summary>
    /// 服务定位器 —— 统一管理所有服务实例（单例）
    /// UI 层通过此类获取所有 Core/Data 层服务
    /// </summary>
    public static class ServiceLocator
    {
        // ── Repositories ──────────────────────────────────────
        public static AppConfigRepository     AppConfigRepo     { get; private set; }
        public static EmailAddressRepository  EmailAddressRepo  { get; private set; }
        public static EmailTemplateRepository EmailTemplateRepo { get; private set; }
        public static SendTaskRepository      SendTaskRepo      { get; private set; }
        public static SendRecordRepository    SendRecordRepo    { get; private set; }
        public static SenderAccountRepository SenderAccountRepo { get; private set; }
        public static BlacklistRepository     BlacklistRepo     { get; private set; }

        // ── ApiClients ────────────────────────────────────────
        public static MeetbyApiClient      MeetbyApi      { get; private set; }
        public static SendCloudApiClient   SendCloudApi   { get; private set; }

        // ── Services ──────────────────────────────────────────
        public static AuthService          AuthService          { get; private set; }
        public static TemplateService      TemplateService      { get; private set; }
        public static EmailListService     EmailListService     { get; private set; }
        public static SendTaskService      SendTaskService      { get; private set; }
        public static SendEngineService    SendEngineService    { get; private set; }
        public static ResultFetchService   ResultFetchService   { get; private set; }
        public static BlacklistService     BlacklistService     { get; private set; }
        public static AiAnalysisService    AiAnalysisService    { get; private set; }

        // ── Helpers ───────────────────────────────────────────
        public static TemplateRenderer     TemplateRenderer     { get; private set; }

        public static void Initialize()
        {
            var meetbyUrl    = ConfigurationManager.AppSettings["MeetbyBaseUrl"];
            var sendCloudUrl = ConfigurationManager.AppSettings["SendCloudApiUrl"];

            // Repositories
            AppConfigRepo     = new AppConfigRepository();
            EmailAddressRepo  = new EmailAddressRepository();
            EmailTemplateRepo = new EmailTemplateRepository();
            SendTaskRepo      = new SendTaskRepository();
            SendRecordRepo    = new SendRecordRepository();
            SenderAccountRepo = new SenderAccountRepository();
            BlacklistRepo     = new BlacklistRepository();

            // 从数据库读取 SendCloud 配置
            var scApiUser = AppConfigRepo.Get("SendCloudApiUser") ?? "";
            var scApiKey  = AppConfigRepo.Get("SendCloudApiKey")  ?? "";

            // ApiClients
            MeetbyApi    = new MeetbyApiClient(meetbyUrl);
            SendCloudApi = new SendCloudApiClient(sendCloudUrl, scApiUser, scApiKey);

            // Helpers
            TemplateRenderer = new TemplateRenderer();

            // Services
            AiAnalysisService  = new AiAnalysisService(AppConfigRepo);
            AuthService        = new AuthService(MeetbyApi, AppConfigRepo);
            TemplateService    = new TemplateService(
                                     MeetbyApi, SendCloudApi,
                                     EmailTemplateRepo, AiAnalysisService);
            EmailListService   = new EmailListService(
                                     MeetbyApi, EmailAddressRepo,
                                     BlacklistRepo, AppConfigRepo);
            BlacklistService   = new BlacklistService(BlacklistRepo);
            SendEngineService  = new SendEngineService(
                                     SendCloudApi, SenderAccountRepo,
                                     SendRecordRepo, EmailAddressRepo,
                                     BlacklistRepo, TemplateRenderer);
            SendTaskService    = new SendTaskService(
                                     SendTaskRepo, EmailListService,
                                     SendEngineService);
            ResultFetchService = new ResultFetchService(
                                     SendCloudApi, SendRecordRepo,
                                     SendTaskRepo, BlacklistRepo);
        }

        /// <summary>重新初始化 SendCloud 客户端（配置变更后调用）</summary>
        public static void ReloadSendCloud()
        {
            var sendCloudUrl = ConfigurationManager.AppSettings["SendCloudApiUrl"];
            var scApiUser    = AppConfigRepo.Get("SendCloudApiUser") ?? "";
            var scApiKey     = AppConfigRepo.Get("SendCloudApiKey")  ?? "";
            SendCloudApi     = new SendCloudApiClient(sendCloudUrl, scApiUser, scApiKey);
        }
    }
}
"""

# ══════════════════════════════════════════════════════════════
# Common/UIHelper.cs
# ══════════════════════════════════════════════════════════════
files["Common/UIHelper.cs"] = """\
using System;
using System.Drawing;
using System.Windows.Forms;

namespace EmailSender.UI.Common
{
    /// <summary>通用 UI 工具类</summary>
    public static class UIHelper
    {
        // ── 消息框 ────────────────────────────────────────────
        public static void Info(string msg, string title = "提示")
            => MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static void Error(string msg, string title = "错误")
            => MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

        public static void Warn(string msg, string title = "警告")
            => MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public static bool Confirm(string msg, string title = "确认")
            => MessageBox.Show(msg, title, MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;

        // ── 跨线程 UI 更新 ────────────────────────────────────
        public static void InvokeIfNeeded(Control ctrl, Action action)
        {
            if (ctrl.InvokeRequired)
                ctrl.Invoke(action);
            else
                action();
        }

        // ── DataGridView 样式 ─────────────────────────────────
        public static void StyleGrid(DataGridView grid)
        {
            grid.BorderStyle                    = BorderStyle.None;
            grid.BackgroundColor                = Color.White;
            grid.GridColor                      = Color.FromArgb(230, 230, 230);
            grid.DefaultCellStyle.Font          = new Font("微软雅黑", 9f);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(46, 116, 181);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font  = new Font("微软雅黑", 9f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 73, 125);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersHeight            = 32;
            grid.RowTemplate.Height             = 28;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
            grid.EnableHeadersVisualStyles      = false;
            grid.SelectionMode                  = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect                    = false;
            grid.ReadOnly                       = true;
            grid.AllowUserToAddRows             = false;
            grid.AllowUserToDeleteRows          = false;
        }

        // ── 进度条窗口 ────────────────────────────────────────
        public static Form CreateProgressForm(string title, out ProgressBar bar, out Label label)
        {
            var form  = new Form
            {
                Text            = title,
                Size            = new Size(420, 130),
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false, MinimizeBox = false,
                ControlBox      = false,
            };
            label = new Label
            {
                Text     = "正在处理...",
                Location = new Point(15, 15),
                Size     = new Size(380, 20),
                Font     = new Font("微软雅黑", 9f),
            };
            bar = new ProgressBar
            {
                Location = new Point(15, 45),
                Size     = new Size(380, 22),
                Style    = ProgressBarStyle.Continuous,
            };
            form.Controls.Add(label);
            form.Controls.Add(bar);
            return form;
        }

        // ── 状态颜色 ──────────────────────────────────────────
        public static Color GetStatusColor(string status)
        {
            return status switch
            {
                "Running"   => Color.FromArgb(0, 120, 215),
                "Done"      => Color.FromArgb(16, 124, 16),
                "Failed"    => Color.FromArgb(196, 43, 28),
                "Paused"    => Color.FromArgb(200, 130, 0),
                "Pending"   => Color.FromArgb(100, 100, 100),
                _           => Color.Black
            };
        }
    }
}
"""

# ══════════════════════════════════════════════════════════════
# Forms/LoginForm.cs
# ══════════════════════════════════════════════════════════════
files["Forms/LoginForm.cs"] = """\
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
"""

files["Forms/LoginForm.Designer.cs"] = """\
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

            txtUsername = new TextBox { Location=new Point(120,82),  Size=new Size(190,24), PlaceholderText="meetby 账号" };
            txtPassword = new TextBox { Location=new Point(120,117), Size=new Size(190,24), UseSystemPasswordChar=true, PlaceholderText="密码" };
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
"""

# ══════════════════════════════════════════════════════════════
# Forms/MainForm.cs
# ══════════════════════════════════════════════════════════════
files["Forms/MainForm.cs"] = """\
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
"""

files["Forms/MainForm.Designer.cs"] = """\
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
"""

# ══════════════════════════════════════════════════════════════
# Forms/WizardForm.cs
# ══════════════════════════════════════════════════════════════
files["Forms/WizardForm.cs"] = """\
using System;
using System.Drawing;
using System.Windows.Forms;
using EmailSender.Models;
using EmailSender.UI.Common;
using EmailSender.UI.WizardSteps;

namespace EmailSender.UI.Forms
{
    /// <summary>新建任务向导（3步）</summary>
    public partial class WizardForm : Form
    {
        private int          _step = 1;
        private SendTask     _task = new SendTask();

        private Step1TemplatePanel _step1;
        private Step2ListPanel     _step2;
        private Step3SettingsPanel _step3;

        public WizardForm()
        {
            InitializeComponent();
            _step1 = new Step1TemplatePanel();
            _step2 = new Step2ListPanel();
            _step3 = new Step3SettingsPanel();
            ShowStep(1);
        }

        private void ShowStep(int step)
        {
            _step = step;
            panelStep.Controls.Clear();

            UserControl ctrl = step switch { 1 => _step1, 2 => _step2, _ => _step3 };
            ctrl.Dock = DockStyle.Fill;
            panelStep.Controls.Add(ctrl);

            lblStepTitle.Text = step switch {
                1 => "第 1 步 / 3  —  选择邮件模版",
                2 => "第 2 步 / 3  —  选择邮件列表",
                _ => "第 3 步 / 3  —  发送设置"
            };

            btnPrev.Enabled = step > 1;
            btnNext.Text    = step == 3 ? "✅ 创建任务" : "下一步 >";
            UpdateStepIndicator();
        }

        private void UpdateStepIndicator()
        {
            for (int i = 1; i <= 3; i++)
            {
                var lbl = Controls.Find($"lblStep{i}", true);
                if (lbl.Length > 0)
                    lbl[0].ForeColor = i == _step
                        ? Color.FromArgb(46, 116, 181) : Color.Gray;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (_step == 1)
            {
                if (_step1.SelectedTemplate == null)
                { UIHelper.Warn("请选择一个模版"); return; }
                _task.TemplateId   = _step1.SelectedTemplate.Id;
                _task.TemplateName = _step1.SelectedTemplate.Name;
                ShowStep(2);
            }
            else if (_step == 2)
            {
                if (_step2.SelectedListId == 0)
                { UIHelper.Warn("请选择一个邮件列表"); return; }
                _task.ListId   = _step2.SelectedListId;
                _task.ListName = _step2.SelectedListName;
                ShowStep(3);
            }
            else
            {
                // 第3步：创建任务
                _step3.ApplyTo(_task);
                if (_task.AccountId == 0)
                { UIHelper.Warn("请选择发送账户"); return; }

                try
                {
                    int taskId = ServiceLocator.SendTaskService.CreateTask(_task);
                    UIHelper.Info($"任务创建成功！任务ID：{taskId}");
                    DialogResult = System.Windows.Forms.DialogResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    UIHelper.Error($"创建任务失败：{ex.Message}");
                }
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
            => ShowStep(_step - 1);

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (UIHelper.Confirm("确定取消新建任务？"))
                Close();
        }
    }
}
"""

files["Forms/WizardForm.Designer.cs"] = """\
using System.Drawing;
using System.Windows.Forms;

namespace EmailSender.UI.Forms
{
    partial class WizardForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel  panelStep;
        private Panel  panelBottom;
        private Panel  panelTop;
        private Label  lblStepTitle;
        private Button btnPrev;
        private Button btnNext;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Text          = "新建发送任务";
            this.Size          = new Size(860, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize   = new Size(800, 580);
            this.Font          = new Font("微软雅黑", 9f);
            this.BackColor     = Color.White;

            panelTop = new Panel {
                Dock      = DockStyle.Top,
                Height    = 52,
                BackColor = Color.FromArgb(31, 73, 125),
            };
            lblStepTitle = new Label {
                Text      = "",
                Font      = new Font("微软雅黑", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                Dock      = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding   = new Padding(20, 0, 0, 0),
            };
            panelTop.Controls.Add(lblStepTitle);

            panelBottom = new Panel {
                Dock      = DockStyle.Bottom,
                Height    = 56,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding   = new Padding(15, 10, 15, 10),
            };
            btnCancel = new Button {
                Text      = "取消",
                Size      = new Size(90, 34),
                Location  = new Point(15, 11),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
            };
            btnPrev = new Button {
                Text      = "< 上一步",
                Size      = new Size(100, 34),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
            };
            btnNext = new Button {
                Text      = "下一步 >",
                Size      = new Size(120, 34),
                BackColor = Color.FromArgb(31, 73, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("微软雅黑", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
            };
            btnNext.FlatAppearance.BorderSize   = 0;
            btnCancel.Click += btnCancel_Click;
            btnPrev.Click   += btnPrev_Click;
            btnNext.Click   += btnNext_Click;

            var flowRight = new FlowLayoutPanel {
                Dock          = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize      = true,
                Padding       = new Padding(0, 8, 0, 0),
            };
            flowRight.Controls.Add(btnNext);
            flowRight.Controls.Add(btnPrev);
            panelBottom.Controls.Add(btnCancel);
            panelBottom.Controls.Add(flowRight);

            panelStep = new Panel {
                Dock    = DockStyle.Fill,
                Padding = new Padding(15),
            };

            this.Controls.Add(panelStep);
            this.Controls.Add(panelBottom);
            this.Controls.Add(panelTop);
        }
    }
}
"""

# ══════════════════════════════════════════════════════════════
# Controls/SendTaskControl.cs
# ══════════════════════════════════════════════════════════════
files["Controls/SendTaskControl.cs"] = """\
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using EmailSender.Models;
using EmailSender.UI.Common;
using EmailSender.UI.Forms;

namespace EmailSender.UI.Controls
{
    public partial class SendTaskControl : UserControl
    {
        public SendTaskControl()
        {
            InitializeComponent();
            UIHelper.StyleGrid(dgvTasks);
            LoadTasks();
        }

        private void LoadTasks()
        {
            var tasks = ServiceLocator.SendTaskService.GetAll();
            dgvTasks.DataSource = null;
            dgvTasks.DataSource = tasks;
            SetupColumns();
        }

        private void SetupColumns()
        {
            if (dgvTasks.Columns.Count == 0) return;
            dgvTasks.Columns["Id"].Visible            = false;
            dgvTasks.Columns["FilterConfig"].Visible  = false;
            dgvTasks.Columns["TemplateId"].Visible    = false;
            dgvTasks.Columns["ListId"].Visible        = false;
            dgvTasks.Columns["AccountId"].Visible     = false;

            if (dgvTasks.Columns["Name"]         != null) dgvTasks.Columns["Name"].HeaderText         = "任务名称";
            if (dgvTasks.Columns["TemplateName"]  != null) dgvTasks.Columns["TemplateName"].HeaderText  = "模版";
            if (dgvTasks.Columns["ListName"]      != null) dgvTasks.Columns["ListName"].HeaderText      = "列表";
            if (dgvTasks.Columns["Status"]        != null) dgvTasks.Columns["Status"].HeaderText        = "状态";
            if (dgvTasks.Columns["TotalCount"]    != null) dgvTasks.Columns["TotalCount"].HeaderText    = "总数";
            if (dgvTasks.Columns["SuccessCount"]  != null) dgvTasks.Columns["SuccessCount"].HeaderText  = "成功";
            if (dgvTasks.Columns["FailCount"]     != null) dgvTasks.Columns["FailCount"].HeaderText     = "失败";
            if (dgvTasks.Columns["OpenCount"]     != null) dgvTasks.Columns["OpenCount"].HeaderText     = "打开";
            if (dgvTasks.Columns["ScheduledAt"]   != null) dgvTasks.Columns["ScheduledAt"].HeaderText   = "计划时间";
            if (dgvTasks.Columns["CreatedAt"]     != null) dgvTasks.Columns["CreatedAt"].HeaderText     = "创建时间";
        }

        private void btnNewTask_Click(object sender, EventArgs e)
        {
            var wizard = new WizardForm();
            if (wizard.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                LoadTasks();
        }

        private async void btnStartTask_Click(object sender, EventArgs e)
        {
            if (dgvTasks.CurrentRow?.DataBoundItem is not SendTask task) return;
            if (!UIHelper.Confirm($"确定启动任务「{task.Name}」？")) return;

            btnStartTask.Enabled = false;
            var progressForm = UIHelper.CreateProgressForm(
                "发送中...", out var bar, out var lbl);
            progressForm.Show(this);

            var progress = new Progress<SendProgressInfo>(info =>
            {
                UIHelper.InvokeIfNeeded(this, () =>
                {
                    bar.Maximum = info.Total;
                    bar.Value   = Math.Min(info.Sent, info.Total);
                    lbl.Text    = $"已发 {info.Sent}/{info.Total}  成功:{info.Success}  失败:{info.Failed}";
                });
            });

            try
            {
                await ServiceLocator.SendTaskService.StartTaskAsync(task.Id, progress);
                UIHelper.Info("任务执行完成！");
            }
            catch (Exception ex)
            {
                UIHelper.Error($"任务执行失败：{ex.Message}");
            }
            finally
            {
                progressForm.Close();
                btnStartTask.Enabled = true;
                LoadTasks();
            }
        }

        private async void btnFetchResult_Click(object sender, EventArgs e)
        {
            if (dgvTasks.CurrentRow?.DataBoundItem is not SendTask task) return;

            btnFetchResult.Enabled = false;
            lblStatus.Text         = "正在拉取结果...";
            try
            {
                int updated = await ServiceLocator.ResultFetchService
                    .FetchPendingResultsAsync(task.Id);
                lblStatus.Text = $"已更新 {updated} 条记录";
                LoadTasks();
            }
            catch (Exception ex)
            {
                UIHelper.Error($"拉取结果失败：{ex.Message}");
                lblStatus.Text = "";
            }
            finally
            {
                btnFetchResult.Enabled = true;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e) => LoadTasks();

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvTasks.CurrentRow?.DataBoundItem is not SendTask task) return;
            if (!UIHelper.Confirm($"确定删除任务「{task.Name}」？")) return;
            ServiceLocator.SendTaskService.Delete(task.Id);
            LoadTasks();
        }
    }
}
"""

files["Controls/SendTaskControl.Designer.cs"] = """\
using System.Drawing;
using System.Windows.Forms;

namespace EmailSender.UI.Controls
{
    partial class SendTaskControl
    {
        private System.ComponentModel.IContainer components = null;
        private DataGridView dgvTasks;
        private Panel        panelToolbar;
        private Button       btnNewTask;
        private Button       btnStartTask;
        private Button       btnFetchResult;
        private Button       btnRefresh;
        private Button       btnDelete;
        private Label        lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;

            panelToolbar = new Panel { Dock=DockStyle.Top, Height=46, BackColor=Color.White, Padding=new Padding(5,8,5,0) };

            Button MakeBtn(string text, Color? bg=null) {
                var b = new Button {
                    Text=text, Height=30, AutoSize=true, Padding=new Padding(10,0,10,0),
                    FlatStyle=FlatStyle.Flat, Cursor=Cursors.Hand,
                    Font=new Font("微软雅黑",9f),
                };
                if (bg.HasValue) { b.BackColor=bg.Value; b.ForeColor=Color.White; b.FlatAppearance.BorderSize=0; }
                return b;
            }

            btnNewTask     = MakeBtn("＋ 新建任务",  Color.FromArgb(31,73,125));
            btnStartTask   = MakeBtn("▶ 启动任务",   Color.FromArgb(16,124,16));
            btnFetchResult = MakeBtn("🔄 刷新结果",  Color.FromArgb(0,120,215));
            btnRefresh     = MakeBtn("↺ 刷新列表");
            btnDelete      = MakeBtn("✕ 删除",       Color.FromArgb(196,43,28));

            btnNewTask.Click     += btnNewTask_Click;
            btnStartTask.Click   += btnStartTask_Click;
            btnFetchResult.Click += btnFetchResult_Click;
            btnRefresh.Click     += btnRefresh_Click;
            btnDelete.Click      += btnDelete_Click;

            lblStatus = new Label { Text="", ForeColor=Color.Gray, AutoSize=true, Padding=new Padding(8,8,0,0) };

            var flow = new FlowLayoutPanel { Dock=DockStyle.Fill, FlowDirection=FlowDirection.LeftToRight, WrapContents=false };
            flow.Controls.AddRange(new Control[]{ btnNewTask,btnStartTask,btnFetchResult,btnRefresh,btnDelete,lblStatus });
            panelToolbar.Controls.Add(flow);

            dgvTasks = new DataGridView { Dock=DockStyle.Fill };

            this.Controls.Add(dgvTasks);
            this.Controls.Add(panelToolbar);
        }
    }
}
"""

# ══════════════════════════════════════════════════════════════
# Controls/TemplateControl.cs
# ══════════════════════════════════════════════════════════════
files["Controls/TemplateControl.cs"] = """\
using System;
using System.Windows.Forms;
using EmailSender.Models;
using EmailSender.UI.Common;

namespace EmailSender.UI.Controls
{
    public partial class TemplateControl : UserControl
    {
        public TemplateControl()
        {
            InitializeComponent();
            UIHelper.StyleGrid(dgvTemplates);
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            dgvTemplates.DataSource = null;
            dgvTemplates.DataSource = ServiceLocator.TemplateService.GetAll();
        }

        private async void btnSyncFromMeetby_Click(object sender, EventArgs e)
        {
            btnSyncFromMeetby.Enabled = false;
            lblStatus.Text = "正在从 meetby 同步模版...";
            try
            {
                var list = await ServiceLocator.TemplateService.SyncFromMeetbyAsync();
                lblStatus.Text = $"同步完成，共 {list.Count} 个模版";
                LoadTemplates();
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); lblStatus.Text = ""; }
            finally { btnSyncFromMeetby.Enabled = true; }
        }

        private async void btnSyncToSendCloud_Click(object sender, EventArgs e)
        {
            if (dgvTemplates.CurrentRow?.DataBoundItem is not EmailTemplate t) return;
            btnSyncToSendCloud.Enabled = false;
            lblStatus.Text = "正在推送到 SendCloud...";
            try
            {
                await ServiceLocator.TemplateService.SyncToSendCloudAsync(t.Id);
                lblStatus.Text = "推送成功！";
                LoadTemplates();
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); lblStatus.Text = ""; }
            finally { btnSyncToSendCloud.Enabled = true; }
        }

        private async void btnAiAnalyze_Click(object sender, EventArgs e)
        {
            if (dgvTemplates.CurrentRow?.DataBoundItem is not EmailTemplate t) return;
            btnAiAnalyze.Enabled = false;
            lblStatus.Text = "AI 分析中，请稍候...";
            try
            {
                await ServiceLocator.TemplateService.AnalyzeWithAiAsync(t.Id);
                var updated = ServiceLocator.TemplateService.GetById(t.Id);
                lblAiScore.Text = $"AI 评分：{updated.AiScore} / 100";
                txtAiResult.Text = $"问题：{updated.AiIssues}\n\n建议：{updated.AiSuggestions}";
                lblStatus.Text = "AI 分析完成";
                LoadTemplates();
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); lblStatus.Text = ""; }
            finally { btnAiAnalyze.Enabled = true; }
        }

        private void dgvTemplates_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvTemplates.CurrentRow?.DataBoundItem is not EmailTemplate t) return;
            webPreview.DocumentText = t.HtmlBody ?? "<p>（无内容）</p>";
            lblAiScore.Text = t.AiScore.HasValue
                ? $"AI 评分：{t.AiScore} / 100" : "AI 评分：未分析";
            txtAiResult.Text = t.AiSuggestions ?? "";
        }
    }
}
"""

# ══════════════════════════════════════════════════════════════
# Controls/EmailListControl.Designer.cs
# ══════════════════════════════════════════════════════════════
files["Controls/EmailListControl.Designer.cs"] = """\
using System.Drawing;
using System.Windows.Forms;

namespace EmailSender.UI.Controls
{
    partial class EmailListControl
    {
        private System.ComponentModel.IContainer components = null;
        private Panel        panelToolbar;
        private ComboBox     cmbList;
        private Button       btnDownload;
        private Button       btnRefreshStats;
        private Label        lblStatus;
        private SplitContainer splitMain;
        private DataGridView dgvDomain;
        private Panel        panelStats;
        private Label        lblTotal;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;

            panelToolbar = new Panel {
                Dock=DockStyle.Top, Height=46,
                BackColor=Color.White, Padding=new Padding(5,8,5,0)
            };

            cmbList = new ComboBox {
                Width=260, DropDownStyle=ComboBoxStyle.DropDownList,
                Font=new Font("微软雅黑",9f)
            };

            Button MakeBtn(string text, Color? bg=null) {
                var b = new Button {
                    Text=text, Height=30, AutoSize=true,
                    Padding=new Padding(10,0,10,0),
                    FlatStyle=FlatStyle.Flat, Cursor=Cursors.Hand,
                    Font=new Font("微软雅黑",9f)
                };
                if (bg.HasValue) {
                    b.BackColor=bg.Value; b.ForeColor=Color.White;
                    b.FlatAppearance.BorderSize=0;
                }
                return b;
            }

            btnDownload     = MakeBtn("⬇ 下载成员", Color.FromArgb(31,73,125));
            btnRefreshStats = MakeBtn("↺ 刷新统计");
            lblStatus       = new Label {
                Text="", ForeColor=Color.Gray,
                AutoSize=true, Padding=new Padding(8,8,0,0)
            };

            btnDownload.Click     += btnDownload_Click;
            btnRefreshStats.Click += btnRefreshStats_Click;

            var flow = new FlowLayoutPanel {
                Dock=DockStyle.Fill, FlowDirection=FlowDirection.LeftToRight,
                WrapContents=false
            };
            flow.Controls.AddRange(new Control[]{
                cmbList, btnDownload, btnRefreshStats, lblStatus
            });
            panelToolbar.Controls.Add(flow);

            // 统计面板
            panelStats = new Panel {
                Dock=DockStyle.Top, Height=36,
                BackColor=Color.FromArgb(240,248,255),
                Padding=new Padding(12,8,0,0)
            };
            lblTotal = new Label {
                Text="有效地址：0 个",
                Font=new Font("微软雅黑",10f,FontStyle.Bold),
                ForeColor=Color.FromArgb(31,73,125),
                AutoSize=true
            };
            panelStats.Controls.Add(lblTotal);

            // 域名分布表格
            dgvDomain = new DataGridView { Dock=DockStyle.Fill };
            UIHelper.StyleGrid(dgvDomain);

            this.Controls.Add(dgvDomain);
            this.Controls.Add(panelStats);
            this.Controls.Add(panelToolbar);
        }
    }
}
"""

# ══════════════════════════════════════════════════════════════
# Controls/BlacklistControl.cs
# ══════════════════════════════════════════════════════════════
files["Controls/BlacklistControl.cs"] = """\
using System;
using System.IO;
using System.Windows.Forms;
using EmailSender.Models;
using EmailSender.UI.Common;

namespace EmailSender.UI.Controls
{
    public partial class BlacklistControl : UserControl
    {
        public BlacklistControl()
        {
            InitializeComponent();
            UIHelper.StyleGrid(dgvBlacklist);
            LoadData();
        }

        private void LoadData()
        {
            dgvBlacklist.DataSource = null;
            dgvBlacklist.DataSource = ServiceLocator.BlacklistService.GetAll();
            lblCount.Text = $"共 {ServiceLocator.BlacklistService.GetCount()} 条";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var email = txtEmail.Text.Trim();
            if (string.IsNullOrEmpty(email))
            { UIHelper.Warn("请输入邮件地址"); return; }

            ServiceLocator.BlacklistService.AddManual(email, txtReason.Text.Trim());
            txtEmail.Clear();
            txtReason.Clear();
            LoadData();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvBlacklist.CurrentRow?.DataBoundItem is not Blacklist b) return;
            if (!UIHelper.Confirm($"确定从黑名单移除 {b.Email}？")) return;
            ServiceLocator.BlacklistService.Remove(b.Email);
            LoadData();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog {
                Title="选择黑名单文件（每行一个邮件地址）",
                Filter="文本文件|*.txt|所有文件|*.*"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                int count = ServiceLocator.BlacklistService.ImportFromFile(dlg.FileName);
                UIHelper.Info($"导入成功，共添加 {count} 条");
                LoadData();
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog {
                Title="导出黑名单",
                Filter="文本文件|*.txt",
                FileName="blacklist_export.txt"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                ServiceLocator.BlacklistService.ExportToFile(dlg.FileName);
                UIHelper.Info("导出成功！");
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); }
        }

        private void btnRefresh_Click(object sender, EventArgs e) => LoadData();

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            var kw = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(kw)) { LoadData(); return; }
            var all = ServiceLocator.BlacklistService.GetAll();
            dgvBlacklist.DataSource = all.FindAll(b =>
                b.Email.ToLower().Contains(kw));
        }
    }
}
"""

files["Controls/BlacklistControl.Designer.cs"] = """\
using System.Drawing;
using System.Windows.Forms;

namespace EmailSender.UI.Controls
{
    partial class BlacklistControl
    {
        private System.ComponentModel.IContainer components = null;
        private Panel        panelTop;
        private Panel        panelAdd;
        private DataGridView dgvBlacklist;
        private Label        lblCount;
        private TextBox      txtEmail;
        private TextBox      txtReason;
        private TextBox      txtSearch;
        private Button       btnAdd;
        private Button       btnDelete;
        private Button       btnImport;
        private Button       btnExport;
        private Button       btnRefresh;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;

            // ── 顶部工具栏 ──────────────────────────────────────
            panelTop = new Panel {
                Dock=DockStyle.Top, Height=46,
                BackColor=Color.White, Padding=new Padding(5,8,5,0)
            };

            Button MakeBtn(string text, Color? bg=null) {
                var b = new Button {
                    Text=text, Height=30, AutoSize=true,
                    Padding=new Padding(10,0,10,0),
                    FlatStyle=FlatStyle.Flat, Cursor=Cursors.Hand,
                    Font=new Font("微软雅黑",9f)
                };
                if (bg.HasValue) {
                    b.BackColor=bg.Value; b.ForeColor=Color.White;
                    b.FlatAppearance.BorderSize=0;
                }
                return b;
            }

            btnDelete  = MakeBtn("✕ 移除",    Color.FromArgb(196,43,28));
            btnImport  = MakeBtn("⬆ 批量导入", Color.FromArgb(0,120,215));
            btnExport  = MakeBtn("⬇ 导出");
            btnRefresh = MakeBtn("↺ 刷新");

            txtSearch = new TextBox {
                Width=200, Height=28,
                Font=new Font("微软雅黑",9f),
                PlaceholderText="搜索邮件地址..."
            };

            lblCount = new Label {
                Text="", ForeColor=Color.Gray,
                AutoSize=true, Padding=new Padding(8,8,0,0)
            };

            btnDelete.Click  += btnDelete_Click;
            btnImport.Click  += btnImport_Click;
            btnExport.Click  += btnExport_Click;
            btnRefresh.Click += btnRefresh_Click;
            txtSearch.TextChanged += txtSearch_TextChanged;

            var flowTop = new FlowLayoutPanel {
                Dock=DockStyle.Fill, FlowDirection=FlowDirection.LeftToRight,
                WrapContents=false
            };
            flowTop.Controls.AddRange(new Control[]{
                txtSearch, btnDelete, btnImport, btnExport, btnRefresh, lblCount
            });
            panelTop.Controls.Add(flowTop);

            // ── 手动添加区 ──────────────────────────────────────
            panelAdd = new Panel {
                Dock=DockStyle.Top, Height=46,
                BackColor=Color.FromArgb(250,250,252),
                Padding=new Padding(5,8,5,0)
            };

            txtEmail = new TextBox {
                Width=240, Height=28,
                Font=new Font("微软雅黑",9f),
                PlaceholderText="输入邮件地址"
            };
            txtReason = new TextBox {
                Width=200, Height=28,
                Font=new Font("微软雅黑",9f),
                PlaceholderText="备注原因（可选）"
            };
            btnAdd = MakeBtn("＋ 手动添加", Color.FromArgb(31,73,125));
            btnAdd.Click += btnAdd_Click;

            var flowAdd = new FlowLayoutPanel {
                Dock=DockStyle.Fill, FlowDirection=FlowDirection.LeftToRight,
                WrapContents=false
            };
            flowAdd.Controls.AddRange(new Control[]{ txtEmail, txtReason, btnAdd });
            panelAdd.Controls.Add(flowAdd);

            // ── 数据表格 ────────────────────────────────────────
            dgvBlacklist = new DataGridView { Dock=DockStyle.Fill };

            this.Controls.Add(dgvBlacklist);
            this.Controls.Add(panelAdd);
            this.Controls.Add(panelTop);
        }
    }
}
"""

# ══════════════════════════════════════════════════════════════
# Controls/SettingsControl.cs
# ══════════════════════════════════════════════════════════════
files["Controls/SettingsControl.cs"] = """\
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
                ServiceLocator.OAuthHelper.StartGmailAuth(acc.Id);
                UIHelper.Info("浏览器已打开，请完成授权后回到此窗口");
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); }
        }

        private void btnOAuthHotmail_Click(object sender, EventArgs e)
        {
            if (dgvAccounts.CurrentRow?.DataBoundItem is not SenderAccount acc) return;
            try
            {
                ServiceLocator.OAuthHelper.StartHotmailAuth(acc.Id);
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
"""

files["Controls/SettingsControl.Designer.cs"] = """\
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
"""

# ══════════════════════════════════════════════════════════════
# WizardSteps/Step1TemplatePanel.cs
# ══════════════════════════════════════════════════════════════
files["WizardSteps/Step1TemplatePanel.cs"] = """\
using System;
using System.Drawing;
using System.Windows.Forms;
using EmailSender.Models;
using EmailSender.UI.Common;

namespace EmailSender.UI.WizardSteps
{
    /// <summary>向导第1步：选择邮件模版</summary>
    public partial class Step1TemplatePanel : UserControl
    {
        public EmailTemplate SelectedTemplate { get; private set; }

        public Step1TemplatePanel()
        {
            InitializeComponent();
            UIHelper.StyleGrid(dgvTemplates);
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            var list = ServiceLocator.TemplateService.GetAll();
            dgvTemplates.DataSource = list;
            SetupColumns();
        }

        private void SetupColumns()
        {
            if (dgvTemplates.Columns.Count == 0) return;
            foreach (DataGridViewColumn col in dgvTemplates.Columns)
                col.Visible = false;

            void Show(string name, string header, int width) {
                if (dgvTemplates.Columns[name] == null) return;
                dgvTemplates.Columns[name].Visible     = true;
                dgvTemplates.Columns[name].HeaderText  = header;
                dgvTemplates.Columns[name].Width       = width;
            }
            Show("Name",    "模版名称", 200);
            Show("Subject", "邮件主题", 280);
            Show("AiScore", "AI评分",    80);
            Show("SendCloudTemplateId", "SendCloud ID", 120);
        }

        private void dgvTemplates_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvTemplates.CurrentRow?.DataBoundItem is not EmailTemplate t) return;
            SelectedTemplate = t;
            lblSelected.Text = $"已选择：{t.Name}（主题：{t.Subject}）";
            lblSelected.ForeColor = Color.FromArgb(16,124,16);

            // 预览
            webPreview.DocumentText = t.HtmlBody ?? "<p>（无内容）</p>";

            // AI 评分提示
            if (t.AiScore.HasValue)
            {
                var color = t.AiScore >= 70 ? Color.FromArgb(16,124,16)
                          : t.AiScore >= 40 ? Color.FromArgb(200,130,0)
                          : Color.FromArgb(196,43,28);
                lblAiHint.ForeColor = color;
                lblAiHint.Text = $"AI 反垃圾评分：{t.AiScore} / 100";
            }
            else
            {
                lblAiHint.ForeColor = Color.Gray;
                lblAiHint.Text = "该模版尚未进行 AI 分析";
            }
        }

        private async void btnSyncTemplates_Click(object sender, EventArgs e)
        {
            btnSyncTemplates.Enabled = false;
            try
            {
                await ServiceLocator.TemplateService.SyncFromMeetbyAsync();
                LoadTemplates();
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); }
            finally { btnSyncTemplates.Enabled = true; }
        }
    }
}
"""

files["WizardSteps/Step1TemplatePanel.Designer.cs"] = """\
using System.Drawing;
using System.Windows.Forms;

namespace EmailSender.UI.WizardSteps
{
    partial class Step1TemplatePanel
    {
        private System.ComponentModel.IContainer components = null;
        private SplitContainer splitMain;
        private DataGridView   dgvTemplates;
        private Panel          panelTop;
        private Button         btnSyncTemplates;
        private Label          lblSelected;
        private Label          lblAiHint;
        private WebBrowser     webPreview;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;

            panelTop = new Panel {
                Dock=DockStyle.Top, Height=46,
                BackColor=Color.White, Padding=new Padding(5,8,5,0)
            };
            btnSyncTemplates = new Button {
                Text="⬇ 从meetby同步模版", Height=30, AutoSize=true,
                Padding=new Padding(12,0,12,0),
                BackColor=Color.FromArgb(31,73,125), ForeColor=Color.White,
                FlatStyle=FlatStyle.Flat, Cursor=Cursors.Hand,
                Font=new Font("微软雅黑",9f)
            };
            btnSyncTemplates.FlatAppearance.BorderSize = 0;
            btnSyncTemplates.Click += btnSyncTemplates_Click;

            lblSelected = new Label {
                Text="请从下方选择一个模版",
                ForeColor=Color.Gray, AutoSize=true,
                Padding=new Padding(12,8,0,0),
                Font=new Font("微软雅黑",9f)
            };

            var flow = new FlowLayoutPanel {
                Dock=DockStyle.Fill, FlowDirection=FlowDirection.LeftToRight,
                WrapContents=false
            };
            flow.Controls.AddRange(new Control[]{ btnSyncTemplates, lblSelected });
            panelTop.Controls.Add(flow);

            dgvTemplates = new DataGridView { Dock=DockStyle.Fill };
            dgvTemplates.SelectionChanged += dgvTemplates_SelectionChanged;

            lblAiHint = new Label {
                Dock=DockStyle.Bottom, Height=24,
                Text="", Font=new Font("微软雅黑",9f),
                Padding=new Padding(5,4,0,0)
            };

            webPreview = new WebBrowser { Dock=DockStyle.Fill };

            splitMain = new SplitContainer {
                Dock=DockStyle.Fill, SplitterDistance=340,
                Orientation=Orientation.Vertical
            };
            splitMain.Panel1.Controls.Add(dgvTemplates);
            splitMain.Panel1.Controls.Add(lblAiHint);
            splitMain.Panel2.Controls.Add(webPreview);

            this.Controls.Add(splitMain);
            this.Controls.Add(panelTop);
        }
    }
}
"""

# ══════════════════════════════════════════════════════════════
# WizardSteps/Step2ListPanel.cs
# ══════════════════════════════════════════════════════════════
files["WizardSteps/Step2ListPanel.cs"] = """\
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EmailSender.UI.Common;

namespace EmailSender.UI.WizardSteps
{
    /// <summary>向导第2步：选择邮件列表 + 数据清洗设置</summary>
    public partial class Step2ListPanel : UserControl
    {
        public int    SelectedListId   { get; private set; }
        public string SelectedListName { get; private set; }

        public Step2ListPanel()
        {
            InitializeComponent();
            LoadLists();
        }

        private async void LoadLists()
        {
            try
            {
                var lists = await ServiceLocator.MeetbyApi.GetEmailListsAsync();
                dgvLists.DataSource = lists.Select(l => new {
                    列表ID   = l.ListId,
                    列表名称 = l.ListName,
                    成员数量 = l.MemberCount
                }).ToList();
            }
            catch (Exception ex) { UIHelper.Error($"获取列表失败：{ex.Message}"); }
        }

        private void dgvLists_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvLists.CurrentRow == null) return;
            SelectedListId   = (int)dgvLists.CurrentRow.Cells["列表ID"].Value;
            SelectedListName = dgvLists.CurrentRow.Cells["列表名称"].Value?.ToString();

            // 刷新本地统计
            int localCount = ServiceLocator.EmailListService.GetCount(SelectedListId);
            lblLocalCount.Text = localCount > 0
                ? $"本地已缓存：{localCount} 个地址"
                : "本地暂无缓存，请先下载";
            lblLocalCount.ForeColor = localCount > 0
                ? Color.FromArgb(16,124,16) : Color.FromArgb(200,130,0);
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            if (SelectedListId == 0) { UIHelper.Warn("请先选择列表"); return; }
            btnDownload.Enabled = false;
            lblDownloadStatus.Text = "下载中...";

            var progress = new Progress<(int current, int total)>(p =>
                UIHelper.InvokeIfNeeded(this, () =>
                    lblDownloadStatus.Text = $"下载中 {p.current}/{p.total}"));
            try
            {
                int count = await ServiceLocator.EmailListService
                    .DownloadAndSaveAsync(SelectedListId, SelectedListName, progress);
                lblDownloadStatus.Text = $"下载完成，共 {count} 个";
                lblLocalCount.Text     = $"本地已缓存：{count} 个地址";
                lblLocalCount.ForeColor = Color.FromArgb(16,124,16);
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); lblDownloadStatus.Text=""; }
            finally { btnDownload.Enabled = true; }
        }

        /// <summary>向导第3步读取清洗参数</summary>
        public (int maxFail, bool excludeThisWeek) GetFilterOptions()
            => ((int)numMaxFail.Value, chkExcludeThisWeek.Checked);
    }
}
"""

files["WizardSteps/Step2ListPanel.Designer.cs"] = """\
using System.Drawing;
using System.Windows.Forms;

namespace EmailSender.UI.WizardSteps
{
    partial class Step2ListPanel
    {
        private System.ComponentModel.IContainer components = null;
        private SplitContainer splitMain;
        private DataGridView   dgvLists;
        private Panel          panelRight;
        private Panel          panelTop;
        private Button         btnDownload;
        private Label          lblLocalCount;
        private Label          lblDownloadStatus;
        private GroupBox       grpFilter;
        private NumericUpDown  numMaxFail;
        private CheckBox       chkExcludeThisWeek;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;

            panelTop = new Panel {
                Dock=DockStyle.Top, Height=46,
                BackColor=Color.White, Padding=new Padding(5,8,5,0)
            };
            btnDownload = new Button {
                Text="⬇ 下载/更新成员", Height=30, AutoSize=true,
                Padding=new Padding(12,0,12,0),
                BackColor=Color.FromArgb(31,73,125), ForeColor=Color.White,
                FlatStyle=FlatStyle.Flat, Cursor=Cursors.Hand,
                Font=new Font("微软雅黑",9f)
            };
            btnDownload.FlatAppearance.BorderSize = 0;
            btnDownload.Click += btnDownload_Click;

            lblDownloadStatus = new Label {
                Text="", ForeColor=Color.Gray, AutoSize=true,
                Padding=new Padding(8,8,0,0)
            };
            var flow = new FlowLayoutPanel {
                Dock=DockStyle.Fill, FlowDirection=FlowDirection.LeftToRight,
                WrapContents=false
            };
            flow.Controls.AddRange(new Control[]{ btnDownload, lblDownloadStatus });
            panelTop.Controls.Add(flow);

            dgvLists = new DataGridView { Dock=DockStyle.Fill };
            UIHelper.StyleGrid(dgvLists);
            dgvLists.SelectionChanged += dgvLists_SelectionChanged;

            // 右侧面板
            lblLocalCount = new Label {
                Text="请选择左侧列表",
                Dock=DockStyle.Top, Height=32,
                Font=new Font("微软雅黑",10f,FontStyle.Bold),
                ForeColor=Color.Gray,
                Padding=new Padding(8,8,0,0)
            };

            // 清洗选项
            grpFilter = new GroupBox {
                Text="数据清洗选项",
                Dock=DockStyle.Top, Height=110,
                Font=new Font("微软雅黑",9f),
                Padding=new Padding(10,8,10,0)
            };

            numMaxFail = new NumericUpDown {
                Minimum=1, Maximum=99, Value=3,
                Width=60, Font=new Font("微软雅黑",9f)
            };
            chkExcludeThisWeek = new CheckBox {
                Text="排除本周已发送地址",
                AutoSize=true, Checked=true,
                Font=new Font("微软雅黑",9f)
            };

            var tbl = new TableLayoutPanel {
                Dock=DockStyle.Fill, ColumnCount=2, RowCount=2
            };
            tbl.Controls.Add(new Label { Text="最大失败次数：", AutoSize=true, Padding=new Padding(0,5,5,0) }, 0,0);
            tbl.Controls.Add(numMaxFail,         1,0);
            tbl.Controls.Add(chkExcludeThisWeek, 0,1);
            tbl.SetColumnSpan(chkExcludeThisWeek, 2);
            grpFilter.Controls.Add(tbl);

            panelRight = new Panel { Dock=DockStyle.Fill };
            panelRight.Controls.Add(grpFilter);
            panelRight.Controls.Add(lblLocalCount);

            splitMain = new SplitContainer {
                Dock=DockStyle.Fill, SplitterDistance=420,
                Orientation=Orientation.Vertical
            };
            splitMain.Panel1.Controls.Add(dgvLists);
            splitMain.Panel2.Controls.Add(panelRight);

            this.Controls.Add(splitMain);
            this.Controls.Add(panelTop);
        }
    }
}
"""

# ══════════════════════════════════════════════════════════════
# WizardSteps/Step3SettingsPanel.cs
# ══════════════════════════════════════════════════════════════
files["WizardSteps/Step3SettingsPanel.cs"] = """\
using System;
using System.Windows.Forms;
using EmailSender.Models;
using EmailSender.UI.Common;

namespace EmailSender.UI.WizardSteps
{
    /// <summary>向导第3步：发送参数设置</summary>
    public partial class Step3SettingsPanel : UserControl
    {
        public Step3SettingsPanel()
        {
            InitializeComponent();
            LoadAccounts();
        }

        private void LoadAccounts()
        {
            cmbAccount.DataSource    = ServiceLocator.SenderAccountRepo.GetAll();
            cmbAccount.DisplayMember = "Name";
            cmbAccount.ValueMember   = "Id";
        }

        private void cmbChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            // SendCloud 通道不需要选账户（使用 API Key）
            cmbAccount.Enabled = cmbChannel.SelectedIndex != 0;
        }

        /// <summary>将界面参数写入 SendTask 对象</summary>
        public void ApplyTo(SendTask task)
        {
            task.Name           = txtTaskName.Text.Trim();
            task.Channel        = (SendChannel)cmbChannel.SelectedIndex;
            task.AccountId      = cmbAccount.SelectedValue is int id ? id : 0;
            task.ThreadCount    = (int)numThreads.Value;
            task.IntervalSeconds= (int)numInterval.Value;
            task.RetryMax       = (int)numRetry.Value;
            task.BatchSize      = (int)numBatch.Value;

            if (chkSchedule.Checked && dtpSchedule.Value > DateTime.Now)
                task.ScheduledAt = dtpSchedule.Value;

            if (string.IsNullOrEmpty(task.Name))
                task.Name = $"任务_{DateTime.Now:yyyyMMdd_HHmm}";
        }
    }
}
"""

files["WizardSteps/Step3SettingsPanel.Designer.cs"] = """\
using System.Drawing;
using System.Windows.Forms;

namespace EmailSender.UI.WizardSteps
{
    partial class Step3SettingsPanel
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox       txtTaskName;
        private ComboBox      cmbChannel;
        private ComboBox      cmbAccount;
        private NumericUpDown numThreads;
        private NumericUpDown numInterval;
        private NumericUpDown numRetry;
        private NumericUpDown numBatch;
        private CheckBox      chkSchedule;
        private DateTimePicker dtpSchedule;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private Label MakeLbl(string text) =>
            new Label { Text=text, AutoSize=true,
                        Font=new Font("微软雅黑",9f),
                        Padding=new Padding(0,7,10,0) };

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.Font = new Font("微软雅黑",9f);

            txtTaskName = new TextBox {
                Width=280, Height=26,
                Font=new Font("微软雅黑",9f),
                PlaceholderText="留空则自动生成"
            };

            cmbChannel = new ComboBox {
                Width=160, DropDownStyle=ComboBoxStyle.DropDownList,
                Font=new Font("微软雅黑",9f)
            };
            cmbChannel.Items.AddRange(new object[]{
                "SendCloud","Gmail","Hotmail","SMTP"
            });
            cmbChannel.SelectedIndex = 0;
            cmbChannel.SelectedIndexChanged += cmbChannel_SelectedIndexChanged;

            cmbAccount = new ComboBox {
                Width=200, DropDownStyle=ComboBoxStyle.DropDownList,
                Font=new Font("微软雅黑",9f)
            };

            NumericUpDown MakeNum(int min, int max, int val, int width=80) =>
                new NumericUpDown {
                    Minimum=min, Maximum=max, Value=val,
                    Width=width, Font=new Font("微软雅黑",9f)
                };

            numThreads  = MakeNum(1,  20,  3);
            numInterval = MakeNum(0, 300,  5);
            numRetry    = MakeNum(1,  10,  3);
            numBatch    = MakeNum(1, 500, 50);

            chkSchedule = new CheckBox {
                Text="定时发送", AutoSize=true,
                Font=new Font("微软雅黑",9f)
            };
            dtpSchedule = new DateTimePicker {
                Format=DateTimePickerFormat.Custom,
                CustomFormat="yyyy-MM-dd HH:mm",
                Width=180, Enabled=false,
                Font=new Font("微软雅黑",9f)
            };
            chkSchedule.CheckedChanged += (s,e) =>
                dtpSchedule.Enabled = chkSchedule.Checked;

            // 表单布局
            var tbl = new TableLayoutPanel {
                Dock=DockStyle.Top, AutoSize=true,
                ColumnCount=2, Padding=new Padding(30,20,30,0)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  100));
            for (int i=0;i<8;i++)
                tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            tbl.Controls.Add(MakeLbl("任务名称："),       0,0); tbl.Controls.Add(txtTaskName,  1,0);
            tbl.Controls.Add(MakeLbl("发送通道："),       0,1); tbl.Controls.Add(cmbChannel,   1,1);
            tbl.Controls.Add(MakeLbl("发送账户："),       0,2); tbl.Controls.Add(cmbAccount,   1,2);
            tbl.Controls.Add(MakeLbl("并发线程数："),     0,3); tbl.Controls.Add(numThreads,   1,3);
            tbl.Controls.Add(MakeLbl("发送间隔（秒）："), 0,4); tbl.Controls.Add(numInterval,  1,4);
            tbl.Controls.Add(MakeLbl("最大重试次数："),   0,5); tbl.Controls.Add(numRetry,     1,5);
            tbl.Controls.Add(MakeLbl("批次大小："),       0,6); tbl.Controls.Add(numBatch,     1,6);

            var flowSchedule = new FlowLayoutPanel {
                AutoSize=true, FlowDirection=FlowDirection.LeftToRight
            };
            flowSchedule.Controls.AddRange(new Control[]{ chkSchedule, dtpSchedule });
            tbl.Controls.Add(MakeLbl("定时发送："), 0,7);
            tbl.Controls.Add(flowSchedule,          1,7);

            // 提示标签
            var lblHint = new Label {
                Text="💡 提示：并发线程数建议 3-5，间隔建议 3-10 秒，避免触发发送频率限制",
                Dock=DockStyle.Top, Height=32,
                ForeColor=Color.FromArgb(0,120,215),
                Font=new Font("微软雅黑",8.5f),
                Padding=new Padding(30,8,0,0)
            };

            this.Controls.Add(lblHint);
            this.Controls.Add(tbl);
        }
    }
}
"""

# ══════════════════════════════════════════════════════════════
# 写入所有文件到磁盘
# ══════════════════════════════════════════════════════════════
for filename, content in files.items():
    full_path = os.path.join(base, filename)
    os.makedirs(os.path.dirname(full_path), exist_ok=True)
    with open(full_path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"  ✅ 已生成: {full_path}")

print(f"\n🎉 EmailSender.UI 生成完毕！共 {len(files)} 个文件")
print(f"📁 输出目录: ./{base}/")
print()
print("文件清单：")
for fname in files:
    print(f"  📄 {fname}")