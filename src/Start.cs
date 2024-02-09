using Spectre.Console;

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
            Debug.dprint("Run");

            for (int i=0; i < args.Length; i++) {
                if (args[i] == "-d") {
                    Utils.isDebug = true;
                    Debug.dprint("\n--- Debug Started ---\n");
                    List<string> argumentsList = new List<string>(args);
                    argumentsList.RemoveAt(0);
                    args = argumentsList.ToArray();
                    break;
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
                if (args[0] == "selfdestruct")
                {
                    AnsiConsole.MarkupLine("[red]Selfdestructing Jammer...[/]");

                    // Get the base directory of the application
                    string baseDirectory = AppContext.BaseDirectory;

                    // Run the uninstaller selfdestruct.bat
                    string selfDestructScriptPath = Path.Combine(baseDirectory, "selfdestruct.bat");
                    System.Diagnostics.Process.Start(selfDestructScriptPath);

                    // Exit the application
                    Environment.Exit(0);
            }
                if (args[0] == "start") {
                    // open explorer in jammer folder
                    AnsiConsole.MarkupLine("[green]Opening Jammer folder...[/]");
                    System.Diagnostics.Process.Start("explorer.exe", Utils.jammerPath);
                    return;
                }
                if (args[0] == "update") {
                    AnsiConsole.MarkupLine("[green]Checking for updates...[/]");

                    string latestVersion = Update.CheckForUpdate(Utils.version);
                    if (latestVersion != "") {
                        AnsiConsole.MarkupLine("[green]Update found![/]" + "\n" + "Version: [green]" + latestVersion + "[/]");
                        AnsiConsole.MarkupLine("[green]Downloading...[/]");
                        string downloadPath = Update.UpdateJammer(latestVersion);

                        AnsiConsole.MarkupLine("[green]Downloaded to: " + downloadPath + "[/]");
                        AnsiConsole.MarkupLine("[cyan]Installing...[/]");
                        // Run run_command.bat with argument as the path to the downloaded file
                        System.Diagnostics.Process.Start("run_command.bat", downloadPath);
                    } else {
                        AnsiConsole.MarkupLine("[green]Jammer is up to date![/]");
                    }

                    Environment.Exit(0);
                }
            }

            Preferences.CheckJammerFolderExists();

            if (args.Length != 0 ) {
                Utils.songs = args;
                Utils.songs = Absolute.Correctify(Utils.songs);
                Play.PlaySong(Utils.songs, Utils.currentSongIndex);
            } else {
                state = MainStates.idle;
                Debug.dprint("Start Loop");
                loopThread = new Thread(Loop);
                loopThread.Start();
            }
        }

        //
        // Main loop
        //
        public static void Loop()
        {
            lastSeconds = -1;
            treshhold = 1;
            // if (Utils.audioStream == null || Utils.currentMusic == null) {
            //     Debug.dprint("Audiostream");
            //     return;
            // }

            TUI.ClearScreen();
            drawOnce = true;

            while (true)
            {
                // if the frist song is "" then there are more songs
                if (Utils.songs[0] == "" && Utils.songs.Length > 1) {
                    state = MainStates.play;
                    Play.DeleteSong(0);
                }

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
                        Debug.dprint("Play");
                        if (Utils.songs.Length > 0) {
                            Debug.dprint("Play - len");
                            Play.PlaySong();
                            TUI.ClearScreen();
                            TUI.DrawPlayer();
                            drawOnce = true;
                            Utils.MusicTimePlayed = 0;
                            state = MainStates.playing;
                        }
                        break;
                    case MainStates.playing:
                        // get current time
                        if (Utils.audioStream != null)
                        {
                            Utils.preciseTime = Utils.audioStream.Position;
                            // get current time in seconds
                            Utils.MusicTimePlayed = Utils.audioStream.Position / Utils.audioStream.WaveFormat.AverageBytesPerSecond;
                            // get whole song length in seconds
                            Utils.currentMusicLength = Utils.audioStream.Length / Utils.audioStream.WaveFormat.AverageBytesPerSecond;
                        }

                        //FIXME(ra) This is a workaround for screen to update once when entering the state.
                        if (drawOnce) {
                            TUI.DrawPlayer();
                            drawOnce = false;
                        }

                        // If the song is finished, play next song
                        if (treshhold % 100 == 0) {
                            // If the time hasn't changed, the song is finished
                            if (lastPlaybackTime == Utils.preciseTime) {
                                Play.MaybeNextSong();
                            }
                        }
                        else {
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
    }
}