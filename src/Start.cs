using Spectre.Console;
using System.Threading;
using Raylib_cs;
using System.Text.RegularExpressions;
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

            // NOTE(ra): This is temporary
            Console.WriteLine("\n\nSpace to start playing the music");
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

        //
        // Main loop
        //
        static void Loop()
        {
            while (true)
            {

                if (Raylib.IsMusicReady(Utils.currentMusic)) {
                    Raylib.UpdateMusicStream(Utils.currentMusic);
                }

                // Get current music position
                if (Math.Floor(Raylib.GetMusicTimePlayed(Utils.currentMusic)) != Utils.prevMusicTimePlayed)  {
                    Console.WriteLine(Math.Floor(Raylib.GetMusicTimePlayed(Utils.currentMusic)));
                    Utils.prevMusicTimePlayed = Math.Floor(Raylib.GetMusicTimePlayed(Utils.currentMusic));
                }             
                
                // TODO(ra): Move keyhandling somewhere away from the main loop
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.Spacebar:
                            if (Raylib.IsMusicReady(Utils.currentMusic) && !Raylib.IsMusicStreamPlaying(Utils.currentMusic)) {
                                Play.PlaySong();
                            }
                            else if (Raylib.IsMusicStreamPlaying(Utils.currentMusic))
                            {
                                Console.WriteLine("Paused");
                                Play.PauseSong();
                            }
                            else
                            //NOTE(ra) Resumed is not called at all. PlaySong resumes after pause.
                            {
                                Console.WriteLine("Resumed");
                                Play.ResumeSong();
                            }
                            break;

                        case ConsoleKey.Q:
                            Console.WriteLine("Quit");
                            Environment.Exit(0);
                            break;
                    }
                }
            }
        }
    }
}