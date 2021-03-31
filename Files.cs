using System;
using System.Diagnostics;
using System.IO;

namespace VidDraw {
    /// <summary>Methods for querying and accessing files.</summary>
    internal static class Files {
        internal static string GetDirectoryOrThrow(this string path)
            => Path.GetDirectoryName(path)
                ?? throw new ArgumentException(
                    paramName: nameof(path),
                    message: "Must include filename, not just folder/drive");

        /// <summary>
        /// Creates a new file with the given filename or one based on it,
        /// ensuring no conflict with any existing file.
        /// </summary>
        /// <param name="preferredPath">The path to try first</param>
        /// <returns>A stream for the file, opened for writing.</returns>
        internal static FileStream CreateWithoutClash(string preferredPath)
        {
            var parts = new DecomposedPath(preferredPath);
            var path = parts.ToString();
            Debug.Assert(StringComparers.Path.Equals(path, preferredPath),
                         "Path doesn't round-trip properly");

            for (; ; ) {
                try {
                    return new FileStream(path, FileMode.CreateNew);
                }
                catch (IOException ex) when (ex.HResult == FileExistsHResult) {
                    parts = parts.Next;
                    path = parts.ToString();
                }
            }
        }

        private const int FileExistsHResult = -2147024816;
    }
}
