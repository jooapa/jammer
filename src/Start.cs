using ManagedBass;
using Spectre.Console;
/* using System; */
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
        // ! Translations needed to locales
        public static MainStates state = MainStates.playing;
        public static bool drawOnce = false;
        private static Thread loopThread = new Thread(() => { });
        public static int consoleWidth = Console.WindowWidth;
        public static int consoleHeight = Console.WindowHeight;
        public static double lastSeconds = -1;
        public static double lastPlaybackTime = -1;
        public static double treshhold = 1;
        private static bool initWMP = false;
        public static double prevMusicTimePlayed = 0;

        //
        // Run
        //
        public static void Run(string[] args)
        {
            Utils.songs = args;
            Debug.dprint("Run");
            if (args.Length > 0) {
                // NOTE(ra) If debug switch is defined remove it from the args list
                for (int i = 0; i < args.Length; i++) {
                    string arg = args[i];
                    switch(arg) {
                        case "-D":
                            Utils.isDebug = true;
                            Debug.dprint("\n--- Debug Started ---\n");

                            long size = Preferences.DirSize(new DirectoryInfo(Utils.jammerPath));
                            Debug.dprint($"JammerDirSize: {size}");
                            Debug.dprint($"JammerDirSize: {Preferences.ToKilobytes(size)}");
                            Debug.dprint($"JammerDirSize: {Preferences.ToMegabytes(size)}");
                            Debug.dprint($"JammerDirSize: {Preferences.ToGigabytes(size)}");

                            List<string> argumentsList = new List<string>(args);
                            argumentsList.RemoveAt(i);
                            args = argumentsList.ToArray();
                            //NOTES(ra) So nasty it breaks my hearth,
                            Utils.songs = args;
                            break;
                    }
                }

                for (int i = 0; i < args.Length; i++) {
                    string arg = args[i];
                    switch (arg) {
                        case "-h":
                        case "--help":
                            TUI.ClearScreen();
                            TUI.CliHelp();
                            return;
                        case "--play":
                        case "-p":
                            if (args.Length > i+1) {
                                Playlists.Play(args[i+1], true);
                            } else {
                                AnsiConsole.WriteLine("No playlist name given");
                                Environment.Exit(1);
                            }
                            break;
                        case "--create":
                        case "-c":
                            if (args.Length > i+1) {
                                Playlists.Create(args[i+1]);
                            } else {
                                AnsiConsole.WriteLine("No playlist name given");
                            }
                            Environment.Exit(0);
                            break;
                        case "--delete":
                        case "-d":
                            if (args.Length > i+1) {
                                Playlists.Delete(args[i+1]);
                            } else {
                                AnsiConsole.WriteLine("No playlist name or song given");
                                Environment.Exit(0);
                            }
                            break;
                        case "--add":
                        case "-a":
                            if (args.Length > i+1) {
                                var splitIndex = i+1;
                                string[] firstHalf = args.Take(splitIndex).ToArray();
                                string[] secondHalf = args.Skip(splitIndex).ToArray();
                                Console.WriteLine(secondHalf[1]);
                                Playlists.Add(secondHalf);
                            } else {
                                AnsiConsole.WriteLine("No playlist name or song given");
                                Environment.Exit(0);
                            }
                            break;
                        case "--remove":
                        case "-r":
                            if (args.Length > i+1) {
                                var splitIndex = i+1;
                                string[] firstHalf = args.Take(splitIndex).ToArray();
                                string[] secondHalf = args.Skip(splitIndex).ToArray();
                                Console.WriteLine(secondHalf[1]);
                                Playlists.Remove(secondHalf);
                            } else {
                                AnsiConsole.WriteLine("No playlist name or song given");
                                Environment.Exit(0);
                            }
                            break;
                        case "--show":
                        case "-s":
                            if (args.Length > i+1) {
                                Playlists.ShowCli(args[i+1]);
                            } else {
                                AnsiConsole.WriteLine("No playlist name or song given");
                                Environment.Exit(0);
                            }
                            return;
                        case "--list":
                        case "-l":
                            Playlists.PrintList();
                            return;
                        case "--version":
                        case "-v":
                            AnsiConsole.MarkupLine("[green]Jammer version: " + Utils.version + "[/]");
                            return;
                        case "start":
                            // open explorer in jammer folder
                            AnsiConsole.MarkupLine("[green]Opening Jammer folder...[/]");
                            // if windows
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                                System.Diagnostics.Process.Start("explorer.exe", Utils.jammerPath);
                            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                                System.Diagnostics.Process.Start("xdg-open", Utils.jammerPath);
                            }
                            return;
                        case "update":
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                                AnsiConsole.MarkupLine("[red]Run the update command[/]");
                                return;
                            }
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
                            return;
                    }
                }
            } 

            Preferences.CheckJammerFolderExists();
            StartUp();
        }

        public static void StartUp() {

            if (!Bass.Init())
            {
                Message.Data(Locale.OutsideItems.InitializeError, Locale.OutsideItems.Error, true);
                return;
            }
            
            // Or specify a specific name in the current dir
            state = MainStates.idle; // Start in idle state if no songs are given
            if (Utils.songs.Length != 0)
            {
                Utils.songs = Absolute.Correctify(Utils.songs);
                //NOTE(ra) Correctify removes filenames from Utils.Songs. 
                //If there is one file that doesn't exist this is a fix
                if (Utils.songs.Length == 0) { 
                    return;
                }
                Utils.currentSong = Utils.songs[0];
                Utils.currentSongIndex = 0;
                state = MainStates.playing; // Start in play state if songs are given
                Play.PlaySong(Utils.songs, Utils.currentSongIndex);
            }

            Debug.dprint("Start Loop");
            loopThread = new Thread(Loop);
            loopThread.Start();
        }

        //
        // Main loop
        //
        public static void Loop()
        {
            lastSeconds = -1;
            treshhold = 1;

            Utils.isInitialized = true;

            TUI.ClearScreen();
            drawOnce = true;
            TUI.RehreshCurrentView();
            while (true)
            {
                if (Utils.songs.Length != 0)
                {
                    // if the first song is "" then there are more songs
                    if (Utils.songs[0] == "" && Utils.songs.Length > 1)
                    {
                        state = MainStates.play;
                        Play.DeleteSong(0);
                        Play.PlaySong();
                    }
                }

                if (consoleWidth != Console.WindowWidth || consoleHeight != Console.WindowHeight)
                {
                    consoleHeight = Console.WindowHeight;
                    consoleWidth = Console.WindowWidth;
                    AnsiConsole.Clear();
                    TUI.RehreshCurrentView();
                }

                switch (state)
                {
                    case MainStates.idle:
                        TUI.ClearScreen();
                        CheckKeyboard();
                        //FIXME(ra) This is a workaround for screen to update once when entering the state.
                        if (drawOnce)
                        {
                            TUI.DrawPlayer();
                            drawOnce = false;
                        }
                        break;

                    case MainStates.play:
                        Debug.dprint("Play");
                        if (Utils.songs.Length > 0)
                        {
                            Debug.dprint("Play - len");
                            Play.PlaySong();
                            TUI.ClearScreen();
                            TUI.DrawPlayer();
                            drawOnce = true;
                            Utils.MusicTimePlayed = 0;
                            state = MainStates.playing;
                        }
                        else
                        {   
                            drawOnce = true;
                            state = MainStates.idle;
                        }
                        break;

                    case MainStates.playing:
                        // get current time

                        Utils.preciseTime = Bass.ChannelBytes2Seconds(Utils.currentMusic, Bass.ChannelGetPosition(Utils.currentMusic));
                        // get current time in seconds
                        Utils.MusicTimePlayed = Bass.ChannelBytes2Seconds(Utils.currentMusic, Bass.ChannelGetPosition(Utils.currentMusic));
                        // get whole song length in seconds
                        //Utils.currentMusicLength = Utils.audioStream.Length / Utils.audioStream.WaveFormat.AverageBytesPerSecond;
                        Utils.currentMusicLength = Bass.ChannelBytes2Seconds(Utils.currentMusic, Bass.ChannelGetLength(Utils.currentMusic));


                        //FIXME(ra) This is a workaround for screen to update once when entering the state.
                        if (drawOnce)
                        {
                            TUI.DrawPlayer();
                            drawOnce = false;
                        }

                        // every second, update screen, use MusicTimePlayed, and prevMusicTimePlayed
                        if (Utils.MusicTimePlayed - prevMusicTimePlayed >= 1)
                        {
                            TUI.RehreshCurrentView();
                            prevMusicTimePlayed = Utils.MusicTimePlayed;
                        }

                        // If the song is finished, play next song
                        if (Bass.ChannelIsActive(Utils.currentMusic) == PlaybackState.Stopped && Utils.MusicTimePlayed > 0)
                        {
                            Play.MaybeNextSong();
                            prevMusicTimePlayed = 0;
                            TUI.RehreshCurrentView();
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
                
                Thread.Sleep(5);
            }
        }
    }
}
