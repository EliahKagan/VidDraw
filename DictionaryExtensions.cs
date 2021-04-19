using System.Collections.Generic;

namespace VidDraw {
    internal static class DictionaryExtensions {
        internal static TValue?
        TryGet<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
                where TValue : struct
            => self.TryGetValue(key, out var value) ? value : null;
    }
}
