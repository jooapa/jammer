using SoundCloudExplode;
using YoutubeExplode;
using YoutubeExplode.Common;
using Spectre.Console;
using SpotifyExplode;
using System.IO;
using TagLib;
using System.Net;
using System.Text.RegularExpressions;

namespace Jammer {
    public class Download {
        public static string songPath = "";
        static SoundCloudClient soundcloud = new SoundCloudClient();
        static SpotifyClient spotify = new SpotifyClient();
        static string[] playlistSongs = { "" };
        public static readonly YoutubeClient youtube = new();

        public static string DownloadSong(string url) {
            songPath = "";

            Debug.dprint($"{Locale.OutsideItems.Downloading}: " + url.ToString());
            if (URL.IsValidSoundcloudSong(url)) {
                DownloadSoundCloudTrackAsync(url).Wait();
            } 
            else if (URL.IsValidYoutubeSong(url)) {
                DownloadYoutubeTrackAsync(url).Wait();
            } 
            else if(URL.IsValidSpotifySong(url)){
                DownloadSpotifyTrackAsync(url).Wait();
            } 
            else if (URL.IsUrl(url)) {
                if (url.EndsWith(".jammer")) {
                    DownloadJammerFile(url).Wait();
                }
                else {
                    GeneralDownload(url).Wait();
                }
            }
            else if (URL.IsUrl(url)) {
                if (url.EndsWith(".jammer")) {
                    DownloadJammerFile(url).Wait();
                }
                else {
                    GeneralDownload(url).Wait();
                }
            } else {
                Console.WriteLine(Locale.OutsideItems.InvalidUrl);
                Debug.dprint("Invalid url");
            }

            Start.drawWhole = true;
            return songPath;
        }

