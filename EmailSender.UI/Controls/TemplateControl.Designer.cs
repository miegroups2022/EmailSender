using System.Drawing;
using System.Windows.Forms;

namespace EmailSender.UI.Controls
{
    partial class TemplateControl
    {
        private System.ComponentModel.IContainer components = null;
        private SplitContainer  splitMain;
        private DataGridView    dgvTemplates;
        private Panel           panelToolbar;
        private Button          btnSyncFromMeetby;
        private Button          btnSyncToSendCloud;
        private Button          btnAiAnalyze;
        private Label           lblStatus;
        private TabControl      tabRight;
        private TabPage         tabPreview;
        private TabPage         tabAi;
        private WebBrowser      webPreview;
        private Label           lblAiScore;
        private TextBox         txtAiResult;

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
                var b = new Button { Text=text, Height=30, AutoSize=true, Padding=new Padding(10,0,10,0), FlatStyle=FlatStyle.Flat, Cursor=Cursors.Hand, Font=new Font("微软雅黑",9f) };
                if (bg.HasValue) { b.BackColor=bg.Value; b.ForeColor=Color.White; b.FlatAppearance.BorderSize=0; }
                return b;
            }

            btnSyncFromMeetby  = MakeBtn("⬇ 从meetby同步", Color.FromArgb(31,73,125));
            btnSyncToSendCloud = MakeBtn("⬆ 推送SendCloud", Color.FromArgb(0,120,215));
            btnAiAnalyze       = MakeBtn("🤖 AI分析",        Color.FromArgb(107,33,168));
            lblStatus          = new Label { Text="", ForeColor=Color.Gray, AutoSize=true, Padding=new Padding(8,8,0,0) };

            btnSyncFromMeetby.Click  += btnSyncFromMeetby_Click;
            btnSyncToSendCloud.Click += btnSyncToSendCloud_Click;
            btnAiAnalyze.Click       += btnAiAnalyze_Click;

            var flow = new FlowLayoutPanel { Dock=DockStyle.Fill, FlowDirection=FlowDirection.LeftToRight, WrapContents=false };
            flow.Controls.AddRange(new Control[]{ btnSyncFromMeetby, btnSyncToSendCloud, btnAiAnalyze, lblStatus });
            panelToolbar.Controls.Add(flow);

            dgvTemplates = new DataGridView { Dock=DockStyle.Fill };
            dgvTemplates.SelectionChanged += dgvTemplates_SelectionChanged;

            webPreview = new WebBrowser { Dock=DockStyle.Fill };
            tabPreview = new TabPage("预览");
            tabPreview.Controls.Add(webPreview);

            lblAiScore  = new Label { Text="AI 评分：未分析", Dock=DockStyle.Top, Height=28, Font=new Font("微软雅黑",10f,FontStyle.Bold), ForeColor=Color.FromArgb(31,73,125), Padding=new Padding(5,5,0,0) };
            txtAiResult = new TextBox { Dock=DockStyle.Fill, Multiline=true, ReadOnly=true, ScrollBars=ScrollBars.Vertical, Font=new Font("微软雅黑",9f) };
            tabAi = new TabPage("AI 分析");
            tabAi.Controls.Add(txtAiResult);
            tabAi.Controls.Add(lblAiScore);

            tabRight = new TabControl { Dock=DockStyle.Fill };
            tabRight.TabPages.AddRange(new[]{ tabPreview, tabAi });

            splitMain = new SplitContainer { Dock=DockStyle.Fill, SplitterDistance=380, Orientation=Orientation.Vertical };
            splitMain.Panel1.Controls.Add(dgvTemplates);
            splitMain.Panel2.Controls.Add(tabRight);

            this.Controls.Add(splitMain);
            this.Controls.Add(panelToolbar);
        }
    }
}
