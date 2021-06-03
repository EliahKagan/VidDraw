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
using System.Collections.Immutable;
using System.Diagnostics;
using System.Windows.Forms;

namespace VidDraw {
    internal sealed partial class HelpWindow : HookForm {
        /// <summary>
        /// Creates a window with a browser that will open the help file.
        /// </summary>
        internal HelpWindow()
        {
            InitializeComponent();
            _menu = new(this);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            BuildMenu();
        }

        private enum MenuItemId : uint {
            UnusedId, // For clarity, pass this when the ID will be ignored.

            About,
            License,
            Setup,
            UsageTips,
            TheMenu,
            KnownBugs,
            Dependencies,
            Notices,

            OpenInWebBrowser,
            VisitGitHubRepository,
        }

        private sealed record Section(string SectionId,
                                      MenuItemId MenuItemId,
                                      string MenuItemLabel);

        private static IReadOnlyList<Section> Sections { get; } =
            ImmutableArray.Create<Section>(
                new(SectionId: "viddraw---record-video-as-you-draw",
                    MenuItemId: MenuItemId.About,
                    MenuItemLabel: "&About"),
                new(SectionId: "license",
                    MenuItemId: MenuItemId.License,
                    MenuItemLabel: "&License"),
                new(SectionId: "setup",
                    MenuItemId: MenuItemId.Setup,
                    MenuItemLabel: "S&etup"),
                new(SectionId: "usage-tips",
                    MenuItemId: MenuItemId.UsageTips,
                    MenuItemLabel: "&Usage Tips"),
                new(SectionId: "the-menu",
                    MenuItemId: MenuItemId.TheMenu,
                    MenuItemLabel: "&The Menu"),
                new(SectionId: "known-bugs",
                    MenuItemId: MenuItemId.KnownBugs,
                    MenuItemLabel: "&Known Bugs"),
                new(SectionId: "dependencies",
                    MenuItemId: MenuItemId.Dependencies,
                    MenuItemLabel: "&Dependencies"),
                new(SectionId: "notices",
                    MenuItemId: MenuItemId.Notices,
                    MenuItemLabel: "N&otices"));

        private void BuildMenu()
        {
            BuildMenuHelpSectionsSection();
            BuildMenuExternalPagesSection();
        }

        private void BuildMenuHelpSectionsSection()
        {
            _menu.AddSeparator();

            foreach (var section in Sections) {
                _menu.AddItem(section.MenuItemId,
                              section.MenuItemLabel,
                              () => ScrollTo(section.SectionId));

                _menu.SetEnabled(section.MenuItemId, false);
            }
        }

        private void BuildMenuExternalPagesSection()
        {
            _menu.AddSeparator();

            _menu.AddItem(MenuItemId.OpenInWebBrowser,
                          "Open in Web &Browser",
                          OpenInWebBrowser);

            _menu.AddItem(MenuItemId.VisitGitHubRepository,
                          "Visit &GitHub Repository",
                          VisitGitHubRepository);
        }

        private void browser_Navigating(object sender,
                                        WebBrowserNavigatingEventArgs e)
        {
            if (!e.Cancel) SetMenuHelpSectionsEnabled(false);
        }

        private void
        browser_DocumentCompleted(object sender,
                                  WebBrowserDocumentCompletedEventArgs e)
        {
            if (!_loaded) {
                _loaded = true;
                SetGeometry();
            }

            SetMenuHelpSectionsEnabled(true);
        }

        /// <summary>
        /// Scales and positions the window so it's wide enough to show the
        /// sidenav when possible, without being off-screen.
        /// </summary>
        /// <remarks>
        /// Autoscaling doesn't take care of this, since the page is scaled
        /// proportionately, not by UI font dimensions or DPI. HelpBrowser (via
        /// its base classes) properly scales the page. So this method is just
        /// setting the window size and location (and the control size).
        /// </remarks>
        private void SetGeometry()
        {
            SetSize();
            ApplyLimits();
        }

        /// <summary>Sets both width and height based on body width.</summary>
        private void SetSize()
        {
            // Shows the sidenav with a comfortable gap, on most screens and
            // configurations. This can go as low as 1.5 and still work okay.
            const float widthScaleFactor = 1.53f;

            // Comfortable size. Essential info is (usually) "above the fold."
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

        private void SetMenuHelpSectionsEnabled(bool enabled)
        {
            foreach (var section in Sections)
                _menu.SetEnabled(section.MenuItemId, enabled);
        }

        private void ScrollTo(string sectionId)
            => _browser.Document.InvokeScript("smoothScrollIntoViewById",
                                              new object[] { sectionId });

        private void OpenInWebBrowser()
            => Shell.Execute(new Uri(MyPaths.HelpFile).AbsoluteUri);

        private void VisitGitHubRepository()
            => Shell.Execute("https://github.com/EliahKagan/VidDraw");

        private readonly SystemMenu<MenuItemId> _menu;

        private bool _loaded = false;
    }
}
