using EmailSender.UI.Common;
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

            Button MakeBtn(string text, System.Drawing.Color? bg=null) {
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

            panelStats = new Panel {
                Dock=DockStyle.Top, Height=36,
                BackColor=Color.FromArgb(240,248,255),
                Padding=new Padding(12,8,0,0)
            };
            lblTotal = new Label {
                Text="有效地址：0 个",
                Font=new Font("微软雅黑",10f,System.Drawing.FontStyle.Bold),
                ForeColor=Color.FromArgb(31,73,125),
                AutoSize=true
            };
            panelStats.Controls.Add(lblTotal);

            dgvDomain = new DataGridView { Dock=DockStyle.Fill };
            UIHelper.StyleGrid(dgvDomain);

            this.Controls.Add(dgvDomain);
            this.Controls.Add(panelStats);
            this.Controls.Add(panelToolbar);
        }
    }
}
