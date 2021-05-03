using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace VidDraw {
    /// <summary>
    /// Methods for synchronizing with other VidDraw application instances.
    /// </summary>
    internal static class Sync {
        internal static Mutex CreateMutex(string label)
            => new(initiallyOwned: false, $"{Uuid}-{Hash(label)}");

        private const string Uuid = "e3f36cbf-64b5-4cd8-b334-f24bf69a65c9";

        private static string Hash(string label)
            => string.Concat(SHA256.HashData(Encoding.UTF8.GetBytes(label))
                                   .Select(octet => octet.ToString("x2")));
    }
}
