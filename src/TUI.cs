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
            mainTable.AddColumns(GetSongWithdots(Utils.currentSong, 90)).Width(100);
            mainTable.AddRow(songsTable.Centered().Width(100));
            songsTable.Border = TableBorder.Square;
            mainTable.AddRow(controlsTable.Centered());
            mainTable.AddRow(UIComponent_Time(timeTable, 75).Centered());
            // mainTable.Width(100);
            var helpTable = new Table();
            helpTable.AddColumn("[red]h[/] for help | [yellow]c[/] for settings | [green]f[/] for playlist");
            helpTable.Border = TableBorder.Rounded;
            
            mainTable.Border = TableBorder.HeavyEdge;
            mainTable.AddRow(helpTable.Centered());
            AnsiConsole.Write(mainTable);            

            // var debug = new Table();
            // debug.AddColumn("Debug");
            // debug.AddRow(Utils.preciseTime + " / " + Utils.audioStream.Length);
            // AnsiConsole.Write(debug);
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
        if (Utils.songs.Length == 0)
        {
            currentSong = "[grey]current  : -[/]";
        }
        else
        {
            currentSong = "[green]current  : " + GetSongWithdots(Utils.songs[Utils.currentSongIndex], 75) + "[/]";
        }

        if (Utils.currentSongIndex > 0)
        {
            prevSong = "[grey]previous : " + GetSongWithdots(Utils.songs[Utils.currentSongIndex - 1], 75) + "[/]";
        }
        else
        {
            prevSong = "[grey]previous : -[/]";
        }


        if (Utils.currentSongIndex < Utils.songs.Length - 1)
        {
            nextSong = "[grey]next     : " + GetSongWithdots(Utils.songs[Utils.currentSongIndex + 1], 75) + "[/]";
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
            case ConsoleKey.D1: // add song to playlist
                AnsiConsole.Markup("\nEnter song to add to playlist: ");
                string? songToAdd = Console.ReadLine();
                if (songToAdd == "" || songToAdd == null) { break; }
                // remove quotes from songToAdd
                songToAdd = songToAdd.Replace("\"", "");
                if (!isValidSong(songToAdd)) {
                    break;
                }
                songToAdd = Absolute.Correctify(new string[] { songToAdd })[0];
                Play.AddSong(songToAdd);
                Playlists.AutoSave();
                break;
            case ConsoleKey.D2: // delete current song from playlist
                Play.DeleteSong(Utils.currentSongIndex);
                Playlists.AutoSave();
                break;
            case ConsoleKey.D3: // show songs in playlist
                AnsiConsole.Markup("\nEnter playlist name: ");
                string? playlistNameToShow = Console.ReadLine();
                if (playlistNameToShow == "" || playlistNameToShow == null) { break; }
                // show songs in playlist
                Playlists.Show(playlistNameToShow);
                AnsiConsole.Markup("\nPress any key to continue");
                Console.ReadKey(true);
                break;
            case ConsoleKey.D4: // list all playlists
                Playlists.List();
                break;
            case ConsoleKey.D5: // play other playlist
                Playlists.ListOnly();
                AnsiConsole.Markup("\nEnter playlist name: ");
                string? playlistNameToPlay = Console.ReadLine();
                if (playlistNameToPlay == "" || playlistNameToPlay == null) { break; }
                // play other playlist
                Playlists.Play(playlistNameToPlay);
                break;
            case ConsoleKey.D6: // save/replace playlist
                AnsiConsole.Markup("\nEnter playlist name: ");
                string? playlistNameToSave = Console.ReadLine();
                if (playlistNameToSave == "" || playlistNameToSave == null) { break; }
                // save playlist
                Playlists.Save(playlistNameToSave);
                break;
            case ConsoleKey.D7: // goto song in playlist
                AnsiConsole.Markup("\nEnter song to goto: ");
                string? songToGoto = Console.ReadLine();
                if (songToGoto == "" || songToGoto == null) { break; }
                // songToGoto = GotoSong(songToGoto);
                break;
            case ConsoleKey.D8: // suffle playlist ( randomize )
                // get the name of the current song
                string currentSong = Utils.songs[Utils.currentSongIndex];
                // suffle playlist
                Play.Suffle();
                // delete duplicates
                Utils.songs = Utils.songs.Distinct().ToArray();

                Utils.currentSongIndex = Array.IndexOf(Utils.songs, currentSong);
                // set newsong from suffle to the current song
                Utils.currentSong = Utils.songs[Utils.currentSongIndex];
                Playlists.AutoSave();
                break;
            case ConsoleKey.D9: // play single song
                AnsiConsole.Markup("\nSeperate songs with space\n");
                AnsiConsole.Markup("Enter song(s) to play: ");
                string[]? songsToPlay = Console.ReadLine()?.Split(" ");
                if (songsToPlay == null) { break; }

                // if blank "  " remove
                songsToPlay = songsToPlay.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                // remove " from start and end of each song
                for (int i = 0; i < songsToPlay.Length; i++) {
                    songsToPlay[i] = songsToPlay[i].Replace("\"", "");
                    songsToPlay[i] = Absolute.ConvertToAbsolutePath(songsToPlay[i]);

                    // if doesnt exist, remove from array
                    if (!isValidSong(songsToPlay[i])) {
                        songsToPlay = songsToPlay.Take(i).Concat(songsToPlay.Skip(i + 1)).ToArray();
                        i--;
                    }
                }
                // if no songs left, break
                if (songsToPlay.Length == 0) { break; }
                
                Utils.songs = songsToPlay;
                Utils.currentSongIndex = 0;
                Utils.currentPlaylist = "";
                Play.StopSong();
                Play.PlaySong(Utils.songs, Utils.currentSongIndex);
                break;
            case ConsoleKey.D0: // exit
                break;
        }
        AnsiConsole.Clear();
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
        table.AddColumn("Looping");
        table.AddColumn("Suffle");
        table.AddColumn("Volume");
        string volume = Preferences.isMuted ? "[grey][strikethrough]" + Math.Round(Preferences.oldVolume * 100) + "%[/][/]" : Math.Round(Preferences.volume * 100) + "%";
        table.AddRow(Preferences.isLoop ? "[green]on[/]" : "[red]off[/]", Preferences.isShuffle ? "[green]on[/]" : "[red]off[/]", volume);
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
        table.AddColumns("Controls", "Description");
        table.AddRow("Space", "Play/Pause");
        table.AddRow("Q", "Quit");
        table.AddRow("Left", "Rewind 5 seconds");
        table.AddRow("Right", "Forward 5 seconds");
        table.AddRow("Up", "Volume up");
        table.AddRow("Down", "Volume down");
        table.AddRow("L", "Toggle looping");
        table.AddRow("M", "Toggle mute");
        table.AddRow("S", "Toggle shuffle");
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

        table.AddRow("Forward seconds", Preferences.forwardSeconds + " sec", "[green]1[/] to change");
        table.AddRow("Rewind seconds", Preferences.rewindSeconds + " sec", "[green]2[/] to change");
        table.AddRow("Change Volume by", Preferences.changeVolumeBy * 100 + " %", "[green]3[/] to change");
        table.AddRow("Auto Save", Preferences.isAutoSave + "", "[green]4[/] to toggle");

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

    public static void PlaylistCMD(string[] args){
        if (args[0] == "playlist")
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
                        return;
                    case "delete":
                        if (args.Length < 3)
                        {
                            AnsiConsole.WriteLine("No playlist name given");
                            return;
                        }
                        Playlists.Delete(args[2]);
                        return;
                    case "add":
                        if (args.Length < 4)
                        {
                            AnsiConsole.WriteLine("No playlist name or song given");
                            return;
                        }
                        Playlists.Add(args);
                        return;
                    case "remove":
                        if (args.Length < 4)
                        {
                            AnsiConsole.WriteLine("No playlist name or song given");
                            return;
                        }
                        Playlists.Remove(args);
                        return;
                    case "show":
                        if (args.Length < 3)
                        {
                            AnsiConsole.WriteLine("No playlist name given");
                            return;
                        }
                        Playlists.Show(args[2]);
                        return;
                    case "list":
                        Playlists.ListOnly();
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
        table.AddRow("[grey]jammer[/] [green]playlist[/]", "Show playlist commands");
        table.AddRow("[grey]jammer[/] [green]selfdestruct[/]", "Uninstall Jammer");
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

        table.AddRow("[grey]jammer[/] [red]playlist[/] [green]play [/]<name>", "Play playlist");
        table.AddRow("[grey]jammer[/] [red]playlist[/] [green]create [/]<name>", "Create playlist");
        table.AddRow("[grey]jammer[/] [red]playlist[/] [green]delete [/]<name>", "Delete playlist");
        table.AddRow("[grey]jammer[/] [red]playlist[/] [green]add [/]<name> <song> ...", "Add songs to playlist");
        table.AddRow("[grey]jammer[/] [red]playlist[/] [green]remove [/]<name> <song> ...", "Remove songs from playlist");
        table.AddRow("[grey]jammer[/] [red]playlist[/] [green]show [/]<name>", "Show songs in playlist");
        table.AddRow("[grey]jammer[/] [red]playlist[/] [green]list [/]", "List all playlists");

        AnsiConsole.Write(table);
    }
    public static void Version() {
        AnsiConsole.MarkupLine("[green]Jammer version " + Utils.version + "[/]");
    }
}
