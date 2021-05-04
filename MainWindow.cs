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
    internal partial class MainWindow : Form {
        internal MainWindow()
        {
            InitializeComponent();

            _rectangle = new(Point.Empty, _canvas.Size);
            _bitmap = new(width: _rectangle.Width, height: _rectangle.Height);
            _graphics = Graphics.FromImage(_bitmap);
            _canvas.Image = _bitmap;
            _pen = new(_colorPicker.Color);
            _recorder = new(_bitmap, this);

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
        }

        protected override void WndProc(ref Message m)
        {
            switch ((Native.WM)m.Msg) {
            case Native.WM.SYSCOMMAND:
                switch ((MyMenuItemId)m.WParam) {
                case MyMenuItemId.ClearCanvas:
                    ClearCanvas();
                    return;

                case MyMenuItemId.PickColor:
                    PickColor();
                    return;

                case MyMenuItemId.About:
                    ShowAboutBox();
                    return;

                case var id when TryGetCodec(id) is Codec codec:
                    SelectCodec(codec);
                    return;

                default:
                    break; // We'll pass other IDs (e.g., for "Close") upward.
                }
                break;

            case Native.WM.INITMENU:
                UpdateMenu();
                return;

            default:
                break; // Other message types must alwayas be passed upward.
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
            About,
        }

        private sealed record CodecChoice(Codec Codec,
                                          MyMenuItemId Id,
                                          string Label);

        private const Codec DefaultCodec = Codec.MotionJpeg;

        private static string MyVideos
            => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

        private static string CurrentPreferredSavePath
            => Path.Combine(
                MyVideos,
                $"VidDraw capture {DateTime.Now:yyyy-MM-dd HH-mm-ss}.avi");

        private static bool CanEncodeH264
            => Mpeg4VideoEncoderVcm.GetAvailableCodecs()
                .Select(c => c.Codec)
                .Contains(KnownFourCCs.Codecs.X264);

        private static IReadOnlyList<CodecChoice> BuildCodecChoices()
        {
            var builder = ImmutableArray.CreateBuilder<CodecChoice>();

            builder.Add(new(Codec.Raw,
                            MyMenuItemId.Raw,
                            "Raw (frame copy)"));

            builder.Add(new(Codec.Uncompressed,
                            MyMenuItemId.Uncompressed,
                            "Uncompressed"));

            builder.Add(new(Codec.MotionJpeg,
                            MyMenuItemId.MotionJpeg,
                            "Motion JPEG"));

            builder.Add(new(Codec.H264,
                            MyMenuItemId.H264,
                            "H.264 (MPEG-4 AVC)"));

            return builder.ToImmutable();
        }

        private static Codec? TryGetCodec(MyMenuItemId id)
            => CodecChoices.FirstOrDefault(choice => choice.Id == id)?.Codec;

        private static string GetLabel(Codec codec)
            => CodecChoices.Single(choice => choice.Codec == codec).Label;

        private static bool IsKnownCodec(Codec codec)
            => CodecChoices.Any(choice => choice.Codec == codec);

        private static Codec? Sanitize(Codec? untrustedCodec)
            => untrustedCodec switch {
                Codec codec when IsKnownCodec(codec) => codec,
                _ => null,
            };

        private static string GetDisplayPath(string path)
            => path.GetDirectoryOrThrow()
                   .Equals(MyVideos, StringComparison.Ordinal)
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

            new ToastContentBuilder()
                .AddArgument(e.Name)
                .AddText("Video capture saved")
                .AddText(GetDisplayPath(e.Name))
                .AddAttributionText($"Encoding: {GetLabel(e.Codec)}")
                .Show();
        }

        private Codec CurrentCodec
        {
            get => CodecChoices.Single(choice => HasCheck(choice.Id)).Codec;

            set {
                foreach (var choice in CodecChoices)
                    SetCheck(choice.Id, choice.Codec == value);

                Debug.Assert(CurrentCodec == value);
            }
        }

        private HMENU MenuHandle
            => PInvoke.GetSystemMenu(hWnd: new(Handle), bRevert: false);

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

        private void BuildMenu(Codec initialCodec)
        {
            AddMenuSeparator();
            AddMenuCodecItems(initialCodec);

            AddMenuSeparator();
            AddMenuItem(MyMenuItemId.ClearCanvas, "Clear Canvas");
            AddMenuItem(MyMenuItemId.PickColor, $"Pick Color{Ch.Hellip}");
            AddMenuItem(MyMenuItemId.About, $"About VidDraw{Ch.Hellip}");
        }

        private void AddMenuCodecItems(Codec initialCodec)
        {
            foreach (var choice in CodecChoices)
                AddMenuItem(choice.Id, choice.Label);

            CurrentCodec = initialCodec;
            UpdateMenuCodecs();
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
            } else {
                if (CurrentCodec is Codec.H264)
                    CurrentCodec = GetH264FallbackCodec();

                SetEnabled(MyMenuItemId.H264, false);
            }
        }

        private Codec GetH264FallbackCodec()
            => Sanitize(Config.TryLoad().Codec) switch {
                null or Codec.H264 => DefaultCodec,
                Codec codec => codec,
            };

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

            Text = $"VidDraw - Recording{Ch.Hellip}";
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
            Text = "VidDraw - Draw to record video";
        }

        private void SelectCodec(Codec codec)
        {
            CurrentCodec = codec;
            new Config() { Codec = codec }.TrySave();
        }

        // TODO: Also save custom color presets (but only when changed).
        private void PickColor()
        {
            if (_colorPicker.ShowDialog(owner: this) is DialogResult.OK) {
                var color = _colorPicker.Color;
                _pen.Color = color;
                new Config { Color = color }.TrySave();
            }
        }

        private void ClearCanvas()
        {
            _graphics.FillRectangle(Brushes.White, _rectangle);
            _canvas.Invalidate();
            _drawn = false;
        }

        // TODO: Make a custom About dialog listing dependencies and their
        //       copyright notices / licenses.
        private void ShowAboutBox()
            => MessageBox.Show(owner: this,
                               text: "VidDraw (alpha), by Eliah Kagan",
                               caption: "About VidDraw");

        private static IReadOnlyList<CodecChoice> CodecChoices { get; } =
            BuildCodecChoices();

        private readonly Rectangle _rectangle;

        private readonly Bitmap _bitmap;

        private readonly Graphics _graphics;

        private readonly Pen _pen;

        private Point _oldLocation = Point.Empty;

        private bool _drawn = false;

        private readonly Recorder _recorder;
    }
}
