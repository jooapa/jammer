namespace Jammer {
    public static class Log {
        public static string[] log = Array.Empty<string>();
        private static void New(string txt, bool isErr = false) {
            var time = DateTime.Now.ToString("HH:mm:ss"); // case sensitive

            var curPlaylist = Playlists.GetJammerPlaylistVisualPath(Utils.currentPlaylist);
            if (curPlaylist == "") {
                curPlaylist = "No playlist";
            }

            if (isErr) {
                log = log.Append("[red]"+time +"[/]"+ ";ERROR;[cyan]" + curPlaylist + "[/]: " + txt).ToArray();
                return;
            }
            log = log.Append("[green3_1]"+time +"[/]"+ ";INFO;[cyan]" + curPlaylist + "[/]: " + txt).ToArray();   
        }

        public static void Info(string txt) {
            New(txt);
        }

        public static void Error(string txt) {
            New(txt, true);
        }

        public static string GetLog() {
            return string.Join("\n", log);
        }
    }
}