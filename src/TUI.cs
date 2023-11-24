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

    static public async Task Draw() {
        var table = new Table();

        table.AddColumn("Foo");
        await AnsiConsole.Live(table)
            .StartAsync(async ctx => 
            {
                AnsiConsole.Clear();
                ctx.Refresh();
                await Task.Delay(1000);

        

            });

        // tableJam.AddColumn("♫ Jamming to: Kukkaruukku ♫");
            // tableJam.AddRow("haloo");
        
        // AnsiConsole.Write(tableJam);
    }
}