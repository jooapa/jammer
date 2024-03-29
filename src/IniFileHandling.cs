using IniParser;
using IniParser.Model;
using System.Reflection;
using System.IO;

namespace jammer
{
    static class IniFileHandling {
    private static readonly string FileContent = @"
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

        private static readonly FileIniDataParser parser = new FileIniDataParser();
        private static IniData KeyData;
        private static IniData LocaleData;
        public static int LocaleAmount = 0;
        public static bool EditingKeybind = false;
        public static bool KeyDataFound = false;
        public static bool LocaleDataFound = false;
        public static bool LocaleAndKeyDataFound = false;
        public static int ScrollIndexKeybind = 0;
        public static int ScrollIndexLanguage = 0;
        public static int KeybindAmount = 0;
        public static ConsoleKey previousClick;

        public static bool isAlt = false;
        public static bool isCtrl = false;
        public static bool isShift = false;
        public static bool isShiftAlt = false;
        public static bool isShiftCtrl = false;
        public static bool isShiftCtrlAlt = false;
        

        static IniFileHandling() {
            try {
                KeyData = parser.ReadFile(Path.Combine(Utils.jammerPath, "KeyData.ini"));
                KeyDataFound = true;
                KeybindAmount = KeyData["Keybinds"].Count;
            } catch(Exception) {
                try {
                    KeyData = parser.ReadFile("KeyData.ini");
                    KeyDataFound = true;
                    KeybindAmount = KeyData["Keybinds"].Count;
                } catch(Exception) {        
                    KeyData = new IniData();
                }
            }

            try {
                LocaleData = parser.ReadFile(Path.Combine(Utils.jammerPath, "locales", $"{Preferences.getLocaleLanguage()}.ini"));
                LocaleDataFound = true;
            } catch(Exception) {
                try {
                    LocaleData = parser.ReadFile(Path.Combine(Utils.jammerPath, "locales", $"{Preferences.getLocaleLanguage()}.ini"));
                    LocaleDataFound = true;
                } catch(Exception) {
                    LocaleData = new IniData();
                }
            }
            LocaleAndKeyDataFound = LocaleDataFound && KeyDataFound;

        }
        public static void ReadNewKeybinds(){
            try {
                KeyData = parser.ReadFile(Path.Combine(Utils.jammerPath, "KeyData.ini"));
                KeyDataFound = true;
                KeybindAmount = KeyData["Keybinds"].Count;
            } catch(Exception) {
                try {
                    KeyData = parser.ReadFile("KeyData.ini");
                    KeyDataFound = true;
                    KeybindAmount = KeyData["Keybinds"].Count;
                } catch(Exception) {        
                    KeyData = new IniData();
                }
            }
        }
        public static void WriteIni_KeyData(){
            string final = previousClick.ToString();
            if(isShiftCtrl){
                final = "Shift + Ctrl + " + final;
            }
            else if(isShiftAlt){
                final = "Shift + Alt + " + final;
            }
            else if(isShift){
                final = "Shift + " + final;
            }
            else if(isCtrl){
                final = "Ctrl + " + final;
            }
            else if(isAlt){
                final = "Alt + " + final;
            }
            bool isExisting = false;
            string isExistingKeyName = "";
            // Check if keybind exists
            foreach (var section in KeyData.Sections){
                foreach (var key in section.Keys){
                    string current = key.Value;
                    if(current.ToLower().Replace(" ", "").Equals(final.ToLower().Replace(" ", ""))){
                        isExisting = true;
                        isExistingKeyName = key.KeyName;
                        break;
                    }
                }
            }
            // Save if not
            if(!isExisting){
                int i = 0;
                foreach (var section in KeyData.Sections){
                    foreach (var key in section.Keys){
                        if(i == ScrollIndexKeybind){
                            KeyData["Keybinds"][key.KeyName] = final;
                            break;
                        }
                        i++;
                    }
                }
                try {
                    parser.WriteFile(Path.Combine(Utils.jammerPath, "KeyData.ini"), KeyData);
                } catch(Exception) {
                    try {
                        parser.WriteFile("KeyData.ini", KeyData);
                    } catch(Exception) {        
                        KeyData = new IniData();
                    }
                }
            } else {
                Message.Data($"{Locale.LocaleKeybind.WriteIni_KeyDataError1} {final} {Locale.LocaleKeybind.WriteIni_KeyDataError2}", $"{Locale.LocaleKeybind.WriteIni_KeyDataError3}");
            }

        }
        public static string ReadIni_KeyData(string section, string key){
            string key_value = KeyData[section][key];
            return key_value;
        }

