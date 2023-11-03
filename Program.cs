using System;
using System.Threading;
using NAudio.Wave;
using NAudio.Vorbis;
using NVorbis;
using NAudio.Utils;
class Program
{
    static Double volume = 0.5f;
    static bool running = false;
    static WaveOutEvent outputDevice = new WaveOutEvent();
    static string audioFilePath = "";
    static bool isLoop = false;
    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: jammer <path_to_audio_file>");
            return;
        }

        audioFilePath = args[0];

        try
        {
            string extension = System.IO.Path.GetExtension(audioFilePath).ToLower();

            switch (extension)
            {

                case ".wav":
                    PlayWav(audioFilePath);
                    break;
                case ".mp3":
                    PlayMp3(audioFilePath);
                    break;
                case ".ogg":
                    PlayOgg(audioFilePath);
                    break;
                case ".flac":
                    PlayFlac(audioFilePath);
                    break;
                default:
                    Console.WriteLine("Unknown file extension: " + extension);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static void PlayWav(string audioFilePath)
    {
        using (var reader = new WaveFileReader(audioFilePath))
        using (var outputDevice = new WaveOutEvent())
        {
            outputDevice.Init(reader);
            outputDevice.Play();

            // Handle key events for volume adjustment
            outputDevice.PlaybackStopped += (sender, e) => { outputDevice.Dispose(); };
            outputDevice.Volume = 0.5f;
            running = true;

            Thread thread = new Thread(() => controls(volume, running, outputDevice, reader));
            thread.Start();

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            manualEvent.WaitOne();
        }
    }

    static void PlayMp3(string audioFilePath)
    {
        using (var reader = new Mp3FileReader(audioFilePath))
        using (var outputDevice = new WaveOutEvent())
        {
            outputDevice.Init(reader);
            outputDevice.Play();

            // Handle key events for volume adjustment
            outputDevice.PlaybackStopped += (sender, e) => { outputDevice.Dispose(); };
            outputDevice.Volume = 0.5f;
            running = true;

            Thread thread = new Thread(() => controls(volume, running, outputDevice, reader));
            thread.Start();

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            manualEvent.WaitOne();
        }
    }

    static void PlayOgg(string audioFilePath)
    {
        using (var reader = new NAudio.Vorbis.VorbisWaveReader(audioFilePath))
        using (var outputDevice = new WaveOutEvent())
        {
            outputDevice.Init(reader);
            outputDevice.Play();

            // Handle key events for volume adjustment
            outputDevice.PlaybackStopped += (sender, e) => { outputDevice.Dispose(); };
            outputDevice.Volume = 0.5f;
            running = true;

            Thread thread = new Thread(() => controls(volume, running, outputDevice, reader));
            thread.Start();

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            manualEvent.WaitOne();


        }
    }

    static void PlayFlac(string audioFilePath)
    {
        using (var reader = new AudioFileReader(audioFilePath))
        using (var outputDevice = new WaveOutEvent())
        {
            outputDevice.Init(reader);
            outputDevice.Play();

            // Handle key events for volume adjustment
            outputDevice.PlaybackStopped += (sender, e) => { outputDevice.Dispose(); };
            outputDevice.Volume = 0.5f;
            running = true;

            Thread thread = new Thread(() => controls(volume, running, outputDevice, reader));
            thread.Start();

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            manualEvent.WaitOne();
        }
    }

    static void controls(Double volume, bool running, WaveOutEvent outputDevice, Object reader)
    {
        WaveStream audioStream = (WaveStream)reader;
        long newPosition;
        Console.WriteLine("Press 'Up Arrow' to increase volume, 'Down Arrow' to decrease volume, and 'Q' to quit.");
        Console.WriteLine("looping: " + isLoop);
        while (running)
        {
            if (audioStream.Position >= audioStream.Length)
            {
                if (isLoop)
                {
                    audioStream.Position = 0;
                }
                else
                {
                    Console.WriteLine("Song ended.");
                }
            }
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        outputDevice.Volume = Math.Min(outputDevice.Volume + 0.05f, 1.0f);
                        break;
                    case ConsoleKey.DownArrow:
                        outputDevice.Volume = Math.Max(outputDevice.Volume - 0.05f, 0.0f);
                        break;
                    case ConsoleKey.LeftArrow:
                        newPosition = audioStream.Position - (audioStream.WaveFormat.AverageBytesPerSecond * 5);

                        // song stops when goes over the end, so restart it
                        if (outputDevice.PlaybackState == PlaybackState.Stopped)
                        {
                            outputDevice.Init(audioStream);
                            outputDevice.Play();
                        }
                        if (newPosition < 0)
                        {
                            newPosition = 0; // Go back to the beginning if newPosition is negative
                        }

                        audioStream.Position = newPosition;
                        break;
                    case ConsoleKey.RightArrow:
                        newPosition = audioStream.Position + (audioStream.WaveFormat.AverageBytesPerSecond * 5);

                        if (newPosition > audioStream.Length)
                        {
                            newPosition = audioStream.Length; // Go back to the beginning if newPosition is negative
                        }

                        audioStream.Position = newPosition;
                        break;
                    case ConsoleKey.Spacebar:
                        if (outputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            outputDevice.Pause();
                        }
                        else
                        {
                            outputDevice.Play();
                        }
                        break;
                    case ConsoleKey.Q:
                        running = false;
                        break;
                    case ConsoleKey.L:
                        isLoop = !isLoop;
                        Console.WriteLine("looping: " + isLoop);
                        break;
                }
            }
            Thread.Sleep(10); // don't hog the CPU
        }
    }
}
    