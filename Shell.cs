using System.Diagnostics;

namespace VidDraw {
    /// <summary>
    /// Convenience methods for interacting with the Windows shell.
    /// </summary>
    internal static class Shell {
        internal static void Execute(string path)
            => Process.Start(new ProcessStartInfo() {
                FileName = path,
                UseShellExecute = true,
            });
    }
}
