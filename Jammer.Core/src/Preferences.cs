using System.Text.Json;
using System.IO;
using System.Runtime.InteropServices;


namespace Jammer
{
    public enum LoopType
    {
        None,
        Once,
        Always
    }
    public enum BackEndTypeYT
    {
        YoutubeExplode,
        YoutubeDL
    }
    public enum SpotifyResolutionProvider
    {
        YouTube,
        SoundCloud
    }

    public class Preferences
    {
        public static int rewindSeconds = GetRewindSeconds();
        public static int forwardSeconds = GetForwardSeconds();
        public static float volume = GetVolume();
        public static float changeVolumeBy = GetChangeVolumeBy();
        public static float oldVolume = GetOldVolume();
        public static LoopType loopType = GetLoopType();
        public static bool isMuted = GetIsMuted();
        public static bool isShuffle = GetIsShuffle();
        public static bool isAutoSave = GetIsAutoSave();
        public static string? localeLanguage = GetLocaleLanguage();
        public static string songsPath = GetSongsPath();
        public static bool isMediaButtons = GetIsMediaButtons();
        public static bool isVisualizer = GetIsVisualizer();
        public static string theme = GetTheme();
        public static string currentSf2 = GetCurrentSf2();
        public static string clientID = GetClientId();
        public static bool isModifierKeyHelper = GetModifierKeyHelper();
        public static bool isSkipErrors = GetIsSkipErrors();
        public static bool showPlaylistPosition = GetShowPlaylistPosition();
        public static bool rssSkipAfterTime = GetRssSkipAfterTime();
        public static int rssSkipAfterTimeValue = GetRssSkipAfterTimeValue();
        public static BackEndTypeYT backEndType = GetBackEndType();
        public static bool isQuickSearch = GetEnableQuickSearch();
        public static bool favoriteExplainer = GetFavoriteExplainer();
        public static bool isQuickPlayFromSearch = GetEnableQuickPlayFromSearch();
        public static string spotifyClientID = GetSpotifyClientId();
        public static SpotifyResolutionProvider spotifyResolutionProvider = GetSpotifyResolutionProvider();
        public static int intInputStep = GetIntInputStep();
        public static float floatInputStep = GetFloatInputStep();

        private const int DefaultFavoriteNotificationTimeoutMs = 1000;

