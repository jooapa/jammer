import pynput, play

def start_key_listener():
    listener = pynput.keyboard.Listener(on_press=on_press)
    print("Starting key listener")
    listener.start()
    print("Key listener started")
        

def on_press(key):
    try:
        if (key.char == ' '):
            play.pause_resume()
        elif (key.char == 'n'):
            play.next()
        elif (key.char == 'p'):
            play.previous()
    except AttributeError:
        pass