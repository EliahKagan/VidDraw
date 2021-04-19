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

namespace VidDraw {
    /// <summary>The application window, containing a drawing canvas.</summary>
    internal partial class MainWindow : Form {
        internal MainWindow()
        {
            InitializeComponent();

            rectangle = new(Point.Empty, canvas.Size);
            bitmap = new(width: rectangle.Width, height: rectangle.Height);
            graphics = Graphics.FromImage(bitmap);
            graphics.FillRectangle(Brushes.White, rectangle);
            pen = new(colorPicker.Color);
            canvas.Image = bitmap;
            recorder = new(bitmap, this);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            BuildMenu();
        }

        protected override void WndProc(ref Message m)
        {
            // If the message is WM_SYSCOMMAND *and* it is one of our system-
            // menu customizations, handle it here instead of pasing it upward.
            if ((WindowMessage)m.Msg is WindowMessage.SYSCOMMAND) {
                switch ((MyMenuItemId)m.WParam) {
                case MyMenuItemId.PickColor:
                    PickColor();
                    return;

                case MyMenuItemId.About:
                    ShowAboutBox();
                    return;

                case MyMenuItemId id when AsCodec(id) is Codec codec:
                    CurrentCodec = codec;
                    return;

                default:
                    break; // We'll pass other IDs (e.g., for "Close") upward.
                }
            }

            // Otherwise, the message MUST be passed upward.
            base.WndProc(ref m);
        }

        /// <summary>Menu flags.</summary>
        // TODO: Can I make C#/Win32 give me this from metadata?
        private enum MF : uint {
            UNCHECKED = 0x00000000,
            CHECKED = 0x00000008,
        }

        private enum MyMenuItemId : uint {
            UnusedId, // For clarity, pass this when the ID will be ignored.

            Raw,
            Uncompressed,
            MotionJpeg,
            H264,

            PickColor,
            About,
        }

        private sealed record CodecChoice(Codec Codec,
                                          MyMenuItemId Id,
                                          string Label);

        private static string MyVideos
            => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

        private static string CurrentPreferredSavePath
            => Path.Combine(
                MyVideos,
                $"VidDraw capture {DateTime.Now:yyyy-MM-dd HH-mm-ss}.avi");

        private static IReadOnlyList<CodecChoice> BuildCodecChoices()
        {
            var builder = ImmutableArray.CreateBuilder<CodecChoice>();

            builder.Add(new(Codec.Raw,
                            MyMenuItemId.Raw,
                            "Raw (no encoder)"));

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

        private static Codec? AsCodec(MyMenuItemId id)
            => codecs.FirstOrDefault(c => c.Id == id)?.Codec;

        private static string GetDisplayPath(string path)
            => path.GetDirectoryOrThrow()
                   .Equals(MyVideos, StringComparison.Ordinal)
                ? Path.GetFileName(path)
                : path;

        private static void NotifySaved(string path)
            => new ToastContentBuilder()
                .AddArgument(path)
                .AddText("Video capture saved")
                .AddText(GetDisplayPath(path))
                .Show();

        private Codec CurrentCodec
        {
            get => codecs.Single(c => HasCheck(c.Id)).Codec;

            set {
                foreach (var (codec, id, _) in codecs)
                    SetCheck(id, codec == value);

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

            return (mii.fState & (uint)MF.CHECKED) != 0;
        }

        private unsafe void SetCheck(MyMenuItemId id, bool @checked)
        {
            var mii = new MENUITEMINFOW {
                cbSize = (uint)sizeof(MENUITEMINFOW),
                fMask = MENUITEMINFOA_fMask.MIIM_STATE,
                fState = (uint)(@checked ? MF.CHECKED : MF.UNCHECKED),
            };

            PInvoke.SetMenuItemInfo(hmenu: MenuHandle,
                                    item: (uint)id,
                                    fByPositon: false, // Misspelled in API.
                                    &mii);
        }

        private void BuildMenu()
        {
            AddMenuSeparator();
            foreach (var (_, id, label) in codecs) AddMenuItem(id, label);
            CurrentCodec = Codec.Uncompressed;

            AddMenuSeparator();
            AddMenuItem(MyMenuItemId.PickColor, $"Pick Color{Ch.Hellip}");
            AddMenuItem(MyMenuItemId.About, $"About VidDraw{Ch.Hellip}");
        }

        private void canvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (rectangle.Contains(e.Location)
                        && e.Button is MouseButtons.Left) {
                bitmap.SetPixel(e.Location.X, e.Location.Y, Color.Black);
                canvas.Invalidate(new Rectangle(e.Location, new Size(1, 1)));
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButtons.Left)) {
                graphics.DrawLine(pen, oldLocation, e.Location);

                var x1 = Math.Min(oldLocation.X, e.Location.X);
                var y1 = Math.Min(oldLocation.Y, e.Location.Y);
                var x2 = Math.Max(oldLocation.X, e.Location.X);
                var y2 = Math.Max(oldLocation.Y, e.Location.Y);

                var corner = new Point(x: x1, y: y1);
                var size = new Size(width: x2 - x1 + 1, height: y2 - y1 + 1);

                canvas.Invalidate(new Rectangle(corner, size));
            }

            oldLocation = e.Location;
        }

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (recorder.IsRunning) return;

            BackColor = Color.Red;
            var output = Files.CreateWithoutClash(CurrentPreferredSavePath);
            var path = output.Name;
            recorder.Start(output, () => NotifySaved(path));
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (!(MouseButtons is MouseButtons.None && recorder.IsRunning))
                return;

            recorder.Finish();
            BackColor = DefaultBackColor;
        }

        private void PickColor()
        {
            if (colorPicker.ShowDialog(owner: this) is DialogResult.OK)
                pen.Color = colorPicker.Color;
        }

        // TODO: Make a custom About dialog listing dependencies and their
        //       copyright notices / licenses.
        private void ShowAboutBox()
            => MessageBox.Show(owner: this,
                               text: "VidDraw (alpha), by Eliah Kagan",
                               caption: "About VidDraw");

        private static readonly IReadOnlyList<CodecChoice> codecs =
            BuildCodecChoices();

        private readonly Rectangle rectangle;

        private readonly Bitmap bitmap;

        private readonly Graphics graphics;

        private readonly Pen pen;

        private Point oldLocation = Point.Empty;

        private readonly Recorder recorder;
    }
}
