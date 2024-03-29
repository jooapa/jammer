using Spectre.Console;
using jammer;
using System.ComponentModel.DataAnnotations;

static class TUI
{
    static bool cls = false; // clear screen

    static public void DrawPlayer() {
        try {

            if (Start.playerView == "help" || Start.playerView == "settings")
            {
                return;
            }
            var mainTable = new Table();
            var songsTable = new Table();
            var controlsTable = new Table();
            var timeTable = new Table();

            if (Start.playerView == "default") {
                UIComponent_Normal(songsTable);
            }
            else if (Start.playerView == "all") {
                UIComponent_Songs(songsTable);
            }

            UIComponent_Controls(controlsTable);

            if (cls) {
                if (Start.playerView != "all") {
                    AnsiConsole.Clear();
                }
                cls = false;
            }
            if (Start.playerView == "default" || Start.playerView == "fake") {
                AnsiConsole.Cursor.SetPosition(0, 0);
            }

            // render maintable with tables in it
            mainTable.AddColumns(GetSongWithdots(Utils.currentSong, Start.consoleWidth - 8)).Width(Start.consoleWidth);
            mainTable.AddRow(songsTable.Centered().Width(Start.consoleWidth));
            songsTable.Border = TableBorder.Square;
            mainTable.AddRow(controlsTable.Centered());
            // mainTable.Width(100);
            var helpTable = new Table();
            helpTable.AddColumn($"[red]{Keybindings.Help}[/] {Locale.Player.ForHelp} | [yellow]{Keybindings.Settings}[/] {Locale.Help.ForSettings} | [green]{Keybindings.ShowHidePlaylist}[/] {Locale.Player.ForPlaylist}");
            helpTable.Border = TableBorder.Rounded;
            
            mainTable.Border = TableBorder.HeavyEdge;
            mainTable.AddRow(helpTable.Centered());

            if (Start.playerView != "all") {
                // add \n to the end of the maintable until the end of the console by height
                int tableRowCount = Start.consoleHeight - 25;
                if (tableRowCount < 0) {
                    tableRowCount = 0;
                }

                for (int i = 0; i < tableRowCount; i++) {
                    mainTable.AddRow("").Width(Start.consoleWidth);
                }
            }
            mainTable.AddRow(UIComponent_Time(timeTable, Start.consoleWidth-20).Centered()).Width(Start.consoleWidth);
            AnsiConsole.Write(mainTable);            

            //var debug = new Table();
            //debug.AddColumn("Debug");
            //debug.AddRow("lastseconds" + Start.prevMusicTimePlayed);
            //AnsiConsole.Write(debug);
        }
        catch (Exception e) {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[red]{Locale.Player.DrawingError}[/]");
            AnsiConsole.MarkupLine($"[red]{Locale.Player.ControlsWillWork}[/]");
            AnsiConsole.MarkupLine("[red]" + e.Message + "[/]");
        }
    }

    static public void ClearScreen() {
        cls = true;
    }

    static public string GetAllSongs() {
        if (Utils.songs.Length == 0) {
            return $"[grey]{Locale.Player.NoSongsInPlaylist}[/]";
        }
        string allSongs = "";
        foreach (string song in Utils.songs) {
            // add green color to current song, based on the index
            if (Utils.songs[Utils.currentSongIndex] == song) {
                allSongs += "[green]" + song + "[/]\n";
                continue;
            }
            allSongs += song + "\n";
        }
        // remove last newline
        allSongs = allSongs.Substring(0, allSongs.Length - 1);
        return allSongs;
    }

    static string GetSongWithdots(string song, int length = 80) {
        if (song.Length > length) {
            song = string.Concat("...", song.AsSpan(song.Length - length));
        }
        return song;
    }
    public static string GetPrevCurrentNextSong() {
        // return previous, current and next song in playlist
        string prevSong;
        string nextSong;
        string currentSong;
        int songLength = Start.consoleWidth - 23;
        if (Utils.songs.Length == 0)
        {
            currentSong = $"[grey]{Locale.Player.Current}  : -[/]";
        }
        else
        {
            currentSong = $"[green]{Locale.Player.Current}  : " + GetSongWithdots(Utils.songs[Utils.currentSongIndex], songLength) + "[/]";
        }

        if (Utils.currentSongIndex > 0)
        {
            prevSong = $"[grey]{Locale.Player.Previos} : " + GetSongWithdots(Utils.songs[Utils.currentSongIndex - 1], songLength) + "[/]";
        }
        else
        {
            prevSong = $"[grey]{Locale.Player.Previos} : -[/]";
        }


        if (Utils.currentSongIndex < Utils.songs.Length - 1)
        {
            nextSong = $"[grey]{Locale.Player.Next}     : " + GetSongWithdots(Utils.songs[Utils.currentSongIndex + 1], songLength) + "[/]";
        }
        else
        {
            nextSong = $"[grey]{Locale.Player.Next}     : -[/]";
        }

        return prevSong + $"\n[green]" + currentSong + "[/]\n" + nextSong;
    }

