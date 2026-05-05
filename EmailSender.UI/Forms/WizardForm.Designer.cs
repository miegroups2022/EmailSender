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
