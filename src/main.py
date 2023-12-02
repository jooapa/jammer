import sys
import start, preferences

if __name__ == '__main__':
    preferences.check_jammer_folder_exists()
    start.run(sys.argv[1:])