    static public string CalculateTime(double time) {
        int minutes = (int)time / 60;
        int seconds = (int)time % 60;
        string timeString = $"{minutes}:{seconds:D2}";
        return timeString;
    }

    public static void PlaylistInput() {
        AnsiConsole.Markup($"\n{Locale.PlaylistOptions.EnterPlayListCmd} \n");
        AnsiConsole.MarkupLine($"[grey]1. {Locale.PlaylistOptions.AddSongToPlaylist}[/]");
        AnsiConsole.MarkupLine($"[grey]2. {Locale.PlaylistOptions.Deletesong}[/]");
        AnsiConsole.MarkupLine($"[grey]3. {Locale.PlaylistOptions.ShowSongs}[/]");
        AnsiConsole.MarkupLine($"[grey]4. {Locale.PlaylistOptions.ListAll}[/]");
        AnsiConsole.MarkupLine($"[grey]5. {Locale.PlaylistOptions.PlayOther}[/]");
        AnsiConsole.MarkupLine($"[grey]6. {Locale.PlaylistOptions.SaveReplace}[/]");
        // AnsiConsole.MarkupLine($"[grey]7. {Locale.PlaylistOptions.GoToSong}[/]");
        AnsiConsole.MarkupLine($"[grey]8. {Locale.PlaylistOptions.Shuffle}[/]");
        AnsiConsole.MarkupLine($"[grey]9. {Locale.PlaylistOptions.PlaySong}[/]");
        AnsiConsole.MarkupLine($"[grey]0. {Locale.PlaylistOptions.Exit}[/]");

        var playlistInput = Console.ReadKey(true).Key;
        // if (playlistInput == "" || playlistInput == null) { return; }
        switch (playlistInput) {
            // Add song to playlist
            case ConsoleKey.D1:
                AddSongToPlaylist();
                break;
            // Delete current song from playlist
            case ConsoleKey.D2:
                DeleteCurrentSongFromPlaylist();
                break;
            // Show songs in playlist
            case ConsoleKey.D3:
                ShowSongsInPlaylist();
                break;
            // List all playlists
            case ConsoleKey.D4:
                ListAllPlaylists();
                break;
            // Play other playlist
            case ConsoleKey.D5:
                PlayOtherPlaylist();
                break;
            // Save/replace playlist
            case ConsoleKey.D6:
                SaveReplacePlaylist();
                break;
            // Goto song in playlist
            case ConsoleKey.D7:
                GotoSongInPlaylist();
                break;
                
            // Shuffle playlist (randomize)
            case ConsoleKey.D8:
                ShufflePlaylist();
                break;
            // Play single song
            case ConsoleKey.D9:
                PlaySingleSong();
                break;
            // Exit
            case ConsoleKey.D0:
                return;
        }
        AnsiConsole.Clear();
    }

    public static void AddSongToPlaylist()
    {
        string songToAdd = Message.Input(Locale.Player.AddSongToPlaylistMessage1, Locale.Player.AddSongToPlaylistMessage2);
        if (songToAdd == "" || songToAdd == null) {
            Message.Data(Locale.Player.AddSongToPlaylistError1, Locale.Player.AddSongToPlaylistError2, true);
            return;
        }
        // remove quotes from songToAdd
        songToAdd = songToAdd.Replace("\"", "");
        if (!IsValidSong(songToAdd)) {
            Message.Data( Locale.Player.AddSongToPlaylistError3+ " " + songToAdd, Locale.Player.AddSongToPlaylistError4, true);
            return;
        }
        songToAdd = Absolute.Correctify(new string[] { songToAdd })[0];
        Play.AddSong(songToAdd);
        Playlists.AutoSave();
    }

