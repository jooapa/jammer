import utils, os, url, pygame, preferences, start

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

    utils.currentSong = path
    # load song
    load_music()


def load_music():
    try:
        pygame.mixer.music.load(utils.currentSong)
        pygame.mixer.music.play()
    except pygame.error:
        print("Could not load " + utils.currentSong)
        return

def stop():
    pygame.mixer.music.stop()

def pause():
    pygame.mixer.music.pause()

def resume():
    pygame.mixer.music.unpause()

def reset():
    pygame.mixer.music.unload(utils.currentSong)

def next():
    reset()
    utils.currentSongIndex = (utils.currentSongIndex + 1) % len(utils.songs)
    start.state = start.MainStates.play
    play_song()

def previous():
    reset()
    utils.currentSongIndex = (utils.currentSongIndex - 1) % len(utils.songs)
    if (utils.currentSongIndex < 0):
        utils.currentSongIndex = len(utils.songs) - 1
    start.state = start.MainStates.play
    play_song()

def pause_resume():
    if (pygame.mixer.music.get_busy()):
        pause()
        start.state = start.MainStates.pause
    else:
        resume()
        start.state = start.MainStates.playing