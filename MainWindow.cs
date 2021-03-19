using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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

        private static string GetSavePath()
            => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                $"VidDraw capture {DateTime.Now:yyyy-MM-dd HH-mm-ss}.avi");

        private static string IncrementPath(string path)
        {
            var match = pathParser.Match(path);

            if (!match.Success) {
                throw new InvalidOperationException(
                        "Can't increment path: " + path);
            }

            var number = match.Groups["number"].Success
                            ? int.Parse(match.Groups["number"].Value)
                            : 1;

            var beforeSuffix = $"{match.Groups["prefix"]} ({number + 1})";

            return match.Groups["suffix"].Success
                    ? beforeSuffix + match.Groups["suffix"].Value
                    : beforeSuffix;
        }

        private static FileStream OpenFileStream()
        {
            var path = GetSavePath();

            for (; ; ) {
                try {
                    return new FileStream(path, FileMode.CreateNew);
                }
                catch (IOException ex) when (ex.HResult == -2147024816) {
                    path = IncrementPath(path);
                }
            }
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (videoStream is null) return;

            var data = new byte[rectangle.Width * rectangle.Height * 4];

            var bits = bitmap.LockBits(rectangle,
                                       ImageLockMode.ReadOnly,
                                       PixelFormat.Format32bppRgb);
            // FIXME: This vertically reflects the image. Don't do that.
            Marshal.Copy(bits.Scan0, data, 0, data.Length);
            bitmap.UnlockBits(bits);

            videoStream.WriteFrame(true, data, 0, data.Length);
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

            fileStream = OpenFileStream();

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
            fileStream = null;

            BackColor = DefaultBackColor;
        }

        private static readonly Regex pathParser = new(
            @"^(?<prefix>.+?)(?: \((?<number>\d+)\))?(?<suffix>\.[^.]+)?$");

        private readonly System.Timers.Timer timer;

        private readonly Rectangle rectangle;

        private readonly Bitmap bitmap;

        private readonly Graphics graphics;

        private readonly Pen pen = new(Color.Black);

        private Point oldLocation = Point.Empty;

        // FIXME: The AviWriter correctly closes this, so don't bother storing
        //        it in a field.
        private Stream? fileStream = null;

        private AviWriter? aviWriter = null;

        private IAviVideoStream? videoStream = null;
    }
}
