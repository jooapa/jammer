using System;
using NAudio.Wave;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace jammer
{
    internal class playFile
    {
    static public Thread thread;

        static public void StopThread()
        {
            Process.GetCurrentProcess().Kill();
        }
        static public void PlayWav(string audioFilePath, float volume, bool running)
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

                Thread thread = new Thread(() => Program.Controls(outputDevice, reader));
                thread.Start();

                ManualResetEvent manualEvent = new ManualResetEvent(false);
                manualEvent.WaitOne();
            }
        }

        static public void PlayMp3(string audioFilePath, float volume, bool running)
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
    }
}
