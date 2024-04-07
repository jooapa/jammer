using SoundCloudExplode;
using YoutubeExplode;
using YoutubeExplode.Common;
using Spectre.Console;
using System.IO;
using TagLib;

namespace jammer {
    internal class Download {
        public static string songPath = "";
        static SoundCloudClient soundcloud = new SoundCloudClient();
        static string url = "";
        static string[] playlistSongs = { "" };
        static readonly YoutubeClient youtube = new();


        public static string DownloadSong(string url2) {
            songPath = "";
            url = url2;
            Debug.dprint($"{Locale.OutsideItems.Downloading}: " + url2.ToString());
            if (URL.IsValidSoundcloudSong(url)) {
                DownloadSoundCloudTrackAsync(url).Wait();
            } else if (URL.IsValidYoutubeSong(url)) {
                DownloadYoutubeTrackAsync(url).Wait();
            } else {
                #if CLI_UI
                Console.WriteLine(Locale.OutsideItems.InvalidUrl);
                #endif
                #if AVALONIA_UI
                // TODO AVALONIA_UI
                #endif
                Debug.dprint("Invalid url");
            }

            return songPath;
        }

        private static async Task DownloadYoutubeTrackAsync(string url)
        {
            string formattedUrl = FormatUrlForFilename(url);

            songPath = Path.Combine(
                Preferences.songsPath,
                formattedUrl
            );

            if (System.IO.File.Exists(songPath))
            {
                return;
            }

            try
            {
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                var streamInfo = streamManifest.GetAudioStreams().FirstOrDefault();
                var video = await youtube.Videos.GetAsync(url);
                
                if (streamInfo != null)
                {
                    var progress = new Progress<double>(data =>
                    {
                        #if CLI_UI
                        AnsiConsole.Clear();
                        Console.WriteLine($"{Locale.OutsideItems.Downloading} {url}: {data:P}");
                        #endif
                        #if AVALONIA_UI
                        // TODO AVALONIA_UI
                        #endif
                    });

                    await youtube.Videos.Streams.DownloadAsync(streamInfo, songPath, progress);

                    // TagLib
                    var file = TagLib.File.Create(songPath);
                    file.Tag.Title = Start.Sanitize(video.Title);
                    file.Tag.Performers = new string[] { video.Author.ChannelTitle };
                    file.Save();
                }
                else
                {
                    #if CLI_UI
                    jammer.Message.Data(Locale.OutsideItems.NoAudioStream, Locale.OutsideItems.Error);
                    #endif
                    #if AVALONIA_UI
                    // TODO AVALONIA_UI
                    #endif
                }
            }
            catch (Exception ex)
            {
                #if CLI_UI
                jammer.Message.Data($"{Locale.OutsideItems.Error}: " + ex.Message, "Error");
                #endif
                #if AVALONIA_UI
                // TODO AVALONIA_UI
                #endif
                songPath = "";
            }
        }

        public static async Task DownloadSoundCloudTrackAsync(string url) {
            // if already downloaded, don't download again
            string formattedUrl = FormatUrlForFilename(url);

            songPath = Path.Combine(
                Preferences.songsPath,
                formattedUrl
            );

            if (System.IO.File.Exists(songPath)) {
                return;
            }

            try {
                var track = await soundcloud.Tracks.GetAsync(url);

                if (track != null) {

                    if(track.Title != null){

                        var progress = new Progress<double>(data => {
                            #if CLI_UI
                            AnsiConsole.Clear();
                            Console.WriteLine($"{Locale.OutsideItems.Downloading} {url}: {data:P} to {songPath}");  
                            #endif
                            #if AVALONIA_UI
                            // TODO AVALONIA_UI
                            #endif
                        });
                        
                        await soundcloud.DownloadAsync(track, songPath, progress);

                        // TagLib
                        var file = TagLib.File.Create(songPath);
                        file.Tag.Title = Start.Sanitize(track.Title);
                        file.Tag.Description = track.Description;
                        if (track.User != null && track.User.Username != null) {
                            file.Tag.Performers = new string[] { track.User.Username };
                        }

                        file.Save();

                    } else {
                        Debug.dprint("track title returns null");
                    }
                } else {
                    Debug.dprint("track returns null");
                }

            }
            catch (Exception ex) { 
                #if CLI_UI
                jammer.Message.Data($"{Locale.OutsideItems.Error}: " + ex.Message +": "+ url
                , Locale.OutsideItems.DownloadErrorSoundcloud);
                #endif
                #if AVALONIA_UI
                // TODO AVALONIA_UI
                #endif
                songPath = "";
            }
        }

