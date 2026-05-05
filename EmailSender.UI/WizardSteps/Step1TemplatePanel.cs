using System;
using System.Drawing;
using System.Windows.Forms;
using EmailSender.Models;
using EmailSender.UI.Common;

namespace EmailSender.UI.WizardSteps
{
    /// <summary>向导第1步：选择邮件模版</summary>
    public partial class Step1TemplatePanel : UserControl
    {
        public EmailTemplate SelectedTemplate { get; private set; }

        public Step1TemplatePanel()
        {
            InitializeComponent();
            UIHelper.StyleGrid(dgvTemplates);
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            var list = ServiceLocator.templateService.GetAll();
            dgvTemplates.DataSource = list;
            SetupColumns();
        }

        private void SetupColumns()
        {
            if (dgvTemplates.Columns.Count == 0) return;
            foreach (DataGridViewColumn col in dgvTemplates.Columns)
                col.Visible = false;

            void Show(string name, string header, int width) {
                if (dgvTemplates.Columns[name] == null) return;
                dgvTemplates.Columns[name].Visible     = true;
                dgvTemplates.Columns[name].HeaderText  = header;
                dgvTemplates.Columns[name].Width       = width;
            }
            Show("Name",    "模版名称", 200);
            Show("Subject", "邮件主题", 280);
            Show("AiScore", "AI评分",    80);
            Show("SendCloudTemplateId", "SendCloud ID", 120);
        }

        private void dgvTemplates_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvTemplates.CurrentRow?.DataBoundItem is not EmailTemplate t) return;
            SelectedTemplate = t;
            lblSelected.Text = $"已选择：{t.Name}（主题：{t.Subject}）";
            lblSelected.ForeColor = Color.FromArgb(16,124,16);

            // 预览
            webPreview.DocumentText = t.HtmlBody ?? "<p>（无内容）</p>";

            // AI 评分提示
            if (t.AiScore.HasValue)
            {
                var color = t.AiScore >= 70 ? Color.FromArgb(16,124,16)
                          : t.AiScore >= 40 ? Color.FromArgb(200,130,0)
                          : Color.FromArgb(196,43,28);
                lblAiHint.ForeColor = color;
                lblAiHint.Text = $"AI 反垃圾评分：{t.AiScore} / 100";
            }
            else
            {
                lblAiHint.ForeColor = Color.Gray;
                lblAiHint.Text = "该模版尚未进行 AI 分析";
            }
        }

        private async void btnSyncTemplates_Click(object sender, EventArgs e)
        {
            btnSyncTemplates.Enabled = false;
            try
            {
                await ServiceLocator.templateService.SyncFromMeetbyAsync();
                LoadTemplates();
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); }
            finally { btnSyncTemplates.Enabled = true; }
        }
    }
}
