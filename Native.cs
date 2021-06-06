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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace VidDraw {
    /// <summary>Native methods (via P/Invoke) and supporting types.</summary>
    internal static class Native {
        internal const int MAX_PATH = 260;

        /// <summary>Menu flags ("MF_") constants.</summary>
        [Flags]
        internal enum MF : uint {
            GRAYED    = 0x0001,
            CHECKED   = 0x0008,
            STRING    = 0x0000,
            SEPARATOR = 0x0800,
        }

        /// <summary>Menu item info member ("MIIM_") constants.</summary>
        [Flags]
        internal enum MIIM : uint {
            STATE  = 0x0001,
            STRING = 0x0040,
        }

        /// <summary>Window message ("WM_") constants.</summary>
        internal enum WM : uint {
            INITDIALOG = 0x0110,
            SYSCOMMAND = 0x0112,
            INITMENU   = 0x0116,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct MENUITEMINFO {
            internal MENUITEMINFO(MIIM fMask)
            {
                // Most fields will start with their types' default values.
                this = default;

                // There is only one correct value for the size, so set it.
                cbSize = (uint)Marshal.SizeOf<MENUITEMINFO>();

                // Set the mask that determines which fields to use and how.
                this.fMask = fMask;
            }

            internal readonly uint cbSize;

            internal readonly MIIM fMask;

            internal uint fType;

            internal MF fState;

            internal uint wID;

            internal nint hSubMenu;

            internal nint hbmpChecked;

            internal nint hbmpUnchecked;

            internal nuint dwItemData;

            [MarshalAs(UnmanagedType.LPWStr)]
            internal string? dwTypeData;

            internal uint cch;

            internal nint hbmpItem;
        }

        [DllImport("shell32", CharSet = CharSet.Unicode)]
        internal static extern nint FindExecutable(string lpFile,
                                                   string? lpDirectory,
                                                   StringBuilder lpResult);

        [DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool AppendMenu(nint hMenu,
                                               MF uFlags,
                                               nuint uIDNewItem,
                                               string? lpNewItem);

        [DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool GetMenuItemInfo(nint hmenu,
                                                    uint item,
                                                    bool fByPosition,
                                                    ref MENUITEMINFO lpmii);

        [DllImport("user32")]
        internal static extern nint GetSystemMenu(nint hWnd, bool bRevert);

        [DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool SetMenuItemInfo(nint hmenu,
                                                    uint item,
                                                    bool fByPosition,
                                                    ref MENUITEMINFO lpmii);

        [DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool SetWindowText(nint hWnd, string lpString);

        internal static void ThrowLastError()
            => throw new Win32Exception(Marshal.GetLastWin32Error());
    }
}
