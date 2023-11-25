using Raylib_cs;

namespace jammer
{
    public class Play
    {
        public static void PlaySong(string[] songs, int Currentindex)
        {
            var path = "";
            // check if file is a local
            if (File.Exists(songs[Currentindex]))
            {
                // id related to local file path, convert to absolute path
                path = Path.GetFullPath(songs[Currentindex]);
            }
            else if (URL.IsValidSoundcloudSong(songs[Currentindex]))
            {
                // id related to url, download and convert to absolute path
                path = Download.DownloadSong(songs[Currentindex]);
            }
            else if (URL.IsValidYoutubeSong(songs[Currentindex]))
            {
                // id related to url, download and convert to absolute path
                path = Download.DownloadSong(songs[Currentindex]);
            }
            else
            {
                Console.WriteLine("Invalid url");
                return;
            }
            Utils.currentSong = path;
            Utils.currentSongIndex = Currentindex;

            // Init audio
            Raylib.InitAudioDevice();
            Raylib.SetMasterVolume(0.5f);

            LoadMusic(Utils.currentSong);
        }

        static void LoadMusic(string path) {
            Utils.currentMusic = Raylib.LoadMusicStream(path);
        }

        public static void PauseSong()
        {
            Raylib.PauseMusicStream(Utils.currentMusic);
        }

        public static void ResumeSong()
        {
            Raylib.ResumeMusicStream(Utils.currentMusic);
        }

        public static void PlaySong() {
            Console.WriteLine("Playing music: " + Utils.currentSong);
            Raylib.PlayMusicStream(Utils.currentMusic);
        }
    }
}