        private static bool GetModifierKeyHelper()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.modifierKeyHelper ?? false;
            }
            else
            {
                return false;
            }
        }

        private static bool GetIsSkipErrors()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isIgnoreErrors ?? false;
            }
            else
            {
                return false;
            }
        }

        private static bool GetShowPlaylistPosition()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.showPlaylistPosition ?? false;
            }
            else
            {
                return false;
            }
        }

        private static bool GetRssSkipAfterTime()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.rssSkipAfterTime ?? false;
            }
            else
            {
                return false;
            }
        }

        private static int GetRssSkipAfterTimeValue()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.rssSkipAfterTimeValue ?? 5;
            }
            else
            {
                return 5;
            }
        }

        private static bool GetFavoriteExplainer()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.favoriteExplainer ?? true;
            }
            else
            {
                return true;
            }
        }

        private static bool GetEnableQuickPlayFromSearch()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.EnableQuickPlayFromSearch ?? false;
            }
            else
            {
                return false;
            }
        }

        static public void CheckJammerFolderExists()
        {
            string JammerPath = Utils.JammerPath;

            if (!Directory.Exists(JammerPath))
            {
                Log.Error("Jammer folder does not exist, creating one...");
                Directory.CreateDirectory(JammerPath);
            }
            if (!Directory.Exists(GetPlaylistsPath()))
            {
                Log.Error("Playlists folder does not exist, creating one...");
                Directory.CreateDirectory(GetPlaylistsPath());
            }
            if (!Directory.Exists(Utils.SoundFontsPath))
            {
                Log.Error("Soundfonts folder does not exist, creating one...");
                Directory.CreateDirectory(Utils.SoundFontsPath);
            }
            if (!Directory.Exists(Utils.LocalesPath))
            {
                Log.Error("Locales folder does not exist, creating one...");
                Directory.CreateDirectory(Utils.LocalesPath);
            }
            Directory.CreateDirectory(Utils.ToolsPath);
            Directory.CreateDirectory(Utils.DownloadsPath);
            Directory.CreateDirectory(Utils.CachePath);

            IniFileHandling.EnsureLocaleFilesAvailable();


            // check if settings.json has every data
            SaveSettings();

            Log.Info("Loading Effects.ini");
            // Effects.ini
            Effects.WriteEffects();
            Effects.ReadEffects();

            Log.Info("Loading Visualizer.ini");
            // Visualizer.ini
            Visual.Write();
            Visual.Read();

            if (!Directory.Exists(songsPath))
            {
                Log.Error("Songs folder does not exist, creating one...");
                Directory.CreateDirectory(songsPath);
            }

            // load if not folder empty
            if (Directory.EnumerateFiles(Utils.LocalesPath, "*.ini").Any())
            {
                // load current locale
                IniFileHandling.SetLocaleData();
            }

            SpotifyIntegrationService.ResumePendingResolutions();


        }

        static public void SaveSettings()
        {
            string JammerPath = Utils.SettingsFilePath;
            Settings settings = new Settings();
            settings.LoopType = loopType;
            settings.Volume = volume;
            settings.isMuted = isMuted;
            settings.OldVolume = oldVolume;
            settings.forwardSeconds = forwardSeconds;
            settings.rewindSeconds = rewindSeconds;
            settings.changeVolumeBy = changeVolumeBy;
            settings.isShuffle = isShuffle;
            settings.isMediaButtons = isMediaButtons;
            settings.isAutoSave = isAutoSave;
            settings.localeLanguage = localeLanguage;
            // settings.songsPath = songsPath;
            settings.isVisualizer = isVisualizer;
            settings.theme = theme;
            settings.currentSf2 = currentSf2;
            settings.clientID = clientID;
            settings.modifierKeyHelper = isModifierKeyHelper;
            settings.isIgnoreErrors = isSkipErrors;
            settings.showPlaylistPosition = showPlaylistPosition;
            settings.rssSkipAfterTime = rssSkipAfterTime;
            settings.rssSkipAfterTimeValue = rssSkipAfterTimeValue;
            settings.backEndType = backEndType;
            settings.EnableQuickSearch = isQuickSearch;
            settings.favoriteExplainer = favoriteExplainer;
            settings.EnableQuickPlayFromSearch = isQuickPlayFromSearch;
            settings.spotifyClientID = spotifyClientID;
            settings.spotifyResolutionProvider = spotifyResolutionProvider;
            settings.intInputStep = intInputStep;
            settings.floatInputStep = floatInputStep;

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string jsonString = JsonSerializer.Serialize(settings, options);
            // delete file if exists
            if (File.Exists(JammerPath))
            {
                File.Delete(JammerPath);
            }
            File.WriteAllText(JammerPath, jsonString, System.Text.Encoding.UTF8);
        }

        static public string GetSongsPath()
        {
            string? configuredPath = Environment.GetEnvironmentVariable("JAMMER_SONGS_PATH");
            if (!string.IsNullOrEmpty(configuredPath))
            {
                return configuredPath;
            }

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")))
            {
                return Utils.SongsPath;
            }

            string value = "";
            if (File.Exists(Utils.SettingsFilePath))
            {
                string jsonString = File.ReadAllText(Utils.SettingsFilePath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                value = settings?.songsPath ?? "";
            }

            if (!string.IsNullOrEmpty(value) && value != Utils.SongsPath)
            {
                string val = Message.Input(
                    string.Format(Locale.UiMessages.SongsPathMigration, Locale.Miscellaneous.NoAnswer, Locale.Miscellaneous.YesAnswer),
                    string.Format(Locale.UiMessages.CurrentSongsPath, Path.Combine(value, "songs")),
                    true).ToLower();
                if (val == Locale.Miscellaneous.NoAnswer)
                {
                    Environment.Exit(0);
                }
            }

            return Utils.SongsPath;
        }

        static public string GetPlaylistsPath()
        {
            string? configuredPath = Environment.GetEnvironmentVariable("JAMMER_PLAYLISTS_PATH");
            return !string.IsNullOrEmpty(configuredPath) ? configuredPath : Utils.PlaylistsPath;
        }

        static public LoopType GetLoopType()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.LoopType ?? LoopType.None;
            }
            else
            {
                return LoopType.None;
            }
        }

        static public BackEndTypeYT GetBackEndType()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.backEndType ?? BackEndTypeYT.YoutubeExplode;
            }
            else
            {
                return BackEndTypeYT.YoutubeExplode;
            }
        }

        static public bool GetEnableQuickSearch()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.EnableQuickSearch ?? true;
            }
            else
            {
                return true;
            }
        }

        static public string GetTheme()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.theme ?? "light";
            }
            else
            {
                return "Jammer Default";
            }
        }

        static public bool GetIsVisualizer()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isVisualizer ?? true;
            }
            else
            {
                return true;
            }
        }

        static public float GetVolume()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.Volume ?? 0.5f;
            }
            else
            {
                return 0.5f;
            }
        }

        static public bool GetIsMuted()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isMuted ?? false;
            }
            else
            {
                return false;
            }
        }

        static public float GetOldVolume()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.OldVolume ?? 0.5f;
            }
            else
            {
                return 0.5f;
            }
        }

        static public int GetForwardSeconds()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.forwardSeconds ?? 5;
            }
            else
            {
                return 5;
            }
        }

        static public int GetRewindSeconds()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.rewindSeconds ?? 5;
            }
            else
            {
                return 5;
            }
        }

        static public float GetChangeVolumeBy()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                try
                {
                    string jsonString = File.ReadAllText(JammerPath);
                    Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                    return settings?.changeVolumeBy ?? 0.05f;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    return 0.05f;
                }
            }
            else
            {
                return 0.05f;
            }
        }

        static public bool GetIsMediaButtons()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isMediaButtons ?? true;
            }
            else
            {
                return true;
            }
        }

        static public bool GetIsShuffle()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isShuffle ?? false;
            }
            else
            {
                return false;
            }
        }


        static public string? GetLocaleLanguage()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.localeLanguage;
            }
            else
            {
                return "en";
            }
        }

        static public bool GetIsAutoSave()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isAutoSave ?? false;
            }
            else
            {
                return false;
            }
        }

        static public string GetCurrentSf2()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.currentSf2 ?? "";
            }
            else
            {
                return "";
            }
        }

        static public string GetClientId()
        {
            string JammerPath = Utils.SettingsFilePath;
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.clientID ?? "";
            }
            else
            {
                return "";
            }
        }

        static public string GetSpotifyClientId()
        {
            string? environmentClientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
            if (!string.IsNullOrWhiteSpace(environmentClientId)) return environmentClientId.Trim();
            if (!File.Exists(Utils.SettingsFilePath)) return "";
            string jsonString = File.ReadAllText(Utils.SettingsFilePath);
            Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
            return settings?.spotifyClientID ?? "";
        }

        static public SpotifyResolutionProvider GetSpotifyResolutionProvider()
        {
            if (!File.Exists(Utils.SettingsFilePath)) return SpotifyResolutionProvider.YouTube;
            string jsonString = File.ReadAllText(Utils.SettingsFilePath);
            Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
            return settings?.spotifyResolutionProvider ?? SpotifyResolutionProvider.YouTube;
        }

        static public int GetIntInputStep()
        {
            if (!File.Exists(Utils.SettingsFilePath)) return 1;
            string jsonString = File.ReadAllText(Utils.SettingsFilePath);
            Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
            return settings?.intInputStep ?? 1;
        }

        static public float GetFloatInputStep()
        {
            if (!File.Exists(Utils.SettingsFilePath)) return 0.1f;
            string jsonString = File.ReadAllText(Utils.SettingsFilePath);
            Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
            return settings?.floatInputStep ?? 0.1f;
        }

        static public void OpenJammerFolder()
        {
            string JammerPath = Utils.JammerPath;
            // start file managert in the given operating system
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.Diagnostics.Process.Start("explorer.exe", JammerPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                System.Diagnostics.Process.Start("xdg-open", JammerPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                System.Diagnostics.Process.Start("open", JammerPath);
            }
        }

        public static long DirSize(System.IO.DirectoryInfo d)
        {

            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            System.IO.DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        public static double ToKilobytes(long bytes) => bytes / 1024d;
        public static double ToMegabytes(long bytes) => ToKilobytes(bytes) / 1024d;
        public static double ToGigabytes(long bytes) => ToMegabytes(bytes) / 1024d;

        public class Settings
        {
            public LoopType? LoopType { get; set; }
            public float? Volume { get; set; }
            public float? OldVolume { get; set; }
            public bool? isMuted { get; set; }
            public int? forwardSeconds { get; set; }
            public int? rewindSeconds { get; set; }
            public float changeVolumeBy { get; set; }
            public bool? isShuffle { get; set; }
            public bool? isMediaButtons { get; set; }
            public bool? isAutoSave { get; set; }
            public string? localeLanguage { get; set; }
            // old songs path, used in the migration process, if its set already
            public string? songsPath { get; set; }
            public bool? isVisualizer { get; set; }
            public string? theme { get; set; }
            public string? currentSf2 { get; set; }
            public string? clientID { get; set; }
            public bool? modifierKeyHelper { get; set; }
            public bool? isIgnoreErrors { get; set; }
            public bool? showPlaylistPosition { get; set; }
            public bool? rssSkipAfterTime { get; set; }
            public int? rssSkipAfterTimeValue { get; set; }
            public BackEndTypeYT? backEndType { get; set; }
            public bool? EnableQuickSearch { get; set; }
            public bool? favoriteExplainer { get; set; }
            public bool? EnableQuickPlayFromSearch { get; set; }
            public string? spotifyClientID { get; set; }
            public SpotifyResolutionProvider? spotifyResolutionProvider { get; set; }
            public int? intInputStep { get; set; }
            public float? floatInputStep { get; set; }
        }
    }
}
