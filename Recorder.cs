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
            _bitmap = bitmap;
            _rectangle = new(Point.Empty, bitmap.Size);
            _buffer = new byte[_rectangle.Width * _rectangle.Height * 4];

            _timer = new(interval: IntervalInMilliseconds) {
                AutoReset = true,
                Enabled = false,
                SynchronizingObject = synchronizingObject,
            };

            _timer.Elapsed += (_, _) => CaptureFrame();
        }

        public void Dispose()
        {
            if (IsRunning) Finish();
            _timer.Dispose();
        }

        internal bool IsRunning => _job is not null;

        internal event EventHandler<RecordedEventArgs>? Recorded;

        internal void Start(Stream outputStream,
                            Codec codec,
                            string? name = null)
        {
            if (IsRunning) {
                throw new InvalidOperationException(
                        "Can't start: already recording");
            }

            var aviWriter = CreateAviWriter(outputStream);

            _job = new(AviWriter: aviWriter,
                       VideoStream: CreateVideoStream(aviWriter, codec),
                       Codec: codec,
                       Name: name ?? (outputStream as FileStream)?.Name);

            CaptureFrame(); // Ensure we always get an initial frame.
            _timer.Enabled = true;
        }

        internal void Finish()
        {
            var job = _job ?? throw new InvalidOperationException(
                                "Can't finish: not recording");

            _timer.Enabled = false;
            _job = null;
            job.AviWriter.Close();
            Recorded?.Invoke(this, new(job.Name, job.Codec));
        }

        private sealed record Job(AviWriter AviWriter,
                                  IAviVideoStream VideoStream,
                                  Codec Codec,
                                  string? Name);

        private const int IntervalInMilliseconds = 30;

        private static AviWriter CreateAviWriter(Stream outputStream)
            => new(outputStream, leaveOpen: false) {
                FramesPerSecond = 1000m / IntervalInMilliseconds,
                EmitIndex1 = true,
            };

        // TODO: Let the user set/adjust the quality of Motion JPEG and H.264.
        private IAviVideoStream CreateVideoStream(AviWriter aviWriter,
                                                  Codec codec)
        {
            Debug.Assert(aviWriter is not null);

            return codec switch {
                Codec.Raw
                    => aviWriter.AddVideoStream(
                                    width: _rectangle.Width,
                                    height: _rectangle.Height),

                Codec.Uncompressed
                    => aviWriter.AddUncompressedVideoStream(
                                    width: _rectangle.Width,
                                    height: _rectangle.Height),

                Codec.MotionJpeg
                    => aviWriter.AddMotionJpegVideoStream(
                                    width: _rectangle.Width,
                                    height: _rectangle.Height,
                                    quality: 100),

                Codec.H264
                    => aviWriter.AddMpeg4VideoStream(
                                    width: _rectangle.Width,
                                    height: _rectangle.Height,
                                    fps: 1000.0 / IntervalInMilliseconds,
                                    codec: KnownFourCCs.Codecs.X264),

                _ => throw new InvalidOperationException(
                        "Unrecognized codec enumerator"),
            };
        }

        private void CaptureFrame()
        {
            if (_job is null) return;

            using (var lb = new LockedBits(_bitmap, _rectangle)) {
                if (_job.Codec is Codec.Raw) {
                    lb.UpsideDownCopyTo(_buffer);
                } else {
                    lb.CopyTo(_buffer);
                }
            }

            _job.VideoStream.WriteFrame(true, _buffer, 0, _buffer.Length);
        }

        private readonly Bitmap _bitmap;

        private readonly Rectangle _rectangle;

        private readonly byte[] _buffer;

        private readonly Timer _timer;

        private Job? _job;
    }
}
