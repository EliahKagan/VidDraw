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
using System.Text.RegularExpressions;

namespace VidDraw {
    /// <summary>Extensions for clearer and more compact regex usage.</summary>
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
