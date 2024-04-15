using IniParser;
using IniParser.Model;
using System.Reflection;
using System.IO;
using ManagedBass;
using ManagedBass.DirectX8;

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
        public static float gargleRate = 0;
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
        private static readonly string FileContent = @"
[Chorus]
Active = false
Frequency = 1.1
WetDryMix = 50
Depth = 10
Feedback = 25
Delay = 16

[Compressor]
Active = false
Gain = 1.0
Attack = 0.1
Release = 0.1
Threshold = -20.0
Ratio = 3.0
Predelay = 4.0

[Distortion]
Active = false
Gain = 0.5
Edge = 15.0
PostEQCenterFrequency = 100.0

[Echo]
Active = false
WetDryMix = 50
Feedback = 50
LeftDelay = 500
RightDelay = 500
PanDelay = false

[Flanger]
Active = false
WetDryMix = 50
Depth = 100
Feedback = -50
Frequency = 0.25
Delay = 2

[Gargle]
Active = false
Rate = 0
WaveShape = 0

[ParamEQ]
Active = false
Center = 8000.0
Bandwidth = 12.0
Gain = 0.0

[Reverb]
Active = false
InGain = 0.0
ReverbMix = 0.0
ReverbTime = 1000.0
HighFreqRTRatio = 0.001
";


        public static void WriteEffects() {
            string path = Path.Combine(Utils.JammerPath, "Effects.ini");
            
            // Create the file if it doesn't exist
            if (!File.Exists(path)) {
                File.WriteAllText(path, FileContent);
            } else {
                // use the fileContent to fill in any missing values
                var parser = new FileIniDataParser();
                IniData data = new();

                try {
                    data = parser.ReadFile(path);
                    IniData defaultData = parser.Parser.Parse(FileContent);

                    foreach (var section in defaultData.Sections) {
                        foreach (var key in section.Keys) {
                            if (!data[section.SectionName].ContainsKey(key.KeyName) || string.IsNullOrEmpty(data[section.SectionName][key.KeyName])) {
                                data[section.SectionName][key.KeyName] = key.Value;
                            }
                        }
                    }
                } catch (IniParser.Exceptions.ParsingException) {
                    Message.Data("Error parsing the Effects.ini file. Please check the file for errors.", "Error");
                    Environment.Exit(1);
                    return;
                }

                parser.WriteFile(path, data);
            }
        }

       public static void ReadEffects() {

            // DXChorusEffect chorus = new();
            // chorus.Frequency = 1.1f;
            // chorus.WetDryMix = 50;
            // chorus.Depth = 10;
            // chorus.Feedback = 25;
            // chorus.Delay = 16;

            // DXCompressorEffect compressor = new();
            // compressor.Gain = 1.0f;
            // compressor.Attack = 0.1f;
            // compressor.Release = 0.1f;
            // compressor.Threshold = -20.0f;
            // compressor.Ratio = 3.0f;
            // compressor.Predelay = 4.0f;

            // DXDistortionEffect distortion = new();
            // distortion.Gain = 0.5f;
            // distortion.Edge = 15.0f;
            // distortion.PostEQCenterFrequency = 100.0f;

            // DXEchoEffect echo = new();
            // echo.WetDryMix = 50;
            // echo.Feedback = 50;
            // echo.LeftDelay = 500;
            // echo.RightDelay = 500;
            // echo.PanDelay = false;

            // DXFlangerEffect flanger = new();
            // flanger.WetDryMix = 50;
            // flanger.Depth = 100;
            // flanger.Feedback = -50;
            // flanger.Frequency = 0.25f;
            // flanger.Delay = 2;

            // DXGargleEffect gargle = new();
            // gargle.Rate = 0;
            // gargle.WaveShape = 0;

            // DXParamEQEffect paramEQ = new();
            // paramEQ.Center = 8000.0f;
            // paramEQ.Bandwidth = 12.0f;
            // paramEQ.Gain = 0.0f;

            // DXReverbEffect reverb = new();
            // reverb.InGain = 0.0f;
            // reverb.ReverbMix = 0.0f;
            // reverb.ReverbTime = 1000.0f;
            // reverb.HighFreqRTRatio = 0.001f;


            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(Utils.JammerPath + "Effects.ini");

            isChorus = bool.Parse(data["Chorus"]["Active"]);
            chorusFrequency = float.Parse(data["Chorus"]["Frequency"]);
            chorusWetDryMix = float.Parse(data["Chorus"]["WetDryMix"]);
            chorusDepth = float.Parse(data["Chorus"]["Depth"]);
            chorusFeedback = float.Parse(data["Chorus"]["Feedback"]);
            chorusDelay = float.Parse(data["Chorus"]["Delay"]);

            isCompressor = bool.Parse(data["Compressor"]["Active"]);
            compressorGain = float.Parse(data["Compressor"]["Gain"]);
            compressorAttack = float.Parse(data["Compressor"]["Attack"]);
            compressorRelease = float.Parse(data["Compressor"]["Release"]);
            compressorThreshold = float.Parse(data["Compressor"]["Threshold"]);
            compressorRatio = float.Parse(data["Compressor"]["Ratio"]);
            compressorPredelay = float.Parse(data["Compressor"]["Predelay"]);

            isDistortion = bool.Parse(data["Distortion"]["Active"]);
            distortionGain = float.Parse(data["Distortion"]["Gain"]);
            distortionEdge = float.Parse(data["Distortion"]["Edge"]);
            distortionPostEQCenterFrequency = float.Parse(data["Distortion"]["PostEQCenterFrequency"]);

            isEcho = bool.Parse(data["Echo"]["Active"]);
            echoWetDryMix = float.Parse(data["Echo"]["WetDryMix"]);
            echoFeedback = float.Parse(data["Echo"]["Feedback"]);
            echoLeftDelay = float.Parse(data["Echo"]["LeftDelay"]);
            echoRightDelay = float.Parse(data["Echo"]["RightDelay"]);
            echoPanDelay = bool.Parse(data["Echo"]["PanDelay"]);

            isFlanger = bool.Parse(data["Flanger"]["Active"]);
            flangerWetDryMix = float.Parse(data["Flanger"]["WetDryMix"]);
            flangerDepth = float.Parse(data["Flanger"]["Depth"]);
            flangerFeedback = float.Parse(data["Flanger"]["Feedback"]);
            flangerFrequency = float.Parse(data["Flanger"]["Frequency"]);
            flangerDelay = float.Parse(data["Flanger"]["Delay"]);

            isGargle = bool.Parse(data["Gargle"]["Active"]);
            gargleRate = float.Parse(data["Gargle"]["Rate"]);
            gargleWaveShape = float.Parse(data["Gargle"]["WaveShape"]);

            isParamEQ = bool.Parse(data["ParamEQ"]["Active"]);
            paramEQCenter = float.Parse(data["ParamEQ"]["Center"]);
            paramEQBandwidth = float.Parse(data["ParamEQ"]["Bandwidth"]);
            paramEQGain = float.Parse(data["ParamEQ"]["Gain"]);

            isReverb = bool.Parse(data["Reverb"]["Active"]);
            reverbInGain = float.Parse(data["Reverb"]["InGain"]);
            reverbReverbMix = float.Parse(data["Reverb"]["ReverbMix"]);
            reverbReverbTime = float.Parse(data["Reverb"]["ReverbTime"]);
            reverbHighFreqRTRatio = float.Parse(data["Reverb"]["HighFreqRTRatio"]);
        }
        
    }
}