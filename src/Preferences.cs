using System.Text.Json;
using System.Runtime.InteropServices;

namespace jammer
{
    public class Preferences
    {
        string FileContent = @"
;Do not use characters outside ascii, if you use it needs to use the same encoding as csharp uses by default. ö = oem7, ä = oem3 etc...
;See https://learn.microsoft.com/en-us/dotnet/api/system.consolekey?view=net-8.0 for allowed characters.
;When using numbers, 'd' part is not needed
; Allowed modifiers are ctrl, shift, alt, ctrl + shift and ctrl + alt
;
[Keybinds]
PlayPause = Spacebar
Quit = Q
NextSong = N
PreviousSong = P
PlaySong = Shift + p
Forward5s = RightArrow
Backwards5s = LeftArrow
VolumeUp = UpArrow
VolumeDown = DownArrow
Shuffle = S
SaveAsPlaylist = Shift + Alt + S
SaveCurrentPlaylist = Shift + S
ShufflePlaylist = Alt + S
Loop = L
Mute = M
ShowHidePlaylist = F
ListAllPlaylists = Shift + F
Help = H
Settings = C
ToSongStart = 0
ToSongEnd = 9
PlaylistOptions = F2
ForwardSecondAmount = 1
BackwardSecondAmount = 2
ChangeVolumeAmount = 3
Autosave = 4
CurrentState = F12
CommandHelpScreen = Tab
DeleteCurrentSong = Delete
AddSongToPlaylist = Shift + A
ShowSongsInPlaylists = Shift + D
PlayOtherPlaylist = Shift + O
RedownloadCurrentSong = Shift + B
EditKeybindings = Shift + E
ChangeLanguage = Shift + L
PlayRandomSong = R
";

        public static int rewindSeconds = GetRewindSeconds();
        public static int forwardSeconds = GetForwardSeconds();
        public static float volume = GetVolume();
        public static float changeVolumeBy = GetChangeVolumeBy();
        public static float oldVolume = GetOldVolume();
        public static bool isLoop = GetIsLoop();
        public static bool isMuted = GetIsMuted();
        public static bool isShuffle = GetIsShuffle();
        public static bool isAutoSave = GetIsAutoSave();
        public static string localeLanguage = getLocaleLanguage();

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
            settings.localeLanguage = localeLanguage;
            
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
            
        }
    }
}

