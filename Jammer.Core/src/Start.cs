using ManagedBass;
using Jammer;
using System.Runtime.InteropServices;
using Spectre.Console;

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
        public static bool drawOnce = false;
        private static Thread loopThread = new Thread(() => { });
        public static int consoleWidth = Console.WindowWidth;
        public static int consoleHeight = Console.WindowHeight;
        public static double lastSeconds = -1;
        public static double lastPlaybackTime = -1;
        public static double treshhold = 1;
        public static double prevMusicTimePlayed = 0;
        public static bool CLI = false;

        //
        // Run
        //
        public static void Run(string[] args, bool isCli = false)
        {
            Start.CLI = isCli;

            System.Diagnostics.Debug.WriteLine(drawOnce);
            if (Start.CLI) {
                try{
                    Console.OutputEncoding = System.Text.Encoding.UTF8;
                } catch (System.Exception e) {
                    Console.WriteLine(e.Message);
                }
            }

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
                        
                            TUI.ClearScreen();
                            TUI.CliHelp();
                            // TODO AVALONIA_UI
                            return;
                        case "--play":
                        case "-p":
                            if (args.Length > i+1) {
                                Playlists.Play(args[i+1], true);
                            } else {
                                if (Start.CLI) {
                                    AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistName);
                                } else {
                                    // TODO AVALONIA_UI
                                }
                                Environment.Exit(1);
                            }
                            break;
                        case "--create":
                        case "-c":
                            if (args.Length > i+1) {
                                Playlists.Create(args[i+1]);
                            } else {
                                if (Start.CLI) {
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistName);
                                } else {
                                
                                // TODO AVALONIA_UI
                                }
                            }
                            Environment.Exit(0);
                            break;
                        case "--delete":
                        case "-d":
                            if (args.Length > i+1) {
                                Playlists.Delete(args[i+1]);
                            } else {
                                if (Start.CLI) {
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistNameSong);
                                } else {
                                
                                // TODO AVALONIA_UI
                                }
                                Environment.Exit(0);
                            }
                            break;
                        case "--add":
                        case "-a":
                            if (args.Length > i+1) {
                                var splitIndex = i+1;
                                string[] firstHalf = args.Take(splitIndex).ToArray();
                                string[] secondHalf = args.Skip(splitIndex).ToArray();
                                if (Start.CLI) {
                                Console.WriteLine(secondHalf[0]);
                                } else {
                                
                                // TODO AVALONIA_UI
                                }
                                Playlists.Add(secondHalf);
                            } else {
                                if (Start.CLI) {
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistNameSong);
                                } else {
                                
                                // TODO AVALONIA_UI
                                }
                                Environment.Exit(0);
                            }
                            break;
                        case "--remove":
                        case "-r":
                            if (args.Length > i+1) {
                                var splitIndex = i+1;
                                string[] firstHalf = args.Take(splitIndex).ToArray();
                                string[] secondHalf = args.Skip(splitIndex).ToArray();
                                if (Start.CLI) {
                                Console.WriteLine(secondHalf[1]);
                                } else {
                                
                                // TODO AVALONIA_UI
                                }
                                Playlists.Remove(secondHalf);
                            } else {
                                if (Start.CLI) {
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistNameSong);
                                } else {
                                
                                // TODO AVALONIA_UI
                                }
                                Environment.Exit(0);
                            }
                            break;
                        case "--show":
                        case "-s":
                            if (args.Length > i+1) {
                                Playlists.ShowCli(args[i+1]);
                            } else {
                                if (Start.CLI) {
                                AnsiConsole.WriteLine(Locale.OutsideItems.NoPlaylistNameSong);
                                } else {
                                
                                // TODO AVALONIA_UI
                                }
                                Environment.Exit(0);
                            }
                            return;
                        case "--list":
                        case "-l":
                            Playlists.PrintList();
                            return;
                        case "--version":
                        case "-v":
                            if (Start.CLI) {
                                AnsiConsole.MarkupLine($"[green]Jammer {Locale.Miscellaneous.Version}: " + Utils.version + "[/]");
                            } else {
                            
                            // TODO AVALONIA_UI
                            }
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
                                    if (Start.CLI) {
                                    AnsiConsole.MarkupLine("[green]Songs path set to: " + Preferences.songsPath + "[/]");
                                    } else {
                                    
                                    // TODO AVALONIA_UI
                                    }
                                }
                                else if (args[i+1] == "") {
                                    if (Start.CLI) {
                                    AnsiConsole.MarkupLine("No path given.");
                                    } else {
                                    
                                    // TODO AVALONIA_UI
                                    }
                                    return;
                                }
                                else if (args[i+1] == "default") {
                                    Preferences.songsPath = Path.Combine(Utils.JammerPath, "songs");
                                    if (Start.CLI) {
                                    AnsiConsole.MarkupLine("[green]Songs path set to default.[/]"); // TODO ADD LOCALE
                                    } else {
                                        // TODO AVALONIA_UI
                                    }
                                } else {
                                    if (Start.CLI) {
                                    AnsiConsole.MarkupLine($"[red]Path [grey]'[/][white]{args[i+1]}[/][grey]'[/] does not exist.[/]"); // TODO ADD LOCALE
                                    } else {
                                    
                                    // TODO AVALONIA_UI
                                    }
                                }

                                Preferences.SaveSettings();
                            } else {
                                if (Start.CLI) {
                                AnsiConsole.MarkupLine("[red]No songs path given.[/]"); // TODO ADD LOCALE
                                } else {
                                
                                // TODO AVALONIA_UI
                                }
                            }
                            return;
                        case "--start":
                            // open explorer in Jammer folder
                            if (Start.CLI) {
                            AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.OpeningFolder}[/]");
                            } else {
                            
                            // TODO AVALONIA_UI
                            }
                            // if windows
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                                System.Diagnostics.Process.Start("explorer.exe", Utils.JammerPath);
                            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                                System.Diagnostics.Process.Start("xdg-open", Utils.JammerPath);
                            }
                            return;
                        case "--update":
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                                if (Start.CLI) {
                                AnsiConsole.MarkupLine($"[red]{Locale.OutsideItems.RunUpdate}[/]");
                                } else {
                                
                                // TODO AVALONIA_UI
                                }
                                return;
                            }
                            if (Start.CLI) {
                            AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.CheckingUpdates}[/]");
                            } else {
                            
                            // TODO AVALONIA_UI
                            }
                            string latestVersion = Update.CheckForUpdate(Utils.version);
                            if (latestVersion != "") {
                                
                                string downloadPath = Update.UpdateJammer(latestVersion);
                                if (Start.CLI) {
                                AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.UpdateFound}[/]" + "\n" + $"{Locale.Miscellaneous.Version}: [green]" + latestVersion + "[/]");
                                AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.Downloading}[/]");
            
                                AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.DownloadedTo}: " + downloadPath + "[/]");
                                AnsiConsole.MarkupLine($"[cyan]{Locale.OutsideItems.Installing}[/]");
                                } else {
                                
                                // TODO AVALONIA_UI
                                }
                                // Run run_command.bat with argument as the path to the downloaded file
                                System.Diagnostics.Process.Start("run_command.bat", downloadPath);
                            } else {
                                if (Start.CLI) {
                                AnsiConsole.MarkupLine($"[green]{Locale.OutsideItems.UpToDate}[/]");
                                } else {
                                
                                // TODO AVALONIA_UI
                                }
                            }
                            return;
                    }
                }
            } 

            Preferences.CheckJammerFolderExists();
            IniFileHandling.Create_KeyDataIni(2);
            StartUp();
        }

        public static void StartUp() {

            if (!Bass.Init())
            {
                if (Start.CLI) {
                Jammer.Message.Data(Locale.OutsideItems.InitializeError, Locale.OutsideItems.Error, true);
                } else {
                
                // TODO AVALONIA_UI
                }
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

            if (Start.CLI) {
            TUI.ClearScreen();
            drawOnce = true;
            TUI.RefreshCurrentView();
            }

            Console.WriteLine("Loop");
            // TODO AVALONIA_UI
            while (true)
            {
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

                    if (Start.CLI) {
                    if (consoleWidth != Console.WindowWidth || consoleHeight != Console.WindowHeight)
                    {
                        consoleHeight = Console.WindowHeight;
                        consoleWidth = Console.WindowWidth;
                        AnsiConsole.Clear();
                        TUI.RefreshCurrentView();
                    }
                    } else {
                    
                    // TODO AVALONIA_UI
                    }

                switch (state)
                {
                    case MainStates.idle:
                        if (Start.CLI) {
                        TUI.ClearScreen();
                        CheckKeyboard();
                        } else {
                        if (Start.CLI) {
                            //FIXME(ra) This is a workaround for screen to update once when entering the state.
                            if (drawOnce)
                            {
                                TUI.DrawPlayer();
                                drawOnce = false;
                            }
                        } else {
                            // TODO AVALONIA_UI
                        }
                        }
                        break;

                    case MainStates.play:
                        Debug.dprint("Play");
                        if (Utils.songs.Length > 0)
                        {
                            Debug.dprint("Play - len");
                            Play.PlaySong();
                            if (Start.CLI) {
                            TUI.ClearScreen();
                            TUI.DrawPlayer();
                            drawOnce = true;
                            } else {
                            
                            // TODO AVALONIA_UI
                            }
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


                        if (Start.CLI) {
                        //FIXME(ra) This is a workaround for screen to update once when entering the state.
                        if (drawOnce)
                        {
                            TUI.DrawPlayer();
                            drawOnce = false;
                        }
                        } else {
                        
                        // TODO AVALONIA_UI
                        }

                        // every second, update screen, use MusicTimePlayed, and prevMusicTimePlayed
                        if (Utils.MusicTimePlayed - prevMusicTimePlayed >= 1)
                        {
                            if (Start.CLI) {
                            TUI.RefreshCurrentView();
                            } else {
                            
                            // TODO AVALONIA_UI
                            }
                            prevMusicTimePlayed = Utils.MusicTimePlayed;
                        }

                        // If the song is finished, play next song
                        if (Bass.ChannelIsActive(Utils.currentMusic) == PlaybackState.Stopped && Utils.MusicTimePlayed > 0)
                        {
                            Play.MaybeNextSong();
                            prevMusicTimePlayed = 0;
                            if (Start.CLI) {
                            TUI.RefreshCurrentView();
                            } else {
                            
                            // TODO AVALONIA_UI
                            }
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
                        if (Start.CLI) {
                        TUI.ClearScreen();
                        } else {
                        
                        // TODO AVALONIA_UI
                        }
                        break;

                    case MainStates.previous:
                        Play.PrevSong();
                        if (Start.CLI) {
                        TUI.ClearScreen();
                        } else {
                        
                        // TODO AVALONIA_UI
                        }
                        break;
                }
                
                Thread.Sleep(5);
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
