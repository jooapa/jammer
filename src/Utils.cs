/* using ManagedBass; */
/* using System; */
/* using System.IO; */
/* using System.Text.RegularExpressions; */

namespace jammer
{
    public struct Utils
    {
        public static int currentMusic { get; set; }
        public static string[] songs = { "" };
        public static string currentSong = ""; // current path to song
        public static double currentMusicLength = 0; // length in seconds
        public static double MusicTimePlayed = 0; // time played in seconds
        public static double preciseTime = 0;
        public static int currentSongIndex = 0;
        public static string scSongPattern = @"^(https?:\/\/)?(www\.)?(soundcloud\.com|snd\.sc)\/(.*)$";
        public static string scPlaylistPattern = @"^https?:\/\/(?:www\.)?soundcloud\.com\/[^\/]+\/sets\/[^\/]+$";
        public static string ytSongPattern = @"^(https?:\/\/)?(www\.)?(youtube\.com|youtu\.be)\/(.*)$";
        public static string urlPatternHTTPS = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)";
        public static string urlPatternHTTP = @"http?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)";
        public static bool mainLoop = true;
        public static string jammerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "jammer");
        public static bool isDebug = false;
        public static string currentPlaylist = "";
        public static bool isInitialized = false;
        public static string version = "2.0.2";
        public static string AppDirMount = Environment.GetEnvironmentVariable("APPDIR");
    }
}
