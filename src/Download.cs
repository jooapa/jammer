
using SoundCloudExplode;
using Spectre.Console;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace jammer {
    internal class Download {
        public static string songPath = "";
        static SoundCloudClient soundcloud = new SoundCloudClient();
        static string url = "";
        static string[] playlistSongs = { "" };

        public static string DownloadSong(string url2) {
            url = url2;
            if (URL.IsValidSoundcloudSong(url)) {
                DownloadSoundCloudTrackAsync(url).Wait();
            } else if (URL.IsValidYoutubeSong(url)) {
                DownloadYoutubeTrackAsync(url).Wait();
            } else {
                Console.WriteLine("Invalid url");
            }
            return songPath;
        }

        private static async Task DownloadYoutubeTrackAsync(string urlGetDownloadUrlAsync)
        {
            string formattedUrl = FormatUrlForFilename(url);

            songPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "jammer", 
                formattedUrl
            );

            if (File.Exists(songPath))
            {
                // Console.WriteLine("Youtube file already exists");
                return;
            }
            try
            {
                var youtube = new YoutubeClient();
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                var streamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                if (streamInfo != null)
                {
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, songPath);
                }
                else
                {
                    // Console.WriteLine("This video has no audio streams");
                }

                songPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\jammer\\" + formattedUrl;
                // Console.WriteLine("Downloaded: " + formattedUrl + " to " + songPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public static async Task DownloadSoundCloudTrackAsync(string url) {
            // if already downloaded, don't download again
            string formattedUrl = FormatUrlForFilename(url);
            string oldUrl = url;
            songPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "jammer",
                formattedUrl
            );

            if (File.Exists(songPath))
            {
                // Console.WriteLine("Youtube file already exists");
                return;
            }
            url = oldUrl;
            var track = await soundcloud.Tracks.GetAsync(url);
            if (track != null) {
                var trackName = formattedUrl;
                songPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\jammer\\" + trackName;

                await soundcloud.DownloadAsync(track, songPath);
            } else {

                Console.WriteLine("NULL");
            }
        }

        public static async Task GetPlaylist(string url) {

            var soundcloud = new SoundCloudClient();
            
            // Get all playlist tracks
            var playlist = await soundcloud.Playlists.GetAsync(url, true);

            if (playlist.Tracks.Count() == 0 || playlist.Tracks == null) {
                Console.WriteLine("No tracks in playlist");
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

        public static string GetSongsFromPlaylist(string url) {
            GetPlaylist(url).Wait();

            // remove the CurrentSong from Utils.songs
            Utils.songs = Utils.songs.Where(val => val != Utils.songs[Utils.currentSongIndex]).ToArray();
            // add all songs from playlist to Utils.songs but start adding at the currentSongIndex
            Utils.songs = Utils.songs.Take(Utils.currentSongIndex).Concat(playlistSongs).Concat(Utils.songs.Skip(Utils.currentSongIndex)).ToArray();
            // delete duplicate songs
            Utils.songs = Utils.songs.Distinct().ToArray();
            
            return DownloadSong(Utils.songs[Utils.currentSongIndex]);
        }

        public static string FormatUrlForFilename(string url)
        {
            // Console.WriteLine("Formatting url for filename: " + url);
            if (URL.isValidSoundCloudPlaylist(url)) {
                Console.WriteLine("Soundcloud playlist");
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
            }
            else if (URL.IsValidYoutubeSong(url))
            {
                Console.WriteLine("Youtube song");
                int index = url.IndexOf("&");
                if (index > 0)
                {
                    url = url.Substring(0, index);
                }
                url.Replace("?", " ");
            }
            string formattedUrl = url.Replace("https://", "")
                                     .Replace("/", " ")
                                     .Replace("-", " ");

            return formattedUrl + ".mp3";
        }

        static public bool GetDownloadUrlAsyncIsUrl(string input)
        {
            if (input == null)
            {
                return false;
            }
            // detect if input is url using regex
            string pattern = @"^(https?:\/\/)?(www\.)?(soundcloud\.com|snd\.sc)\/(.*)$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            if (regex.IsMatch(input))
            {
                return true;
            }
            else
            {
                // detect youtbe url
                string pattern2 = @"^(https?:\/\/)?(www\.)?(youtube\.com|youtu\.be)\/(.*)$";
                Regex regex2 = new Regex(pattern2, RegexOptions.IgnoreCase);
                if (regex2.IsMatch(input))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}