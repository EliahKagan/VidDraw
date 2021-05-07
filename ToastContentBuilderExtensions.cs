using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Toolkit.Uwp.Notifications;

namespace VidDraw {
    /// <summary>
    /// Extensions to "degrade gracefully" on platforms without toast support.
    /// </summary>
    internal static class ToastContentBuilderExtensions {
        internal static void ShowOr(this ToastContentBuilder builder,
                                    Action fallback)
        {
            try {
                builder.Show();
            } catch (Exception ex) when (ex is EntryPointNotFoundException
                                            or TargetInvocationException) {
                Debug.Print($"Can't toast (not Windows 10?): {ex.Message}");
                fallback();
            }
        }
    }
}
