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
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Windows.Sdk;

namespace VidDraw {
    internal abstract class CustomMenuForm<TMenuItemId> : Form
            where TMenuItemId : struct, Enum {
        internal CustomMenuForm()
        {
            if (Enum.GetUnderlyingType(typeof(TMenuItemId)) != typeof(uint)) {
                throw new NotSupportedException(
                        "Enum underlying type must be uint (System.UInt32)");
            }

            if (Enum.GetName(UnusedId) != nameof(UnusedId)) {
                throw new NotSupportedException(
                        $"Enum must name 0 as \"{nameof(UnusedId)}\"");
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch ((Native.WM)m.Msg) {
            case Native.WM.SYSCOMMAND
                    when TryAsId((uint)m.WParam) is TMenuItemId id:
                OnMenuItemClick(id);
                break;

            case Native.WM.INITMENU:
                OnMenuOpening();
                break;

            default:
                base.WndProc(ref m);
                break;
            }
        }

        protected internal virtual void OnMenuOpening() { }

        protected internal abstract void OnMenuItemClick(TMenuItemId id);

        protected internal void AddMenuSeparator()
            => PInvoke.AppendMenu(hMenu: MenuHandle,
                                  uFlags: MENU_FLAGS.MF_SEPARATOR,
                                  uIDNewItem: Convert.ToUInt32(UnusedId),
                                  lpNewItem: null);

        protected internal unsafe void AddMenuItem(TMenuItemId uIDNewItem,
                                                   string lpNewItem)
        {
            fixed (char* p = lpNewItem) {
                PInvoke.AppendMenu(hMenu: MenuHandle,
                                   uFlags: MENU_FLAGS.MF_STRING,
                                   uIDNewItem: Convert.ToUInt32(uIDNewItem),
                                   lpNewItem: new(p));
            }
        }

        protected internal unsafe bool HasCheck(TMenuItemId id)
        {
            var mii = new MENUITEMINFOW {
                cbSize = (uint)sizeof(MENUITEMINFOW),
                fMask = MENUITEMINFOA_fMask.MIIM_STATE,
            };

            PInvoke.GetMenuItemInfo(hmenu: MenuHandle,
                                    item: Convert.ToUInt32(id),
                                    fByPosition: false,
                                    &mii);

            return (mii.fState & (uint)MENU_FLAGS.MF_CHECKED) != 0;
        }

        protected internal unsafe void SetCheck(TMenuItemId id, bool @checked)
        {
            var mii = new MENUITEMINFOW {
                cbSize = (uint)sizeof(MENUITEMINFOW),
                fMask = MENUITEMINFOA_fMask.MIIM_STATE,
                fState = (uint)(@checked ? MENU_FLAGS.MF_CHECKED
                                         : MENU_FLAGS.MF_UNCHECKED),
            };

            PInvoke.SetMenuItemInfo(hmenu: MenuHandle,
                                    item: Convert.ToUInt32(id),
                                    fByPositon: false, // Misspelled in API.
                                    &mii);
        }

        protected internal void SetEnabled(TMenuItemId id, bool enabled)
            => PInvoke.EnableMenuItem(
                    hMenu: MenuHandle,
                    uIDEnableItem: Convert.ToUInt32(id),
                    uEnable: (enabled ? MENU_FLAGS.MF_ENABLED
                                      : MENU_FLAGS.MF_GRAYED));

        protected internal unsafe void SetText(TMenuItemId id, string text)
        {
            fixed (char* p = text) {
                var mii = new MENUITEMINFOW {
                    cbSize = (uint)sizeof(MENUITEMINFOW),
                    fMask = MENUITEMINFOA_fMask.MIIM_STRING,
                    dwTypeData = new(p),
                };

                PInvoke.SetMenuItemInfo(
                        hmenu: MenuHandle,
                        item: Convert.ToUInt32(id),
                        fByPositon: false, // Misspelled in API.
                        &mii);
            }
        }

        private static TMenuItemId? TryAsId(uint value)
        {
            var id = (TMenuItemId)Enum.ToObject(typeof(TMenuItemId), value);
            return UsedIds.Contains(id) ? id : null;
        }

        // Note: Renaming this is a breaking change.
        private static TMenuItemId UnusedId => default;

        private static ImmutableArray<TMenuItemId> UsedIds { get; } =
            Enum.GetValues<TMenuItemId>()
                .Where(id => !id.Equals(UnusedId))
                .ToImmutableArray();

        private HMENU MenuHandle
            => PInvoke.GetSystemMenu(hWnd: new(Handle), bRevert: false);
    }
}
