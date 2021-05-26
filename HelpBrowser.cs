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
        internal HelpBrowser() => Url = new(HelpPath);

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

            // We never follow other kinds of links in the help browser itself.
            e.Cancel = true;

            switch (e.Url.Scheme) {
            // Open links to local directories in Explorer windows.
            case "file" when e.Url.AbsolutePath.EndsWithDirectorySeparator():
                Shell.Execute(e.Url.AbsolutePath);
                break;

            // Hackishly open all other local links in the default browser.
            case "file":
                var path = Path.GetRelativePath(relativeTo: DocDir,
                                                path: e.Url.AbsolutePath);

                // FIXME: This doesn't work, because ShellExecute doesn't
                // preserve query strings. So this will have to be done a
                // totally different way, maybe with:
                // https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-findexecutablew
                var redirect = new UriBuilder(RedirectPath) {
                    Query = "path=" + path,
                }.Uri;
                Debug.Print(redirect.AbsoluteUri); // It has the query.
                Shell.Execute(redirect.AbsoluteUri); // ShellExecute drops it.

                // As expected, this is no better:
                //Shell.Execute($"{new Uri(RedirectPath).AbsoluteUri}?path={path.Replace('\\', '/')}");

                break;

            // Open web links in the default browser.
            case "http":
            case "https":
                Shell.Execute(e.Url.AbsoluteUri);
                break;

            default:
                Debug.Print(nameof(HelpBrowser)
                            + $"Unexpected protocol \"{e.Url.Scheme}\"");
                break;
            }
        }

        private static string DocDir { get; } =
            Path.Combine(Files.ExecutableDirectory, "doc");

        private static string HelpPath { get; } =
            Path.Combine(DocDir, "index.html");

        private static string RedirectPath { get; } =
            Path.Combine(DocDir, "redirect.html");
    }
}
