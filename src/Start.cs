using Spectre.Console;
using NAudio.Wave;
using AngleSharp.Common;
using System.Runtime.InteropServices;
using System;

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
        public static double lastSeconds = -1;
        public static double lastPlaybackTime = -1;
        public static double treshhold = 1;
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
                if (args[i] == "-help" || args[i] == "-h" || args[i] == "--help" || args[i] == "--h" || args[i] == "-?" || args[i] == "?" || args[i] == "help") {
                    TUI.ClearScreen();
                    TUI.Help();
                    return;
                }
                if (args[i] == "-v" || args[i] == "--version" || args[i] == "version") {
                    TUI.ClearScreen();
                    TUI.Version();
                    return;
                }
            }

            if (args.Length != 0) {
                
                if (args[0] == "playlist") {
                    TUI.ClearScreen();
                    TUI.PlaylistCMD(args);
                    return;
                }
                if (args[0] == "selfdestruct") {
                    var assembly = System.Reflection.Assembly.GetEntryAssembly();
                    if (assembly != null)
                    {
                        AnsiConsole.MarkupLine("[red]Selfdestructing Jammer...[/]");
                        // run the uninstaller selfdestruct.bat
                        string? path = Path.GetDirectoryName(assembly.Location);
                        System.Diagnostics.Process.Start(path + "/selfdestruct.bat");
                        Environment.Exit(0);
                    }
                }
                if (args[0] == "start") {
                    // open explorer in jammer folder
                    AnsiConsole.MarkupLine("[green]Opening Jammer folder...[/]");
                    System.Diagnostics.Process.Start("explorer.exe", Utils.jammerPath);
                    return;
                }
            }

            Preferences.CheckJammerFolderExists();
            Utils.songs = args;
            // Turns relative paths into absolute paths, and adds https:// to urls
            Utils.songs = Absolute.Correctify(Utils.songs);

            StartPlaying();
            TUI.ClearScreen();
        }


        //
        // StartPlaying
        //
        public static void StartPlaying()
        {
            Play.PlaySong(Utils.songs, Utils.currentSongIndex);
        }

        //
        // Main loop
        //
        public static void Loop()
        {
            lastSeconds = -1;
            treshhold = 1;
            if (Utils.audioStream == null || Utils.currentMusic == null) {
                return;
            }

            TUI.ClearScreen();
            drawOnce = true;
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
                        if (Utils.preciseTime == lastPlaybackTime)
                        {
                            // If the time hasn't changed, it might be near the end of the song
                            // Check if it's close to the end and play the next song
                            if (Utils.preciseTime >= Utils.audioStream.Length - treshhold)
                            {
                                Play.MaybeNextSong();
                            }
                        }
                        else
                        {
                            // If the time has changed, update the last observed playback time
                            lastPlaybackTime = Utils.preciseTime;
                        }

                        // every second, update screen
                        if (Utils.MusicTimePlayed >= lastSeconds + 1)
                        {
                            lastSeconds = Utils.MusicTimePlayed;

                            // this check is to prevent lastSeconds from being greater than the song length, 
                            // early bug when AudioStream.position was changed to 0
                            if (lastSeconds >= Utils.currentMusicLength)
                            {
                                lastSeconds = -1;
                                treshhold += 1;
                            }
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
        public static void FakeLoop()
        {
            Utils.currentMusicLength = 0;
            Utils.MusicTimePlayed = 0;
            Utils.currentSong = "";
            Utils.currentSongIndex = 0;
            Start.playerView = "default";
            AnsiConsole.Clear();
            TUI.DrawFakePlayer();
            while (true)
            {
                if (consoleWidth != Console.WindowWidth || consoleHeight != Console.WindowHeight) {
                    consoleHeight = Console.WindowHeight;
                    consoleWidth = Console.WindowWidth;
                    TUI.ClearScreen();
                    TUI.DrawPlayer();
                }
                CheckKeyboard();
                if (Utils.songs.Length != 0) {
                    playerView = "default";
                    TUI.ClearScreen();
                    Run(Utils.songs);
                }
            }
        }
    }
}