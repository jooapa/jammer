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
            try{
                Console.OutputEncoding = System.Text.Encoding.UTF8;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }

            Utils.songs = args;
            // Theme init
            Themes.Init();

            Debug.dprint("Run");
            if (args.Length > 0) {
                CheckArgs(args);
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
        private static bool alreadyDrewHelp = false;

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
                    alreadyDrewHelp = false;
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
                        if (state == MainStates.playing || state == MainStates.pause || state == MainStates.stop || state == MainStates.idle) {
                            TUI.DrawVisualizer();
                        }
                    } if (drawTime) {
                        TUI.DrawTime();
                    } if (drawWhole) {
                        TUI.RefreshCurrentView();
                    }
                }
                else {
                    if (playerView == "help" && !alreadyDrewHelp) {
                        TUI.DrawHelp();
                        alreadyDrewHelp = true;
                    }
                    else if (playerView != "help") {
                        alreadyDrewHelp = false;
                        TUI.RefreshCurrentView();
                    }
                }

                previousView = playerView;
                drawVisualizer = false;
                drawTime = false;
                drawWhole = false;

                if (playerView == "default" || playerView == "all") {
                    Thread.Sleep(1);
                    alreadyDrewHelp = false;
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
        