    // Delete current song from playlist
    public static void DeleteCurrentSongFromPlaylist()
    {
        Play.DeleteSong(Utils.currentSongIndex);
        Playlists.AutoSave();
    }

    // Show songs in playlist
    public static void ShowSongsInPlaylist()
    {
        string? playlistNameToShow = Message.Input(Locale.Player.ShowSongsInPlaylistMessage1, Locale.Player.ShowSongsInPlaylistMessage2);
        if (playlistNameToShow == "" || playlistNameToShow == null) { 
            Message.Data(Locale.Player.ShowSongsInPlaylistError1, Locale.Player.ShowSongsInPlaylistError2, true);
            return;
        }
        AnsiConsole.Clear();
        // show songs in playlist
        Message.Data(Playlists.GetShow(playlistNameToShow), Locale.Player.SongsInPlaylist +" "+ playlistNameToShow);
    }

    // List all playlists
    public static void ListAllPlaylists()
    {
        Message.Data(Playlists.GetList(), Locale.Player.AllPlaylists);
    }

    // Play other playlist
    public static void PlayOtherPlaylist()
    {
        string? playlistNameToPlay = Message.Input(Locale.Player.PlayOtherPlaylistMessage1,Locale.Player.PlayOtherPlaylistMessage2);
        if (playlistNameToPlay == "" || playlistNameToPlay == null) { 
            Message.Data(Locale.Player.PlayOtherPlaylistError1, Locale.Player.PlayOtherPlaylistError2, true);
            return;
        }

        // play other playlist
        Playlists.Play(playlistNameToPlay, false);
    }

    // Save/replace playlist
    public static void SaveReplacePlaylist()
    {
        string playlistNameToSave = Message.Input(Locale.Player.SaveReplacePlaylistMessage1, Locale.Player.SaveReplacePlaylistMessage2);
        if (playlistNameToSave == "" || playlistNameToSave == null) {
            Message.Data(Locale.Player.SaveReplacePlaylistError1, Locale.Player.SaveReplacePlaylistError2, true);
            return;
        }
        // save playlist
        Playlists.Save(playlistNameToSave);
    }

    public static void SaveCurrentPlaylist()
    {
        if (Utils.currentPlaylist == "") {
            Message.Data(Locale.Player.SaveCurrentPlaylistError1,Locale.Player.SaveCurrentPlaylistError2, true);
            return;
        }
        // save playlist
        Playlists.Save(Utils.currentPlaylist, true);
    }

    public static void SaveAsPlaylist()
    {
        string playlistNameToSave = Message.Input(Locale.Player.SaveAsPlaylistMessage1, Locale.Player.SaveAsPlaylistMessage2);
        if (playlistNameToSave == "" || playlistNameToSave == null) {
            Message.Data(Locale.Player.SaveAsPlaylistError1, Locale.Player.SaveAsPlaylistError2, true);
            return;
        }
        // save playlist
        Playlists.Save(playlistNameToSave);
    }

    // Goto song in playlist
    public static void GotoSongInPlaylist()
    {
        string songToGoto = Message.Input(Locale.Player.GotoSongInPlaylistMessage1, Locale.Player.GotoSongInPlaylistMessage2);
        if (songToGoto == "" || songToGoto == null) {
            Message.Data(Locale.Player.GotoSongInPlaylistError1, Locale.Player.GotoSongInPlaylistError2, true);
            return;
        }
        // songToGoto = GotoSong(songToGoto);
    }

    // Shuffle playlist (randomize)
    public static void ShufflePlaylist()
    {
        // get the name of the current song
        string currentSong = Utils.songs[Utils.currentSongIndex];
        // shuffle playlist
        Play.Shuffle();
        // delete duplicates
        Utils.songs = Utils.songs.Distinct().ToArray();

        Utils.currentSongIndex = Array.IndexOf(Utils.songs, currentSong);
        // set new song from shuffle to the current song
        Utils.currentSong = Utils.songs[Utils.currentSongIndex];
        Playlists.AutoSave();
    }

