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

using System.IO;
using System.Reflection;

namespace VidDraw {
    /// <summary>Paths specific to this program or an instance of it.</summary>
    internal static class MyPaths {
        /// <summary>The path to the help file shown in the About box.</summary>
        internal static string HelpFile { get; } =
            Path.Combine(DocDir, "index.html");

        private static string DocDir => Path.Combine(ExeDir, "doc");

        private static string ExeDir => Files.GetDirectoryOrThrow(ExeFile);

        private static string ExeFile
            => Assembly.GetExecutingAssembly().Location;
    }
}
