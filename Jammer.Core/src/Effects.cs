using IniParser;
using IniParser.Model;
using System.Reflection;
using System.IO;
using ManagedBass;
using ManagedBass.DirectX8;
using System.Globalization;
using Spectre.Console;

namespace Jammer {
    public static class Effects {

        public static bool isChorus = false;
        public static float chorusFrequency = 1.1f;
        public static float chorusWetDryMix = 50;
        public static float chorusDepth = 10;
        public static float chorusFeedback = 25;
        public static float chorusDelay = 16;
        
        public static bool isCompressor = false;
        public static float compressorGain = 1.0f;
        public static float compressorAttack = 0.1f;
        public static float compressorRelease = 0.1f;
        public static float compressorThreshold = -20.0f;
        public static float compressorRatio = 3.0f;
        public static float compressorPredelay = 4.0f;

        public static bool isDistortion = false;
        public static float distortionGain = 0.5f;
        public static float distortionEdge = 15.0f;
        public static float distortionPostEQCenterFrequency = 100.0f;

        public static bool isEcho = false;
        public static float echoWetDryMix = 50;
        public static float echoFeedback = 50;
        public static float echoLeftDelay = 500;
        public static float echoRightDelay = 500;
        public static bool echoPanDelay = false;

        public static bool isFlanger = false;
        public static float flangerWetDryMix = 50;
        public static float flangerDepth = 100;
        public static float flangerFeedback = -50;
        public static float flangerFrequency = 0.25f;
        public static float flangerDelay = 2;

        public static bool isGargle = false;
        public static int gargleRate = 0;
        public static float gargleWaveShape = 0;

        public static bool isParamEQ = false;
        public static float paramEQCenter = 8000.0f;
        public static float paramEQBandwidth = 12.0f;
        public static float paramEQGain = 0.0f;

        public static bool isReverb = false;
        public static float reverbInGain = 0.0f;
        public static float reverbReverbMix = 0.0f;
        public static float reverbReverbTime = 1000.0f;
        public static float reverbHighFreqRTRatio = 0.001f;
        private static readonly string FileContent = @"[Chorus] 
Active = false
; Frequency of the LFO, in the range from 0 to 10.
Frequency = 1.1
; Ratio of wet (processed) signal to dry (unprocessed) signal. Must be in the
; range from 0 through 100 (all wet).
WetDryMix = 0
; Percentage by which the delay time is modulated by the low-frequency
; oscillator, in hundredths of a percentage point. Must be in the range from 0
; through 100.
Depth = 10
; Percentage of output signal to feed back into the effect's input, in the
; range from -99 to 99.
Feedback = 25
; Number of milliseconds the input is delayed before it is played back, in the
; range from 0 to 20.
Delay = 16

[Compressor] 
Active = false
; Output gain of signal in dB after compression, in the range from -60 to 60..
Gain = 1.0
; Time in ms before compression reaches its full value, in the range from 0.01
; to 500.
Attack = 0.1
; Time (speed) in ms at which compression is stopped after input drops below
; DXCompressorParameters. fThreshold, in the range from 50 to 3000.
Release = 0.1
; Point at which compression begins, in dB, in the range from -60 to 0.
Threshold = -20.0
; Compression ratio, in the range from 1 to 100. The default value is 3, which
; means 3:1 compression.
Ratio = 3.0
; Time in ms after DXCompressorParameters. fThreshold is reached before attack
; phase is started, in milliseconds, in the range from 0 to 4.
Predelay = 4.0

[Distortion] 
Active = false
; Amount of signal change after distortion, in the range from -60 through 0.
Gain = 0.5
; Percentage of distortion intensity, in the range in the range from 0 through
; 100.
Edge = 15.0
; Center frequency of harmonic content addition, in the range from 100 through
; 8000.
PostEQCenterFrequency = 100.0

[Echo] 
Active = false
; Ratio of wet (processed) signal to dry (unprocessed) signal. Must be in the
; range from 0 through 100 (all wet).
WetDryMix = 0
; Percentage of output fed back into input, in the range from 0 through 100.
Feedback = 50
; Delay for left channel, in milliseconds, in the range from 1 through 2000.
LeftDelay = 500
; Delay for right channel, in milliseconds, in the range from 1 through 2000.
RightDelay = 500
; Value that specifies whether to swap left and right delays with each
; successive echo. The default value is false, meaning no swap.
PanDelay = false

[Flanger] 
Active = false
; Ratio of wet (processed) signal to dry (unprocessed) signal. Must be in the
; range from 0 through 100 (all wet).
WetDryMix = 0
; Percentage by which the delay time is modulated by the low-frequency
; oscillator (LFO), in hundredths of a percentage point. Must be in the range
; from 0 through 100.
Depth = 100
; Percentage of output signal to feed back into the effect's input, in the
; range from -99 to 99.
Feedback = -50
; Frequency of the LFO, in the range from 0 to 10.
Frequency = 0.25
; Number of milliseconds the input is delayed before it is played back, in the
; range from 0 to 4.
Delay = 2

[Gargle] 
Active = false
; Rate of modulation, in Hertz. Must be in the range from 1 through 1000.
Rate = 0

[ParamEQ] 
Active = false
; Center frequency, in hertz, in the range from 80 to 16000. This value cannot
; exceed one-third of the frequency of the channel.
Center = 8000.0
; Bandwidth, in semitones, in the range from 1 to 36.
Bandwidth = 12.0
; Gain, in the range from -15 to 15.
Gain = 0.0

[Reverb] 
Active = false
; Input gain of signal, in decibels (dB), in the range from -96 through 0.
InGain = 0.0
; Reverb mix, in dB, in the range from -96 through 0.
ReverbMix = 0.0
; Reverb time, in milliseconds
ReverbTime = 1000.0
; In the range from 0.001 through 0.999.
HighFreqRTRatio = 0.001
";


