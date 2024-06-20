/* using ManagedBass; */
/* using System; */
/* using System.IO; */
/* using System.Text.RegularExpressions; */
using System.IO;
namespace Jammer
{
    public struct Utils
    {
        /// <summary>
        /// for avalonia
        /// </summary>
        public static List<string> oldPlaylist = new ();
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
        public static string JammerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "jammer");
        public static bool isDebug = false;
        public static string currentPlaylist = "";
        public static bool isInitialized = false;
        public static string version = "2.8.2.3";
        public static string? AppDirMount = Environment.GetEnvironmentVariable("APPDIR");
        public static float MusicTimePercentage = 0;
    }
}
