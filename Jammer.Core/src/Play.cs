using ManagedBass;
using ManagedBass.Aac;
using ManagedBass.DirectX8;
using ManagedBass.Midi;
// using ManagedBass.Fx;
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
                Utils.songs = Utils.songs.Where((source, i) => i != Currentindex).ToArray();
                if (Utils.songs.Length == 0)
                {
                    Start.state = MainStates.pause;
                    return;
                }
                if (Currentindex == Utils.songs.Length)
                {
                    Currentindex--;
                }
                PlaySong(Utils.songs, Currentindex);
                return;
            }

            if (songs.Length == 0)
            {
                AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.NoSongsInPlaylist}[/]");
                Currentindex = 0;
                Start.Run(new string[] {});
                return;
            }

            while(Currentindex > songs.Length){
                Currentindex--;
            }
            Debug.dprint("Play song");

            Utils.currentSongIndex = Currentindex;
            Utils.currentPlaylistSongIndex = Currentindex;
            
            // get song details
            // Utils.Song song = UtilFuncs.GetSongDetails(songs[Utils.currentSongIndex]);
            Song song = new Song() {
                URI = songs[Utils.currentSongIndex]
            };
            string fullPath = "";

            song.ExtractSongDetails();

            // check if file is a local
            if (System.IO.File.Exists(song.URI))
            {
                // id related to local file path, convert to absolute path
                fullPath = Path.GetFullPath(song.URI);
            }
            // if folder
            else if (Directory.Exists(song.URI))
            {
                // skip if folder
                NextSong();
                return;
            }
            else if (URL.isValidSoundCloudPlaylist(song.URI)) {
                // id related to url, download and convert to absolute path
                Debug.dprint("Soundcloud playlist.");
                fullPath = Download.GetSongsFromPlaylist(song.URI, "soundcloud");
            }
            else if (URL.IsValidSoundcloudSong(song.URI))
            {
                // id related to url, download and convert to absolute path
                fullPath = Download.DownloadSong(song.URI);
            }
            else if (URL.IsValidYoutubePlaylist(song.URI))
            {
                // id related to url, download and convert to absolute path
                fullPath = Download.GetSongsFromPlaylist(song.URI, "youtube");
            }
            else if (URL.IsValidYoutubeSong(song.URI))
            {
                // id related to url, download and convert to absolute path
                fullPath = Download.DownloadSong(song.URI);
            }
            else if (URL.IsUrl(song.URI))
            {
                fullPath = Download.DownloadSong(song.URI);
                // Message.Data(path, song);
            }

            // Message.Data(fullPath, "path");

            // Message.Data(fullPath + " || " + song.Path, "path");
            // if the Utils.songs current is not the same as the song.Path
            if (fullPath != Utils.songs[Utils.currentSongIndex])
            {
                song.Title = ""; // TODO might break something :/
                song.URI = Utils.songs[Utils.currentSongIndex];
            }
            // Message.Data(fullPath + " || " + song.Path, "path");

            Start.prevMusicTimePlayed = -1;
            Start.lastSeconds = -1;
            Utils.currentSong = fullPath;
            Start.drawWhole = true;

            Log.Info("Playing: " + Utils.songs[Utils.currentSongIndex]);

            // Message.Data(Utils.currentSongIndex + "#" + Utils.currentPlaylistSongIndex, "s");
            // taglib get title to display

            TagLib.File? tagFile;
            string title = "", author = "", album = "", year = "", genre = "";
            try {
                tagFile = TagLib.File.Create(fullPath);
                title = tagFile.Tag.Title;
                author = tagFile.Tag.FirstPerformer;
                album = tagFile.Tag.Album;
                year = tagFile.Tag.Year.ToString();
                genre = tagFile.Tag.FirstGenre;
            } catch (Exception) {
                tagFile = null;
                Log.Error("Error getting title of the song");
            }
            

            // append title to song
            if (song.Title == null || song.Title == "")
            {
                song.Title = title;
            }
            if (song.Author == null || song.Author == "")
            {
                song.Author = author;
            }
            if (song.Album == null || song.Album == "")
            {
                song.Album = album;
            }
            if (song.Year == null || song.Year == "" || song.Year == "0")
            {
                song.Year = year;
            }
            if (song.Genre == null || song.Genre == "")
            {
                song.Genre = genre;
            }

            Utils.songs[Utils.currentSongIndex] = song.ToSongString();
            // Concatenate all song strings
            // string allSongs = string.Join("\n", Utils.songs);
            //Message.Data(allSongs, "Current Playlist");

            Start.drawWhole = true;

            try
            {
                string extension = Path.GetExtension(fullPath).ToLower();

                if (extension == ".jammer")
                {
                    HandleJammerPlaylist(fullPath);
                }
                else if (extension == ".m3u" || extension == ".m3u8")
                {
                    Utils.songs = M3u.ParseM3u(fullPath);

                    PlaySong(Utils.songs, Utils.currentSongIndex);
                }
                else
                {
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

        public static void HandleJammerPlaylist(string fullPath)
        {
            Debug.dprint("jammer");

            // get absolute path
            Utils.currentPlaylist = Path.GetFullPath(fullPath);

            string[] playlist = System.IO.File.ReadAllLines(fullPath);

            // MARK: - Detect if playlist is using the old format
            string newPlaylist = "";
            bool isOldFormat = false;
            foreach (string s in playlist)
            {
                if (s.Contains('½') && !s.Contains(Utils.jammerFileDelimeter))
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
                    "Update Playlist? (y/n)",
                    "Hold On a Second! 🤠" + Environment.NewLine + 
                    "This might be an old playlist format." + Environment.NewLine +
                    "Do you want to update it to the new format?" + Environment.NewLine +
                    "The old format is outdated and will not work at all." + Environment.NewLine +
                    "but just incase a backup will be created to 'playlist/backups'.",
                    true
                    );
                
                if (input == "y")
                {
                    if (!Directory.Exists(Path.Combine(Utils.JammerPath, "playlists", "backups")))
                    {
                        Directory.CreateDirectory(Path.Combine(Utils.JammerPath, "playlists", "backups"));
                    }

                    string backupPath = Path.Combine(Utils.JammerPath, "playlists", "backups", Path.GetFileNameWithoutExtension(fullPath) + "_"+ DateTime.Now.ToString("dd-MM_HH-mm-ss") + ".jammer");
                    System.IO.File.WriteAllText(backupPath, System.IO.File.ReadAllText(fullPath), Encoding.UTF8);

                    // Message.Data(fullPath + " " + newPlaylist, "newPlaylist");
                    System.IO.File.WriteAllText(fullPath, newPlaylist, Encoding.UTF8);
                    playlist = System.IO.File.ReadAllLines(fullPath);
                }
            }

            // add all songs in playlist to Utils.songs
            foreach (string s in playlist)
            {
                AddSong(s, false);
            }
            // remove playlist from Utils.songs
            Utils.songs = Utils.songs.Where((source, i) => i != Utils.currentSongIndex).ToArray();
            if (Utils.currentSongIndex == Utils.songs.Length)
            {
                Utils.currentSongIndex = Utils.songs.Length - 1;
            }
            Start.state = MainStates.playing;
            PlaySong(Utils.songs, Utils.currentSongIndex);
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
            Bass.ChannelPlay(Utils.currentMusic);
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
            Start.drawWhole = true;
        }
        public static void NextSong()
        {
            if (Utils.songs.Length == 0)
            {
                Start.state = MainStates.pause;
                return;
            }

            if(Utils.queueSongs.Count > 0){
                PlayDrawReset();
                PlaySong(Utils.queueSongs.ToArray(), 0);
                PlaySong();
                int index = Array.IndexOf(Utils.songs, Utils.queueSongs[0]);
                Utils.previousSongIndex = Utils.currentSongIndex;
                Utils.currentSongIndex = index;
                Utils.queueSongs.RemoveAt(0);
            } else {

                if (Utils.songs.Length >= 1) // no next song if only one song or less
                {
                    Utils.currentSongIndex = (Utils.currentSongIndex + 1) % Utils.songs.Length;
                    PlayDrawReset();
                    PlaySong(Utils.songs, Utils.currentSongIndex);
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
            if (Utils.songs.Length == 0)
            {
                Start.state = MainStates.pause;
                return;
            }

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
                    pos = pos - Bass.ChannelSeconds2Bytes(Utils.currentMusic, Preferences.GetRewindSeconds());
                }
                else
                {
                    pos = pos + Bass.ChannelSeconds2Bytes(Utils.currentMusic, Preferences.GetForwardSeconds());
                }
                // Clamp again to ensure it's within the valid range
                //pos = Math.Max(0, Math.Min(pos, Utils.audioStream.Length));
            }
            else
            {
                // Clamp the seek position to be within the valid range [0, Utils.audioStream.Length]
                pos = Bass.ChannelSeconds2Bytes(Utils.currentMusic, seconds);
            }

            // Update the audio stream's position
            //if (Utils.audioStream.Length == pos)
            if (pos < 0)
            {
                Bass.ChannelSetPosition(Utils.currentMusic, 0);
            }
            else if (pos >= Bass.ChannelGetLength(Utils.currentMusic))
            {
                Bass.ChannelSetPosition(Utils.currentMusic, Bass.ChannelGetLength(Utils.currentMusic) - 1);
            }
            else
            {
                Bass.ChannelSetPosition(Utils.currentMusic, pos);
            }

            if (Bass.ChannelGetPosition(Utils.currentMusic) >= Bass.ChannelGetLength(Utils.currentMusic))
            {
                MaybeNextSong();
            }
            Start.prevMusicTimePlayed = -1;
            // PlayDrawReset();
            return;
        }
        public static void SetVolume(float volume) {
            Preferences.volume = volume;
            if (Preferences.volume > 1) {
                Preferences.volume = 1;
            } else if (Preferences.volume < 0) {
                Preferences.volume = 0;
            }
            if(volume > 0) {
                Preferences.isMuted = false;
            }
            Bass.ChannelSetAttribute(Utils.currentMusic, ChannelAttribute.Volume, Preferences.volume);

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
            if (Preferences.isLoop)
            {
                Bass.ChannelSetPosition(Utils.currentMusic, 0);
                Bass.ChannelPlay(Utils.currentMusic);
            }
            else if (Utils.songs.Length == 1 && !Preferences.isLoop){
                //Utils.audioStream.Position = Utils.audioStream.Length;
                Bass.ChannelSetPosition(Utils.currentMusic, Bass.ChannelGetLength(Utils.currentMusic));
                Start.state = MainStates.pause;
                //Utils.currentMusic.Pause();
                Bass.ChannelPause(Utils.currentMusic);
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

        public static void ReDownloadSong()
        {
            if (Utils.songs[Utils.currentSongIndex].Contains("https://") || Utils.songs[Utils.currentSongIndex].Contains("http://"))
            {
                System.IO.File.Delete(Utils.currentSong);
                PlaySong(Utils.songs, Utils.currentSongIndex);
                SeekSong(0, false);
                return;
            }
        }

        public static void AddSong(string song, bool AddNext = true )
        {
            if (AddNext)
                Utils.songs = Utils.songs.Take(Utils.currentSongIndex + 1).Concat(new string[] { song }).Concat(Utils.songs.Skip(Utils.currentSongIndex + 1)).ToArray();
            else {
                // add song to current Utils.songs
                Array.Resize(ref Utils.songs, Utils.songs.Length + 1);
                Utils.songs[Utils.songs.Length - 1] = song;
            }

            if (Utils.songs.Length == 1)
            {
                Utils.currentSongIndex = 0;
                PlaySong(Utils.songs, Utils.currentSongIndex);
            }
        }
        public static void  DeleteSong(int index, bool isQueue, bool hardDelete = false)
        {
            if (Utils.songs.Length == 0)
            {
                return;
            }
            // check if index is in range
            if (index < 0 || index > Utils.songs.Length)
            {
                AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.IndexOoR}[/]");
                return;
            }
            // delete song from disk
            if (hardDelete)
            {
                System.IO.File.Delete(Utils.currentSong);
            }

            // remove song from current Utils.songs
            Utils.songs = Utils.songs.Where((source, i) => i != index).ToArray();
            Utils.currentSongIndex--;
            if(Utils.currentSongIndex == -1){
                Utils.currentSongIndex = 0;
            }
            // PREV RESET
            // Console.WriteLine((index < Utils.currentSongIndex   ) + " " + Utils.currentPlaylistSongIndex);
            if (index == Utils.songs.Length){
                if (Utils.songs.Length == 0) {
                    Utils.songs = new string[] { "" };
                    ResetMusic();
                    Start.state = MainStates.pause;
                }
                else if(index >= Utils.currentSongIndex){
                    _ = Utils.currentSongIndex;
                }
                else {
                    Utils.currentSongIndex = Utils.songs.Length - 1;
                    // Start.state = MainStates.playing;
                }
            } else {
                if(index < Utils.currentPlaylistSongIndex && index != Utils.currentPlaylistSongIndex){
                    if(Utils.currentPlaylistSongIndex == Utils.songs.Length){
                        Utils.currentPlaylistSongIndex--;
                    } else {
                        Utils.currentPlaylistSongIndex++;
                    }
                } else {
                    Utils.currentPlaylistSongIndex++;
                }
            }

            // if no songs left, add "" to Utils.songs
            if (!isQueue)
            {
                PlaySong(Utils.songs, Utils.currentSongIndex);
            }
            else
            {
                if (Utils.currentSongIndex == index)
                {
                    PlaySong(Utils.songs, Utils.currentSongIndex);
                }
            }
        }
        public static void SetEffectsToChannel() {
            
            // Start playing from the same position
            long pos = Bass.ChannelGetPosition(Utils.currentMusic);
            StartPlaying();
            Bass.ChannelSetPosition(Utils.currentMusic, pos);
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
            
            // Message.Data(Utils.currentSong, "Playing: ");
            // flags
            BassFlags flags = BassFlags.Default;
            
            BassAac.PlayAudioFromMp4 = true;
            BassAac.AacSupportMp4 = true;

            ////
            Utils.currentMusic = Bass.CreateStream(Utils.currentSong, 0, 0, flags);
            //Message.Data(Utils.currentMusic.ToString(), "Current Music");
            if (Utils.currentMusic == 0)
                Utils.currentMusic = BassAac.CreateStream(Utils.currentSong, 0, 0, flags);
                //Message.Data(Utils.currentMusic.ToString(), "Current Music");
            if (Utils.currentMusic == 0)
                Utils.currentMusic = BassAac.CreateMp4Stream(Utils.currentSong, 0, 0, flags);
                //Message.Data(Utils.currentMusic.ToString(), "Current Music");
            if (Utils.currentMusic == 0) {
                int newFont;
                newFont = BassMidi.FontInit(Path.Combine(Utils.JammerPath, "soundfonts", Preferences.GetCurrentSf2()), FontInitFlags.Unicode);

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
                Utils.currentMusic = BassMidi.CreateStream(Utils.currentSong, 0, 0, flags);
            }
            
            // create stream
            if (Utils.currentMusic == 0)
            {
                // Message.Data(Locale.OutsideItems.StartPlayingMessage1, $"{Locale.OutsideItems.StartPlayingMessage2}: " + Utils.currentSong);

                // DeleteSong(Utils.currentSongIndex, false);
                // return;
                Utils.curSongError = true;
                Log.Error(Bass.LastError.ToString() + " " + Utils.currentSong);
            } 
            else {
                Utils.curSongError = false;
                Log.Info("Started playing: " + Utils.currentSong);
            }

            // set volume
            Bass.ChannelSetAttribute(Utils.currentMusic, ChannelAttribute.Volume, Preferences.GetVolume());

            // set FXs
            SetFXs();

            // set sync
            Bass.ChannelSetSync(Utils.currentMusic, SyncFlags.End, 0, (a, b, c, d) => {
                MaybeNextSong();
                Start.drawWhole = true;
                Start.prevMusicTimePlayed = -1;
            }, IntPtr.Zero);


            Start.state = MainStates.playing;
            // play stream
            Start.prevMusicTimePlayed = -1;
            PlayDrawReset();
            Bass.ChannelPlay(Utils.currentMusic);


            // TUI.RefreshCurrentView();
            Start.drawWhole = true;
        }

        public static void SetSoundFont(string soundFontPath)
        {
            BassMidi.FontFree(Utils.currentMusic);
            var newFont = BassMidi.FontInit(soundFontPath, FontInitFlags.Unicode);
            if (newFont == 0)
            {
                Message.Data("Can't load the SoundFont", soundFontPath, true);
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
        
        public static void SetFXs() {
            // MARK: - EFFECT FX EXAMPLES
            // DXReverbParameters reverb = new()
            // {
            //     fInGain = 3.0f,
            //     fReverbMix = 3.0f,
            //     fReverbTime = 500.0f
            // };

            // int reverbHandle = Bass.ChannelSetFX(Utils.currentMusic, EffectType.DXReverb, 1);
            // Bass.FXSetParameters(reverbHandle, reverb);

            if (Effects.isChorus) {
                DXChorusParameters chorus = new()
                {
                    fWetDryMix = Effects.chorusWetDryMix,
                    fDepth = Effects.chorusDepth,
                    fFeedback = Effects.chorusFeedback,
                    fFrequency = Effects.chorusFrequency,
                    fDelay = Effects.chorusDelay
                };

                int chorusHandle = Bass.ChannelSetFX(Utils.currentMusic, EffectType.Chorus, 1);
                Bass.FXSetParameters(chorusHandle, chorus);
            }

            if (Effects.isCompressor) {
                DXCompressorParameters compressor = new()
                {
                    fGain = Effects.compressorGain,
                    fAttack = Effects.compressorAttack,
                    fRelease = Effects.compressorRelease,
                    fThreshold = Effects.compressorThreshold,
                    fRatio = Effects.compressorRatio,
                    fPredelay = Effects.compressorPredelay
                };

                int compressorHandle = Bass.ChannelSetFX(Utils.currentMusic, EffectType.Compressor, 1);
                Bass.FXSetParameters(compressorHandle, compressor);
            }

            if (Effects.isDistortion) {
                DXDistortionParameters distortion = new()
                {
                    fGain = Effects.distortionGain,
                    fEdge = Effects.distortionEdge,
                    fPostEQCenterFrequency = Effects.distortionPostEQCenterFrequency
                };

                int distortionHandle = Bass.ChannelSetFX(Utils.currentMusic, EffectType.Distortion, 1);
                Bass.FXSetParameters(distortionHandle, distortion);
            }

            if (Effects.isEcho) {
                DXEchoParameters echo = new()
                {
                    fWetDryMix = Effects.echoWetDryMix,
                    fFeedback = Effects.echoFeedback,
                    fLeftDelay = Effects.echoLeftDelay,
                    fRightDelay = Effects.echoRightDelay,
                    lPanDelay = Effects.echoPanDelay
                };

                int echoHandle = Bass.ChannelSetFX(Utils.currentMusic, EffectType.Echo, 1);
                Bass.FXSetParameters(echoHandle, echo);
            }

            if (Effects.isFlanger) {
                DXFlangerParameters flanger = new()
                {
                    fWetDryMix = Effects.flangerWetDryMix,
                    fDepth = Effects.flangerDepth,
                    fFeedback = Effects.flangerFeedback,
                    fFrequency = Effects.flangerFrequency,
                    fDelay = Effects.flangerDelay,
                };

                int flangerHandle = Bass.ChannelSetFX(Utils.currentMusic, EffectType.DXFlanger, 1);
                Bass.FXSetParameters(flangerHandle, flanger);
            }

            if (Effects.isGargle) {
                DXGargleParameters gargle = new()
                {
                    dwRateHz = Effects.gargleRate,
                };

                int gargleHandle = Bass.ChannelSetFX(Utils.currentMusic, EffectType.DXGargle, 1);
                Bass.FXSetParameters(gargleHandle, gargle);
            }

            if (Effects.isParamEQ) {
                DXParamEQParameters paramEq = new()
                {
                    fCenter = Effects.paramEQCenter,
                    fBandwidth = Effects.paramEQBandwidth,
                    fGain = Effects.paramEQGain
                };

                int paramEqHandle = Bass.ChannelSetFX(Utils.currentMusic, EffectType.DXParamEQ, 1);
                Bass.FXSetParameters(paramEqHandle, paramEq);
            }

            if (Effects.isReverb) {
                DXReverbParameters reverb = new()
                {
                    fInGain = Effects.reverbInGain,
                    fReverbMix = Effects.reverbReverbMix,
                    fReverbTime = Effects.reverbReverbTime,
                    fHighFreqRTRatio = Effects.reverbHighFreqRTRatio
                };

                int reverbHandle = Bass.ChannelSetFX(Utils.currentMusic, EffectType.DXReverb, 1);
                Bass.FXSetParameters(reverbHandle, reverb);
            }
        }
    }
}