    // Play single song
    public static void PlaySingleSong()
    {
        string[]? songsToPlay = Message.Input(Locale.Player.PlaySingleSongMessage1, Locale.Player.PlaySingleSongMessage2).Split(" ");
        
        if (songsToPlay == null || songsToPlay.Length == 0) {
            Message.Data(Locale.Player.PlaySingleSongError1, Locale.Player.PlaySingleSongError2, true);
            return;
        }

        // if blank "  " remove
        songsToPlay = songsToPlay.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

        songsToPlay = Absolute.Correctify(songsToPlay);
        
        // if no songs left, return
        if (songsToPlay.Length == 0) { return; }

        Utils.songs = songsToPlay;
        Utils.currentSongIndex = 0;
        Utils.currentPlaylist = "";
        Play.StopSong();
        Play.PlaySong(Utils.songs, Utils.currentSongIndex);
    }

    public static bool IsValidSong(string song) {
        if (File.Exists(song) || URL.IsUrl(song) || Directory.Exists(song)) {
            AnsiConsole.Markup($"\n[green]{Locale.Player.ValidSong}[/]");
            return true;
        }
        AnsiConsole.Markup($"\n[red]{Locale.Player.InvalidSong}[/]");
        return false;
    }

    // "Components" of the TUI
    static public void UIComponent_Controls(Table table) {
        table.AddColumn(Locale.Player.State);
        table.AddColumn(Locale.Player.Looping);
        table.AddColumn(Locale.Player.Shuffle);
        table.AddColumn(Locale.Player.Volume);
        string volume = Preferences.isMuted ? "[grey][strikethrough]" + Math.Round(Preferences.oldVolume * 100) + "%[/][/]" : Math.Round(Preferences.volume * 100) + "%";
        // TODO ADD STATE TO LOCALE
        table.AddRow(Start.state.ToString(), 
        Preferences.isLoop ? $"[green]{Locale.Miscellaneous.On}[/]" : 
                            $"[red]{Locale.Miscellaneous.Off}[/]", 
        Preferences.isShuffle ? $"[green]{Locale.Miscellaneous.On}[/]" : 
                            $"[red]{Locale.Miscellaneous.Off}[/]", volume);
    }

    static public void UIComponent_Songs(Table table) {
        if (Utils.currentPlaylist == "") {
            table.AddColumn(GetAllSongs());
        } else {
            table.AddColumn($"{Locale.Player.Playlist} [cyan]" + Utils.currentPlaylist + "[/]");
            table.AddRow(GetAllSongs());
        }
    }

    static public void UIComponent_Normal(Table table) {
        if (Utils.currentPlaylist == "") {
            table.AddColumn(GetPrevCurrentNextSong());
        } else {
            table.AddColumn($"{Locale.Player.Playlist} [cyan]" + Utils.currentPlaylist + "[/]");
            table.AddRow(GetPrevCurrentNextSong());
        }
    }

    public static Table UIComponent_Time(Table table, int? length = 100) {
        table.AddColumn(ProgressBar(Utils.MusicTimePlayed, Utils.currentMusicLength, length));
        return table;
    }

    public static string ProgressBar(double value, double max, int? length = 100) {
        if (length == null) {
            length = 100;
        }
        int progress = (int)(value / max * length);
        string progressBar = CalculateTime(value) + " |";
        for (int i = 0; i < length; i++) {
            if (i < progress) {
                progressBar += "█";
            }
            else {
                progressBar += " ";
            }
        }
        progressBar += "| " + CalculateTime(max);
        return progressBar;
    }

