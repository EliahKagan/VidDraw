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

using System.Threading;

namespace VidDraw {
    /// <summary>
    /// Methods for synchronizing with other VidDraw application instances.
    /// </summary>
    internal static class Sync {
        internal static Mutex CreateMutex(string label)
            => new(initiallyOwned: false, PrependUuid(label));

        internal static Mutex CreateMutex(string label, out bool createdNew)
            => new(initiallyOwned: false, PrependUuid(label), out createdNew);

        private const string Uuid = "e3f36cbf-64b5-4cd8-b334-f24bf69a65c9";

        private static string PrependUuid(string label) => $"{Uuid}-{label}";
    }
}
