
using System.Text.Json;


namespace Jammer
{
    public struct Utils
    {
        /// <summary>
        /// for avalonia
        /// </summary>
        public static List<string> oldPlaylist = new ();
        public static int currentMusic { get; set; }
        // public static bool isM3u;
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
        public static string currentPlaylist = "";
        public static bool isInitialized = false;
        public static string version = "2.14.6.6";
        public static string? AppDirMount = Environment.GetEnvironmentVariable("APPDIR");
        public static float MusicTimePercentage = 0;

        public class Song
        {
            public string? Path { get; set; }
            public string? Title { get; set; }
            public string? Author { get; set; }
            public string? Album { get; set; }
            public string? Year { get; set; }
            public string? Genre { get; set; }
        }
    }

    // Class to hold Util related Functions
    public static class UtilFuncs {
        // Return user preferred path for JammerPath
        public static string GetJammerPath() {
            string defaultJammerFolderName = "jammer";
            // use xdg_config_home if it is set
            if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("XDG_CONFIG_HOME"))) {
                return Path.Combine(Environment.GetEnvironmentVariable("XDG_CONFIG_HOME"), defaultJammerFolderName);
            }

            // use JAMMER_CONFIG_PATH if it is set
            if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAMMER_CONFIG_PATH"))) {
                return Environment.GetEnvironmentVariable("JAMMER_CONFIG_PATH");
            }

            // use the default user profile path
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), defaultJammerFolderName);
        }

        // INFO ABOUT JAMMER SONG STRING
        // ½ is a separator between song path and song title and more...
        // Example:
        // /home/user/Music/song.mp3½Song Title
        // NEW SYSTEM
        // still the same but the details can be dynamic
        // Example:
        // /home/user/Music/song.mp3###@@@###{ "title": "Song Title", "author": "Author Name", "album": "Album Name", "year": "2021", "genre": "Rock" }

        /// <summary>
        /// Extracts song details from a given string.
        /// </summary>
        /// <param name="song">The song string, which may contain metadata separated by "###@@@###".</param>
        /// <returns>A <see cref="Song"/> object containing the song details.</returns>
        public static Utils.Song GetSongDetails(string song) {
            Utils.Song songDetails = new();
            if (song.Contains("###@@@###"))
            {
                string[] songParts = song.Split("###@@@###");
                songDetails.Path = songParts[0];
                string json = songParts[1];
                if (!string.IsNullOrEmpty(json))
                {
                    songDetails = JsonSerializer.Deserialize<Utils.Song>(json) ?? new Utils.Song();
                }
                else
                {
                    songDetails = new Utils.Song();
                }
            }
            else
            {
                songDetails.Path = song;
            }
            // compund assignment operator
            // if null, assign new Song()
            songDetails ??= new Utils.Song();
            return songDetails;
        }
        
        /// <summary>
        /// Combines the path and serialized representation of a song into a single string.
        /// </summary>
        /// <param name="song">The song to combine.</param>
        /// <returns>The combined string.</returns>
        /// <remarks>
        /// The combined string is in the format of "path###@@@###{json}".
        /// </remarks>
        public static string CombineToSongString(Utils.Song song) {
            string songString = song.Path + "###@@@###";
            songString += JsonSerializer.Serialize(song);
            return songString;
        }

        /// <summary>
        /// Get the title of the song
        /// </summary>
        /// <param name="title">title</param>
        /// <param name="getOrNot">get | not | getMeta</param>
        /// <returns></returns>
        public static string Title(string song)
        {
            Utils.Song songDetails = GetSongDetails(song);
            string title_new = songDetails?.Title ?? "";
            if (title_new != "")
            {
                return title_new;
            }
        
            TagLib.File? tagFile;
            try
            {
                tagFile = TagLib.File.Create(song);
                title_new = tagFile.Tag.Title;
        
                if (title_new == null || title_new == "")
                    title_new = Path.GetFileName(song);
        
                if (title_new != null)
                    return title_new;
            }
            catch (Exception)
            {
                tagFile = null;
            }
            return songDetails.Path;
        }

        public static string Author(string song) {
            Utils.Song songDetails = GetSongDetails(song);
            string author = songDetails?.Author ?? "";
            if (author != "")
            {
                return author;
            }
        
            TagLib.File? tagFile;
            try
            {
                tagFile = TagLib.File.Create(song);
                author = tagFile.Tag.FirstPerformer;
        
                if (author == null || author == "")
                    author = "Unknown";
        
                if (author != null)
                    return author;
            }
            catch (Exception)
            {
                tagFile = null;
            }
            return songDetails.Path;
        }
    }
}
