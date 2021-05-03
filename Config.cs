using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using YamlDotNet.Serialization;

namespace VidDraw {
    /// <summary>Partial or complete YAML-backed configuration data.</summary>
    internal sealed record Config(Codec? Codec, Color? Color) {
        public Config() : this(null, null) { }

        internal static Config TryLoad()
        {
            using var @lock = new Lock(Mutex);
            return TryRead();
        }

        internal void TrySave()
        {
            using var @lock = new Lock(Mutex);
            TryRead().PatchedBy(this).TryWrite();
        }

        private const string ProgramName = "VidDraw";

        private const string ConfigFilename = ProgramName + ".yml";

        private static string AppDataDir
            => Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData);

        private static string ConfigDir
            => Path.Combine(AppDataDir, ProgramName);

        private static Mutex Mutex { get; } =
            Sync.CreateMutex($"APPDATA::{ConfigFilename}");

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
                .IgnoreUnmatchedProperties()
                .Build()
                .Deserialize<Config>(TrySlurpConfig())
            ?? new();

        private void TryWrite()
        {
            if (TryCreateConfigPath() is string path) {
                File.WriteAllText(path: path,
                                  contents: new Serializer().Serialize(this),
                                  encoding: Encoding.UTF8);
            }
        }
    }
}
