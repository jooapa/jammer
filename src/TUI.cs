using Spectre.Console;
using jammer;

static class TUI
{
    static bool cls = false;
    
    static public void InitScreen()
    {
        // Loading statusbar just for kicks
        AnsiConsole.Progress()
            .Start(ctx =>
            {
                var task1 = ctx.AddTask("[green]Loading[/]");

                while(!ctx.IsFinished)
                {
                    task1.Increment(5.5);
                    Thread.Sleep(1);
                }
            });
    }

    static public void DrawPlayer() {
        try {

            if (Start.playerView == "help" || Start.playerView == "settings")
            {
                return;
            }
            var table = new Table();
            var controlsTable = new Table();

            if (Start.playerView == "default") {
                UIComponent_Normal(table);
            }
            else if (Start.playerView == "all") {
                UIComponent_Songs(table);
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

            // move cursor to top left
            AnsiConsole.Write(table);
            AnsiConsole.Write(controlsTable);

            // var debug = new Table();
            // debug.AddColumn("Debug");
            // debug.AddRow(Utils.preciseTime + " / " + Utils.audioStream.Length);
            // AnsiConsole.Write(debug);

            AnsiConsole.Markup("Press [red]h[/] for help");
            AnsiConsole.Markup("\nPress [yellow]c[/] to show settings");
            AnsiConsole.Markup("\nPress [green]f[/] to show playlist\n");
        }
        catch (Exception e) {
            AnsiConsole.MarkupLine("[red]Error in DrawPlayer()[/]");
            AnsiConsole.MarkupLine("[red] Controls still work[/]");
            AnsiConsole.MarkupLine("[red]" + e.Message + "[/]");
        }
    }

    static public void ClearScreen() {
        cls = true;
    }

    static public string GetAllSongs() {
        if (Utils.songs.Length == 0) {
            return "[grey]No songs in playlist[/]";
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
            currentSong = "[green]current  : " + Utils.songs[Utils.currentSongIndex] + "[/]";
        }

        if (Utils.currentSongIndex > 0)
        {
            prevSong = "[grey]previous : " + Utils.songs[Utils.currentSongIndex - 1] + "[/]";
        }
        else
        {
            prevSong = "[grey]previous : -[/]";
        }


        if (Utils.currentSongIndex < Utils.songs.Length - 1)
        {
            nextSong = "[grey]next     : " + Utils.songs[Utils.currentSongIndex + 1] + "[/]";
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

        string? playlistInput = Console.ReadLine();
        if (playlistInput == "" || playlistInput == null) { return; }
        switch (playlistInput) {
            case "1": // add song to playlist
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
                break;
            case "2": // delete current song from playlist
                Play.DeleteSong(Utils.currentSongIndex);
                break;
            case "3": // show songs in playlist
                AnsiConsole.Markup("\nEnter playlist name: ");
                string? playlistNameToShow = Console.ReadLine();
                if (playlistNameToShow == "" || playlistNameToShow == null) { break; }
                // show songs in playlist
                Playlists.Show(playlistNameToShow);
                AnsiConsole.Markup("\nPress any key to continue");
                Console.ReadKey(true);
                break;
            case "4": // list all playlists
                Playlists.List();
                break;
            case "5": // play other playlist
                Playlists.ListOnly();
                AnsiConsole.Markup("\nEnter playlist name: ");
                string? playlistNameToPlay = Console.ReadLine();
                if (playlistNameToPlay == "" || playlistNameToPlay == null) { break; }
                // play other playlist
                Playlists.Play(playlistNameToPlay);
                break;
            case "6": // save/replace playlist
                AnsiConsole.Markup("\nEnter playlist name: ");
                string? playlistNameToSave = Console.ReadLine();
                if (playlistNameToSave == "" || playlistNameToSave == null) { break; }
                // save playlist
                Playlists.Save(playlistNameToSave);
                break;
            case "7": // goto song in playlist
                AnsiConsole.Markup("\nEnter song to goto: ");
                string? songToGoto = Console.ReadLine();
                if (songToGoto == "" || songToGoto == null) { break; }
                // songToGoto = GotoSong(songToGoto);
                break;
            case "8": // suffle playlist ( randomize )
                // get the name of the current song
                string currentSong = Utils.songs[Utils.currentSongIndex];
                // suffle playlist
                Play.Suffle();
                // delete duplicates
                Utils.songs = Utils.songs.Distinct().ToArray();

                Utils.currentSongIndex = Array.IndexOf(Utils.songs, currentSong);
                // set newsong from suffle to the current song
                Utils.currentSong = Utils.songs[Utils.currentSongIndex];
                break;
            case "9": // play single song
                AnsiConsole.Markup("\nSeperate songs with space\n");
                AnsiConsole.Markup("Enter song(s) to play: ");
                string[]? songsToPlay = Console.ReadLine()?.Split(" ");
                if (songsToPlay == null) { break; }
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
                Play.StopSong();
                Play.PlaySong(Utils.songs, Utils.currentSongIndex);
                break;

            case "0": // exit
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
        table.AddColumn("State");
        table.AddColumn("Current Position");
        table.AddColumn("Looping");
        table.AddColumn("Suffle");
        table.AddColumn("Volume");
        table.AddColumn("Muted");
        table.AddRow(Start.state + "", CalculateTime(Utils.MusicTimePlayed) + " / " + CalculateTime(Utils.currentMusicLength), Preferences.isLoop + "", Preferences.isShuffle + "", Math.Round(Preferences.volume * 100) + "%", Preferences.isMuted + "");
    }

    static public void UIComponent_Songs(Table table) {
        table.AddColumn("Jamming to: " + Utils.currentSong);
        if (Utils.currentPlaylist == "") {
            table.AddRow(GetAllSongs());
        } else {
            table.AddRow("'playlist " + Utils.currentPlaylist + ".jammer'\n" + GetAllSongs());
        }
    }

    static public void UIComponent_Normal(Table table) {
        table.AddColumns("Jamming to: " + Utils.currentSong);
        if (Utils.currentPlaylist == "") {
            table.AddRow(GetPrevCurrentNextSong());
        } else {
            table.AddRow("'playlist " + Utils.currentPlaylist + ".jammer'\n" + GetPrevCurrentNextSong());
        }
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
