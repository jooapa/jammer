using Spectre.Console;
using jammer;
using Raylib_cs;
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
            if (Start.playerView == "default") {
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
    }

    // "Components" of the TUI
    static public void Comp_Controls(Table table) {
        table.AddColumn("State");
        table.AddColumn("Current Position");
        table.AddColumn("Looping");
        table.AddColumn("Suffle");
        table.AddColumn("Volume");
        table.AddColumn("Muted");
        table.AddRow(Start.state + "", CalculateTime(Utils.MusicTimePlayed) + " / " + CalculateTime(Utils.currentMusicLength), Preferences.isLoop + "", Preferences.isShuffle + "", Math.Round(Preferences.volume * 100) / 100 + "", Preferences.isMuted + "");
    }

    static public void Comp_Songs(Table table) {
        table.AddColumn("Jamming to: " + Utils.currentSong);
        table.AddRow("'playlist name.jammer'\n" + GetAllSongs());
    }

    static public void Comp_Normal(Table table) {
        table.AddColumn("Jamming to: " + Utils.currentSong);
        table.AddRow("'playlist name.jammer'\n" + GetPrevCurrentNextSong());
    }
}