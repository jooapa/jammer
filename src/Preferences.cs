using System.Text.Json;
using System.Runtime.InteropServices;

namespace jammer
{
    public class Preferences
    {
        static public void CheckJammerFolderExists()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer";
            if (!System.IO.Directory.Exists(jammerPath))
            {
                System.IO.Directory.CreateDirectory(jammerPath);
                System.IO.Directory.CreateDirectory(jammerPath + "/playlists");
            }

            // check if settings.json exists but the file is empty, if so, delete it
            if (System.IO.File.Exists(jammerPath + "/settings.json"))
            {
                string jsonString = System.IO.File.ReadAllText(jammerPath + "/settings.json");
                if (jsonString == "")
                {
                    System.IO.File.Delete(jammerPath + "/settings.json");
                }
            }
        }

        static public void SaveSettings()
        {
            // string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
            // // wirte hello world to a file
            // Settings settings = new Settings();
            // settings.IsLoop = Program.isLoop;
            // settings.Volume = Program.volume;
            // settings.isMuted = Program.isMuted;
            // settings.OldVolume = Program.oldVolume;
            // settings.refreshTimes = UI.refreshTimes;
            // settings.forwardSeconds = Program.forwardSeconds;
            // settings.rewindSeconds = Program.rewindSeconds;
            // settings.changeVolumeBy = Program.changeVolumeBy;
            // settings.isShuffle = Program.isShuffle;
            // string jsonString = JsonSerializer.Serialize(settings);
            // // delete file if exists
            // if (System.IO.File.Exists(jammerPath))
            // {
            //     System.IO.File.Delete(jammerPath);
            // }
            // System.IO.File.WriteAllText(jammerPath, jsonString);
        }

        static public bool GetIsLoop()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
            if (System.IO.File.Exists(jammerPath))
            {
                string jsonString = System.IO.File.ReadAllText(jammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.IsLoop ?? 0;
            }
            else
            {
                return false;
            }
        }

        static public float GetVolume()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
            if (System.IO.File.Exists(jammerPath))
            {
                string jsonString = System.IO.File.ReadAllText(jammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.Volume ?? 0;
            }
            else
            {
                return 0.5f;
            }
        }

        static public bool GetIsMuted()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
            if (System.IO.File.Exists(jammerPath))
            {
                string jsonString = System.IO.File.ReadAllText(jammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isMuted ?? 0;
            }
            else
            {
                return false;
            }
        }

        static public float GetOldVolume()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
            if (System.IO.File.Exists(jammerPath))
            {
                string jsonString = System.IO.File.ReadAllText(jammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.OldVolume ?? 0;
            }
            else
            {
                return 0.5f;
            }
        }

        static public int GetRefreshTimes()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
            if (System.IO.File.Exists(jammerPath))
            {
                string jsonString = System.IO.File.ReadAllText(jammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.refreshTimes ?? 0;
            }
            else
            {
                return 40;
            }
        }

        static public int GetForwardSeconds()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
            if (System.IO.File.Exists(jammerPath))
            {
                string jsonString = System.IO.File.ReadAllText(jammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.forwardSeconds ?? 0;
            }
            else
            {
                return 5;
            }
        }

        static public int GetRewindSeconds()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
            if (System.IO.File.Exists(jammerPath))
            {
                string jsonString = System.IO.File.ReadAllText(jammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.rewindSeconds ?? 0;
            }
            else
            {
                return 5;
            }
        }

        static public float GetChangeVolumeBy()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
            if (System.IO.File.Exists(jammerPath))
            {
                string jsonString = System.IO.File.ReadAllText(jammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.changeVolumeBy ?? 0;
            }
            else
            {
                return 0.05f;
            }
        }

        static public bool GetIsShuffle()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
            if (System.IO.File.Exists(jammerPath))
            {
                string jsonString = System.IO.File.ReadAllText(jammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings?.isShuffle ?? 0;
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
            public int refreshTimes { get; set; }
            public int forwardSeconds { get; set; }
            public int rewindSeconds { get; set; }
            public float changeVolumeBy { get; set; }
            public bool isShuffle { get; set; }
        }
    }
}