using ManagedBass;
#if !NO_BASS_AAC
using ManagedBass.Aac;
#endif
using ManagedBass.DirectX8;
using ManagedBass.Midi;
using ManagedBass.Opus;
using ManagedBass.Fx;
using Spectre.Console;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TagLib;


namespace Jammer
{
    public class Play
    {
        public static string[] songExtensions = {   ".mp3", ".ogg", ".wav", ".mp2", ".mp1", ".aiff", ".m2a",
                                                    ".mpa", ".m1a", ".mpg", ".mpeg", ".aif", ".mp3pro", ".bwf",
                                                    ".mus", ".mod", ".mo3", ".s3m", ".xm", ".it", ".mtm", ".umx",
                                                    ".mdz", ".s3z", ".itz", ".xmz"};
        public static string[] aacExtensions = { ".aac", ".m4a", ".adts", ".m4b" };
        public static string[] mp4Extensions = { ".mp4" };
        public static string[] midiExtensions = { ".mid", ".midi", ".rmi", ".kar" };
        public static int songsThatWereNotFound = 0;
        public static bool showNoSongsFoundMessage = true;

        // RSS auto-skip timer tracking
        private static DateTime? rssStartTime = null;

        public static bool isValidExtension(string checkingExt, string[] exts)
        {
            // if checkingExt is in exts
            foreach (string ext in exts)
            {
                if (checkingExt == ext)
                {
                    return true;
                }
            }
            return false;
        }

        // playsong function will play the song at the index of the array and get the path of the song
        public static void PlaySong(string[] songs, int Currentindex)
        {
            // delete all empty songs
            if (songs[Currentindex] == "")
            {
                Utils.Songs = Utils.UtilFuncs.RemoveAt(Utils.Songs, Currentindex);
                if (Utils.Songs.Length == 0)
                {
                    Start.state = MainStates.pause;
                    return;
                }
                if (Currentindex == Utils.Songs.Length)
                {
                    Currentindex--;
                }
                PlaySong(Utils.Songs, Currentindex);
                return;
            }

            if (songs.Length == 0)
            {
                AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.NoSongsInPlaylist}[/]");
                Currentindex = 0;
                Start.Run(new string[] { });
                return;
            }

            while (Currentindex > songs.Length)
            {
                Currentindex--;
            }
            Debug.dprint("Play song");

            Utils.CurrentSongIndex = Currentindex;
            Utils.CurrentPlaylistSongIndex = Currentindex;

            // Reset RSS timer for new song
            rssStartTime = null;

            // get song details
            // Utils.Song song = UtilFuncs.GetSongDetails(songs[Utils.currentSongIndex]);

            // if the songs[Utils.CurrentSongIndex] has the JammerFile Delimeter in the order it should be removed

            Song song = new Song()
            {
                URI = songs[Utils.CurrentSongIndex]
            };

            string fullPathToFile = "";

            song.ExtractSongDetails();

            Start.ClearKeyboardBuffer();

            // if file extension is .jammer-fav or .jammer:favorites
            if (song.URI != null && (song.URI.EndsWith(".jammer:fav") || song.URI.EndsWith(".jammer:favorites")))
            {
                song.Stream = JammerPlaylistStream.Favorites;
                song.URI = song.URI.Replace(".jammer:fav", ".jammer").Replace(".jammer:favorites", ".jammer");
            }
            
