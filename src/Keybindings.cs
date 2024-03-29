namespace jammer{
    public static class Keybindings{
        public static string ToMainMenu = CheckValue("ToMainMenu", "Escape");
        public static string PlayPause = CheckValue("PlayPause", "Spacebar");
        public static string CurrentState = CheckValue("CurrentState", "F12");
        public static string Quit = CheckValue("Quit", "Q");
        public static string NextSong = CheckValue("NextSong", "N");
        public static string PreviousSong = CheckValue("PreviousSong", "P");
        public static string PlaySong = CheckValue("PlaySong", "Shift + p");
        public static string Forward5s = CheckValue("Forward5s", "RightArrow");
        public static string Backwards5s = CheckValue("Backwards5s", "LeftArrow");
        public static string VolumeUp = CheckValue("VolumeUp", "UpArrow");
        public static string VolumeDown = CheckValue("VolumeDown", "DownArrow");
        public static string Shuffle = CheckValue("Shuffle", "S");
        public static string SaveAsPlaylist = CheckValue("SaveAsPlaylist", "Shift + Alt + S");
        public static string SaveCurrentPlaylist = CheckValue("SaveCurrentPlaylist", "Shift + S");
        public static string ShufflePlaylist = CheckValue("ShufflePlaylist", "Alt + S");
        public static string Loop = CheckValue("Loop", "L");
        public static string Mute = CheckValue("Mute", "M");
        public static string ShowHidePlaylist = CheckValue("ShowHidePlaylist", "F");
        public static string ListAllPlaylists = CheckValue("ListAllPlaylists", "Shift + F");
        public static string Help = CheckValue("Help", "H");
        public static string Settings = CheckValue("Settings", "C");
        public static string ToSongStart = CheckValue("ToSongStart", "0");
        public static string ToSongEnd = CheckValue("ToSongEnd", "9");
        public static string PlaylistOptions = CheckValue("PlaylistOptions", "F2");
        public static string ForwardSecondAmount = CheckValue("ForwardSecondAmount", "1");
        public static string BackwardSecondAmount = CheckValue("BackwardSecondAmount", "2");
        public static string ChangeVolumeAmount = CheckValue("ChangeVolumeAmount", "3");
        public static string Autosave = CheckValue("Autosave", "4");
        public static string CommandHelpScreen = CheckValue("CommandHelpScreen", "Tab");
        public static string DeleteCurrentSong = CheckValue("DeleteCurrentSong", "Delete");
        public static string AddSongToPlaylist = CheckValue("AddSongToPlaylist", "Shift + A");
        public static string ShowSongsInPlaylists = CheckValue("ShowSongsInPlaylists", "Shift + D");
        public static string PlayOtherPlaylist = CheckValue("PlayOtherPlaylist", "Shift + O");
        public static string RedownloadCurrentSong = CheckValue("RedownloadCurrentSong", "Shift + B");
        public static string EditKeybindings = CheckValue("EditKeybindings", "Shift + E");
        public static string ChangeLanguage = CheckValue("ChangeLanguage", "Shift + L");
        public static string PlayRandomSong = CheckValue("PlayRandomSong", "R");

        public static string CheckValue(string value, string defaultValue){
            string finalValue = IniFileHandling.ReadIni_KeyData("Keybinds", value);
            if (finalValue == null || finalValue.Equals(""))
            {
                return defaultValue;
            }
            else
            {
                return finalValue;
            }
        }
    }
}