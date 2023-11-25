using Spectre.Console;
using System.Threading;
using Raylib_cs;
namespace jammer
{
    public class Start
    {   
        public static void Run(string[] args)
        {
            Utils.songs = args;
            // Turns relative paths into absolute paths, and adds https:// to urls
            Absolute.Correctify(Utils.songs);

            // if no args, ask for input
            if (Utils.songs.Length == 0)
            {
                AnsiConsole.MarkupLine("[red]No arguments given, please enter a URL or file path[/]");
            }

            StartPlaying();

        }

        public static void StartPlaying()
        {
            // new thread for drawing TUI
            // var tuiThread = new Thread(() => {
            //     TUI.Draw();
            // });
            // tuiThread.Start();

            Play.PlaySong(Utils.songs, Utils.currentSongIndex);
            new Thread(() => {
                Loop();
            }).Start();
        }

        static void Loop()
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.Spacebar:
                            if (Raylib.IsSoundPlaying(Utils.currentSound))
                            {
                                Play.PauseSong();
                            }
                            else
                            {
                                Play.ResumeSong();
                            }
                            break;
                    }
                }
            }
        }
    }
}