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

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace VidDraw {
    /// <summary>
    /// Convenience methods for interacting with the Windows shell.
    /// </summary>
    internal static class Shell {
        /// <summary>
        /// Opens one item in whatever application would open another item.
        /// </summary>
        /// <param name="pathToOpen">The item to actually open.</param>
        /// <param name="pathToConsult">
        /// An item the shell would automatically open in the application one
        /// wishes to open <c>pathToOpen</c>.
        /// </param>
        /// <remarks>
        /// The items need not be actual paths in the filesystem, so long as
        /// the shell knows how to open <c>pathToConsult</c> and the
        /// application it would use for it also knows how to (try to) open or
        /// create <c>pathToOpen</c>.
        /// </remarks>
        internal static void OpenLike(this string pathToOpen,
                                      string pathToConsult)
            => Process.Start(fileName: FindExecutable(pathToConsult),
                             arguments: new[] { pathToOpen });

        /// <summary>
        /// Run or open a program or other file through the shell.
        /// </summary>
        /// <param name="path">A name of the item to open or run.</param>
        /// <remarks>
        /// Ultimately uses <c>ShellExecute</c> or <c>ShellExecuteEx</c>. See
        /// <a href="https://docs.microsoft.com/en-us/windows/win32/shell/launch">Launching Applications</a>.
        /// </remarks>
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

        private static string FindExecutable(string path)
        {
            var buffer = new StringBuilder(capacity: Native.MAX_PATH + 1);

            var hInstance = Native.FindExecutable(lpFile: path,
                                                  lpDirectory: null,
                                                  lpResult: buffer);

            if (hInstance <= 32) {
                throw new IOException(
                    $"Can't find executable that would open \"{path}\"; "
                    + $"error code {hInstance}");
            }

            return buffer.ToString();
        }
    }
}
