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
                    task1.Increment(1.5);
                    Thread.Sleep(5);
                }
            });
    }

    static public void AskForUrl() {
        var name = AnsiConsole.Ask<string>("Enter URL?");
        Download.DownloadSoundCloudTrackAsync(name).Wait();
    }
}