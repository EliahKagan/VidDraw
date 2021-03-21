<Query Kind="Statements">
  <NuGetReference Version="2.1.2">SharpAvi.NetStandard</NuGetReference>
</Query>

using SharpAvi.Codecs;

// Most systems will have no MPEG-4 codecs installed.
Mpeg4VideoEncoderVcm.GetAvailableCodecs().Dump();