    static public void DrawHelp() {
        var table = new Table();
        char separator = '+';
        string[] AddSongToPlaylist = (Keybindings.AddSongToPlaylist).Replace(" ", "").Split(separator);
        string[] ShowSongsInPlaylists = (Keybindings.ShowSongsInPlaylists).Replace(" ", "").Split(separator);
        string[] ListAllPlaylists = (Keybindings.ListAllPlaylists).Replace(" ", "").Split(separator);
        string[] PlayOtherPlaylist = (Keybindings.PlayOtherPlaylist).Replace(" ", "").Split(separator);
        string[] SaveCurrentPlaylist = (Keybindings.SaveCurrentPlaylist).Replace(" ", "").Split(separator);
        string[] SaveAsPlaylist = (Keybindings.SaveAsPlaylist).Replace(" ", "").Split(separator);
        string[] ShufflePlaylist = (Keybindings.ShufflePlaylist).Replace(" ", "").Split(separator);
        string[] PlaySong = (Keybindings.PlaySong).Replace(" ", "").Split(separator);
        string[] RedownloadCurrentSong = (Keybindings.RedownloadCurrentSong).Replace(" ", "").Split(separator);
        string[] PlayPause = (Keybindings.PlayPause).Replace(" ", "").Split(separator);
        string[] Quit = (Keybindings.Quit).Replace(" ", "").Split(separator);
        string[] Backwards5s = (Keybindings.Backwards5s).Replace(" ", "").Split(separator);
        string[] Forward5s = (Keybindings.Forward5s).Replace(" ", "").Split(separator);
        string[] VolumeUp = (Keybindings.VolumeUp).Replace(" ", "").Split(separator);
        string[] VolumeDown = (Keybindings.VolumeDown).Replace(" ", "").Split(separator);
        string[] Loop = (Keybindings.Loop).Replace(" ", "").Split(separator);
        string[] Mute = (Keybindings.Mute).Replace(" ", "").Split(separator);
        string[] Shuffle = (Keybindings.Shuffle).Replace(" ", "").Split(separator);
        string[] NextSong = (Keybindings.NextSong).Replace(" ", "").Split(separator);
        string[] PreviousSong = (Keybindings.PreviousSong).Replace(" ", "").Split(separator);
        string[] PlayRandomSong = (Keybindings.PlayRandomSong).Replace(" ", "").Split(separator);
        string[] DeleteCurrentSong = (Keybindings.DeleteCurrentSong).Replace(" ", "").Split(separator);
        string[] PlaylistOptions = (Keybindings.PlaylistOptions).Replace(" ", "").Split(separator);
        string[] CommandHelpScreen = (Keybindings.CommandHelpScreen).Replace(" ", "").Split(separator);
        string[] EditKeybindings = (Keybindings.EditKeybindings).Replace(" ", "").Split(separator);
        string[] ChangeLanguage = (Keybindings.ChangeLanguage).Replace(" ", "").Split(separator);


        table.AddColumns(Locale.Help.Controls, Locale.Help.Description,Locale.Help.ModControls,Locale.Help.Description);

        table.AddRow(DrawHelpTextColouring(PlayPause), Locale.Help.PlayPause,                                               DrawHelpTextColouring(AddSongToPlaylist), Locale.Help.AddsongToPlaylist);
        table.AddRow(DrawHelpTextColouring(Quit), Locale.Help.Quit,                                                         DrawHelpTextColouring(ShowSongsInPlaylists), Locale.Help.ListAllSongsInOtherPlaylist);
        table.AddRow(DrawHelpTextColouring(Backwards5s), $"{Locale.Help.Rewind} {Preferences.changeVolumeBy * 100} {Locale.Help.Seconds}",  
                                                                                                                                                        DrawHelpTextColouring(ListAllPlaylists), Locale.Help.ListAllPlaylists);
        table.AddRow(DrawHelpTextColouring(Forward5s), $"{Locale.Help.Forward} {Preferences.changeVolumeBy * 100} {Locale.Help.Seconds}",   
                                                                                                                                                        DrawHelpTextColouring(PlayOtherPlaylist), Locale.Help.PlayOtherPlaylist);
        table.AddRow(DrawHelpTextColouring(VolumeUp), Locale.Help.VolumeUp,                                                 DrawHelpTextColouring(SaveCurrentPlaylist), Locale.Help.SavePlaylist);
        table.AddRow(DrawHelpTextColouring(VolumeDown), Locale.Help.VolumeDown,                                             DrawHelpTextColouring(SaveAsPlaylist), Locale.Help.SaveAs);
        table.AddRow(DrawHelpTextColouring(Loop), Locale.Help.ToggleLooping,                                                DrawHelpTextColouring(ShufflePlaylist), Locale.Help.ShufflePlaylist);
        table.AddRow(DrawHelpTextColouring(Mute), Locale.Help.ToggleMute,                                                       DrawHelpTextColouring(PlaySong), Locale.Help.PlaySongs);
        table.AddRow(DrawHelpTextColouring(Shuffle), Locale.Help.ToggleShuffle,                                             DrawHelpTextColouring(RedownloadCurrentSong), Locale.Help.RedownloadCurrentSong);

        table.AddRow(Locale.Help.Playlist, "" ,DrawHelpTextColouring(EditKeybindings), Locale.Help.EditKeybinds);
        table.AddRow(DrawHelpTextColouring(NextSong), Locale.Help.NextSong, DrawHelpTextColouring(ChangeLanguage), Locale.Help.ChangeLanguage);
        table.AddRow(DrawHelpTextColouring(PreviousSong), Locale.Help.PreviousSong);
        table.AddRow(DrawHelpTextColouring(PlayRandomSong), Locale.Help.PlayRandomSong);
        table.AddRow(DrawHelpTextColouring(DeleteCurrentSong), Locale.Help.DeleteCurrentSongFromPlaylist);
        table.AddRow(DrawHelpTextColouring(PlaylistOptions), Locale.Help.ShowPlaylistOptions);
        table.AddRow(DrawHelpTextColouring(CommandHelpScreen), Locale.Help.ShowCmdHelp);
        AnsiConsole.Clear();
        AnsiConsole.Write(table);
        DrawHelpSettingInfo();
    }
    
