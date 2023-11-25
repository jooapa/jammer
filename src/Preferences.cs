using System.Text.Json;
using System.Runtime.InteropServices;

namespace jammer
{
    public class Preferences
    {

        public static int rewindSeconds = 5;
        public static int forwardSeconds = 5;
        public static float volume = 0.5f;
        public static float changeVolumeBy = 0.05f;
        public static float oldVolume = 0.5f;
        public static bool isLoop = false;
        public static bool isMuted = false;
        public static bool isShuffle = false;

        static public void CheckJammerFolderExists()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer";
            if (!Directory.Exists(jammerPath))
            {
                Directory.CreateDirectory(jammerPath);
                Directory.CreateDirectory(jammerPath + "/playlists");
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
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
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
            string jsonString = JsonSerializer.Serialize(settings);
            // delete file if exists
            if (System.IO.File.Exists(jammerPath))
            {
                System.IO.File.Delete(jammerPath);
            }
            System.IO.File.WriteAllText(jammerPath, jsonString);
        }

        static public bool GetIsLoop()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
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
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
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
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
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
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
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
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
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
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
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
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
            if (File.Exists(jammerPath))
            {
                string jsonString = File.ReadAllText(jammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.changeVolumeBy ?? 0.05f;
            }
            else
            {
                return 0.05f;
            }
        }

        static public bool GetIsShuffle()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
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

        static public void OpenJammerFolder()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\jammer";
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
        }
    }
}