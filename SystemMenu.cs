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
using System.Collections.Generic;

namespace VidDraw {
    /// <summary>Augmentable system menu abstraction.</summary>
    /// <typeparam name="TMenuItemId">
    /// The enumeration of new menu ids. Must contain 0 as <c>UnusedId</c>.
    /// </typeparam>
    /// <remarks>
    /// This provides a higher level, more convenient API than
    /// <see cref="RawSystemMenu"/>, which it delegates to.
    /// </remarks>
    internal sealed class SystemMenu<TMenuItemId>
            where TMenuItemId : struct, Enum {
        internal SystemMenu(HookForm form, Action? onOpening = null)
        {
            _rawMenu = new(form);

            if (onOpening is not null)
                _rawMenu.Opening += (_, _) => onOpening();

            _rawMenu.ItemClick += rawMenu_ItemClick;
        }

        private void
        rawMenu_ItemClick(object? sender,
                          RawSystemMenu<TMenuItemId>.ItemClickEventArgs e)
        {
            if (!_actions.TryGetValue(e.Id, out var action)) {
                throw new InvalidOperationException(
                        $"No action registered for menu item {e.Id}");
            }

            action();
        }

        internal void AddSeparator() => _rawMenu.AddSeparator();

        internal void AddItem(TMenuItemId uIDNewItem,
                              string lpNewItem,
                              Action onItemClick)
        {
            _rawMenu.AddItem(uIDNewItem, lpNewItem);
            _actions[uIDNewItem] = onItemClick;
        }

        internal bool HasCheck(TMenuItemId id) => _rawMenu.HasCheck(id);

        internal void SetCheck(TMenuItemId id, bool @checked)
            => _rawMenu.SetCheck(id, @checked);

        internal void SetEnabled(TMenuItemId id, bool enabled)
            => _rawMenu.SetEnabled(id, enabled);

        internal void SetText(TMenuItemId id, string text)
            => _rawMenu.SetText(id, text);

        private readonly RawSystemMenu<TMenuItemId> _rawMenu;

        private readonly IDictionary<TMenuItemId, Action> _actions =
            new Dictionary<TMenuItemId, Action>();
    }
}
