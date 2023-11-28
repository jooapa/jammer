using Raylib_cs;

namespace jammer
{
    public class Play
    {
        public static void PlaySong(string[] songs, int Currentindex)
        {
            if (Currentindex < 0 || Currentindex >= songs.Length)
            {
                Console.WriteLine("Index out of range");
                return;
            }
            Debug.dprint("Play song");

            var path = "";
            // check if file is a local
            if (File.Exists(songs[Currentindex]))
            {
                // id related to local file path, convert to absolute path
                path = Path.GetFullPath(songs[Currentindex]);
            }
            else if (URL.isValidSoundCloudPlaylist(songs[Currentindex])) {
                // id related to url, download and convert to absolute path
                Debug.dprint("Soundcloud playlist.");
                path = Download.GetSongsFromPlaylist(songs[Currentindex]);
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
                Debug.dprint("Invalid url");
                return;
            }

            Utils.currentSong = path;
            Utils.currentSongIndex = Currentindex;

            // Init audio
            if (!Raylib.IsAudioDeviceReady()) {
                Raylib.InitAudioDevice();
            }

            Raylib.SetMasterVolume(0.5f); // set how loud the music is
            LoadMusic(Utils.currentSong);
        }

        public static void LoadMusic(string path)
        {
            Utils.currentMusic = Raylib.LoadMusicStream(path);
            Utils.currentMusicLength = Math.Round(Raylib.GetMusicTimeLength(Utils.currentMusic));
            Raylib.SetMusicVolume(Utils.currentMusic, Preferences.volume);
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
        }
        public static void NextSong()
        {
            ResetMusic();
            Utils.currentSongIndex = (Utils.currentSongIndex + 1) % Utils.songs.Length;
            PlaySong(Utils.songs, Utils.currentSongIndex);
            Start.state = MainStates.play;
        }

        public static void RandomSong()
        {
            ResetMusic();
            int lastSongIndex = Utils.currentSongIndex;
            Random rnd = new Random();
            Utils.currentSongIndex = rnd.Next(0, Utils.songs.Length);
            // make sure we dont play the same song twice in a row
            while (Utils.currentSongIndex == lastSongIndex)
            {
                Utils.currentSongIndex = rnd.Next(0, Utils.songs.Length);
            }
            PlaySong(Utils.songs, Utils.currentSongIndex);
            Start.state = MainStates.play;
        }
        public static void PrevSong()
        {
            ResetMusic();
            Utils.currentSongIndex = (Utils.currentSongIndex - 1) % Utils.songs.Length;
            if (Utils.currentSongIndex < 0)
            {
                Utils.currentSongIndex = Utils.songs.Length - 1;
            }
            PlaySong(Utils.songs, Utils.currentSongIndex);
            Start.state = MainStates.play;
        }

        public static void SeekSong(float seconds)
        {
            // if musictimeplayed under 0
            if (Utils.preciseTime + seconds <= 0)
            {
                Raylib.SeekMusicStream(Utils.currentMusic, 0.1f); // goto to start if under 0
            }
            else if (Utils.preciseTime + seconds >= Utils.currentMusicLength) // if musictimeplayed over song length
            {
                MaybeNextSong();
            }
            else {
                Raylib.SeekMusicStream(Utils.currentMusic, (float)(Utils.MusicTimePlayed + seconds));
            }
        }

        public static void ModifyVolume(float volume)
        {
            Preferences.volume += volume;
            if (Preferences.volume > 5)
            {
                Preferences.volume = 5;
            }
            else if (Preferences.volume < 0)
            {
                Preferences.volume = 0;
            }

            Raylib.SetMusicVolume(Utils.currentMusic, Preferences.volume);
        }

        public static void MuteSong()
        {
            if (Preferences.isMuted)
            {
                Preferences.isMuted = false;
                Preferences.volume = Preferences.oldVolume;
                Raylib.SetMusicVolume(Utils.currentMusic, Preferences.volume);
            }
            else
            {
                Preferences.isMuted = true;
                Preferences.oldVolume = Preferences.volume;
                Preferences.volume = 0;
                Raylib.SetMusicVolume(Utils.currentMusic, Preferences.volume);
            }
        }

        public static void MaybeNextSong()
        {
            if (Utils.songs.Length == 1)
            {
                Raylib.SeekMusicStream(Utils.currentMusic, 0);
                return;
            }
            if (Preferences.isLoop)
            {
                Raylib.SeekMusicStream(Utils.currentMusic, 0); // goto to start if under 0
            }
            else if (Preferences.isShuffle)
            {
                RandomSong();
            }
            else
            {
                NextSong();
            }
        }

        public static void AddSong(string song)
        {
            // check if song is already in playlist
            foreach (string s in Utils.songs)
            {
                if (s == song)
                {
                    Console.WriteLine("Song already in playlist");
                    return;
                }
            }
            // add song to current Utils.songs
            Array.Resize(ref Utils.songs, Utils.songs.Length + 1);
            Utils.songs[Utils.songs.Length - 1] = song;
        }
        public static void DeleteSong(int index)
        {
            // check if index is in range
            if (index < 0 || index >= Utils.songs.Length)
            {
                Console.WriteLine("Index out of range");
                return;
            }
            // remove song from current Utils.songs
            Utils.songs = Utils.songs.Where((source, i) => i != index).ToArray();
            ResetMusic();
            PlaySong(Utils.songs, Utils.currentSongIndex);
            Start.state = MainStates.play;
        }

        public static void Suffle()
        {
            // suffle songs
            Random rnd = new Random();
            Utils.songs = Utils.songs.OrderBy(x => rnd.Next()).ToArray();
        }
    }
}