            // check if file is a local
            if (System.IO.File.Exists(song.URI))
            {
                // id related to local file path, convert to absolute path
                fullPathToFile = Path.GetFullPath(song.URI);
            }
            // if folder
            else if (Directory.Exists(song.URI))
            {
                // skip if folder
                // NextSong();
                Start.state = MainStates.next;
                return;
            }
            else if (URL.isValidSoundCloudPlaylist(song.URI))
            {
                // id related to url, download and convert to absolute path
                Debug.dprint("Soundcloud playlist.");
                fullPathToFile = Download.GetSongsFromPlaylist(song.URI, "soundcloud");
            }
            else if (URL.IsValidSoundcloudSong(song.URI))
            {
                // id related to url, download and convert to absolute path
                Download.CancelActiveDownload();
                var cts = new CancellationTokenSource();
                Download.ActiveDownloadCts = cts;

                var downloadTask = Task.Run(() => Download.DownloadSongAsync(song.URI, cts.Token));

                while (!downloadTask.IsCompleted)
                {
                    Start.CheckKeyboardAsync().Wait();
                    if (Start.state == MainStates.next || Start.state == MainStates.previous || Start.state == MainStates.stop || Start.Action == "NextSong" || Start.Action == "PreviousSong" || Start.Action == "Quit")
                    {
                        Download.CancelActiveDownload();
                        return;
                    }
                    Thread.Sleep(50);
                }

                try 
                {
                    (fullPathToFile, song) = downloadTask.GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (AggregateException e) when (e.InnerException is OperationCanceledException)
                {
                    return;
                }
            }
            else if (URL.IsValidYoutubePlaylist(song.URI))
            {
                // id related to url, download and convert to absolute path
                fullPathToFile = Download.GetSongsFromPlaylist(song.URI, "youtube");
            }
            else if (URL.IsValidYoutubeSong(song.URI))
            {
                // id related to url, download and convert to absolute path
                Download.CancelActiveDownload();
                var cts = new CancellationTokenSource();
                Download.ActiveDownloadCts = cts;

                var downloadTask = Task.Run(() => Download.DownloadSongAsync(song.URI, cts.Token));

                while (!downloadTask.IsCompleted)
                {
                    Start.CheckKeyboardAsync().Wait();
                    if (Start.state == MainStates.next || Start.state == MainStates.previous || Start.state == MainStates.stop || Start.Action == "NextSong" || Start.Action == "PreviousSong" || Start.Action == "Quit")
                    {
                        Download.CancelActiveDownload();
                        return;
                    }
                    Thread.Sleep(50);
                }

                try 
                {
                    (fullPathToFile, song) = downloadTask.GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (AggregateException e) when (e.InnerException is OperationCanceledException)
                {
                    return;
                }
                // Message.Data(SongExtensions.ToSongString(song), "123");
            }
            else if (URL.IsValidRssFeed(song.URI))
            {
                if (song.Title == null || song.Author == null)
                {
                    Download.CancelActiveDownload();
                var cts = new CancellationTokenSource();
                Download.ActiveDownloadCts = cts;

                var downloadTask = Task.Run(() => Download.DownloadSongAsync(song.URI, cts.Token));

                while (!downloadTask.IsCompleted)
                {
                    Start.CheckKeyboardAsync().Wait();
                    if (Start.state == MainStates.next || Start.state == MainStates.previous || Start.state == MainStates.stop || Start.Action == "NextSong" || Start.Action == "PreviousSong" || Start.Action == "Quit")
                    {
                        Download.CancelActiveDownload();
                        return;
                    }
                    Thread.Sleep(50);
                }

                try 
                {
                    (fullPathToFile, song) = downloadTask.GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (AggregateException e) when (e.InnerException is OperationCanceledException)
                {
                    return;
                }
                }
                // Message.Data(SongExtensions.ToSongString(song), "33");

            }
            else if (URL.IsUrl(song.URI))
            {
                Download.CancelActiveDownload();
                var cts = new CancellationTokenSource();
                Download.ActiveDownloadCts = cts;

                var downloadTask = Task.Run(() => Download.DownloadSongAsync(song.URI, cts.Token));

                while (!downloadTask.IsCompleted)
                {
                    Start.CheckKeyboardAsync().Wait();
                    if (Start.state == MainStates.next || Start.state == MainStates.previous || Start.state == MainStates.stop || Start.Action == "NextSong" || Start.Action == "PreviousSong" || Start.Action == "Quit")
                    {
                        Download.CancelActiveDownload();
                        return;
                    }
                    Thread.Sleep(50);
                }

                try 
                {
                    (fullPathToFile, song) = downloadTask.GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (AggregateException e) when (e.InnerException is OperationCanceledException)
                {
                    return;
                }
                // Message.Data(SongExtensions.ToSongString(song), fullPathToFile);
                // Message.Data(path, song);
            }

            song ??= new Song()
                {
                    URI = songs[Utils.CurrentSongIndex],
                };

            // Message.Data(SongExtensions.ToSongString(song), "111");
            // {
            //     string tmpstr = SongExtensions.ToSongString(song);

            //     Message.Data(tmpstr, "22");

            //     Song tempSong = SongExtensions.ToSong(tmpstr);

            //     if (tempSong.URI != null)
            //         Message.Data(tempSong.URI, "a");
            //     if (tempSong.Title != null)
            //         Message.Data(tempSong.Title, "b");
            //     if (tempSong.Author != null)
            //         Message.Data(tempSong.Author, "c");

            //     song = tempSong;
            // }

            // if the song has the URI and no other properties check for them in the songs[Utils.CurrentSongIndex],
            if (
                song.URI != null &&
                    (
                        song.Title == null || song.Author == null || song.Album == null || song.Year == null || song.Genre == null || song.Duration == null || song.Description == null || song.PubDate == null || song.IsFavorite == null
                    )
                )
            {
                Song tempSong = SongExtensions.ToSong(songs[Utils.CurrentSongIndex]);
                song.Title ??= tempSong.Title;
                song.Author ??= tempSong.Author;
                song.Album ??= tempSong.Album;
                song.Year ??= tempSong.Year;
                song.Genre ??= tempSong.Genre;
                song.Duration ??= tempSong.Duration;
                song.Description ??= tempSong.Description;
                song.PubDate ??= tempSong.PubDate;
                song.IsFavorite ??= tempSong.IsFavorite;
            }

            // Message.Data(songs[Utils.CurrentSongIndex], "path");
            // Message.Data(SongExtensions.ToSongString(song), "11");

            // Message.Data(fullPath + " || " + song.Path, "path");
            // if the Utils.songs current is not the same as the song.Path
            // if (fullPathToFile != Utils.Songs[Utils.CurrentSongIndex])
            // {
            //     Log.Info("Song path is different from the current song path: " + fullPathToFile + " != " + Utils.Songs[Utils.CurrentSongIndex]);
            //     // song.Title = ""; // TODO might break something :/ // TODO: This might just break something else :OO
            //     song.URI = SongExtensions.ToSong(Utils.Songs[Utils.CurrentSongIndex]).URI;
            // }
            // Message.Data(fullPath + " || " + song.Path, "path");

            Start.prevMusicTimePlayed = -1;
            Start.lastSeconds = -1;
            Utils.CurrentSongPath = fullPathToFile;
            Start.drawWhole = true;

            Log.Info("Playing: " + Utils.Songs[Utils.CurrentSongIndex]);

            // Message.Data(Utils.currentSongIndex + "#" + Utils.currentPlaylistSongIndex, "s");
            // taglib get title to display

            string? title = "", author = "", album = "", year = "", genre = "";
            TagLib.File? tagFile;
            if (song.Title == null || song.Title == "" ||
                song.Author == null || song.Author == "" ||
                song.Album == null || song.Album == "" ||
                song.Year == null || song.Year == "" ||
                song.Genre == null || song.Genre == "")
            {
            // Message.Data(SongExtensions.ToSongString(song), "44");

                try
                {
                    using (tagFile = TagLib.File.Create(fullPathToFile))
                    {
                        title = tagFile.Tag.Title;
                        author = tagFile.Tag.FirstPerformer;
                        album = tagFile.Tag.Album;
                        year = tagFile.Tag.Year.ToString();
                        genre = tagFile.Tag.FirstGenre;
                    }
                }
                catch (Exception)
                {
                    tagFile = null;
                    // Log.Error("Error getting title of the song");
                }
            }

            // Message.Data(SongExtensions.ToSongString(song), "22");

            // append title to song
            if ((song.Title == null || song.Title == "") && !string.IsNullOrEmpty(title))
            {
                // Log.Info("______trying to get it from the path");
                // Message.Data(SongExtensions.ToSongString(song), "55" + title);
                song.Title = title;
            }
            if ((song.Author == null || song.Author == "") && !string.IsNullOrEmpty(author))
            {
                song.Author = author;
            }
            if ((song.Album == null || song.Album == "") && !string.IsNullOrEmpty(album))
            {
                song.Album = album;
            }
            if (song.Year == null || song.Year == "")
            {
                if (year == "0")
                {
                    year = null;
                }
                song.Year = year;
            }
            if ((song.Genre == null || song.Genre == "") && !string.IsNullOrEmpty(genre))
            {
                song.Genre = genre;
            }


            // Message.Data(SongExtensions.ToSongString(song), "11");

            // Log.Info("setting current song to: " + song.ToSongString());
            Utils.Songs[Utils.CurrentSongIndex] = song.ToSongString();
            // Concatenate all song strings
            // string allSongs = string.Join("\n", Utils.songs);
            //Message.Data(allSongs, "Current Playlist");

            Start.drawWhole = true;

            try
            {
                string extension = Path.GetExtension(fullPathToFile).ToLower();

                if (extension == ".jammer")
                {
                    HandleJammerPlaylist(song, fullPathToFile);
                }
                else if (extension == ".m3u" || extension == ".m3u8")
                {
                    Utils.Songs = M3u.ParseM3u(fullPathToFile);

                    PlaySong(Utils.Songs, Utils.CurrentSongIndex);
                }
                else
                {
                    Start.ClearKeyboardBuffer();
                    StartPlaying();
                }

            }
            catch (Exception e)
            {

                Console.WriteLine($"{Locale.OutsideItems.Error}: " + e);

                Debug.dprint("Error: " + e);
                Log.Error(e.ToString() + "##");
                return;
            }
            Debug.dprint("End of PlaySong");
        }

        public static void HandleJammerPlaylist(Song song, string path)
        {
            Debug.dprint("jammer");

            // Message.Data(song.URI, "s");
            string[] playlist = System.IO.File.ReadAllLines(path ?? "");
            if (playlist == null || playlist.Length == 0)
            {
                // Empty playlist: stop any currently playing music so it doesn't
                // keep playing in the background with no way to stop it.
                ResetMusic();
                Utils.CurrentMusic = 0;
                Utils.Songs = Array.Empty<string>();
                Utils.CurrentSongIndex = 0;
                Start.state = MainStates.idle;
                Start.drawWhole = true;
                return;
            }

            // MARK: - Detect if playlist is using the old format
            {
                string newPlaylist = "";
                bool isOldFormat = false;
                foreach (string s in playlist)
                {
                    if (s.Contains('½') && !s.Contains(Utils.JammerFileDelimeter))
                    {
                        isOldFormat = true;

                        string[] split = s.Split('½');
                        Song newSong = new Song()
                        {
                            URI = split[0],
                            Title = split[1]
                        };
                        newPlaylist += newSong.ToSongString() + "\n";
                    }
                    else
                    {
                        newPlaylist += s + "\n";
                    }
                }

                if (isOldFormat)
                {
                    string input = Message.Input(
                        string.Format(Locale.UiMessages.UpdateOldPlaylistPrompt, Locale.Miscellaneous.YesNo),
                        Locale.UiMessages.UpdateOldPlaylistTitle,
                        true
                        );

                    if (input.Equals(Locale.Miscellaneous.YesAnswer, StringComparison.OrdinalIgnoreCase))
                    {
                        string playlistBackupsPath = Utils.GetPlaylistBackupsPath(Preferences.GetPlaylistsPath());
                        if (!Directory.Exists(playlistBackupsPath))
                        {
                            Directory.CreateDirectory(playlistBackupsPath);
                        }

                        string backupPath = Path.Combine(playlistBackupsPath, Path.GetFileNameWithoutExtension(song.URI) + "_" + DateTime.Now.ToString("dd-MM_HH-mm-ss") + ".jammer");
                        System.IO.File.WriteAllText(backupPath, System.IO.File.ReadAllText(song.URI), Encoding.UTF8);

                        // Message.Data(fullPath + " " + newPlaylist, "newPlaylist");
                        System.IO.File.WriteAllText(song.URI, newPlaylist, Encoding.UTF8);
                        playlist = System.IO.File.ReadAllLines(song.URI);
                    }
                }
            }

            // add all songs in playlist to Utils.songs
            foreach (string s in playlist)
            {
                if (song.Stream == JammerPlaylistStream.Favorites)
                {
                    Song plSong = SongExtensions.ToSong(s);
                    if (plSong.IsFavorite != null && plSong.IsFavorite == "true")
                    {
                        AddSong(s, false);
                    }
                    continue;
                }
                else
                {
                    AddSong(s, false);
                }
            }
            
            // remove playlist from Utils.songs
            Utils.Songs = Utils.UtilFuncs.RemoveAt(Utils.Songs, Utils.CurrentSongIndex);
            if (Utils.CurrentSongIndex == Utils.Songs.Length)
            {
                Utils.CurrentSongIndex = Utils.Songs.Length - 1;
            }



            Start.state = MainStates.playing;
            PlaySong(Utils.Songs, Utils.CurrentSongIndex);
        }

        public static bool EmptySpaces(string v)
        {
            // if string is totally empty even with spaces
            foreach (char c in v)
            {
                if (c != ' ')
                {
                    return false;
                }
            }

            return true;
        }

        public static void PauseSong()
        {
            Bass.ChannelPause(Utils.CurrentMusic);
        }

        public static void ResumeSong()
        {
            if (Utils.Songs.Length == 0)
            {
                return;
            }
            Bass.ChannelPause(Utils.CurrentMusic);
        }

        public static void PlaySong()
        {
            if (Utils.Songs.Length == 0)
            {
                return;
            }
            Bass.ChannelPlay(Utils.CurrentMusic);
        }

        public static void StopSong()
        {
            Bass.ChannelPause(Utils.CurrentMusic);
        }

        public static void ResetMusic()
        {
            if (Utils.CurrentMusic != 0)
            {
                Bass.StreamFree(Utils.CurrentMusic);
            }
            Start.drawWhole = true;
        }
        public static void NextSong()
        {
            Start.prevMusicTimePlayed = -1;
            Start.lastSeconds = -1;
            Start.drawTime = true;


            if (Utils.Songs.Length == 0)
            {
                Start.state = MainStates.pause;
                return;
            }

            if (Utils.QueueSongs.Count > 0)
            {
                PlayDrawReset();
                PlaySong(Utils.QueueSongs.ToArray(), 0);
                PlaySong();
                int index = Array.IndexOf(Utils.Songs, Utils.QueueSongs[0]);
                Utils.PreviousSongIndex = Utils.CurrentSongIndex;
                Utils.CurrentSongIndex = index;
                Utils.QueueSongs.RemoveAt(0);
            }
            else
            {

                if (Utils.Songs.Length >= 1) // no next song if only one song or less
                {
                    Utils.CurrentSongIndex = (Utils.CurrentSongIndex + 1) % Utils.Songs.Length;
                    PlayDrawReset();
                    PlaySong(Utils.Songs, Utils.CurrentSongIndex);
                }
            }
        }

        public static void PlayDrawReset() // play, draw, reset lastSeconds
        {
            Start.prevMusicTimePlayed = -1;
            Start.drawWhole = true;
        }

        public static void RandomSong()
        {
            int lastSongIndex = Utils.CurrentSongIndex;
            Random rnd = new Random();
            Utils.CurrentSongIndex = rnd.Next(0, Utils.Songs.Length);
            // make sure we dont play the same song twice in a row
            while (Utils.CurrentSongIndex == lastSongIndex)
            {
                Utils.CurrentSongIndex = rnd.Next(0, Utils.Songs.Length);
            }
            PlayDrawReset();
            PlaySong(Utils.Songs, Utils.CurrentSongIndex);
        }
        public static void PrevSong()
        {
            Start.prevMusicTimePlayed = -1;
            Start.lastSeconds = -1;

            if (Utils.Songs.Length == 0)
            {
                Start.state = MainStates.pause;
                return;
            }

            Utils.CurrentSongIndex = (Utils.CurrentSongIndex - 1) % Utils.Songs.Length;
            if (Utils.CurrentSongIndex < 0)
            {
                Utils.CurrentSongIndex = Utils.Songs.Length - 1;
            }
            PlayDrawReset();
            PlaySong(Utils.Songs, Utils.CurrentSongIndex);
        }
        public static void SeekSong(float seconds, bool relative)
        {
            // Calculate the seek position based on the requested seconds
            var pos = Bass.ChannelGetPosition(Utils.CurrentMusic);

            // If seeking relative to the current position, adjust the seek position
            if (relative)
            {
                // if negative, move backwards
                if (seconds < 0)
                {
                    pos = pos - Bass.ChannelSeconds2Bytes(Utils.CurrentMusic, Preferences.GetRewindSeconds());
                }
                else
                {
                    pos = pos + Bass.ChannelSeconds2Bytes(Utils.CurrentMusic, Preferences.GetForwardSeconds());
                }
                // Clamp again to ensure it's within the valid range
                //pos = Math.Max(0, Math.Min(pos, Utils.audioStream.Length));
            }
            else
            {
                // Clamp the seek position to be within the valid range [0, Utils.audioStream.Length]
                pos = Bass.ChannelSeconds2Bytes(Utils.CurrentMusic, seconds);
            }

            // Update the audio stream's position
            //if (Utils.audioStream.Length == pos)
            if (pos < 0)
            {
                Bass.ChannelSetPosition(Utils.CurrentMusic, 0);
            }
            else if (pos >= Bass.ChannelGetLength(Utils.CurrentMusic))
            {
                Bass.ChannelSetPosition(Utils.CurrentMusic, Bass.ChannelGetLength(Utils.CurrentMusic) - 1);
            }
            else
            {
                Bass.ChannelSetPosition(Utils.CurrentMusic, pos);
            }

            if (Bass.ChannelGetPosition(Utils.CurrentMusic) >= Bass.ChannelGetLength(Utils.CurrentMusic))
            {
                MaybeNextSong();
            }

            Start.prevMusicTimePlayed = -1;
            // PlayDrawReset();
            return;
        }
        public static void SetVolume(float volume)
        {
            Preferences.volume = volume;
            if (Preferences.volume > 1)
            {
                Preferences.volume = 1;
            }
            else if (Preferences.volume < 0)
            {
                Preferences.volume = 0;
            }
            if (volume > 0)
            {
                Preferences.isMuted = false;
            }
            Bass.ChannelSetAttribute(Utils.CurrentMusic, ChannelAttribute.Volume, Preferences.volume);

        }
        public static void ModifyVolume(float volume)
        {
            Preferences.volume += volume;
            if (Preferences.volume > 1.5f)
            {
                Preferences.volume = 1.5f;
            }
            else if (Preferences.volume < 0)
            {
                Preferences.volume = 0;
            }

            Bass.ChannelSetAttribute(Utils.CurrentMusic, ChannelAttribute.Volume, Preferences.volume);

        }

        public static void ToggleMute()
        {
            if (Preferences.isMuted)
            {
                Preferences.isMuted = false;
                //Utils.currentMusic.Volume = Preferences.oldVolume;
                Bass.ChannelSetAttribute(Utils.CurrentMusic, ChannelAttribute.Volume, Preferences.oldVolume);
                Preferences.volume = Preferences.oldVolume;
            }
            else
            {
                Preferences.isMuted = true;
                Preferences.oldVolume = Preferences.volume;
                Bass.ChannelSetAttribute(Utils.CurrentMusic, ChannelAttribute.Volume, 0);
                Preferences.volume = 0;
            }
        }

        public static void MaybeNextSong(bool forceNoLoop = false)
        {
            LoopType loopType = Preferences.loopType;
            if (forceNoLoop)
            {
                loopType = LoopType.None;
            }

            Start.state = MainStates.play;

            switch (loopType)
            {
                case LoopType.Always:
                    Bass.ChannelSetPosition(Utils.CurrentMusic, 0);
                    Bass.ChannelPlay(Utils.CurrentMusic);
                    PlayDrawReset();
                    break;

                case LoopType.Once:
                    if (Utils.Songs.Length == 1)
                    {
                        Bass.ChannelSetPosition(Utils.CurrentMusic, Bass.ChannelGetLength(Utils.CurrentMusic));
                        Start.state = MainStates.idle;
                        Bass.ChannelPause(Utils.CurrentMusic);
                    }
                    else
                    {
                        Start.state = MainStates.idle;
                    }
                    break;

                case LoopType.None:
                    if (Utils.Songs.Length == 1)
                    {
                        Bass.ChannelSetPosition(Utils.CurrentMusic, Bass.ChannelGetLength(Utils.CurrentMusic));
                        Start.state = MainStates.pause;
                        Bass.ChannelPause(Utils.CurrentMusic);
                    }
                    else if (Preferences.isShuffle)
                    {
                        RandomSong();
                        PlayDrawReset();
                    }
                    else
                    {
                        Start.state = MainStates.next;
                    }
                    break;
            }
        }

        public static void ReDownloadSong()
        {
            if (URL.IsUrl(Utils.Songs[Utils.CurrentSongIndex]))
            {
                if (System.IO.File.Exists(Utils.CurrentSongPath))
                    System.IO.File.Delete(Utils.CurrentSongPath);

                PlaySong(Utils.Songs, Utils.CurrentSongIndex);
                SeekSong(0, false);
                return;
            }
        }

        public static void AddSong(string song, bool AddNext = true)
        {
            if (AddNext)
                Utils.Songs = Utils.Songs.Take(Utils.CurrentSongIndex + 1).Concat(new string[] { song }).Concat(Utils.Songs.Skip(Utils.CurrentSongIndex + 1)).ToArray();
            else
            {
                // add song to current Utils.songs
                Array.Resize(ref Utils.Songs, Utils.Songs.Length + 1);
                Utils.Songs[Utils.Songs.Length - 1] = song;
            }

            if (Utils.Songs.Length == 1)
            {
                Utils.CurrentSongIndex = 0;
                PlaySong(Utils.Songs, Utils.CurrentSongIndex);
            }
        }
        public static void DeleteSong(int index, bool isQueue, bool hardDelete = false, bool goForward = false)
        {
            if (Utils.Songs.Length == 0)
            {
                return;
            }
            // check if index is in range
            if (index < 0 || index > Utils.Songs.Length)
            {
                AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.IndexOoR}[/]");
                return;
            }
            // delete song from disk
            if (hardDelete)
            {
                if (System.IO.File.Exists(Utils.CurrentSongPath))
                    System.IO.File.Delete(Utils.CurrentSongPath);
            }

            // remove song from current Utils.songs
            Utils.Songs = Utils.UtilFuncs.RemoveAt(Utils.Songs, index);

            if (goForward)
            {
                // Don't decrement index when going forward
                if (Utils.CurrentSongIndex >= Utils.Songs.Length)
                {
                    Utils.CurrentSongIndex = 0;
                }
            }
            else
            {
                Utils.CurrentSongIndex--;
                if (Utils.CurrentSongIndex == -1)
                {
                    Utils.CurrentSongIndex = 0;
                }
            }

            // Handle playlist index updates
            if (index == Utils.Songs.Length)
            {
                if (Utils.Songs.Length == 0)
                {
                    Utils.Songs = new string[] { "" };
                    ResetMusic();
                    Start.state = MainStates.pause;
                }
                else if (index >= Utils.CurrentSongIndex)
                {
                    _ = Utils.CurrentSongIndex;
                }
                else
                {
                    Utils.CurrentSongIndex = Utils.Songs.Length - 1;
                }
            }

            // if no songs left, add "" to Utils.songs
            if (!isQueue)
            {
                PlaySong(Utils.Songs, Utils.CurrentSongIndex);
            }
            else if (Utils.CurrentSongIndex == index)
            {
                PlaySong(Utils.Songs, Utils.CurrentSongIndex);
            }
        }
        public static void SetEffectsToChannel()
        {

            // Start playing from the same position
            long pos = Bass.ChannelGetPosition(Utils.CurrentMusic);
            StartPlaying();
            Bass.ChannelSetPosition(Utils.CurrentMusic, pos);
        }
        public static void Shuffle()
        {
            // suffle songs
            Random rnd = new Random();
            Utils.Songs = Utils.Songs.OrderBy(x => rnd.Next()).ToArray();
        }

        static public void StartPlaying()
        {
            // MARK: StartPlaying
            ResetMusic();

            string songEntry = Utils.Songs[Utils.CurrentSongIndex];
            string songUri = songEntry;
            if (songEntry.Contains(Utils.JammerFileDelimeter))
            {
                songUri = songEntry.Split(Utils.JammerFileDelimeter)[0];
            }
            var metadata = SongMetadataStore.Get(songUri);
            bool needsFxWrapper = metadata.Speed != 1.0f || metadata.Reversed;

            // Message.Data(Utils.currentSong, "Playing: ");
            // flags
            BassFlags flags = needsFxWrapper ? BassFlags.Decode : BassFlags.Default;

#if !NO_BASS_AAC
            BassAac.PlayAudioFromMp4 = true;
            BassAac.AacSupportMp4 = true;
#endif

            int sourceStream = 0;

            ////
            sourceStream = Bass.CreateStream(Utils.CurrentSongPath, 0, 0, flags);
            //Message.Data(Utils.currentMusic.ToString(), "Current Music");
#if !NO_BASS_AAC
            if (sourceStream == 0)
                sourceStream = BassAac.CreateStream(Utils.CurrentSongPath, 0, 0, flags);
            //Message.Data(Utils.currentMusic.ToString(), "Current Music");
            if (sourceStream == 0)
                sourceStream = BassAac.CreateMp4Stream(Utils.CurrentSongPath, 0, 0, flags);
            //Message.Data(Utils.currentMusic.ToString(), "Current Music");
#endif
            if (sourceStream == 0)
                sourceStream = BassOpus.CreateStream(Utils.CurrentSongPath, 0, 0, flags);

            if (sourceStream == 0)
            {
                int newFont;
                newFont = BassMidi.FontInit(Path.Combine(Utils.SoundFontsPath, Preferences.GetCurrentSf2()), FontInitFlags.Unicode);

                if (newFont == 0)
                {
                    newFont = BassMidi.FontInit(Path.Combine(Preferences.GetCurrentSf2()), FontInitFlags.Unicode);

                    //if (newFont == 0 && Utils.curSongError)
                    //    Message.Data("Can't load the SoundFont: " + Preferences.GetCurrentSf2(), "Error loading the soundfont", true);
                }
                var sf = new MidiFont[]
                {
                    new() {
                        Handle = newFont,
                        Preset = -1, // Use all presets
                        Bank = 0 // Use default bank(s)
                    }
                };
                int m = BassMidi.StreamSetFonts(0, sf, 1);
                if (m == -1)
                {
                    throw new Exception("Can't set the SoundFont");
                }
                sourceStream = BassMidi.CreateStream(Utils.CurrentSongPath, 0, 0, flags);

                // MIDI streams do not support tempo/reverse wrappers; disable FX wrapper for MIDI.
                if (sourceStream != 0)
                {
                    needsFxWrapper = false;
                }
            }

            Utils.CurrentMusic = sourceStream;

            if (needsFxWrapper && sourceStream != 0)
            {
                try
                {
                    int wrapped = sourceStream;

                    if (metadata.Reversed)
                    {
                        wrapped = BassFx.ReverseCreate(wrapped, 2.0f, BassFlags.Default);
                        if (wrapped != 0)
                        {
                            sourceStream = wrapped;
                            Utils.CurrentMusic = wrapped;
                        }
                    }

                    if ((metadata.Speed != 1.0f || metadata.Pitch != 0.0f) && Utils.CurrentMusic != 0)
                    {
                        wrapped = BassFx.TempoCreate(Utils.CurrentMusic, BassFlags.Default);
                        if (wrapped != 0)
                        {
                            Utils.CurrentMusic = wrapped;
                            if (metadata.Speed != 1.0f)
                            {
                                Bass.ChannelSetAttribute(Utils.CurrentMusic, ChannelAttribute.Tempo, (metadata.Speed - 1.0f) * 100f);
                            }
                            if (metadata.Pitch != 0.0f)
                            {
                                Bass.ChannelSetAttribute(Utils.CurrentMusic, ChannelAttribute.Pitch, metadata.Pitch);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to apply playback metadata FX: {ex}");
                    Utils.CurrentMusic = sourceStream;
                }
            }

            // create stream
            if (Utils.CurrentMusic == 0)
            {
                // Message.Data(Locale.OutsideItems.StartPlayingMessage1, $"{Locale.OutsideItems.StartPlayingMessage2}: " + Utils.currentSong);

                // DeleteSong(Utils.currentSongIndex, false);
                // return;

                if (Funcs.IsCurrentSongARssFeed())
                {
                    Utils.CurSongError = false;
                    Utils.CustomTopErrorMessage = Locale.UiMessages.RssFeedCanBeOpened +
                    (Preferences.rssSkipAfterTime ? " " + string.Format(Locale.UiMessages.RssWillSkipAfter, Preferences.rssSkipAfterTimeValue) : "");

                    // Initialize RSS auto-skip timer if enabled
                    if (Preferences.rssSkipAfterTime)
                    {
                        rssStartTime = DateTime.Now;
                    }
                }
                // skip to next song if skiperrors is enabled
                else if (Preferences.isSkipErrors && !Utils.PlaylistCheckedForAllTheSongsAndNoneOfThemWereFound)
                {
                    Log.Error("Skipping song");

                    songsThatWereNotFound++;
                    // Message.Data("Song not found", Utils.Songs.Length.ToString(), true);

                    if (songsThatWereNotFound >= Utils.Songs.Length) // to not show it all the time
                    {
                        if (showNoSongsFoundMessage)
                        {
                            Message.Data(Locale.UiMessages.AllSongsNotFound, Locale.UiMessages.NoSongsFoundTitle, true);
                            showNoSongsFoundMessage = false;
                        }
                        songsThatWereNotFound = 0;
                        Utils.PlaylistCheckedForAllTheSongsAndNoneOfThemWereFound = true;
                        return;
                    }

                    Message.Data(string.Format(Locale.UiMessages.SongNotFound, Utils.CurrentSongPath), Locale.OutsideItems.Error, true);
                    NextSong();
                    return;
                }

                Utils.CurSongError = true;
                Log.Error(Bass.LastError.ToString() + " " + Utils.CurrentSongPath);
            }
            else
            {
                Utils.CurSongError = false;
                Log.Info("Started playing: " + Utils.CurrentSongPath);
                songsThatWereNotFound = 0;
                Utils.PlaylistCheckedForAllTheSongsAndNoneOfThemWereFound = false;
                showNoSongsFoundMessage = true;
            }


            // set volume
            Bass.ChannelSetAttribute(Utils.CurrentMusic, ChannelAttribute.Volume, Preferences.GetVolume());

            // set FXs
            if (metadata.UseCustomEffects)
            {
                SetFXs(metadata.Effects);
            }
            else
            {
                SetFXs();
            }

            // set sync
            Bass.ChannelSetSync(Utils.CurrentMusic, SyncFlags.End, 0, (a, b, c, d) =>
            {
                MaybeNextSong();
                Start.drawWhole = true;
                Start.prevMusicTimePlayed = -1;
            }, IntPtr.Zero);

            // Trim end sync
            if (metadata.TrimEndSeconds.HasValue && Utils.CurrentMusic != 0)
            {
                long trimEndBytes = Bass.ChannelSeconds2Bytes(Utils.CurrentMusic, metadata.TrimEndSeconds.Value);
                long lengthBytes = Bass.ChannelGetLength(Utils.CurrentMusic);
                if (trimEndBytes > 0 && trimEndBytes < lengthBytes)
                {
                    Bass.ChannelSetSync(Utils.CurrentMusic, SyncFlags.Position, trimEndBytes, (a, b, c, d) =>
                    {
                        MaybeNextSong();
                        Start.drawWhole = true;
                        Start.prevMusicTimePlayed = -1;
                    }, IntPtr.Zero);
                }
            }

            Start.state = MainStates.playing;
            // play stream
            Start.prevMusicTimePlayed = -1;
            PlayDrawReset();
            Bass.ChannelPlay(Utils.CurrentMusic);

            // Seek to trim start after playback begins.
            if (metadata.TrimStartSeconds.HasValue && Utils.CurrentMusic != 0)
            {
                SeekSong((float)metadata.TrimStartSeconds.Value, false);
            }


            // TUI.RefreshCurrentView();
            Start.drawWhole = true;
        }

        public static void SetSoundFont(string soundFontPath)
        {
            BassMidi.FontFree(Utils.CurrentMusic);
            var newFont = BassMidi.FontInit(soundFontPath, FontInitFlags.Unicode);
            if (newFont == 0)
            {
                Message.Data(Locale.UiMessages.CannotLoadSoundFont, soundFontPath, true);
            }
            var sf = new MidiFont[]
            {
                new() {
                    Handle = newFont,
                    Preset = -1, // Use all presets
                    Bank = 0 // Use default bank(s)
                }
            };
            int m = BassMidi.StreamSetFonts(0, sf, 1);
            if (m == -1)
            {
                throw new Exception("Can't set the SoundFont");
            }
        }

        public static void SetFXs()
        {
            SetFXs(null);
        }

        public static void SetFXs(SongEffectSettings? songEffects)
        {
            // MARK: - EFFECT FX EXAMPLES
            // DXReverbParameters reverb = new()
            // {
            //     fInGain = 3.0f,
            //     fReverbMix = 3.0f,
            //     fReverbTime = 500.0f
            // };

            // int reverbHandle = Bass.ChannelSetFX(Utils.currentMusic, EffectType.DXReverb, 1);
            // Bass.FXSetParameters(reverbHandle, reverb);

            bool isChorus = songEffects?.IsChorus ?? Effects.isChorus;
            if (isChorus)
            {
                DXChorusParameters chorus = new()
                {
                    fWetDryMix = songEffects?.ChorusWetDryMix ?? Effects.chorusWetDryMix,
                    fDepth = songEffects?.ChorusDepth ?? Effects.chorusDepth,
                    fFeedback = songEffects?.ChorusFeedback ?? Effects.chorusFeedback,
                    fFrequency = songEffects?.ChorusFrequency ?? Effects.chorusFrequency,
                    fDelay = songEffects?.ChorusDelay ?? Effects.chorusDelay
                };

                int chorusHandle = Bass.ChannelSetFX(Utils.CurrentMusic, EffectType.Chorus, 1);
                Bass.FXSetParameters(chorusHandle, chorus);
            }

            bool isCompressor = songEffects?.IsCompressor ?? Effects.isCompressor;
            if (isCompressor)
            {
                DXCompressorParameters compressor = new()
                {
                    fGain = songEffects?.CompressorGain ?? Effects.compressorGain,
                    fAttack = songEffects?.CompressorAttack ?? Effects.compressorAttack,
                    fRelease = songEffects?.CompressorRelease ?? Effects.compressorRelease,
                    fThreshold = songEffects?.CompressorThreshold ?? Effects.compressorThreshold,
                    fRatio = songEffects?.CompressorRatio ?? Effects.compressorRatio,
                    fPredelay = songEffects?.CompressorPredelay ?? Effects.compressorPredelay
                };

                int compressorHandle = Bass.ChannelSetFX(Utils.CurrentMusic, EffectType.Compressor, 1);
                Bass.FXSetParameters(compressorHandle, compressor);
            }

            bool isDistortion = songEffects?.IsDistortion ?? Effects.isDistortion;
            if (isDistortion)
            {
                DXDistortionParameters distortion = new()
                {
                    fGain = songEffects?.DistortionGain ?? Effects.distortionGain,
                    fEdge = songEffects?.DistortionEdge ?? Effects.distortionEdge,
                    fPostEQCenterFrequency = songEffects?.DistortionPostEQCenterFrequency ?? Effects.distortionPostEQCenterFrequency
                };

                int distortionHandle = Bass.ChannelSetFX(Utils.CurrentMusic, EffectType.Distortion, 1);
                Bass.FXSetParameters(distortionHandle, distortion);
            }

            bool isEcho = songEffects?.IsEcho ?? Effects.isEcho;
            if (isEcho)
            {
                DXEchoParameters echo = new()
                {
                    fWetDryMix = songEffects?.EchoWetDryMix ?? Effects.echoWetDryMix,
                    fFeedback = songEffects?.EchoFeedback ?? Effects.echoFeedback,
                    fLeftDelay = songEffects?.EchoLeftDelay ?? Effects.echoLeftDelay,
                    fRightDelay = songEffects?.EchoRightDelay ?? Effects.echoRightDelay,
                    lPanDelay = songEffects?.EchoPanDelay ?? Effects.echoPanDelay
                };

                int echoHandle = Bass.ChannelSetFX(Utils.CurrentMusic, EffectType.Echo, 1);
                Bass.FXSetParameters(echoHandle, echo);
            }

            bool isFlanger = songEffects?.IsFlanger ?? Effects.isFlanger;
            if (isFlanger)
            {
                DXFlangerParameters flanger = new()
                {
                    fWetDryMix = songEffects?.FlangerWetDryMix ?? Effects.flangerWetDryMix,
                    fDepth = songEffects?.FlangerDepth ?? Effects.flangerDepth,
                    fFeedback = songEffects?.FlangerFeedback ?? Effects.flangerFeedback,
                    fFrequency = songEffects?.FlangerFrequency ?? Effects.flangerFrequency,
                    fDelay = songEffects?.FlangerDelay ?? Effects.flangerDelay,
                };

                int flangerHandle = Bass.ChannelSetFX(Utils.CurrentMusic, EffectType.DXFlanger, 1);
                Bass.FXSetParameters(flangerHandle, flanger);
            }

            bool isGargle = songEffects?.IsGargle ?? Effects.isGargle;
            if (isGargle)
            {
                DXGargleParameters gargle = new()
                {
                    dwRateHz = songEffects?.GargleRate ?? Effects.gargleRate,
                };

                int gargleHandle = Bass.ChannelSetFX(Utils.CurrentMusic, EffectType.DXGargle, 1);
                Bass.FXSetParameters(gargleHandle, gargle);
            }

            bool isParamEQ = songEffects?.IsParamEQ ?? Effects.isParamEQ;
            if (isParamEQ)
            {
                DXParamEQParameters paramEq = new()
                {
                    fCenter = songEffects?.ParamEQCenter ?? Effects.paramEQCenter,
                    fBandwidth = songEffects?.ParamEQBandwidth ?? Effects.paramEQBandwidth,
                    fGain = songEffects?.ParamEQGain ?? Effects.paramEQGain
                };

                int paramEqHandle = Bass.ChannelSetFX(Utils.CurrentMusic, EffectType.DXParamEQ, 1);
                Bass.FXSetParameters(paramEqHandle, paramEq);
            }

            bool isReverb = songEffects?.IsReverb ?? Effects.isReverb;
            if (isReverb)
            {
                DXReverbParameters reverb = new()
                {
                    fInGain = songEffects?.ReverbInGain ?? Effects.reverbInGain,
                    fReverbMix = songEffects?.ReverbReverbMix ?? Effects.reverbReverbMix,
                    fReverbTime = songEffects?.ReverbReverbTime ?? Effects.reverbReverbTime,
                    fHighFreqRTRatio = songEffects?.ReverbHighFreqRTRatio ?? Effects.reverbHighFreqRTRatio
                };

                int reverbHandle = Bass.ChannelSetFX(Utils.CurrentMusic, EffectType.DXReverb, 1);
                Bass.FXSetParameters(reverbHandle, reverb);
            }
        }

        /// <summary>
        /// Checks if the current RSS feed should be auto-skipped based on timer
        /// </summary>
        /// <returns>True if RSS should be skipped, false otherwise</returns>
        public static bool ShouldSkipRss()
        {
            // Only check for RSS auto-skip if all conditions are met
            if (!Preferences.rssSkipAfterTime ||
                !Funcs.IsCurrentSongARssFeed() ||
                Funcs.IsInsideOfARssFeed() ||
                rssStartTime == null)
            {
                return false;
            }
            // Check if enough time has elapsed
            TimeSpan elapsed = CalculateElapsedTime();
            return elapsed.TotalSeconds >= Preferences.rssSkipAfterTimeValue;
        }

        private static TimeSpan CalculateElapsedTime()
        {
            return rssStartTime.HasValue ? DateTime.Now - rssStartTime.Value : TimeSpan.Zero;
        }
    }
}
