using EmailSender.UI.Common;
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
