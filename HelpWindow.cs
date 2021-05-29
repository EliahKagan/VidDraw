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
using System.Diagnostics;
using System.Windows.Forms;

namespace VidDraw {
    internal sealed partial class HelpWindow : Form {
        /// <summary>
        /// Creates a window with a browser that will open the help file.
        /// </summary>
        internal HelpWindow() => InitializeComponent();

        /// <summary>
        /// Scales and positions the window on initial page load, so it's wide
        /// enough to show the sidenav when possible, without being off-screen.
        /// </summary>
        /// <remarks>
        /// Autoscaling doesn't take care of this, since the page is scaled
        /// proprtionately, not by UI font dimensions or DPI. HelpBrowser (via
        /// code its base classes) properly scales the page. So this method is
        /// just setting the window size and location (and the control size).
        /// </remarks>
        private void
        browser_DocumentCompleted(object sender,
                                  WebBrowserDocumentCompletedEventArgs e)
        {
            // Only resize the browser the first time the page is loaded.
            _browser.DocumentCompleted -= browser_DocumentCompleted;

            SetSize();
            ApplyLimits();
        }

        /// <summary>Sets both width and height based on body width.</summary>
        private void SetSize()
        {
            const float widthScaleFactor = 1.5f;
            const float heightScaleFactor = 1.07f;

            var bodyWidth = _browser.Document.Body.ClientRectangle.Width;

            ClientSize = new(width: (int)(bodyWidth * widthScaleFactor),
                             height: (int)(bodyWidth * heightScaleFactor));
        }

        /// <summary>
        /// Corrects size and location based on the screen's working area.
        /// </summary>
        private void ApplyLimits()
        {
            const int widthLimitMargin = 10;

            var limits = Screen.FromHandle(Handle).WorkingArea;
            Debug.Assert(!limits.IsEmpty);

            if (Width > limits.Width + widthLimitMargin * 2)
                Width = limits.Width + widthLimitMargin * 2;

            if (Height > limits.Height) Height = limits.Height;

            if (Right > limits.Right + widthLimitMargin) {
                Left = Math.Max(limits.Left - widthLimitMargin,
                                limits.Right + widthLimitMargin - Width);
            }

            if (Bottom > limits.Bottom)
                Top = Math.Max(limits.Top, limits.Bottom - Height);
        }
    }
}
