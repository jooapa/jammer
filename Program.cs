using jammer;
using NAudio.Wave;
using Spectre.Console;

class Program
{
    static WaveOutEvent outputDevice = new WaveOutEvent();
    static public float volume = JammerFolder.GetVolume();
    static public bool isLoop = JammerFolder.GetIsLoop();
    static public bool isMuted = JammerFolder.GetIsMuted();
    static public float oldVolume = JammerFolder.GetOldVolume();
    static bool running = false;
    static public string audioFilePath = "";
    static public double currentPositionInSeconds = 0.0;
    static double positionInSeconds = 0.0;
    static int pMinutes = 0;
    static int pSeconds = 0;
    static public string positionInSecondsText = "";
    static long newPosition;
    static bool isPlaying = false;
    static public string[] songs = {""};
    static public int currentSongArgs = 0;
    static public bool wantPreviousSong = false;
    static bool cantDo = true; // used that you cant spam Controls() with multiple threads 
    static public string textRenderedType = "normal";
    static public int forwardSeconds = JammerFolder.GetForwardSeconds();
    static public int rewindSeconds = JammerFolder.GetRewindSeconds();
    static public float changeVolumeBy = JammerFolder.GetChangeVolumeBy();
    static public bool isShuffle = JammerFolder.GetIsShuffle();

    static void Main(string[] args)
    {

        JammerFolder.CheckJammerFolderExists();
        if (args.Length == 0)
        {
            AnsiConsole.WriteLine("No songs given");
            ConstrolsWithoutSongs();
            return;
        }

        if (args[0] == "start")
        {
            AnsiConsole.WriteLine("Starting Jammer folder...");
            JammerFolder.OpenJammerFolder();
            return;
        }
        if (args[0] == "selfdestruct")
        {
            AnsiConsole.WriteLine("Self destructing Jammer :(");
            // if on windows run C:\Program Files (x86)\jammer\Uninstall.exe
            if (System.Environment.OSVersion.Platform == System.PlatformID.Win32NT)
            {
                System.Diagnostics.Process.Start("C:\\Program Files (x86)\\jammer\\selfdestruct.bat");
                Environment.Exit(0);
            }
            return;
        }
        // absoulutify arg if its a relative path and add https:// if url
        args = AbsolutefyPath.Absolutefy(args);
    
        songs = args;
        audioFilePath = songs[currentSongArgs];

        audioFilePath = URL.CheckIfURL(audioFilePath);
        if (audioFilePath == "Soundcloud Playlist") {
            // remove currentsongargs +1 from songs
            string[] currentSong = new string[songs.Length - 1];
            for (int i = 0; i < currentSongArgs; i++) {
                currentSong[i] = songs[i];
            }

            songs = currentSong;
            AnsiConsole.WriteLine("Soundcloud Playlist");
            foreach (var song in songs) {
                AnsiConsole.WriteLine(song);
            }
            foreach (var song in URL.songs)
            {
                AnsiConsole.WriteLine(song);
            }
            // Console.ReadKey();
            // add URL.songs to songs array keep the songs content
            string[] newSongs = new string[songs.Length + URL.songs.Length];
            for (int i = 0; i < songs.Length; i++)
            {
                newSongs[i] = songs[i];
            }
            for (int i = 0; i < URL.songs.Length; i++)
            {
                newSongs[i + songs.Length] = URL.songs[i];
            }
            songs = newSongs;
            URL.jammerPath = "";
            Main(songs);
        }

        try
        {
            string extension = System.IO.Path.GetExtension(audioFilePath).ToLower();

            switch (extension)
            {
                case ".wav":
                    playFile.PlayWav(audioFilePath, volume, running);
                    break;
                case ".mp3":
                    playFile.PlayMp3(audioFilePath, volume, running);
                    break;
                case ".ogg":
                    playFile.PlayOgg(audioFilePath, volume, running);
                    break;
                case ".flac":
                    playFile.PlayFlac(audioFilePath, volume, running);
                    break;
                default:
                    ConstrolsWithoutSongs();
                    break;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }

    static public void ConstrolsWithoutSongs() {
        running = true;
        textRenderedType = "fakePlayer";            
        UI.Ui(null);
        cantDo = false;
        while (running) {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                HandleUserInput(key, null, null);
            }
        }
        AnsiConsole.Clear();
        AnsiConsole.WriteLine("Stopped");


    }

    static public void Controls(WaveOutEvent outputDevice, object reader)
    {
        UI.ForceUpdate();
        try
        {
            WaveStream audioStream;
            running = true;
            if (reader is WaveStream)
            {
                audioStream = (WaveStream)reader;
            }
            else
            {
                audioStream = null;
            }

            if (audioStream == null)
            {
                // Handle the case where the reader is not a WaveStream.
                return;
            }

            positionInSeconds = audioStream.TotalTime.TotalSeconds;
            pMinutes = (int)(positionInSeconds / 60);
            pSeconds = (int)(positionInSeconds % 60);
            positionInSecondsText = $"{pMinutes}:{pSeconds:D2}";
            JammerFolder.SaveSettings();

            cantDo = false;
            if (isMuted) {
                outputDevice.Volume = 0.0f;
            } else {
                outputDevice.Volume = volume;
            }
            while (running)
            {
                // if outputDevice is Error: NAudio.MmException: BadDeviceId calling waveOutGetVolume
                if (outputDevice != null && audioStream != null)
                {
                    try
                    {
                        // settings and help screen lags
                        if (textRenderedType != "settings") {
                            UI.Update();
                            UI.Ui(outputDevice);
                        }
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.WriteException(ex);
                    }

                    currentPositionInSeconds = audioStream.CurrentTime.TotalSeconds;
                    positionInSeconds = audioStream.TotalTime.TotalSeconds;

                    if (audioStream.Position >= audioStream.Length)
                    {
                        if (isLoop)
                        {
                            audioStream.Position = 0;
                        }
                        else
                        {
                            if (songs != null && songs.Length > 1)
                            {
                                running = false;
                            }

                            SetState(outputDevice, "stopped", audioStream);
                        }
                    }

                    if (outputDevice?.PlaybackState == PlaybackState.Stopped && isPlaying)
                    {
                        outputDevice.Init(audioStream);

                        if (audioStream.Position < audioStream.Length)
                        {
                            SetState(outputDevice, "playing", audioStream);
                        }
                    }

                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true).Key;

                        if (outputDevice != null)
                        {
                            HandleUserInput(key, audioStream, outputDevice);
                        }
                    }
                    // UI.UpdateSongList();
                    Thread.Sleep(1); // Don't hog the CPU
                }
            }
            // AnsiConsole.Clear();
            AnsiConsole.WriteLine("Stopped");
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }

