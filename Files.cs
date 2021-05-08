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
using System.Text.RegularExpressions;
using static System.Text.RegularExpressions.RegexOptions;

namespace VidDraw {
    /// <summary>Methods for querying and accessing files.</summary>
    internal static class Files {
        internal static string GetDirectoryOrThrow(this string path)
            => Path.GetDirectoryName(path)
                ?? throw new ArgumentException(
                    paramName: nameof(path),
                    message: "Must include filename, not just folder/drive");

        /// <summary>
        /// Creates a new file with the given filename or one based on it,
        /// ensuring no conflict with any existing file.
        /// </summary>
        /// <param name="preferredPath">The path to try first</param>
        /// <returns>A stream for the file, opened for writing.</returns>
        internal static FileStream CreateWithoutClash(string preferredPath)
        {
            var parts = new DecomposedPath(preferredPath);
            var path = parts.ToString();
            Debug.Assert(StringComparers.Path.Equals(path, preferredPath),
                         "Path doesn't round-trip properly");

            for (; ; ) {
                try {
                    return new FileStream(path, FileMode.CreateNew);
                }
                catch (IOException ex) when (ex.HResult == FileExistsHResult) {
                    parts = parts.Next;
                    path = parts.ToString();
                }
            }
        }

        private const int FileExistsHResult = -2147024816;

        private sealed record DecomposedPath {
            internal DecomposedPath(string path)
            {
                Directory = path.GetDirectoryOrThrow();

                var match = MatchFilename(path);

                Prefix = match.Get("prefix");
                Number = match.TryGetInt32("number");
                Suffix = match.Get("suffix");
            }

            public string Directory { get; init; }

            public string Prefix { get; init; }

            public int? Number { get; init; }

            public string Suffix { get; init; }

            public override string ToString()
                => Path.Combine(Directory, BuildFilename());

            internal DecomposedPath Next => this with {
                Number = (Number ?? 1) + 1,
            };

            private static Regex FilenameParser { get; } =
                new(@"^(?<prefix>.+?)
                       (?:[ ]\((?<number>\d+)\))?
                       (?<suffix>(?:\.[^.]+)?)$",
                    Compiled | IgnorePatternWhitespace);

            private static Match MatchFilename(string path)
            {
                var match = FilenameParser.Match(Path.GetFileName(path));

                if (!match.Success) {
                    throw new ArgumentException(
                            paramName: nameof(path),
                            message: "Can't parse filename");
                }

                return match;
            }

            private string BuildFilename() => Number switch {
                int number => $"{Prefix} ({number}){Suffix}",
                null => Prefix + Suffix,
            };
        }
    }
}
