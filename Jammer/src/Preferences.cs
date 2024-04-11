using System.Text.Json;
using System.IO;
using System.Runtime.InteropServices;


namespace Jammer
{
    internal class Preferences
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
        public static string? localeLanguage = getLocaleLanguage();
        public static string songsPath = GetSongsPath();

        static public void CheckJammerFolderExists()
        {
            string jammerPath = Path.Combine(Utils.jammerPath);

            if (!Directory.Exists(jammerPath))
            {
                Directory.CreateDirectory(jammerPath);
                Directory.CreateDirectory(Path.Combine(jammerPath, "playlists"));
            }

            // check if settings.json has every data
            string settingsPath = Path.Combine(Utils.jammerPath, "settings.json");
            if (!File.Exists(settingsPath))
            {
                SaveSettings();
            }
            

            if (!Directory.Exists(songsPath))
            {
                Directory.CreateDirectory(songsPath);
            }
        }

        static public void SaveSettings()
        {
            string jammerPath = Path.Combine(Utils.jammerPath, "settings.json");
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
            settings.isAutoSave = isAutoSave;
            settings.localeLanguage = localeLanguage;
            settings.songsPath = songsPath;
            
            string jsonString = JsonSerializer.Serialize(settings);
            // delete file if exists
            if (File.Exists(jammerPath))
            {
                File.Delete(jammerPath);
            }
            File.WriteAllText(jammerPath, jsonString);
        }

        static public string GetSongsPath()
        {
            string jammerPath = Path.Combine(Utils.jammerPath, "settings.json");
            if (File.Exists(jammerPath))
            {
                string jsonString = File.ReadAllText(jammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.songsPath ?? Path.Combine(Utils.jammerPath, "songs");
            }
            else
            {
                return Path.Combine(Utils.jammerPath, "songs");
            }
        }
        static public bool GetIsLoop()
        {
            string jammerPath = Path.Combine(Utils.jammerPath, "settings.json");
            if (File.Exists(jammerPath))
            {
                string jsonString = File.ReadAllText(jammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.IsLoop ?? false;
            }
            else
            {
                return false;
            }
        }

        static public float GetVolume()
        {
            string jammerPath = Path.Combine(Utils.jammerPath, "settings.json");
            if (File.Exists(jammerPath))
            {
                string jsonString = File.ReadAllText(jammerPath);
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
            string jammerPath = Path.Combine(Utils.jammerPath, "settings.json");
            if (File.Exists(jammerPath))
            {
                string jsonString = File.ReadAllText(jammerPath);
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
            string jammerPath = Path.Combine(Utils.jammerPath, "settings.json");
            if (File.Exists(jammerPath))
            {
                string jsonString = File.ReadAllText(jammerPath);
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
            string jammerPath = Path.Combine(Utils.jammerPath, "settings.json");
            if (File.Exists(jammerPath))
            {
                string jsonString = File.ReadAllText(jammerPath);
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
            string jammerPath = Path.Combine(Utils.jammerPath, "settings.json");
            if (File.Exists(jammerPath))
            {
                string jsonString = File.ReadAllText(jammerPath);
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
            string jammerPath = Path.Combine(Utils.jammerPath, "settings.json");
            if (File.Exists(jammerPath))
            {
                try {
                    string jsonString = File.ReadAllText(jammerPath);
                    Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                    return settings?.changeVolumeBy ?? 0.05f;
                }
                catch (Exception e)
                {
                    #if CLI_UI
                    Console.WriteLine(e);
                    #endif
                    #if AVALONIA_UI
                    // TODO Add error message
                    #endif
                    return 0.05f;
                }   
            }
            else
            {
                return 0.05f;
            }
        }

        static public bool GetIsShuffle()
        {
            string jammerPath = Path.Combine(Utils.jammerPath, "settings.json");
            if (File.Exists(jammerPath))
            {
                string jsonString = File.ReadAllText(jammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isShuffle ?? false;
            }
            else
            {
                return false;
            }
        }
        static public string? getLocaleLanguage()
        {
            string jammerPath = Path.Combine(Utils.jammerPath, "settings.json");
            if (File.Exists(jammerPath))
            {
                string jsonString = File.ReadAllText(jammerPath);
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
            string jammerPath = Path.Combine(Utils.jammerPath, "settings.json");
            if (File.Exists(jammerPath))
            {
                string jsonString = File.ReadAllText(jammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isAutoSave ?? false;
            }
            else
            {
                return false;
            }
        }

        static public void OpenJammerFolder()
        {
            string jammerPath = Path.Combine(Utils.jammerPath, "settings.json");
            // start file managert in the given operating system
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.Diagnostics.Process.Start("explorer.exe", jammerPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                System.Diagnostics.Process.Start("xdg-open", jammerPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                System.Diagnostics.Process.Start("open", jammerPath);
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
            public bool isAutoSave { get; set; }
            public string? localeLanguage { get; set; }
            public string? songsPath { get; set; }
        }
    }
}

