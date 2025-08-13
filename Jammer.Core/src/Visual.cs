using ManagedBass;
using System.Text;
using IniParser;
using IniParser.Model;
using Spectre.Console;
using System.Globalization;

namespace Jammer
{
    public class Visual
    {
        public static string FileContent = @"[Audio Visualizer]
; Refresh time in milliseconds
RefreshTime = 35
; Buffer size for FFT data
; meaning higher values will detect more frequencies,
BufferSize = 41000
; Better FFT will be more accurate and smooth
; but can be out of sync with the audio
; Best:    FFT32768 FFT16384        (Recommended)
; Fast:    FFT8192  FFT4098 FFT2048  (Recommended)
; Fastest: FFT1024  FFT512  FFT256  (Not Recommended)
; if nothing shows up, try changing the 'FrequencyMultiplier'
DataFlags = FFT4098
MinFrequency = 50
MaxFrequency = 17000
FrequencyMultiplier = 900000000
; Logarithmic multiplier for FFT values, meaning
; higher values will be more sensitive
; combo with the 'FrequencyMultiplier' to adjust
; Note: the change in the Log value, 
; can be very sensitive with the 'FrequencyMultiplier'
LogarithmicMultiplier = 4
; When the song is paused, the visualizer will gradually decrease in scale
PausingEffect = true
";


        public static int refreshTime = 33; // Visualizer enabled flag
        public static int bufferSize = 41000; // FFT data buffer size
        public static string dataFlags = "FFT4098"; // FFT data flags
        public static int minFrequency = 50; // Minimum frequency
        public static int maxFrequency = 17000; // Maximum frequency
        public static int frequencyMultiplier = 900000000; // Frequency multiplier
        public static float logarithmicMultiplier = 4f; // Logarithmic multiplier
        public static bool pausingEffect = true; // Pausing effect flag

        private static float scaleFactor = 1.0f;
        public static string GetSongVisual(int length, bool isPlaying)
        {
            // If the song is not playing, gradually decrease the scale factor
            if (!isPlaying && pausingEffect)
            {
                scaleFactor *= (float)Math.Pow(0.95, Math.Max(1, refreshTime) / 6.0f);
            }
            else
            {
                scaleFactor = 1.0f; // Reset the scale factor when the song starts playing again
            }

            int _bufferSize = bufferSize; // FFT data buffer size
            var fftData = new float[_bufferSize]; // FFT data buffer

            // Retrieve FFT data from current music channel
            int bytesRead = Bass.ChannelGetData(Utils.CurrentMusic, fftData, (int)GetFFTDataFlags());
            if (bytesRead <= 0)
            {
                string songPath;
                if (Utils.CurrentMusic == 0 && Utils.CurSongError)
                {
                    songPath = "Error: cannot read FFT data";
                }
                else
                {
                    songPath = "";
                }

                return songPath;
            }

            // Map FFT values to ASCII characters
            string[] unicodeMap = GetUnicodeMap();

            // Calculate the number of frequencies to display based on the console width
            int frequencyCount = Math.Max(1, length);

            // Ensure frequencyCount is within bounds of the array
            frequencyCount = Math.Min(frequencyCount, fftData.Length);

            // Calculate this value once before the loop
            int maxLength = Math.Max(length - 43, 1);

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
                fftValue = (float)Math.Pow(fftValue, logarithmicMultiplier);

                // Apply logarithmic scale to the FFT value
                float average = (float)Math.Log10(1 + fftValue * frequencyMultiplier);

                // Map the average value to an ASCII character
                int index = (int)(average * (unicodeMap.Length - 1));

                // Ensure index is within bounds of the array
                index = Math.Min(index, unicodeMap.Length - 1);

                // Append the mapped character to the frequency string
                frequencyBuilder.Append(unicodeMap[index]);
            }

            string str = frequencyBuilder.ToString();

            if (isPlaying)
            {
                return Themes.sColor(str, Themes.CurrentTheme.Visualizer.PlayingColor);
            }
            else
            {
                return Themes.sColor(str, Themes.CurrentTheme.Visualizer.PausedColor);
            }
        }

