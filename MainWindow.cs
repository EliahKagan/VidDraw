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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Windows.Sdk;
using SharpAvi;
using SharpAvi.Codecs;

namespace VidDraw {
    /// <summary>The application window, containing a drawing canvas.</summary>
    internal sealed partial class MainWindow : Form {
        internal MainWindow()
        {
            InitializeComponent();

            _rectangle = new(Point.Empty, _canvas.Size);
            _bitmap = new(width: _rectangle.Width, height: _rectangle.Height);
            _graphics = Graphics.FromImage(_bitmap);
            _canvas.Image = _bitmap;
            _pen = new(_colorPicker.Color);
            _recorder = new(_bitmap, this);
            _menuActions = BuildMenuActions();

            SetInitialTitle();
            _recorder.Recorded += recorder_Recorded;
            ClearCanvas();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            var config = Config.TryLoad();

            BuildMenu(Sanitize(config.Codec) ?? DefaultCodec);

            if (config.Color is Color color)
                _pen.Color = _colorPicker.Color = color;

            TryApplyCustomColors(config.CustomColors);
        }

        protected override void WndProc(ref Message m)
        {
            switch ((Native.WM)m.Msg) {
            case Native.WM.SYSCOMMAND:
                if (_menuActions.TryGetValue((MyMenuItemId)m.WParam,
                                             out var action)) {
                    action();
                    return;
                }
                break; // We'll pass other IDs (e.g., for "Close") upward.

            case Native.WM.INITMENU:
                UpdateMenu();
                return;

            default:
                break; // Other message types must always be passed upward.
            }

            base.WndProc(ref m);
        }

        private enum MyMenuItemId : uint {
            UnusedId, // For clarity, pass this when the ID will be ignored.

            Raw,
            Uncompressed,
            MotionJpeg,
            H264,

            ClearCanvas,
            PickColor,
            OpenVideosFolder,
            DownloadOrConfigureX264vfw,

            About,
        }

        private sealed record CodecChoice(Codec Codec,
                                          MyMenuItemId Id,
                                          string Label);

        private const Codec DefaultCodec = Codec.MotionJpeg;

        private const string X264vfwDownloadUrl =
            "https://sourceforge.net/projects/x264vfw/files/latest/download";

        private static IReadOnlyList<CodecChoice> CodecChoices { get; } =
            ImmutableArray.Create<CodecChoice>(
                new(Codec.Raw,
                    MyMenuItemId.Raw,
                    "Ra&w (frame copy)"),
                new(Codec.Uncompressed,
                    MyMenuItemId.Uncompressed,
                    "&Uncompressed"),
                new(Codec.MotionJpeg,
                    MyMenuItemId.MotionJpeg,
                    "Motion &JPEG"),
                new(Codec.H264,
                    MyMenuItemId.H264,
                    "&H.264 (MPEG-4 AVC)"));

        private static string CurrentPreferredSavePath
            => Path.Combine(
                Dirs.Videos,
                $"VidDraw capture {DateTime.Now:yyyy-MM-dd HH-mm-ss}.avi");

        private static bool CanEncodeH264
            => Mpeg4VideoEncoderVcm.GetAvailableCodecs()
                .Select(info => info.Codec)
                .Contains(KnownFourCCs.Codecs.X264);

        private static string GetLabel(Codec codec)
            => CodecChoices.Single(choice => choice.Codec == codec)
                           .Label
                           .Replace(oldValue: "&", newValue: null);

        private static bool IsKnownCodec(Codec codec)
            => CodecChoices.Any(choice => choice.Codec == codec);

        private static Codec? Sanitize(Codec? untrustedCodec)
            => untrustedCodec switch {
                Codec codec when IsKnownCodec(codec) => codec,
                _ => null,
            };

        private static Codec GetH264FallbackCodec()
            => Sanitize(Config.TryLoad().Codec) switch {
                null or Codec.H264 => DefaultCodec,
                Codec codec => codec,
            };

        private static void DownloadX264vfw()
            => Shell.Execute(X264vfwDownloadUrl);

        private static void ConfigureX264vfw()
        {
            var dir = Dirs.System;
            var dll = $"x264vfw{(Environment.Is64BitProcess ? "64" : "")}.dll";

            // FIXME: Should I handle errors from this?
            Process.Start(new ProcessStartInfo {
                FileName = Path.Combine(dir, "rundll32.exe"),
                Arguments = $"{dll},Configure",
                UseShellExecute = false,
                WorkingDirectory = dir,
            });
        }

        private static string GetDisplayPath(string path)
            => Files.GetDirectoryOrThrow(path)
                    .Equals(Dirs.Videos, StringComparison.Ordinal)
                ? Path.GetFileName(path)
                : path;

