using System.Drawing;
using System.Windows.Forms;
using EmailSender.UI.Common;

namespace EmailSender.UI.Controls
{
    partial class BlacklistControl
    {
        private System.ComponentModel.IContainer components = null;
        private Panel panelTop;
        private Panel panelAdd;
        private DataGridView dgvBlacklist;
        private Label lblCount;
        private TextBox txtEmail;
        private TextBox txtReason;
        private TextBox txtSearch;
        private Button btnAdd;
        private Button btnDelete;
        private Button btnImport;
        private Button btnExport;
        private Button btnRefresh;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;

            // ── 顶部工具栏 ──────────────────────────────────────
            panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = Color.White,
                Padding = new Padding(5, 8, 5, 0)
            };

            Button MakeBtn(string text, Color? bg = null)
            {
                var b = new Button
                {
                    Text = text,
                    Height = 30,
                    AutoSize = true,
                    Padding = new Padding(10, 0, 10, 0),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Font = new Font("微软雅黑", 9f)
                };
                if (bg.HasValue)
                {
                    b.BackColor = bg.Value; b.ForeColor = Color.White;
                    b.FlatAppearance.BorderSize = 0;
                }
                return b;
            }

            btnDelete = MakeBtn("✕ 移除", Color.FromArgb(196, 43, 28));
            btnImport = MakeBtn("⬆ 批量导入", Color.FromArgb(0, 120, 215));
            btnExport = MakeBtn("⬇ 导出");
            btnRefresh = MakeBtn("↺ 刷新");

            // ✅ 用 UIHelper.SetPlaceholder 替代 PlaceholderText 属性
            txtSearch = new TextBox
            {
                Width = 200,
                Height = 28,
                Font = new Font("微软雅黑", 9f)
            };
            UIHelper.SetPlaceholder(txtSearch, "搜索邮件地址...");

            lblCount = new Label
            {
                Text = "",
                ForeColor = Color.Gray,
                AutoSize = true,
                Padding = new Padding(8, 8, 0, 0)
            };

            btnDelete.Click += btnDelete_Click;
            btnImport.Click += btnImport_Click;
            btnExport.Click += btnExport_Click;
            btnRefresh.Click += btnRefresh_Click;
            txtSearch.TextChanged += txtSearch_TextChanged;

            var flowTop = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            flowTop.Controls.AddRange(new Control[]{
                txtSearch, btnDelete, btnImport, btnExport, btnRefresh, lblCount
            });
            panelTop.Controls.Add(flowTop);

            // ── 手动添加区 ──────────────────────────────────────
            panelAdd = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = Color.FromArgb(250, 250, 252),
                Padding = new Padding(5, 8, 5, 0)
            };

            // ✅ 用 UIHelper.SetPlaceholder 替代 PlaceholderText 属性
            txtEmail = new TextBox
            {
                Width = 240,
                Height = 28,
                Font = new Font("微软雅黑", 9f)
            };
            UIHelper.SetPlaceholder(txtEmail, "输入邮件地址");

            txtReason = new TextBox
            {
                Width = 200,
                Height = 28,
                Font = new Font("微软雅黑", 9f)
            };
            UIHelper.SetPlaceholder(txtReason, "备注原因（可选）");

            btnAdd = MakeBtn("＋ 手动添加", Color.FromArgb(31, 73, 125));
            btnAdd.Click += btnAdd_Click;

            var flowAdd = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            flowAdd.Controls.AddRange(new Control[] { txtEmail, txtReason, btnAdd });
            panelAdd.Controls.Add(flowAdd);

            // ── 数据表格 ────────────────────────────────────────
            dgvBlacklist = new DataGridView { Dock = DockStyle.Fill };

            this.Controls.Add(dgvBlacklist);
            this.Controls.Add(panelAdd);
            this.Controls.Add(panelTop);
        }
    }
}
