using System;

namespace Jammer {
    public static class SoundFont {
        public static string[] GetSoundFonts() {
            if (!Path.Exists(Path.Combine(Utils.JammerPath, "soundfonts"))) {
                return new string[0];
            }

            string[] soundfonts = Directory.GetFiles(Path.Combine(Utils.JammerPath, "soundfonts"), "*.sf2");
            string[] jammerSfFonts = Directory.GetFiles(Path.Combine(Utils.JammerPath, "soundfonts"), "*.jammer-sf");


            for (int i = 0; i < soundfonts.Length; i++) {
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
                File.WriteAllText(sfFilePath, soundfontPath);
            }
        }
    }
}