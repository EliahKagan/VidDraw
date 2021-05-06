using System.Threading;

namespace VidDraw {
    /// <summary>
    /// Methods for synchronizing with other VidDraw application instances.
    /// </summary>
    internal static class Sync {
        internal static Mutex CreateMutex(string label)
            => new(initiallyOwned: false, $"{Uuid}-{label}");

        internal static Mutex CreateMutex(string label, out bool createdNew)
            => new(initiallyOwned: false, $"{Uuid}-{label}", out createdNew);

        private const string Uuid = "e3f36cbf-64b5-4cd8-b334-f24bf69a65c9";
    }
}
