using Spectre.Console;
using System.Threading;
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
        var table = new Table();
        table.AddColumn("Jamming to: " + Utils.currentSong);
        table.AddColumn("DEBUG ");
        table.AddRow("Playlist:\n" + GetAllSongs(), "state: " + Start.state);
        // table.AddRow(Play.GetTimeFormat());
        table.AddRow("Time: " + CalculateTime(Utils.MusicTimePlayed) + " / " + CalculateTime(Utils.currentMusicLength));


        if (cls) {
            AnsiConsole.Clear();
            cls = false;
        }
        // move cursor to top left
        AnsiConsole.Cursor.SetPosition(0, 0);
        AnsiConsole.Write(table);             
    }

    static public void ClearScreen() {
        cls = true;
    }

    static public string GetAllSongs() {
        string allSongs = "";
        foreach (string song in Utils.songs) {
            allSongs += song + "\n";
        }
        // remove last newline
        allSongs = allSongs.Substring(0, allSongs.Length - 1);
        return allSongs;
    }

    static public string CalculateTime(double time) {
        int minutes = (int)time / 60;
        int seconds = (int)time % 60;
        string timeString = $"{minutes}:{seconds:D2}";
        return timeString;
    }
}