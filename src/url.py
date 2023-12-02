import re, utils

def is_valid_soundcloud_url(url):
    return re.match(utils.scSongPattern, url) is not None

def is_valid_soundcloud_playlist_url(url):
    return re.match(utils.scPlaylistPattern, url) is not None

def is_valid_youtube_url(url):
    return re.match(utils.ytSongPattern, url) is not None

def is_valid_url(url):
    return is_valid_soundcloud_url(url) or is_valid_youtube_url(url)
