using Spectre.Console;
using System.Threading;
using Raylib_cs;
using System.Text.RegularExpressions;
using System.Collections;
using System.ComponentModel.Design;
namespace jammer
{
    public enum MainStates
    {
        playing,
        paused,
        idle
    }

    public class Start
    {
        public static MainStates state = MainStates.idle;
        private static bool drawOnce = false;

        public static void Run(string[] args)
        {
            Utils.songs = args;
            // Turns relative paths into absolute paths, and adds https:// to urls
            Absolute.Correctify(Utils.songs);
            // if no args, ask for input
            if (Utils.songs.Length == 0) {
                AnsiConsole.MarkupLine("[red]No arguments given, please enter a URL or file path[/]");
            }
            StartPlaying();
            // NOTE(ra): This is for testing purposes.
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
            new Thread(() =>
            {
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
                switch (state)
                {
                    case MainStates.idle:
                        CheckKeyboard();
                        // Draw player once
                        if (drawOnce) {
                            TUI.DrawPlayer();
                            drawOnce = false;
                        }
                        break;
                    case MainStates.playing:
                        if (Raylib.IsMusicReady(Utils.currentMusic))
                        {
                            Raylib.UpdateMusicStream(Utils.currentMusic);
                        }

                        // Draw once a second
                        // TODO(ra) Move this somwhere. TUI-draw, where?
                        if (Math.Floor(Raylib.GetMusicTimePlayed(Utils.currentMusic)) != Utils.prevMusicTimePlayed)
                        {
                            TUI.DrawPlayer();
                            Utils.prevMusicTimePlayed = Math.Floor(Raylib.GetMusicTimePlayed(Utils.currentMusic));
                        }
                        CheckKeyboard();
                        break;
                    case MainStates.paused:
                        Play.PauseSong();
                        state = MainStates.idle;
                        break;
                }
            }
        }

        public static void CheckKeyboard()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.Spacebar:
                        if (Raylib.IsMusicReady(Utils.currentMusic) && !Raylib.IsMusicStreamPlaying(Utils.currentMusic))
                        {
                            Play.PlaySong();
                            state = MainStates.playing;
                        }
                        else if (Raylib.IsMusicStreamPlaying(Utils.currentMusic))
                        {
                            Console.WriteLine("Paused");
                            state = MainStates.paused;
                            drawOnce = true;
                        }
                        else
                        //NOTE(ra) Resumed is not called at all. PlaySong resumes after pause.
                        {
                            Console.WriteLine("Resumed");
                            Play.ResumeSong();
                        }
                        break;
                    case ConsoleKey.F12:
                        Console.WriteLine("CurrentState: " + state);
                        break;

                    case ConsoleKey.Q:
                        Console.WriteLine("Quit");
                        Environment.Exit(0);
                        break;

                    case ConsoleKey.Escape:
                        Console.WriteLine("Quit");
                        Environment.Exit(0);
                        break;
                }
            }
        }
    }
}