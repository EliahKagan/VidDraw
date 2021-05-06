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
                // This happens when we don't have permission to access the
                // other process's metadata. VidDraw only queries processes
                // running in the same session as it, so this is most commonly
                // because the other process is UAC-elevated (but we are not).
            } catch (InvalidOperationException) {
                // This happens if a process exited after being enumerated but
                // before its metadata are accessed through the Process object.
            }

            return null;
        }

        internal static bool HasKnownPath(this Process process,
                                          string fileName)
            => fileName.Equals(process.TryGetPath(), StringComparison.Ordinal);
    }
}
