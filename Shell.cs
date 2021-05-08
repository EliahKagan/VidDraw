// Copyright (c) 2021 Eliah Kagan
//
// Permission to use, copy, modify, and/or distribute this software for any
// purpose with or without fee is hereby granted.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
// SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION
// OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN
// CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

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
