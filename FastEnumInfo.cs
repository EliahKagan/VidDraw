using System;
using System.Collections.Generic;
using System.Linq;

namespace VidDraw {
    /// <summary>Methods to get information about enums quickly.</summary>
    /// <typeparam name="T">The enum to provide information about.</typeparam>
    /// <remarks>
    /// Works by caching results of slow reflective operations.
    /// </remarks>
    internal static class FastEnumInfo<T> where T : struct, Enum {
        internal static IEnumerable<T> Values => _values.Select(x => x);

        private static readonly T[] _values = Enum.GetValues<T>();
    }
}
