using System;

namespace VidDraw {
    /// <summary>
    /// Provides video metadata for the <see cref="Recorder.Recorded"/> event.
    /// </summary>
    internal sealed class RecordedEventArgs : EventArgs {
        internal RecordedEventArgs(string? name, Codec codec)
            => (Name, Codec) = (name, codec);

        internal string? Name { get; }

        internal Codec Codec { get; }
    }
}
