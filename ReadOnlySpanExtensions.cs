using System;

namespace VidDraw {
    /// <summary>
    /// Extensions for spans, with stricter error checking for special cases.
    /// </summary>
    internal static class ReadOnlySpanExtensions {
        internal static void CopyToSameSize<T>(this ReadOnlySpan<T> source,
                                               Span<T> destination)
        {
            if (source.Length != destination.Length) {
                throw new ArgumentException(
                        paramName: nameof(destination),
                        message: "Destination not same length as source");
            }

            source.CopyTo(destination);
        }
    }
}
