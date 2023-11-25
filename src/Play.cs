using Raylib_cs;
using System.Threading;

namespace jammer
{
    public class Play
    {
        public static void PlaySong(string[] songs, int Currentindex)
        {
            Console.WriteLine("Play song");

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

            Console.WriteLine("Path: " + path);

            Utils.currentSong = path;
            Utils.currentSongIndex = Currentindex;

            // Init audio
            if ( !Raylib.IsAudioDeviceReady()) {
                Raylib.InitAudioDevice();
                Raylib.SetMasterVolume(0.5f);
            }

            LoadMusic(Utils.currentSong);
        }

        public static void LoadMusic(string path)
        {
            Utils.currentMusic = Raylib.LoadMusicStream(path);
            Utils.currentMusicLength = Math.Round(Raylib.GetMusicTimeLength(Utils.currentMusic));
        }

        public static void PauseSong()
        {
            Raylib.PauseMusicStream(Utils.currentMusic);
        }

        public static void ResumeSong()
        {
            Raylib.ResumeMusicStream(Utils.currentMusic);
        }

        public static void PlaySong()
        {
            Raylib.PlayMusicStream(Utils.currentMusic);
        }

        public static void StopSong()
        {
            Raylib.StopMusicStream(Utils.currentMusic);
        }

        public static void ResetMusic() {
            Raylib.StopMusicStream(Utils.currentMusic);
            Raylib.UnloadMusicStream(Utils.currentMusic);
            // Raylib.CloseAudioDevice();
        }
        public static void NextSong()
        {
            Utils.currentSongIndex = (Utils.currentSongIndex + 1) % Utils.songs.Length;
            // Utils.mainLoop = false;
            // Start.state = MainStates.play;
            // Start.drawOnce = true;
        }

        public static void PrevSong()
        {
            Utils.currentSongIndex = (Utils.currentSongIndex - 1) % Utils.songs.Length;
            if (Utils.currentSongIndex < 0)
            {
                Utils.currentSongIndex = Utils.songs.Length - 1;
            }
            Utils.mainLoop = false;
            Start.state = MainStates.play;
            Start.drawOnce = true;
        }  

    }
}