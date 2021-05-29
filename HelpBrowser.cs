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
using System.IO;
using System.Windows.Forms;

namespace VidDraw {
    internal sealed class HelpBrowser : WebBrowser {
        /// <inheritdoc/>
        /// <remarks>
        /// This is public because making it internal triggers a Designer bug
        /// where fields of this type are unassigned in generated C# code.
        /// </remarks>
        public HelpBrowser()
        {
            if (Program.IsRunning) Url = new(MyPaths.HelpFile);
        }

        protected override void OnNavigating(WebBrowserNavigatingEventArgs e)
        {
            base.OnNavigating(e);
            if (e.Cancel) return;

            // Allow the initial page load.
            if (Url is null) return;

            // Unintuitively, Uri.Equals doesn't compare hash fragments. So
            // equality comparison of old and new Uri objects checks if a
            // navigation would (at most) change sections on the same page.
            if (e.Url.Equals(Url)) return;

            // Other links are opened (if at all) outside the help browser.
            e.Cancel = true;
            OpenOutside(e.Url);
        }

        protected override void
        OnDocumentCompleted(WebBrowserDocumentCompletedEventArgs e)
        {
            base.OnDocumentCompleted(e);

            Debug.Print($"Body width: {Document.Body.ScrollRectangle.Width}");
            Debug.Print($"Control width: {Width}");
            Debug.Print($"Client width: {ClientSize.Width}");

            Debug.Print($"Body height: {Document.Body.ScrollRectangle.Height}");
            Debug.Print($"Control height: {Height}");
            Debug.Print($"Client height: {ClientSize.Height}");
        }

        private static void OpenOutside(Uri url)
        {
            switch (url.Scheme) {
            // Open links to local directories in Explorer windows.
            case "file" when Directory.Exists(url.AbsolutePath):
                Shell.Execute(url.AbsolutePath);
                break;

            // Hackishly open all other local links in the default browser.
            case "file":
                url.AbsoluteUri.OpenLike(MyPaths.HelpFile);
                break;

            // Open web links in the default browser.
            case "http":
            case "https":
                Shell.Execute(url.AbsoluteUri);
                break;

            default:
                Debug.Print(nameof(HelpBrowser)
                            + $"Unexpected protocol \"{url.Scheme}\"");
                break;
            }
        }
    }
}
