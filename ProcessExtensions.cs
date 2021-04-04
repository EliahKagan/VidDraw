using System;
using System.ComponentModel;
using System.Diagnostics;

namespace VidDraw {
    /// <summary>Extensions for comparing processes.</summary>
    internal static class ProcessExtensions {
        internal static string? TryGetPath(this Process process)
        {
            try {
                return process.MainModule?.FileName;
            } catch (Win32Exception) {
                return null;
            }
        }

        internal static bool HasKnownPath(this Process process,
                                          string fileName)
            => fileName.Equals(process.TryGetPath(), StringComparison.Ordinal);
    }
}
