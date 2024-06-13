using ManagedBass;
using Jammer;
using System.Runtime.InteropServices;
using Spectre.Console;
using System.Diagnostics;


namespace Jammer
{
    //NOTES(ra) A way to fix the drawonce - prevState

    // idle - the program wait for user input. Song is not played
    // play - Start playing - Play.PlaySong
    // playing - The music is playing. Update screen once a second or if a -
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

    public static partial class Start
    {
        //NOTE(ra) Starting state to playing.
        // public static MainStates state = MainStates.idle;
        // ! Translations needed to locales
        public static MainStates state = MainStates.playing;
        private static Thread loopThread = new(() => { });
        private static Thread visualizerThread = new(() => { });
#if CLI_UI
        public static int consoleWidth = Console.WindowWidth;
        public static int consoleHeight = Console.WindowHeight;        
#elif AVALONIA_UI
        public static int consoleWidth = 0;
        public static int consoleHeight = 0;
#endif
        public static bool CLI = false;
        public static double lastSeconds = -1;
        public static double lastPlaybackTime = -1;
        public static double treshhold = 1;
        public static double prevMusicTimePlayed = 0;
        public static bool LoopRunning = true;

        //
        // Run
        //

        public static void Run(string[] args)
        {
            #if AVALONIA_UI
                System.Diagnostics.Debug.WriteLine("AVALONIA_UI");
            #elif CLI_UI
                System.Diagnostics.Debug.WriteLine("CLI_UI");
            #else
                Console.WriteLine("No UI defined. Exiting.");
                System.Diagnostics.Debug.WriteLine("No UI defined. Exiting.");
                Environment.Exit(1);
            #endif


            #if CLI_UI
                try{
                    Console.OutputEncoding = System.Text.Encoding.UTF8;
                } catch (System.Exception e) {
                    Console.WriteLine(e.Message);
                }
            #endif

            #if CLI_UI
                System.Diagnostics.Debug.WriteLine("CLIUI");
            #endif
            #if AVALONIA_UI
                System.Diagnostics.Debug.WriteLine("AVALONIA_UI");
            #endif
            Utils.songs = args;
            // Theme init
            Themes.Init();

            Debug.dprint("Run");
            if (args.Length > 0) {
                // NOTE(ra) If debug switch is defined remove it from the args list
                for (int i = 0; i < args.Length; i++) {
                    string arg = args[i];
                    switch(arg) {
                        case "-D":
                            Utils.isDebug = true;
                            Debug.dprint("\n--- Debug Started ---\n");
                            Debug.dprint($"HOME Path Environment Variable: {Environment.GetEnvironmentVariable("APPDIR")}");
                            var APPDIRlen = Environment.GetEnvironmentVariable("APPDIR");
                            if (APPDIRlen == null) { 
                                Debug.dprint("APPDIR == null");
                            } else {
                                Debug.dprint(APPDIRlen.Length.ToString());
                            }

                            long size = Preferences.DirSize(new System.IO.DirectoryInfo(Utils.JammerPath));
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
#if CLI_UI
                                TUI.ClearScreen();
                                TUI.CliHelp();
#else
                                // TODO AVALONIA_UI
#endif
                            return;
                        case "--play":
                        case "-p":
                            if (args.Length > i+1) {
                                Playlists.Play(args[i+1], true);
                            } else {
#if CLI_UI
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistName);
#endif
#if AVALONIA_UI
                                // TODO AVALONIA_UI
#endif
                                Environment.Exit(1);
                            }
                            break;
                        case "--create":
                        case "-c":
                            if (args.Length > i+1) {
                                Playlists.Create(args[i+1]);
                            } else {
#if CLI_UI
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistName);
#endif
#if AVALONIA_UI
                                // TODO AVALONIA_UI
#endif
                            }
                            Environment.Exit(0);
                            break;
                        case "--delete":
                        case "-d":
                            if (args.Length > i+1) {
                                Playlists.Delete(args[i+1]);
                            } else {
#if CLI_UI
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistNameSong);
#endif
#if AVALONIA_UI
                                // TODO AVALONIA_UI
#endif
                                Environment.Exit(0);
                            }
                            break;
                        case "--add":
                        case "-a":
                            if (args.Length > i+1) {
                                var splitIndex = i+1;
                                string[] firstHalf = args.Take(splitIndex).ToArray();
                                string[] secondHalf = args.Skip(splitIndex).ToArray();
#if CLI_UI
                                Console.WriteLine(secondHalf[0]);
#endif
#if AVALONIA_UI
                                // TODO AVALONIA_UI
#endif
                                Playlists.Add(secondHalf);
                            } else {
#if CLI_UI
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistNameSong);

#endif
#if AVALONIA_UI
                                // TODO AVALONIA_UI
#endif
                                Environment.Exit(0);
                            }
                            break;
                        case "--remove":
                        case "-r":
                            if (args.Length > i+1) {
                                var splitIndex = i+1;
                                string[] firstHalf = args.Take(splitIndex).ToArray();
                                string[] secondHalf = args.Skip(splitIndex).ToArray();
#if CLI_UI
                                Console.WriteLine(secondHalf[1]);
#endif
#if AVALONIA_UI
                                // TODO AVALONIA_UI
#endif
                                Playlists.Remove(secondHalf);
                            } else {
#if CLI_UI
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistNameSong);
#endif
#if AVALONIA_UI
                                // TODO AVALONIA_UI
#endif
                                Environment.Exit(0);
                            }
                            break;
                        case "--show":
                        case "-s":
                            if (args.Length > i+1) {
                                Playlists.ShowCli(args[i+1]);
                            } else {
#if CLI_UI
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistNameSong);
#endif
#if AVALONIA_UI
                                // TODO AVALONIA_UI
#endif
                                Environment.Exit(0);
                            }
                            return;
                        case "--list":
                        case "-l":
                            Playlists.PrintList();
                            return;
                        case "--version":
                        case "-v":
