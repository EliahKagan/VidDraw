using System;
using System.Diagnostics;

namespace VidDraw {
    /// <summary>Extensions for comparing processes.</summary>
    internal static class ProcessExtensions {
        internal static string? GetPath(this Process process)
            => process.MainModule?.FileName;

        internal static bool HasPath(this Process process, string fileName)
            => fileName.Equals(process.GetPath(), StringComparison.Ordinal);
    }
}
