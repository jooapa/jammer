# utils.py
import os

songs = []
currentSong = ""
currentMusicLength = 0
MusicTimePlayed = 0
preciseTime = 0
currentMusic = None
currentSongIndex = 0
scSongPattern = r"^(https?:\/\/)?(www\.)?(soundcloud\.com|snd\.sc)\/(.*)$"
scPlaylistPattern = r"^https?:\/\/(?:www\.)?soundcloud\.com\/[^\/]+\/sets\/[^\/]+$"
ytSongPattern = r"^(https?:\/\/)?(www\.)?(youtube\.com|youtu\.be)\/(.*)$"
mainLoop = True
jammerPath = os.path.join(os.path.expanduser("~"), "jammer")
isDebug = False
currentPlaylist = ""
