using System.Text.RegularExpressions;

namespace Jammer
{
    internal class URL
    {
        public static bool IsValidSoundcloudSong(string uri)
        {
            Regex regex = new Regex(Utils.scSongPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(uri);
        }

        public static bool isValidSoundCloudPlaylist(string uri)
        {
            Regex regex = new Regex(Utils.scPlaylistPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(uri);
        }

        public static bool IsValidYoutubePlaylist(string uri)
        {
            Regex regex = new Regex(Utils.ytPlaylistPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(uri);
        }
        public static bool IsValidYoutubeSong(string uri)
        {
            Regex regex = new Regex(Utils.ytSongPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(uri);
        }

        public static bool IsUrl(string uri)
        {
            return IsUrlHTTPS(uri) || IsUrlHTTP(uri);
        }


        public static bool IsUrlHTTPS(string uri)
        {
            Regex regex = new Regex(Utils.urlPatternHTTPS, RegexOptions.IgnoreCase);
            return regex.IsMatch(uri);
        }

        public static bool IsUrlHTTP(string uri)
        {
            Regex regex = new Regex(Utils.urlPatternHTTP, RegexOptions.IgnoreCase);
            return regex.IsMatch(uri);
        }
    }
}
