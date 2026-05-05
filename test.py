import os
import subprocess
import sys

# ══════════════════════════════════════════════════════════════
# fix_nuget_packages.py
# 1. 为 EmailSender.Data 写入 packages.config
# 2. 修复 EmailSender.Data.csproj 的 HintPath 引用
# 3. 补充生成缺失的 EmailListControl.cs 文件
# ══════════════════════════════════════════════════════════════

# ── Step 1: 写入 packages.config ──────────────────────────────
packages_config = """\
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Dapper"             version="2.1.28"  targetFramework="net48" />
  <package id="System.Data.SQLite" version="1.0.118" targetFramework="net48" />
  <package id="MySql.Data"         version="8.3.0"   targetFramework="net48" />
  <package id="Newtonsoft.Json"    version="13.0.3"  targetFramework="net48" />
</packages>
"""
pkg_path = "EmailSender.Data/packages.config"
with open(pkg_path, "w", encoding="utf-8") as f:
    f.write(packages_config)
print(f"  ✅ 已生成: {pkg_path}")

# ── Step 2: 修复 EmailSender.Data.csproj HintPath ─────────────
csproj_path = "EmailSender.Data/EmailSender.Data.csproj"
if os.path.exists(csproj_path):
    with open(csproj_path, "r", encoding="utf-8") as f:
        content = f.read()

    # 检查是否已有 Dapper 引用
    if "Dapper" not in content:
        ref_block = """\
  <ItemGroup>
    <Reference Include="Dapper">
      <HintPath>..\\packages\\Dapper.2.1.28\\lib\\net461\\Dapper.dll</HintPath>
    </Reference>
    <Reference Include="System.Data.SQLite">
      <HintPath>..\\packages\\System.Data.SQLite.1.0.118\\lib\\net46\\System.Data.SQLite.dll</HintPath>
    </Reference>
    <Reference Include="MySql.Data">
      <HintPath>..\\packages\\MySql.Data.8.3.0\\lib\\net48\\MySql.Data.dll</HintPath>
    </Reference>
    <Reference Include="Newtonsoft.Json">
      <HintPath>..\\packages\\Newtonsoft.Json.13.0.3\\lib\\net45\\Newtonsoft.Json.dll</HintPath>
    </Reference>
    <Reference Include="System.Data" />
  </ItemGroup>
"""
        # 插入到 </ItemGroup> 最后一个之前（Import 之前）
        insert_marker = "  <Import Project=\"$(MSBuildToolsPath)"
        content = content.replace(insert_marker, ref_block + "  " + insert_marker.lstrip())
        with open(csproj_path, "w", encoding="utf-8") as f:
            f.write(content)
        print(f"  ✅ 已修复 csproj 引用: {csproj_path}")
    else:
        print(f"  ✅ csproj 已有引用，跳过: {csproj_path}")

# ── Step 3: 补充生成缺失的 EmailListControl.cs ─────────────────
ctrl_dir = "EmailSender.UI/Controls"
os.makedirs(ctrl_dir, exist_ok=True)

missing_files = {}

missing_files["EmailListControl.cs"] = """\
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
"""

missing_files["EmailListControl.Designer.cs"] = """\
using System.Drawing;
using System.Windows.Forms;

namespace EmailSender.UI.Controls
{
    partial class EmailListControl
    {
        private System.ComponentModel.IContainer components = null;
        private Panel        panelToolbar;
        private ComboBox     cmbList;
        private Button       btnDownload;
        private Button       btnRefreshStats;
        private Label        lblStatus;
        private DataGridView dgvDomain;
        private Panel        panelStats;
        private Label        lblTotal;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;

            panelToolbar = new Panel {
                Dock=DockStyle.Top, Height=46,
                BackColor=Color.White, Padding=new Padding(5,8,5,0)
            };

            cmbList = new ComboBox {
                Width=260, DropDownStyle=ComboBoxStyle.DropDownList,
                Font=new Font("微软雅黑",9f)
            };

            Button MakeBtn(string text, System.Drawing.Color? bg=null) {
                var b = new Button {
                    Text=text, Height=30, AutoSize=true,
                    Padding=new Padding(10,0,10,0),
                    FlatStyle=FlatStyle.Flat, Cursor=Cursors.Hand,
                    Font=new Font("微软雅黑",9f)
                };
                if (bg.HasValue) {
                    b.BackColor=bg.Value; b.ForeColor=Color.White;
                    b.FlatAppearance.BorderSize=0;
                }
                return b;
            }

            btnDownload     = MakeBtn("⬇ 下载成员", Color.FromArgb(31,73,125));
            btnRefreshStats = MakeBtn("↺ 刷新统计");
            lblStatus       = new Label {
                Text="", ForeColor=Color.Gray,
                AutoSize=true, Padding=new Padding(8,8,0,0)
            };

            btnDownload.Click     += btnDownload_Click;
            btnRefreshStats.Click += btnRefreshStats_Click;

            var flow = new FlowLayoutPanel {
                Dock=DockStyle.Fill, FlowDirection=FlowDirection.LeftToRight,
                WrapContents=false
            };
            flow.Controls.AddRange(new Control[]{
                cmbList, btnDownload, btnRefreshStats, lblStatus
            });
            panelToolbar.Controls.Add(flow);

            panelStats = new Panel {
                Dock=DockStyle.Top, Height=36,
                BackColor=Color.FromArgb(240,248,255),
                Padding=new Padding(12,8,0,0)
            };
            lblTotal = new Label {
                Text="有效地址：0 个",
                Font=new Font("微软雅黑",10f,System.Drawing.FontStyle.Bold),
                ForeColor=Color.FromArgb(31,73,125),
                AutoSize=true
            };
            panelStats.Controls.Add(lblTotal);

            dgvDomain = new DataGridView { Dock=DockStyle.Fill };
            UIHelper.StyleGrid(dgvDomain);

            this.Controls.Add(dgvDomain);
            this.Controls.Add(panelStats);
            this.Controls.Add(panelToolbar);
        }
    }
}
"""

for filename, content in missing_files.items():
    path = os.path.join(ctrl_dir, filename)
    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"  ✅ 已生成: {path}")

# ── Step 4: 尝试用 nuget.exe 还原包 ───────────────────────────
print()
print("━" * 50)
print("📦 尝试自动还原 NuGet 包...")
nuget_cmd = "nuget restore EmailSender.sln"
result = subprocess.run(nuget_cmd, shell=True, capture_output=True, text=True)
if result.returncode == 0:
    print("  ✅ NuGet 包还原成功！")
else:
    print("  ⚠️  nuget.exe 未找到或还原失败，请手动在 VS 中还原")
    print("     方法：右键解决方案 → 【还原 NuGet 包】")

print()
print("━" * 50)
print("🎉 修复完成！后续步骤：")
print("  1. 在 Visual Studio 中右键解决方案 → 【还原 NuGet 包】")
print("  2. 右键解决方案 → 【重新加载所有项目】")
print("  3. Ctrl+Shift+B 重新生成解决方案")