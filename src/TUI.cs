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
        if (Start.playerView == "help" || Start.playerView == "settings")
        {
            return;
        }
        var table = new Table();
        var controlsTable = new Table();

        if (Start.playerView == "default") {
            Comp_Normal(table);
        }
        else if (Start.playerView == "all") {
            Comp_Songs(table);
        }

        Comp_Controls(controlsTable);

        if (cls) {
            if (Start.playerView == "all") {
                cls = false;
                return;
            }
            AnsiConsole.Clear();
            cls = false;
        }
        if (Start.playerView == "default") {
            AnsiConsole.Cursor.SetPosition(0, 0);
        }
        
        // move cursor to top left
        AnsiConsole.Write(table);
        AnsiConsole.Write(controlsTable);

        AnsiConsole.Markup("Press [red]h[/] for help");
        AnsiConsole.Markup("\nPress [yellow]c[/] to hide settings");
        AnsiConsole.Markup("\nPress [green]f[/] to show playlist");
    }

    static public void ClearScreen() {
        cls = true;
    }

    static public string GetAllSongs() {
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
        string prevSong = "";
        string currentSong = "";
        string nextSong = "";

        if (Utils.currentSongIndex > 0) {
            prevSong = "[grey]previous : " + Utils.songs[Utils.currentSongIndex - 1] + "[/]";
        }
        else {
            prevSong = "[grey]previous : -[/]";
        }

        currentSong =  "[grey]current  : [/]" + Utils.songs[Utils.currentSongIndex];

        if (Utils.currentSongIndex < Utils.songs.Length - 1) {
            nextSong = "[grey]next     : " + Utils.songs[Utils.currentSongIndex + 1] + "[/]";
        }
        else {
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

        string? playlistInput = Console.ReadLine();
        if (playlistInput == "" || playlistInput == null) { return; }
        switch (playlistInput) {
            case "1": // add song to playlist
                AnsiConsole.Markup("\nEnter song to add to playlist: ");
                string? songToAdd = Console.ReadLine();
                if (songToAdd == "" || songToAdd == null) { break; }
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
                Console.ReadLine();
                break;
            case "4": // list all playlists
                Playlists.List();
                break;
            case "5": // play other playlist
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
        }
        AnsiConsole.Clear();
    }

    // "Components" of the TUI
    static public void Comp_Controls(Table table) {
        table.AddColumn("State");
        table.AddColumn("Current Position");
        table.AddColumn("Looping");
        table.AddColumn("Suffle");
        table.AddColumn("Volume");
        table.AddColumn("Muted");
        table.AddRow(Start.state + "", CalculateTime(Utils.MusicTimePlayed) + " / " + CalculateTime(Utils.currentMusicLength), Preferences.isLoop + "", Preferences.isShuffle + "", Math.Round(Preferences.volume * 100) + "%", Preferences.isMuted + "");
    }

    static public void Comp_Songs(Table table) {
        table.AddColumn("Jamming to: " + Utils.currentSong);
        table.AddRow("'playlist name.jammer'\n" + GetAllSongs());
    }

    static public void Comp_Normal(Table table) {
        table.AddColumns("Jamming to: " + Utils.currentSong);
        table.AddRow("'playlist name.jammer'\n" + GetPrevCurrentNextSong());

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
        table.AddRow("F2", "Show playlist options");

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
                        Playlists.List();
                        return;
                }
            }

            PlaylistHelp();
            return;
        }
    }

    static void PlaylistHelp() {
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
    public static void Help() {
        var table = new Table();
        table.AddColumn("Commands");
        table.AddColumn("Description");

        table.AddRow("[grey]jammer[/] <url> ...", "Play song(s) from url(s)");
        table.AddRow("[grey]jammer[/] <file> ...", "Play song(s) from file(s)");
        table.AddRow("[grey]jammer[/] [green]soundcloud[/] <url> ...", "Play song(s) from soundcloud url(s)");
        table.AddRow("[grey]jammer[/] [green]youtube[/] <url> ...", "Play song(s) from youtube url(s)");
        table.AddRow("[grey]jammer[/] [green]playlist[/]", "Show playlist commands");
        table.AddRow("[grey]jammer[/] [green]selfdestruct[/]", "Uninstall Jammer");
        
        AnsiConsole.Write(table);

        PlaylistHelp();
    }
}