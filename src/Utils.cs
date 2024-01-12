using NAudio.Wave;

namespace jammer
{
    public struct Utils
    {
        public static WaveOutEvent currentMusic = new WaveOutEvent();
        public static WaveStream? audioStream = null;
        public static string[] songs = { "" };
        public static string currentSong = "";
        public static long currentMusicLength = 0; // length in seconds
        public static double MusicTimePlayed = 0; // time played in seconds
        public static double preciseTime = 0;
        public static int currentSongIndex = 0;
        public static string scSongPattern = @"^(https?:\/\/)?(www\.)?(soundcloud\.com|snd\.sc)\/(.*)$";
        public static string scPlaylistPattern = @"^https?:\/\/(?:www\.)?soundcloud\.com\/[^\/]+\/sets\/[^\/]+$";
        public static string ytSongPattern = @"^(https?:\/\/)?(www\.)?(youtube\.com|youtu\.be)\/(.*)$";
        public static string urlPatternHTTPS = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)";
        public static string urlPatternHTTP = @"http?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)";
        public static bool mainLoop = true;
        public static string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\jammer\\";
        public static bool isDebug = false;
        public static string currentPlaylist = "";
        public static string version = "1.1.6";
    }
}