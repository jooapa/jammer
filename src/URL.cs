
using SoundCloudExplode;
using System.Text.RegularExpressions;

namespace jammer {
    internal class URL {

        static string jammerPath = "";
        static SoundCloudClient soundcloud = new SoundCloudClient();
        static string url = "";
        static public string CheckIfURL(string url2) {

            url = url2;

            if (IsSoundCloudUrlValid(url)) {

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

        static public bool IsSoundCloudUrlValid(string uri)
        {
            string pattern = @"^(https?:\/\/)?(www\.)?(soundcloud\.com|snd\.sc)\/(.*)$";

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return regex.IsMatch(uri);
        }
    }
}