    static private string DrawHelpTextColouring(string[] textArray){
        if(textArray.Length == 1){
            return textArray[0];
        }
        else if(textArray.Length == 2){
            return $"[green1]{textArray[0]}[/] + {textArray[1]}";
        }
        else if(textArray.Length == 3){
            return $"[green1]{textArray[0]}[/] + [turquoise2]{textArray[1]}[/] + {textArray[2]}";
        } else {
            return textArray[0];
        }
    } 
    static public void DrawSettings() {
        string ForwardSecondAmount = (Keybindings.ForwardSecondAmount);
        string BackwardSecondAmount = (Keybindings.BackwardSecondAmount);
        string ChangeVolumeAmount = (Keybindings.ChangeVolumeAmount);
        string Autosave = (Keybindings.Autosave);

        var table = new Table();
        table.AddColumns(Locale.Settings._Settings, Locale.Settings.Value, Locale.Settings.ChangeValue);
        table.AddRow(Locale.Settings.Forwardseconds, Preferences.forwardSeconds + " sec", $"[green]{ForwardSecondAmount}[/] {Locale.Settings.ToChange}");
        table.AddRow(Locale.Settings.Rewindseconds, Preferences.rewindSeconds + " sec", $"[green]{BackwardSecondAmount}[/] {Locale.Settings.ToChange}");
        table.AddRow(Locale.Settings.ChangeVolumeBy, Preferences.changeVolumeBy * 100 + " %", $"[green]{ChangeVolumeAmount}[/] {Locale.Settings.ToChange}");
        table.AddRow(Locale.Settings.AutoSave, Preferences.isAutoSave ? Locale.Miscellaneous.True : Locale.Miscellaneous.False + "", $"[green]{Autosave}[/] {Locale.Settings.ToToggle}");
        AnsiConsole.Clear();
        AnsiConsole.Write(table);
        DrawHelpSettingInfo();
    }

    public static void RehreshCurrentView() {
        AnsiConsole.Clear();
        if (Start.playerView == "default") {
            DrawPlayer();
        }
        else if (Start.playerView == "help") {
            DrawHelp();
        }
        else if (Start.playerView == "settings") {
            DrawSettings();
        }
        else if (Start.playerView == "all") {
            DrawPlayer();
        }
        else if (Start.playerView == "fake") {
            DrawPlayer();
        }
        else if (Start.playerView == "editkeybindings") {
            EditKeyBindings();
        }
        else if (Start.playerView == "changelanguage") {
            ChangeLanguage();
        }
        
    }
    private static void DrawHelpSettingInfo(){
        AnsiConsole.Markup($"{Locale.Help.Press} [red]{Keybindings.Help}[/] {Locale.Help.ToHideHelp}");
        AnsiConsole.Markup($"\n{Locale.Help.Press} [yellow]{Keybindings.Settings}[/] {Locale.Help.ForSettings}");
        AnsiConsole.Markup($"\n{Locale.Help.Press} [green]{Keybindings.ShowHidePlaylist}[/] {Locale.Help.ToShowPlaylist}\n");
    }
    
