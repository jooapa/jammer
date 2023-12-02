import utils, play, preferences, url, keyboard
import threading
import pygame
import time

class MainStates():
            idle = "idle"
            play = "play"
            playing = "playing"
            pause = "pause"
            stop = "stop"
            next = "next"
            previous = "previous"

state = MainStates.play

def run(songs):
    utils.songs = songs
    start_playing()
         
def start_playing():
    global state
    state = MainStates.play
    audio_thread.start()
    play.play_song()

def loop():
    global state
    while True:
        if state == MainStates.idle:
            keyboard.check_keyboard()
            
audio_thread = threading.Thread(target=loop)