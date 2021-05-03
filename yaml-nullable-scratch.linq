<Query Kind="Statements">
  <NuGetReference Version="11.1.1">YamlDotNet</NuGetReference>
</Query>

using System.Drawing;
using YamlDotNet.Serialization;

//new Config { Color = Color.Red }.TrySave();
//Config.TryLoad().Dump();
//new Config { Codec = Codec.H264 }.TrySave();


// After running this:
new Config {
    Color = Color.Red,
    Codec = Codec.H264,
}.TrySave();

// This fails due to https://github.com/aaubry/YamlDotNet/issues/360:
Config.TryLoad().Dump();

/// <summary>Video stream encoding selections.</summary>
internal enum Codec : uint {
    Raw,
    Uncompressed,
    MotionJpeg,
    H264,
}

internal sealed record Box<T>(T Value) where T : struct {
    public Box() : this(default(T)) { }
}

/// <summary>Partial or complete YAML-backed configuration data.</summary>
internal sealed record Config(Codec? Codec, Color? Color) {
    public Config() : this(null, null) { }

    internal static Config TryLoad()
    {
        //using var @lock = new Lock(Mutex);
        return TryRead();
    }

    internal void TrySave()
    {
        //using var @lock = new Lock(Mutex);
        TryRead().PatchedBy(this).TryWrite();
    }

    private sealed record BoxConfig(Box<Codec>? Codec, Box<Color>? Color) {
        public BoxConfig() : this(null, null) { }

        internal Config Debox() => new(Codec?.Value, Color?.Value);
    }

    private BoxConfig Enbox()
        => new(Codec: (Codec is Codec codec ? new Box<Codec>(codec) : null),
               Color: (Color is Color color ? new Box<Color>(color) : null));

    private const string ProgramName = "VidDraw-scratch";

    private const string ConfigFilename = ProgramName + ".yml";

    private static string AppDataDir
        => Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData);

    private static string ConfigDir
        => Path.Combine(AppDataDir, ProgramName);

    //private static Mutex Mutex { get; } =
    //    Sync.CreateMutex($"APPDATA::{ConfigFilename}");

    private static string GetConfigPath()
        => Path.Combine(ConfigDir, ConfigFilename);

    private static string? TryCreateConfigPath()
    {
        var dir = ConfigDir;

        try {
            // If it already exists, this does nothing and succeeds.
            Directory.CreateDirectory(dir);
        } catch (SystemException ex) when (IsNonFatal(ex)) {
            Debug.Print($"Can't create config dir: {ex.Message}");
            return null;
        }

        return Path.Combine(dir, ConfigFilename);
    }

    private static string TrySlurpConfig()
    {
        try {
            return File.ReadAllText(GetConfigPath(), Encoding.UTF8);
        } catch (SystemException ex) when (IsNonFatal(ex)) {
            Debug.Print($"Can't read config file: {ex.Message}");
            return string.Empty;
        }
    }

    private static bool IsNonFatal(SystemException ex)
        => ex is IOException or UnauthorizedAccessException;

    private Config PatchedBy(Config delta)
        => new(delta.Codec ?? Codec,
               delta.Color ?? Color);

    // FIXME: Cache Serializer and Deserializer instances thread-locally
    // instead of constructing them every time in TryRead and TryWrite.

    private static Config TryRead()
        => new DeserializerBuilder()
            //.IgnoreUnmatchedProperties() // Commented to reveal bug 360 (see above).
            .Build()
            .Deserialize<BoxConfig>(TrySlurpConfig())
            ?.Debox()
        ?? new();

    private void TryWrite()
    {
        if (TryCreateConfigPath() is string path) {
            File.WriteAllText(path: path,
                              contents: new Serializer().Serialize(Enbox()),
                              encoding: Encoding.UTF8);
        }
    }
}
