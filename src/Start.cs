using Spectre.Console;
using NAudio.Wave;
using AngleSharp.Common;
using System.Runtime.InteropServices;

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
        public static MainStates state = MainStates.playing;
        public static bool drawOnce = false;
        private static Thread loopThread = new Thread(() => { });
        private static int consoleWidth = Console.WindowWidth;
        private static int consoleHeight = Console.WindowHeight;
        public static double lastSeconds = 0;
        //  
        // Run
        //
        public static void Run(string[] args)
        {
            for (int i=0; i < args.Length; i++) {
                if (args[i] == "-d") {
                    Utils.isDebug = true;
                    Debug.dprint("--- Started ---");
                }
            }

            Preferences.CheckJammerFolderExists();
            Utils.songs = args;
            // Turns relative paths into absolute paths, and adds https:// to urls
            Utils.songs = Absolute.Correctify(Utils.songs);
            // if no args, ask for input
            if (Utils.songs.Length == 0) {
                AnsiConsole.MarkupLine("[red]No arguments given, please enter a URL or file path[/]");
                Utils.songs = new string[1];
                Utils.songs[0] = AnsiConsole.Ask<string>("Enter URL or file path");
            }
            // Play.InitAudio();
            StartPlaying();
            // NOTE(ra): This is for testing purposes.
            TUI.ClearScreen();
        }


        //
        // StartPlaying
        //
        public static void StartPlaying()
        {
            // new thread for playing music
            // loopThread = new Thread(() => {
            //     Loop();
            // });
            // loopThread.Start();
            Play.PlaySong(Utils.songs, Utils.currentSongIndex);
        }

        //
        // Main loop
        //
        public static void Loop()
        {
            if (Utils.audioStream == null || Utils.currentMusic == null) {
                return;
            }

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
                        // get current time 
                        Utils.preciseTime = Utils.audioStream.Position;
                        // get current time in seconds
                        Utils.MusicTimePlayed = Utils.audioStream.Position / Utils.audioStream.WaveFormat.AverageBytesPerSecond;
                        // get whole song length in seconds
                        Utils.currentMusicLength = Utils.audioStream.Length / Utils.audioStream.WaveFormat.AverageBytesPerSecond;

                        //FIXME(ra) This is a workaround for screen to update once when entering the state.
                        if (drawOnce) {
                            TUI.DrawPlayer();
                            drawOnce = false;
                        }

                        // if song is finished, play next song
                        if (Utils.MusicTimePlayed >= Utils.currentMusicLength)
                        {
                            Play.StopSong();
                            Play.MaybeNextSong();
                        }

                        // every second, update screen
                        if (Utils.MusicTimePlayed >= lastSeconds + 1)
                        {
                            lastSeconds = Utils.MusicTimePlayed;
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
                        Debug.dprint("next");
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