        public static string[] GetUnicodeMap()
        {
            if (Themes.CurrentTheme == null || Themes.CurrentTheme.Visualizer == null || Themes.CurrentTheme.Visualizer.UnicodeMap == null)
            {
                return new string[] { " ", "▁", "▂", "▃", "▄", "▅", "▆", "▇", "█" };
            }
            else
            {
                return Themes.CurrentTheme.Visualizer.UnicodeMap;
            }
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

        public static void Write()
        {
            string path = Path.Combine(Utils.JammerPath, "Visualizer.ini");

            // Create the file if it doesn't exist
            if (!File.Exists(path))
            {
                Log.Info("Creating Visualizer.ini file");
                File.WriteAllText(path, FileContent, Encoding.UTF8);
            }
            else
            {
                Log.Info("Visualizer.ini file already exists");
            }
        }

        public static void Read()
        {
            Log.Info("Reading Visualizer.ini file");
            string path = Path.Combine(Utils.JammerPath, "Visualizer.ini");
            var parser = new FileIniDataParser();
            IniData data;

            if (!File.Exists(path))
            {
                Write();
                data = parser.ReadFile(path);
            }
            else
            {
                data = parser.ReadFile(path);
                bool changed = false;
                // Check if the desired entries exist, and add them if they don't
                if (!data["Audio Visualizer"].ContainsKey("RefreshTime"))
                {
                    data["Audio Visualizer"]["RefreshTime"] = "10";
                    changed = true;
                }
                if (!data["Audio Visualizer"].ContainsKey("BufferSize"))
                {
                    data["Audio Visualizer"]["BufferSize"] = "41000";
                    changed = true;
                }
                if (!data["Audio Visualizer"].ContainsKey("DataFlags"))
                {
                    data["Audio Visualizer"]["DataFlags"] = "FFT16384";
                    changed = true;
                }
                if (!data["Audio Visualizer"].ContainsKey("MinFrequency"))
                {
                    data["Audio Visualizer"]["MinFrequency"] = "50";
                    changed = true;
                }
                if (!data["Audio Visualizer"].ContainsKey("MaxFrequency"))
                {
                    data["Audio Visualizer"]["MaxFrequency"] = "17000";
                    changed = true;
                }
                if (!data["Audio Visualizer"].ContainsKey("FrequencyMultiplier"))
                {
                    data["Audio Visualizer"]["FrequencyMultiplier"] = "6000";
                    changed = true;
                }
                if (!data["Audio Visualizer"].ContainsKey("LogarithmicMultiplier"))
                {
                    data["Audio Visualizer"]["LogarithmicMultiplier"] = "2.0";
                    changed = true;
                }
                if (!data["Audio Visualizer"].ContainsKey("PausingEffect"))
                {
                    data["Audio Visualizer"]["PausingEffect"] = "true";
                    changed = true;
                }

                if (changed)
                {
                    parser.WriteFile(path, data);
                }
            }

            refreshTime = int.Parse(data["Audio Visualizer"]["RefreshTime"], CultureInfo.InvariantCulture);
            bufferSize = int.Parse(data["Audio Visualizer"]["BufferSize"], CultureInfo.InvariantCulture);
            dataFlags = data["Audio Visualizer"]["DataFlags"];
            minFrequency = int.Parse(data["Audio Visualizer"]["MinFrequency"], CultureInfo.InvariantCulture);
            maxFrequency = int.Parse(data["Audio Visualizer"]["MaxFrequency"], CultureInfo.InvariantCulture);
            frequencyMultiplier = int.Parse(data["Audio Visualizer"]["FrequencyMultiplier"], CultureInfo.InvariantCulture);
            logarithmicMultiplier = float.Parse(data["Audio Visualizer"]["LogarithmicMultiplier"], CultureInfo.InvariantCulture);
            pausingEffect = bool.Parse(data["Audio Visualizer"]["PausingEffect"]);
        }
    }
}