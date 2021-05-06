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

        private static string PrependUuid(string label) => $"{Uuid}-{label}";

        private const string Uuid = "e3f36cbf-64b5-4cd8-b334-f24bf69a65c9";
    }
}
