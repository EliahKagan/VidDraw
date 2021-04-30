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
            this.bitmap = bitmap;

            metadata = bitmap.LockBits(rectangle,
                                       ImageLockMode.ReadOnly,
                                       PixelFormat.Format32bppArgb);
        }

        internal void Dispose() => bitmap.UnlockBits(metadata);

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
            => new ReadOnlySpan<byte>(metadata.Scan0.ToPointer(), SizeInBytes);

        private int Width => metadata.Width;

        private int Height => metadata.Height;

        private int BytesPerRow => Width * 4;

        private int SizeInBytes => BytesPerRow * Height;

        private readonly Bitmap bitmap;

        private readonly BitmapData metadata;
    }
}