        public static async Task GetPlaylist(string url) {

            var soundcloud = new SoundCloudClient();

            // Get all playlist tracks
            var playlist = await soundcloud.Playlists.GetAsync(url, true);

            if (playlist.Tracks.Count() == 0 || playlist.Tracks == null) {
                #if CLI_UI
                Console.WriteLine(Locale.OutsideItems.NoTrackPlaylist);
                Console.ReadLine();
                #endif
                #if AVALONIA_UI
                // TODO AVALONIA_UI
                #endif
                return;
            }

            // add all tracks permalinkUrl to songs array
            playlistSongs = new string[playlist.Tracks.Count()];
            int i = 0;
            foreach (var track in playlist.Tracks) {
                playlistSongs[i] = track.PermalinkUrl?.ToString() ?? string.Empty;
                i++;
            }
        }
        public static async Task GetPlaylistYoutube(string url) {
            // Get all playlist tracks
            var playlist = await youtube.Playlists.GetVideosAsync(url);
            #if CLI_UI
            Console.WriteLine(playlist[0]);
            #endif
            #if AVALONIA_UI
            // TODO AVALONIA_UI
            #endif
            if (playlist.Count() == 0 || playlist == null) {
                #if CLI_UI
                Console.WriteLine(Locale.OutsideItems.NoTrackPlaylist);
                Console.ReadLine();
                #endif
                #if AVALONIA_UI
                // TODO AVALONIA_UI
                #endif
                return;
            }

            // add all tracks permalinkUrl to songs array
            playlistSongs = new string[playlist.Count()];
            int i = 0;
            foreach (var track in playlist) {
                var _url = track.Url?.ToString() ?? string.Empty;
                var index = _url.IndexOf('&');
                if (index != -1) {
                    _url = _url.Substring(0, index);
                }
                playlistSongs[i] = _url;
                i++;
            }
        }


        public static string GetSongsFromPlaylist(string url, string service) {
            if(service == "soundcloud"){
                GetPlaylist(url).Wait();
            }
            else if( service == "youtube"){
                GetPlaylistYoutube(url).Wait();

            }
            

            // remove the CurrentSong from Utils.songs
            Utils.songs = Utils.songs.Where(val => val != Utils.songs[Utils.currentSongIndex]).ToArray();
            // add all songs from playlist to Utils.songs but start adding at the currentSongIndex
            Utils.songs = Utils.songs.Take(Utils.currentSongIndex).Concat(playlistSongs).Concat(Utils.songs.Skip(Utils.currentSongIndex)).ToArray();
            // delete duplicate songs
            Utils.songs = Utils.songs.Distinct().ToArray();

            return DownloadSong(Utils.songs[Utils.currentSongIndex]);
        }

        public static string FormatUrlForFilename(string url, bool isCheck = false)
        {
            if (URL.isValidSoundCloudPlaylist(url)) {
                return "Soundcloud Playlist";
            }
            else if (URL.IsValidSoundcloudSong(url))
            {
                // remove ? and everything after
                int index = url.IndexOf("?");
                if (index > 0)
                {
                    url = url.Substring(0, index);
                }

                string formattedSCUrl = url.Replace("https://", "")
                                     .Replace("/", " ")
                                     .Replace("?", " ");
                if (isCheck)
                {
                    return formattedSCUrl;
                }
                else
                {
                    return formattedSCUrl + ".mp3";
                }
            }
            else if (URL.IsValidYoutubeSong(url))
            {
                int index = url.IndexOf("&");
                if (index > 0)
                {
                    url = url.Substring(0, index);
                }
            }
            string formattedYTUrl = url.Replace("https://", "")
                                     .Replace("/", " ")
                                     .Replace("?", " ");

                            

            if (isCheck)
            {
                return formattedYTUrl;
            }
            else
            {
                return formattedYTUrl + ".mp4";
            }
        }
    }
}
