using SoundCloudExplode;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using Spectre.Console;
using System.Net;

namespace jammer {
    internal class Download {
        public static string songPath = "";
        static SoundCloudClient soundcloud = new SoundCloudClient();
        static string url = "";
        static string[] playlistSongs = { "" };

        public static string DownloadSong(string url2) {
            url = url2;
            Debug.dprint("Downloading: " + url2.ToString());
            if (URL.IsValidSoundcloudSong(url)) {
                DownloadSoundCloudTrackAsync(url).Wait();
            } else if (URL.IsValidYoutubeSong(url)) {
                DownloadYoutubeTrackAsync(url).Wait();
            } else {
                Console.WriteLine("Invalid url");
                Debug.dprint("Invalid url");
            }
            return songPath;
        }

        private static async Task DownloadYoutubeTrackAsync(string url)
        {
            string formattedUrl = FormatUrlForFilename(url);

            songPath = Path.Combine(
                Utils.jammerPath,
                formattedUrl
            );

            if (File.Exists(songPath))
            {
                Console.WriteLine("Youtube file already exists");
                return;
            }
            try
            {
                var youtube = new YoutubeClient();
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                var streamInfo = streamManifest.GetAudioStreams().FirstOrDefault();
                if (streamInfo != null)
                {
                    var progress = new Progress<double>(data =>
                    {
                        AnsiConsole.Clear();
                        Console.WriteLine($"Downloading {url}: {data:P}");
                    });

                    await youtube.Videos.Streams.DownloadAsync(streamInfo, songPath, progress);
                }
                else
                {
                    Console.WriteLine("This video has no audio streams");
                }

                songPath = Path.Combine(
                    Utils.jammerPath,
                    formattedUrl
                );
                Console.WriteLine("Downloaded: " + formattedUrl + " to " + songPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.ReadLine();
            }
        }

        public static async Task DownloadSoundCloudTrackAsync(string url) {
            // if already downloaded, don't download again
            string formattedUrl = FormatUrlForFilename(url);
            string oldUrl = url;
            songPath = Path.Combine(
                Utils.jammerPath,
                formattedUrl
            );

            if (File.Exists(songPath))
            {
                return;
            }
            url = oldUrl;
            try {
                var track = await soundcloud.Tracks.GetAsync(url);
                if (track != null) {
                    var trackName = formattedUrl;
                    songPath = Path.Combine(
                        Utils.jammerPath,
                        formattedUrl
                    );

                    var progress = new Progress<double>(data => {
                        AnsiConsole.Clear();
                        Console.WriteLine($"Downloading {trackName}: {data:P}");
                    });

                    await soundcloud.DownloadAsync(track, songPath, progress);
                } else {
                    Debug.dprint("track returns null");
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Error: " + ex.Message);
                songPath = "";
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
                return formattedSCUrl + ".mp3";
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

            return formattedYTUrl + ".mp4";
        }
    }
}
