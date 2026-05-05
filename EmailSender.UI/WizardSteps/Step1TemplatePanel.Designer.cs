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
