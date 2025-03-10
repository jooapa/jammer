
using System.Text.Json;
using System.Text.Json.Serialization;


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
        public static bool IsDebug = false;

        /// <summary>
        /// path to current playlist
        /// </summary>
        public static string CurrentPlaylist = "";
        public static string JammerFileDelimeter = "?|";
        public static bool IsInitialized = false;
        public static string Version = "3.42";
        public static string? AppDirMount = Environment.GetEnvironmentVariable("APPDIR");
        public static float MusicTimePercentage = 0;

        // Class to hold Util related Functions
        public static class UtilFuncs
        {
            // Return user preferred path for JammerPath
            public static string GetJammerPath()
            {
                string defaultJammerFolderName = "jammer";
                // use xdg_config_home if it is set
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")))
                {
                    return Path.Combine(Environment.GetEnvironmentVariable("XDG_CONFIG_HOME"), defaultJammerFolderName);
                }

                // use JAMMER_CONFIG_PATH if it is set
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAMMER_CONFIG_PATH")))
                {
                    return Environment.GetEnvironmentVariable("JAMMER_CONFIG_PATH");
                }

                // use the default user profile path
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), defaultJammerFolderName);
            }
        }
    }
}
