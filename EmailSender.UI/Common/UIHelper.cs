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


        /// <summary>
        /// 为 TextBox 设置占位符文本（.NET Framework 4.8 兼容实现）
        /// </summary>
        public static void SetPlaceholder(System.Windows.Forms.TextBox textBox, string placeholder)
        {
            if (textBox == null) return;

            // 初始状态设置
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = placeholder;
                textBox.ForeColor = System.Drawing.Color.Gray;
            }

            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Text == placeholder && textBox.ForeColor == System.Drawing.Color.Gray)
                {
                    textBox.Text = "";
                    textBox.ForeColor = System.Drawing.Color.Black;
                }
            };

            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = System.Drawing.Color.Gray;
                }
            };
        }


    }



}
