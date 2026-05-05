using System;
using System.Windows.Forms;
using EmailSender.Models;
using EmailSender.UI.Common;

namespace EmailSender.UI.Controls
{
    public partial class TemplateControl : UserControl
    {
        public TemplateControl()
        {
            InitializeComponent();
            UIHelper.StyleGrid(dgvTemplates);
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            dgvTemplates.DataSource = null;
            dgvTemplates.DataSource = ServiceLocator.TemplateService.GetAll();
        }

        private async void btnSyncFromMeetby_Click(object sender, EventArgs e)
        {
            btnSyncFromMeetby.Enabled = false;
            lblStatus.Text = "正在从 meetby 同步模版...";
            try
            {
                var list = await ServiceLocator.TemplateService.SyncFromMeetbyAsync();
                lblStatus.Text = $"同步完成，共 {list.Count} 个模版";
                LoadTemplates();
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); lblStatus.Text = ""; }
            finally { btnSyncFromMeetby.Enabled = true; }
        }

        private async void btnSyncToSendCloud_Click(object sender, EventArgs e)
        {
            if (dgvTemplates.CurrentRow?.DataBoundItem is not EmailTemplate t) return;
            btnSyncToSendCloud.Enabled = false;
            lblStatus.Text = "正在推送到 SendCloud...";
            try
            {
                await ServiceLocator.TemplateService.SyncToSendCloudAsync(t.Id);
                lblStatus.Text = "推送成功！";
                LoadTemplates();
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); lblStatus.Text = ""; }
            finally { btnSyncToSendCloud.Enabled = true; }
        }

        private async void btnAiAnalyze_Click(object sender, EventArgs e)
        {
            if (dgvTemplates.CurrentRow?.DataBoundItem is not EmailTemplate t) return;
            btnAiAnalyze.Enabled = false;
            lblStatus.Text = "AI 分析中，请稍候...";
            try
            {
                await ServiceLocator.TemplateService.AnalyzeWithAiAsync(t.Id);
                var updated = ServiceLocator.TemplateService.GetById(t.Id);
                lblAiScore.Text = $"AI 评分：{updated.AiScore} / 100";

                // ✅ 修复：使用 \n 替代实际换行符
                txtAiResult.Text = $"问题：{updated.AiIssues}\r\n\r\n建议：{updated.AiSuggestions}";

                lblStatus.Text = "AI 分析完成";
                LoadTemplates();
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); lblStatus.Text = ""; }
            finally { btnAiAnalyze.Enabled = true; }
        }

        private void dgvTemplates_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvTemplates.CurrentRow?.DataBoundItem is not EmailTemplate t) return;
            webPreview.DocumentText = t.HtmlBody ?? "<p>（无内容）</p>";
            lblAiScore.Text = t.AiScore.HasValue
                ? $"AI 评分：{t.AiScore} / 100" : "AI 评分：未分析";
            txtAiResult.Text = t.AiSuggestions ?? "";
        }
    }
}