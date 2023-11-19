
using SoundCloudExplode;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace jammer {
    internal class URL {

        static public string jammerPath = "";
        static SoundCloudClient soundcloud = new SoundCloudClient();
        static string url = "";
        static public string[] songs = { "" };
        static public string CheckIfURL(string url2) {

            url = url2;

            if (IsSoundCloudUrlValid(url)) {
                
                string playlistPattern = @"^https?:\/\/(?:www\.)?soundcloud\.com\/[^\/]+\/sets\/[^\/]+$";
                Regex playlistRegex = new Regex(playlistPattern, RegexOptions.IgnoreCase);
                if (playlistRegex.IsMatch(url)) {
                    GetPlaylist(url).Wait();
                    return jammerPath;
                }
                int index = url.IndexOf("?");
                if (index > 0)
                {
                    url = url.Substring(0, index);
                }

                DownloadSoundCloudTrackAsync(url).Wait();
            } else {
                return url2;
            }   
            
            return jammerPath;
        }

        static public async Task DownloadSoundCloudTrackAsync(string url) {

            // if already downloaded, don't download again
            string oldUrl = url;
            url = url.Replace("https://", "");
            url = url.Replace("/", " ");
            url = url.Replace("-", " ");
            url = url + ".mp3";
            jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\jammer\\" + url;
            if (File.Exists(jammerPath)) {
                Console.WriteLine("File already exists");
                return;
            }
            url = oldUrl;

            var track = await soundcloud.Tracks.GetAsync(url);

            // track name split / by spaces'
            
            var trackName = "soundcloud.com " + url.Split('/')[3] + " " + url.Split('/')[4];
            trackName = trackName.Replace("-", " ");
            trackName = trackName.Replace("/", " ");
            trackName.Replace("https://", "");
            jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\jammer\\" + trackName.ToLower() + ".mp3";

            await soundcloud.DownloadAsync(track, jammerPath);
        }

        static public async Task GetPlaylist(string url) {

            var soundcloud = new SoundCloudClient();
            
            // Get all playlist tracks
            var playlist = await soundcloud.Playlists.GetAsync(url, true);

            if (playlist.Tracks.Count() == 0 || playlist.Tracks == null) {
                Console.WriteLine("No tracks in playlist");
                Console.ReadLine();
                return;
            }

            // add all tracks permalinkUrl to songs array
            songs = new string[playlist.Tracks.Count()];
            int i = 0;
            foreach (var track in playlist.Tracks) {
                songs[i] = track.PermalinkUrl?.ToString() ?? string.Empty;
                i++;
            }

            // Console.WriteLine(string.Join(Environment.NewLine, songs));
            // Console.ReadLine();
            jammerPath = "Soundcloud Playlist";
        }


        static public bool IsSoundCloudUrlValid(string uri)
        {
            string pattern = @"^(https?:\/\/)?(www\.)?(soundcloud\.com|snd\.sc)\/(.*)$";

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return regex.IsMatch(uri);
        }

        static public bool isValidSoundCloudPLaylist(string uri) {
            string pattern = @"^https?:\/\/(?:www\.)?soundcloud\.com\/[^\/]+\/sets\/[^\/]+$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(uri);
        }
    }
}