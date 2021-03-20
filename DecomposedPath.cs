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
            Directory = GetDirectory(path);

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

        private static Regex FilenameParser { get; } =
            new(@"^(?<prefix>.+?)
                   (?:[ ]\((?<number>\d+)\))?
                   (?<suffix>(?:\.[^.]+)?)$",
                RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        private static string GetDirectory(string path)
            => Path.GetDirectoryName(path)
                ?? throw new ArgumentException(
                    paramName: nameof(path),
                    message: "Must include filename, not just folder/drive");

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
