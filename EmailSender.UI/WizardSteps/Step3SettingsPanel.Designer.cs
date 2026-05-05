using System.Drawing;
using System.Windows.Forms;
using EmailSender.UI.Common;

namespace EmailSender.UI.WizardSteps
{
    partial class Step3SettingsPanel
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtTaskName;
        private ComboBox cmbChannel;
        private ComboBox cmbAccount;
        private NumericUpDown numThreads;
        private NumericUpDown numInterval;
        private NumericUpDown numRetry;
        private NumericUpDown numBatch;
        private CheckBox chkSchedule;
        private DateTimePicker dtpSchedule;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private Label MakeLbl(string text) =>
            new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("微软雅黑", 9f),
                Padding = new Padding(0, 7, 10, 0)
            };

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.Font = new Font("微软雅黑", 9f);

            txtTaskName = new TextBox
            {
                Width = 280,
                Height = 26,
                Font = new Font("微软雅黑", 9f)
            };
            UIHelper.SetPlaceholder(txtTaskName, "留空则自动生成"); // ✅ 替代 PlaceholderText

            cmbChannel = new ComboBox
            {
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("微软雅黑", 9f)
            };
            cmbChannel.Items.AddRange(new object[]{
                "SendCloud","Gmail","Hotmail","SMTP"
            });
            cmbChannel.SelectedIndex = 0;
            cmbChannel.SelectedIndexChanged += cmbChannel_SelectedIndexChanged;

            cmbAccount = new ComboBox
            {
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("微软雅黑", 9f)
            };

            NumericUpDown MakeNum(int min, int max, int val, int width = 80) =>
                new NumericUpDown
                {
                    Minimum = min,
                    Maximum = max,
                    Value = val,
                    Width = width,
                    Font = new Font("微软雅黑", 9f)
                };

            numThreads = MakeNum(1, 20, 3);
            numInterval = MakeNum(0, 300, 5);
            numRetry = MakeNum(1, 10, 3);
            numBatch = MakeNum(1, 500, 50);

            chkSchedule = new CheckBox
            {
                Text = "定时发送",
                AutoSize = true,
                Font = new Font("微软雅黑", 9f)
            };
            dtpSchedule = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm",
                Width = 180,
                Enabled = false,
                Font = new Font("微软雅黑", 9f)
            };
            chkSchedule.CheckedChanged += (s, e) =>
                dtpSchedule.Enabled = chkSchedule.Checked;

            // 表单布局
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(30, 20, 30, 0)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 8; i++)
                tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            tbl.Controls.Add(MakeLbl("任务名称："), 0, 0); tbl.Controls.Add(txtTaskName, 1, 0);
            tbl.Controls.Add(MakeLbl("发送通道："), 0, 1); tbl.Controls.Add(cmbChannel, 1, 1);
            tbl.Controls.Add(MakeLbl("发送账户："), 0, 2); tbl.Controls.Add(cmbAccount, 1, 2);
            tbl.Controls.Add(MakeLbl("并发线程数："), 0, 3); tbl.Controls.Add(numThreads, 1, 3);
            tbl.Controls.Add(MakeLbl("发送间隔（秒）："), 0, 4); tbl.Controls.Add(numInterval, 1, 4);
            tbl.Controls.Add(MakeLbl("最大重试次数："), 0, 5); tbl.Controls.Add(numRetry, 1, 5);
            tbl.Controls.Add(MakeLbl("批次大小："), 0, 6); tbl.Controls.Add(numBatch, 1, 6);

            var flowSchedule = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight
            };
            flowSchedule.Controls.AddRange(new Control[] { chkSchedule, dtpSchedule });
            tbl.Controls.Add(MakeLbl("定时发送："), 0, 7);
            tbl.Controls.Add(flowSchedule, 1, 7);

            // 提示标签
            var lblHint = new Label
            {
                Text = "💡 提示：并发线程数建议 3-5，间隔建议 3-10 秒，避免触发发送频率限制",
                Dock = DockStyle.Top,
                Height = 32,
                ForeColor = Color.FromArgb(0, 120, 215),
                Font = new Font("微软雅黑", 8.5f),
                Padding = new Padding(30, 8, 0, 0)
            };

            this.Controls.Add(lblHint);
            this.Controls.Add(tbl);
        }
    }
}
