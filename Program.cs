using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;

namespace VidDraw {
    internal static class Program {
        private static string WinDir
            => Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        [STAThread]
        private static void Main()
        {
            ToastNotificationManagerCompat.OnActivated +=
                ToastNotificationManagerCompat_OnActivated;

            try {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainWindow());
            } finally {
                ToastNotificationManagerCompat.Uninstall();
            }
        }

        private static void ToastNotificationManagerCompat_OnActivated(
                ToastNotificationActivatedEventArgsCompat e)
            => Process.Start(fileName: Path.Combine(WinDir, "explorer.exe"),
                             arguments: $"/select,{e.Argument}");
    }
}
