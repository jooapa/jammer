# locales en.ini has all the availble current locale words
# go through all the other locale files and check if none of them are missing, add them

import os


def validate_all_locales() -> None:
    with open("locales/en.ini", "r", encoding="utf-8") as file:
        en_lines = file.readlines()
        
    # get the keys from en.ini
    # ie. ToMainMenu = To Main Menu
    
    en_keys = [line.split("=")[0].strip() for line in en_lines]
    
    # get all the locale files
    locale_files = os.listdir("locales")
    
    for locale_file in locale_files:
        if locale_file == "en.ini":
            continue
        
        with open(f"locales/{locale_file}", "r", encoding="utf-8") as file:
            locale_lines = file.readlines()
        
        locale_keys = [line.split("=")[0].strip() for line in locale_lines]
        
        missing_keys = []
        
        for en_key in en_keys:
            if en_key not in locale_keys:
                missing_keys.append(en_key)
        
        if missing_keys:
            print(f"Missing keys in {locale_file}:")
            for missing_key in missing_keys:
                print(f"    {missing_key}")
            print()
        else:
            print(f"{locale_file} is valid!")
    
if __name__ == "__main__":
    os.chdir(os.path.dirname(os.path.abspath(__file__)))
    os.chdir('..')
    
    validate_all_locales()