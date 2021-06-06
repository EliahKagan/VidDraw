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
        /// <summary>Creates a browser that will open the help file.</summary>
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
            Debug.Assert(!e.Cancel); // Any cancellation would come later.

            if (Url is null || IsCurrentPage(e.Url)) {
                // Allow initial page load and navigation within the page.
                base.OnNavigating(e);
            } else {
                // Other links are opened (if at all) outside the help browser.
                e.Cancel = true;
                OpenOutside(e.Url);
            }
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

        /// <summary>Checks if a given URI is on the current page.</summary>
        /// <remarks>
        /// Unintuitively, Uri.Equals doesn't compare hash fragments. So
        /// equality comparison of old and new Uri objects checks if a
        /// navigation would (at most) change sections on the same page.
        /// </remarks>
        private bool IsCurrentPage(Uri uri) => uri.Equals(Url);
    }
}
