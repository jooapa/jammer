using System.Threading;
using NAudio.Wave;
using NAudio.Vorbis;
using NVorbis;
using NAudio.Utils;
using System.Runtime.InteropServices;
class Program
{
    static float volume = 0.1f;
    static bool running = false;
    static WaveOutEvent outputDevice = new WaveOutEvent();
    static string audioFilePath = "";
    static bool isLoop = false;
    static Double currentPositionInSeconds = 0.0f;
    static Double positionInSeconds = 0.0f;
    // positionInSeconds
    static int pMinutes = 0;
    static int pSeconds = 0;
    static string positionInSecondsText = "";
    static long newPosition;
    static bool isPlaying = false;
    static bool isMuted = false;
    static float oldVolume = 0.0f;


    enum musicState
    {
        Stopped,
        Playing,
        Paused
    }

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
            outputDevice.Volume = volume;
            running = true;

            Thread thread = new Thread(() => Controls(volume, running, outputDevice, reader));
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
            outputDevice.Volume = volume;
            running = true;

            Thread thread = new Thread(() => Controls(volume, running, outputDevice, reader));
            thread.Start();

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            manualEvent.WaitOne();
        }
    }

    static void PlayOgg(string audioFilePath)
    {
        try
        {
            using (var reader = new NAudio.Vorbis.VorbisWaveReader(audioFilePath))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(reader);
                outputDevice.Play();

                // Handle key events for volume adjustment
                outputDevice.PlaybackStopped += (sender, e) => { outputDevice.Dispose(); };
                outputDevice.Volume = volume;
                running = true;

                Thread thread = new Thread(() => Controls(volume, running, outputDevice, reader));
                thread.Start();

                ManualResetEvent manualEvent = new ManualResetEvent(false);
                manualEvent.WaitOne();


            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
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
            outputDevice.Volume = volume;
            running = true;

            Thread thread = new Thread(() => Controls(volume, running, outputDevice, reader));
            thread.Start();

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            manualEvent.WaitOne();
        }
    }

    static void Controls(Double volume, bool running, WaveOutEvent outputDevice, Object reader)
    {
        WaveStream audioStream = (WaveStream)reader;
        positionInSeconds = (double)audioStream.Length / audioStream.WaveFormat.AverageBytesPerSecond;
        pMinutes = (int)(positionInSeconds / 60);
        pSeconds = (int)(positionInSeconds % 60);
        positionInSecondsText = $"{pMinutes}:{pSeconds:D2}";

        try
        {
            while (running)
            {
                currentPositionInSeconds = (double)audioStream.Position / audioStream.WaveFormat.AverageBytesPerSecond;
                positionInSeconds = (double)audioStream.Length / audioStream.WaveFormat.AverageBytesPerSecond;

                if (audioStream.Position >= audioStream.Length) // if at the end of the audio file
                {
                    if (isLoop)
                    {
                        audioStream.Position = 0;
                    }
                    else
                    {
                        outputDevice.Stop();
                        isPlaying = false;
                    }
                }

                if (outputDevice.PlaybackState == PlaybackState.Stopped)
                {
                    outputDevice.Init(audioStream);
                    // if not in the end
                    if (audioStream.Position < audioStream.Length)
                    {
                        outputDevice.Play();
                    }
                }

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    HandleUserInput(key, audioStream, outputDevice, newPosition);
                }

                Console.Clear();
                Console.WriteLine(UI(outputDevice));
                Thread.Sleep(100); // don't hog the CPU
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERORROROOR: " + ex);
        }
    }

    static string UI(WaveOutEvent outputDevice)
    {
        var loopText = isLoop ? "looping: true" : "looping: false";
        var isPlayingText = outputDevice.PlaybackState == PlaybackState.Playing ? "Playing" : "Paused";
        var ismuteText = isMuted ? "Muted" : "";

        // currentPositionInSeconds
        int cupMinutes = (int)(currentPositionInSeconds / 60);
        int cupSeconds = (int)(currentPositionInSeconds % 60);

        string currentPositionInSecondsText = $"{cupMinutes}:{cupSeconds:D2}";


        return "Current Position: " + currentPositionInSecondsText + " / " + positionInSecondsText + " minutes\n" +
        "\nPress 'Up Arrow' to increase volume, 'Down Arrow' to decrease volume, and 'Q' to quit.\n" +
        "Press 'Left Arrow' to rewind 5 seconds, 'Right Arrow' to fast forward 5 seconds.\n" +
        loopText + "\n" +
        isPlayingText + "\n" +
        "Volume: " + Math.Round(outputDevice.Volume * 100) + "%" + "\n" +
        ismuteText;

    }

    static void HandleUserInput(ConsoleKey key, WaveStream audioStream, WaveOutEvent outputDevice, long newPosition)
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

                audioStream.Position = newPosition;
                break;
            case ConsoleKey.RightArrow:
                newPosition = audioStream.Position + (audioStream.WaveFormat.AverageBytesPerSecond * 5);

                if (newPosition > audioStream.Length)
                {
                    newPosition = audioStream.Length;
                    outputDevice.Stop();
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
                    if (outputDevice.PlaybackState == PlaybackState.Stopped)
                    {
                        audioStream.Position = 0;
                    }
                    outputDevice.Play();
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
    }
}