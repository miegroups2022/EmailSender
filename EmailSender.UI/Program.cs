using System;
using System.Windows.Forms;
using EmailSender.Data;
using EmailSender.UI.Common;
using EmailSender.UI.Forms;

namespace EmailSender.UI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // 初始化数据库
                DatabaseHelper.Initialize();

                // 初始化服务容器
                ServiceLocator.Initialize();

                // 尝试恢复上次登录会话
                var auth = ServiceLocator.authService;
                if (auth.TryRestoreSession())
                {
                    Application.Run(new MainForm());
                }
                else
                {
                    Application.Run(new LoginForm());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"程序启动失败：{ex.Message}\n\n{ex.StackTrace}",
                    "启动错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
