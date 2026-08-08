using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jammer
{
    /// <summary>
    /// Loads and saves per-song playback metadata from metadata.json in the Jammer folder.
    /// </summary>
    public static class SongMetadataStore
    {
        public static string FilePath => Path.Combine(Utils.JammerPath, "metadata.json");

        private static Dictionary<string, SongPlaybackMetadata> _metadata = new();
        private static bool _loaded = false;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Returns a stable lookup key for a song URI/path.
        /// </summary>
        public static string GetKey(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return string.Empty;

            if (Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
            {
                return parsed.ToString().ToLowerInvariant();
            }

            try
            {
                return Path.GetFullPath(uri).ToLowerInvariant();
            }
            catch
            {
                return uri.ToLowerInvariant();
            }
        }

        /// <summary>
        /// Loads metadata.json from disk. Safe to call multiple times.
        /// </summary>
        public static void Load()
        {
            if (_loaded)
                return;

            _metadata = new Dictionary<string, SongPlaybackMetadata>();

            if (File.Exists(FilePath))
            {
                try
                {
                    string json = File.ReadAllText(FilePath);
                    var deserialized = JsonSerializer.Deserialize<Dictionary<string, SongPlaybackMetadata>>(json, JsonOptions);
                    if (deserialized != null)
                    {
                        _metadata = deserialized;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to load {FilePath}: {ex}");
                }
            }

            _loaded = true;
        }

        /// <summary>
        /// Saves the current metadata dictionary to disk.
        /// </summary>
        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                string json = JsonSerializer.Serialize(_metadata, JsonOptions);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to save {FilePath}: {ex}");
            }
        }

        /// <summary>
        /// Gets metadata for a song, returning defaults if none exists.
        /// </summary>
        public static SongPlaybackMetadata Get(string uri)
        {
            Load();
            string key = GetKey(uri);
            if (_metadata.TryGetValue(key, out var meta))
                return meta;

            return new SongPlaybackMetadata();
        }

        /// <summary>
        /// Sets metadata for a song and persists it.
        /// </summary>
        public static void Set(string uri, SongPlaybackMetadata metadata)
        {
            Load();
            string key = GetKey(uri);

            bool isDefault =
                metadata.Speed == 1.0f &&
                metadata.Pitch == 0.0f &&
                !metadata.Reversed &&
                string.IsNullOrWhiteSpace(metadata.TrimStart) &&
                string.IsNullOrWhiteSpace(metadata.TrimEnd) &&
                !metadata.UseCustomEffects;

            if (isDefault)
            {
                if (_metadata.Remove(key))
                {
                    Save();
                }
            }
            else
            {
                _metadata[key] = metadata;
                Save();
            }
        }

        /// <summary>
        /// Removes metadata for a song.
        /// </summary>
        public static void Remove(string uri)
        {
            Load();
            string key = GetKey(uri);
            if (_metadata.Remove(key))
            {
                Save();
            }
        }
    }
}
