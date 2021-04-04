using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;

namespace VidDraw {
    internal partial class MainWindow : Form {
        public MainWindow()
        {
            InitializeComponent();

            rectangle = new(Point.Empty, canvas.Size);
            bitmap = new(width: rectangle.Width, height: rectangle.Height);
            graphics = Graphics.FromImage(bitmap);
            graphics.FillRectangle(Brushes.White, rectangle);
            canvas.Image = bitmap;
            recorder = new Recorder(bitmap, this);
        }

        private static string MyVideos
            => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

        private static string CurrentPreferredSavePath
            => Path.Combine(
                MyVideos,
                $"VidDraw capture {DateTime.Now:yyyy-MM-dd HH-mm-ss}.avi");

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

        private readonly Rectangle rectangle;

        private readonly Bitmap bitmap;

        private readonly Graphics graphics;

        private readonly Pen pen = new(Color.Black);

        private Point oldLocation = Point.Empty;

        private readonly Recorder recorder;
    }
}
