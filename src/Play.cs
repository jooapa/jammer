using ManagedBass;
using Spectre.Console;
using System.IO;


namespace jammer
{
    public class Play
    {
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

                if (extension == ".mp3" || extension == ".ogg" || extension == ".wav" || extension == ".mp2" || extension == ".mp1" ||
                    extension == ".aiff" || extension == ".m2a" || extension == ".mpa" || extension == ".m1a" || extension == ".mpg" ||
                    extension == ".mpeg" || extension == ".aif" || extension == ".mp3pro" || extension == ".bwf" || extension == ".mus" ||
                    extension == ".mod" || extension == ".mo3" || extension == ".s3m" || extension == ".xm" || extension == ".it" ||
                   extension == ".mtm" || extension == ".umx" || extension == ".mdz" || extension == ".s3z" || extension == ".itz" ||
                    extension == ".xmz")
                {
                    Debug.dprint("Audiofile");
                    StartPlaying();
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
            Bass.ChannelPause(Utils.currentMusic);
        }

        public static void ResumeSong()
        {
            if (Utils.songs.Length == 0)
            {
                return;
            }
            Bass.ChannelPause(Utils.currentMusic);
        }

        public static void PlaySong()
        {
            if (Utils.songs.Length == 0)
            {
                return;
            }
            Bass.ChannelPause(Utils.currentMusic);
        }

        public static void StopSong()
        {
            Bass.ChannelPause(Utils.currentMusic);
        }

        public static void ResetMusic()
        {
            if (Utils.currentMusic != 0)
            {
                Bass.StreamFree(Utils.currentMusic);    
            }
        }
        public static void NextSong()
        {
            Utils.currentSongIndex = (Utils.currentSongIndex + 1) % Utils.songs.Length;
            PlayDrawReset();
            PlaySong(Utils.songs, Utils.currentSongIndex);
        }

        private static void PlayDrawReset() // play, draw, reset lastSeconds
        {
            Start.state = MainStates.playing;
            Start.drawOnce = true;
            Start.lastSeconds = 0;
        }

        public static void RandomSong()
        {
            int lastSongIndex = Utils.currentSongIndex;
            Random rnd = new Random();
            Utils.currentSongIndex = rnd.Next(0, Utils.songs.Length);
            // make sure we dont play the same song twice in a row
            while (Utils.currentSongIndex == lastSongIndex)
            {
                Utils.currentSongIndex = rnd.Next(0, Utils.songs.Length);
            }
            PlayDrawReset();
            PlaySong(Utils.songs, Utils.currentSongIndex);
        }
        public static void PrevSong()
        {
            Utils.currentSongIndex = (Utils.currentSongIndex - 1) % Utils.songs.Length;
            if (Utils.currentSongIndex < 0)
            {
                Utils.currentSongIndex = Utils.songs.Length - 1;
            }
            PlayDrawReset();
            PlaySong(Utils.songs, Utils.currentSongIndex);
        }
        public static void SeekSong(float seconds, bool relative)
        {
            // Calculate the seek position based on the requested seconds
            var pos = Bass.ChannelGetPosition(Utils.currentMusic);

            // If seeking relative to the current position, adjust the seek position
            if (relative)
            {
                // if negative, move backwards
                if (seconds < 0)
                {
                    pos = pos - Bass.ChannelSeconds2Bytes(Utils.currentMusic, 5);
                }
                else
                {
                    pos = pos + Bass.ChannelSeconds2Bytes(Utils.currentMusic, 5);
                }
                // Clamp again to ensure it's within the valid range
                //pos = Math.Max(0, Math.Min(pos, Utils.audioStream.Length));
                Start.drawOnce = true;
            }
            else
            {
                // Clamp the seek position to be within the valid range [0, Utils.audioStream.Length]
                pos = Math.Max(0, Math.Min(pos, pos));
            }

            // Update the audio stream's position
            //if (Utils.audioStream.Length == pos)
            if (Bass.ChannelGetPosition(Utils.currentMusic) >= Bass.ChannelGetLength(Utils.currentMusic))
            {
                MaybeNextSong();
            }
            else if (pos < 0)
            {
                Bass.ChannelSetPosition(Utils.currentMusic, 0);
            }
            else
            {
                Bass.ChannelSetPosition(Utils.currentMusic, pos);
            }
            PlayDrawReset();
            return;
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

            Bass.ChannelSetAttribute(Utils.currentMusic, ChannelAttribute.Volume, Preferences.volume);

        }

        public static void ToggleMute()
        {
            if (Preferences.isMuted)
            {
                Preferences.isMuted = false;
                //Utils.currentMusic.Volume = Preferences.oldVolume;
                Bass.ChannelSetAttribute(Utils.currentMusic, ChannelAttribute.Volume, Preferences.oldVolume);
                Preferences.volume = Preferences.oldVolume;
            }
            else
            {
                Preferences.isMuted = true;
                Preferences.oldVolume = Preferences.volume;
                Bass.ChannelSetAttribute(Utils.currentMusic, ChannelAttribute.Volume, 0);
                Preferences.volume = 0;
            }
        }

        public static void MaybeNextSong()
        {
            Bass.ChannelSetPosition(Utils.currentMusic, 0);
            Start.drawOnce = true;

            if (Preferences.isLoop)
            {
                Start.state = MainStates.playing;
            }
            else if (Utils.songs.Length == 1 && !Preferences.isLoop){
                //Utils.audioStream.Position = Utils.audioStream.Length;
                Bass.ChannelSetPosition(Utils.currentMusic, Bass.ChannelGetLength(Utils.currentMusic));
                Start.state = MainStates.pause;
                //Utils.currentMusic.Pause();
                Bass.ChannelPause(Utils.currentMusic);
                Start.lastSeconds = 0;
                
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
            // PREV RESET
            if (index == Utils.songs.Length)
            {
                if (Utils.songs.Length == 0) {
                    Utils.songs = new string[] { "" };
                    ResetMusic();
                    Start.state = MainStates.pause;
                }
                else {
                    Utils.currentSongIndex = Utils.songs.Length - 1;
                    Start.state = MainStates.playing;
                }
            }
            
            Playlists.AutoSave();
            // if no songs left, add "" to Utils.songs
            PlaySong(Utils.songs, Utils.currentSongIndex);
        }

        public static void Shuffle()
        {
            // suffle songs
            Random rnd = new Random();
            Utils.songs = Utils.songs.OrderBy(x => rnd.Next()).ToArray();
        }

        static public void StartPlaying()
        {

            ResetMusic();

            // create stream
            Utils.currentMusic = Bass.CreateStream(Utils.currentSong, 0, 0, BassFlags.Default);
            if (Utils.currentMusic == 0)
            {
                Message.Data("Deleting song from playlist", "Error: Can't play song");

                DeleteSong(Utils.currentSongIndex);
                
            }

            // set volume
            Bass.ChannelSetAttribute(Utils.currentMusic, ChannelAttribute.Volume, Preferences.volume);

            // play stream
            PlayDrawReset();
            Bass.ChannelPlay(Utils.currentMusic);
            TUI.RehreshCurrentView();
        }
    }
}
