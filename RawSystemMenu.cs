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
            => PInvoke.AppendMenu(hMenu: MenuHandle,
                                  uFlags: MENU_FLAGS.MF_SEPARATOR,
                                  uIDNewItem: Convert.ToUInt32(UnusedId),
                                  lpNewItem: null);

        internal unsafe void AddItem(TMenuItemId uIDNewItem, string lpNewItem)
        {
            EnsureUsedId(uIDNewItem, nameof(uIDNewItem));

            fixed (char* p = lpNewItem) {
                PInvoke.AppendMenu(hMenu: MenuHandle,
                                   uFlags: MENU_FLAGS.MF_STRING,
                                   uIDNewItem: Convert.ToUInt32(uIDNewItem),
                                   lpNewItem: new(p));
            }
        }

        internal unsafe bool HasCheck(TMenuItemId id)
        {
            EnsureUsedId(id, nameof(id));

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

        internal unsafe void SetCheck(TMenuItemId id, bool @checked)
        {
            EnsureUsedId(id, nameof(id));

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

        internal void SetEnabled(TMenuItemId id, bool enabled)
        {
            EnsureUsedId(id, nameof(id));

            PInvoke.EnableMenuItem(hMenu: MenuHandle,
                                   uIDEnableItem: Convert.ToUInt32(id),
                                   uEnable: (enabled ? MENU_FLAGS.MF_ENABLED
                                                     : MENU_FLAGS.MF_GRAYED));
        }

        internal unsafe void SetText(TMenuItemId id, string text)
        {
            EnsureUsedId(id, nameof(id));

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

        private HMENU MenuHandle
            => PInvoke.GetSystemMenu(hWnd: new(_form.Handle), bRevert: false);

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

        private readonly Form _form;
    }
}
