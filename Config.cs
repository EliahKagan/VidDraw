using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VidDraw {
    /// <summary>Partial or complete JSON-backed configuration data.</summary>
    internal sealed record Config(
            [property: JsonConverter(typeof(StringEnumConverter))]
            Codec? Codec,

            Color? Color,

            int[]? CustomColors) {
        internal static Config TryLoad()
        {
            using var @lock = new Hold(Mutex);
            return TryRead();
        }

        internal Config() : this(null, null, null) { }

        internal void TrySave()
        {
            using var @lock = new Hold(Mutex);
            TryRead().PatchedBy(this).TryWrite();
        }

        private const string ProgramName = "VidDraw";

        private const string ConfigFilename = ProgramName + ".json";

        private static string ConfigDir
            => Path.Combine(Dirs.AppData, ProgramName);

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

        private static Config TryRead()
        {
            var json = TrySlurpConfig();

            try {
                return JsonConvert.DeserializeObject<Config>(json) ?? new();
            } catch (JsonException ex) {
                Debug.Print($"Bad JSON: {ex.Message}");
                return new();
            }
        }

        private Config PatchedBy(Config delta)
            => new(delta.Codec ?? Codec,
                   delta.Color ?? Color,
                   delta.CustomColors ?? CustomColors);

        private void TryWrite()
        {
            if (TryCreateConfigPath() is not string path) return;

            var json = JsonConvert.SerializeObject(this, Formatting.Indented);

            File.WriteAllText(path: path,
                              contents: json,
                              encoding: Encoding.UTF8);
        }
    }
}
