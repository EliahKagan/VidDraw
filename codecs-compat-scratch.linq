<Query Kind="Statements">
  <NuGetReference Version="2.1.2">SharpAvi.NetStandard</NuGetReference>
</Query>

using System.Drawing;
using SharpAvi;
using SharpAvi.Codecs;
using SharpAvi.Output;

// Most systems will have no MPEG-4 codecs installed.
Mpeg4VideoEncoderVcm.GetAvailableCodecs().Dump();

Mp3AudioEncoderLame.SetLameDllLocation(@"D:\src\SharpAvi\Sample\lameenc64.dll");

const int IntervalInMilliseconds = 30;

var rectangle = new Rectangle(Point.Empty, new Size(width: 800, height: 600));

IAviVideoStream CreateVideoStream(Codec codec, AviWriter aviWriter)
    => codec switch {
        Codec.Raw
            => aviWriter.AddVideoStream(
                            width: rectangle.Width,
                            height: rectangle.Height),

        Codec.Uncompressed
            => aviWriter.AddUncompressedVideoStream(
                            width: rectangle.Width,
                            height: rectangle.Height),

        //Codec.MotionJpeg
        //    => aviWriter.AddMotionJpegVideoStream(
        //                    width: rectangle.Width,
        //                    height: rectangle.Height),

        Codec.H264
            => aviWriter.AddMpeg4VideoStream(
                            width: rectangle.Width,
                            height: rectangle.Height,
                            fps: 1000.0 / IntervalInMilliseconds,
                            codec: KnownFourCCs.Codecs.X264),
        
        _ => throw new Exception(),//
    };

internal enum Codec : uint {
    Raw,
    Uncompressed,
    MotionJpeg,
    H264,
}