    public static void CliHelp() {
        var table = new Table();
        table.AddColumn(Locale.CliHelp.Commands);
        table.AddColumn(Locale.CliHelp.Description);

        // table.AddRow("[grey]jammer[/] <url> ...", "Play song(s) from url(s)");
        table.AddRow("[grey]jammer[/] <[green]file[/]> ...", Locale.CliHelp.PlaySongFromFile);
        table.AddRow($"[grey]jammer[/] [green]soundcloud.com/{Locale.CliHelp.Username}/{Locale.CliHelp.TrackName} [/] ...", Locale.CliHelp.PlaySongFromSoundcloud);
        table.AddRow($"[grey]jammer[/] [green]soundcloud.com/{Locale.CliHelp.Username}/sets/{Locale.CliHelp.PlaylistName}[/] ...", Locale.CliHelp.PlaySongFromSoundcloudPlaylist);
        table.AddRow($"[grey]jammer[/] [green]youtube.com/watch?v=video-id[/] ...", Locale.CliHelp.PlaySongFromYoutube);
        /* table.AddRow("[grey]jammer[/] [green]playlist[/]", Locale.CliHelp.ShowPlaylistCommands); */
        table.AddRow($"[grey]jammer[/] [green]start[/]", Locale.CliHelp.OpenJammerFolder);
        table.AddRow($"[grey]jammer[/] [green]update[/]", Locale.CliHelp.AutoUpdateJammer);
        table.AddRow($"[grey]jammer[/] [green]-v[/][grey],[/][green] --version[/]", $"{Locale.CliHelp.ShowJammerVersion} [grey]" + Utils.version + "[/]");
        AnsiConsole.Write(table);

        PlaylistHelp();
    }

    static public void PlaylistHelp() {
        var table = new Table();
        table.AddColumn(Locale.CliHelp.PlaylistCommands);
        table.AddColumn(Locale.CliHelp.Description);

        table.AddRow($"[grey]jammer[/] [red]-p[/][grey],[/][red] --play  [/] <{Locale.CliHelp.Name}>", Locale.CliHelp.PlayPlaylist);
        table.AddRow($"[grey]jammer[/] [red]-c[/][grey],[/][red] --create[/] <{Locale.CliHelp.Name}>", Locale.CliHelp.CreatePlaylist);
        table.AddRow($"[grey]jammer[/] [red]-d[/][grey],[/][red] --delete[/] <{Locale.CliHelp.Name}>", Locale.CliHelp.DeletePlaylist);
        table.AddRow($"[grey]jammer[/] [red]-a[/][grey],[/][red] --add   [/] <{Locale.CliHelp.Name}> <song> ...", Locale.CliHelp.AddSongsToPlaylist);
        table.AddRow($"[grey]jammer[/] [red]-r[/][grey],[/][red] --remove[/] <{Locale.CliHelp.Name}> <{Locale.CliHelp.Song}> ...", Locale.CliHelp.RemoveSongsFromPlaylist);
        table.AddRow($"[grey]jammer[/] [red]-s[/][grey],[/][red] --show  [/] <{Locale.CliHelp.Name}>", Locale.CliHelp.ShowSongsInPlaylist);
        table.AddRow($"[grey]jammer[/] [red]-l[/][grey],[/][red] --list  [/] ", Locale.CliHelp.ListAllPlaylists);
        AnsiConsole.Write(table);
    }
    public static void Version() {
        AnsiConsole.MarkupLine($"[green]Jammer {Locale.Miscellaneous.Version} " + Utils.version + "[/]");
    }

