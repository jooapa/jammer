"""
This script is used to add a new locale word to the project.
    
Example:
    python.bat .\add_locale_word.py -s CliHelp -k ShowHelpMessage -v "k akkalol asdasd asd asd asd" 
"""

import sys
import os
import argparse

# public static class Settings
key_start_word = "        public static class "

# ??? = CheckValueLocale((string key, string value, string defaultString)
value_start_word = "            public static string "

"""
Add a new locale word to the project.
"""


def add_locale_word(key, value, default) -> None:
    with open("Jammer.Core/src/Locale.cs", "r", encoding="utf-8") as file:
        lines = file.readlines()

    with open("Jammer.Core/src/Locale.cs", "w", encoding="utf-8") as file:
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
                           value + '", "' + default + '");\n')
            else:
                file.write(line)

    locales_dir = "locales"
    for filename in os.listdir(locales_dir):
        if filename.endswith(".ini"):
            file_path = os.path.join(locales_dir, filename)
            with open(file_path, "r", encoding="utf-8") as file:
                ini_lines = file.readlines()

            with open(file_path, "w", encoding="utf-8") as file:
                for line in ini_lines:
                    if "[" + key + "]" in line:
                        file.write(line)
                        file.write(value + " = " + default + "\n")
                    else:
                        file.write(line)
                # Add the new key-value pair at the end of the file if not found
                if not any("[" + key + "]" in line for line in ini_lines):
                    file.write(f"\n[{key}]\n{value} = " + default + "\n")

    print("New locale word added successfully!")


if __name__ == "__main__":
    if len(sys.argv) == 1:
        print("Please run the script with the following arguments:")
        print("python add_locale_word.py -s <section> -k <key> -v <value>")
        sys.exit(1)

    parser = argparse.ArgumentParser(
        description="This script is used to add a new locale word to the project.")

    parser.add_argument(
        "--section", "-s",
        help="The key for the new locale word")
    parser.add_argument(
        "--key", "-k",
        help="The value for the new locale word")
    parser.add_argument(
        "--value", "-v", nargs='?', default="Temp Wordings",
        help="The default value for the new locale word (optional)")

    args = parser.parse_args()
    
    os.chdir(os.path.dirname(os.path.abspath(__file__)))
    os.chdir('..')

    add_locale_word(args.section, args.key, args.value)
