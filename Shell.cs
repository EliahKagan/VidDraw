using System.Diagnostics;
using System.IO;

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

        /// <summary>
        /// Opens an Explorer window and selects a single item.
        /// </summary>
        /// <param name="path">A valid path to an item to highlight.</param>
        // TODO: Consider using SHOpenFolderAndSelectItems instead. See:
        //  - https://stackoverflow.com/q/13680415
        //  - https://docs.microsoft.com/en-us/windows/win32/api/shlobj_core/nf-shlobj_core-shopenfolderandselectitems
        internal static void Select(string path)
            => Process.Start(
                fileName: Path.Combine(Dirs.Windows, "explorer.exe"),
                arguments: $"/select,\"{path}\"");
    }
}