#if CLI_UI
                            AnsiConsole.MarkupLine($"[green]Jammer {Locale.Miscellaneous.Version}: " + Utils.version + "[/]");
#endif
#if AVALONIA_UI
                            // TODO AVALONIA_UI
#endif
                            return;
                        case "--flush":
                        case "-f":
                            Songs.Flush();
                            return;
                        case "--set-path":
                        case "-sp": // TODO ADD LOCALE :)) https://www.youtube.com/watch?v=thPv_v7890g
                            if (args.Length > i+1) {
                                if (Directory.Exists(args[i+1])) {
                                    Preferences.songsPath = Path.GetFullPath(Path.Combine(args[i+1], "songs"));
#if CLI_UI
                                    AnsiConsole.MarkupLine("[green]Songs path set to: " + Preferences.songsPath + "[/]");
#endif
#if AVALONIA_UI
                                    // TODO AVALONIA_UI
#endif
                                }
                                else if (args[i+1] == "") {
#if CLI_UI
                                    AnsiConsole.MarkupLine("No path given.");
#endif
#if AVALONIA_UI
                                    // TODO AVALONIA_UI
#endif
                                    return;
                                }
                                else if (args[i+1] == "default") {
                                    Preferences.songsPath = Path.Combine(Utils.JammerPath, "songs");
#if CLI_UI
                                    AnsiConsole.MarkupLine("[green]Songs path set to default.[/]"); // TODO ADD LOCALE
#endif
#if AVALONIA_UI
                                    // TODO AVALONIA_UI
#endif
                                } else {
#if CLI_UI
                                    AnsiConsole.MarkupLine($"[red]Path [grey]'[/][white]{args[i+1]}[/][grey]'[/] does not exist.[/]"); // TODO ADD LOCALE
#endif
#if AVALONIA_UI
                                    // TODO AVALONIA_UI
#endif
                                }

                                Preferences.SaveSettings();
                            } else {
#if CLI_UI
                                AnsiConsole.MarkupLine("[red]No songs path given.[/]"); // TODO ADD LOCALE
                                
#endif
#if AVALONIA_UI
                                // TODO AVALONIA_UI
#endif
                            }
                            return;
                        case "--get-path":
                        case "-gp":
                            AnsiConsole.MarkupLine("[green]Songs path: " + Preferences.songsPath + "[/]"); // TODO ADD LOCALE
                            return;
                        case "--songs":
                        case "-so":
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                                System.Diagnostics.Process.Start("explorer.exe", Preferences.songsPath);
                            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                                System.Diagnostics.Process.Start("xdg-open", Preferences.songsPath);
                            }
                            return;
                        case "--home":
                        case "-hm":
                                // if(Utils.songs.Length != 1 && args.Length != 1) {
                                //     AnsiConsole.MarkupLine("[red]When using --songs or -so, do not provide any other arguments.[/]"); // TODO ADD LOCALE
                                //     System.Environment.Exit(1);
                                // } 
                                Utils.songs[0] = Preferences.songsPath;
                                break;
                        case "--start":
                            // open explorer in Jammer folder
