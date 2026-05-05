using System;
using System.Linq;
using System.Windows.Forms;
using EmailSender.UI.Common;

namespace EmailSender.UI.Controls
{
    public partial class EmailListControl : UserControl
    {
        private int    _selectedListId;
        private string _selectedListName;

        public EmailListControl()
        {
            InitializeComponent();
            LoadLists();
        }

        private async void LoadLists()
        {
            try
            {
                var lists = await ServiceLocator.MeetbyApi.GetEmailListsAsync();
                cmbList.DataSource    = lists;
                cmbList.DisplayMember = "ListName";
                cmbList.ValueMember   = "ListId";
            }
            catch (Exception ex) { UIHelper.Error($"获取列表失败：{ex.Message}"); }
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            if (cmbList.SelectedItem == null) return;

            dynamic item      = cmbList.SelectedItem;
            _selectedListId   = (int)item.ListId;
            _selectedListName = item.ListName?.ToString();

            btnDownload.Enabled = false;
            lblStatus.Text      = "正在下载...";

            var progress = new Progress<(int current, int total)>(p =>
                UIHelper.InvokeIfNeeded(this, () =>
                    lblStatus.Text = $"下载中 {p.current}/{p.total}"));
            try
            {
                int count = await ServiceLocator.EmailListService
                    .DownloadAndSaveAsync(_selectedListId, _selectedListName, progress);
                lblStatus.Text = $"下载完成，共 {count} 个地址";
                RefreshStats();
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); lblStatus.Text = ""; }
            finally { btnDownload.Enabled = true; }
        }

        private void RefreshStats()
        {
            if (_selectedListId == 0) return;
            int total  = ServiceLocator.EmailListService.GetCount(_selectedListId);
            var domain = ServiceLocator.EmailListService.GetDomainStats(_selectedListId);

            lblTotal.Text = $"有效地址：{total} 个";
            dgvDomain.DataSource = domain
                .Select(kv => new { 域名 = kv.Key, 数量 = kv.Value })
                .OrderByDescending(x => x.数量)
                .ToList();
        }

        private void btnRefreshStats_Click(object sender, EventArgs e) => RefreshStats();
    }
}
