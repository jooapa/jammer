using ManagedBass;
using System.Text;
using IniParser;
using IniParser.Model;

namespace Jammer {
    public class Visual {
        public static string FileContent = @"[Audio Visualizer]
Enabled = true
BufferSize = 41000
; FFT32768 FFT16384           (Recommended)     detect 20Hz - 20kHz
; FFT8192  FFT4098  FFT2048                    detect 1kHz - 20kHz
; FFT1024  FFT512   FFT256   (Not Recommended) detect 10kHz - 20kHz
; if nothing shows up, try changing the 'FrequencyMultiplier'
DataFlags = FFT16384
MinFrequency = 50
MaxFrequency = 17000
FrequencyMultiplier = 1
";
        public static bool enabled = true; // Visualizer enabled flag
        public static int bufferSize = 41000; // FFT data buffer size
        public static string dataFlags = "Best"; // FFT data flags
        public static int minFrequency = 50; // Minimum frequency
        public static int maxFrequency = 17000; // Maximum frequency
        public static int frequencyMultiplier = 6000; // Frequency multiplier


        private static float scaleFactor = 1.0f;
        public static string GetSongVisual(int length, bool isPlaying = true)
        {
            // If the song is not playing, gradually decrease the scale factor
            if (!isPlaying)
            {
                scaleFactor *= 0.99f; // Adjust this value to control the speed of the decrease
            }
            else
            {
                scaleFactor = 1.0f; // Reset the scale factor when the song starts playing again
            }

            int _bufferSize = bufferSize; // FFT data buffer size
            var fftData = new float[_bufferSize]; // FFT data buffer

            // Retrieve FFT data from current music channel
            int bytesRead = Bass.ChannelGetData(Utils.currentMusic, fftData, (int)DataFlags.FFT16384);
            if (bytesRead <= 0)
            {
                // Handle error if data retrieval fails
                return "Error: Unable to retrieve FFT data.";
            }

            // Map FFT values to ASCII characters
            string[] unicodeMap = new string[] { " ", "▁", "▂", "▃", "▄", "▅", "▆", "▇", "█" };
            StringBuilder frequencyBuilder = new StringBuilder();

            // Calculate the number of frequencies to display based on the console width
            int frequencyCount = Math.Max(1, length);

            // Ensure frequencyCount is within bounds of the array
            frequencyCount = Math.Min(frequencyCount, fftData.Length);

            // Iterate through the FFT data and map values to ASCII characters
            for (int i = 0; i < frequencyCount; i++)
            {
                // If the length of the frequency string equals the console width, break the loop
                if (frequencyBuilder.Length == Math.Max(length - 43 , 1))
                {
                    break;
                }

                // Calculate the frequency for the current index using a logarithmic scale
                double frequency = minFrequency * Math.Pow(maxFrequency / minFrequency, (double)i / frequencyCount);

                // Find the closest index in the FFT data
                int fftIndex = (int)(frequency / 44100 * fftData.Length);

                // Ensure fftIndex is within bounds of the array
                fftIndex = Math.Min(fftIndex, fftData.Length - 1);

                // Get the FFT value at the calculated index
                float fftValue = fftData[fftIndex] * scaleFactor;

                // // Apply a power function to make higher values more sensitive
                fftValue = (float)Math.Pow(fftValue, 1.5);

                // Apply logarithmic scale to the FFT value
                float average = (float)Math.Log10(1 + fftValue * frequencyMultiplier);

                // Map the average value to an ASCII character
                int index = (int)(average * (unicodeMap.Length - 1));

                // Ensure index is within bounds of the array
                index = Math.Min(index, unicodeMap.Length - 1);

                // Append the mapped character to the frequency string
                frequencyBuilder.Append(unicodeMap[index]);
            }

            // Return the frequency string
            return frequencyBuilder.ToString();
        }

        public static void Write() {
            string path = Path.Combine(Utils.JammerPath, "Effects.ini");
            
            // Create the file if it doesn't exist
            if (!File.Exists(path)) {
                File.WriteAllText(path, FileContent);
            } 
        }
    }
}