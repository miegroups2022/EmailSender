using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EmailSender.UI.Common;

namespace EmailSender.UI.WizardSteps
{
    /// <summary>向导第2步：选择邮件列表 + 数据清洗设置</summary>
    public partial class Step2ListPanel : UserControl
    {
        public int    SelectedListId   { get; private set; }
        public string SelectedListName { get; private set; }

        public Step2ListPanel()
        {
            InitializeComponent();
            LoadLists();
        }

        private async void LoadLists()
        {
            try
            {
                var lists = await ServiceLocator.MeetbyApi.GetEmailListsAsync();
                dgvLists.DataSource = lists.Select(l => new {
                    列表ID   = l.ListId,
                    列表名称 = l.ListName,
                    成员数量 = l.MemberCount
                }).ToList();
            }
            catch (Exception ex) { UIHelper.Error($"获取列表失败：{ex.Message}"); }
        }

        private void dgvLists_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvLists.CurrentRow == null) return;
            SelectedListId   = (int)dgvLists.CurrentRow.Cells["列表ID"].Value;
            SelectedListName = dgvLists.CurrentRow.Cells["列表名称"].Value?.ToString();

            // 刷新本地统计
            int localCount = ServiceLocator.emailListService.GetCount(SelectedListId);
            lblLocalCount.Text = localCount > 0
                ? $"本地已缓存：{localCount} 个地址"
                : "本地暂无缓存，请先下载";
            lblLocalCount.ForeColor = localCount > 0
                ? Color.FromArgb(16,124,16) : Color.FromArgb(200,130,0);
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            if (SelectedListId == 0) { UIHelper.Warn("请先选择列表"); return; }
            btnDownload.Enabled = false;
            lblDownloadStatus.Text = "下载中...";

            var progress = new Progress<(int current, int total)>(p =>
                UIHelper.InvokeIfNeeded(this, () =>
                    lblDownloadStatus.Text = $"下载中 {p.current}/{p.total}"));
            try
            {
                int count = await ServiceLocator.emailListService
                    .DownloadAndSaveAsync(SelectedListId, SelectedListName, progress);
                lblDownloadStatus.Text = $"下载完成，共 {count} 个";
                lblLocalCount.Text     = $"本地已缓存：{count} 个地址";
                lblLocalCount.ForeColor = Color.FromArgb(16,124,16);
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); lblDownloadStatus.Text=""; }
            finally { btnDownload.Enabled = true; }
        }

        /// <summary>向导第3步读取清洗参数</summary>
        public (int maxFail, bool excludeThisWeek) GetFilterOptions()
            => ((int)numMaxFail.Value, chkExcludeThisWeek.Checked);
    }
}
