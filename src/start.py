import utils
import play
import preferences
import url
import key
import threading
import pygame
import time
from pynput import keyboard

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
    pygame.init()
    utils.songs = songs
    start_playing()


def start_playing():
    global state
    state = MainStates.playing
    play.play_song()
    loop()


def loop():
    key.start_key_listener()
    global state
    while True:
        # check if space is pressed
        print(state)
        time.sleep(0.1)