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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VidDraw {
    /// <summary>Augmentable system menu wrapper.</summary>
    /// <typeparam name="TMenuItemId">
    /// The enumeration of new menu ids. Must contain 0 as <c>UnusedId</c>.
    /// </typeparam>
    /// <remarks>
    /// This offers an API whose structure reflects the implementation, with
    /// methods to add and query menu items and events hooked into
    /// <see cref="Form.WndProc"/>. For a more convenient API, see also
    /// <seealso cref="SystemMenu"/>.
    /// </remarks>
    internal sealed class RawSystemMenu<TMenuItemId>
            where TMenuItemId : struct, Enum {
        internal sealed class ItemClickEventArgs : EventArgs {
            internal ItemClickEventArgs(TMenuItemId id) => Id = id;

            internal TMenuItemId Id { get; }
        }

        internal RawSystemMenu(HookForm form)
        {
            CheckTypeParameter();
            _form = form;
            form.ReceivedMessage += WndProcHook;
        }

        internal event EventHandler? Opening;

        internal event EventHandler<ItemClickEventArgs>? ItemClick;

        internal void AddSeparator()
            => DoAppendMenu(Native.MF.SEPARATOR, UnusedId, null);

        internal void AddItem(TMenuItemId uIDNewItem, string lpNewItem)
        {
            EnsureUsedId(uIDNewItem, nameof(uIDNewItem));
            DoAppendMenu(Native.MF.STRING, uIDNewItem, lpNewItem);
        }

        internal bool HasCheck(TMenuItemId item)
        {
            var mii = new Native.MENUITEMINFO(fMask: Native.MIIM.STATE);
            DoGetMenuItemInfo(item, ref mii);
            return mii.fState.HasFlag(Native.MF.CHECKED);
        }

        internal void SetCheck(TMenuItemId item, bool @checked)
            => SetStateFlag(item, Native.MF.CHECKED, @checked);

        internal void SetEnabled(TMenuItemId item, bool enabled)
            => SetStateFlag(item, Native.MF.GRAYED, !enabled);

        internal void SetText(TMenuItemId item, string text)
        {
            var mii = new Native.MENUITEMINFO(fMask: Native.MIIM.STRING) {
                dwTypeData = text,
            };

            DoSetMenuItemInfo(item, ref mii);
        }

        // Note: Renaming this is a breaking change.
        private static TMenuItemId UnusedId => default;

        private static ImmutableArray<TMenuItemId> UsedIds { get; } =
            Enum.GetValues<TMenuItemId>()
                .Where(id => !id.Equals(UnusedId))
                .ToImmutableArray();

        private static void CheckTypeParameter()
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

        private static void EnsureUsedId(TMenuItemId id, string paramName)
        {
            if (!UsedIds.Contains(id)) {
                throw new ArgumentException(
                        paramName: paramName,
                        message: $"{id} is not usable for a menu item");
            }
        }

        private static TMenuItemId? TryAsId(uint value)
        {
            var id = (TMenuItemId)Enum.ToObject(typeof(TMenuItemId), value);
            return UsedIds.Contains(id) ? id : null;
        }

        private nint MenuHandle
        {
            get {
                var hMenu = Native.GetSystemMenu(hWnd: _form.Handle,
                                                 bRevert: false);
                Debug.Assert(hMenu != 0, "Failed to get system menu handle");
                return hMenu;
            }
        }

        private void WndProcHook(ref Message m, ref bool handled)
        {
            if (handled) return;

            switch ((Native.WM)m.Msg) {
            case Native.WM.SYSCOMMAND
                    when TryAsId((uint)m.WParam) is TMenuItemId id:
                ItemClick?.Invoke(this, new(id));
                break;

            case Native.WM.INITMENU:
                Opening?.Invoke(this, EventArgs.Empty);
                break;

            default:
                return; // So handled remains false.
            }

            handled = true;
        }

        private void SetStateFlag(TMenuItemId item, Native.MF flag, bool on)
        {
            var mii = new Native.MENUITEMINFO(fMask: Native.MIIM.STATE);
            DoGetMenuItemInfo(item, ref mii);

            if (on)
                mii.fState |= flag;
            else
                mii.fState &= ~flag;

            DoSetMenuItemInfo(item, ref mii);
        }

        private void DoAppendMenu(Native.MF uFlags,
                                  TMenuItemId uIDNewItem,
                                  string? lpNewItem)
        {
            if (!Native.AppendMenu(hMenu: MenuHandle,
                                   uFlags: uFlags,
                                   uIDNewItem: Convert.ToUInt32(uIDNewItem),
                                   lpNewItem: lpNewItem)) {
                Native.ThrowLastError();
            }
        }

        private void DoGetMenuItemInfo(TMenuItemId item,
                                       ref Native.MENUITEMINFO mii)
        {
            EnsureUsedId(item, nameof(item));

            if (!Native.GetMenuItemInfo(hmenu: MenuHandle,
                                        item: Convert.ToUInt32(item),
                                        fByPosition: false,
                                        lpmii: ref mii)) {
                Native.ThrowLastError();
            }
        }

        private void DoSetMenuItemInfo(TMenuItemId item,
                                       ref Native.MENUITEMINFO mii)
        {
            EnsureUsedId(item, nameof(item));

            if (!Native.SetMenuItemInfo(hmenu: MenuHandle,
                                        item: Convert.ToUInt32(item),
                                        fByPosition: false,
                                        ref mii)) {
                Native.ThrowLastError();
            }
        }

        private readonly Form _form;
    }
}
