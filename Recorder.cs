using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Timers;
using SharpAvi.Output;

namespace VidDraw {
    /// <summary>Captures frames from a bitmap as AVI video.</summary>
    /// <remarks>This class is single-threaded.</remarks>
    internal sealed class Recorder : IDisposable {
        internal Recorder(Bitmap bitmap,
                          ISynchronizeInvoke synchronizingObject)
        {
            this.bitmap = bitmap;
            rectangle = new(Point.Empty, bitmap.Size);
            buffer = new byte[rectangle.Width * rectangle.Height * 4];

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

        internal void Start(Stream outputStream, Action? onFinish = null)
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

            this.onFinish = onFinish;

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

            if (this.onFinish is Action onFinish) {
                this.onFinish = null;
                onFinish();
            }
        }

        private const int IntervalInMilliseconds = 30;

        private void CaptureFrame()
        {
            if (videoStream is null) return;

            using (var lb = new LockedBits(bitmap, rectangle))
                lb.UpsideDownCopyTo(buffer);

            videoStream.WriteFrame(true, buffer, 0, buffer.Length);
        }

        private readonly Bitmap bitmap;

        private readonly Rectangle rectangle;

        private readonly byte[] buffer;

        private readonly Timer timer;

        private AviWriter? aviWriter = null;

        private IAviVideoStream? videoStream = null;

        private Action? onFinish = null;
    }
}
