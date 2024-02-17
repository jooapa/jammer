using NAudio.Wave;
using NAudio.Vorbis;
using Spectre.Console;

namespace jammer
{
    public class Play
    {
        private static Thread loopThread = new Thread(() => { });

        // playsong function will play the song at the index of the array and get the path of the song
        public static void PlaySong(string[] songs, int Currentindex)
        {
            if (songs.Length == 0)
            {
                AnsiConsole.MarkupLine("[red]No songs in playlist[/]");
                Currentindex = 0;
                Start.Run(new string[] {});
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
            // iof folder
            else if (Directory.Exists(songs[Currentindex]))
            {
                int originalLengthMinusFolder = Utils.songs.Length - 1;
                // add all files in folder to Utils.songs
                string[] files = Directory.GetFiles(songs[Currentindex]);
                foreach (string file in files)
                {
                    AddSong(file);
                }

                // remove folder from Utils.songs
                Play.DeleteSong(Currentindex);
                
                AnsiConsole.MarkupLine("[bold]" + Currentindex + "[/] : " + Utils.songs.Length + " : " + Utils.currentSongIndex + " : " + originalLengthMinusFolder);

                if (Utils.songs.Length == originalLengthMinusFolder) {
                    path = Utils.songs[originalLengthMinusFolder - 1];
                }
                else {
                    path = Utils.songs[Currentindex];
                }
                
                
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
                AnsiConsole.MarkupLine("[red] Song not found[/]");
                return;
            }

            Start.lastSeconds = -1;
            Utils.currentSong = path;
            Utils.currentSongIndex = Currentindex;
            Playlists.AutoSave();
            // Init audio
            try
            {
                Debug.dprint("Init audio");
                string extension = Path.GetExtension(path).ToLower();

                if (extension == ".mp3" || extension == ".wav" || extension == ".flac" || extension == ".aac" || extension == ".wma" || extension == ".mp4")
                {
                    Debug.dprint("Audiofile");
                    PlayMediaFoundation();
                }
                else if (extension == ".ogg")
                {
                    Debug.dprint("Oggfile");
                    PlayOgg();
                }
                else if (extension == ".jammer") {
                    Debug.dprint("Jammer");
                    // read playlist
                    string[] playlist = File.ReadAllLines(path);
                    // add all songs in playlist to Utils.songs
                    foreach (string song in playlist) {
                        AddSong(song);
                    }
                    // remove playlist from Utils.songs
                    Utils.songs = Utils.songs.Where((source, i) => i != Currentindex).ToArray();
                    if (Currentindex == Utils.songs.Length)
                    {
                        Utils.currentSongIndex = Utils.songs.Length - 1;
                    }
                    Start.state = MainStates.playing;
                    PlaySong(Utils.songs, Utils.currentSongIndex);
                }
                else
                {
                    Console.WriteLine("Unsupported file format");
                    Debug.dprint("Unsupported file format");
                    // remove song from current Utils.songs
                    Utils.songs = Utils.songs.Where((source, i) => i != Currentindex).ToArray();
                    if (Currentindex == Utils.songs.Length)
                    {
                        Utils.currentSongIndex = Utils.songs.Length - 1;
                    }
                    Start.state = MainStates.playing;
                    PlaySong(Utils.songs, Utils.currentSongIndex);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
                Debug.dprint("Error: " + e);
                return;
            }
            Debug.dprint("End of PlaySong");
        }

        public static void PauseSong()
        {
            Utils.currentMusic.Pause();
        }

        public static void ResumeSong()
        {
            if (Utils.songs.Length == 0)
            {
                return;
            }
            Utils.currentMusic.Play();
        }

        public static void PlaySong()
        {
            if (Utils.songs.Length == 0)
            {
                return;
            }
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
            if (Utils.audioStream.Length == seekPosition)
            {
                MaybeNextSong();
                return;
            }
            try {
                Utils.audioStream.Position = seekPosition;
            }
            catch (Exception e) {
                AnsiConsole.WriteException(e);
            }
        }

        public static void ModifyVolume(float volume)
        {
            Preferences.volume += volume;
            if (Preferences.volume > 1)
            {
                Preferences.volume = 1;
            }
            else if (Preferences.volume < 0)
            {
                Preferences.volume = 0;
            }

            Utils.currentMusic.Volume = Preferences.volume;

        }

        public static void MuteSong()
        {
            if (Preferences.isMuted)
            {
                Preferences.isMuted = false;
                Utils.currentMusic.Volume = Preferences.oldVolume;
                Preferences.volume = Preferences.oldVolume;
            }
            else
            {
                Preferences.isMuted = true;
                Preferences.oldVolume = Preferences.volume;
                Utils.currentMusic.Volume = 0;
                Preferences.volume = 0;
            }
        }

        public static void MaybeNextSong()
        {
            if (Preferences.isLoop && Utils.audioStream != null)
            {
                Utils.audioStream.Position = 0;
                Start.state = MainStates.playing;
            }
            else if (Utils.songs.Length == 1 && !Preferences.isLoop && Utils.audioStream != null){
                Utils.audioStream.Position = Utils.audioStream.Length;
                Start.state = MainStates.pause;
                Utils.currentMusic.Pause();
                Start.lastSeconds = 0;
                Start.drawOnce = true;
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
            if (Utils.songs.Length == 0)
            {
                return;
            }
            // check if index is in range
            if (index < 0 || index > Utils.songs.Length)
            {
                AnsiConsole.MarkupLine("[red]Index out of range[/]");
                return;
            }
            // remove song from current Utils.songs
            Utils.songs = Utils.songs.Where((source, i) => i != index).ToArray();
            ResetMusic();
            if (index == Utils.songs.Length)
            {
                if (Utils.songs.Length == 0) {
                    Utils.songs = new string[] { "" };
                }
                else {
                    Utils.currentSongIndex = Utils.songs.Length - 1;
                }
            }
            Start.state = MainStates.playing;
            
            Playlists.AutoSave();
            // if no songs left, add "" to Utils.songs
            PlaySong(Utils.songs, Utils.currentSongIndex);
        }

        public static void Suffle()
        {
            // suffle songs
            Random rnd = new Random();
            Utils.songs = Utils.songs.OrderBy(x => rnd.Next()).ToArray();
        }

        static public void PlayMediaFoundation()
        {
            Debug.dprint("PlayMediaFoundation");

            using var reader = new MediaFoundationReader(Utils.currentSong);
            // if file extension is mp4
            if (Path.GetExtension(Utils.currentSong) == ".mp4")
            {
                string mp4Path = Path.Combine(
                    Utils.jammerPath,
                    Path.GetFileNameWithoutExtension(Utils.currentSong) + ".aac"
                );
                AnsiConsole.MarkupLine("[green]Converting mp4 to aac[/]");
                MediaFoundationEncoder.EncodeToAac(reader, mp4Path);
                // position reader to start of file
                reader.Position = 0;
            }
            using var outputDevice = new WaveOutEvent();
            outputDevice.Init(reader);
            Utils.currentMusic = outputDevice;
            Utils.audioStream = reader;
            Utils.currentMusic.Play();

            // start loop thread
            StartLoopThread();

            //NOTE(ra) When music is playing code execution stops here
            ManualResetEvent manualEvent = new ManualResetEvent(false);
            manualEvent.WaitOne();

            Debug.dprint("End of PlayMediaFoundation");
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
            StartLoopThread();

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            manualEvent.WaitOne();
        }
        static void StartLoopThread()
        {
            // start loop thread
            loopThread = new Thread(Start.Loop);
            loopThread.Start();
        }
    }

}
