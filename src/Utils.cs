using Raylib_cs;

namespace jammer
{
    public struct Utils
    {
        public static string[] songs = { "" };
        public static string currentSong = "";
        public static double currentMusicLength = 0; // current song length in seconds
        public static double MusicTimePlayed = 0;  // current song time played so far
        public static double preciseTime = 0;
        public static Music currentMusic;
        public static int currentSongIndex = 0;
        public static string scSongPattern = @"^(https?:\/\/)?(www\.)?(soundcloud\.com|snd\.sc)\/(.*)$";
        public static string scPlaylistPattern = @"^https?:\/\/(?:www\.)?soundcloud\.com\/[^\/]+\/sets\/[^\/]+$";
        public static string ytSongPattern = @"^(https?:\/\/)?(www\.)?(youtube\.com|youtu\.be)\/(.*)$";
        public static bool mainLoop = true;
        public static string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\jammer\\";
        public static bool isDebug = false;
        public static string currentPlaylist = "";
    }
}