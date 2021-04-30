namespace VidDraw {
    /// <summary>Native types/methods not covered by CsWin32.</summary>
    internal static class Native {
        /// <summary>Window message ("WM_") constants.</summary>
        internal enum WM : uint {
            INITDIALOG = 0x0110,
            SYSCOMMAND = 0x0112,
            INITMENU   = 0x0116,
        }
    }
}
