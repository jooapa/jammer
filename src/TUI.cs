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
        var debugTable = new Table();
        
        table.AddColumn("Jamming to: " + Utils.currentSong);
        table.AddColumn("DEBUG ");
        table.AddRow("'playlist name.jammer'\n" + GetPrevCurrentNextSong(), "state: " + Start.state + "\n" + Math.Round(Utils.preciseTime, 2) +"||"+ Math.Round(Raylib.GetMusicTimeLength(Utils.currentMusic), 2));

        debugTable.AddColumn("State");
        debugTable.AddColumn("Current Position");
        debugTable.AddColumn("Looping");
        debugTable.AddColumn("Suffle");
        debugTable.AddColumn("Volume");
        debugTable.AddColumn("Muted");
        debugTable.AddRow(Start.state + "", CalculateTime(Utils.MusicTimePlayed) + " / " + CalculateTime(Utils.currentMusicLength), Preferences.isLoop + "", Preferences.isShuffle + "", Math.Round(Preferences.volume * 100) / 100 + "",  Preferences.isMuted + "");

        if (cls) {
            AnsiConsole.Clear();
            cls = false;
        }
        // move cursor to top left
        AnsiConsole.Cursor.SetPosition(0, 0);
        AnsiConsole.Write(table);
        AnsiConsole.Write(debugTable);
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
}