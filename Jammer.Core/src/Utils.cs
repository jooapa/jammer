
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;


namespace Jammer
{
    public struct Utils
    {
        public static int CurrentMusic { get; set; }
        public static string[] Songs = { "" };
        public static List<string> QueueSongs = new List<string>();
        /// <summary>
        /// current path to song
        /// </summary>
        public static string CurrentSongPath = ""; // 
        /// <summary>
        /// length in seconds
        /// </summary>
        public static double SongDurationInSec = 0;
        /// <summary>
        ///  time played in seconds
        /// </summary>
        public static double TotalMusicDurationInSec = 0;
        public static bool CurSongError = false;
        public static double PreciseTime = 0;
        public static int CurrentSongIndex = 0;
        public static int PreviousSongIndex = 0;
        public static int CurrentPlaylistSongIndex = 0;
        public static string SCSongPattern = @"^(https?:\/\/)?(www\.)?(soundcloud\.com|snd\.sc)\/(.*)$";
        public static string SCPlaylistPattern = @"^https?:\/\/(?:www\.)?soundcloud\.com\/[^\/]+\/sets\/[^\/]+$";
        public static string YTSongPattern = @"^(https?:\/\/)?(www\.)?(youtube\.com|youtu\.be)\/(.*)$";
        public static string YTPlaylistPattern = @"^https?:\/\/(?:www\.)?youtube\.com\/playlist\?list=[\w-]+$";
        public static string UrlPatternHTTPS = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)";
        public static string UrlPatternHTTP = @"http?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)";
        public static bool MainLoop = true;
        public static string JammerPath = UtilFuncs.GetJammerPath();
        public static string SongsPath => Path.Combine(JammerPath, "songs");
        public static string PlaylistsPath => Path.Combine(JammerPath, "playlists");
        public static string ToolsPath => GetToolsPath(JammerPath);
        public static string DownloadsPath => Path.Combine(JammerPath, "downloads");
        public static string CachePath => Path.Combine(JammerPath, "cache");
        public static string YtDlpCachePath => Path.Combine(CachePath, "yt-dlp");
        public static string LocalesPath => Path.Combine(JammerPath, "locales");
        public static string SoundFontsPath => Path.Combine(JammerPath, "soundfonts");
        public static string ThemesPath => Path.Combine(JammerPath, "themes");
        public static string PlaylistBackupsPath => GetPlaylistBackupsPath(PlaylistsPath);
        public static string SettingsFilePath => Path.Combine(JammerPath, "settings.json");
        public static string KeyDataFilePath => Path.Combine(JammerPath, "KeyData.ini");
        public static string EffectsFilePath => Path.Combine(JammerPath, "Effects.ini");
        public static string VisualizerFilePath => Path.Combine(JammerPath, "Visualizer.ini");
        public static string GetToolsPath(string jammerPath) => Path.Combine(jammerPath, "tools");
        public static string GetPlaylistBackupsPath(string playlistsPath) => Path.Combine(playlistsPath, "backups");
        public static bool IsDebug = false;
        public static bool SCClientIdAlreadyLookedAndItsIncorrect = false;
        public static string CustomTopErrorMessage = "";
        public static bool PlaylistCheckedForAllTheSongsAndNoneOfThemWereFound = false;

        /// <summary>
        /// path to current playlist
        /// </summary>
        public static string CurrentPlaylist = "";
        public static string JammerFileDelimeter = "?|";
        public static bool IsInitialized = false;
        public static string Version = typeof(Utils).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion.Split('+')[0] ?? "development";
        public static string? AppDirMount = Environment.GetEnvironmentVariable("APPDIR");
        public static float MusicTimePercentage = 0;
        public static Song RssFeedSong = new Song();
        public static int lastPositionInPreviousPlaylist = -1;
        public static string[]? BackUpSongs = null;
        public static string? BackUpPlaylistName = null;
        public static string? RssFeedSavedName = null;


        // Class to hold Util related Functions
        public static class UtilFuncs
        {
            // Return user preferred path for JammerPath
            public static string GetJammerPath()
            {
                const string defaultJammerFolderName = "jammer";
                string? configuredPath = Environment.GetEnvironmentVariable("JAMMER_CONFIG_PATH");
                if (!string.IsNullOrWhiteSpace(configuredPath))
                {
                    return Path.GetFullPath(configuredPath);
                }

                string? xdgConfigPath = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
                if (!string.IsNullOrWhiteSpace(xdgConfigPath))
                {
                    return Path.GetFullPath(Path.Combine(xdgConfigPath, defaultJammerFolderName));
                }

                return Path.GetFullPath(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    defaultJammerFolderName));
            }
        }
    }
}
