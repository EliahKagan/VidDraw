using System.Collections.Generic;
using System.Linq;

namespace VidDraw {
    /// <summary>Additional LINQ-to-objects operators.</summary>
    internal static class EnumerableExtensions {
        internal static IEnumerable<TSource>
        Without<TSource>(this IEnumerable<TSource> source, TSource key)
            => source.Without(key, EqualityComparer<TSource>.Default);

        internal static IEnumerable<TSource>
        Without<TSource>(this IEnumerable<TSource> source, TSource key,
                         IEqualityComparer<TSource> comparer)
            => source.Where(item => !comparer.Equals(item, key));
    }
}
