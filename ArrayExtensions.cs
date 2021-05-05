using System;

namespace VidDraw {
    internal static class ArrayExtensions {
        internal static void CopyToFit<T>(this T[] source, T[] destination)
            => Array.Copy(sourceArray: source,
                          destinationArray: destination,
                          length: Math.Min(source.Length, destination.Length));
    }
}
