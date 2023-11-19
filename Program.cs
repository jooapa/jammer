using jammer;
using NAudio.Wave;
using SoundCloudExplode.Playlists;
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
    static public string[] oldArgs = {""};
    static public string deleteSong = "";
    static int gotoSong = -100;

    static public void Main(string[] args)
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

        if (args[0] == "playlist")
        {
            if (args.Length < 2)
            {
                AnsiConsole.WriteLine("No playlist command given");
            }
            else
            {
                switch (args[1])
                {
                    case "play":
                        if (args.Length < 3)
                        {
                            AnsiConsole.WriteLine("No playlist name given");
                            return;
                        }
                        Playlists.Play(args[2]);
                        return;
                    case "create":
                        if (args.Length < 3)
                        {
                            AnsiConsole.WriteLine("No playlist name given");
                            return;
                        }
                        Playlists.Create(args[2]);
                        return;
                    case "delete":
                        if (args.Length < 3)
                        {
                            AnsiConsole.WriteLine("No playlist name given");
                            return;
                        }
                        Playlists.Delete(args[2]);
                        return;
                    case "add":
                        if (args.Length < 4)
                        {
                            AnsiConsole.WriteLine("No playlist name or song given");
                            return;
                        }
                        Playlists.Add(args);
                        return;
                    case "remove":
                        if (args.Length < 4)
                        {
                            AnsiConsole.WriteLine("No playlist name or song given");
                            return;
                        }
                        Playlists.Remove(args);
                        return;
                    case "show":
                        if (args.Length < 3)
                        {
                            AnsiConsole.WriteLine("No playlist name given");
                            return;
                        }
                        Playlists.Show(args[2]);
                        return;
                    case "list":
                        Playlists.List();
                        return;
                }
            }

            var table = new Table();
            table.AddColumn("Commands");
            table.AddColumn("Description");

            table.AddRow("jammer playlist play <name>", "Play playlist");
            table.AddRow("jammer playlist create <name>", "Create playlist");
            table.AddRow("jammer playlist delete <name>", "Delete playlist");
            table.AddRow("jammer playlist add <name> <song> ...", "Add songs to playlist");
            table.AddRow("jammer playlist remove <name> <song> ...", "Remove songs from playlist");
            table.AddRow("jammer playlist show <name>", "Show songs in playlist");
            table.AddRow("jammer playlist list", "List all playlists");

            AnsiConsole.Write(table);
            return;
        }
        
        if (args[0] == "selfdestruct")
        {
            AnsiConsole.WriteLine("Self destructing Jammer :(");
            // if on windows run Uninstall.exe on the current directory whwre jammer.exe is
            if (System.Environment.OSVersion.Platform == System.PlatformID.Win32NT)
            {
                System.Diagnostics.Process.Start("Uninstall.exe");
                Environment.Exit(0);
            }
            return;
        }
        
        if (args.Length > 1) {
            // if old args is not empty, add them to args
            if (oldArgs[0] != "") {
                Console.WriteLine("Old args is not empty");
                args = args.Concat(oldArgs).ToArray();
                oldArgs = new string[]{""};
            }
            if (URL.isValidSoundCloudPLaylist(args[0])) {
                Console.WriteLine("Soundcloud Playlist as first arg");
                // store old args but remove the first one
                oldArgs = args.Skip(1).ToArray();
                // remove all from args except the first one
                args = args.Take(1).ToArray();
            }
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

            // // delete song from playlist
            // if (deleteSong != "")
            // {
            //     List<string> songList = songs.ToList();
            //     songList.Remove(deleteSong);
            //     songs = songList.ToArray();
            //     deleteSong = "";
            // }
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
        if (gotoSong != -100) { // if goto song, used in playlist
            currentSongArgs = gotoSong - 1;
            if (currentSongArgs < 0) {
                currentSongArgs = songs.Length - 1;
            }
            gotoSong = -100;
        }
        try
        {
            outputDevice?.Stop();
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }

        if (outputDevice != null) {
            if (deleteSong != "")
            {
                List<string> songList = songs.ToList();
                songList.Remove(deleteSong);
                songs = songList.ToArray();
                deleteSong = "";
            }
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
            case ConsoleKey.F2: // pause
                PlaylistInput(outputDevice);
                break;
            case ConsoleKey.Escape: // exit to player
                if (outputDevice == null) { textRenderedType = "fakePlayer"; break;}
                textRenderedType = "normal";
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

        static void PlaylistInput(WaveOutEvent outputDevice)
        {
            // 1. add song to playlist
                // 1. song from url or file

            // 2. delete song from playlist
                // 1. current song
                // 2. song from url or file

            // 3. show songs in playlist

            // 4. list all playlists

            // 5. play other playlist

            // 6. save/replace playlist

            AnsiConsole.Markup("\nEnter playlist command: \n");
            AnsiConsole.MarkupLine("[grey]1. add song to playlist[/]");
            AnsiConsole.MarkupLine("[grey]2. delete song current song from playlist[/]");
            AnsiConsole.MarkupLine("[grey]3. show songs in playlist[/]");
            AnsiConsole.MarkupLine("[grey]4. list all playlists[/]");
            AnsiConsole.MarkupLine("[grey]5. play other playlist[/]");
            AnsiConsole.MarkupLine("[grey]6. save/replace playlist[/]");
            AnsiConsole.MarkupLine("[grey]7. goto song in playlist[/]");

            string playlistInput = "";
            playlistInput = Console.ReadLine();
            if (playlistInput == "" || playlistInput == null) { return; }
            switch (playlistInput)
            {
                case "1": // add song to playlist
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
                    if (URL.IsSoundCloudUrlValid(songToAdd))
                    {
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

                    if (outputDevice == null)
                    { // if no song is playing
                        running = false;
                        textRenderedType = "normal";
                        Main(songs);
                    }
                    break;
                case "2":
                    // delete current song from playlist
                    deleteSong = "";
                    if (currentSongArgs < songs.Length)
                    {
                        deleteSong = songs[currentSongArgs];
                        AnsiConsole.MarkupLine(deleteSong);
                    }
                    else
                    {
                        deleteSong = songs[0];
                    }

                    running = false;
                    // delete song will happen in Controls()
                    break;
                case "3":
                    AnsiConsole.Markup("\nEnter playlist name: ");
                    string playlistNameToShow = Console.ReadLine();
                    if (playlistNameToShow == "" || playlistNameToShow == null) { break; }
                    // show songs in playlist
                    Playlists.Show(playlistNameToShow);
                    AnsiConsole.Markup("\nPress any key to continue");
                    Console.ReadLine();
                    break;
                case "4":
                    // list all playlists
                    Playlists.List();
                    break;
                case "5":
                    AnsiConsole.Markup("\nEnter playlist name: ");
                    string playlistNameToPlay = Console.ReadLine();
                    if (playlistNameToPlay == "" || playlistNameToPlay == null) { break; }
                    // play other playlist
                    SetState(outputDevice, "stopped", null);
                    Playlists.Play(playlistNameToPlay);
                    break;
                case "6":
                    AnsiConsole.Markup("\nEnter playlist name: ");
                    string playlistNameToSave = Console.ReadLine();
                    if (playlistNameToSave == "" || playlistNameToSave == null) { break; }
                    // save playlist
                    Playlists.Save(playlistNameToSave, songs);
                    break;
                case "7":
                    AnsiConsole.Markup("\nEnter song to goto: ");
                    string songToGoto = Console.ReadLine();
                    if (songToGoto == "" || songToGoto == null) { break; }
                    // goto song in playlist
                    if (songs.Contains(songToGoto))
                    {
                        running = false;
                        gotoSong = Array.IndexOf(songs, songToGoto);
                    }
                    else
                    {
                        AnsiConsole.WriteLine("Song not in playlist");
                    }
                    break;
            }
        }
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