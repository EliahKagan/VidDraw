<Query Kind="Statements">
  <NuGetReference Version="0.0.9">Nier.Commons</NuGetReference>
</Query>

using Nier.Commons.Collections;

var idToCodec = new BiDirectionDictionary<MyMenuItemId, Codec> {
    { MyMenuItemId.Raw, Codec.Raw },
    { MyMenuItemId.Uncompressed, Codec.Uncompressed },
    { MyMenuItemId.MotionJpeg, Codec.MotionJpeg },
    { MyMenuItemId.H264, Codec.H264 },
};

idToCodec.Dump(nameof(idToCodec));

idToCodec[MyMenuItemId.MotionJpeg]
    .Display("idToCodec[MyMenuItemId.MotionJpeg]");

idToCodec.ReverseDirection[Codec.MotionJpeg]
    .Display("idToCodec.ReverseDirection[Codec.MotionJpeg]");


internal enum MyMenuItemId : uint {
    UnusedId, // For clarity, pass this when the ID will be ignored.

    Raw,
    Uncompressed,
    MotionJpeg,
    H264,

    PickColor,
    About,
}

internal enum Codec : uint {
    Raw,
    Uncompressed,
    MotionJpeg,
    H264,
}

internal static class Extensions {
    internal static void Display<T>(this T self, string? label = null)
            where T : notnull
        => $"{self.GetType()}.{self}".Dump(label);
}
