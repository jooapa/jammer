using System;
using System.Threading;
using NAudio.Wave;
using NAudio.Vorbis;
using NVorbis;
class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: Program.exe <path_to_audio_file>");
            return;
        }

        string audioFilePath = args[0];

        try
        {
            // Determine the audio format based on the file extension, and then play it.
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

            // Block the main thread until audio playback is complete
            outputDevice.PlaybackStopped += (sender, e) => { outputDevice.Dispose(); };
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

            // Block the main thread until audio playback is complete
            outputDevice.PlaybackStopped += (sender, e) => { outputDevice.Dispose(); };
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
            bool running = true;
            Console.WriteLine("Press 'Up Arrow' to increase volume, 'Down Arrow' to decrease volume, and 'Q' to quit.");
            while (running)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true).Key;
                    switch (key)
                    {
                        case ConsoleKey.UpArrow:
                            outputDevice.Volume = Math.Min(outputDevice.Volume + 0.1f, 1.0f);
                            Console.WriteLine("Volume: " + outputDevice.Volume);
                            break;
                        case ConsoleKey.DownArrow:
                            outputDevice.Volume = Math.Max(outputDevice.Volume - 0.1f, 0.0f);
                            Console.WriteLine("Volume: " + outputDevice.Volume);
                            break;
                        case ConsoleKey.Q:
                            running = false;
                            break;
                    }
                }
                Thread.Sleep(10);
            }
        }
    }

    static void PlayFlac(string audioFilePath)
    {
        using (var reader = new AudioFileReader(audioFilePath))
        using (var outputDevice = new WaveOutEvent())
        {
            outputDevice.Init(reader);
            outputDevice.Play();

            // Block the main thread until audio playback is complete
            outputDevice.PlaybackStopped += (sender, e) => { outputDevice.Dispose(); };
            ManualResetEvent manualEvent = new ManualResetEvent(false);
            manualEvent.WaitOne();
        }
    }

    static Double GetVolume(byte[] buffer)
    {
        int count = buffer.Length / 2;
        int format = 1;
        Double sum = 0;
        for (int i = 0; i < count; i++)
        {
            Int16 sample = BitConverter.ToInt16(buffer, i * 2);
            sum += (sample / 32768.0) * (sample / 32768.0);
        }
        return Math.Sqrt(sum / count);
    }   
}
