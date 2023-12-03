using NAudio.Wave;
using NAudio.Vorbis;
using System.Diagnostics;
using NAudio.Wave.SampleProviders;
using Spectre.Console;

namespace jammer
{
    public class Play
    {
        private static Thread loopThread = new Thread(() => { });
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
            try
            {
                string extension = Path.GetExtension(path);

                if (extension == ".mp3" || extension == ".wav" || extension == ".flac")
                {
                    PlayMediaFoundation();
                }
                else if (extension == ".ogg")
                {
                    PlayOgg();
                }
                else
                {
                    Console.WriteLine("Unsupported file format");
                    Debug.dprint("Unsupported file format");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
                Debug.dprint("Error: " + e);
                return;
            }
            // LoadMusic(Utils.currentSong);
        }

        public static void PauseSong()
        {
            Utils.currentMusic.Pause();
        }

        public static void ResumeSong()
        {
            Utils.currentMusic.Play();
        }

        public static void PlaySong()
        {
            // Utils.currentMusic.Stop();
            Utils.currentMusic.Play();
        }

        public static void StopSong()
        {
            Utils.currentMusic.Stop();
        }

        public static void ResetMusic() {
            Utils.currentMusic.Stop();
            Utils.currentMusic.Dispose();
        }
        public static void NextSong()
        {
            ResetMusic();
            Utils.currentSongIndex = (Utils.currentSongIndex + 1) % Utils.songs.Length;
            playDrawReset();
            PlaySong(Utils.songs, Utils.currentSongIndex);
        }

        private static void playDrawReset() // play, draw, reset lastSeconds
        {
            Start.state = MainStates.playing;
            Start.drawOnce = true;
            Start.lastSeconds = 0;
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
            playDrawReset();
            PlaySong(Utils.songs, Utils.currentSongIndex);
        }
        public static void PrevSong()
        {
            ResetMusic();
            Utils.currentSongIndex = (Utils.currentSongIndex - 1) % Utils.songs.Length;
            if (Utils.currentSongIndex < 0)
            {
                Utils.currentSongIndex = Utils.songs.Length - 1;
            }
            playDrawReset();
            PlaySong(Utils.songs, Utils.currentSongIndex);
        }

        public static void SeekSong(float seconds, bool relative)
        {
            if (Utils.audioStream == null)
            {
                return;
            }

            // Calculate the seek position based on the requested seconds
            long seekPosition = (long)(Utils.audioStream.WaveFormat.AverageBytesPerSecond * Math.Abs(seconds));

            // If seeking relative to the current position, adjust the seek position
            if (relative)
            {
                // if negative, move backwards
                if (seconds < 0)
                {
                    seekPosition = Utils.audioStream.Position - seekPosition;
                }
                else
                {
                    seekPosition = Utils.audioStream.Position + seekPosition;
                }
                // Clamp again to ensure it's within the valid range
                seekPosition = Math.Max(0, Math.Min(seekPosition, Utils.audioStream.Length));
                Start.lastSeconds = 0;
                Start.drawOnce = true;
            }
            else
            {
                // Clamp the seek position to be within the valid range [0, Utils.audioStream.Length]
                seekPosition = Math.Max(0, Math.Min(seekPosition, Utils.audioStream.Length));
            }

            // Update the audio stream's position
            Utils.audioStream.Position = seekPosition;
        }

        public static void ModifyVolume(float volume)
        {
            // Preferences.volume += volume;
            // if (Preferences.volume > 5)
            // {
            //     Preferences.volume = 5;
            // }
            // else if (Preferences.volume < 0)
            // {
            //     Preferences.volume = 0;
            // }

            // Raylib.SetMusicVolume(Utils.currentMusic, Preferences.volume);
        }

        public static void MuteSong()
        {
            // if (Preferences.isMuted)
            // {
            //     Preferences.isMuted = false;
            //     Preferences.volume = Preferences.oldVolume;
            //     Raylib.SetMusicVolume(Utils.currentMusic, Preferences.volume);
            // }
            // else
            // {
            //     Preferences.isMuted = true;
            //     Preferences.oldVolume = Preferences.volume;
            //     Preferences.volume = 0;
            //     Raylib.SetMusicVolume(Utils.currentMusic, Preferences.volume);
            // }
        }

        public static void MaybeNextSong()
        {
            if (Utils.songs.Length == 1)
            {
                SeekSong(0, false);
                return;
            }
            if (Preferences.isLoop)
            {
                SeekSong(0, false);
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
            Start.state = MainStates.playing;
        }

        public static void Suffle()
        {
            // suffle songs
            Random rnd = new Random();
            Utils.songs = Utils.songs.OrderBy(x => rnd.Next()).ToArray();
        }

        static public void PlayMediaFoundation()
        {
            using var reader = new MediaFoundationReader(Utils.currentSong);
            using var outputDevice = new WaveOutEvent();
            outputDevice.Init(reader);
            Utils.currentMusic = outputDevice;
            Utils.audioStream = reader;
            Utils.currentMusic.Play();

            // start loop thread
            loopThread = new Thread(Start.Loop);
            loopThread.Start();

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            manualEvent.WaitOne();
        }

        static public void PlayOgg()
        {
            using var reader = new VorbisWaveReader(Utils.currentSong);
            using var outputDevice = new WaveOutEvent();
            outputDevice.Init(reader);
            Utils.currentMusic = outputDevice;
            Utils.audioStream = reader;
            Utils.currentMusic.Play();

            // start loop thread
            loopThread = new Thread(Start.Loop);
            loopThread.Start();

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            manualEvent.WaitOne();
        }
    }
}