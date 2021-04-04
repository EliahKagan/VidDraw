using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;

namespace VidDraw {
    internal static class Program {
        private const string Uuid = "e3f36cbf-64b5-4cd8-b334-f24bf69a65c9";

        static Program()
        {
            var path = Me.TryGetPath();
            Debug.Assert(path is not null);
            MyPath = path;
        }

        private static string WinDir
            => Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        private static Process Me { get; } = Process.GetCurrentProcess();

        private static string MyPath { get; }

        private static string MyPathHash
            => string.Concat(new SHA1Managed()
                                .ComputeHash(Encoding.UTF8.GetBytes(MyPath))
                                .Select(octet => octet.ToString("x2")));

        private static bool InstanceIsUnique
            => !Process.GetProcesses().Any(p => p.SessionId == Me.SessionId
                                             && p.HasKnownPath(MyPath)
                                             && p.Id != Me.Id);

        [STAThread]
        private static void Main()
        {
            using var mutex = CreateMutex();
            mutex.Acquire();
            mutex.ReleaseMutex();

            ToastNotificationManagerCompat.OnActivated +=
                ToastNotificationManagerCompat_OnActivated;

            try {
                Run();
            } finally {
                mutex.Acquire();
                try {
                    if (InstanceIsUnique)
                        ToastNotificationManagerCompat.Uninstall();
                } finally {
                    mutex.ReleaseMutex();
                }
            }
        }

        private static Mutex CreateMutex()
            => new Mutex(initiallyOwned: false, $"{Uuid}-{MyPathHash}");

        private static void ToastNotificationManagerCompat_OnActivated(
                ToastNotificationActivatedEventArgsCompat e)
            => Process.Start(fileName: Path.Combine(WinDir, "explorer.exe"),
                             arguments: $"/select,{e.Argument}");

        private static void Run()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
