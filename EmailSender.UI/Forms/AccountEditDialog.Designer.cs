using System.Drawing;
using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace EmailSender.UI.Forms
{
    partial class AccountEditDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlMain = new Panel();
            lblProvider = new Label();
            cmbProvider = new ComboBox();
            lblAccountName = new Label();
            txtAccountName = new TextBox();
            lblGuide = new Label();
            pnlVpnTip = new Panel();
            lblVpnTip = new Label();
            pnlSmtp = new Panel();
            tblSmtp = new TableLayoutPanel();
            lblHost = new Label();
            txtHost = new TextBox();
            lblPort = new Label();
            nudPort = new NumericUpDown();
            lblSsl = new Label();
            chkSsl = new CheckBox();
            lblUser = new Label();
            txtUser = new TextBox();
            lblPass = new Label();
            txtPass = new TextBox();
            lblSenderName = new Label();
            txtSenderName = new TextBox();
            lblSenderEmail = new Label();
            txtSenderEmail = new TextBox();
            pnlOAuth = new Panel();
            btnOAuthLogin = new Button();
            lblOAuthNote = new Label();
            grpAdvanced = new GroupBox();
            tblAdvanced = new TableLayoutPanel();
            lblDailyLimit = new Label();
            nudDailyLimit = new NumericUpDown();
            lblIntervalMin = new Label();
            nudIntervalMin = new NumericUpDown();
            lblIntervalMax = new Label();
            nudIntervalMax = new NumericUpDown();
            lblVpn = new Label();
            chkVpn = new CheckBox();
            pnlBtns = new FlowLayoutPanel();
            btnCancel = new Button();
            btnOk = new Button();
            btnTest = new Button();
            sep = new Panel();
            pnlMain.SuspendLayout();
            pnlVpnTip.SuspendLayout();
            pnlSmtp.SuspendLayout();
            tblSmtp.SuspendLayout();
            ((ISupportInitialize)nudPort).BeginInit();
            pnlOAuth.SuspendLayout();
            grpAdvanced.SuspendLayout();
            tblAdvanced.SuspendLayout();
            ((ISupportInitialize)nudDailyLimit).BeginInit();
            ((ISupportInitialize)nudIntervalMin).BeginInit();
            ((ISupportInitialize)nudIntervalMax).BeginInit();
            pnlBtns.SuspendLayout();
            SuspendLayout();
            // 
            // pnlMain
            // 
            pnlMain.AutoScroll = true;
            pnlMain.Controls.Add(lblProvider);
            pnlMain.Controls.Add(cmbProvider);
            pnlMain.Controls.Add(lblAccountName);
            pnlMain.Controls.Add(txtAccountName);
            pnlMain.Controls.Add(lblGuide);
            pnlMain.Controls.Add(pnlVpnTip);
            pnlMain.Controls.Add(pnlSmtp);
            pnlMain.Controls.Add(pnlOAuth);
            pnlMain.Controls.Add(grpAdvanced);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Name = "pnlMain";
            pnlMain.Padding = new Padding(12, 10, 12, 4);
            pnlMain.Size = new Size(456, 523);
            pnlMain.TabIndex = 0;
            // 
            // lblProvider
            // 
            lblProvider.Location = new Point(12, 14);
            lblProvider.Name = "lblProvider";
            lblProvider.Size = new Size(100, 24);
            lblProvider.TabIndex = 0;
            lblProvider.Text = "邮箱服务商:";
            lblProvider.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbProvider
            // 
            cmbProvider.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProvider.Location = new Point(120, 12);
            cmbProvider.Name = "cmbProvider";
            cmbProvider.Size = new Size(230, 25);
            cmbProvider.TabIndex = 1;
            cmbProvider.SelectedIndexChanged += cmbProvider_SelectedIndexChanged;
            // 
            // lblAccountName
            // 
            lblAccountName.Location = new Point(12, 48);
            lblAccountName.Name = "lblAccountName";
            lblAccountName.Size = new Size(100, 24);
            lblAccountName.TabIndex = 2;
            lblAccountName.Text = "账号别名:";
            lblAccountName.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtAccountName
            // 
            txtAccountName.Location = new Point(120, 46);
            txtAccountName.Name = "txtAccountName";
            txtAccountName.PlaceholderText = "自定义名称，如：公司主邮箱";
            txtAccountName.Size = new Size(310, 23);
            txtAccountName.TabIndex = 3;
            // 
            // lblGuide
            // 
            lblGuide.Font = new Font("微软雅黑", 8.5F);
            lblGuide.ForeColor = Color.FromArgb(0, 120, 215);
            lblGuide.Location = new Point(12, 80);
            lblGuide.Name = "lblGuide";
            lblGuide.Size = new Size(430, 34);
            lblGuide.TabIndex = 4;
            // 
            // pnlVpnTip
            // 
            pnlVpnTip.BackColor = Color.FromArgb(255, 243, 205);
            pnlVpnTip.Controls.Add(lblVpnTip);
            pnlVpnTip.Location = new Point(12, 118);
            pnlVpnTip.Name = "pnlVpnTip";
            pnlVpnTip.Size = new Size(430, 26);
            pnlVpnTip.TabIndex = 5;
            pnlVpnTip.Visible = false;
            // 
            // lblVpnTip
            // 
            lblVpnTip.Dock = DockStyle.Fill;
            lblVpnTip.ForeColor = Color.FromArgb(133, 100, 4);
            lblVpnTip.Location = new Point(0, 0);
            lblVpnTip.Name = "lblVpnTip";
            lblVpnTip.Padding = new Padding(6, 0, 0, 0);
            lblVpnTip.Size = new Size(430, 26);
            lblVpnTip.TabIndex = 0;
            lblVpnTip.Text = "⚠  此邮箱服务在中国大陆需要 VPN 才能访问，请先确保 VPN 已开启。";
            lblVpnTip.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlSmtp
            // 
            pnlSmtp.Controls.Add(tblSmtp);
            pnlSmtp.Location = new Point(12, 150);
            pnlSmtp.Name = "pnlSmtp";
            pnlSmtp.Size = new Size(430, 238);
            pnlSmtp.TabIndex = 6;
            // 
            // tblSmtp
            // 
            tblSmtp.ColumnCount = 2;
            tblSmtp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tblSmtp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblSmtp.Controls.Add(lblHost, 0, 0);
            tblSmtp.Controls.Add(txtHost, 1, 0);
            tblSmtp.Controls.Add(lblPort, 0, 1);
            tblSmtp.Controls.Add(nudPort, 1, 1);
            tblSmtp.Controls.Add(lblSsl, 0, 2);
            tblSmtp.Controls.Add(chkSsl, 1, 2);
            tblSmtp.Controls.Add(lblUser, 0, 3);
            tblSmtp.Controls.Add(txtUser, 1, 3);
            tblSmtp.Controls.Add(lblPass, 0, 4);
            tblSmtp.Controls.Add(txtPass, 1, 4);
            tblSmtp.Controls.Add(lblSenderName, 0, 5);
            tblSmtp.Controls.Add(txtSenderName, 1, 5);
            tblSmtp.Controls.Add(lblSenderEmail, 0, 6);
            tblSmtp.Controls.Add(txtSenderEmail, 1, 6);
            tblSmtp.Dock = DockStyle.Fill;
            tblSmtp.Location = new Point(0, 0);
            tblSmtp.Name = "tblSmtp";
            tblSmtp.RowCount = 7;
            tblSmtp.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblSmtp.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblSmtp.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblSmtp.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblSmtp.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblSmtp.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblSmtp.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblSmtp.Size = new Size(430, 238);
            tblSmtp.TabIndex = 0;
            // 
            // lblHost
            // 
            lblHost.Dock = DockStyle.Fill;
            lblHost.Location = new Point(3, 0);
            lblHost.Name = "lblHost";
            lblHost.Size = new Size(104, 34);
            lblHost.TabIndex = 0;
            lblHost.Text = "SMTP服务器:";
            lblHost.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtHost
            // 
            txtHost.Dock = DockStyle.Fill;
            txtHost.Location = new Point(113, 3);
            txtHost.Name = "txtHost";
            txtHost.PlaceholderText = "如：smtp.163.com";
            txtHost.Size = new Size(314, 23);
            txtHost.TabIndex = 1;
            // 
            // lblPort
            // 
            lblPort.Dock = DockStyle.Fill;
            lblPort.Location = new Point(3, 34);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(104, 34);
            lblPort.TabIndex = 2;
            lblPort.Text = "端口:";
            lblPort.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudPort
            // 
            nudPort.Dock = DockStyle.Fill;
            nudPort.Location = new Point(113, 37);
            nudPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            nudPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudPort.Name = "nudPort";
            nudPort.Size = new Size(314, 23);
            nudPort.TabIndex = 3;
            nudPort.Value = new decimal(new int[] { 465, 0, 0, 0 });
            // 
            // lblSsl
            // 
            lblSsl.Dock = DockStyle.Fill;
            lblSsl.Location = new Point(3, 68);
            lblSsl.Name = "lblSsl";
            lblSsl.Size = new Size(104, 34);
            lblSsl.TabIndex = 4;
            lblSsl.Text = "加密:";
            lblSsl.TextAlign = ContentAlignment.MiddleRight;
            // 
            // chkSsl
            // 
            chkSsl.Checked = true;
            chkSsl.CheckState = CheckState.Checked;
            chkSsl.Dock = DockStyle.Fill;
            chkSsl.Location = new Point(113, 71);
            chkSsl.Name = "chkSsl";
            chkSsl.Size = new Size(314, 28);
            chkSsl.TabIndex = 5;
            chkSsl.Text = "启用 SSL / TLS";
            // 
            // lblUser
            // 
            lblUser.Dock = DockStyle.Fill;
            lblUser.Location = new Point(3, 102);
            lblUser.Name = "lblUser";
            lblUser.Size = new Size(104, 34);
            lblUser.TabIndex = 6;
            lblUser.Text = "用户名/邮箱:";
            lblUser.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtUser
            // 
            txtUser.Dock = DockStyle.Fill;
            txtUser.Location = new Point(113, 105);
            txtUser.Name = "txtUser";
            txtUser.PlaceholderText = "登录邮箱地址";
            txtUser.Size = new Size(314, 23);
            txtUser.TabIndex = 7;
            // 
            // lblPass
            // 
            lblPass.Dock = DockStyle.Fill;
            lblPass.Location = new Point(3, 136);
            lblPass.Name = "lblPass";
            lblPass.Size = new Size(104, 34);
            lblPass.TabIndex = 8;
            lblPass.Text = "密码/授权码:";
            lblPass.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtPass
            // 
            txtPass.Dock = DockStyle.Fill;
            txtPass.Location = new Point(113, 139);
            txtPass.Name = "txtPass";
            txtPass.PlaceholderText = "密码或授权码";
            txtPass.Size = new Size(314, 23);
            txtPass.TabIndex = 9;
            txtPass.UseSystemPasswordChar = true;
            // 
            // lblSenderName
            // 
            lblSenderName.Dock = DockStyle.Fill;
            lblSenderName.Location = new Point(3, 170);
            lblSenderName.Name = "lblSenderName";
            lblSenderName.Size = new Size(104, 34);
            lblSenderName.TabIndex = 10;
            lblSenderName.Text = "发件人名称:";
            lblSenderName.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtSenderName
            // 
            txtSenderName.Dock = DockStyle.Fill;
            txtSenderName.Location = new Point(113, 173);
            txtSenderName.Name = "txtSenderName";
            txtSenderName.PlaceholderText = "发件人显示名称";
            txtSenderName.Size = new Size(314, 23);
            txtSenderName.TabIndex = 11;
            // 
            // lblSenderEmail
            // 
            lblSenderEmail.Dock = DockStyle.Fill;
            lblSenderEmail.Location = new Point(3, 204);
            lblSenderEmail.Name = "lblSenderEmail";
            lblSenderEmail.Size = new Size(104, 34);
            lblSenderEmail.TabIndex = 12;
            lblSenderEmail.Text = "发件人邮箱:";
            lblSenderEmail.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtSenderEmail
            // 
            txtSenderEmail.Dock = DockStyle.Fill;
            txtSenderEmail.Location = new Point(113, 207);
            txtSenderEmail.Name = "txtSenderEmail";
            txtSenderEmail.PlaceholderText = "留空则同用户名";
            txtSenderEmail.Size = new Size(314, 23);
            txtSenderEmail.TabIndex = 13;
            // 
            // pnlOAuth
            // 
            pnlOAuth.Controls.Add(btnOAuthLogin);
            pnlOAuth.Controls.Add(lblOAuthNote);
            pnlOAuth.Location = new Point(12, 150);
            pnlOAuth.Name = "pnlOAuth";
            pnlOAuth.Size = new Size(430, 90);
            pnlOAuth.TabIndex = 7;
            pnlOAuth.Visible = false;
            // 
            // btnOAuthLogin
            // 
            btnOAuthLogin.BackColor = Color.FromArgb(33, 150, 243);
            btnOAuthLogin.Dock = DockStyle.Top;
            btnOAuthLogin.FlatStyle = FlatStyle.Flat;
            btnOAuthLogin.ForeColor = Color.White;
            btnOAuthLogin.Location = new Point(0, 50);
            btnOAuthLogin.Name = "btnOAuthLogin";
            btnOAuthLogin.Size = new Size(430, 34);
            btnOAuthLogin.TabIndex = 0;
            btnOAuthLogin.Text = "📋 查看申请步骤 & 打开申请页面";
            btnOAuthLogin.UseVisualStyleBackColor = false;
            btnOAuthLogin.Click += btnOAuthLogin_Click;
            // 
            // lblOAuthNote
            // 
            lblOAuthNote.Dock = DockStyle.Top;
            lblOAuthNote.ForeColor = Color.FromArgb(60, 60, 60);
            lblOAuthNote.Location = new Point(0, 0);
            lblOAuthNote.Name = "lblOAuthNote";
            lblOAuthNote.Padding = new Padding(4, 4, 0, 0);
            lblOAuthNote.Size = new Size(430, 50);
            lblOAuthNote.TabIndex = 1;
            lblOAuthNote.Text = "Gmail / Outlook 需使用「应用专用密码」。\r\n点击下方按钮查看详细步骤并打开申请页面，获取密码后填入 SMTP 密码栏即可。";
            // 
            // grpAdvanced
            // 
            grpAdvanced.Controls.Add(tblAdvanced);
            grpAdvanced.Font = new Font("微软雅黑", 9F);
            grpAdvanced.Location = new Point(12, 396);
            grpAdvanced.Name = "grpAdvanced";
            grpAdvanced.Size = new Size(430, 80);
            grpAdvanced.TabIndex = 8;
            grpAdvanced.TabStop = false;
            grpAdvanced.Text = "高级发送设置";
            // 
            // tblAdvanced
            // 
            tblAdvanced.ColumnCount = 4;
            tblAdvanced.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            tblAdvanced.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblAdvanced.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            tblAdvanced.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblAdvanced.Controls.Add(lblDailyLimit, 0, 0);
            tblAdvanced.Controls.Add(nudDailyLimit, 1, 0);
            tblAdvanced.Controls.Add(lblIntervalMin, 2, 0);
            tblAdvanced.Controls.Add(nudIntervalMin, 3, 0);
            tblAdvanced.Controls.Add(lblIntervalMax, 0, 1);
            tblAdvanced.Controls.Add(nudIntervalMax, 1, 1);
            tblAdvanced.Controls.Add(lblVpn, 2, 1);
            tblAdvanced.Controls.Add(chkVpn, 3, 1);
            tblAdvanced.Dock = DockStyle.Fill;
            tblAdvanced.Location = new Point(3, 19);
            tblAdvanced.Name = "tblAdvanced";
            tblAdvanced.RowCount = 2;
            tblAdvanced.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblAdvanced.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblAdvanced.Size = new Size(424, 58);
            tblAdvanced.TabIndex = 0;
            // 
            // lblDailyLimit
            // 
            lblDailyLimit.Dock = DockStyle.Fill;
            lblDailyLimit.Location = new Point(3, 0);
            lblDailyLimit.Name = "lblDailyLimit";
            lblDailyLimit.Size = new Size(84, 30);
            lblDailyLimit.TabIndex = 0;
            lblDailyLimit.Text = "每日限额:";
            lblDailyLimit.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudDailyLimit
            // 
            nudDailyLimit.Dock = DockStyle.Fill;
            nudDailyLimit.Location = new Point(93, 3);
            nudDailyLimit.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudDailyLimit.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudDailyLimit.Name = "nudDailyLimit";
            nudDailyLimit.Size = new Size(116, 23);
            nudDailyLimit.TabIndex = 1;
            nudDailyLimit.Value = new decimal(new int[] { 200, 0, 0, 0 });
            // 
            // lblIntervalMin
            // 
            lblIntervalMin.Dock = DockStyle.Fill;
            lblIntervalMin.Location = new Point(215, 0);
            lblIntervalMin.Name = "lblIntervalMin";
            lblIntervalMin.Size = new Size(84, 30);
            lblIntervalMin.TabIndex = 2;
            lblIntervalMin.Text = "最小间隔(秒):";
            lblIntervalMin.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudIntervalMin
            // 
            nudIntervalMin.Dock = DockStyle.Fill;
            nudIntervalMin.Location = new Point(305, 3);
            nudIntervalMin.Maximum = new decimal(new int[] { 3600, 0, 0, 0 });
            nudIntervalMin.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudIntervalMin.Name = "nudIntervalMin";
            nudIntervalMin.Size = new Size(116, 23);
            nudIntervalMin.TabIndex = 3;
            nudIntervalMin.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // lblIntervalMax
            // 
            lblIntervalMax.Dock = DockStyle.Fill;
            lblIntervalMax.Location = new Point(3, 30);
            lblIntervalMax.Name = "lblIntervalMax";
            lblIntervalMax.Size = new Size(84, 30);
            lblIntervalMax.TabIndex = 4;
            lblIntervalMax.Text = "最大间隔(秒):";
            lblIntervalMax.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudIntervalMax
            // 
            nudIntervalMax.Dock = DockStyle.Fill;
            nudIntervalMax.Location = new Point(93, 33);
            nudIntervalMax.Maximum = new decimal(new int[] { 3600, 0, 0, 0 });
            nudIntervalMax.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudIntervalMax.Name = "nudIntervalMax";
            nudIntervalMax.Size = new Size(116, 23);
            nudIntervalMax.TabIndex = 5;
            nudIntervalMax.Value = new decimal(new int[] { 90, 0, 0, 0 });
            // 
            // lblVpn
            // 
            lblVpn.Dock = DockStyle.Fill;
            lblVpn.Location = new Point(215, 30);
            lblVpn.Name = "lblVpn";
            lblVpn.Size = new Size(84, 30);
            lblVpn.TabIndex = 6;
            lblVpn.Text = "需要VPN:";
            lblVpn.TextAlign = ContentAlignment.MiddleRight;
            // 
            // chkVpn
            // 
            chkVpn.Dock = DockStyle.Fill;
            chkVpn.Location = new Point(305, 33);
            chkVpn.Name = "chkVpn";
            chkVpn.Size = new Size(116, 24);
            chkVpn.TabIndex = 7;
            chkVpn.Text = "是";
            // 
            // pnlBtns
            // 
            pnlBtns.Controls.Add(btnCancel);
            pnlBtns.Controls.Add(btnOk);
            pnlBtns.Controls.Add(btnTest);
            pnlBtns.Dock = DockStyle.Bottom;
            pnlBtns.FlowDirection = FlowDirection.RightToLeft;
            pnlBtns.Location = new Point(0, 524);
            pnlBtns.Name = "pnlBtns";
            pnlBtns.Padding = new Padding(8, 6, 8, 0);
            pnlBtns.Size = new Size(456, 46);
            pnlBtns.TabIndex = 2;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(357, 9);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 30);
            btnCancel.TabIndex = 0;
            btnCancel.Text = "取消";
            // 
            // btnOk
            // 
            btnOk.BackColor = Color.FromArgb(33, 150, 243);
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.ForeColor = Color.White;
            btnOk.Location = new Point(261, 9);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(90, 30);
            btnOk.TabIndex = 1;
            btnOk.Text = "💾 保存";
            btnOk.UseVisualStyleBackColor = false;
            btnOk.Click += btnOk_Click;
            // 
            // btnTest
            // 
            btnTest.Location = new Point(155, 9);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(100, 30);
            btnTest.TabIndex = 2;
            btnTest.Text = "🔌 测试连接";
            btnTest.Click += btnTest_Click;
            // 
            // sep
            // 
            sep.BackColor = Color.LightGray;
            sep.Dock = DockStyle.Bottom;
            sep.Location = new Point(0, 523);
            sep.Name = "sep";
            sep.Size = new Size(456, 1);
            sep.TabIndex = 1;
            // 
            // AccountEditDialog
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(456, 570);
            Controls.Add(pnlMain);
            Controls.Add(sep);
            Controls.Add(pnlBtns);
            Font = new Font("微软雅黑", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AccountEditDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "添加邮箱账号";
            pnlMain.ResumeLayout(false);
            pnlMain.PerformLayout();
            pnlVpnTip.ResumeLayout(false);
            pnlSmtp.ResumeLayout(false);
            tblSmtp.ResumeLayout(false);
            tblSmtp.PerformLayout();
            ((ISupportInitialize)nudPort).EndInit();
            pnlOAuth.ResumeLayout(false);
            grpAdvanced.ResumeLayout(false);
            tblAdvanced.ResumeLayout(false);
            ((ISupportInitialize)nudDailyLimit).EndInit();
            ((ISupportInitialize)nudIntervalMin).EndInit();
            ((ISupportInitialize)nudIntervalMax).EndInit();
            pnlBtns.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Label lblProvider, lblAccountName, lblGuide, lblVpnTip;
        private System.Windows.Forms.Panel pnlVpnTip;
        private System.Windows.Forms.ComboBox cmbProvider;
        private System.Windows.Forms.TextBox txtAccountName;
        private System.Windows.Forms.Panel pnlSmtp;
        private System.Windows.Forms.TableLayoutPanel tblSmtp;
        private System.Windows.Forms.Label lblHost, lblPort, lblSsl, lblUser, lblPass;
        private System.Windows.Forms.Label lblSenderName, lblSenderEmail;
        private System.Windows.Forms.TextBox txtHost, txtUser, txtPass, txtSenderName, txtSenderEmail;
        private System.Windows.Forms.NumericUpDown nudPort;
        private System.Windows.Forms.CheckBox chkSsl;
        private System.Windows.Forms.Panel pnlOAuth;
        private System.Windows.Forms.Label lblOAuthNote;
        private System.Windows.Forms.Button btnOAuthLogin;
        private System.Windows.Forms.GroupBox grpAdvanced;
        private System.Windows.Forms.TableLayoutPanel tblAdvanced;
        private System.Windows.Forms.Label lblDailyLimit, lblIntervalMin, lblIntervalMax, lblVpn;
        private System.Windows.Forms.NumericUpDown nudDailyLimit, nudIntervalMin, nudIntervalMax;
        private System.Windows.Forms.CheckBox chkVpn;
        private System.Windows.Forms.FlowLayoutPanel pnlBtns;
        private System.Windows.Forms.Button btnOk, btnCancel, btnTest;
        private System.Windows.Forms.Panel sep;
    }
}