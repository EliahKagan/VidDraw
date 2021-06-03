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

namespace VidDraw {
    /// <summary>
    /// A message-passing interface for triggering a (parameterized) host
    /// program action from a script, without exposing more host functionality
    /// than necessary to the script.
    /// </summary>
    /// <remarks>
    /// Intended for use as an
    /// <see cref="System.Windows.Forms.WebBrowser.ObjectForScripting"/>.
    /// </remarks>
    public sealed class Bridge {
        /// <summary>
        /// Universally unique identifier so scripts can check if the object
        /// they have is (or claims to be) an instance of this bridge.
        /// </summary>
        /// <remarks>
        /// This doesn't, can't, and isn't intended to provide security.
        /// </remarks>
        public string Uuid => "c4454b65-66c1-408f-bca0-19c0c51bc4ba";

        /// <summary>
        /// Sends a string message through the bridge to a preset receiver.
        /// </summary>
        /// <param name="message">The content of the message to send.</param>
        public void Send(string message) => _receiver(message);

        internal Bridge(Action<string> receiver) => _receiver = receiver;

        private readonly Action<string> _receiver;
    }
}
