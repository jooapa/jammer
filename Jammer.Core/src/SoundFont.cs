using System;

namespace Jammer {
    public static class SoundFont {
        public static string[] GetSoundFonts() {
            if (!Path.Exists(Path.Combine(Utils.JammerPath, "soundfonts"))) {
                return new string[0];
            }

            string[] soundfonts = Directory.GetFiles(Path.Combine(Utils.JammerPath, "soundfonts"), "*");
            string[] soundfontsExt = new string[soundfonts.Length];
            string[] jammerSfFonts = Array.Empty<string>();


            for (int i = 0; i < soundfonts.Length; i++) {
                // if the file is a jammer-sf file, add it to the list
                if (Path.GetExtension(soundfonts[i]) == ".jammer-sf") {
                    jammerSfFonts = jammerSfFonts.Append(soundfonts[i]).ToArray();
                }
                // file extension to lower case, and check for .sf2, .sf3, .sfz, sf2pack
                soundfontsExt[i] = Path.GetExtension(soundfonts[i]).ToLowerInvariant();
                if (soundfontsExt[i] != ".sf2" && soundfontsExt[i] != ".sf3" && soundfontsExt[i] != ".sfz" && soundfontsExt[i] != ".sf2pack") {
                    soundfonts[i] = string.Empty;
                }

                soundfonts[i] = Path.GetFileName(soundfonts[i]);
            }

            if (jammerSfFonts.Length > 0) {
                foreach (string jammerSfFont in jammerSfFonts) {
                    string sfPath = File.ReadAllText(jammerSfFont);
                    if (!soundfonts.Contains(sfPath)) {
                        soundfonts = soundfonts.Append(sfPath).ToArray();
                    }
                }
            }

            // remove empty strings
            soundfonts = soundfonts.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            return soundfonts;
        }

        public static string ImportSoundFont(string path) {
            string sfPath = Path.Combine(Utils.JammerPath, "soundfonts", Path.GetFileName(path));

            if (Path.Exists(sfPath)) {
                Message.Data("Soundfont already exists", "Error", true);
                return string.Empty;
            }

            try {
                File.Copy(path, sfPath);
            } catch (Exception e) {
                Message.Data(e.Message, "Error", true);
                return string.Empty;
            }
            return Path.GetFileName(path);
        }

        public static void MakeAbsoluteSfFile(string soundfontPath) {
            // make file called {soundfont}.jammer-sf
            // get the name of the soundfont
            string sfName = Path.GetFileNameWithoutExtension(soundfontPath);

            // create the file
            string sfFilePath = Path.Combine(Utils.JammerPath, "soundfonts", $"{sfName}.jammer-sf");
            
            // if it doesn't exist, create it
            if (!Path.Exists(sfFilePath)) {
                File.WriteAllText(sfFilePath, soundfontPath, System.Text.Encoding.UTF8);
            }
        }
    }
}