using System.Text.Json;
using System.IO;
using System.Runtime.InteropServices;


namespace Jammer
{
    public enum LoopType
    {
        None,
        Once,
        Always
    }
    public class Preferences
    {
        public static int rewindSeconds = GetRewindSeconds();
        public static int forwardSeconds = GetForwardSeconds();
        public static float volume = GetVolume();
        public static float changeVolumeBy = GetChangeVolumeBy();
        public static float oldVolume = GetOldVolume();
        public static LoopType loopType = GetLoopType();
        public static bool isMuted = GetIsMuted();
        public static bool isShuffle = GetIsShuffle();
        public static bool isAutoSave = GetIsAutoSave();
        public static string? localeLanguage = GetLocaleLanguage();
        public static string songsPath = GetSongsPath();
        public static bool isMediaButtons = GetIsMediaButtons();
        public static bool isVisualizer = GetIsVisualizer();
        public static string theme = GetTheme();
        public static string currentSf2 = GetCurrentSf2();
        public static string clientID = GetClientId();
        public static bool isModifierKeyHelper = GetModifierKeyHelper();
        public static bool isSkipErrors = GetIsSkipErrors();
        public static bool showPlaylistPosition = GetShowPlaylistPosition();

        private static bool GetModifierKeyHelper()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.modifierKeyHelper ?? false;
            }
            else
            {
                return false;
            }
        }

        private static bool GetIsSkipErrors()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isIgnoreErrors ?? false;
            }
            else
            {
                return false;
            }
        }

        private static bool GetShowPlaylistPosition()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.showPlaylistPosition ?? false;
            }
            else
            {
                return false;
            }
        }

        static public void CheckJammerFolderExists()
        {
            string JammerPath = Path.Combine(Utils.JammerPath);

            if (!Directory.Exists(JammerPath))
            {
                Log.Error("Jammer folder does not exist, creating one...");
                Directory.CreateDirectory(JammerPath);
            }
            if (!Directory.Exists(GetPlaylistsPath()))
            {
                Log.Error("Playlists folder does not exist, creating one...");
                Directory.CreateDirectory(GetPlaylistsPath());
            }
            if (!Directory.Exists(Path.Combine(JammerPath, "soundfonts")))
            {
                Log.Error("Soundfonts folder does not exist, creating one...");
                Directory.CreateDirectory(Path.Combine(JammerPath, "soundfonts"));
            }
            if (!Directory.Exists(Path.Combine(JammerPath, "locales")))
            {
                Log.Error("Soundfonts folder does not exist, creating one...");
                Directory.CreateDirectory(Path.Combine(JammerPath, "locales"));
            }


            // check if settings.json has every data
            SaveSettings();

            Log.Info("Loading Effects.ini");
            // Effects.ini
            Effects.WriteEffects();
            Effects.ReadEffects();

            Log.Info("Loading Visualizer.ini");
            // Visualizer.ini
            Visual.Write();
            Visual.Read();

            if (!Directory.Exists(songsPath))
            {
                Log.Error("Songs folder does not exist, creating one...");
                Directory.CreateDirectory(songsPath);
            }

            // load if not folder empty
            if (Directory.GetFiles(Path.Combine(JammerPath, "locales")).Length > 0)
            {
                // load current locale
                IniFileHandling.SetLocaleData();
            }


        }

        static public void SaveSettings()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            Settings settings = new Settings();
            settings.LoopType = loopType;
            settings.Volume = volume;
            settings.isMuted = isMuted;
            settings.OldVolume = oldVolume;
            settings.forwardSeconds = forwardSeconds;
            settings.rewindSeconds = rewindSeconds;
            settings.changeVolumeBy = changeVolumeBy;
            settings.isShuffle = isShuffle;
            settings.isMediaButtons = isMediaButtons;
            settings.isAutoSave = isAutoSave;
            settings.localeLanguage = localeLanguage;
            // settings.songsPath = songsPath;
            settings.isVisualizer = isVisualizer;
            settings.theme = theme;
            settings.currentSf2 = currentSf2;
            settings.clientID = clientID;
            settings.modifierKeyHelper = isModifierKeyHelper;
            settings.isIgnoreErrors = isSkipErrors;
            settings.showPlaylistPosition = showPlaylistPosition;

            string jsonString = JsonSerializer.Serialize(settings);
            // delete file if exists
            if (File.Exists(JammerPath))
            {
                File.Delete(JammerPath);
            }
            File.WriteAllText(JammerPath, jsonString, System.Text.Encoding.UTF8);
        }

        static public string GetSongsPath()
        {

            // check if environment variable JAMMER_SONGS_PATH exists
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAMMER_SONGS_PATH")))
            {
                return Environment.GetEnvironmentVariable("JAMMER_SONGS_PATH") ?? Path.Combine(Utils.JammerPath, "songs");
            }

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")))
            {
                return Path.Combine(Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? string.Empty, "jammer", "songs");
            }

            // check if settings.json exists
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            var value = "";
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                value = settings?.songsPath;
            }

            // Show a message box and ask the user if they want to change the path to the new Environment variable version
            if (!string.IsNullOrEmpty(value) && value != Path.Combine(Utils.JammerPath, "songs"))
            {
                var val =
                    Message.Input("Jammer songs path has moved from the settings.json to the Environment variable JAMMER_SONGS_PATH. Now would be a good time to set the variable. Exit by pressing 'n', or if you want to use the default location press 'y'.", "Current songs Path " + Path.Combine(value, "songs"), true);
                val = val.ToLower();
                if (val == "n")
                {
                    Environment.Exit(0);
                }
            }

            // return the normal path
            return Path.Combine(Utils.JammerPath, "songs");
        }

        static public string GetPlaylistsPath()
        {
            // check if environment variable JAMMER_PLAYLISTS_PATH exists
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAMMER_PLAYLISTS_PATH")))
            {
                return Environment.GetEnvironmentVariable("JAMMER_PLAYLISTS_PATH") ?? Path.Combine(Utils.JammerPath, "playlists");
            }

            // return the normal path
            return Path.Combine(Utils.JammerPath, "playlists");
        }

        static public LoopType GetLoopType()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.LoopType ?? LoopType.None;
            }
            else
            {
                return LoopType.None;
            }
        }

        static public string GetTheme()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.theme ?? "light";
            }
            else
            {
                return "Jammer Default";
            }
        }

        static public bool GetIsVisualizer()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isVisualizer ?? true;
            }
            else
            {
                return true;
            }
        }

        static public float GetVolume()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.Volume ?? 0.5f;
            }
            else
            {
                return 0.5f;
            }
        }

        static public bool GetIsMuted()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isMuted ?? false;
            }
            else
            {
                return false;
            }
        }

        static public float GetOldVolume()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.OldVolume ?? 0.5f;
            }
            else
            {
                return 0.5f;
            }
        }

        static public int GetForwardSeconds()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.forwardSeconds ?? 5;
            }
            else
            {
                return 5;
            }
        }

        static public int GetRewindSeconds()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.rewindSeconds ?? 5;
            }
            else
            {
                return 5;
            }
        }

        static public float GetChangeVolumeBy()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                try
                {
                    string jsonString = File.ReadAllText(JammerPath);
                    Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                    return settings?.changeVolumeBy ?? 0.05f;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    return 0.05f;
                }
            }
            else
            {
                return 0.05f;
            }
        }

        static public bool GetIsMediaButtons()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isMediaButtons ?? true;
            }
            else
            {
                return true;
            }
        }

        static public bool GetIsShuffle()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isShuffle ?? false;
            }
            else
            {
                return false;
            }
        }


        static public string? GetLocaleLanguage()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.localeLanguage;
            }
            else
            {
                return "en";
            }
        }

        static public bool GetIsAutoSave()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isAutoSave ?? false;
            }
            else
            {
                return false;
            }
        }

        static public string GetCurrentSf2()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.currentSf2 ?? "";
            }
            else
            {
                return "";
            }
        }

        static public string GetClientId()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.clientID ?? "";
            }
            else
            {
                return "";
            }
        }

        static public void OpenJammerFolder()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            // start file managert in the given operating system
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.Diagnostics.Process.Start("explorer.exe", JammerPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                System.Diagnostics.Process.Start("xdg-open", JammerPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                System.Diagnostics.Process.Start("open", JammerPath);
            }
        }

        public static long DirSize(System.IO.DirectoryInfo d)
        {

            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            System.IO.DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        public static double ToKilobytes(long bytes) => bytes / 1024d;
        public static double ToMegabytes(long bytes) => ToKilobytes(bytes) / 1024d;
        public static double ToGigabytes(long bytes) => ToMegabytes(bytes) / 1024d;

        public class Settings
        {
            public LoopType LoopType { get; set; }
            public float Volume { get; set; }
            public float OldVolume { get; set; }
            public bool isMuted { get; set; }
            public int forwardSeconds { get; set; }
            public int rewindSeconds { get; set; }
            public float changeVolumeBy { get; set; }
            public bool isShuffle { get; set; }
            public bool? isMediaButtons { get; set; }
            public bool isAutoSave { get; set; }
            public string? localeLanguage { get; set; }
            // old songs path, used in the migration process, if its set already
            public string? songsPath { get; set; }
            public bool isVisualizer { get; set; }
            public string? theme { get; set; }
            public string? currentSf2 { get; set; }
            public string? clientID { get; set; }
            public bool? modifierKeyHelper { get; set; }
            public bool? isIgnoreErrors { get; set; }
            public bool? showPlaylistPosition { get; set; }
        }
    }
}