        public static void Create_KeyDataIni(bool hardReset){
            if(!hardReset){
                string filePath = Path.Combine(Utils.jammerPath, "KeyData.ini");
                if (!File.Exists(filePath))
                {
                    
                    File.WriteAllText(filePath, FileContent);
                }
            } else {
                string filePath = Path.Combine(Utils.jammerPath, "KeyData.ini");
                // Check if the file exists, if so, delete it
                if (File.Exists(filePath)){
                    File.Delete(filePath);
                }
                // Create the file and write the content to it
                File.WriteAllText(filePath, FileContent);
            }
            ReadNewKeybinds();
        }
        public static string FindMatch_KeyData(
            ConsoleKey key_pressed,
            bool isAlt,
            bool isCtrl,
            bool isShift,
            bool isShiftAlt,
            bool isShiftCtrl,
            bool isShiftCtrlAlt
            ){
            char separator = '+'; // Inside .ini file
            string key_pressed_string = key_pressed.ToString().ToLower();

            // If number key. d0, d1, d2 etc ->
            if(key_pressed_string[..1] == "d" && key_pressed_string.Length == 2){
                key_pressed_string = key_pressed_string.Substring(1,1);
            }
            string currentKeyPress = "";

            foreach (var section in KeyData.Sections){
                foreach (var key in section.Keys){
                    // Parse key value
                    bool altModifier = false;
                    bool ctrlModifier = false;
                    bool shiftModifier = false;
                    // bool shiftAltModifier = false;
                    // bool shiftCtrlModifier = false;

                    // Base value string
                    string keyValue = key.Value;

                    // Parse spaces
                    keyValue = keyValue.Replace(" ", "");

                    // Split to array if using separator
                    string[] parts = keyValue.Split(separator);

                    // Loop through parts in the value
                    foreach (string part in parts){
                        // Turn to lower case
                        string lowerCasePart = part.ToLower();
                        switch(lowerCasePart){
                            case "shift":
                                shiftModifier = true;
                                break;
                            case "ctrl":
                                ctrlModifier = true;
                                break;
                            case "alt":
                                altModifier = true;
                                break;
                            default:
                                currentKeyPress = lowerCasePart;
                                break;
                        }
                    }
                    if(currentKeyPress.Equals(key_pressed_string)){
                        bool isShiftCtrlModifier = ctrlModifier && shiftModifier;
                        bool isShiftAltModifier = altModifier && shiftModifier;
                        bool isShiftAltCtrlModifier = altModifier && shiftModifier && ctrlModifier;
                        // Look through matches in modifiers
                        if(isShiftAltCtrlModifier && isShiftCtrlAlt){
                            return key.KeyName;
                        }
                        else if(isShiftCtrlModifier && isShiftCtrl){
                            return key.KeyName;
                        }
                        else if(isShiftAltModifier && isShiftAlt){
                            return key.KeyName;
                        }
                        else if(!isShiftAltModifier && altModifier && isAlt){
                            return key.KeyName;
                        }
                        else if(!isShiftCtrlModifier && ctrlModifier && isCtrl){
                            return key.KeyName;
                        }
                        else if(!isShiftAltCtrlModifier && !isShiftAltModifier && !isShiftCtrlModifier && shiftModifier && isShift){
                            return key.KeyName;
                        } else if (!isAlt && !isCtrl && !isShift
                                    && !isShiftAlt && !isShiftCtrl &&
                                    !altModifier && !ctrlModifier&& !shiftModifier){
                            return key.KeyName;
                        }
                        
                    }
                }
            }

            // If no keypresses were found, resort to basics
            Type type = typeof(Keybindings);
            FieldInfo[] properties = type.GetFields();
            
            foreach (FieldInfo property in properties)
            {
                // Parse key value
                bool altModifier = false;
                bool ctrlModifier = false;
                bool shiftModifier = false;
                string? keyValue = property.GetValue(property.Name)?.ToString();
                if(keyValue == null){continue;}
                // Parse spaces
                keyValue = keyValue.Replace(" ", "");

                // Split to array if using separator
                string[] parts = keyValue.Split(separator);

                // Loop through parts in the value
                foreach (string part in parts){
                    // Turn to lower case
                    string lowerCasePart = part.ToLower();
                    switch(lowerCasePart){
                        case "shift":
                            shiftModifier = true;
                            break;
                        case "ctrl":
                            ctrlModifier = true;
                            break;
                        case "alt":
                            altModifier = true;
                            break;
                        default:
                            currentKeyPress = lowerCasePart;
                            break;
                    }
                }
                if(currentKeyPress.Equals(key_pressed_string)){
                    bool isShiftCtrlModifier = ctrlModifier && shiftModifier;
                    bool isShiftAltModifier = altModifier && shiftModifier;
                    bool isShiftAltCtrlModifier = altModifier && shiftModifier && ctrlModifier;

                    // Look through matches in modifiers
                    if(isShiftAltCtrlModifier && isShiftCtrlAlt){
                        return property.Name.ToString();
                    }
                    else if(isShiftCtrlModifier && isShiftCtrl){
                        return property.Name.ToString();
                    }
                    else if(isShiftAltModifier && isShiftAlt){
                        return property.Name.ToString();
                    }
                    else if(!isShiftAltModifier && altModifier && isAlt){
                        return property.Name.ToString();
                    }
                    else if(!isShiftCtrlModifier && ctrlModifier && isCtrl){
                        return property.Name.ToString();
                    }
                    else if(!isShiftAltModifier && !isShiftCtrlModifier && shiftModifier && isShift){
                        return property.Name.ToString();
                    } else if (!isAlt && !isCtrl && !isShift
                                && !isShiftAlt && !isShiftCtrl &&
                                !altModifier && !ctrlModifier&& !shiftModifier){
                        return property.Name.ToString();
                    }
                }
            }
            // Console.ReadKey();
            return "NULL"; // idk if possible?
        }
        
