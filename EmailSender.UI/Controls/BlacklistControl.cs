using System;
using System.IO;
using System.Windows.Forms;
using EmailSender.Models;
using EmailSender.UI.Common;

namespace EmailSender.UI.Controls
{
    public partial class BlacklistControl : UserControl
    {
        public BlacklistControl()
        {
            InitializeComponent();
            UIHelper.StyleGrid(dgvBlacklist);
            LoadData();
        }

        private void LoadData()
        {
            dgvBlacklist.DataSource = null;
            dgvBlacklist.DataSource = ServiceLocator.BlacklistService.GetAll();
            lblCount.Text = $"共 {ServiceLocator.BlacklistService.GetCount()} 条";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var email = txtEmail.Text.Trim();
            if (string.IsNullOrEmpty(email))
            { UIHelper.Warn("请输入邮件地址"); return; }

            ServiceLocator.BlacklistService.AddManual(email, txtReason.Text.Trim());
            txtEmail.Clear();
            txtReason.Clear();
            LoadData();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvBlacklist.CurrentRow?.DataBoundItem is not Blacklist b) return;
            if (!UIHelper.Confirm($"确定从黑名单移除 {b.Email}？")) return;
            ServiceLocator.BlacklistService.Remove(b.Email);
            LoadData();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog {
                Title="选择黑名单文件（每行一个邮件地址）",
                Filter="文本文件|*.txt|所有文件|*.*"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                int count = ServiceLocator.BlacklistService.ImportFromFile(dlg.FileName);
                UIHelper.Info($"导入成功，共添加 {count} 条");
                LoadData();
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog {
                Title="导出黑名单",
                Filter="文本文件|*.txt",
                FileName="blacklist_export.txt"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                ServiceLocator.blacklistService.ExportToFile(dlg.FileName);
                UIHelper.Info("导出成功！");
            }
            catch (Exception ex) { UIHelper.Error(ex.Message); }
        }

        private void btnRefresh_Click(object sender, EventArgs e) => LoadData();

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            var kw = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(kw)) { LoadData(); return; }
            var all = ServiceLocator.BlacklistService.GetAll();
            dgvBlacklist.DataSource = all.FindAll(b =>
                b.Email.ToLower().Contains(kw));
        }
    }
}
