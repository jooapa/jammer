
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Jammer
{
    public struct Utils
    {
        public static int currentMusic { get; set; }
        public static string[] songs = { "" };
        public static List<string> queueSongs = new List<string>();
        /// <summary>
        /// current path to song
        /// </summary>
        public static string currentSong = ""; // 
        /// <summary>
        /// length in seconds
        /// </summary>
        public static double currentMusicLength = 0;
        /// <summary>
        ///  time played in seconds
        /// </summary>
        public static double MusicTimePlayed = 0;
        public static bool curSongError = false;
        public static double preciseTime = 0;
        public static int currentSongIndex = 0;
        public static int previousSongIndex = 0;
        public static int currentPlaylistSongIndex = 0;
        public static string scSongPattern = @"^(https?:\/\/)?(www\.)?(soundcloud\.com|snd\.sc)\/(.*)$";
        public static string scPlaylistPattern = @"^https?:\/\/(?:www\.)?soundcloud\.com\/[^\/]+\/sets\/[^\/]+$";
        public static string ytSongPattern = @"^(https?:\/\/)?(www\.)?(youtube\.com|youtu\.be)\/(.*)$";
        public static string ytPlaylistPattern = @"^https?:\/\/(?:www\.)?youtube\.com\/playlist\?list=[\w-]+$";
        public static string urlPatternHTTPS = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)";
        public static string urlPatternHTTP = @"http?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)";
        public static bool mainLoop = true;
        public static string JammerPath = UtilFuncs.GetJammerPath();
        public static bool isDebug = false;

        /// <summary>
        /// path to current playlist
        /// </summary>
        public static string currentPlaylist = "";
        public static string jammerFileDelimeter = "?|";
        public static bool isInitialized = false;
        public static string version = "3.0.0.0";
        public static string? AppDirMount = Environment.GetEnvironmentVariable("APPDIR");
        public static float MusicTimePercentage = 0;

        // Class to hold Util related Functions
        public static class UtilFuncs {
            // Return user preferred path for JammerPath
            public static string GetJammerPath() {
                string defaultJammerFolderName = "jammer";
                // use xdg_config_home if it is set
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("XDG_CONFIG_HOME"))) {
                    return Path.Combine(Environment.GetEnvironmentVariable("XDG_CONFIG_HOME"), defaultJammerFolderName);
                }

                // use JAMMER_CONFIG_PATH if it is set
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAMMER_CONFIG_PATH"))) {
                    return Environment.GetEnvironmentVariable("JAMMER_CONFIG_PATH");
                }

                // use the default user profile path
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), defaultJammerFolderName);
            }
        }
    }
}
