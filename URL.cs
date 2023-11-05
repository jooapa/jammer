
using System;
using System.IO;
using System.Text.RegularExpressions;
using SoundCloudExplode;
using jammer;

namespace jammer {
    internal class URL {

        static string jammerPath = "";
        static SoundCloudClient soundcloud = new SoundCloudClient();
        static string url = "";
        static public string CheckIfURL(string url2) {

            url = url2;

            if (IsSoundCloudUrlValid()) {
                Console.WriteLine("Valid SoundCloud URL");
                CheckJammerFolder.CheckJammerFolderExists();

                DownloadSoundCloudTrackAsync(url).Wait();
            } else {
                Console.WriteLine("Invalid SoundCloud URL");
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

        static bool IsSoundCloudUrlValid()
        {
            // Use a regular expression pattern to validate SoundCloud URLs www.soundcloud.com/username/track-name
            string pattern = @"^(https?:\/\/)?(www\.)?(soundcloud\.com|snd\.sc)\/(.*)$";

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            
            if (regex.IsMatch(url))
            {
                return true;
            }
            else
            {
                url = "https://" + url;
                Console.WriteLine("new URL: " + url);
                return regex.IsMatch(url);
            }
        }
    }
}