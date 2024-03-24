using System.Text.Json;
using System.Runtime.InteropServices;

namespace jammer
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

        static public void CheckJammerFolderExists()
        {
            string jammerPath = Path.Combine(Utils.jammerPath);

            if (!Directory.Exists(jammerPath))
            {
                Directory.CreateDirectory(jammerPath);
                Directory.CreateDirectory(Path.Combine(jammerPath, "playlists"));
            }

            // check if settings.json exists but the file is empty, if so, delete it
            if (File.Exists(jammerPath + "/settings.json"))
            {
                string jsonString = File.ReadAllText(jammerPath + "/settings.json");
                if (jsonString == "")
                {
                    File.Delete(jammerPath + "/settings.json");
                }
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
            
            string jsonString = JsonSerializer.Serialize(settings);
            // delete file if exists
            if (File.Exists(jammerPath))
            {
                File.Delete(jammerPath);
            }
            File.WriteAllText(jammerPath, jsonString);
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
                    Console.WriteLine(e);
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
        }
    }
}