        private static void recorder_Recorded(object? sender,
                                              RecordedEventArgs e)
        {
            if (e.Name is null) {
                throw new ArgumentException(
                    paramName: nameof(e),
                    message: $"Expected non-null {nameof(e.Name)} property");
            }

            if (Platform.CanToast) {
                new ToastContentBuilder()
                    .AddArgument(e.Name)
                    .AddText("Video capture saved")
                    .AddText(GetDisplayPath(e.Name))
                    .AddAttributionText($"Encoding: {GetLabel(e.Codec)}")
                    .Show();
            } else {
                // We can't notify, so we just open the folder window now.
                Shell.Select(e.Name);
            }
        }

        private IReadOnlyDictionary<MyMenuItemId, Action> BuildMenuActions()
        {
            var builder =
                ImmutableDictionary.CreateBuilder<MyMenuItemId, Action>();

            builder.Add(MyMenuItemId.ClearCanvas, ClearCanvas);
            builder.Add(MyMenuItemId.PickColor, PickColor);
            builder.Add(MyMenuItemId.OpenVideosFolder, OpenVideosFolder);
            builder.Add(MyMenuItemId.DownloadOrConfigureX264vfw,
                        () => _downloadOrConfigureX264vfw());
            builder.Add(MyMenuItemId.About, ShowAboutBox);

            foreach (var choice in CodecChoices)
                builder.Add(choice.Id, () => SelectCodec(choice.Codec));

            return builder.ToImmutable();
        }

        private void SetTitle(string message)
            => Text = $"VidDraw ({Platform.XStyleArch}) - {message}";

        private void SetInitialTitle() => SetTitle("Draw to record video");

        private HMENU MenuHandle
            => PInvoke.GetSystemMenu(hWnd: new(Handle), bRevert: false);

        private Codec CurrentCodec
        {
            get => CodecChoices.Single(choice => HasCheck(choice.Id)).Codec;

            set {
                foreach (var choice in CodecChoices)
                    SetCheck(choice.Id, choice.Codec == value);

                Debug.Assert(CurrentCodec == value);
            }
        }

        private void AddMenuSeparator()
            => PInvoke.AppendMenu(hMenu: MenuHandle,
                                  uFlags: MENU_FLAGS.MF_SEPARATOR,
                                  uIDNewItem: (nuint)MyMenuItemId.UnusedId,
                                  lpNewItem: null);

        private unsafe void AddMenuItem(MyMenuItemId uIDNewItem,
                                        string lpNewItem)
        {
            fixed (char* p = lpNewItem) {
                PInvoke.AppendMenu(hMenu: MenuHandle,
                                   uFlags: MENU_FLAGS.MF_STRING,
                                   uIDNewItem: (nuint)uIDNewItem,
                                   lpNewItem: new(p));
            }
        }

        private unsafe bool HasCheck(MyMenuItemId id)
        {
            var mii = new MENUITEMINFOW {
                cbSize = (uint)sizeof(MENUITEMINFOW),
                fMask = MENUITEMINFOA_fMask.MIIM_STATE,
            };

            PInvoke.GetMenuItemInfo(hmenu: MenuHandle,
                                    item: (uint)id,
                                    fByPosition: false,
                                    &mii);

            return (mii.fState & (uint)MENU_FLAGS.MF_CHECKED) != 0;
        }

        private unsafe void SetCheck(MyMenuItemId id, bool @checked)
        {
            var mii = new MENUITEMINFOW {
                cbSize = (uint)sizeof(MENUITEMINFOW),
                fMask = MENUITEMINFOA_fMask.MIIM_STATE,
                fState = (uint)(@checked ? MENU_FLAGS.MF_CHECKED
                                         : MENU_FLAGS.MF_UNCHECKED),
            };

            PInvoke.SetMenuItemInfo(hmenu: MenuHandle,
                                    item: (uint)id,
                                    fByPositon: false, // Misspelled in API.
                                    &mii);
        }

        private void SetEnabled(MyMenuItemId id, bool enabled)
            => PInvoke.EnableMenuItem(
                    hMenu: MenuHandle,
                    uIDEnableItem: (uint)id,
                    uEnable: (enabled ? MENU_FLAGS.MF_ENABLED
                                      : MENU_FLAGS.MF_GRAYED));

        private unsafe void SetText(MyMenuItemId id, string text)
        {
            fixed (char* p = text) {
                var mii = new MENUITEMINFOW {
                    cbSize = (uint)sizeof(MENUITEMINFOW),
                    fMask = MENUITEMINFOA_fMask.MIIM_STRING,
                    dwTypeData = new(p),
                };

                PInvoke.SetMenuItemInfo(
                        hmenu: MenuHandle,
                        item: (uint)id,
                        fByPositon: false, // Misspelled in API.
                        &mii);
            }
        }

        private void BuildMenu(Codec initialCodec)
        {
            AddMenuSeparator();
            AddMenuCodecItems(initialCodec);

            AddMenuSeparator();
            AddMenuItem(MyMenuItemId.ClearCanvas, "Clear Canva&s");
            AddMenuItem(MyMenuItemId.PickColor, $"&Pick Color{Ch.Hellip}");
            AddMenuItem(MyMenuItemId.OpenVideosFolder, "&Open Videos Folder");
            AddMenuItem(MyMenuItemId.DownloadOrConfigureX264vfw, "(error)");

            AddMenuSeparator();
            AddMenuItem(MyMenuItemId.About, $"&About VidDraw{Ch.Hellip}");

            UpdateMenuCodecs();
        }

