using System.Text;
using m3uParser;

namespace Jammer
{
    public static class M3u
    {
        private static readonly string[] separator = new string[] { "\n", "\r\n" };

        /// <summary>
        /// Adjusts the m3u file to work with this parser.
        /// if line starts with #EXTINF, then the next not empty line is the song URI or path
        /// if the line does not start with #EXTINF, then it's the song URI or path and add #EXTINF:0, to the previous line
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string AdjustM3uFileToWorkWithThisParser(string content)
        {
            // Split the content by line endings
            var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Initialize a StringBuilder to hold the modified content
            var modifiedContent = new StringBuilder();

            // Flag to indicate if the previous line was an EXTINF line
            bool previousIsExtinf = false;

            foreach (var line in lines)
            {
                // Check if the current line is an EXTINF line
                if (line.StartsWith('#')) // #EXT-X is for m3u8 files
                {
                    // Append the current line as it is
                    modifiedContent.AppendLine(line);
                    previousIsExtinf = true;
                }
                else
                {
                    // If the previous line was not EXTINF, add a default EXTINF line
                    if (!previousIsExtinf)
                    {
                        modifiedContent.AppendLine("#EXTINF:0,");
                    }
                    // Append the current line
                    modifiedContent.AppendLine(line);
                    previousIsExtinf = false;
                }
            }

            return modifiedContent.ToString();
        }
        public static string[] ParseM3u(string path)
        {
            var content = System.IO.File.ReadAllText(path);
            // if first line is not #EXTM3U, then it's not a m3u file
            if (!content.StartsWith("#EXTM3U"))
            {
                var jammerFileSongs = content.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < jammerFileSongs.Length; i++)
                {
                    jammerFileSongs[i] = jammerFileSongs[i].Trim();
                }


                Utils.CurrentPlaylist = path;

                return jammerFileSongs;
            }

            string AdjContent = AdjustM3uFileToWorkWithThisParser(content);
            // Message.Data(AdjContent, "ParseM3u");

            var contentM3u = M3U.Parse(AdjContent);
            var songs = contentM3u.Medias.Select(x => x.MediaFile).ToArray();
            var songsTitles = contentM3u.Medias.Select(x => x.Title.InnerTitle).ToArray();
            var songsDuration = contentM3u.Medias.Select(x => x.Duration.ToString()).ToArray();

            string[] jammerSongs = new string[songs.Length];
            // turn to jammer file format
            for (int i = 0; i < songs.Length; i++)
            {
                // Message.Data($"Song {i + 1} of {songs.Length}", "ParseM3u");
                if (songsTitles[i] == null || songsTitles[i].DefaultIfEmpty().All(char.IsWhiteSpace))
                {
                    jammerSongs[i] = songs[i];
                    continue;
                }

                Song song = new()
                {
                    Title = songsTitles[i],
                    Duration = songsDuration[i]
                };
                song.ExtractSongDetails();
                song.URI = songs[i];

                jammerSongs[i] = SongExtensions.ToSongString(song);
                // Message.Data(jammerSongs[i], "ParseM3u");
            }

            Utils.CurrentPlaylist = path;

            return jammerSongs;
        }

    }
}