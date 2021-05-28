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
using System.Windows.Forms;

namespace VidDraw {
    internal static class Program {
        /// <summary>
        /// Hack to suppress behavior in custom controls that is needed at
        /// runtime but cannot succeed at design time.
        /// </summary>
        /// <remarks>
        /// See <a href="https://stackoverflow.com/a/7281838"/> by
        /// <a href="https://stackoverflow.com/users/382783/boris-b"/>.
        /// </remarks>
        internal static bool IsRunning { get; private set; } = false;

        [STAThread]
        private static void Main()
        {
            IsRunning = true;

            using var coordinator =
                ToastCoordinator.TryCreate(e => Shell.Select(e.Argument));

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
