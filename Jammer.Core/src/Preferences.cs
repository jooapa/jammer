using System.Text.Json;
using System.IO;
using System.Runtime.InteropServices;


namespace Jammer
{
    public class Preferences
    {
        public static int rewindSeconds = GetRewindSeconds();
        public static int forwardSeconds = GetForwardSeconds();
        public static float volume = GetVolume();
        public static float changeVolumeBy = GetChangeVolumeBy();
        public static float oldVolume = GetOldVolume();
        public static bool isLoop = GetIsLoop();
        public static bool isMuted = GetIsMuted();
        public static bool isShuffle = GetIsShuffle();
        public static bool isAutoSave = GetIsAutoSave();
        public static string? localeLanguage = GetLocaleLanguage();
        public static string songsPath = GetSongsPath();
        public static bool isMediaButtons = GetIsMediaButtons();
        public static bool isVisualizer = GetIsVisualizer();
        public static string theme = GetTheme();
        public static string currentSf2 = GetCurrentSf2();


        static public void CheckJammerFolderExists()
        {
            string JammerPath = Path.Combine(Utils.JammerPath);

            if (!Directory.Exists(JammerPath))
            {
                Directory.CreateDirectory(JammerPath);
            }
            if (!Directory.Exists(Path.Combine(JammerPath, "playlists"))){
                Directory.CreateDirectory(Path.Combine(JammerPath, "playlists"));
            }
            if (!Directory.Exists(Path.Combine(JammerPath, "soundfonts"))){
                Directory.CreateDirectory(Path.Combine(JammerPath, "soundfonts"));
            }


            // check if settings.json has every data
            SaveSettings();

            // Effects.ini
            Effects.WriteEffects();
            Effects.ReadEffects();
            
            // Visualizer.ini
            Visual.Write();
            Visual.Read();

            if (!Directory.Exists(songsPath))
            {
                Directory.CreateDirectory(songsPath);
            }
        }

        static public void SaveSettings()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            // wirte hello world to a file
            Settings settings = new Settings();
            settings.IsLoop = isLoop;
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
            settings.songsPath = songsPath;
            settings.isVisualizer = isVisualizer;
            settings.theme = theme;
            settings.currentSf2 = currentSf2;
            
            string jsonString = JsonSerializer.Serialize(settings);
            // delete file if exists
            if (File.Exists(JammerPath))
            {
                File.Delete(JammerPath);
            }
            File.WriteAllText(JammerPath, jsonString);
        }

        static public string GetSongsPath()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.songsPath ?? Path.Combine(Utils.JammerPath, "songs");
            }
            else
            {
                return Path.Combine(Utils.JammerPath, "songs");
            }
        }
        static public bool GetIsLoop()
        {
            string JammerPath = Path.Combine(Utils.JammerPath, "settings.json");
            if (File.Exists(JammerPath))
            {
                string jsonString = File.ReadAllText(JammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.IsLoop ?? false;
            }
            else
            {
                return false;
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
                try {
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
	    foreach (FileInfo fi in fis) {
	        size += fi.Length;
	    }
	    // Add subdirectory sizes.
	    System.IO.DirectoryInfo[] dis = d.GetDirectories();
	    foreach (DirectoryInfo di in dis) {
	        size += DirSize(di);
	    }
	    return size;
	}

        public static double ToKilobytes(long bytes) => bytes / 1024d;
        public static double ToMegabytes(long bytes) => ToKilobytes(bytes) / 1024d;
        public static double ToGigabytes(long bytes) => ToMegabytes(bytes) / 1024d;

        public class Settings
        {
            public bool IsLoop { get; set; }
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
            public string? songsPath { get; set; }
            public bool isVisualizer { get; set; }
            public string? theme { get; set; }
            public string? currentSf2 { get; set; }
        }
    }
}

