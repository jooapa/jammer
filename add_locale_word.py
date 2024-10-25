"""
This script is used to add a new locale word to the project.

Usage:
    python add_locale_word.py <key> <value> optional: <default_value>
    
Example:
    python add_locale_word.py "CliHelp" "ShowHelpMessage"
"""

import sys
import os

# public static class Settings
key_start_word = "        public static class "

# ??? = CheckValueLocale((string key, string value, string defaultString)
value_start_word = "            public static string "

"""
Add a new locale word to the project.
"""
def add_locale_word(key, value) -> None:
    with open("Jammer.Core/src/Locale.cs", "r") as file:
        lines = file.readlines()
        
    with open("Jammer.Core/src/Locale.cs", "w") as file:
        for line in lines:
            if key_start_word + key in line:
                # append to next line the new value
                file.write(line)
                file.write(value_start_word + 
                           value +
                            ' = CheckValueLocale(' +
                           '"' + 
                           key + 
                           '", "' + 
                           value + '", "Temp Wordings"); // TODO Dont leave me here\n')
            else:
                file.write(line)
                
    with open("locales/en.ini", "r") as file:
        ini_lines = file.readlines()
        
    with open("locales/en.ini", "w") as file:
        for line in ini_lines:
            if "[" + key + "]" in line:
                file.write(line)
                file.write(value + " = Temp Wordings" + "\n")
            else:
                file.write(line)
                
    print("New locale word added successfully!")

if __name__ == "__main__":
    if len(sys.argv) != 3 and len(sys.argv) != 4:
        print("Usage: python add_locale_word.py <key> <value> optional: <default_value>")
        sys.exit(1)
        
    key = sys.argv[1]
    value = sys.argv[2]
    # rest is the default value
    default = sys.argv[3] if len(sys.argv) == 4 else "Temp Wordings"
    
    add_locale_word(key, value)