        // --------- NEXT SONG ---------
        cantDo = true; // used that you cant spam Controls() with multiple threads
        try
        {
            outputDevice?.Stop();
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }

        if (outputDevice != null) {
            if (isShuffle) // if shuffle
            {
                if (songs.Length > 1)
                {
                    // get random song that isn't the current song
                    Random rnd = new Random();
                    int randomNum = rnd.Next(0, 100 * songs.Length);
                    int randomSong = randomNum % songs.Length;

                    while (randomSong == currentSongArgs)
                    {
                        randomNum = rnd.Next(0, 100 * songs.Length);
                        randomSong = randomNum % songs.Length;
                    }
                    currentSongArgs = randomSong;
                    audioFilePath = songs[currentSongArgs];
                    Main(songs);
                } else { // if only one song
                    currentSongArgs = 0;
                    audioFilePath = songs[currentSongArgs];
                    Main(songs);
                }
            }
            else {
                if (wantPreviousSong) // if previous song
                {
                    wantPreviousSong = false;
                    if (songs.Length > 1) // if more than one song
                    {
                        currentSongArgs--;
                        if (currentSongArgs < 0)
                        {
                            currentSongArgs = songs.Length - 1;
                        }
                        audioFilePath = songs[currentSongArgs];
                        wantPreviousSong = false;
                        Main(songs);
                    }
                    else // if only one song
                    {
                        currentSongArgs = 0;
                        audioFilePath = songs[currentSongArgs];
                        wantPreviousSong = false;
                        Main(songs);
                    }
                }
                // start next song
                if (songs.Length > 1) // if more than one song
                {
                    currentSongArgs++;
                    if (currentSongArgs < songs.Length)
                    {
                        audioFilePath = songs[currentSongArgs];
                        Main(songs);
                    }
                    else // if last song
                    {
                        currentSongArgs = 0;
                        audioFilePath = songs[currentSongArgs];
                        Main(songs);
                    }
                }
            }
        }
        cantDo = false;
    }


    static void HandleUserInput(ConsoleKey key, WaveStream audioStream, WaveOutEvent outputDevice)
    {
        // AnsiConsole.WriteLine("key pressed");
        if (cantDo)
        {
            return;
        }
        switch (key)
        {
            case ConsoleKey.UpArrow: // volume up
                if (outputDevice == null || audioStream == null) { break; }
                if (isMuted)
                {
                    isMuted = false;
                    outputDevice.Volume = oldVolume;
                }
                else
                {
                    outputDevice.Volume = Math.Min(outputDevice.Volume + changeVolumeBy, 1.0f);
                    volume = outputDevice.Volume;
                }
                break;
            case ConsoleKey.DownArrow: // volume down
                if (outputDevice == null || audioStream == null) { break; }
                if (isMuted)
                {
                    isMuted = false;
                    outputDevice.Volume = oldVolume;
                }
                else
                {
                    outputDevice.Volume = Math.Max(outputDevice.Volume - changeVolumeBy, 0.0f);
                    volume = outputDevice.Volume;
                }
                break;
            case ConsoleKey.LeftArrow: // rewind
                if (outputDevice == null || audioStream == null) { break; }
                newPosition = audioStream.Position - (audioStream.WaveFormat.AverageBytesPerSecond * rewindSeconds);

                if (newPosition < 0)
                {
                    newPosition = 0; // Go back to the beginning if newPosition is negative
                }

                if (audioStream.Position < audioStream.Length || outputDevice.PlaybackState == PlaybackState.Stopped)
                {
                    try {
                        if (outputDevice.PlaybackState != PlaybackState.Playing)
                        {
                            outputDevice.Init(audioStream);
                        }
                    } catch (Exception ex) {
                        AnsiConsole.WriteException(ex);
                    }
                    SetState(outputDevice, "playing", audioStream);
                }

                audioStream.Position = newPosition;
                break;
            case ConsoleKey.RightArrow: // fast forward
                if (outputDevice == null || audioStream == null) { break; }
                newPosition = audioStream.Position + (audioStream.WaveFormat.AverageBytesPerSecond * forwardSeconds);

                if (newPosition > audioStream.Length)
                {
                    newPosition = audioStream.Length;
                    if (isLoop)
                    {
                        audioStream.Position = 0;
                    }
                    else
                    {
                        SetState(outputDevice, "stopped", audioStream);
                    }
                }

                audioStream.Position = newPosition;
                break;
            case ConsoleKey.Spacebar: // toggle play/pause
                if (outputDevice == null || audioStream == null) { break; }
                if (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    SetState(outputDevice, "paused", audioStream);
                }
                else
                {
                    if (outputDevice.PlaybackState == PlaybackState.Stopped)
                    {
                        audioStream.Position = 0;
                    }
                    SetState(outputDevice, "playing", audioStream);
                }
                break;
            case ConsoleKey.Q: // quit
                Console.Clear();
                Environment.Exit(0);
                break;
            case ConsoleKey.L: // loop
                isLoop = !isLoop;
                break;
            case ConsoleKey.M: // mute
                if (outputDevice == null || audioStream == null) { break; }
                if (isMuted)
                {
                    isMuted = false;
                    outputDevice.Volume = oldVolume;
                }
                else
                {
                    isMuted = true;
                    oldVolume = outputDevice.Volume;
                    outputDevice.Volume = 0.0f;
                }
                break;
            case ConsoleKey.N: // next song
                if (outputDevice == null || audioStream == null) { break; }
                if (songs.Length == 1) { break; }
                wantPreviousSong = false;
                running = false;
                break;
            case ConsoleKey.P: // previous song
                if (outputDevice == null || audioStream == null) { break; }
                if (songs.Length == 1) { break;}
                wantPreviousSong = true;
                running = false;
                break;
            case ConsoleKey.R: // shuffle
                if (outputDevice == null || audioStream == null) { break; }
                if (songs.Length == 1) { break; }
                Random rnd = new Random();
                int randomSong = rnd.Next(0, songs.Length);
                while (currentSongArgs == randomSong)
                {
                    randomSong = rnd.Next(0, songs.Length);
                }
                currentSongArgs = randomSong - 1;
                audioFilePath = songs[currentSongArgs];
                running = false;
                break;
            case ConsoleKey.H: // help
                switch (textRenderedType)
                {
                    case "settings":
                        textRenderedType = "help";
                        break;
                    case "help":
                        if (outputDevice == null)
                        {
                            textRenderedType = "fakePlayer";
                            break;
                        }
                        textRenderedType = "normal";
                        break;
                    case "normal":
                        textRenderedType = "help";
                        break;
                    case "fakePlayer":
                        textRenderedType = "help";
                        break;
                }
                break;
            case ConsoleKey.C: //settings
                switch (textRenderedType)
                {
                    case "settings":
                        if (outputDevice == null) 
                        {
                            textRenderedType = "fakePlayer";
                            break;
                        }
                        textRenderedType = "normal";
                        break;
                    case "help":
                        textRenderedType = "settings";
                        break;
                    case "normal":
                        textRenderedType = "settings";
                        break;
                    case "fakePlayer":
                        textRenderedType = "settings";
                        break;
                }
                break;
            case ConsoleKey.O: // add song to playlist
                AnsiConsole.Markup("\nEnter song to add to playlist: ");
                string songToAdd = Console.ReadLine();
                if (songToAdd == "" || songToAdd == null) { break; }
                // remove "" from start and end
                if (songToAdd.StartsWith("\"") && songToAdd.EndsWith("\""))
                {
                    songToAdd = songToAdd.Substring(1, songToAdd.Length - 2);
                }
                songToAdd = AbsolutefyPath.Absolutefy(new string[] { songToAdd })[0];
                // break if file doesnt exist or its not a valid soundcloud url
                if (!File.Exists(songToAdd) && !URL.IsSoundCloudUrlValid(songToAdd)) { break; }
                if (URL.IsSoundCloudUrlValid(songToAdd)) {
                    // splice ? and everything after it
                    int index = songToAdd.IndexOf("?");
                    if (index > 0)
                    {
                        songToAdd = songToAdd.Substring(0, index);
                    }
                }
                // add song to playlist
                string[] newSongs = new string[songs.Length + 1];
                for (int i = 0; i < songs.Length; i++)
                {
                    newSongs[i] = songs[i];
                }
                newSongs[songs.Length] = songToAdd;
                songs = newSongs;

                // delete duplicates
                songs = Array.FindAll(songs, s => !string.IsNullOrEmpty(s));
                songs = new HashSet<string>(songs).ToArray();

                if (outputDevice == null) { // if no song is playing
                    running = false;
                    textRenderedType = "normal";
                    Main(songs);
                }
                break;
            case ConsoleKey.S: // shuffle
                isShuffle = !isShuffle;
                break;
            case ConsoleKey.D1: // set refresh rate
                AnsiConsole.Markup("\nEnter refresh rate [grey](50 is about 1 sec)[/]: ");
                string refreshRate = Console.ReadLine();
                try
                {
                    int refreshRateInt = int.Parse(refreshRate);
                    UI.refreshTimes = refreshRateInt;
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteException(ex);
                }
                break;
            case ConsoleKey.D2: // set forward seconds
                AnsiConsole.Markup("\nEnter forward seconds: ");
                string forwardSecondsString = Console.ReadLine();
                try
                {
                    int forwardSecondsInt = int.Parse(forwardSecondsString);
                    forwardSeconds = forwardSecondsInt;
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteException(ex);
                }
                break;
            case ConsoleKey.D3: // set rewind seconds
                AnsiConsole.Markup("\nEnter rewind seconds: ");
                string rewindSecondsString = Console.ReadLine();
                try
                {
                    int rewindSecondsInt = int.Parse(rewindSecondsString);
                    rewindSeconds = rewindSecondsInt;
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteException(ex);
                }
                break;
            case ConsoleKey.D4: // set change volume by
                AnsiConsole.Markup("\nEnter change volume by: ");
                string changeVolumeByString = Console.ReadLine();
                try
                {
                    float changeVolumeByFloat = float.Parse(changeVolumeByString) / 100;
                    changeVolumeBy = changeVolumeByFloat;
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteException(ex);
                }
                break;

        }
        JammerFolder.SaveSettings();

        UI.ForceUpdate();
        UI.Ui(outputDevice);
    }

    static public void SetState(WaveOutEvent outputDevice, string state, WaveStream audioStream)
    {
        if (state == "playing")
        {
            // if not initialized
            if (outputDevice.PlaybackState == PlaybackState.Stopped)
            {
                outputDevice.Init(audioStream);
            }
            outputDevice.Play();
            isPlaying = true;
        }
        else if (state == "paused")
        {
            outputDevice.Pause();
            isPlaying = false;
        }
        else if (state == "stopped")
        {
            outputDevice.Stop();
            isPlaying = false;
        }
    }
}