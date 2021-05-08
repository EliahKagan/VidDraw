// Copyright (c) 2021 Eliah Kagan
//
// Permission to use, copy, modify, and/or distribute this software for any
// purpose with or without fee is hereby granted.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
// SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION
// OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN
// CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

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
