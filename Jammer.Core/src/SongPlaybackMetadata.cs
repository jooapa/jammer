using System.Text.Json.Serialization;

namespace Jammer
{
    /// <summary>
    /// Per-song playback overrides stored in metadata.json.
    /// </summary>
    public class SongPlaybackMetadata
    {
        public float Speed { get; set; } = 1.0f;
        public bool Reversed { get; set; } = false;

        /// <summary>
        /// Trim start in "seconds:milliseconds" format, e.g. "10:500".
        /// Null means start from the beginning.
        /// </summary>
        public string? TrimStart { get; set; }

        /// <summary>
        /// Trim end in "seconds:milliseconds" format, e.g. "210:000".
        /// Null means play to the end.
        /// </summary>
        public string? TrimEnd { get; set; }

        public bool UseCustomEffects { get; set; } = false;
        public SongEffectSettings Effects { get; set; } = new();

        [JsonIgnore]
        public double? TrimStartSeconds => TimeStringToSeconds(TrimStart);

        [JsonIgnore]
        public double? TrimEndSeconds => TimeStringToSeconds(TrimEnd);

        /// <summary>
        /// Formats a time span as "seconds:milliseconds" (e.g. "10:500").
        /// </summary>
        public static string? SecondsToTimeString(double? seconds)
        {
            if (seconds == null)
                return null;

            double value = seconds.Value;
            if (value < 0)
                value = 0;

            int wholeSeconds = (int)value;
            int milliseconds = (int)Math.Round((value - wholeSeconds) * 1000);
            if (milliseconds >= 1000)
            {
                milliseconds -= 1000;
                wholeSeconds++;
            }

            return $"{wholeSeconds}:{milliseconds:D3}";
        }

        /// <summary>
        /// Parses "seconds:milliseconds" or a plain number of seconds into total seconds.
        /// </summary>
        public static double? TimeStringToSeconds(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            input = input.Trim();

            if (input.Contains(':'))
            {
                string[] parts = input.Split(':');
                if (parts.Length != 2)
                    return null;

                if (!int.TryParse(parts[0], out int seconds))
                    return null;
                if (!int.TryParse(parts[1], out int milliseconds))
                    return null;

                if (milliseconds < 0 || milliseconds >= 1000)
                    return null;

                return seconds + milliseconds / 1000.0;
            }

            if (double.TryParse(input, out double plainSeconds))
                return plainSeconds;

            return null;
        }
    }

    /// <summary>
    /// Inline per-song effect settings. Mirrors the global <see cref="Effects"/> fields.
    /// </summary>
    public class SongEffectSettings
    {
        public bool IsChorus { get; set; } = false;
        public float ChorusFrequency { get; set; } = 1.1f;
        public float ChorusWetDryMix { get; set; } = 50f;
        public float ChorusDepth { get; set; } = 10f;
        public float ChorusFeedback { get; set; } = 25f;
        public float ChorusDelay { get; set; } = 16f;

        public bool IsCompressor { get; set; } = false;
        public float CompressorGain { get; set; } = 1.0f;
        public float CompressorAttack { get; set; } = 0.1f;
        public float CompressorRelease { get; set; } = 0.1f;
        public float CompressorThreshold { get; set; } = -20.0f;
        public float CompressorRatio { get; set; } = 3.0f;
        public float CompressorPredelay { get; set; } = 4.0f;

        public bool IsDistortion { get; set; } = false;
        public float DistortionGain { get; set; } = 0.5f;
        public float DistortionEdge { get; set; } = 15.0f;
        public float DistortionPostEQCenterFrequency { get; set; } = 100.0f;

        public bool IsEcho { get; set; } = false;
        public float EchoWetDryMix { get; set; } = 50f;
        public float EchoFeedback { get; set; } = 50f;
        public float EchoLeftDelay { get; set; } = 500f;
        public float EchoRightDelay { get; set; } = 500f;
        public bool EchoPanDelay { get; set; } = false;

        public bool IsFlanger { get; set; } = false;
        public float FlangerWetDryMix { get; set; } = 50f;
        public float FlangerDepth { get; set; } = 100f;
        public float FlangerFeedback { get; set; } = -50f;
        public float FlangerFrequency { get; set; } = 0.25f;
        public float FlangerDelay { get; set; } = 2f;

        public bool IsGargle { get; set; } = false;
        public int GargleRate { get; set; } = 0;
        public float GargleWaveShape { get; set; } = 0f;

        public bool IsParamEQ { get; set; } = false;
        public float ParamEQCenter { get; set; } = 8000.0f;
        public float ParamEQBandwidth { get; set; } = 12.0f;
        public float ParamEQGain { get; set; } = 0.0f;

        public bool IsReverb { get; set; } = false;
        public float ReverbInGain { get; set; } = 0.0f;
        public float ReverbReverbMix { get; set; } = 0.0f;
        public float ReverbReverbTime { get; set; } = 1000.0f;
        public float ReverbHighFreqRTRatio { get; set; } = 0.001f;

        /// <summary>
        /// Copies values from the global <see cref="Effects"/> static class.
        /// </summary>
        public static SongEffectSettings FromGlobalEffects()
        {
            return new SongEffectSettings
            {
                IsChorus = Effects.isChorus,
                ChorusFrequency = Effects.chorusFrequency,
                ChorusWetDryMix = Effects.chorusWetDryMix,
                ChorusDepth = Effects.chorusDepth,
                ChorusFeedback = Effects.chorusFeedback,
                ChorusDelay = Effects.chorusDelay,

                IsCompressor = Effects.isCompressor,
                CompressorGain = Effects.compressorGain,
                CompressorAttack = Effects.compressorAttack,
                CompressorRelease = Effects.compressorRelease,
                CompressorThreshold = Effects.compressorThreshold,
                CompressorRatio = Effects.compressorRatio,
                CompressorPredelay = Effects.compressorPredelay,

                IsDistortion = Effects.isDistortion,
                DistortionGain = Effects.distortionGain,
                DistortionEdge = Effects.distortionEdge,
                DistortionPostEQCenterFrequency = Effects.distortionPostEQCenterFrequency,

                IsEcho = Effects.isEcho,
                EchoWetDryMix = Effects.echoWetDryMix,
                EchoFeedback = Effects.echoFeedback,
                EchoLeftDelay = Effects.echoLeftDelay,
                EchoRightDelay = Effects.echoRightDelay,
                EchoPanDelay = Effects.echoPanDelay,

                IsFlanger = Effects.isFlanger,
                FlangerWetDryMix = Effects.flangerWetDryMix,
                FlangerDepth = Effects.flangerDepth,
                FlangerFeedback = Effects.flangerFeedback,
                FlangerFrequency = Effects.flangerFrequency,
                FlangerDelay = Effects.flangerDelay,

                IsGargle = Effects.isGargle,
                GargleRate = Effects.gargleRate,
                GargleWaveShape = Effects.gargleWaveShape,

                IsParamEQ = Effects.isParamEQ,
                ParamEQCenter = Effects.paramEQCenter,
                ParamEQBandwidth = Effects.paramEQBandwidth,
                ParamEQGain = Effects.paramEQGain,

                IsReverb = Effects.isReverb,
                ReverbInGain = Effects.reverbInGain,
                ReverbReverbMix = Effects.reverbReverbMix,
                ReverbReverbTime = Effects.reverbReverbTime,
                ReverbHighFreqRTRatio = Effects.reverbHighFreqRTRatio,
            };
        }
    }
}
