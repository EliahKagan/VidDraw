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
using System.Drawing;
using System.Drawing.Imaging;

namespace VidDraw {
    /// <summary>
    /// A specialized 2-dimensional span managing lifetime and access to a
    /// region in a 32-bit ARGB bitmap image.
    /// </summary>
    internal readonly ref struct LockedBits {
        internal LockedBits(Bitmap bitmap, Rectangle rectangle)
        {
            _bitmap = bitmap;

            _metadata = bitmap.LockBits(rectangle,
                                        ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppArgb);
        }

        internal void Dispose() => _bitmap.UnlockBits(_metadata);

        internal void CopyTo(Span<byte> buffer)
            => FullData.CopyToSameSize(buffer);

        internal void UpsideDownCopyTo(Span<byte> buffer)
        {
            var data = FullData;

            for (var fromTop = 0; fromTop < Height; ++fromTop) {
                var fromBottom = Height - (fromTop + 1);

                var source = data.Slice(start: fromBottom * BytesPerRow,
                                        length: BytesPerRow);

                var destination = buffer.Slice(start: fromTop * BytesPerRow,
                                               length: BytesPerRow);

                source.CopyToSameSize(destination);
            }
        }

        private unsafe ReadOnlySpan<byte> FullData
            => new(_metadata.Scan0.ToPointer(), SizeInBytes);

        private int Width => _metadata.Width;

        private int Height => _metadata.Height;

        private int BytesPerRow => Width * 4;

        private int SizeInBytes => BytesPerRow * Height;

        private readonly Bitmap _bitmap;

        private readonly BitmapData _metadata;
    }
}