        private static async Task GeneralDownload(string url) {
            string formattedUrl = FormatUrlForFilename(url, true);
            songPath = Path.Combine(
                Preferences.songsPath,
                formattedUrl
            );
            Utils.memory_for_songPath = songPath;
            AnsiConsole.MarkupLine($"{Locale.OutsideItems.Downloading}: {url} to {songPath}");
            if (System.IO.File.Exists(songPath)) {
                return;
            }
            
            try {
                using (var httpClient = new HttpClient()) {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
                    httpClient.Timeout = TimeSpan.FromMinutes(10);

                    var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(songPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long totalBytes = response.Content.Headers.ContentLength ?? -1;

                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;

                            if (totalBytes > 0)
                            {
                                double progressPercentage = (double)totalBytesRead / totalBytes * 100;
                                Console.WriteLine($"{Locale.OutsideItems.Downloaded} {totalBytesRead} {Locale.OutsideItems.Of} {totalBytes} {Locale.OutsideItems.Bytes} ({progressPercentage:P}).");
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                AnsiConsole.MarkupLine($"{Locale.OutsideItems.ErrorDownload}" + ex.Message);
            }
        }

        private static string GetDownloadedJammerFileName(string url) {
            string downloadedPath = Path.Combine(Utils.JammerPath, "playlists", url);
            return downloadedPath.LastIndexOf("/") > 0 ? downloadedPath.Substring(downloadedPath.LastIndexOf("/") + 1) : downloadedPath;
        }  

        private static async Task DownloadJammerFile(string url) {
            string downloadedPath = Path.Combine(Utils.JammerPath, "playlists", GetDownloadedJammerFileName(url));

            if (System.IO.File.Exists(downloadedPath)) {
                string input = Message.Input($"Playlist of same name already exists. Overwrite? (y/n)", "Warning");
                if (input != "y") {
                    songPath = downloadedPath;
                    return;
                }
            }

            try {
                using (var httpClient = new HttpClient()) {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
                    httpClient.Timeout = TimeSpan.FromMinutes(10);

                    var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(downloadedPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long totalBytes = response.Content.Headers.ContentLength ?? -1;

                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;

                            if (totalBytes > 0)
                            {
                                double progressPercentage = (double)totalBytesRead / totalBytes * 100;
                                Console.WriteLine($"{Locale.OutsideItems.Downloaded} {totalBytesRead} {Locale.OutsideItems.Of} {totalBytes} {Locale.OutsideItems.Bytes} ({progressPercentage:P}).");
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                AnsiConsole.MarkupLine($"{Locale.OutsideItems.ErrorDownload} " + ex.Message, "Error");
            }

            songPath = Path.Combine(Utils.JammerPath, "playlists", GetDownloadedJammerFileName(url));
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
                        AnsiConsole.Clear();
                        Console.WriteLine($"{Locale.OutsideItems.Downloading} {url}: {data:P}");
                    });

                    await youtube.Videos.Streams.DownloadAsync(streamInfo, songPath, progress);

                    // TagLib
                    var file = TagLib.File.Create(songPath);
                    file.Tag.Title = Start.Sanitize(video.Title);
                    file.Tag.Performers = new string[] { video.Author.ChannelTitle };
                    file.Tag.Album = video.Author.ChannelTitle;
                    file.Save();
                }
                else
                {
                    Jammer.Message.Data(Locale.OutsideItems.NoAudioStream, Locale.OutsideItems.Error);
                }
            }
            catch (Exception ex)
            {
                Jammer.Message.Data($"{Locale.OutsideItems.Error}: " + ex.Message, "Error");
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
                            AnsiConsole.Clear();
                            Console.WriteLine($"{Locale.OutsideItems.Downloading} {url}: {data:P} to {songPath}"); //TODO ADD LOCALE
                        });
                        
                        await soundcloud.DownloadAsync(track, songPath, progress);

                        var file = TagLib.File.Create(songPath);
                        file.Tag.Title = Start.Sanitize(track.Title);
                        file.Tag.Description = track.Description;
                        if (track.User != null && track.User.Username != null) {
                            file.Tag.Performers = new string[] { track.User.Username };
                        }           
                        file.Save();
                        if (track.ArtworkUrl != null)
                            await DownloadThumbnailAsync(track.ArtworkUrl, songPath);

                    } else {
                        Debug.dprint("track title returns null");
                    }
                } else {
                    Debug.dprint("track returns null");
                }

            }
            catch (Exception ex) { 
                Jammer.Message.Data($"{Locale.OutsideItems.Error}: " + ex.Message +": "+ url
                , Locale.OutsideItems.DownloadErrorSoundcloud);
                songPath = "";
            }
        }

        static async Task DownloadThumbnailAsync(Uri imageUrl, string songPath)
        {
            var file = TagLib.File.Create(songPath);
            WebClient webClient = new WebClient();
            byte[] imageBytes = webClient.DownloadData(imageUrl);            
            Picture picture = new Picture(imageBytes);  
            file.Tag.Pictures = Array.Empty<IPicture>();
            file.Tag.Pictures = new IPicture[] { picture };
            file.Save();
        }

        public static async Task GetPlaylist(string plurl) {

            var soundcloud = new SoundCloudClient();

            // Get all playlist tracks
            var playlist = await soundcloud.Playlists.GetAsync(plurl, true);

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

            // debug
            foreach (var song in playlistSongs) {
                Console.WriteLine(song);
            }
        }
        public static async Task GetPlaylistYoutube(string plurl) {
            // Get all playlist tracks
            var playlist = await youtube.Playlists.GetVideosAsync(plurl);
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

        private static async Task DownloadSpotifyTrackAsync(string url){
            string formattedUrl = FormatUrlForFilename(url);
            songPath = Path.Combine(
                Preferences.songsPath,
                formattedUrl
            );
            if (System.IO.File.Exists(songPath)) {
                return;
            }
            string? downloadUrl = await spotify.Tracks.GetDownloadUrlAsync(url);
            if(downloadUrl == null){
                AnsiConsole.MarkupLine($"{Locale.OutsideItems.Error}: {Locale.OutsideItems.NoAudioStream}", "Error");
                return;
            }
            try
            {
                // var streamManifest = await spotify.Tracks.;
                // var streamInfo = streamManifest.GetAudioStreams().FirstOrDefault();
                // var video = await youtube.Videos.GetAsync(url);
                
                string newPath = Path.Combine(Preferences.songsPath, formattedUrl);
                if (System.IO.File.Exists(newPath)) {
                    return;
                }
                AnsiConsole.MarkupLine($"{Locale.OutsideItems.Downloading}: {url} to {newPath}");
                GeneralDownload(downloadUrl).Wait();
                formattedUrl += ".mp3";
                songPath = newPath;
                System.IO.File.Move(Utils.memory_for_songPath, newPath);
                // if (streamInfo != null)
                // {
                    // var progress = new Progress<double>(data =>
                    // {
                    //     AnsiConsole.Clear();
                    //     Console.WriteLine($"{Locale.OutsideItems.Downloading} {url}: {data:P}");
                    // });

                    // await youtube.Videos.Streams.DownloadAsync(streamInfo, songPath, progress);

                    // TagLib
                    // var file = TagLib.File.Create(songPath);
                    // file.Tag.Title = Start.Sanitize(video.Title);
                    // file.Tag.Performers = new string[] { video.Author.ChannelTitle };
                    // file.Tag.Album = video.Author.ChannelTitle;
                    // file.Save();
                // }
                // else
                // {
                //     Jammer.Message.Data(Locale.OutsideItems.NoAudioStream, Locale.OutsideItems.Error);
                // }
            }
            catch (Exception ex)
            {
                Jammer.Message.Data($"{Locale.OutsideItems.Error}: " + ex.Message, "Error");
                songPath = "";
            }
        }
        public static async Task GetPlaylistSpotify(string plurl){
            Console.WriteLine("Getting playlist: " + plurl);
            var playlist = await spotify.Playlists.GetAsync(plurl);
            List<SpotifyExplode.Tracks.Track> tracks = await spotify.Playlists.GetTracksAsync(plurl);

            if (tracks.Count == 0 || tracks == null) {
                Console.WriteLine(Locale.OutsideItems.NoTrackPlaylist);
                Console.ReadLine();
                return;
            }

            // add all tracks permalinkUrl to songs array
            playlistSongs = new string[tracks.Count];
            int i = 0;
            foreach (SpotifyExplode.Tracks.Track track in tracks) {
                var _url = track.Url?.ToString() ?? string.Empty;
                var index = _url.IndexOf('?');
                if (index != -1) {
                    _url = _url.Substring(0, index);
                }
                playlistSongs[i] = _url;
                i++;
            }
        }

        public static string GetSongsFromPlaylist(string plurl, string service) {
            if(service == "soundcloud"){
                GetPlaylist(plurl).Wait();
            }
            else if( service == "youtube"){
                GetPlaylistYoutube(plurl).Wait();
            }
            else if (service == "spotify") {
                GetPlaylistSpotify(plurl).Wait();
            }


            // remove the CurrentSong from Utils.songs
            Utils.songs = Utils.songs.Where(val => val != Utils.songs[Utils.currentSongIndex]).ToArray();

            // Message.Data("current song: " + Utils.songs[Utils.currentSongIndex], "Debug");
            // Message.Data("playlist first song: " + playlistSongs[0], "Debug");
            // Message.Data("last song: " + Utils.songs[Utils.songs.Length - 1], "Debug");

            // add all songs from playlist to Utils.songs but start adding at the currentSongIndex
            Utils.songs = Utils.songs.Take(Utils.currentSongIndex).Concat(playlistSongs).Concat(Utils.songs.Skip(Utils.currentSongIndex)).ToArray();
            // Message.Data(Utils.songs[0], "d1");
            
            // delete duplicate songs
            Utils.songs = Utils.songs.Distinct().ToArray();
            // Message.Data(Utils.songs[0], "d2");
            

            return DownloadSong(Utils.songs[Utils.currentSongIndex]);
        }

        public static string FormatUrlForFilename(string url, bool isCheck = false)
        {
            if (URL.isValidSoundCloudPlaylist(url)) {
                return "Soundcloud Playlist";
            }
            else if (URL.IsValidSpotifyPlaylist(url)){
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
            else if (URL.IsValidSpotifySong(url)){
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
            else if (URL.IsValidSpotifyAlbum(url)){
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
            else if (URL.IsValidSpotifyMate(url)){
                return "Spotify Mate____TEMP____DELETE____ME____AFTER____DOWNLOAD____";
            }
            else if (URL.IsValidSpotifyArtist(url)){
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
            } else {
                // remove every thing that cant be in filename
                url = url.Replace("https://", "")
                            .Replace("/", " ")
                            .Replace("?", " ");
                return url;
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