        private void AddMenuCodecItems(Codec initialCodec)
        {
            foreach (var choice in CodecChoices)
                AddMenuItem(choice.Id, choice.Label);

            CurrentCodec = initialCodec;
        }

        private void UpdateMenu()
        {
            UpdateMenuCodecs();
            SetEnabled(MyMenuItemId.ClearCanvas, _drawn);
        }

        private void UpdateMenuCodecs()
        {
            if (CanEncodeH264) {
                SetEnabled(MyMenuItemId.H264, true);
                SetText(MyMenuItemId.DownloadOrConfigureX264vfw,
                        $"Configure x264&vfw ({Platform.XStyleArch})");
                _downloadOrConfigureX264vfw = ConfigureX264vfw;
            } else {
                if (CurrentCodec is Codec.H264)
                    CurrentCodec = GetH264FallbackCodec();

                SetEnabled(MyMenuItemId.H264, false);
                SetText(MyMenuItemId.DownloadOrConfigureX264vfw,
                        "Download x264&vfw");
                _downloadOrConfigureX264vfw = DownloadX264vfw;
            }
        }

        private void canvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (_rectangle.Contains(e.Location)
                        && e.Button is MouseButtons.Left) {
                _bitmap.SetPixel(e.Location.X, e.Location.Y, _pen.Color);
                _canvas.Invalidate(new Rectangle(e.Location, new Size(1, 1)));
                _drawn = true;
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButtons.Left)) {
                _graphics.DrawLine(_pen, _oldLocation, e.Location);

                var x1 = Math.Min(_oldLocation.X, e.Location.X);
                var y1 = Math.Min(_oldLocation.Y, e.Location.Y);
                var x2 = Math.Max(_oldLocation.X, e.Location.X);
                var y2 = Math.Max(_oldLocation.Y, e.Location.Y);

                var corner = new Point(x: x1, y: y1);
                var size = new Size(width: x2 - x1 + 1, height: y2 - y1 + 1);

                _canvas.Invalidate(new Rectangle(corner, size));
                _drawn = true;
            }

            _oldLocation = e.Location;
        }

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (_recorder.IsRunning) return;

            UpdateMenuCodecs();

            SetTitle($"Recording{Ch.Hellip}");
            BackColor = Color.Red;

            _recorder.Start(Files.CreateWithoutClash(CurrentPreferredSavePath),
                            CurrentCodec);
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (!(MouseButtons is MouseButtons.None && _recorder.IsRunning))
                return;

            _recorder.Finish();

            BackColor = DefaultBackColor;
            SetInitialTitle();
        }

        private void aboutBox_FormClosed(object? sender, FormClosedEventArgs e)
        {
            Debug.Assert(_aboutBox is not null);
            Debug.Assert(sender == _aboutBox);

            _aboutBox.Dispose();
            _aboutBox = null;
        }

        private void SelectCodec(Codec codec)
        {
            CurrentCodec = codec;
            new Config { Codec = codec }.TrySave();
        }

        private void PickColor()
        {
            TryApplyCustomColors(Config.TryLoad().CustomColors);

            if (_colorPicker.ShowDialog(owner: this) is DialogResult.OK) {
                _pen.Color = _colorPicker.Color;

                new Config {
                    Color = _colorPicker.Color,
                    CustomColors = _colorPicker.CustomColors,
                }.TrySave();
            }
        }

        private void TryApplyCustomColors(int[]? colors)
        {
            if (colors is not null) _colorPicker.CustomColors = colors;
        }

        private void ClearCanvas()
        {
            _graphics.FillRectangle(Brushes.White, _rectangle);
            _canvas.Invalidate();
            _drawn = false;
        }

        private void OpenVideosFolder()
        {
            var path = Dirs.Videos;

            if (!Directory.Exists(path)) {
                throw new InvalidOperationException(
                        $"MyVideos path not a directory: {path}");
            }

            Shell.Execute(path);
        }

        private void ShowAboutBox()
        {
            if (_aboutBox is null) {
                _aboutBox = new();
                _aboutBox.FormClosed += aboutBox_FormClosed;
                _aboutBox.Show();
            } else {
                _aboutBox.WindowState = FormWindowState.Normal;
                _aboutBox.Activate();
            }
        }

        private readonly Rectangle _rectangle;

        private readonly Bitmap _bitmap;

        private readonly Graphics _graphics;

        private readonly Pen _pen;

        private Point _oldLocation = Point.Empty;

        private bool _drawn = false;

        private readonly Recorder _recorder;

        private Action _downloadOrConfigureX264vfw =
            () => throw new InvalidOperationException(
                "Bug: Don't know whether to download or configure x264vfw.");

        private readonly IReadOnlyDictionary<MyMenuItemId, Action>
        _menuActions;

        private HelpWindow? _aboutBox = null;
    }
}
