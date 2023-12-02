import os
import json
import platform
import subprocess


def _get_setting(setting_name):
    jammer_path = os.path.join(
        os.path.expanduser("~"), "jammer", "settings.json")
    if os.path.exists(jammer_path):
        with open(jammer_path, "r") as file:
            settings = json.load(file)
            return settings[setting_name]
    else:
        return None
    
def get_is_loop():
    return _get_setting("IsLoop")

def get_volume():
    return _get_setting("Volume")

def get_is_muted():
    return _get_setting("isMuted")

def get_old_volume():
    return _get_setting("OldVolume")

def get_forward_seconds():
    return _get_setting("forwardSeconds")

def get_rewind_seconds():
    return _get_setting("rewindSeconds")

def get_change_volume_by():
    return _get_setting("changeVolumeBy")

def get_is_shuffle():
    return _get_setting("isShuffle")

# Define default values
is_loop = get_is_loop() or False
volume = get_volume() or 50
is_muted = get_is_muted() or False
old_volume = get_old_volume() or 0
forward_seconds = get_forward_seconds() or 10
rewind_seconds = get_rewind_seconds() or 10
change_volume_by = get_change_volume_by() or 5
is_shuffle = get_is_shuffle() or False

def check_jammer_folder_exists():
    jammer_path = os.path.join(os.path.expanduser("~"), "jammer")
    if not os.path.exists(jammer_path):
        os.makedirs(jammer_path)
        os.makedirs(os.path.join(jammer_path, "playlists"))

    settings_path = os.path.join(jammer_path, "settings.json")
    if os.path.exists(settings_path):
        with open(settings_path, "r") as file:
            if file.read() == "":
                set_settings()
                save_settings()
    else:
        set_settings()
        save_settings()

def save_settings():
    jammer_path = os.path.join(
        os.path.expanduser("~"), "jammer", "settings.json")

    settings = {
        "IsLoop": is_loop,
        "Volume": volume,
        "isMuted": is_muted,
        "OldVolume": old_volume,
        "forwardSeconds": forward_seconds,
        "rewindSeconds": rewind_seconds,
        "changeVolumeBy": change_volume_by,
        "isShuffle": is_shuffle
    }

    with open(jammer_path, "w") as file:
        json.dump(settings, file)

def set_settings():
    global is_loop, volume, is_muted, old_volume, forward_seconds, rewind_seconds, change_volume_by, is_shuffle
    is_loop = False 
    volume = 50
    is_muted = False
    old_volume = 0
    forward_seconds = 10
    rewind_seconds = 10
    change_volume_by = 5
    is_shuffle = False
    
def open_jammer_folder():
    jammer_path = os.path.join(os.path.expanduser("~"), "jammer")

    if platform.system() == "Windows":
        subprocess.Popen(["explorer.exe", jammer_path], shell=True)
    elif platform.system() == "Linux":
        subprocess.Popen(["xdg-open", jammer_path])
    elif platform.system() == "Darwin":
        subprocess.Popen(["open", jammer_path])
