using jammer;
using System;
using NVorbis;
using NAudio.Wave;
using NAudio.Utils;
using NAudio.Vorbis;
using System.Windows;
using Spectre.Console;
using System.Threading;
using System.Management;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using System.Runtime.InteropServices;
using System.ComponentModel.DataAnnotations;


class Program
{
    static WaveOutEvent outputDevice = new WaveOutEvent();
    static float volume = JammerFolder.GetVolume();
    static public bool isLoop = JammerFolder.GetIsLoop();
    static public bool isMuted = JammerFolder.GetIsMuted();
    static float oldVolume = JammerFolder.GetOldVolume();
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
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.Write("Example: jammer npc_music/Unity.wav");
            AnsiConsole.Write("Example: jammer soundcloud.com/username/track");
            Environment.Exit(0);
        }

        JammerFolder.CheckJammerFolderExists();
        if (args[0] == "start")
        {
            AnsiConsole.WriteLine("Starting Jammer folder...");
            JammerFolder.OpenJammerFolder();
            return;
        }
        // absoulutify arg if its a relative path
        args = AbsolutefyPath.Absolutefy(args);
    
        songs = args;
        audioFilePath = args[currentSongArgs];

        AnsiConsole.WriteLine("args.Length: " + args.Length);


        audioFilePath = URL.CheckIfURL(audioFilePath);

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
                    Console.WriteLine("Unknown file extension: " + extension);
                    break;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }

    static public void Controls(WaveOutEvent outputDevice, object reader)
    {
        UI.ForceUpdate();
        try
        {
            running = true;
            WaveStream audioStream = reader as WaveStream;

            if (audioStream == null)
            {
                // Handle the case where the reader is not a WaveStream.
                return;
            }

            positionInSeconds = audioStream.TotalTime.TotalSeconds;
            pMinutes = (int)(positionInSeconds / 60);
            pSeconds = (int)(positionInSeconds % 60);
            positionInSecondsText = $"{pMinutes}:{pSeconds:D2}";
            JammerFolder.SaveSettings(isLoop, outputDevice.Volume, isMuted, oldVolume, UI.refreshTimes, forwardSeconds, rewindSeconds, changeVolumeBy);

            cantDo = false;

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
        cantDo = false;
    }


    static void HandleUserInput(ConsoleKey key, WaveStream audioStream, WaveOutEvent outputDevice)
    {
        if (cantDo)
        {
            return;
        }
        switch (key)
        {
            case ConsoleKey.UpArrow: // volume up
                if (isMuted)
                {
                    isMuted = false;
                    outputDevice.Volume = oldVolume;
                }
                else
                {
                    outputDevice.Volume = Math.Min(outputDevice.Volume + changeVolumeBy, 1.0f);
                }
                break;
            case ConsoleKey.DownArrow: // volume down
                if (isMuted)
                {
                    isMuted = false;
                    outputDevice.Volume = oldVolume;
                }
                else
                {
                    outputDevice.Volume = Math.Max(outputDevice.Volume - changeVolumeBy, 0.0f);
                }
                break;
            case ConsoleKey.LeftArrow: // rewind
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
                if (songs.Length == 1) { break; }
                wantPreviousSong = false;
                running = false;
                break;
            case ConsoleKey.P: // previous song
                if (songs.Length == 1) { break;}
                wantPreviousSong = true;
                running = false;
                break;
            case ConsoleKey.R: // shuffle
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
                        textRenderedType = "normal";
                        break;
                    case "normal":
                        textRenderedType = "help";
                        break;
                }
                break;
            case ConsoleKey.C: //settings
                switch (textRenderedType)
                {
                    case "settings":
                        textRenderedType = "normal";
                        break;
                    case "help":
                        textRenderedType = "settings";
                        break;
                    case "normal":
                        textRenderedType = "settings";
                        break;
                }
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
        JammerFolder.SaveSettings(isLoop, outputDevice.Volume, isMuted, oldVolume, UI.refreshTimes, forwardSeconds, rewindSeconds, changeVolumeBy);

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