#if CLI_UI
                            AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.OpeningFolder}[/]");
#endif
#if AVALONIA_UI
                            // TODO AVALONIA_UI
#endif
                            // if windows
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                                System.Diagnostics.Process.Start("explorer.exe", Utils.JammerPath);
                            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                                System.Diagnostics.Process.Start("xdg-open", Utils.JammerPath);
                            }
                            return;
                        case "--update":
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
#if CLI_UI
                                AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.RunUpdate}[/]");
                                
#endif
#if AVALONIA_UI
                                // TODO AVALONIA_UI
#endif
                                return;
                            }
#if CLI_UI
                            AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.CheckingUpdates}[/]");
#endif
#if AVALONIA_UI
                            // TODO AVALONIA_UI
#endif
                            string latestVersion = Update.CheckForUpdate(Utils.version);
                            if (latestVersion != "") {
                                
                                string downloadPath = Update.UpdateJammer(latestVersion);
#if CLI_UI
                                AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.UpdateFound}[/]" + "\n" + $"{Locale.Miscellaneous.Version}: [green]" + latestVersion + "[/]");
                                AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.Downloading}[/]");
            
                                AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.DownloadedTo}: " + downloadPath + "[/]");
                                AnsiConsole.MarkupLine($"[cyan]{Locale.OutsideItems.Installing}[/]");
                                
#endif
#if AVALONIA_UI
                                // TODO AVALONIA_UI
#endif
                                // Run run_command.bat with argument as the path to the downloaded file
                                System.Diagnostics.Process.Start("run_command.bat", downloadPath);
                            } else {
#if CLI_UI
                                AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.UpToDate}[/]");
#endif
#if AVALONIA_UI
                                // TODO AVALONIA_UI
