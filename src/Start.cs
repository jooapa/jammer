using Spectre.Console;
using System.Threading;
using Raylib_cs;
using System.Text.RegularExpressions;
using System.Collections;
using System.ComponentModel.Design;
namespace jammer
{
    //NOTES(ra) A way to fix the drawonce - prevState

    // idle - the program wait for user input. Song is not played
    // play - Start playing - Play.PlaySong
    // playing - The music is playing. Update screen once a second or if a
    // button is pressed
    // pause - Pause song, returns to idle state

    public enum MainStates
    {
        idle,
        play,
        playing,
        pause,
        stop,
        next
    }

    public class Start
    {
        //NOTE(ra) Starting state to playing. 
        // public static MainStates state = MainStates.idle;
        public static MainStates state = MainStates.play;
        public static bool drawOnce = false;
        private static Thread loopThread = new Thread(() => { });

        public static void Run(string[] args)
        {
            Utils.songs = args;
            // Turns relative paths into absolute paths, and adds https:// to urls
            Absolute.Correctify(Utils.songs);
            // if no args, ask for input
            if (Utils.songs.Length == 0) {
                AnsiConsole.MarkupLine("[red]No arguments given, please enter a URL or file path[/]");
            }
            // Play.InitAudio();
            StartPlaying();
            // NOTE(ra): This is for testing purposes.
            Console.WriteLine("\n\nSpace to start playing the music");
        }

        public static void StartPlaying()
        {
            Console.WriteLine("Start playing");
            Play.PlaySong(Utils.songs, Utils.currentSongIndex);
            // new thread for playing music
            loopThread = new Thread(() => {
                Loop();
            });
            loopThread.Start();
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
                        //FIXME(ra) This is a workaround for screen to update once when entering the state.
                        if (drawOnce) {
                            TUI.DrawPlayer();
                            drawOnce = false;
                        }
                        break;
                    case MainStates.play:
                        Play.PlaySong();
                        TUI.DrawPlayer();
                        drawOnce = true;
                        Utils.MusicTimePlayed = 0;
                        state = MainStates.playing;
                        break;
                    case MainStates.playing:
                        //FIXME(ra) This is a workaround for screen to update once when entering the state.
                        if (drawOnce) {
                            TUI.DrawPlayer();
                            drawOnce = false;
                        }
                        
                        if (Raylib.IsMusicReady(Utils.currentMusic))
                        {
                            Raylib.UpdateMusicStream(Utils.currentMusic);
                        }

                        // Draw once a second
                        // TODO(ra) Move this somwhere. TUI-draw, where?
                        if (Math.Floor(Raylib.GetMusicTimePlayed(Utils.currentMusic)) != Utils.MusicTimePlayed)
                        {
                            Utils.MusicTimePlayed = Math.Floor(Raylib.GetMusicTimePlayed(Utils.currentMusic));
                            // Utils.temp =Raylib.GetMusicTimePlayed(Utils.currentMusic);
                            TUI.DrawPlayer();
                        }
                        CheckKeyboard();
                        break;
                    case MainStates.pause:
                        Play.PauseSong();
                        state = MainStates.idle;
                        break;
                    case MainStates.stop:
                        Play.StopSong();
                        state = MainStates.idle;
                        break;
                    case MainStates.next:
                        // Play.StopSong();
                        // Play.ResetMusic();
                        Raylib.StopMusicStream(Utils.currentMusic);
                        Raylib.UnloadMusicStream(Utils.currentMusic);
                        Play.NextSong();
                        Play.PlaySong(Utils.songs, Utils.currentSongIndex);
                        state = MainStates.play;
                        break;


                }
                // if music at the end of the song, play next song
                // if (state == MainStates.playing && Raylib.GetMusicTimePlayed(Utils.currentMusic) >= Utils.currentMusicLength)
                // {
                //     Play.NextSong();
                // }
            }

            // // Clean up
            // Play.ResetMusic();
            // Utils.mainLoop = true;
            // Run(Utils.songs);
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
                            drawOnce = true;
                        }
                        else if (Raylib.IsMusicStreamPlaying(Utils.currentMusic))
                        {
                            Console.WriteLine("Paused");
                            state = MainStates.pause;
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
                    case ConsoleKey.RightArrow:
                        state = MainStates.next;
                        break;
                    case ConsoleKey.LeftArrow:
                        Play.PrevSong();
                        break;
                }
            }
        }
    }
}