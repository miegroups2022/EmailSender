import os

# ══════════════════════════════════════════════════════════════
# generate_EmailSender_sln.py
# 生成 Visual Studio 2019/2022 解决方案文件
# 项目 GUID 与各 csproj 中保持一致
# ══════════════════════════════════════════════════════════════

sln_content = """\
\xef\xbb\xbf
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "EmailSender.Models", "EmailSender.Models\\EmailSender.Models.csproj", "{11111111-1111-1111-1111-111111111111}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "EmailSender.Data", "EmailSender.Data\\EmailSender.Data.csproj", "{22222222-2222-2222-2222-222222222222}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "EmailSender.Core", "EmailSender.Core\\EmailSender.Core.csproj", "{33333333-3333-3333-3333-333333333333}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "EmailSender.UI", "EmailSender.UI\\EmailSender.UI.csproj", "{44444444-4444-4444-4444-444444444444}"
EndProject
Global
\tGlobalSection(SolutionConfigurationPlatforms) = preSolution
\t\tDebug|Any CPU = Debug|Any CPU
\t\tRelease|Any CPU = Release|Any CPU
\tEndGlobalSection
\tGlobalSection(ProjectConfigurationPlatforms) = postSolution
\t\t{11111111-1111-1111-1111-111111111111}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
\t\t{11111111-1111-1111-1111-111111111111}.Debug|Any CPU.Build.0 = Debug|Any CPU
\t\t{11111111-1111-1111-1111-111111111111}.Release|Any CPU.ActiveCfg = Release|Any CPU
\t\t{11111111-1111-1111-1111-111111111111}.Release|Any CPU.Build.0 = Release|Any CPU
\t\t{22222222-2222-2222-2222-222222222222}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
\t\t{22222222-2222-2222-2222-222222222222}.Debug|Any CPU.Build.0 = Debug|Any CPU
\t\t{22222222-2222-2222-2222-222222222222}.Release|Any CPU.ActiveCfg = Release|Any CPU
\t\t{22222222-2222-2222-2222-222222222222}.Release|Any CPU.Build.0 = Release|Any CPU
\t\t{33333333-3333-3333-3333-333333333333}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
\t\t{33333333-3333-3333-3333-333333333333}.Debug|Any CPU.Build.0 = Debug|Any CPU
\t\t{33333333-3333-3333-3333-333333333333}.Release|Any CPU.ActiveCfg = Release|Any CPU
\t\t{33333333-3333-3333-3333-333333333333}.Release|Any CPU.Build.0 = Release|Any CPU
\t\t{44444444-4444-4444-4444-444444444444}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
\t\t{44444444-4444-4444-4444-444444444444}.Debug|Any CPU.Build.0 = Debug|Any CPU
\t\t{44444444-4444-4444-4444-444444444444}.Release|Any CPU.ActiveCfg = Release|Any CPU
\t\t{44444444-4444-4444-4444-444444444444}.Release|Any CPU.Build.0 = Release|Any CPU
\tEndGlobalSection
\tGlobalSection(SolutionProperties) = preSolution
\t\tHideSolutionNode = FALSE
\tEndGlobalSection
\tGlobalSection(ExtensibilityGlobals) = postSolution
\t\tSolutionGuid = {55555555-5555-5555-5555-555555555555}
\tEndGlobalSection
EndGlobal
"""

# 写入 sln 文件（UTF-8 with BOM，VS 要求）
output_path = "EmailSender.sln"
with open(output_path, "w", encoding="utf-8-sig") as f:
    # 第一行的 BOM 已由 utf-8-sig 处理，去掉内容里手动写的 BOM 占位
    content = sln_content.lstrip("\xef\xbb\xbf").lstrip()
    f.write("\n" + content)

print(f"  ✅ 已生成: {output_path}")
print()
print("解决方案包含项目：")
projects = [
    ("EmailSender.Models", "{11111111-1111-1111-1111-111111111111}", "数据模型层"),
    ("EmailSender.Data",   "{22222222-2222-2222-2222-222222222222}", "数据访问层"),
    ("EmailSender.Core",   "{33333333-3333-3333-3333-333333333333}", "业务逻辑层"),
    ("EmailSender.UI",     "{44444444-4444-4444-4444-444444444444}", "UI 表现层（启动项目）"),
]
for name, guid, desc in projects:
    print(f"  📦 {name:<25} {guid}  ← {desc}")

print()
print("使用方式：")
print("  双击 EmailSender.sln 用 Visual Studio 打开")
print("  右键 EmailSender.UI → 设为启动项目")
print("  Ctrl+Shift+B 生成解决方案")