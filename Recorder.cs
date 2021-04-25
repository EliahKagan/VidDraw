using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Timers;
using SharpAvi;
using SharpAvi.Codecs;
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

        internal void Start(Stream outputStream,
                            Codec codec,
                            Action? onFinish = null)
        {
            if (IsRunning) {
                throw new InvalidOperationException(
                        "Can't start: already recording");
            }

            aviWriter = CreateAviWriter(outputStream);
            videoStream = CreateVideoStream(codec);
            flip = codec is Codec.Raw;
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

        private AviWriter CreateAviWriter(Stream outputStream)
            => new(outputStream, leaveOpen: false) {
                FramesPerSecond = 1000m / IntervalInMilliseconds,
                EmitIndex1 = true,
            };

        // TODO: Let the user set/adjust the quality of Motion JPEG and H.264.
        private IAviVideoStream CreateVideoStream(Codec codec)
        {
            Debug.Assert(aviWriter is not null);

            return codec switch {
                Codec.Raw
                    => aviWriter.AddVideoStream(
                                    width: rectangle.Width,
                                    height: rectangle.Height),

                Codec.Uncompressed
                    => aviWriter.AddUncompressedVideoStream(
                                    width: rectangle.Width,
                                    height: rectangle.Height),

                Codec.MotionJpeg
                    => aviWriter.AddMotionJpegVideoStream(
                                    width: rectangle.Width,
                                    height: rectangle.Height,
                                    quality: 100),

                Codec.H264
                    => aviWriter.AddMpeg4VideoStream(
                                    width: rectangle.Width,
                                    height: rectangle.Height,
                                    fps: 1000.0 / IntervalInMilliseconds,
                                    codec: KnownFourCCs.Codecs.X264),

                _ => throw new InvalidOperationException(
                        "Unrecognized codec enumerator"),
            };
        }

        private void CaptureFrame()
        {
            if (videoStream is null) return;

            using (var lb = new LockedBits(bitmap, rectangle)) {
                if (flip) {
                    lb.UpsideDownCopyTo(buffer);
                } else {
                    lb.CopyTo(buffer);
                }
            }

            videoStream.WriteFrame(true, buffer, 0, buffer.Length);
        }

        private readonly Bitmap bitmap;

        private readonly Rectangle rectangle;

        private readonly byte[] buffer;

        private readonly Timer timer;

        private AviWriter? aviWriter = null;

        private IAviVideoStream? videoStream = null;

        private bool flip = false;

        private Action? onFinish = null;
    }
}
