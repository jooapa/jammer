using ManagedBass;
using System.Text;
using System.Text.Json;


namespace Jammer {
    public class Visual {
        public static string GetSongVisual()
        {
            const int bufferSize = 41000; // FFT data buffer size
            var fftData = new float[bufferSize]; // FFT data buffer

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
            int frequencyCount = Math.Max(Start.consoleWidth, 1);

            // Ensure frequencyCount is within bounds of the array
            frequencyCount = Math.Min(frequencyCount, fftData.Length);

            // Define the frequency range
            double minFrequency = 50;
            double maxFrequency = 17000;

            // Iterate through the FFT data and map values to ASCII characters
            for (int i = 0; i < frequencyCount; i++)
            {
                // Calculate the frequency for the current index using a logarithmic scale
                double frequency = minFrequency * Math.Pow(maxFrequency / minFrequency, (double)i / frequencyCount);

                // Find the closest index in the FFT data
                int fftIndex = (int)(frequency / 44100 * fftData.Length);

                // Ensure fftIndex is within bounds of the array
                fftIndex = Math.Min(fftIndex, fftData.Length - 1);

                // Get the FFT value at the calculated index
                float fftValue = fftData[fftIndex];

                // Apply logarithmic scale to the FFT value
                float average = (float)Math.Log10(1 + fftValue * 700);

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
    }
}