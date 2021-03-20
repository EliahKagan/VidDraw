using System;
using System.Text.RegularExpressions;

namespace VidDraw {
    internal static class MatchExtensions {
        internal static string? TryGet(this Match match, string name)
            => match.Groups[name] switch {
                { Success: true, Value: string value } => value,
                { Success: false } => null,
            };

        internal static int? TryGetInt32(this Match match, string name)
            => match.TryGet(name) switch {
                string value => int.Parse(value),
                null => null,
            };

        internal static string Get(this Match match, string name)
            => match.TryGet(name)
                ?? throw new ArgumentException(
                        paramName: nameof(name),
                        message: "No capture in named group: " + name);
    }
}