    public static void EditKeyBindings(){
        string[] description = {
            Locale.Help.PlayPause,
            Locale.Help.Quit,
            Locale.Help.NextSong,
            Locale.Help.PreviousSong,
            Locale.Help.PlaySongs,
            Locale.Help.Forward,
            Locale.Help.Rewind,
            Locale.Help.VolumeUp,
            Locale.Help.VolumeDown,
            Locale.Help.ToggleShuffle,
            Locale.Help.SaveAs,
            Locale.Help.SavePlaylist,
            Locale.Help.ShufflePlaylist,
            Locale.Help.ToggleLooping,
            Locale.Help.ToggleShuffle,
            Locale.Help.ListAllPlaylists,
            Locale.Help.ToHideHelp,
            Locale.Help.ForSettings,
            Locale.LocaleKeybind.GoToSongStart,
            Locale.LocaleKeybind.GoToSongEnd,
            Locale.Help.ShowPlaylistOptions,
            Locale.LocaleKeybind.FOrwardSecAmount,
            Locale.LocaleKeybind.BackwardSecAmount,
            Locale.LocaleKeybind.ChangeVolume,
            Locale.LocaleKeybind.ToggleAutosave,
            Locale.LocaleKeybind.CurrentState, 
            Locale.Help.ShowCmdHelp,
            Locale.Help.DeleteCurrentSongFromPlaylist,
            Locale.Help.AddsongToPlaylist,
            Locale.Help.ListAllSongsInOtherPlaylist,
            Locale.CliHelp.ShowSongsInPlaylist,
            Locale.Help.PlayOtherPlaylist,
            Locale.Help.RedownloadCurrentSong,
            Locale.Help.EditKeybinds,
            Locale.Help.ChangeLanguage,
            Locale.Help.PlayRandomSong,
        };
        IniFileHandling.Create_KeyDataIni(false);
        // Construct description same way as in readalldata
        List<string> results = new();
        int maximum = 15;
        for(int i = 0; i < description.Length; i++){
            string keyValue = description[i];
            if(i >= IniFileHandling.ScrollIndexKeybind && results.Count != maximum){
                results.Add(keyValue);
            }
        }

        for(int i = 0; i < description.Length; i++){
            string keyValue = description[i];
            if(i < IniFileHandling.ScrollIndexKeybind && results.Count != maximum){
                results.Add(keyValue);
            }
        }
        description = results.ToArray();

        var table = new Table();
        table.AddColumn(Locale.LocaleKeybind.Description);
        table.AddColumn(Locale.LocaleKeybind.CurrentControl);
        string[] _elements = IniFileHandling.ReadAll_KeyData();

        // Counter to track the index for the description array
        int descIndex = 0;

        // Loop through the _elements array
        for(int i = 0; i < _elements.Length; i++) {
            // Ensure the description index stays within bounds
            if (descIndex <= description.Length) {
                // Check if the description at descIndex is not empty
                if (!string.IsNullOrEmpty(description[descIndex])) {
                    // Add row to the table
                    if(descIndex == 0){
                        table.AddRow("[red]"+description[descIndex]+"[/]", "[red]"+_elements[i]+"[/]");
                    } else {
                        table.AddRow(description[descIndex], _elements[i]);
                    }
                } else {
                    i++;
                }
                // Move to the next description index
                descIndex++;
            }
        }
        AnsiConsole.Write(table);
        if(IniFileHandling.EditingKeybind){
            string final = IniFileHandling.previousClick.ToString();
            if(IniFileHandling.isShiftCtrlAlt){
                final = "Shift + Ctrl + Alt + " + final;
            }
            else if(IniFileHandling.isShiftCtrl){
                final = "Shift + Ctrl + " + final;
            }
            else if(IniFileHandling.isShiftAlt){
                final = "Shift + Alt + " + final;
            }
            else if(IniFileHandling.isShift){
                final = "Shift + " + final;
            }
            else if(IniFileHandling.isCtrl){
                final = "Ctrl + " + final;
            }
            else if(IniFileHandling.isAlt){
                final = "Alt + " + final;
            }
            AnsiConsole.Markup($"[green]{Locale.LocaleKeybind.EditKeyBindMessage1}[/]\n");
            AnsiConsole.Markup($"{Locale.LocaleKeybind.EditKeyBindMessage2}\n");
            AnsiConsole.Markup($"[cyan]{final}[/]\n\n");

        } else {
            AnsiConsole.Markup($"[green]{Locale.LocaleKeybind.EditKeyBindMessage3}[/]\n"); // Press Enter to edit
            AnsiConsole.Markup($"[green]{Locale.LocaleKeybind.EditKeyBindMessage4}[/]\n");
        }
        DrawHelpSettingInfo();
    }
    public static void ChangeLanguage(){

        var table = new Table();
        table.AddColumn(Locale.LocaleKeybind.Description);
        string[] _elements = IniFileHandling.ReadAll_Locales();
        
        // Loop through the _elements array
        for(int i = 0; i < _elements.Length; i++) {
            if(i==0){
                table.AddRow("[red]"+_elements[i]+"[/]");
            } else {
                table.AddRow(_elements[i]);
            }
        }
        AnsiConsole.Write(table);
        AnsiConsole.Markup($"[green]{Locale.LocaleKeybind.ChangeLanguageMessage1}[/]\n");
        DrawHelpSettingInfo();
    }

}
