using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;

namespace VidDraw {
    internal static class Program {
        internal static string XStyleArch
            => Environment.Is64BitProcess ? "x64" : "x86";

        private static string CheckerName => $"checker-{XStyleArch}";

        [STAThread]
        private static void Main()
        {
            using var locker = Sync.CreateMutex("main");

            Mutex checker;
            using (var @lock = new Hold(locker))
                checker = Sync.CreateMutex(CheckerName);

            ToastNotificationManagerCompat.OnActivated +=
                e => Shell.Select(e.Argument);

            try {
                Run();
            } finally {
                using var hold = new Hold(locker);
                checker.Dispose();
                checker = Sync.CreateMutex(CheckerName, out var createdNew);
                if (createdNew) ToastNotificationManagerCompat.Uninstall();
                checker.Dispose(); // Not strictly needed, as the OS cleans up.
            }
        }

        private static void Run()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
