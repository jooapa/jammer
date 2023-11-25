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
        next,
        previous
    }

    public partial class Start
    {
        //NOTE(ra) Starting state to playing. 
        // public static MainStates state = MainStates.idle;
        public static MainStates state = MainStates.play;
        public static bool drawOnce = false;
        private static Thread loopThread = new Thread(() => { });
        private static int consoleWidth = Console.WindowWidth;
        private static int consoleHeight = Console.WindowHeight;
        public static void Run(string[] args)
        {
            Preferences.CheckJammerFolderExists();
            Raylib.SetTraceLogLevel(TraceLogLevel.LOG_WARNING);
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
            // Console.WriteLine("Start playing");
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
            TUI.ClearScreen();
            while (true)
            {
                if (consoleWidth != Console.WindowWidth || consoleHeight != Console.WindowHeight) {
                    consoleHeight = Console.WindowHeight;
                    consoleWidth = Console.WindowWidth;
                    TUI.ClearScreen();        
                    TUI.DrawPlayer();
                }
                switch (state)
                {
                    case MainStates.idle:
                        TUI.ClearScreen();
                        CheckKeyboard();
                        //FIXME(ra) This is a workaround for screen to update once when entering the state.
                        if (drawOnce) {
                            TUI.DrawPlayer();
                            drawOnce = false;
                        }
                        break;
                    case MainStates.play:
                        Play.PlaySong();
                        TUI.ClearScreen();
                        TUI.DrawPlayer();
                        drawOnce = true;
                        Utils.MusicTimePlayed = 0;
                        state = MainStates.playing;
                        break;
                    case MainStates.playing:
                        Utils.preciseTime = Raylib.GetMusicTimePlayed(Utils.currentMusic);
                        //FIXME(ra) This is a workaround for screen to update once when entering the state.
                        if (drawOnce) {
                            TUI.DrawPlayer();
                            drawOnce = false;
                        }

                        // if song is finished, play next song
                        if (Utils.preciseTime >= Raylib.GetMusicTimeLength(Utils.currentMusic) - 0.1f)
                        {
                            Play.StopSong();
                            Play.MaybeNextSong();
                            Play.PlaySong();
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
                        Play.NextSong();
                        TUI.ClearScreen();
                        break;
                    case MainStates.previous:
                        Play.PrevSong();
                        TUI.ClearScreen();
                        break;
                }
            }
        }
    }
}