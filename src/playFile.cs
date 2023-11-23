using NAudio.Wave;
using System.Diagnostics;
using NAudio.Wave.SampleProviders;


namespace jammer
{
    internal class playFile
    {
    static public Thread? thread;

        static public void StopThread()
        {
            Process.GetCurrentProcess().Kill();
        }
        static public void PlayWav(string audioFilePath, float volume, bool running)
        {
            using (var inputstream = new FileStream(audioFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new WaveFileReader(inputstream))
            using (var waveChannel = new WaveChannel32(reader))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(waveChannel);
                outputDevice.Play();

                // Handle key events for volume adjustment
                outputDevice.PlaybackStopped += (sender, e) => { outputDevice.Dispose(); };
                outputDevice.Volume = volume;
                running = true;

                Thread thread = new Thread(() => Program.Controls(outputDevice, reader));
                thread.Start();

                ManualResetEvent manualEvent = new ManualResetEvent(false);
                manualEvent.WaitOne();
            }
        }

        static public void PlayMp3(string audioFilePath, float volume, bool running)
        {
            using (var reader = new MediaFoundationReader(audioFilePath))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(reader);
                outputDevice.Play();

                // Handle key events for volume adjustment
                outputDevice.PlaybackStopped += (sender, e) => { outputDevice.Dispose(); };
                outputDevice.Volume = volume;
                running = true;

                Thread thread = new Thread(() => Program.Controls(outputDevice, reader));
                thread.Start();

                ManualResetEvent manualEvent = new ManualResetEvent(false);
                manualEvent.WaitOne();
            }
        }

        static public void PlayOgg(string audioFilePath, float volume, bool running)
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

                    Thread thread = new Thread(() => Program.Controls(outputDevice, reader));
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

        static public void PlayFlac(string audioFilePath, float volume, bool running)
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

                Thread thread = new Thread(() => Program.Controls(outputDevice, reader));
                thread.Start();

                ManualResetEvent manualEvent = new ManualResetEvent(false);
                manualEvent.WaitOne();
            }
        }

        static public void ConvertAudio(string inputFilePath, string outputFilePath, int targetSampleRate)
        {
            // remove sample rate changes from .mp3 file
            using (var reader = new MediaFoundationReader(inputFilePath))
            {
                var resampler = new MediaFoundationResampler(reader, new WaveFormat(targetSampleRate, reader.WaveFormat.Channels));
                WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
            }
        }
    }
}
