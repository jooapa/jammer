import utils, os, url, pygame, preferences

def play_song():
    if (utils.currentSongIndex < 0 or utils.currentSongIndex >= len(utils.songs)):
        utils.currentSongIndex = 0

    utils.currentSong = utils.songs[utils.currentSongIndex]

    if os.path.isfile(utils.currentSong):
        path = utils.currentSong
    elif (url.is_valid_youtube_url(utils.currentSong)):
        path = utils.jammerPath + utils.currentSong
    elif (url.is_valid_soundcloud_url(utils.currentSong)):
        path = utils.jammerPath + utils.currentSong
    elif (url.is_valid_soundcloud_playlist_url(utils.currentSong)):
        path = utils.jammerPath + utils.currentSong
    else:
        print("Invalid URL")
        return
    
    print("Playing " + utils.currentSong)

    # init audio
    pygame.mixer.init()
    pygame.mixer.music.set_volume(preferences.volume)

    # load song
    load_music(path)


def load_music(path):
    try:
        pygame.mixer.music.load(path)
        pygame.mixer.music.play()
    except pygame.error:
        print("Could not load " + path)
        return
    