        public static string[] ReadAll_KeyData()
        {
            List<string> results = new();
            int i = 0;
            int maximum = 15;
            foreach (var section in KeyData.Sections){
                foreach (var key in section.Keys){
                    string keyValue = key.Value;
                    if(i >= ScrollIndexKeybind && results.Count != maximum){
                        results.Add(keyValue);
                    }
                    i++;
                }
            }

            i = 0;
            foreach (var section in KeyData.Sections){
                foreach (var key in section.Keys){
                    string keyValue = key.Value;
                    if(i < ScrollIndexKeybind && results.Count != maximum){
                        results.Add(keyValue);
                    }
                    i++;
                }
            }
            return results.ToArray(); // Convert List<string> to string[]
        }




        
        // country code. en, fi, se, de etc...
        public static void Ini_LoadNewLocale(){
            DirectoryInfo? di = null;
            try {
                di = new DirectoryInfo(Path.Combine(Utils.jammerPath, "locales"));
            } catch(Exception) {
                try {
                    di = new DirectoryInfo(Path.Combine(Utils.jammerPath, "locales"));
                } catch(Exception) {
                    return;
                }
            }
            FileInfo[] files = di.GetFiles("*.ini");
            string country_code = "en";
            for(int i = 0; i < files.Length; i++){
                if(i==ScrollIndexLanguage){
                    string filename = Path.GetFileName(files[i].ToString());
                    country_code = filename.Substring(0,2);
                    break;
                }
            }
            try {
                LocaleData = parser.ReadFile(Path.Combine(Utils.jammerPath, "locales", $"{country_code}.ini"));
                Message.Data(Locale.LocaleKeybind.Ini_LoadNewLocaleMessage1, $"{Locale.LocaleKeybind.Ini_LoadNewLocaleMessage2}");
                Preferences.localeLanguage = country_code;
                Preferences.SaveSettings();
            } catch(Exception) {
                try {
                    Message.Data(Locale.LocaleKeybind.Ini_LoadNewLocaleMessage1, $"{Locale.LocaleKeybind.Ini_LoadNewLocaleMessage2}");
                    LocaleData = parser.ReadFile(Path.Combine(Utils.jammerPath, "locales", $"{country_code}.ini"));
                    Preferences.localeLanguage = country_code;
                    Preferences.SaveSettings();
                } catch(Exception) {
                    Message.Data(Locale.LocaleKeybind.Ini_LoadNewLocaleError1, Locale.LocaleKeybind.Ini_LoadNewLocaleError2);

                }
            }

        }

        public static string ReadIni_LocaleData(string section, string key){
            string key_value = LocaleData[section][key];
            return key_value;
        }

        public static string[] ReadAll_Locales(){
        List<string> results = new();
        DirectoryInfo? di = null;
        string path = Path.Combine(Utils.jammerPath, "locales");

        if (!Directory.Exists(path)) {
            // Handle the situation when the directory does not exist
            // For example, you can throw an exception or return an empty array
            Message.Data("Could not find the 'locales' directory in: '" + path + "' Exiting to Main View... ", "Error");
            Start.playerView = "default";
            return results.ToArray();
        }

        di = new DirectoryInfo(path);
        FileInfo[] files = di.GetFiles("*.ini");
        LocaleAmount = files.Length;

        if (LocaleAmount == 0) {
            throw new Exception("No .ini files found in the locales directory.");
        }

            int maximum = 15;
            for(int i = 0; i < files.Length; i++){
                string keyValue = files[i].ToString();
                if(i >= IniFileHandling.ScrollIndexLanguage && results.Count != maximum){
                    results.Add(keyValue);
                }
            }

            for(int i = 0; i < files.Length; i++){
                string keyValue = files[i].ToString();
                if(i < IniFileHandling.ScrollIndexLanguage && results.Count != maximum){
                    results.Add(keyValue);
                }
            }

            return results.ToArray(); // Convert List<string> to string[]
        }
    }
}