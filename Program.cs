using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;

namespace VidDraw {
    internal static class Program {
        static Program()
        {
            var path = Me.TryGetPath();
            Debug.Assert(path is not null);
            MyPath = path;
        }

        private static Process Me { get; } = Process.GetCurrentProcess();

        private static string MyPath { get; }

        private static bool InstanceIsUnique
            => !Process.GetProcesses().Any(p => p.SessionId == Me.SessionId
                                             && p.HasKnownPath(MyPath)
                                             && p.Id != Me.Id);

        [STAThread]
        private static void Main()
        {
            using var mutex = Sync.CreateMutex(MyPath);

            // If any VidDraw process may be in the middle of shutting down,
            // wait for it before proceeding, so that if it uninstalls toasts,
            // it does so before any of this instance's toasts are dispatched.
            using (var @lock = new Lock(mutex)) { }

            ToastNotificationManagerCompat.OnActivated +=
                ToastNotificationManagerCompat_OnActivated;

            try {
                Run();
            } finally {
                using var @lock = new Lock(mutex);

                if (InstanceIsUnique)
                    ToastNotificationManagerCompat.Uninstall();
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
