using SoundCloudExplode;
using YoutubeExplode;
using YoutubeExplode.Common;
using Spectre.Console;
using YoutubeExplode.Videos;

namespace jammer {
    internal class Download {
        public static string songPath = "";
        static SoundCloudClient soundcloud = new SoundCloudClient();
        static string url = "";
        static string[] playlistSongs = { "" };
        static readonly YoutubeClient youtube = new();
        private static string pipe = "";


        public static (string, string) DownloadSong(string url2) {
            songPath = "";
            pipe = "";
            url = url2;
            Debug.dprint($"{Locale.OutsideItems.Downloading}: " + url2.ToString());
            if (URL.IsValidSoundcloudSong(url)) {
                DownloadSoundCloudTrackAsync(url).Wait();
            } else if (URL.IsValidYoutubeSong(url)) {
                DownloadYoutubeTrackAsync(url).Wait();
            } else {
                Console.WriteLine(Locale.OutsideItems.InvalidUrl);
                Debug.dprint("Invalid url");
            }

            return (songPath, pipe);
        }

        public static (string, string) CheckExistingSong(string url) {
            // Message.Data("Checking existing song: " + url, "Check Existing Song");
            string formattedUrl = FormatUrlForFilename(url, true);
            // Message.Data("FormaUIHUIHUHItdted URL: " + formattedUrl, "Check Existing Song '" + formattedUrl + "'");
            string[] files = Directory.GetFiles(Utils.jammerPath);
            foreach (string file in files)
            {
                // Message.Data("File: " + file, "Check Existing Song '" + file.Contains(formattedUrl) + "'" + formattedUrl);
                if (file.Contains(formattedUrl))
                {
                    // return the path and the title of the song using the pipe
                    string[] split = file.Split("^");
                    return (file, split[1].Substring(0, split[1].Length - 4));
                }
            }
            return ("", "");
        }
        private static async Task DownloadYoutubeTrackAsync(string url)
        {
            string formattedUrl = FormatUrlForFilename(url);

            songPath = Path.Combine(
                Utils.jammerPath,
                formattedUrl
            );
            string construction = songPath;
            string value = "";
            (value, pipe) = CheckExistingSong(url);
            // Message.Data("Value: " + value, "Check Existing Song:" + formattedUrl.Substring(0, formattedUrl.Length - 4));
            if (value != "")
            {
                songPath = value;
                return;
            }

            pipe = "";
            try
            {
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                var streamInfo = streamManifest.GetAudioStreams().FirstOrDefault();
                var video = await youtube.Videos.GetAsync(url);
                
                if (streamInfo != null)
                {
                    var progress = new Progress<double>(data =>
                    {
                        AnsiConsole.Clear();
                        Console.WriteLine($"{Locale.OutsideItems.Downloading} {url}: {data:P}");
                    });

                    // metadata to pipe
                    pipe = video.Title;

                    await youtube.Videos.Streams.DownloadAsync(streamInfo, songPath, progress);
                    int pos_dot = songPath.LastIndexOf(".");
                    construction = songPath[..pos_dot] + "^" + pipe + ".mp4";
                    File.Move(songPath, construction);
                }
                else
                {
                    Message.Data(Locale.OutsideItems.NoAudioStream, Locale.OutsideItems.Error);
                }
                songPath = construction;
            }
            catch (Exception ex)
            {
                Message.Data($"{Locale.OutsideItems.Error}: " + ex.Message, "Error");
                songPath = "";
            }
        }

        public static async Task DownloadSoundCloudTrackAsync(string url) {
            // if already downloaded, don't download again
            string formattedUrl = FormatUrlForFilename(url);

            songPath = Path.Combine(
                Utils.jammerPath,
                formattedUrl
            );

            string construction = songPath; 
            string value = "";
            (value, pipe) = CheckExistingSong(url);
            if (value != "")
            {
                songPath = value;
                return;
            }

            pipe = "";

            try {
                var track = await soundcloud.Tracks.GetAsync(url);

                if (track != null) {

                    if(track.Title != null){

                        var progress = new Progress<double>(data => {
                            AnsiConsole.Clear();
                            Console.WriteLine($"{Locale.OutsideItems.Downloading} {url}: {data:P}");    
                        });

                        // metadata to pipe
                        pipe = track.Title;

                        await soundcloud.DownloadAsync(track, songPath, progress);

                        int pos_dot = songPath.LastIndexOf(".");
                        construction = songPath[..pos_dot] + "^" + pipe + ".mp3";
                        File.Move(songPath, construction);

                    } else {
                        Debug.dprint("track title returns null");
                    }
                } else {
                    Debug.dprint("track returns null");
                }

                songPath = construction;
            }
            catch (Exception ex) { 
                Message.Data($"{Locale.OutsideItems.Error}: " + ex.Message +": "+ url
                , "Soundcloud Download Error"); // Todo aDD LOCALE
                songPath = "";
            }
        }

        public static async Task GetPlaylist(string url) {

            var soundcloud = new SoundCloudClient();

            // Get all playlist tracks
            var playlist = await soundcloud.Playlists.GetAsync(url, true);

            if (playlist.Tracks.Count() == 0 || playlist.Tracks == null) {
                Console.WriteLine(Locale.OutsideItems.NoTrackPlaylist);
                Console.ReadLine();
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
            Console.WriteLine(playlist[0]);
            if (playlist.Count() == 0 || playlist == null) {
                Console.WriteLine(Locale.OutsideItems.NoTrackPlaylist);
                Console.ReadLine();
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


        public static (string,string) GetSongsFromPlaylist(string url, string service) {
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
            // Console.WriteLine("Formatting url for filename: " + url);
            if (URL.isValidSoundCloudPlaylist(url)) {
                return "Soundcloud Playlist";
            }
            else if (URL.IsValidSoundcloudSong(url))
            {
                // Console.WriteLine("Soundcloud song");
                // remove ? and everything after
                int index = url.IndexOf("?");
                if (index > 0)
                {
                    url = url.Substring(0, index);
                }

                string formattedSCUrl = url.Replace("https://", "")
                                     .Replace("/", " ")
                                     .Replace("-", " ")
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
                                     .Replace("-", " ")
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