#endif
                            }
                            return;
                    }
                }
            } 
            
            Preferences.CheckJammerFolderExists();
            IniFileHandling.Create_KeyDataIni(0);
            IniFileHandling.Create_KeyDataIni(2);
            StartUp();
        }

        public static void StartUp() {
            try {
                if (!Bass.Init()) {   
                    Message.Data(Locale.OutsideItems.InitializeError, Locale.OutsideItems.Error, true);
                    return;
                }
                // Additional code if initialization is successful
            } catch (Exception ex) {
                // Log the exception message and stack trace
                Console.WriteLine($"Exception during BASS initialization: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            InitializeSharpHook();
            // Or specify a specific name in the current dir
            state = MainStates.idle; // Start in idle state if no songs are given
            if (Utils.songs.Length != 0)
            {
                Utils.songs = Absolute.Correctify(Utils.songs);
                //NOTE(ra) Correctify removes filenames from Utils.Songs. 
                //If there is one file that doesn't exist this is a fix
                if (Utils.songs.Length == 0) {
                    Debug.dprint("No songs found");
                    AnsiConsole.WriteLine("No songs found. Exiting..."); 
                    Environment.Exit(1);
                }
                Utils.currentSong = Utils.songs[0];
                Utils.currentSongIndex = 0;
                state = MainStates.playing; // Start in play state if songs are given
                Play.PlaySong(Utils.songs, Utils.currentSongIndex);
            }

            Console.CancelKeyPress += new ConsoleCancelEventHandler(Exit.OnExit);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(Exit.OnProcessExit);

            Debug.dprint("Start Loop");
            loopThread = new Thread(Loop);
            visualizerThread = new Thread(EqualizerLoop);
            loopThread.Start();
            visualizerThread.Start();
        }

        //
        // Main loop
        //
        public static bool drawTime = false;
        public static bool drawVisualizer = false;
        public static bool drawWhole = false;

        public static string previousView = "default";
        public static void Loop()
        {
            lastSeconds = -1;
            treshhold = 1;

            Utils.isInitialized = true;

            AnsiConsole.Clear();
            TUI.RefreshCurrentView();
            AnsiConsole.Cursor.Hide();

            while (LoopRunning)
            {
                AnsiConsole.Cursor.Hide();
                if (Utils.songs.Length != 0)
                {
                    // if the first song is "" then there are more songs
                    if (Utils.songs[0] == "" && Utils.songs.Length > 1)
                    {
                        state = MainStates.play;
                        Play.DeleteSong(0, false);
                        Play.PlaySong();
                    }
                }

                if (consoleWidth != Console.WindowWidth || consoleHeight != Console.WindowHeight)
                {
                    consoleHeight = Console.WindowHeight;
                    consoleWidth = Console.WindowWidth;
                    AnsiConsole.Clear();
                    drawWhole = true;
                }

                switch (state)
                {
                    case MainStates.idle:
                        // TUI.ClearScreen();
                        CheckKeyboard();
                        break;

                    case MainStates.play:
                        Debug.dprint("Play");
                        if (Utils.songs.Length > 0)
                        {
                            Debug.dprint("Play - len");
                            Play.PlaySong();
                            TUI.ClearScreen();
                            TUI.DrawPlayer();

                            Utils.MusicTimePlayed = 0;
                            state = MainStates.playing;
                        }
                        else
                        {   
                            drawWhole = true;
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

                        Utils.MusicTimePercentage = (float)(Utils.MusicTimePlayed / Utils.currentMusicLength * 100);

                        // every second, update screen, use MusicTimePlayed, and prevMusicTimePlayed
                        if (Utils.MusicTimePlayed - prevMusicTimePlayed >= 1)
                        {
                            drawTime = true;
                            prevMusicTimePlayed = Utils.MusicTimePlayed;
                        }

                        // If the song is finished
                        if (Bass.ChannelIsActive(Utils.currentMusic) == PlaybackState.Stopped && Utils.MusicTimePlayed > 0)
                        {
                            prevMusicTimePlayed = 0;
                            drawTime = true;
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
                        // AnsiConsole.Clear();
                        break;
                    case MainStates.previous:
                        if(Utils.MusicTimePlayed > 3){ // if the song is played for more than 5 seconds, go to the beginning
                            Play.SeekSong(0, false);
                            state = MainStates.playing;
                        } else {
                            Play.PrevSong();
                        }
                        break;
                }

                // If the view is changed, refresh the screen
                if (previousView != playerView)
                {
                    drawWhole = true;
                }

                if (playerView == "default" || playerView == "all")
                {
                    if (drawVisualizer && Preferences.isVisualizer) {
                        TUI.DrawVisualizer();
                    } if (drawTime) {
                        TUI.DrawTime();
                    } if (drawWhole) {
                        TUI.RefreshCurrentView();
                    }
                }
                else {
                    TUI.RefreshCurrentView();
                }

                previousView = playerView;
                drawVisualizer = false;
                drawTime = false;
                drawWhole = false;

                if (playerView == "default" || playerView == "all") {
                    Thread.Sleep(1);
                } else
                    Thread.Sleep(50);

            }
        }

        static bool canVisualize = false;
        private static void EqualizerLoop()
        {
            while (true)
            {
                if (Preferences.isVisualizer) {
                    if (playerView == "default" || playerView == "all") {
                        canVisualize = true;
                    } else {
                        canVisualize = false;
                    }

                    if (canVisualize)
                    {
                        drawVisualizer = true;
                    }
                }
                Thread.Sleep(Visual.refreshTime);
            }
        }
        

        /// <summary>
        /// Removes "[" and "]" from a string to prevent Spectre.Console from blowing up.
        /// </summary>
        /// <param name="input">The string to sanitize</param>
        /// <returns>The sanitized string</returns>
        /// <remarks>
        /// This is a workaround for a bug in Spectre.Console that causes it to crash when it encounters "[" or "]" in a string.
        /// </remarks>
        /// <example>
        /// <code>
        /// string sanitized = Sanitize("Hello world [lol]");
        /// Output: "Hello world lol"
        /// </code>        
        public static string Sanitize(string input)
        {
            // Remove [ ] from input
            input = input.Replace("[", "");
            input = input.Replace("]", "");
            input = input.Replace("\"", "\'");
            // input = input.Replace("\"", "");
            // input = input.Replace("\'", "");
            return input;
        }

        /// <summary>
        /// Sanitizes an array of strings by calling the Sanitize method on each element.
        /// </summary>
        /// <param name="input">The array of strings to be sanitized.</param>
        /// <returns>The sanitized array of strings.</returns>
        public static string[] Sanitize(string[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                input[i] = Sanitize(input[i]);
            }
            return input;
        }
    }
}
        
