using ManagedBass;
using Jammer;
using System.Runtime.InteropServices;
using Spectre.Console;
using System.Diagnostics;
using System.Text.RegularExpressions;


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
        public static int consoleWidth = Console.WindowWidth;
        public static int consoleHeight = Console.WindowHeight;
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
            Log.Info("Starting Jammer...");
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Log.Info("Output encoding set to UTF8");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Log.Error("Error setting output encoding to UTF8");
            }

            Utils.Songs = args;
            // Theme init
            Themes.Init();
            Log.Info("Themes initialized");
            Debug.dprint("Run");
            if (args.Length > 0)
            {
                CheckArgs(args);
            }

            Preferences.CheckJammerFolderExists();
            IniFileHandling.Create_KeyDataIni(0);
            IniFileHandling.Create_KeyDataIni(2);
            StartUp();
        }

        public static void StartUp()
        {
            try
            {
                if (!Bass.Init())
                {
                    /* Message.Data(Locale.OutsideItems.InitializeError, Locale.OutsideItems.Error, true); */
                    Log.Error("BASS initialization failed");
                    return;
                }
                // Additional code if initialization is successful
            }
            catch (Exception ex)
            {
                // Log the exception message and stack trace
                Console.WriteLine($"Exception during BASS initialization: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            Log.Info("BASS initialized");

            // Initialize the keyboard hook
            Log.Info("Initializing keyboard hook");
            InitializeSharpHook();
            // Or specify a specific name in the current dir
            state = MainStates.idle; // Start in idle state if no songs are given
            if (Utils.Songs.Length != 0)
            {
                Utils.Songs = Absolute.Correctify(Utils.Songs);
                //NOTE(ra) Correctify removes filenames from Utils.Songs. 
                //If there is one file that doesn't exist this is a fix
                if (Utils.Songs.Length == 0)
                {
                    Debug.dprint("No songs found");
                    AnsiConsole.WriteLine("No songs found. Exiting...");
                    Environment.Exit(1);
                }
                Utils.CurrentSongPath = Utils.Songs[0];
                Utils.CurrentSongIndex = 0;
                state = MainStates.playing; // Start in play state if songs are given
                Play.PlaySong(Utils.Songs, Utils.CurrentSongIndex);
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
        public static bool debug = false;
        public static void Loop()
        {
            lastSeconds = -1;
            treshhold = 1;

            Utils.IsInitialized = true;

            AnsiConsole.Clear();
            TUI.RefreshCurrentView();
            AnsiConsole.Cursor.Hide();

            while (LoopRunning)
            {
                // Start performance monitoring for this loop iteration
                PerformanceMonitor.IncrementLoopIterations();
                PerformanceMonitor.StartLoopTiming();
                
                AnsiConsole.Cursor.Hide();
                if (Utils.Songs.Length != 0)
                {
                    // if the first song is "" then there are more songs
                    if (Utils.Songs[0] == "" && Utils.Songs.Length > 1)
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
                        CheckKeyboardAsync();
                        break;

                    case MainStates.play:
                        Debug.dprint("Play");
                        if (Utils.Songs.Length > 0)
                        {
                            Debug.dprint("Play - len");
                            Play.PlaySong();
                            TUI.ClearScreen();
                            TUI.DrawPlayer();

                            Utils.TotalMusicDurationInSec = 0;
                            state = MainStates.playing;
                        }
                        else
                        {
                            drawWhole = true;
                            state = MainStates.idle;
                        }
                        break;

                    case MainStates.playing:
                        // every second, update screen, use MusicTimePlayed, and prevMusicTimePlayed
                        if (Utils.TotalMusicDurationInSec - prevMusicTimePlayed >= 1)
                        {
                            drawTime = true;
                            prevMusicTimePlayed = Utils.TotalMusicDurationInSec;
                        }

                        // If the song is finished
                        if (Bass.ChannelIsActive(Utils.CurrentMusic) == PlaybackState.Stopped && Utils.TotalMusicDurationInSec > 0)
                        {
                            prevMusicTimePlayed = 0;
                            drawTime = true;
                        }
                        if (debug)
                            Message.Data("asd", "asd");
                        CheckKeyboardAsync();
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
                        break;
                    case MainStates.previous:
                        if (Utils.TotalMusicDurationInSec > 3)
                        { // if the song is played for more than 5 seconds, go to the beginning
                            Play.SeekSong(0, false);
                            state = MainStates.playing;
                        }
                        else
                        {
                            Play.PrevSong();
                        }
                        break;
                }

                if (debug)
                    // print the call stack
                    Message.Data(Environment.StackTrace, "Call Stack", true);

                Utils.PreciseTime = Bass.ChannelBytes2Seconds(Utils.CurrentMusic, Bass.ChannelGetPosition(Utils.CurrentMusic));
                // get current time in seconds
                Utils.TotalMusicDurationInSec = Bass.ChannelBytes2Seconds(Utils.CurrentMusic, Bass.ChannelGetPosition(Utils.CurrentMusic));
                // get whole song length in seconds
                //Utils.currentMusicLength = Utils.audioStream.Length / Utils.audioStream.WaveFormat.AverageBytesPerSecond;
                Utils.SongDurationInSec = Bass.ChannelBytes2Seconds(Utils.CurrentMusic, Bass.ChannelGetLength(Utils.CurrentMusic));

                Utils.MusicTimePercentage = (float)(Utils.TotalMusicDurationInSec / Utils.SongDurationInSec * 100);

                // if no song is playing, set the current song to ""
                if (Utils.Songs.Length == 0)
                {
                    Utils.CurrentSongPath = "";
                }

                // If the view is changed, refresh the screen
                if (previousView != playerView)
                {
                    drawWhole = true;
                }

                if (playerView == "default" || playerView == "all" || playerView == "rss")
                {
                    if (debug)
                        Message.Data(drawWhole.ToString(), "22");

                    if (drawVisualizer && Preferences.isVisualizer)
                    {
                        if (state == MainStates.playing || state == MainStates.pause || state == MainStates.stop || state == MainStates.idle)
                        {
                            TUI.DrawVisualizer();
                        }
                    }
                    if (drawTime)
                    {
                        TUI.DrawTime();
                    }
                    if (drawWhole)
                    {
                        TUI.RefreshCurrentView();
                    }
                }
                else
                {
                    if (drawWhole)
                    {
                        TUI.RefreshCurrentView();
                    }
                }

                previousView = playerView;
                drawVisualizer = false;
                drawTime = false;
                drawWhole = false;

                // Log performance metrics periodically
                PerformanceMonitor.LogPerformanceMetrics();
                
                // End performance timing before sleep
                PerformanceMonitor.EndLoopTiming();

                if (playerView == "default" || playerView == "all" || playerView == "rss")
                {
                    Thread.Sleep(16);
                }
                else
                    Thread.Sleep(5);
            }
        }

        static bool canVisualize = false;
        private static void EqualizerLoop()
        {
            while (true)
            {
                if (Preferences.isVisualizer)
                {
                    if (playerView == "default" || playerView == "all")
                    {
                        canVisualize = true;
                    }
                    else
                    {
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
        public static string Sanitize(string input, bool removeBrakets = false)
        {
            if (removeBrakets)
            {
                input = input.Replace("[", "");
                input = input.Replace("]", "");
            }
            else
            {
                input = input.Replace("[", "[[");
                input = input.Replace("]", "]]");
            }
            input = input.Replace("\"", "\'");
            return input;
        }

        // replace inputSaying every character inside of [] @"\[.*?\]
        /// <summary>
        /// Removes all occurrences of text enclosed in square brackets from the input string.
        /// </summary>
        /// <param name="input">The input string to be purged.</param>
        /// <returns>The input string with all occurrences of text enclosed in square brackets removed.</returns>
        public static string Purge(string input)
        {
            string pattern = @"\[.*?\]";
            string replacement = "";
            Regex rgx = new(pattern);
            string text = rgx.Replace(input, replacement);
            return text;
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

