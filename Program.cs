using jammer;
using NVorbis;
using NAudio.Wave;
using NAudio.Utils;
using NAudio.Vorbis;
using System.Windows;
using Spectre.Console;
using NAudio.CoreAudioApi;
using System.Runtime.InteropServices;
using System.ComponentModel.DataAnnotations;
using System;
using System.Diagnostics;
using System.Threading;
using System.Management;


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

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.Write("Example: jammer npc_music/Unity.wav");
            AnsiConsole.Write("Example: jammer soundcloud.com/username/track");
            Environment.Exit(0);
        }

        JammerFolder.CheckJammerFolderExists();

        audioFilePath = args[0];

        if (audioFilePath == "start")
        {
            AnsiConsole.WriteLine("Starting Jammer folder...");
            JammerFolder.OpenJammerFolder();
            return;
        }
        // audioFilePath = "npc_music/Unity.wav";
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

    static public void Controls(bool running, WaveOutEvent outputDevice, object reader)
    {
        WaveStream audioStream = (WaveStream)reader;
        positionInSeconds = audioStream.TotalTime.TotalSeconds;
        pMinutes = (int)(positionInSeconds / 60);
        pSeconds = (int)(positionInSeconds % 60);
        positionInSecondsText = $"{pMinutes}:{pSeconds:D2}";
        JammerFolder.SaveSettings(isLoop, outputDevice.Volume, isMuted, oldVolume);
        
        try
        {
            while (running)
            {
                try {
                    // if outputDevice is Error: NAudio.MmException: BadDeviceId calling waveOutGetVolume
                    if ( outputDevice != null) {
                        AnsiConsole.Clear();
                        UI.Ui(outputDevice);
                    }
                }
                catch (Exception ex) {
                    AnsiConsole.WriteException(ex);
                }
                currentPositionInSeconds = audioStream.CurrentTime.TotalSeconds;
                positionInSeconds = audioStream.TotalTime.TotalSeconds;

                if (audioStream.Position >= audioStream.Length) // if at the end of the audio file
                {
                    if (isLoop)
                    {
                        audioStream.Position = 0;
                    }
                    else
                    {
                        if (outputDevice != null)
                        {
                            SetState(outputDevice, "stopped", audioStream);
                        }
                    }
                }

                if (outputDevice?.PlaybackState == PlaybackState.Stopped && isPlaying)
                {
                    outputDevice.Init(audioStream);
                    // if not in the end
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

                Thread.Sleep(1); // don't hog the CPU
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }

    static void HandleUserInput(ConsoleKey key, WaveStream audioStream, WaveOutEvent outputDevice)
    {
        switch (key)
        {
            case ConsoleKey.UpArrow:
                if (isMuted)
                {
                    isMuted = false;
                    outputDevice.Volume = oldVolume;
                }
                else
                {
                    outputDevice.Volume = Math.Min(outputDevice.Volume + 0.05f, 1.0f);
                }
                break;
            case ConsoleKey.DownArrow:
                if (isMuted)
                {
                    isMuted = false;
                    outputDevice.Volume = oldVolume;
                }
                else
                {
                    outputDevice.Volume = Math.Max(outputDevice.Volume - 0.05f, 0.0f);
                }
                break;
            case ConsoleKey.LeftArrow:
                newPosition = audioStream.Position - (audioStream.WaveFormat.AverageBytesPerSecond * 5);

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
            case ConsoleKey.RightArrow:
                newPosition = audioStream.Position + (audioStream.WaveFormat.AverageBytesPerSecond * 5);

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
            case ConsoleKey.Spacebar:
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
            case ConsoleKey.Q:
                Console.Clear();
                Environment.Exit(0);
                break;
            case ConsoleKey.L:
                isLoop = !isLoop;
                break;
            case ConsoleKey.M:
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
        }
        JammerFolder.SaveSettings(isLoop, outputDevice.Volume, isMuted, oldVolume);
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