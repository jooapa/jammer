using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices;


namespace jammer
{
    public class JammerFolder
    {
        static public void CheckJammerFolderExists()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer";
            if (!System.IO.Directory.Exists(jammerPath))
            {
                System.IO.Directory.CreateDirectory(jammerPath);
            }
        }

        static public void SaveSettings(bool isLoop, float volume, bool isMuted, float oldVolume)
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer/settings.json";
            // wirte hello world to a file
            Settings settings = new Settings();
            settings.IsLoop = isLoop;
            settings.Volume = volume;
            settings.isMuted = isMuted;
            settings.OldVolume = oldVolume;
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
            if (System.IO.File.Exists(jammerPath))
            {
                string jsonString = System.IO.File.ReadAllText(jammerPath);
                Settings? settings = JsonSerializer.Deserialize<Settings>(jsonString);
                return settings.IsLoop;
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
                return settings.Volume;
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
                return settings.isMuted;
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
                return settings.OldVolume;
            }
            else
            {
                return 0.5f;
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
        }
    }
}
