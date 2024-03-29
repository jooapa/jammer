namespace jammer{
    public static class Keybindings{
        public static string PlayPause = CheckValue("Keybinds", "PlayPause", "Spacebar");
        public static string CurrentState = CheckValue("Keybinds", "CurrentState", "F12");
        public static string Quit = CheckValue("Keybinds", "Quit", "Q");
        public static string NextSong = CheckValue("Keybinds", "NextSong", "N");
        public static string PreviousSong = CheckValue("Keybinds", "PreviousSong", "P");
        public static string PlaySong = CheckValue("Keybinds", "PlaySong", "Shift + p");
        public static string Forward5s = CheckValue("Keybinds", "Forward5s", "RightArrow");
        public static string Backwards5s = CheckValue("Keybinds", "Backwards5s", "LeftArrow");
        public static string VolumeUp = CheckValue("Keybinds", "VolumeUp", "UpArrow");
        public static string VolumeDown = CheckValue("Keybinds", "VolumeDown", "DownArrow");
        public static string Shuffle = CheckValue("Keybinds", "Shuffle", "S");
        public static string SaveAsPlaylist = CheckValue("Keybinds", "SaveAsPlaylist", "Shift + Alt + S");
        public static string SaveCurrentPlaylist = CheckValue("Keybinds", "SaveCurrentPlaylist", "Shift + S");
        public static string ShufflePlaylist = CheckValue("Keybinds", "ShufflePlaylist", "Alt + S");
        public static string Loop = CheckValue("Keybinds", "Loop", "L");
        public static string Mute = CheckValue("Keybinds", "Mute", "M");
        public static string ShowHidePlaylist = CheckValue("Keybinds", "ShowHidePlaylist", "F");
        public static string ListAllPlaylists = CheckValue("Keybinds", "ListAllPlaylists", "Shift + F");
        public static string Help = CheckValue("Keybinds", "Help", "H");
        public static string Settings = CheckValue("Keybinds", "Settings", "C");
        public static string ToSongStart = CheckValue("Keybinds", "ToSongStart", "0");
        public static string ToSongEnd = CheckValue("Keybinds", "ToSongEnd", "9");
        public static string PlaylistOptions = CheckValue("Keybinds", "PlaylistOptions", "F2");
        public static string ForwardSecondAmount = CheckValue("Keybinds", "ForwardSecondAmount", "Ctrl + 1");
        public static string BackwardSecondAmount = CheckValue("Keybinds", "BackwardSecondAmount", "Ctrl + 2");
        public static string ChangeVolumeAmount = CheckValue("Keybinds", "ChangeVolumeAmount", "Ctrl + 3");
        public static string Autosave = CheckValue("Keybinds", "Autosave", "Ctrl + 4");
        public static string CommandHelpScreen = CheckValue("Keybinds", "CommandHelpScreen", "Tab");
        public static string DeleteCurrentSong = CheckValue("Keybinds", "DeleteCurrentSong", "Delete");
        public static string AddSongToPlaylist = CheckValue("Keybinds", "AddSongToPlaylist", "Shift + A");
        public static string ShowSongsInPlaylists = CheckValue("Keybinds", "ShowSongsInPlaylists", "Shift + D");
        public static string PlayOtherPlaylist = CheckValue("Keybinds", "PlayOtherPlaylist", "Shift + O");
        public static string RedownloadCurrentSong = CheckValue("Keybinds", "RedownloadCurrentSong", "Shift + B");
        public static string EditKeybindings = CheckValue("Keybinds", "EditKeybindings", "Shift + E");
        public static string ChangeLanguage = CheckValue("Keybinds", "ChangeLanguage", "Shift + L");
        public static string PlayRandomSong = CheckValue("Keybinds", "PlayRandomSong", "R");

        public static string CheckValue(string key, string value, string defaultValue){
            string finalValue = ReadWriteFile.ReadIni_KeyData(key, value);
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