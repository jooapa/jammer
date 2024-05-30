using ManagedBass;
using System.Text;
using IniParser;
using IniParser.Model;
using Spectre.Console;

namespace Jammer {
    public class Visual {
        public static string FileContent = @"[Audio Visualizer]
; Refresh time in milliseconds
RefreshTime = 10
; Offset in milliseconds for the visualizer. 
; Increase this value if the visualizer is out of sync with the audio.
; Value can only be positive or zero. 0 >=
Offset = 0
; Unicode map for visualizer in the format ' ,▁,▂,▃,▄,▅,▆,▇,█'
UnicodeMap =  ,▁,▂,▃,▄,▅,▆,▇,█
; UnicodeMap = ▫,▪,▬,□,▢,▭,▣,▯,▤,▥,▮,█
; UnicodeMap =  ,⠁,⠁,⠃,⠇,⠏,⠛,⠿,⡿,⣿
; UnicodeMap =  ⣿,⡿,⡏,⠛,⠏,⠇,⠃,⠁
; UnicodeMap =  ,⣀,⣄,⣤,⣦,⣶,⣷,⣿
; Buffer size for FFT data
BufferSize = 41000
; FFT32768 FFT16384           (Recommended)     detect 20Hz - 20kHz
; FFT8192  FFT4098  FFT2048                    detect 1kHz - 20kHz
; FFT1024  FFT512   FFT256   (Not Recommended) detect 10kHz - 20kHz
; if nothing shows up, try changing the 'FrequencyMultiplier'
DataFlags = FFT16384
MinFrequency = 50
MaxFrequency = 17000
FrequencyMultiplier = 6000
";


        public static int refreshTime = 10; // Visualizer enabled flag
        public static int offset = 0; // Offset in milliseconds for the visualizer
        public static int bufferSize = 41000; // FFT data buffer size
        public static string dataFlags = "FFT16384"; // FFT data flags
        public static int minFrequency = 50; // Minimum frequency
        public static int maxFrequency = 17000; // Maximum frequency
        public static int frequencyMultiplier = 6000; // Frequency multiplier
        public static string unicodeMap = " ,▁,▂,▃,▄,▅,▆,▇,█";


        private static float scaleFactor = 1.0f;
        public static string GetSongVisual(int length, bool isPlaying = true)
        {
            // If the song is not playing, gradually decrease the scale factor
            if (!isPlaying)
            {
                scaleFactor *= 0.95f; // Adjust this value to control the speed of the decrease
            }
            else
            {
                scaleFactor = 1.0f; // Reset the scale factor when the song starts playing again
            }

            int _bufferSize = bufferSize; // FFT data buffer size
            var fftData = new float[_bufferSize]; // FFT data buffer

            // Retrieve FFT data from current music channel
            int bytesRead = Bass.ChannelGetData(Utils.currentMusic, fftData, (int)GetFFTDataFlags());
            if (bytesRead <= 0)
            {
                // Handle error if data retrieval fails
                return "Error: Unable to retrieve FFT data.";
            }

            // Map FFT values to ASCII characters
            string[] unicodeMap = GetUnicodeMap();

            // Calculate the number of frequencies to display based on the console width
            int frequencyCount = Math.Max(1, length);

            // Ensure frequencyCount is within bounds of the array
            frequencyCount = Math.Min(frequencyCount, fftData.Length);

            // Calculate this value once before the loop
            int maxLength = Math.Max(length - 43 , 1);

            StringBuilder frequencyBuilder = new StringBuilder(frequencyCount);
            
            // Iterate through the FFT data and map values to ASCII characters
            for (int i = 0; i < frequencyCount; i++)
            {
                // If the length of the frequency string equals the console width, break the loop
                if (frequencyBuilder.Length == maxLength)
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

        public static string[] GetUnicodeMap()
        {
            string[] map = unicodeMap.Split(',');

            if (map[0].Length == 0)
            {
                map[0] = " ";
            }

            return map; 
        }

        public static DataFlags GetFFTDataFlags()
        {
            switch (dataFlags)
            {
                case "FFT32768":
                    return DataFlags.FFT32768;
                case "FFT16384":
                    return DataFlags.FFT16384;
                case "FFT8192":
                    return DataFlags.FFT8192;
                case "FFT4096":
                    return DataFlags.FFT4096;
                case "FFT2048":
                    return DataFlags.FFT2048;
                case "FFT1024":
                    return DataFlags.FFT1024;
                case "FFT512":
                    return DataFlags.FFT512;
                case "FFT256":
                    return DataFlags.FFT256;
                default:
                    return DataFlags.FFT16384;
            }
        }

        public static void Write() {
            string path = Path.Combine(Utils.JammerPath, "Visualizer.ini");
            
            // Create the file if it doesn't exist
            if (!File.Exists(path)) {
                File.WriteAllText(path, FileContent, Encoding.UTF8);
            } 
        }

        public static void Read() {
            string path = Path.Combine(Utils.JammerPath, "Visualizer.ini");
            var parser = new FileIniDataParser();
            IniData data;

            if (!File.Exists(path)) {
                Write();
                data = parser.ReadFile(path);
            } else {
                data = parser.ReadFile(path);

                // Check if the desired entries exist, and add them if they don't
                if (!data["Audio Visualizer"].ContainsKey("RefreshTime"))
                {
                    data["Audio Visualizer"]["RefreshTime"] = "10";
                }
                if (!data["Audio Visualizer"].ContainsKey("BufferSize"))
                {
                    data["Audio Visualizer"]["BufferSize"] = "41000";
                }
                if (!data["Audio Visualizer"].ContainsKey("DataFlags"))
                {
                    data["Audio Visualizer"]["DataFlags"] = "FFT16384";
                }
                if (!data["Audio Visualizer"].ContainsKey("MinFrequency"))
                {
                    data["Audio Visualizer"]["MinFrequency"] = "50";
                }
                if (!data["Audio Visualizer"].ContainsKey("MaxFrequency"))
                {
                    data["Audio Visualizer"]["MaxFrequency"] = "17000";
                }
                if (!data["Audio Visualizer"].ContainsKey("FrequencyMultiplier"))
                {
                    data["Audio Visualizer"]["FrequencyMultiplier"] = "6000";
                }
                if (!data["Audio Visualizer"].ContainsKey("UnicodeMap"))
                {
                    data["Audio Visualizer"]["UnicodeMap"] = " ,▁,▂,▃,▄,▅,▆,▇,█";
                }
                if (!data["Audio Visualizer"].ContainsKey("Offset"))
                {
                    data["Audio Visualizer"]["Offset"] = "0";
                }

                // Write the data back to the file
                parser.WriteFile(path, data);
            }

            refreshTime = int.Parse(data["Audio Visualizer"]["RefreshTime"]);   
            bufferSize = int.Parse(data["Audio Visualizer"]["BufferSize"]);
            dataFlags = data["Audio Visualizer"]["DataFlags"];
            minFrequency = int.Parse(data["Audio Visualizer"]["MinFrequency"]);
            maxFrequency = int.Parse(data["Audio Visualizer"]["MaxFrequency"]);
            frequencyMultiplier = int.Parse(data["Audio Visualizer"]["FrequencyMultiplier"]);
            unicodeMap = data["Audio Visualizer"]["UnicodeMap"];
        }
    }
}