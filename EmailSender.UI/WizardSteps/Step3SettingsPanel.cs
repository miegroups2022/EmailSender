using System;
using System.Windows.Forms;
using EmailSender.Models;
using EmailSender.UI.Common;

namespace EmailSender.UI.WizardSteps
{
    /// <summary>向导第3步：发送参数设置</summary>
    public partial class Step3SettingsPanel : UserControl
    {
        public Step3SettingsPanel()
        {
            InitializeComponent();
            LoadAccounts();
        }

        private void LoadAccounts()
        {
            cmbAccount.DataSource    = ServiceLocator.SenderAccountRepo.GetAll();
            cmbAccount.DisplayMember = "Name";
            cmbAccount.ValueMember   = "Id";
        }

        private void cmbChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            // SendCloud 通道不需要选账户（使用 API Key）
            cmbAccount.Enabled = cmbChannel.SelectedIndex != 0;
        }

        /// <summary>将界面参数写入 SendTask 对象</summary>
        public void ApplyTo(SendTask task)
        {
            task.Name           = txtTaskName.Text.Trim();
            task.Channel        = (SendChannel)cmbChannel.SelectedIndex;
            task.AccountId      = cmbAccount.SelectedValue is int id ? id : 0;
            task.ThreadCount    = (int)numThreads.Value;
            task.IntervalSeconds= (int)numInterval.Value;
            task.RetryMax       = (int)numRetry.Value;
            task.BatchSize      = (int)numBatch.Value;

            if (chkSchedule.Checked && dtpSchedule.Value > DateTime.Now)
                task.ScheduledAt = dtpSchedule.Value;

            if (string.IsNullOrEmpty(task.Name))
                task.Name = $"任务_{DateTime.Now:yyyyMMdd_HHmm}";
        }
    }
}
