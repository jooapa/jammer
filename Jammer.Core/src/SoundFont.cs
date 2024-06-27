using System;

namespace Jammer {
    public static class SoundFont {
        public static string[] GetSoundFonts(bool fullpaths = false) {
            if (!Path.Exists(Path.Combine(Utils.JammerPath, "soundfonts"))) {
                return new string[0];
            }

            string[] soundfonts = Directory.GetFiles(Path.Combine(Utils.JammerPath, "soundfonts"), "*.sf2");
            if (fullpaths) {
                return soundfonts;
            }

            for (int i = 0; i < soundfonts.Length; i++) {
                soundfonts[i] = Path.GetFileName(soundfonts[i]);
            }

            return soundfonts;
        }
    }
}