        public static void WriteEffects() {
            string path = Path.Combine(Utils.JammerPath, "Effects.ini");
            
            // Create the file if it doesn't exist
            if (!File.Exists(path)) {
                File.WriteAllText(path, FileContent);
            } 
            // else {
            //     // use the fileContent to fill in any missing values
            //     var parser = new FileIniDataParser();
            //     IniData data = new();

            //     try {
            //         data = parser.ReadFile(path);
            //         IniData defaultData = parser.Parser.Parse(FileContent);

            //         foreach (var section in defaultData.Sections) {
            //             foreach (var key in section.Keys) {
            //                 if (!data[section.SectionName].ContainsKey(key.KeyName) || string.IsNullOrEmpty(data[section.SectionName][key.KeyName])) {
            //                     data[section.SectionName][key.KeyName] = key.Value;
            //                 }
            //             }
            //         }
            //     } catch (IniParser.Exceptions.ParsingException) {
            //         Message.Data("Error parsing the Effects.ini file. Please check the file for errors.", "Error");
            //         Environment.Exit(1);
            //         return;
            //     }

            //     parser.WriteFile(path, data);
            // }
        }

       public static void ReadEffects() {
            try {
                var parser = new FileIniDataParser();
                IniData data = new();
                data = parser.ReadFile(Path.Combine(Utils.JammerPath, "Effects.ini"));

                chorusFrequency = float.Parse(data["Chorus"]["Frequency"], CultureInfo.InvariantCulture);
                chorusWetDryMix = float.Parse(data["Chorus"]["WetDryMix"], CultureInfo.InvariantCulture);
                chorusDepth = float.Parse(data["Chorus"]["Depth"], CultureInfo.InvariantCulture);
                chorusFeedback = float.Parse(data["Chorus"]["Feedback"], CultureInfo.InvariantCulture);
                chorusDelay = float.Parse(data["Chorus"]["Delay"], CultureInfo.InvariantCulture);

                isCompressor = bool.Parse(data["Compressor"]["Active"]);
                compressorGain = float.Parse(data["Compressor"]["Gain"], CultureInfo.InvariantCulture);
                compressorAttack = float.Parse(data["Compressor"]["Attack"], CultureInfo.InvariantCulture);
                compressorRelease = float.Parse(data["Compressor"]["Release"], CultureInfo.InvariantCulture);
                compressorThreshold = float.Parse(data["Compressor"]["Threshold"], CultureInfo.InvariantCulture);
                compressorRatio = float.Parse(data["Compressor"]["Ratio"], CultureInfo.InvariantCulture);
                compressorPredelay = float.Parse(data["Compressor"]["Predelay"], CultureInfo.InvariantCulture);

                isDistortion = bool.Parse(data["Distortion"]["Active"]);
                distortionGain = float.Parse(data["Distortion"]["Gain"], CultureInfo.InvariantCulture);
                distortionEdge = float.Parse(data["Distortion"]["Edge"], CultureInfo.InvariantCulture);
                distortionPostEQCenterFrequency = float.Parse(data["Distortion"]["PostEQCenterFrequency"], CultureInfo.InvariantCulture);

                isEcho = bool.Parse(data["Echo"]["Active"]);
                echoWetDryMix = float.Parse(data["Echo"]["WetDryMix"], CultureInfo.InvariantCulture);
                echoFeedback = float.Parse(data["Echo"]["Feedback"], CultureInfo.InvariantCulture);
                echoLeftDelay = float.Parse(data["Echo"]["LeftDelay"], CultureInfo.InvariantCulture);
                echoRightDelay = float.Parse(data["Echo"]["RightDelay"], CultureInfo.InvariantCulture);
                echoPanDelay = bool.Parse(data["Echo"]["PanDelay"]);

                isFlanger = bool.Parse(data["Flanger"]["Active"]);
                flangerWetDryMix = float.Parse(data["Flanger"]["WetDryMix"], CultureInfo.InvariantCulture);
                flangerDepth = float.Parse(data["Flanger"]["Depth"], CultureInfo.InvariantCulture);
                flangerFeedback = float.Parse(data["Flanger"]["Feedback"], CultureInfo.InvariantCulture);
                flangerFrequency = float.Parse(data["Flanger"]["Frequency"], CultureInfo.InvariantCulture);
                flangerDelay = float.Parse(data["Flanger"]["Delay"], CultureInfo.InvariantCulture);

                isGargle = bool.Parse(data["Gargle"]["Active"]);
                gargleRate = int.Parse(data["Gargle"]["Rate"], CultureInfo.InvariantCulture);

                isParamEQ = bool.Parse(data["ParamEQ"]["Active"]);
                paramEQCenter = float.Parse(data["ParamEQ"]["Center"], CultureInfo.InvariantCulture);
                paramEQBandwidth = float.Parse(data["ParamEQ"]["Bandwidth"], CultureInfo.InvariantCulture);
                paramEQGain = float.Parse(data["ParamEQ"]["Gain"], CultureInfo.InvariantCulture);

                isReverb = bool.Parse(data["Reverb"]["Active"]);
                reverbInGain = float.Parse(data["Reverb"]["InGain"], CultureInfo.InvariantCulture);
                reverbReverbMix = float.Parse(data["Reverb"]["ReverbMix"], CultureInfo.InvariantCulture);
                reverbReverbTime = float.Parse(data["Reverb"]["ReverbTime"], CultureInfo.InvariantCulture);
                reverbHighFreqRTRatio = float.Parse(data["Reverb"]["HighFreqRTRatio"], CultureInfo.InvariantCulture);
            }
            catch (Exception e) {
                AnsiConsole.MarkupLine("[red]Error parsing the Effects.ini file. Please check the file for errors.[/]");
                AnsiConsole.MarkupLine($"[red]{e.Message}[/]");
                Environment.Exit(1);
                return;
            }
        }
    }
}