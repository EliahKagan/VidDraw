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

using System.Windows.Forms;

namespace VidDraw {
    /// <summary>
    /// A form to which WndProc hooks may be added and removed.
    /// </summary>
    internal class HookForm : Form {
        /// <summary>
        /// Represents a method that adds <see cref="WndProc"/> behavior.
        /// </summary>
        /// <param name="m">The window message that might be handled.</param>
        /// <param name="handled">Whether processing is still needed.</param>
        /// <remarks>See also <seealso cref="ReceivedMessage"/>.</remarks>
        internal delegate void Hook(ref Message m, ref bool handled);

        /// <summary>
        /// The event <see cref="WndProc"/> delegates to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Most handlers should first check <c>handled</c> and immediately
        /// return if it is already true. The only handlers that should not are
        /// those that examine ("spy on") messages rather than handling them.
        /// </para>
        /// <para>See also <seealso cref="Hook"/>.</para>
        /// </remarks>
        internal event Hook? ReceivedMessage;

        /// <inheritdoc/>
        protected override void WndProc(ref Message m)
        {
            var handled = false;
            ReceivedMessage?.Invoke(ref m, ref handled);
            if (!handled) base.WndProc(ref m);
        }
    }
}
