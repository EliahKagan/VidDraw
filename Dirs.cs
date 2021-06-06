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

using static System.Environment;

namespace VidDraw {
    /// <summary>Some systemwide and per-user directories.</summary>
    internal static class Dirs {
        internal static string AppData
            => GetFolderPath(SpecialFolder.ApplicationData);

        internal static string System
            => GetFolderPath(Is64BitProcess ? SpecialFolder.System
                                            : SpecialFolder.SystemX86);

        internal static string Videos
            => GetFolderPath(SpecialFolder.MyVideos);

        internal static string Windows
            => GetFolderPath(SpecialFolder.Windows);
    }
}
