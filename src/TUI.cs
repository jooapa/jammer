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
            helpTable.AddColumn("[red]h[/] for help | [yellow]c[/] for settings | [green]f[/] for playlist");
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
            AnsiConsole.MarkupLine("[red]Error in Drawing the player[/]");
            AnsiConsole.MarkupLine("[red] Controls still work[/]");
            AnsiConsole.MarkupLine("[red]" + e.Message + "[/]");
        }
    }

    static public void ClearScreen() {
        cls = true;
    }

    static public string GetAllSongs() {
        if (Utils.songs.Length == 0) {
            return "[grey]No songs in 'on' playlist[/]";
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
            currentSong = "[grey]current  : -[/]";
        }
        else
        {
            currentSong = "[green]current  : " + GetSongWithdots(Utils.songs[Utils.currentSongIndex], songLength) + "[/]";
        }

        if (Utils.currentSongIndex > 0)
        {
            prevSong = "[grey]previous : " + GetSongWithdots(Utils.songs[Utils.currentSongIndex - 1], songLength) + "[/]";
        }
        else
        {
            prevSong = "[grey]previous : -[/]";
        }


        if (Utils.currentSongIndex < Utils.songs.Length - 1)
        {
            nextSong = "[grey]next     : " + GetSongWithdots(Utils.songs[Utils.currentSongIndex + 1], songLength) + "[/]";
        }
        else
        {
            nextSong = "[grey]next     : -[/]";
        }

        return prevSong + "\n[green]" + currentSong + "[/]\n" + nextSong;
    }

    static public string CalculateTime(double time) {
        int minutes = (int)time / 60;
        int seconds = (int)time % 60;
        string timeString = $"{minutes}:{seconds:D2}";
        return timeString;
    }

    public static void PlaylistInput() {
        AnsiConsole.Markup("\nEnter playlist command: \n");
        AnsiConsole.MarkupLine("[grey]1. add song to playlist[/]");
        AnsiConsole.MarkupLine("[grey]2. delete song current song from playlist[/]");
        AnsiConsole.MarkupLine("[grey]3. show songs in other playlist[/]");
        AnsiConsole.MarkupLine("[grey]4. list all playlists[/]");
        AnsiConsole.MarkupLine("[grey]5. play other playlist[/]");
        AnsiConsole.MarkupLine("[grey]6. save/replace playlist[/]");
        // AnsiConsole.MarkupLine("[grey]7. goto song in playlist[/]");
        AnsiConsole.MarkupLine("[grey]8. suffle playlist[/]");
        AnsiConsole.MarkupLine("[grey]9. play song(s)[/]");
        AnsiConsole.MarkupLine("[grey]0. exit[/]");

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
        string songToAdd = Message.Input("Enter song to add to playlist:", "Add song to playlist");
        if (songToAdd == "" || songToAdd == null) {
            Message.Data("Error: Add song to playlist", "no song given", true);
            return;
        }
        // remove quotes from songToAdd
        songToAdd = songToAdd.Replace("\"", "");
        if (!isValidSong(songToAdd)) {
            Message.Data("Error: " + songToAdd, "invalid song: Make sure you typed it correctly", true);
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
        string? playlistNameToShow = Message.Input("Enter playlist name:", "Show songs in playlist");
        if (playlistNameToShow == "" || playlistNameToShow == null) { 
            Message.Data("Error: Show songs in playlist", "no playlist given", true);
            return;
        }
        AnsiConsole.Clear();
        // show songs in playlist
        Message.Data(Playlists.GetShow(playlistNameToShow), "Songs in playlist " + playlistNameToShow);
    }

    // List all playlists
    public static void ListAllPlaylists()
    {
        Message.Data(Playlists.GetList(), "All playlists");
    }

    // Play other playlist
    public static void PlayOtherPlaylist()
    {
        string? playlistNameToPlay = Message.Input("Enter playlist name:", "Play other playlist");
        if (playlistNameToPlay == "" || playlistNameToPlay == null) { 
            Message.Data("Error: Play other playlist", "no playlist given", true);
            return;
        }

        // play other playlist
        Playlists.Play(playlistNameToPlay);
    }

    // Save/replace playlist
    public static void SaveReplacePlaylist()
    {
        string playlistNameToSave = Message.Input("Enter playlist name:", "Save/Replace playlist");
        if (playlistNameToSave == "" || playlistNameToSave == null) {
            Message.Data("Error: Save/Replace playlist", "no playlist given", true);
            return;
        }
        // save playlist
        Playlists.Save(playlistNameToSave);
    }

    public static void SaveCurrentPlaylist()
    {
        if (Utils.currentPlaylist == "") {
            Message.Data("Error: Save playlist", "no playlist given", true);
            return;
        }
        // save playlist
        Playlists.Save(Utils.currentPlaylist, true);
    }

    public static void SaveAsPlaylist()
    {
        string playlistNameToSave = Message.Input("Enter playlist name:", "Save as playlist");
        if (playlistNameToSave == "" || playlistNameToSave == null) {
            Message.Data("Error: Save as playlist", "no playlist given", true);
            return;
        }
        // save playlist
        Playlists.Save(playlistNameToSave);
    }

    // Goto song in playlist
    public static void GotoSongInPlaylist()
    {
        string songToGoto = Message.Input("Enter song to goto:", "Goto song in playlist");
        if (songToGoto == "" || songToGoto == null) {
            Message.Data("Error: Goto song in playlist", "no song given", true);
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
        string[]? songsToPlay = Message.Input("Enter song(s) to play:", "Play song(s) | Separate songs with space").Split(" ");
        
        if (songsToPlay == null || songsToPlay.Length == 0) {
            Message.Data("Error: Play song(s)", "no song(s) given", true);
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

    public static bool isValidSong(string song) {
        if (File.Exists(song) || URL.IsUrl(song) || Directory.Exists(song)) {
            AnsiConsole.Markup("\n[green]Valid song[/]");
            return true;
        }
        AnsiConsole.Markup("\n[red]Invalid song[/]");
        return false;
    }

    // "Components" of the TUI
    static public void UIComponent_Controls(Table table) {
        table.AddColumn("State");
        table.AddColumn("Looping");
        table.AddColumn("Suffle");
        table.AddColumn("Volume");
        string volume = Preferences.isMuted ? "[grey][strikethrough]" + Math.Round(Preferences.oldVolume * 100) + "%[/][/]" : Math.Round(Preferences.volume * 100) + "%";
        table.AddRow(Start.state.ToString(), Preferences.isLoop ? "[green]on[/]" : "[red]off[/]", Preferences.isShuffle ? "[green]on[/]" : "[red]off[/]", volume);
    }

    static public void UIComponent_Songs(Table table) {
        if (Utils.currentPlaylist == "") {
            table.AddColumn(GetAllSongs());
        } else {
            table.AddColumn("playlist [cyan]" + Utils.currentPlaylist + "[/]");
            table.AddRow(GetAllSongs());
        }
    }

    static public void UIComponent_Normal(Table table) {
        if (Utils.currentPlaylist == "") {
            table.AddColumn(GetPrevCurrentNextSong());
        } else {
            table.AddColumn("playlist [cyan]" + Utils.currentPlaylist + "[/]");
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

    public static void DrawAllSongsView() {
        AnsiConsole.Clear();
        var table = new Table();
        UIComponent_Songs(table);
        AnsiConsole.Write(table);
        AnsiConsole.Markup("Press [red]h[/] for help");
        AnsiConsole.Markup("\nPress [yellow]c[/] to show settings");
        AnsiConsole.Markup("\nPress [green]f[/] to show playlist");
        AnsiConsole.WriteLine("\n");
    }

    static public void DrawHelp() {
        var table = new Table();
        table.AddColumns("Controls", "Description",      "Mod Controls", "Description");
        table.AddRow("Space", "Play/Pause",              "[green1]shift[/] + [cyan1]A[/]", "Add song to playlist");
        table.AddRow("Q", "Quit",                        "[green1]shift[/] + [cyan1]D[/]", "List all songs in other playlist");
        table.AddRow("Left", "Rewind 5 seconds",         "[green1]shift[/] + [cyan1]F[/]", "List all playlists");
        table.AddRow("Right", "Forward 5 seconds",       "[green1]shift[/] + [cyan1]O[/]", "Play other playlist");
        table.AddRow("Up", "Volume up",                  "[green1]shift[/] + [cyan1]S[/]", "Save playlist");
        table.AddRow("Down", "Volume down",              "[green1]shift[/] + [turquoise2]alt[/] + [cyan1]S[/]", "Save as");
        table.AddRow("L", "Toggle looping",              "[turquoise2]alt[/] + [cyan1]S[/]", "Suffle playlist");
        table.AddRow("M", "Toggle mute",                 "[green1]shift[/] + [cyan1]P[/]", "Play song(s)");
        table.AddRow("S", "Toggle shuffle",              "[green1]shift[/] + [cyan1]B[/]", "Redownload current song");
        table.AddRow("Playlist", "");
        table.AddRow("P", "Previous song");
        table.AddRow("N", "Next song");
        table.AddRow("R", "Play random song");
        table.AddRow("Delete", "Delete current song from playlist");
        table.AddRow("F2", "Show playlist options");
        table.AddRow("tab", "Show cmd Help");

        AnsiConsole.Clear();
        AnsiConsole.Write(table);
        AnsiConsole.Markup("Press [red]h[/] to hide help");
        AnsiConsole.Markup("\nPress [yellow]c[/] for settings");
        AnsiConsole.Markup("\nPress [green]f[/] to show playlist");
    }
    static public void DrawSettings() {
        var table = new Table();

        table.AddColumns("Settings", "Value", "Change Value");

        table.AddRow("Forward seconds", Preferences.forwardSeconds + " sec", "[green]ctrl + 1[/] to change");
        table.AddRow("Rewind seconds", Preferences.rewindSeconds + " sec", "[green]ctrl + 2[/] to change");
        table.AddRow("Change Volume by", Preferences.changeVolumeBy * 100 + " %", "[green]ctrl + 3[/] to change");
        table.AddRow("Auto Save", Preferences.isAutoSave + "", "[green]ctrl + 4[/] to toggle");

        AnsiConsole.Clear();
        AnsiConsole.Write(table);
        AnsiConsole.Markup("Press [red]h[/] for help");
        AnsiConsole.Markup("\nPress [yellow]c[/] to hide settings");
        AnsiConsole.Markup("\nPress [green]f[/] to show playlist");
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
    }

    public static void PlaylistCli(string[] args){
        if (args[0] == "playlist" || args[0] == "pl") 
        {
            if (args.Length < 2)
            {
                AnsiConsole.WriteLine("No playlist command given");
            }
            else
            {
                switch (args[1])
                {
                    case "play":
                        if (args.Length < 3)
                        {
                            AnsiConsole.WriteLine("No playlist name given");
                            return;
                        }
                        Playlists.Play(args[2]);
                        return;
                    case "create":
                        if (args.Length < 3)
                        {
                            AnsiConsole.WriteLine("No playlist name given");
                            return;
                        }
                        Playlists.Create(args[2]);
                        Environment.Exit(0);
                        return;
                    case "delete":
                        if (args.Length < 3)
                        {
                            AnsiConsole.WriteLine("No playlist name given");
                            return;
                        }
                        Playlists.Delete(args[2]);
                        Environment.Exit(0);
                        return;
                    case "add":
                        if (args.Length < 4)
                        {
                            AnsiConsole.WriteLine("No playlist name or song given");
                            return;
                        }
                        Playlists.Add(args);
                        Environment.Exit(0);
                        return;
                    case "remove":
                        if (args.Length < 4)
                        {
                            AnsiConsole.WriteLine("No playlist name or song given");
                            return;
                        }
                        Playlists.Remove(args);
                        Environment.Exit(0);
                        return;
                    case "show":
                        if (args.Length < 3)
                        {
                            AnsiConsole.WriteLine("No playlist name given");
                            return;
                        }
                        Playlists.ShowCli(args[2]);
                        Environment.Exit(0);
                        return;
                    case "list":
                        Playlists.PrintList();
                        Environment.Exit(0);
                        return;
                }
            }

            PlaylistHelp();
            return;
        }
    }

    public static void Help() {
        var table = new Table();
        table.AddColumn("Commands");
        table.AddColumn("Description");

        table.AddRow("[grey]jammer[/] <url> ...", "Play song(s) from url(s)");
        table.AddRow("[grey]jammer[/] <file> ...", "Play song(s) from file(s)");
        table.AddRow("[grey]jammer[/] [green]soundcloud.com/username/track-name [/] ...", "Play song(s) from soundcloud url(s)");
        table.AddRow("[grey]jammer[/] [green]soundcloud.com/username/sets/playlist-name[/] ...", "Play song(s) from soundcloud playlist url(s)");
        table.AddRow("[grey]jammer[/] [green]youtube.com/watch?v=video-id[/] ...", "Play song(s) from youtube url(s)");
        /* table.AddRow("[grey]jammer[/] [green]playlist[/]", "Show playlist commands"); */
        table.AddRow("[grey]jammer[/] [green]start[/]", "Open Jammer folder");
        table.AddRow("[grey]jammer[/] [green]update[/]", "Auto Update Jammer");
        table.AddRow("[grey]jammer[/] [green]version[/]", "Show Jammer version [grey]" + Utils.version + "[/]");

        AnsiConsole.Write(table);

        PlaylistHelp();
    }

    static public void PlaylistHelp() {
        var table = new Table();
        table.AddColumn("Playlist Commands");
        table.AddColumn("Description");

        table.AddRow("[grey]jammer[/] [red]-p|--play[/] <name>", "Play playlist");
        table.AddRow("[grey]jammer[/] [red]-c|--create[/] <name>", "Create playlist");
        table.AddRow("[grey]jammer[/] [red]-d|--delete[/] <name>", "Delete playlist");
        table.AddRow("[grey]jammer[/] [red]-a|--add[/] <name> <song> ...", "Add songs to playlist");
        table.AddRow("[grey]jammer[/] [red]-r|--remove[/] <name> <song> ...", "Remove songs from playlist");
        table.AddRow("[grey]jammer[/] [red]-s|--show[/] <name>", "Show songs in playlist");
        table.AddRow("[grey]jammer[/] [red]-l|--list[/] ", "List all playlists");

        AnsiConsole.Write(table);
    }
    public static void Version() {
        AnsiConsole.MarkupLine("[green]Jammer version " + Utils.version + "[/]");
    }
}
