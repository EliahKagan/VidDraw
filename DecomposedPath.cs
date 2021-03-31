using System;
using System.IO;
using System.Text.RegularExpressions;

namespace VidDraw {
    /// <summary>
    /// Helper type for <see cref="Files.CreateWithoutClash"/>.
    /// </summary>
    internal sealed record DecomposedPath {
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
                RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

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
