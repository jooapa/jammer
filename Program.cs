using System.Threading;
using NAudio.Wave;
using NAudio.Vorbis;
using NVorbis;
using NAudio.Utils;
using System.Runtime.InteropServices;
using jammer;
class Program
{
    static float volume = 0.1f;
    static bool running = false;
    static WaveOutEvent outputDevice = new WaveOutEvent();
    static string audioFilePath = "";
    static public bool isLoop = false;
    static public Double currentPositionInSeconds = 0.0f;
    static Double positionInSeconds = 0.0f;
    // positionInSeconds
    static int pMinutes = 0;
    static int pSeconds = 0;
    static public string positionInSecondsText = "";
    static long newPosition;
    static bool isPlaying = false;
    static public bool isMuted = false;
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
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static public void Controls(Double volume, bool running, WaveOutEvent outputDevice, Object reader)
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
                Console.WriteLine(UI.Ui(outputDevice));
                Thread.Sleep(100); // don't hog the CPU
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERORROROOR: " + ex);
        }
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