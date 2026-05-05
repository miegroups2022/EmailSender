using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using EmailSender.Core.Services;
using EmailSender.Models;
using EmailSender.UI.Common;
using EmailSender.UI.Forms;

namespace EmailSender.UI.Controls
{
    public partial class SendTaskControl : UserControl
    {
        public SendTaskControl()
        {
            InitializeComponent();
            UIHelper.StyleGrid(dgvTasks);
            LoadTasks();
        }

        private void LoadTasks()
        {
            var tasks = ServiceLocator.sendTaskService.GetAll();
            dgvTasks.DataSource = null;
            dgvTasks.DataSource = tasks;
            SetupColumns();
        }

        private void SetupColumns()
        {
            if (dgvTasks.Columns.Count == 0) return;
            dgvTasks.Columns["Id"].Visible = false;
            dgvTasks.Columns["FilterConfig"].Visible = false;
            dgvTasks.Columns["TemplateId"].Visible = false;
            dgvTasks.Columns["ListId"].Visible = false;
            dgvTasks.Columns["AccountId"].Visible = false;

            if (dgvTasks.Columns["Name"] != null) dgvTasks.Columns["Name"].HeaderText = "任务名称";
            if (dgvTasks.Columns["TemplateName"] != null) dgvTasks.Columns["TemplateName"].HeaderText = "模版";
            if (dgvTasks.Columns["ListName"] != null) dgvTasks.Columns["ListName"].HeaderText = "列表";
            if (dgvTasks.Columns["Status"] != null) dgvTasks.Columns["Status"].HeaderText = "状态";
            if (dgvTasks.Columns["TotalCount"] != null) dgvTasks.Columns["TotalCount"].HeaderText = "总数";
            if (dgvTasks.Columns["SuccessCount"] != null) dgvTasks.Columns["SuccessCount"].HeaderText = "成功";
            if (dgvTasks.Columns["FailCount"] != null) dgvTasks.Columns["FailCount"].HeaderText = "失败";
            if (dgvTasks.Columns["OpenCount"] != null) dgvTasks.Columns["OpenCount"].HeaderText = "打开";
            if (dgvTasks.Columns["ScheduledAt"] != null) dgvTasks.Columns["ScheduledAt"].HeaderText = "计划时间";
            if (dgvTasks.Columns["CreatedAt"] != null) dgvTasks.Columns["CreatedAt"].HeaderText = "创建时间";
        }

        private void btnNewTask_Click(object sender, EventArgs e)
        {
            var wizard = new WizardForm();
            if (wizard.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                LoadTasks();
        }

        private async void btnStartTask_Click(object sender, EventArgs e)
        {
            if (dgvTasks.CurrentRow?.DataBoundItem is not SendTask task) return;
            if (!UIHelper.Confirm($"确定启动任务「{task.Name}」？")) return;

            btnStartTask.Enabled = false;
            var progressForm = UIHelper.CreateProgressForm(
                "发送中...", out var bar, out var lbl);
            progressForm.Show(this);

            var progress = new Progress<SendProgressInfo>(info =>
            {
                UIHelper.InvokeIfNeeded(this, () =>
                {
                    bar.Maximum = info.Total;
                    bar.Value = Math.Min(info.Sent, info.Total);
                    lbl.Text = $"已发 {info.Sent}/{info.Total}  成功:{info.Success}  失败:{info.Failed}";
                });
            });

            try
            {
                await ServiceLocator.sendTaskService.StartTaskAsync(task.Id, progress);
                UIHelper.Info("任务执行完成！");
            }
            catch (Exception ex)
            {
                UIHelper.Error($"任务执行失败：{ex.Message}");
            }
            finally
            {
                progressForm.Close();
                btnStartTask.Enabled = true;
                LoadTasks();
            }
        }

        private async void btnFetchResult_Click(object sender, EventArgs e)
        {
            if (dgvTasks.CurrentRow?.DataBoundItem is not SendTask task) return;

            btnFetchResult.Enabled = false;
            lblStatus.Text = "正在拉取结果...";
            try
            {
                int updated = await ServiceLocator.resultFetchService
                    .FetchPendingResultsAsync(task.Id);
                lblStatus.Text = $"已更新 {updated} 条记录";
                LoadTasks();
            }
            catch (Exception ex)
            {
                UIHelper.Error($"拉取结果失败：{ex.Message}");
                lblStatus.Text = "";
            }
            finally
            {
                btnFetchResult.Enabled = true;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e) => LoadTasks();

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvTasks.CurrentRow?.DataBoundItem is not SendTask task) return;
            if (!UIHelper.Confirm($"确定删除任务「{task.Name}」？")) return;
            ServiceLocator.sendTaskService.Delete(task.Id);
            LoadTasks();
        }
    }
}