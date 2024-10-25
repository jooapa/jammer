"""
This script is used to add a new locale word to the project.

Usage:
    python add_locale_word.py <key> <value>
    
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
def add_locale_word(key, value) -> bool:
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
                
    
if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python add_locale_word.py <key> <value>")
        sys.exit(1)
        
    key = sys.argv[1]
    value = sys.argv[2]
    
    add_locale_word(key, value)