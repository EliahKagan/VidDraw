using static System.Environment;

namespace VidDraw {
    /// <summary>Some systemwide and per-user directories.</summary>
    internal static class Dirs {
        internal static string AppData
            => GetFolderPath(SpecialFolder.ApplicationData);

        // FIXME: Check that SpecialFolder.SystemX86 works even when
        //        Is64BitOperatingSystem is false.
        internal static string System
            => GetFolderPath(Is64BitProcess ? SpecialFolder.System
                                            : SpecialFolder.SystemX86);

        internal static string Windows
            => GetFolderPath(SpecialFolder.Windows);
    }
}
