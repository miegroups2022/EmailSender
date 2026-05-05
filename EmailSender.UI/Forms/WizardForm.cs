using System;
using System.Drawing;
using System.Windows.Forms;
using EmailSender.Models;
using EmailSender.UI.Common;
using EmailSender.UI.WizardSteps;

namespace EmailSender.UI.Forms
{
    /// <summary>新建任务向导（3步）</summary>
    public partial class WizardForm : Form
    {
        private int          _step = 1;
        private SendTask     _task = new SendTask();

        private Step1TemplatePanel _step1;
        private Step2ListPanel     _step2;
        private Step3SettingsPanel _step3;

        public WizardForm()
        {
            InitializeComponent();
            _step1 = new Step1TemplatePanel();
            _step2 = new Step2ListPanel();
            _step3 = new Step3SettingsPanel();
            ShowStep(1);
        }

        private void ShowStep(int step)
        {
            _step = step;
            panelStep.Controls.Clear();

            UserControl ctrl = step switch { 1 => _step1, 2 => _step2, _ => _step3 };
            ctrl.Dock = DockStyle.Fill;
            panelStep.Controls.Add(ctrl);

            lblStepTitle.Text = step switch {
                1 => "第 1 步 / 3  —  选择邮件模版",
                2 => "第 2 步 / 3  —  选择邮件列表",
                _ => "第 3 步 / 3  —  发送设置"
            };

            btnPrev.Enabled = step > 1;
            btnNext.Text    = step == 3 ? "✅ 创建任务" : "下一步 >";
            UpdateStepIndicator();
        }

        private void UpdateStepIndicator()
        {
            for (int i = 1; i <= 3; i++)
            {
                var lbl = Controls.Find($"lblStep{i}", true);
                if (lbl.Length > 0)
                    lbl[0].ForeColor = i == _step
                        ? Color.FromArgb(46, 116, 181) : Color.Gray;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (_step == 1)
            {
                if (_step1.SelectedTemplate == null)
                { UIHelper.Warn("请选择一个模版"); return; }
                _task.TemplateId   = _step1.SelectedTemplate.Id;
                _task.TemplateName = _step1.SelectedTemplate.Name;
                ShowStep(2);
            }
            else if (_step == 2)
            {
                if (_step2.SelectedListId == 0)
                { UIHelper.Warn("请选择一个邮件列表"); return; }
                _task.ListId   = _step2.SelectedListId;
                _task.ListName = _step2.SelectedListName;
                ShowStep(3);
            }
            else
            {
                // 第3步：创建任务
                _step3.ApplyTo(_task);
                if (_task.AccountId == 0)
                { UIHelper.Warn("请选择发送账户"); return; }

                try
                {
                    int taskId = ServiceLocator.sendTaskService.CreateTask(_task);
                    UIHelper.Info($"任务创建成功！任务ID：{taskId}");
                    DialogResult = System.Windows.Forms.DialogResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    UIHelper.Error($"创建任务失败：{ex.Message}");
                }
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
            => ShowStep(_step - 1);

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (UIHelper.Confirm("确定取消新建任务？"))
                Close();
        }
    }
}
