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
