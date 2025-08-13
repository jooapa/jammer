namespace Jammer
{
    public static class Locale
    {
        /*
        Adding new Locale line:
        1. Create a new class or add to an existing class. Classname is the name for the keyarea

        2. Create a new public static string variable with a new key name unique for the class, for example: RefreshScreenErrorMessage1
        ```
        public static string KeyName = CheckValueLocale("KeyArea", "Keyvalue", "Default text");
        public static string RefreshScreenErrorMessage1 = CheckValueLocale("OutsideItems", "RefreshScreenErrorMessage1", "Error refreshing the screen");
        ```

        3. Call the variable to display the text
        ```
        Jammer.Message.Data(Locale.OutsideItems.KeyName, "Error title here");
        Jammer.Message.Data(Locale.OutsideItems.RefreshScreenErrorMessage1, "Error title here");
        ```

        4. Add new locale line to en.ini file in its corresponding keyarea or create a new one for it
        ```
        [KeyArea]
        KeyName = Default text

        [OutsideItems]
        RefreshScreenErrorMessage1 = Error refreshing the screen
        ```
        */
        public static class Country
        {
            public static string _Country = CheckValueLocale("Country", "Country", "United kingdom");
            public static string Language = CheckValueLocale("Country", "Language", "English");
            public static string CountryCode = CheckValueLocale("Country", "CountryCode", "GB");
        }
        public static class Player
        {
            public static string Playlist = CheckValueLocale("Player", "Playlist", "playlist");
            public static string State = CheckValueLocale("Player", "State", "State");
            public static string Looping = CheckValueLocale("Player", "Looping", "Looping");
            public static string Shuffle = CheckValueLocale("Player", "Shuffle", "Shuffle");
            public static string Volume = CheckValueLocale("Player", "Volume", "Volume");
            public static string InvalidSong = CheckValueLocale("Player", "InvalidSong", "Invalid song");
            public static string ValidSong = CheckValueLocale("Player", "ValidSong", "Valid song");
            public static string Previos = CheckValueLocale("Player", "Previos", "previous");
            public static string Current = CheckValueLocale("Player", "Current", "current");
            public static string Next = CheckValueLocale("Player", "Next", "next");
            public static string NoSongsInPlaylist = CheckValueLocale("Player", "NoSongsInPlaylist", "No songs in playlist");
            public static string DrawingError = CheckValueLocale("Player", "DrawingError", "Error occured while drawing the UI");
            public static string ControlsWillWork = CheckValueLocale("Player", "ControlsWillWork", "Controls still work");
            public static string ForHelp = CheckValueLocale("Player", "ForHelp", "for help");
            public static string ForPlaylist = CheckValueLocale("Player", "ForPlaylist", "for playlist");
            public static string PlaySingleSongMessage1 = CheckValueLocale("Player", "PlaySingleSongMessage1", "Enter song(s) to play:");
            public static string PlaySingleSongMessage2 = CheckValueLocale("Player", "PlaySingleSongMessage2", "Play song(s) | Separate songs with space");
            public static string PlaySingleSongError1 = CheckValueLocale("Player", "PlaySingleSongError1", "Error in: playing songs");
            public static string PlaySingleSongError2 = CheckValueLocale("Player", "PlaySingleSongError2", "no song(s) given");
            public static string GotoSongInPlaylistMessage1 = CheckValueLocale("Player", "GotoSongInPlaylistMessage1", "Enter song to goto:");
            public static string GotoSongInPlaylistMessage2 = CheckValueLocale("Player", "GotoSongInPlaylistMessage2", "Goto song in playlist");
            public static string GotoSongInPlaylistError1 = CheckValueLocale("Player", "GotoSongInPlaylistError1", "Error: Goto song in playlist");
            public static string GotoSongInPlaylistError2 = CheckValueLocale("Player", "GotoSongInPlaylistError2", "no song given");
            public static string SaveAsPlaylistMessage1 = CheckValueLocale("Player", "SaveAsPlaylistMessage1", "Enter playlist name:");
            public static string SaveAsPlaylistMessage2 = CheckValueLocale("Player", "SaveAsPlaylistMessage2", "Save as playlist");
            public static string SaveAsPlaylistError1 = CheckValueLocale("Player", "SaveAsPlaylistError1", "Error: Save as playlist");
            public static string SaveAsPlaylistError2 = CheckValueLocale("Player", "SaveAsPlaylistError2", "no playlist given");
            public static string SaveCurrentPlaylistError1 = CheckValueLocale("Player", "SaveCurrentPlaylistError1", "Error: Save playlist");
            public static string SaveCurrentPlaylistError2 = CheckValueLocale("Player", "SaveCurrentPlaylistError2", " no playlist given");
            public static string SaveReplacePlaylistMessage1 = CheckValueLocale("Player", "SaveReplacePlaylistMessage1", "Enter playlist name:");
            public static string SaveReplacePlaylistMessage2 = CheckValueLocale("Player", "SaveReplacePlaylistMessage2", "Save/Replace playlist");
            public static string SaveReplacePlaylistError1 = CheckValueLocale("Player", "SaveReplacePlaylistError1", "Error: Save/Replace playlist");
            public static string SaveReplacePlaylistError2 = CheckValueLocale("Player", "SaveReplacePlaylistError2", "no playlist given");
            public static string PlayOtherPlaylistMessage1 = CheckValueLocale("Player", "PlayOtherPlaylistMessage1", "Enter playlist name:");
            public static string PlayOtherPlaylistMessage2 = CheckValueLocale("Player", "PlayOtherPlaylistMessage2", "Play other playlist");
            public static string PlayOtherPlaylistError1 = CheckValueLocale("Player", "PlayOtherPlaylistError1", "Error: Play other playlist");
            public static string PlayOtherPlaylistError2 = CheckValueLocale("Player", "PlayOtherPlaylistError2", "no playlist given");
            public static string AllPlaylists = CheckValueLocale("Player", "AllPlaylists", "All playlists");
            public static string ShowSongsInPlaylistMessage1 = CheckValueLocale("Player", "ShowSongsInPlaylistMessage1", "Enter playlist name:");
            public static string ShowSongsInPlaylistMessage2 = CheckValueLocale("Player", "ShowSongsInPlaylistMessage2", "Show songs in playlist");
            public static string ShowSongsInPlaylistError1 = CheckValueLocale("Player", "ShowSongsInPlaylistError1", "Error: Show songs in playlist");
            public static string ShowSongsInPlaylistError2 = CheckValueLocale("Player", "ShowSongsInPlaylistError2", "no playlist given");
            public static string SongsInPlaylist = CheckValueLocale("Player", "SongsInPlaylist", "Songs in playlist");
            public static string AddSongToPlaylistMessage1 = CheckValueLocale("Player", "AddSongToPlaylistMessage1", "Enter song to add to playlist:");
            public static string AddSongToPlaylistMessage2 = CheckValueLocale("Player", "AddSongToPlaylistMessage2", "Add song to playlist");
            public static string AddSongToPlaylistError1 = CheckValueLocale("Player", "AddSongToPlaylistError1", "Error: Add song to playlist");
            public static string AddSongToPlaylistError2 = CheckValueLocale("Player", "AddSongToPlaylistError2", "no song given");
            public static string AddSongToPlaylistError3 = CheckValueLocale("Player", "AddSongToPlaylistError3", "Error:");
            public static string AddSongToPlaylistError4 = CheckValueLocale("Player", "AddSongToPlaylistError4", "invalid song: Make sure you typed it correctly");
        }
        public static class Help
        {
            public static string ToMainMenu = CheckValueLocale("Help", "ToMainMenu", "To Main Menu");
            public static string Controls = CheckValueLocale("Help", "Controls", "Keybinds");
            public static string Description = CheckValueLocale("Help", "Description", "Description");
            public static string ModControls = CheckValueLocale("Help", "ModControls", "Keybinds");
            public static string PlayPause = CheckValueLocale("Help", "PlayPause", "Play/Pause");
            public static string Quit = CheckValueLocale("Help", "Quit", "Quit");
            public static string Rewind = CheckValueLocale("Help", "Rewind", "Rewind");
            public static string Forward = CheckValueLocale("Help", "Forward", "Forward");
            public static string Seconds = CheckValueLocale("Help", "Seconds", "second(s)");
            public static string VolumeUp = CheckValueLocale("Help", "VolumeUp", "Volume up");
            public static string VolumeDown = CheckValueLocale("Help", "VolumeDown", "Volume down");
            public static string ToggleLooping = CheckValueLocale("Help", "ToggleLooping", "Toggle looping");
            public static string ToggleMute = CheckValueLocale("Help", "ToggleMute", "Toggle mute");
            public static string ToggleShuffle = CheckValueLocale("Help", "ToggleShuffle", "Toggle shuffle");
            public static string EditKeybinds = CheckValueLocale("Help", "EditKeybinds", "Edit keybinds");
            public static string Playlist = CheckValueLocale("Help", "Playlist", "Playlist");
            public static string PreviousSong = CheckValueLocale("Help", "PreviousSong", "Previous song");
            public static string NextSong = CheckValueLocale("Help", "NextSong", "Next song");
            public static string PlayRandomSong = CheckValueLocale("Help", "PlayRandomSong", "Play random song");
            public static string DeleteCurrentSongFromPlaylist = CheckValueLocale("Help", "DeleteCurrentSongFromPlaylist", "Delete from playlist");
            public static string ShowPlaylistOptions = CheckValueLocale("Help", "ShowPlaylistOptions", "Show playlist options");
            public static string ShowCmdHelp = CheckValueLocale("Help", "ShowCmdHelp", "Show cmd Help");
            public static string Press = CheckValueLocale("Help", "Press", "Press");
            public static string ToHideHelp = CheckValueLocale("Help", "ToHideHelp", "to hide/show help");
            public static string ForSettings = CheckValueLocale("Help", "ForSettings", "for settings");
            public static string ToShowPlaylist = CheckValueLocale("Help", "ToShowPlaylist", "to show playlist");
            public static string AddsongToPlaylist = CheckValueLocale("Help", "AddsongToPlaylist", "Add to playlist");
            public static string ListAllSongsInOtherPlaylist = CheckValueLocale("Help", "ListAllSongsInOtherPlaylist", "Show playlist songs");
            public static string ListAllPlaylists = CheckValueLocale("Help", "ListAllPlaylists", "List all playlists");
            public static string PlayOtherPlaylist = CheckValueLocale("Help", "PlayOtherPlaylist", "Play other playlist");
            public static string SavePlaylist = CheckValueLocale("Help", "SavePlaylist", "Save playlist");
            public static string SaveAs = CheckValueLocale("Help", "SaveAs", "Save as");
            public static string ShufflePlaylist = CheckValueLocale("Help", "ShufflePlaylist", "Suffle playlist");
            public static string PlaySongs = CheckValueLocale("Help", "PlaySongs", "Play song(s)");
            public static string RedownloadCurrentSong = CheckValueLocale("Help", "RedownloadCurrentSong", "Redownload song");
            public static string ChangeLanguage = CheckValueLocale("Help", "ChangeLanguage", "Change language");
        }

        public static class Settings
        {
            public static string _Settings = CheckValueLocale("Settings", "Settings", "Settings");
            public static string Value = CheckValueLocale("Settings", "Value", "Value");
            public static string ChangeValue = CheckValueLocale("Settings", "ChangeValue", "Change Value");
            public static string Forwardseconds = CheckValueLocale("Settings", "Forwardseconds", "Forward seconds");
            public static string Rewindseconds = CheckValueLocale("Settings", "Rewindseconds", "Rewind seconds");
            public static string ChangeVolumeBy = CheckValueLocale("Settings", "ChangeVolumeBy", "Change Volume By");
            public static string AutoSave = CheckValueLocale("Settings", "AutoSave", "Playlist Auto Save");
            public static string ToChange = CheckValueLocale("Settings", "ToChange", "To Change");
            public static string ToToggle = CheckValueLocale("Settings", "ToToggle", "To Toggle");
        }

        public static class CliHelp
        {
            public static string Commands = CheckValueLocale("CliHelp", "Commands", "Commands");
            public static string Description = CheckValueLocale("CliHelp", "Description", "Description");
            public static string PlaySongFromUrl = CheckValueLocale("CliHelp", "PlaySongFromUrl", "Play song(s) from url(s)");
            public static string PlaySongFromFile = CheckValueLocale("CliHelp", "PlaySongFromFile", "Play song(s) from file(s)");
            public static string PlaySongFromSoundcloud = CheckValueLocale("CliHelp", "PlaySongFromSoundcloud", "Play song(s) from soundcloud url(s)");
            public static string PlaySongFromSoundcloudPlaylist = CheckValueLocale("CliHelp", "PlaySongFromSoundcloudPlaylist", "Play song(s) from soundcloud playlist url(s)");
            public static string PlaySongFromYoutube = CheckValueLocale("CliHelp", "PlaySongFromYoutube", "Play song(s) from youtube url(s)");
            public static string PlayPlaylistFromYoutube = CheckValueLocale("CliHelp", "PlayPlaylistFromYoutube", "Play playlist(s) from youtube url(s) ");
            public static string ShowPlaylistCommands = CheckValueLocale("CliHelp", "ShowPlaylistCommands", "Show playlist commands");
            public static string OpenJammerFolder = CheckValueLocale("CliHelp", "OpenJammerFolder", "Open Jammer folder");
            public static string AutoUpdateJammer = CheckValueLocale("CliHelp", "AutoUpdateJammer", "Auto Update Jammer");
            public static string ShowJammerVersion = CheckValueLocale("CliHelp", "ShowJammerVersion", "Show Jammer version");
            public static string Url = CheckValueLocale("CliHelp", "Url", "url");
            public static string File = CheckValueLocale("CliHelp", "File", "file");
            public static string Username = CheckValueLocale("CliHelp", "Username", "username");
            public static string TrackName = CheckValueLocale("CliHelp", "TrackName", "track-name");
            public static string PlaylistName = CheckValueLocale("CliHelp", "PlaylistName", "playlist-name");
            public static string PlaylistCommands = CheckValueLocale("CliHelp", "PlaylistCommands", "Playlist Commands");
            public static string PlayPlaylist = CheckValueLocale("CliHelp", "PlayPlaylist", "Play playlist");
            public static string CreatePlaylist = CheckValueLocale("CliHelp", "CreatePlaylist", "Create playlist");
            public static string DeletePlaylist = CheckValueLocale("CliHelp", "DeletePlaylist", "Delete playlist");
            public static string AddSongsToPlaylist = CheckValueLocale("CliHelp", "AddSongsToPlaylist", "Add songs to playlist");
            public static string RemoveSongsFromPlaylist = CheckValueLocale("CliHelp", "RemoveSongsFromPlaylist", "Remove songs from playlist");
            public static string ShowSongsInPlaylist = CheckValueLocale("CliHelp", "ShowSongsInPlaylist", "Show songs in playlist");
            public static string ListAllPlaylists = CheckValueLocale("CliHelp", "ListAllPlaylists", "List all playlists");
            public static string Name = CheckValueLocale("CliHelp", "Name", "name");
            public static string Song = CheckValueLocale("CliHelp", "Song", "song");
        }
        public static class PlaylistOptions
        {
            public static string EnterPlayListCmd = CheckValueLocale("PlaylistOptions", "EnterPlayListCmd", "Enter playlist command:");
            public static string AddSongToPlaylist = CheckValueLocale("PlaylistOptions", "AddSongToPlaylist", "add song to playlist");
            public static string Deletesong = CheckValueLocale("PlaylistOptions", "Deletesong", "delete song current song from playlist");
            public static string ShowSongs = CheckValueLocale("PlaylistOptions", "ShowSongs", "show songs in other playlist");
            public static string ListAll = CheckValueLocale("PlaylistOptions", "ListAll", "list all playlists");
            public static string PlayOther = CheckValueLocale("PlaylistOptions", "PlayOther", "play other playlist");
            public static string SaveReplace = CheckValueLocale("PlaylistOptions", "SaveReplace", "save/replace playlist");
            public static string GoToSong = CheckValueLocale("PlaylistOptions", "GoToSong", "go to song in playlist");
            public static string Shuffle = CheckValueLocale("PlaylistOptions", "Shuffle", "shuffle playlist");
            public static string PlaySong = CheckValueLocale("PlaylistOptions", "PlaySong", "play song(s)");
            public static string Exit = CheckValueLocale("PlaylistOptions", "Exit", "exit");
        }
        public static class Miscellaneous
        {
            public static string SearchASongFromSoundcloudByName = CheckValueLocale("Miscellaneous", "SearchASongFromSoundcloudByName", "Search a song from SoundCloud by its name");
            public static string SearchForSongOnYoutubeorSoundcloud = CheckValueLocale("Miscellaneous", "SearchForSongOnYoutubeorSoundcloud", "Search for a song on Youtube or SoundCloud");
            public static string SearchASongFromYoutubeByName = CheckValueLocale("Miscellaneous", "SearchASongFromYoutubeByName", "Search a song from Youtube by its name");
            public static string ThemeFileCreatedInJammerFolder = CheckValueLocale("Miscellaneous", "ThemeFileCreatedInJammerFolder", "Theme file created in the jammer/themes folder");
            public static string ChooseSoundFont = CheckValueLocale("Miscellaneous", "ChooseSoundFont", "Choose a soundfont:");
            public static string GoEditThemeFile = CheckValueLocale("Miscellaneous", "GoEditThemeFile", "Go edit the theme file in <jammer/themes>");
            public static string EnterThemeName = CheckValueLocale("Miscellaneous", "EnterThemeName", "Enter a theme name");
            public static string ChooseTheme = CheckValueLocale("Miscellaneous", "ChooseTheme", "Choose a theme:");
            public static string NameOfYourAwesomeTheme = CheckValueLocale("Miscellaneous", "NameOfYourAwesomeTheme", "Name of your AWESOME theme");
            public static string On = CheckValueLocale("Miscellaneous", "On", "On");
            public static string Off = CheckValueLocale("Miscellaneous", "Off", "Off");
            public static string True = CheckValueLocale("Miscellaneous", "True", "True");
            public static string False = CheckValueLocale("Miscellaneous", "False", "False");
            public static string Version = CheckValueLocale("Miscellaneous", "Version", "version");
            public static string YesNo = CheckValueLocale("Miscellaneous", "YesNo", "(y/n),");


        }

        public static class OutsideItems
        {
            public static string LatestVersion = CheckValueLocale("OutsideItems", "LatestVersion", "Latest version");
            public static string OpeningFolder = CheckValueLocale("OutsideItems", "OpeningFolder", "Opening Jammer folder...");
            public static string RunUpdate = CheckValueLocale("OutsideItems", "RunUpdate", "Run the update command");
            public static string CheckingUpdates = CheckValueLocale("OutsideItems", "CheckingUpdates", "Checking for updates...");
            public static string UpdateFound = CheckValueLocale("OutsideItems", "UpdateFound", "Update found!");
            public static string Downloading = CheckValueLocale("OutsideItems", "Downloading", "Downloading...");
            public static string DownloadedTo = CheckValueLocale("OutsideItems", "DownloadedTo", "Downloaded to");
            public static string Installing = CheckValueLocale("OutsideItems", "Installing", "Installing...");
            public static string UpToDate = CheckValueLocale("OutsideItems", "UpToDate", "Jammer is up to date!");
            public static string InitializeError = CheckValueLocale("OutsideItems", "InitializeError", "Can not initialize device");
            public static string Error = CheckValueLocale("OutsideItems", "Error", "Error");
            public static string AlreadyExists = CheckValueLocale("OutsideItems", "AlreadyExists", "Playlist already exists in ");
            public static string Overwrite = CheckValueLocale("OutsideItems", "Overwrite", "Overwrite?");
            public static string Showing = CheckValueLocale("OutsideItems", "Showing", "Showing ");
            public static string IsEmpty = CheckValueLocale("OutsideItems", "IsEmpty", " is empty");
            public static string DoesntExist = CheckValueLocale("OutsideItems", "DoesntExist", " does not exist");
            public static string RemovingFrom = CheckValueLocale("OutsideItems", "RemovingFrom", "Removing songs from");
            public static string Removing = CheckValueLocale("OutsideItems", "Removing", "Removing");
            public static string NotInPlaylist = CheckValueLocale("OutsideItems", "NotInPlaylist", "is not in playlist");
            public static string Playlist = CheckValueLocale("OutsideItems", "Playlist", "Playlist");
            public static string Done = CheckValueLocale("OutsideItems", "Done", "Done");
            public static string IsALreadyInPlaylist = CheckValueLocale("OutsideItems", "IsALreadyInPlaylist", "is already in playlist");
            public static string Adding = CheckValueLocale("OutsideItems", "Adding", "Adding");
            public static string AddingSongsTo = CheckValueLocale("OutsideItems", "AddingSongsTo", "Adding songs to");
            public static string ErrorPlaying = CheckValueLocale("OutsideItems", "ErrorPlaying", "Error Playing Playlist");
            public static string Playing = CheckValueLocale("OutsideItems", "Playing", "Playing");
            public static string StartingUp = CheckValueLocale("OutsideItems", "StartingUp", "Starting up");
            public static string Playlists = CheckValueLocale("OutsideItems", "Playlists", "Playlists");
            public static string Deleting = CheckValueLocale("OutsideItems", "Deleting", "Deleting");
            public static string CreatingPlaylist = CheckValueLocale("OutsideItems", "CreatingPlaylist", "Creating playlist");
            public static string Downloaded = CheckValueLocale("OutsideItems", "Downloaded", "Downloaded");
            public static string Of = CheckValueLocale("OutsideItems", "Of", "of ");
            public static string Bytes = CheckValueLocale("OutsideItems", "Bytes", "bytes");
            public static string ErrorDownload = CheckValueLocale("OutsideItems", "ErrorDownload", "Error occurred during download: ");
            public static string NoPlaylistName = CheckValueLocale("OutsideItems", "NoPlaylistName", "No playlist name given");
            public static string NoPlaylistNameSong = CheckValueLocale("OutsideItems", "NoPlaylistNameSong", "o playlist name or song given");
            public static string IsValid = CheckValueLocale("OutsideItems", "IsValid", "is valid");
            public static string IsntValid = CheckValueLocale("OutsideItems", "IsntValid", "is not valid");
            public static string Checking = CheckValueLocale("OutsideItems", "Checking", "Checking");
            public static string File = CheckValueLocale("OutsideItems", "File", "File");
            public static string NoTrackPlaylist = CheckValueLocale("OutsideItems", "NoTrackPlaylist", "o tracks in playlist");
            public static string ToLocation = CheckValueLocale("OutsideItems", "ToLocation", "to");
            public static string NoAudioStream = CheckValueLocale("OutsideItems", "NoAudioStream", "This video has no audio stream");
            public static string YtFileExists = CheckValueLocale("OutsideItems", "YtFileExists", "Youtube file already exists");
            public static string InvalidUrl = CheckValueLocale("OutsideItems", "InvalidUrl", "Invalid url");
            public static string PressToContinue = CheckValueLocale("OutsideItems", "PressToContinue", "Press any key to continue");
            public static string InvalidInput = CheckValueLocale("OutsideItems", "InvalidInput", "Invalid input");
            public static string EnterForwardSeconds = CheckValueLocale("OutsideItems", "EnterForwardSeconds", "Enter forward seconds");
            public static string EnterBackwardSeconds = CheckValueLocale("OutsideItems", "EnterBackwardSeconds", "Enter backward seconds");
            public static string EnterVolumeChange = CheckValueLocale("OutsideItems", "EnterVolumeChange", "Enter volume change");
            public static string NoCommand = CheckValueLocale("OutsideItems", "NoCommand", "No playlist command given");
            public static string NoSongsInPlaylist = CheckValueLocale("OutsideItems", "NoSongsInPlaylist", "No songs in playlist");
            public static string SongNotFound = CheckValueLocale("OutsideItems", "SongNotFound", "Song not found");
            public static string UnsupportedFileFormat = CheckValueLocale("OutsideItems", "UnsupportedFileFormat", "Unsupported file format");
            public static string SongInPlaylist = CheckValueLocale("OutsideItems", "SongInPlaylist", "Song already in playlist");
            public static string IndexOoR = CheckValueLocale("OutsideItems", "IndexOoR", "Index out of range");
            public static string StartPlayingMessage1 = CheckValueLocale("OutsideItems", "StartPlayingMessage1", "Deleting song from playlist");
            public static string StartPlayingMessage2 = CheckValueLocale("OutsideItems", "StartPlayingMessage2", "Error: Cannot play song");
            public static string ShowingPlaylist = CheckValueLocale("OutsideItems", "ShowingPlaylist", "Showing playlist");
            public static string PlaylistIsEmpty = CheckValueLocale("OutsideItems", "PlaylistIsEmpty", "Playlist is empty");
            public static string PlaylistDoesntExist = CheckValueLocale("OutsideItems", "PlaylistDoesntExist", "Playlist does not exist");
            public static string CouldntFindLocales1 = CheckValueLocale("OutsideItems", "CouldntFindLocales1", "Could not find the 'locales' directory in:");
            public static string CouldntFindLocales2 = CheckValueLocale("OutsideItems", "CouldntFindLocales2", "Exiting to Main View...");
            public static string NoLocaleInDir = CheckValueLocale("OutsideItems", "NoLocaleInDir", "No .ini files found in the locales directory.");
            public static string ErrorLoadingDescription = CheckValueLocale("OutsideItems", "ErrorLoadingDescription", "Error loading description");
            public static string CurrentPlaylist = CheckValueLocale("OutsideItems", "CurrentPlaylist", "Current playlist");
            public static string CurrentQueue = CheckValueLocale("OutsideItems", "CurrentQueue", "Current queue");
            public static string CurrPlaylistView = CheckValueLocale("OutsideItems", "CurrPlaylistView", "Move with");
            public static string PlaySongWith = CheckValueLocale("OutsideItems", "PlaySongWith", "Play with");
            public static string DeleteSongWith = CheckValueLocale("OutsideItems", "DeleteSongWith", "Delete with");
            public static string AddToQueue = CheckValueLocale("OutsideItems", "AddToQueue", "Add to queue with");
            public static string DownloadErrorSoundcloud = CheckValueLocale("OutsideItems", "DownloadErrorSoundcloud", "Soundcloud download error");
        }
        public static class LocaleKeybind
        {
            public static string HardDeleteCurrentSong = CheckValueLocale("LocaleKeybind", "HardDeleteCurrentSong", "Delete song from playlist and PC");
            public static string Description = CheckValueLocale("LocaleKeybind", "Description", "Language file");
            public static string CurrentControl = CheckValueLocale("LocaleKeybind", "CurrentControl", "Keybind");
            public static string EditKeyBindMessage1 = CheckValueLocale("LocaleKeybind", "EditKeyBindMessage1", "Press 'Escape' to cancel. Save with");
            public static string EditKeyBindMessage2 = CheckValueLocale("LocaleKeybind", "EditKeyBindMessage2", "Allowed modifiers: ctrl, alt, shift and their combinations");
            public static string EditKeyBindMessage3 = CheckValueLocale("LocaleKeybind", "EditKeyBindMessage3", "Press Enter to edit highlighted keybind, move up and down with:");
            public static string EditKeyBindMessage4 = CheckValueLocale("LocaleKeybind", "EditKeyBindMessage4", "Press Shift + Alt + Delete to reset keybinds");
            public static string ChangeLanguageMessage1 = CheckValueLocale("LocaleKeybind", "ChangeLanguageMessage1", "Enter to choose the language, move up and down with:");
            public static string Ini_LoadNewLocaleMessage1 = CheckValueLocale("LocaleKeybind", "Ini_LoadNewLocaleMessage1", "Language has been changed succesfully. Reset needed to load new language");
            public static string Ini_LoadNewLocaleMessage2 = CheckValueLocale("LocaleKeybind", "Ini_LoadNewLocaleMessage2", "Language changed succesfully!");
            public static string Ini_LoadNewLocaleError1 = CheckValueLocale("LocaleKeybind", "Ini_LoadNewLocaleError1", "Error changing languages. Resorting back to English");
            public static string Ini_LoadNewLocaleError2 = CheckValueLocale("LocaleKeybind", "Ini_LoadNewLocaleError2", "Error: Could not change language");
            public static string WriteIni_KeyDataError1 = CheckValueLocale("LocaleKeybind", "WriteIni_KeyDataError1", "Keybind");
            public static string WriteIni_KeyDataError2 = CheckValueLocale("LocaleKeybind", "WriteIni_KeyDataError2", "already exists");
            public static string WriteIni_KeyDataError3 = CheckValueLocale("LocaleKeybind", "WriteIni_KeyDataError3", "Error: Keybind already exists");
            public static string GoToSongStart = CheckValueLocale("LocaleKeybind", "GoToSongStart", "Go to song start");
            public static string GoToSongEnd = CheckValueLocale("LocaleKeybind", "GoToSongEnd", "Go to song end");
            public static string FOrwardSecAmount = CheckValueLocale("LocaleKeybind", "FOrwardSecAmount", "Forward second amount");
            public static string BackwardSecAmount = CheckValueLocale("LocaleKeybind", "BackwardSecAmount", "Backward second amount");
            public static string ChangeVolume = CheckValueLocale("LocaleKeybind", "ChangeVolume", "Change volume by");
            public static string ToggleAutosave = CheckValueLocale("LocaleKeybind", "ToggleAutosave", "Toggle autosave");
            public static string CurrentState = CheckValueLocale("LocaleKeybind", "CurrentState", "Show current state");
            public static string KeybindResettedMessage1 = CheckValueLocale("LocaleKeybind", "KeybindResettedMessage1", "Keybinds resetted");
            public static string KeybindResettedMessage2 = CheckValueLocale("LocaleKeybind", "KeybindResettedMessage2", "Keybinds have been resetted");
        }


        public static class EditKeysTexts
        {
            public static string ToMainMenu = CheckValueLocale("EditKeysTexts", "ToMainMenu", "To main menu");
            public static string PlayPause = CheckValueLocale("EditKeysTexts", "PlayPause", "Play/Pause");
            public static string Quit = CheckValueLocale("EditKeysTexts", "Quit", "Quit");
            public static string NextSong = CheckValueLocale("EditKeysTexts", "NextSong", "Next song");
            public static string PreviousSong = CheckValueLocale("EditKeysTexts", "PreviousSong", "Previous song");
            public static string PlaySong = CheckValueLocale("EditKeysTexts", "PlaySong", "Play song");
            public static string Forward5s = CheckValueLocale("EditKeysTexts", "Forward5s", "Forward");
            public static string Backwards5s = CheckValueLocale("EditKeysTexts", "Backwards5s", "Rewind");
            public static string VolumeUp = CheckValueLocale("EditKeysTexts", "VolumeUp", "Volume up");
            public static string VolumeDown = CheckValueLocale("EditKeysTexts", "VolumeDown", "Volume down");
            public static string Shuffle = CheckValueLocale("EditKeysTexts", "Shuffle", "Shuffle");
            public static string SaveAsPlaylist = CheckValueLocale("EditKeysTexts", "SaveAsPlaylist", "Save as playlist");
            public static string SaveCurrentPlaylist = CheckValueLocale("EditKeysTexts", "SaveCurrentPlaylist", "Save current playlist");
            public static string ShufflePlaylist = CheckValueLocale("EditKeysTexts", "ShufflePlaylist", "Shuffle playlist");
            public static string Loop = CheckValueLocale("EditKeysTexts", "Loop", "Toggle Looping");
            public static string Mute = CheckValueLocale("EditKeysTexts", "Mute", "Toggle Mute");
            public static string ShowHidePlaylist = CheckValueLocale("EditKeysTexts", "ShowHidePlaylist", "Show playlist");
            public static string ListAllPlaylists = CheckValueLocale("EditKeysTexts", "ListAllPlaylists", "List all playlists");
            public static string Help = CheckValueLocale("EditKeysTexts", "Help", "Help");
            public static string Settings = CheckValueLocale("EditKeysTexts", "Settings", "Settings");
            public static string ToSongStart = CheckValueLocale("EditKeysTexts", "ToSongStart", "To song start");
            public static string ToSongEnd = CheckValueLocale("EditKeysTexts", "ToSongEnd", "To song end");
            public static string PlaylistOptions = CheckValueLocale("EditKeysTexts", "PlaylistOptions", "Playlist options");
            public static string ForwardSecondAmount = CheckValueLocale("EditKeysTexts", "ForwardSecondAmount", "Forward second amount");
            public static string BackwardSecondAmount = CheckValueLocale("EditKeysTexts", "BackwardSecondAmount", "Backward second amount");
            public static string ChangeVolumeAmount = CheckValueLocale("EditKeysTexts", "ChangeVolumeAmount", "Change volume amount");
            public static string Autosave = CheckValueLocale("EditKeysTexts", "Autosave", "Toggle autosave");
            public static string CurrentState = CheckValueLocale("EditKeysTexts", "CurrentState", "Show current state");
            public static string CommandHelpScreen = CheckValueLocale("EditKeysTexts", "CommandHelpScreen", "Command prompt help screen");
            public static string DeleteCurrentSong = CheckValueLocale("EditKeysTexts", "DeleteCurrentSong", "Delete current song");
            public static string AddSongToPlaylist = CheckValueLocale("EditKeysTexts", "AddSongToPlaylist", "Add song to playlist");
            public static string ShowSongsInPlaylists = CheckValueLocale("EditKeysTexts", "ShowSongsInPlaylists", "Show songs in playlist");
            public static string PlayOtherPlaylist = CheckValueLocale("EditKeysTexts", "PlayOtherPlaylist", "Play other playlist");
            public static string RedownloadCurrentSong = CheckValueLocale("EditKeysTexts", "RedownloadCurrentSong", "Redownload current song");
            public static string EditKeybindings = CheckValueLocale("EditKeysTexts", "EditKeybindings", "Edit keybindings");
            public static string ChangeLanguage = CheckValueLocale("EditKeysTexts", "ChangeLanguage", "Change language");
            public static string ChangeTheme = CheckValueLocale("EditKeysTexts", "ChangeTheme", "Change theme");
            public static string PlayRandomSong = CheckValueLocale("EditKeysTexts", "PlayRandomSong", "Play random song");
            public static string PlaylistViewScrollup = CheckValueLocale("EditKeysTexts", "PlaylistViewScrollup", "Scroll up in tables");
            public static string PlaylistViewScrolldown = CheckValueLocale("EditKeysTexts", "PlaylistViewScrolldown", "Scroll down in tables");
            public static string Enter = CheckValueLocale("EditKeysTexts", "Enter", "Choose highlighted item in tables");
            public static string AddSongToQueue = CheckValueLocale("EditKeysTexts", "AddSongToQueue", "Add song to queue in playlist view");
            public static string ToggleInfo = CheckValueLocale("EditKeysTexts", "ToggleInfo", "Toggle info view");
            public static string LoadEffects = CheckValueLocale("EditKeysTexts", "LoadEffects", "Load effects");
            public static string ToggleMediaButtons = CheckValueLocale("EditKeysTexts", "ToggleMediaButtons", "Toggle media buttons");
            public static string ToggleVisualizer = CheckValueLocale("EditKeysTexts", "ToggleVisualizer", "Toggle Visualizer");
            public static string LoadVisualizer = CheckValueLocale("EditKeysTexts", "LoadVisualizer", "Load Visualizer");
            public static string Choose = CheckValueLocale("EditKeysTexts", "Choose", "Choose");
            public static string ChangeSoundFont = CheckValueLocale("EditKeysTexts", "ChangeSoundFont", "Change soundfont");
            public static string ShowLog = CheckValueLocale("EditKeysTexts", "ShowLog", "Show log");
            public static string Search = CheckValueLocale("EditKeysTexts", "Search", "Search");
            public static string HardDeleteCurrentSong = CheckValueLocale("EditKeysTexts", "HardDeleteCurrentSong", "Hard delete current song");
            public static string VolumeUpByOne = CheckValueLocale("EditKeysTexts", "VolumeUpByOne", "Increase volume by 1%");
            public static string VolumeDownByOne = CheckValueLocale("EditKeysTexts", "VolumeDownByOne", "Decrease volume by 1%");
            public static string RenameSong = CheckValueLocale("EditKeysTexts", "RenameSong", "Rename song");
            public static string ExitRssFeed = CheckValueLocale("EditKeysTexts", "ExitRssFeed", "Exit RSS feed");
            public static string SearchInPlaylist = CheckValueLocale("EditKeysTexts", "SearchInPlaylist", "Search in playlist");

        }
        static string CheckValueLocale(string key, string value, string defaultString)
        {
            string finalValue = IniFileHandling.ReadIni_LocaleData(key, value);

            if (finalValue == null || finalValue.Equals(""))
            {
                return defaultString;
            }
            else
            {
                return finalValue;
            }
        }
    }
}
