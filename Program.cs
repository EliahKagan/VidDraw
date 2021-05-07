using System;
using System.Windows.Forms;

namespace VidDraw {
    internal static class Program {
        [STAThread]
        private static void Main()
        {
            using var coordinator =
                ToastCoordinator.TryCreate(e => Shell.Select(e.Argument));

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
