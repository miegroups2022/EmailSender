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
