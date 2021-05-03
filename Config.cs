using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;

namespace VidDraw {
    internal record Config(Codec? Codec, Color? Color) {
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
            } catch (SystemException ex)
                    when (ex is IOException or UnauthorizedAccessException) {
                Debug.Print($"Can't create config dir: {ex.Message}");
                return null;
            }

            return Path.Combine(dir, ConfigFilename);
        }

        private Config PatchedBy(Config delta)
            => new(delta.Codec ?? Codec,
                   delta.Color ?? Color);

        private static Config TryRead()
        {

        }

        private void TryWrite()
        {

        }
    }
}
