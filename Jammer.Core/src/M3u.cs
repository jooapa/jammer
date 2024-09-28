using m3uParser;

namespace Jammer {
    public static class M3u {
        private static readonly string[] separator = new string[] { "\n", "\r\n" };

        public static string[] ParseM3u(string path) {
            var content = System.IO.File.ReadAllText(path);
            // if first line is not #EXTM3U, then it's not a m3u file
            if (!content.StartsWith("#EXTM3U")) {
                var jammerFileSongs = content.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < jammerFileSongs.Length; i++) {
                    jammerFileSongs[i] = jammerFileSongs[i].Trim();
                }


                Utils.currentPlaylist = path;

                return jammerFileSongs;
            }

            var contentM3u = M3U.Parse(content);
            var songs = contentM3u.Medias.Select(x => x.MediaFile).ToArray();
            var songsTitles = contentM3u.Medias.Select(x => x.Title.InnerTitle).ToArray();
            var songsDuration = contentM3u.Medias.Select(x => x.Duration.ToString()).ToArray();
            
            string[] jammerSongs = new string[songs.Length];
            // turn to jammer file format
            for (int i = 0; i < songs.Length; i++) {
                if (songsTitles[i] == null || songsTitles[i] == "") {
                    jammerSongs[i] = songs[i];
                    continue;
                }

                Song song = new() {
                    Title = songsTitles[i],
                    Duration = songsDuration[i]
                };
                song.ExtractSongDetails();
                song.URI = songs[i];

                jammerSongs[i] = SongExtensions.ToSongString(song);
                // Message.Data(jammerSongs[i], "ParseM3u");
            }

            Utils.currentPlaylist = path;

            return jammerSongs;
        }

    }
}