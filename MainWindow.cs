using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;
using SharpAvi.Output;

namespace VidDraw {
    public partial class MainWindow : Form {
        public MainWindow()
        {
            InitializeComponent();

            timer = new(interval: IntervalInMilliseconds) {
                Enabled = false,
                SynchronizingObject = this,
            };
            timer.Elapsed += timer_Elapsed;

            rectangle = new(Point.Empty, canvas.Size);
            bitmap = new(width: rectangle.Width, height: rectangle.Height);
            graphics = Graphics.FromImage(bitmap);
            graphics.FillRectangle(Brushes.White, rectangle);
            canvas.Image = bitmap;
        }

        private const int IntervalInMilliseconds = 30;

        private static string GetPreferredSavePath()
            => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                $"VidDraw capture {DateTime.Now:yyyy-MM-dd HH-mm-ss}.avi");

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (videoStream is null) return;

            var bytesPerRow = rectangle.Width * 4;
            var buffer = new byte[rectangle.Height * bytesPerRow];

            var bits = bitmap.LockBits(rectangle,
                                       ImageLockMode.ReadOnly,
                                       PixelFormat.Format32bppArgb);
            try {
                nint bottom = bits.Scan0;

                for (var fromTop = 0; fromTop < rectangle.Height; ++fromTop) {
                    var fromBottom = rectangle.Height - (fromTop + 1);

                    Marshal.Copy(source: bottom + fromBottom * bytesPerRow,
                                 destination: buffer,
                                 startIndex: fromTop * bytesPerRow,
                                 length: bytesPerRow);
                }
            } finally {
                bitmap.UnlockBits(bits);
            }

            videoStream.WriteFrame(true, buffer, 0, buffer.Length);
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
            if (aviWriter is not null) return;

            var fileStream = Files.CreateWithoutClash(GetPreferredSavePath());

            aviWriter = new(fileStream, leaveOpen: false) {
                FramesPerSecond = 1000m / IntervalInMilliseconds,
                EmitIndex1 = true,
            };

            Debug.Assert(videoStream is null);
            videoStream = aviWriter.AddVideoStream(width: rectangle.Width,
                                           height: rectangle.Height);

            BackColor = Color.Red;

            timer.Enabled = true;
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (MouseButtons is not MouseButtons.None || aviWriter is null)
                return;

            timer.Enabled = false;

            videoStream = null;
            aviWriter.Close();
            aviWriter = null;

            BackColor = DefaultBackColor;
        }

        private readonly System.Timers.Timer timer;

        private readonly Rectangle rectangle;

        private readonly Bitmap bitmap;

        private readonly Graphics graphics;

        private readonly Pen pen = new(Color.Black);

        private Point oldLocation = Point.Empty;

        private AviWriter? aviWriter = null;

        private IAviVideoStream? videoStream = null;
    }
}
