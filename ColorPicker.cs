using System;
using System.Windows.Forms;
using Microsoft.Windows.Sdk;

namespace VidDraw {
    /// <summary>A pen-color picker.</summary>
    /// <remarks>A <see cref="ColorDialog"/> with a custom title.</remarks>
    internal sealed class ColorPicker : ColorDialog {
        protected override IntPtr
        HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            if ((Native.WM)msg is Native.WM.INITDIALOG)
                PInvoke.SetWindowText(new(hWnd), "Choose a pen color");

            return base.HookProc(hWnd, msg, wparam, lparam);
        }
    }
}
