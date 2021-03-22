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

        internal void Start(FileStream fileStream)
        {
            if (IsRunning) {
                throw new InvalidOperationException(
                        "Can't start: already recording");
            }

            aviWriter = new(fileStream, leaveOpen: false) {
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

        private void CaptureFrame()
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

        private readonly Bitmap bitmap;

        private readonly Rectangle rectangle;

        private readonly Timer timer;

        private AviWriter? aviWriter = null;

        private IAviVideoStream? videoStream = null;
    }
}
