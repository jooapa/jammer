using Spectre.Console;
using System.Threading;
using jammer;

static class TUI
{
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

    static public void AskForUrl() {
        var name = AnsiConsole.Ask<string>("Enter URL?");
        Download.DownloadSoundCloudTrackAsync(name).Wait();
    }

    static public void Draw() {
        while (true) {
            var table = new Table();
            table.AddColumn("♫ Jamming to: " + Utils.currentSong);
            table.AddRow("Playlist: " + GetAllSongs());

            
            // move cursor to top left
            // AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Clear();
            AnsiConsole.Write(table);             
        }
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
}