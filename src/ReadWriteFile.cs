using IniParser;
using IniParser.Model;
using System.Reflection;

namespace jammer
{
    static class ReadWriteFile {
        private static readonly FileIniDataParser parser = new FileIniDataParser();
        private static IniData KeyData;
        private static IniData LocaleData;
        
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

        static ReadWriteFile() {
            try {
                KeyData = parser.ReadFile("KeyData.ini");
                KeyDataFound = true;
                KeybindAmount = KeyData["Keybinds"].Count;
            } catch(Exception ex) {
                // Console.WriteLine("Error loading KeyData.ini: " + ex.Message);
                // Console.WriteLine("Press any key to continue");
                // Console.ReadKey();
                KeyData = new IniData();
            }

            try {
                LocaleData = parser.ReadFile("locales\\fi.ini");
                LocaleDataFound = true;
            } catch(Exception ex) {
                // Console.WriteLine("Error loading locales\\en.ini: " + ex.Message);
                // Console.WriteLine("Press any key to continue");
                // Console.ReadKey();
                // Initialize LocaleData with empty IniData if failed to load
                LocaleData = new IniData();
            }
            LocaleAndKeyDataFound = LocaleDataFound && KeyDataFound;

        }
        public static void ReadNewKeybinds(){
            try {
                KeyData = parser.ReadFile("KeyData.ini");
                KeyDataFound = true;
            } catch(Exception ex) {
                // Console.WriteLine("Error loading KeyData.ini: " + ex.Message);
                // Console.WriteLine("Press any key to continue");
                // Console.ReadKey();
                KeyData = new IniData();
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
            KeyData["Keybinds"]["PlayPause"] = final;
            parser.WriteFile("KeyData.ini", KeyData);
        }
        public static string ReadIni_KeyData(string section, string key){
            string key_value = KeyData[section][key];
            return key_value;
        }

        public static string FindMatch_KeyData(
            ConsoleKey key_pressed,
            bool isAlt,
            bool isCtrl,
            bool isShift,
            bool isShiftAlt,
            bool isShiftCtrl
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

                        // Console.WriteLine($"{ctrlModifier} {shiftModifier} {isShiftCtrl}");
                        // Console.WriteLine($"{altModifier} {shiftModifier} {isShiftAlt}");
                        // Console.WriteLine($"{altModifier} {isAlt}");
                        // Console.WriteLine($"{ctrlModifier} {isCtrl}");
                        // Console.WriteLine($"{shiftModifier} {isShift}");
                        // Console.WriteLine($"{isShiftAltModifier} {isShiftCtrlModifier}");
                        // Console.WriteLine(key.KeyName);
                        // Console.WriteLine();
                        // Console.ReadKey();
                        // Look through matches in modifiers
                        if(isShiftCtrlModifier && isShiftCtrl){
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
                        else if(!isShiftAltModifier && !isShiftCtrlModifier && shiftModifier && isShift){
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
                string keyValue = property.GetValue(property.Name).ToString();
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

                    // Console.WriteLine($"{ctrlModifier} {shiftModifier} {isShiftCtrl}");
                    // Console.WriteLine($"{altModifier} {shiftModifier} {isShiftAlt}");
                    // Console.WriteLine($"{altModifier} {isAlt}");
                    // Console.WriteLine($"{ctrlModifier} {isCtrl}");
                    // Console.WriteLine($"{shiftModifier} {isShift}");
                    // Console.WriteLine($"{isShiftAltModifier} {isShiftCtrlModifier}");
                    // Console.WriteLine(property.Name.ToString());
                    // Console.WriteLine();
                    // Console.ReadKey();
                    // Look through matches in modifiers
                    if(isShiftCtrlModifier && isShiftCtrl){
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
            Console.ReadKey();
            return "NULL"; // idk if possible?
        }
        
        /*
        for jindex, letter in enumerate(text):
            if jindex >= index and len(_final_text) != max:
                _final_text += letter
            pass
        # Looppaa indexistä taaksepäin kaikki kirjaimet tekstiin
        for jindex, letter in enumerate(text):
            if jindex < index and len(_final_text) != max:
                _final_text += letter
            pass
        return _final_text
*/
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
        public static void Ini_LoadNewLocale(string country_code){
            LocaleData = parser.ReadFile($"locales\\{country_code}.ini");
        }

        public static string ReadIni_LocaleData(string section, string key){
            string key_value = LocaleData[section][key];
            return key_value;
        }
    }
}