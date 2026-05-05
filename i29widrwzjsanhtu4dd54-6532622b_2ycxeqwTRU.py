
import os
import re
import sys

# ── 自动从当前脚本所在目录向上查找解决方案根目录 ──────────────────
def find_solution_root():
    """从脚本所在目录开始，向上查找包含 .sln 文件的目录"""
    start = os.path.dirname(os.path.abspath(__file__))
    current = start
    for _ in range(6):  # 最多向上找6层
        slns = [f for f in os.listdir(current) if f.endswith('.sln')]
        if slns:
            print(f"[✓] 找到解决方案根目录: {current}")
            return current
        parent = os.path.dirname(current)
        if parent == current:
            break
        current = parent
    # 找不到就用脚本所在目录
    print(f"[!] 未找到 .sln，使用脚本所在目录: {start}")
    return start

ROOT = find_solution_root()
print(f"[i] 扫描根目录: {ROOT}\n")

# ── 工具函数 ──────────────────────────────────────────────────────
def read(path):
    with open(path, 'r', encoding='utf-8-sig') as f:
        return f.read()

def write(path, content):
    with open(path, 'w', encoding='utf-8-sig') as f:
        f.write(content)

def find_files(name):
    """在整个解决方案目录下递归查找指定文件名"""
    results = []
    for dirpath, dirnames, filenames in os.walk(ROOT):
        # 跳过 bin/obj 目录
        dirnames[:] = [d for d in dirnames if d not in ('bin', 'obj', '.git')]
        for fn in filenames:
            if fn == name:
                results.append(os.path.join(dirpath, fn))
    return results

changed = []
skipped = []

# ════════════════════════════════════════════════════════════════
# 任务1：删除 PlaceholderText 行
# 目标文件：BlacklistControl.Designer.cs / Step3SettingsPanel.Designer.cs
# ════════════════════════════════════════════════════════════════
placeholder_targets = [
    'BlacklistControl.Designer.cs',
    'Step3SettingsPanel.Designer.cs',
]

for fname in placeholder_targets:
    files = find_files(fname)
    if not files:
        print(f"[!] 未找到文件: {fname}")
        skipped.append(fname)
        continue
    for fpath in files:
        original = read(fpath)
        # 删除包含 PlaceholderText 的整行
        new_content = re.sub(r'[ \t]*.*\.PlaceholderText\s*=\s*[^\n]*\n?', '', original)
        if new_content != original:
            write(fpath, new_content)
            removed = original.count('PlaceholderText') 
            print(f"[✓] 删除 PlaceholderText x{removed} 行: {fpath}")
            changed.append(fpath)
        else:
            print(f"[=] 无需修改(已干净): {fpath}")

# ════════════════════════════════════════════════════════════════
# 任务2：添加 using EmailSender.UI.Common 到 Designer.cs
# 目标文件：EmailListControl.Designer.cs / SettingsControl.Designer.cs / Step2ListPanel.Designer.cs
# ════════════════════════════════════════════════════════════════
using_targets = [
    'EmailListControl.Designer.cs',
    'SettingsControl.Designer.cs',
    'Step2ListPanel.Designer.cs',
]

USING_LINE = 'using EmailSender.UI.Common;'

for fname in using_targets:
    files = find_files(fname)
    if not files:
        print(f"[!] 未找到文件: {fname}")
        skipped.append(fname)
        continue
    for fpath in files:
        original = read(fpath)
        if USING_LINE in original:
            print(f"[=] 已存在 using，跳过: {fpath}")
            continue
        # 在第一个 using 行之前插入，或在文件开头插入
        if 'using ' in original:
            new_content = original.replace('using ', USING_LINE + '\nusing ', 1)
        else:
            new_content = USING_LINE + '\n' + original
        write(fpath, new_content)
        print(f"[✓] 添加 using: {fpath}")
        changed.append(fpath)

# ════════════════════════════════════════════════════════════════
# 任务3：全局扫描所有 Designer.cs，删除残留 PlaceholderText
# ════════════════════════════════════════════════════════════════
print("\n[i] 全局扫描所有 Designer.cs 中的 PlaceholderText...")
for dirpath, dirnames, filenames in os.walk(ROOT):
    dirnames[:] = [d for d in dirnames if d not in ('bin', 'obj', '.git')]
    for fn in filenames:
        if fn.endswith('.Designer.cs'):
            fpath = os.path.join(dirpath, fn)
            original = read(fpath)
            if 'PlaceholderText' in original:
                new_content = re.sub(r'[ \t]*.*\.PlaceholderText\s*=\s*[^\n]*\n?', '', original)
                write(fpath, new_content)
                cnt = original.count('PlaceholderText')
                print(f"[✓] 全局清理 PlaceholderText x{cnt}: {fpath}")
                if fpath not in changed:
                    changed.append(fpath)

# ════════════════════════════════════════════════════════════════
# 汇总
# ════════════════════════════════════════════════════════════════
print(f"\n{'='*55}")
print(f"完成！共修改 {len(changed)} 个文件，跳过 {len(skipped)} 个")
if skipped:
    print("未找到的文件：")
    for s in skipped:
        print(f"  - {s}")
print('='*55)
print("\n⚠️  还需在 Visual Studio 中手动操作：")
print("   右键 EmailSender.UI → 添加引用 → 框架")
print("   → 搜索并勾选 Microsoft.CSharp → 确定")
