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
using System.Diagnostics.CodeAnalysis;
using static System.IO.Path;

namespace VidDraw {
    /// <summary>
    /// Special-purpose <see cref="StringComparer"/> implementations.
    /// </summary>
    internal static class StringComparers {
        internal static StringComparer Path { get; } = new PathComparer();

        private sealed class PathComparer : StringComparer {
            public override int Compare(string? lhs, string? rhs)
                => Ordinal.Compare(Fold(lhs), Fold(rhs));

            public override bool Equals(string? lhs, string? rhs)
                => Ordinal.Equals(Fold(lhs), Fold(rhs));

            public override int GetHashCode(string path)
                => Ordinal.GetHashCode(Fold(path));

            [return: NotNullIfNotNull("path")]
            private static string? Fold(string? path)
                => path?.Replace(oldChar: AltDirectorySeparatorChar,
                                 newChar: DirectorySeparatorChar);
        }
    }
}
