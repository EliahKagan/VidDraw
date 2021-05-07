using System;

namespace VidDraw {
    /// <summary>Information about the platform we are running on.</summary>
    internal static class Platform {
        internal static string XStyleArch
            => Environment.Is64BitProcess ? "x64" : "x86";

        internal static bool CanToast
            => MinimumToastingVersion <= Environment.OSVersion.Version;

        private static Version MinimumToastingVersion { get; } =
            new(major: 10, minor: 0, build: 11763);
    }
}
