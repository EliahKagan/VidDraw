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

        [STAThread]
        private static void Main()
        {
            using var locker = Sync.CreateMutex("main");

            Mutex checker;
            using (var @lock = new Hold(locker))
                checker = Sync.CreateMutex($"checker-{XStyleArch}");

            ToastNotificationManagerCompat.OnActivated +=
                ToastNotificationManagerCompat_OnActivated;

            try {
                Run();
            } finally {
                using var hold = new Hold(locker);
                checker.Dispose();
                checker = Sync.CreateMutex("checker", out var createdNew);
                if (createdNew) ToastNotificationManagerCompat.Uninstall();
                checker.Dispose(); // Not strictly needed, as the OS cleans up.
            }
        }

        // TODO: Consider using SHOpenFolderAndSelectItems instead. See:
        //  - https://stackoverflow.com/q/13680415
        //  - https://docs.microsoft.com/en-us/windows/win32/api/shlobj_core/nf-shlobj_core-shopenfolderandselectitems
        private static void ToastNotificationManagerCompat_OnActivated(
                ToastNotificationActivatedEventArgsCompat e)
            => Process.Start(
                fileName: Path.Combine(Dirs.Windows, "explorer.exe"),
                arguments: $"/select,\"{e.Argument}\"");

        private static void Run()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
