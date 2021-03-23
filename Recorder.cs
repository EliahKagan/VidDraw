using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using SharpAvi.Output;

namespace VidDraw {
    internal sealed class Recorder : IDisposable {
        internal Recorder(Bitmap bitmap,
                          ISynchronizeInvoke synchronizingObject)
        {
            this.bitmap = bitmap;
            rectangle = new(Point.Empty, bitmap.Size);
            buffer = new byte[Height * BytesPerRow];

            timer = new(interval: IntervalInMilliseconds) {
                AutoReset = true,
                Enabled = false,
                SynchronizingObject = synchronizingObject,
            };

            timer.Elapsed += (_, _) => CaptureFrame();
        }

        public void Dispose()
        {
            if (IsRunning) Finish();
            timer.Dispose();
        }

        internal bool IsRunning => aviWriter is not null;

        internal void Start(Stream outputStream)
        {
            if (IsRunning) {
                throw new InvalidOperationException(
                        "Can't start: already recording");
            }

            aviWriter = new(outputStream, leaveOpen: false) {
                FramesPerSecond = 1000m / IntervalInMilliseconds,
                EmitIndex1 = true,
            };

            videoStream = aviWriter.AddVideoStream(width: rectangle.Width,
                                                   height: rectangle.Height);

            CaptureFrame(); // Ensure we always get an initial frame.

            timer.Enabled = true;
        }

        internal void Finish()
        {
            if (!IsRunning) {
                throw new InvalidOperationException(
                        "Can't finish: not recording");
            }

            timer.Enabled = false;

            videoStream = null;

            Debug.Assert(aviWriter is not null);
            aviWriter.Close();
            aviWriter = null;
        }

        private const int IntervalInMilliseconds = 30;

        private int Width => rectangle.Width;

        private int Height => rectangle.Height;

        private int BytesPerRow => Width * 4;

        private void CaptureFrame()
        {
            if (videoStream is null) return;

            var bits = bitmap.LockBits(rectangle,
                                       ImageLockMode.ReadOnly,
                                       PixelFormat.Format32bppArgb);
            try {
                nint bottom = bits.Scan0;

                for (var fromTop = 0; fromTop < Height; ++fromTop) {
                    var fromBottom = Height - (fromTop + 1);

                    Marshal.Copy(source: bottom + fromBottom * BytesPerRow,
                                 destination: buffer,
                                 startIndex: fromTop * BytesPerRow,
                                 length: BytesPerRow);
                }
            } finally {
                bitmap.UnlockBits(bits);
            }

            videoStream.WriteFrame(true, buffer, 0, buffer.Length);
        }

        private readonly Bitmap bitmap;

        private readonly Rectangle rectangle;

        private readonly byte[] buffer;

        private readonly Timer timer;

        private AviWriter? aviWriter = null;

        private IAviVideoStream? videoStream = null;
    }
}
