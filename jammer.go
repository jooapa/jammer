package main

import (
	"fmt"
	"os"
	"strings"
	"time"

	"github.com/eiannone/keyboard"
	"github.com/faiface/beep"
	"github.com/faiface/beep/effects"
	"github.com/faiface/beep/mp3"
	"github.com/faiface/beep/speaker"
	"github.com/faiface/beep/vorbis"
)

var (
	volumeEffect = &effects.Volume{
		Streamer: beep.Silence(0),
		Base:     2,
		Volume:   1,
		Silent:   false,
	}
	streamer beep.StreamSeekCloser
)

func main() {
	if len(os.Args) < 2 {
		fmt.Println("Usage: jammer <song>")
		os.Exit(1)
	}

	song := os.Args[1]

	fmt.Println("Jamming to", song)

	go playSound(song)
	time.Sleep(500 * time.Millisecond)

	listenForKeyPresses()
}

func playSound(file string) {
	f, err := os.Open(file)
	if err != nil {
		fmt.Println("Error opening sound file:", err)
		return
	}
	defer f.Close()

	var streamer beep.StreamSeekCloser
	var format beep.Format

	if strings.HasSuffix(file, ".mp3") {
		streamer, format, err = mp3.Decode(f)
		if err != nil {
			fmt.Println("Error decoding MP3 sound file:", err)
			return
		}
	} else if strings.HasSuffix(file, ".ogg") {
		streamer, format, err = vorbis.Decode(f)
		if err != nil {
			fmt.Println("Error decoding OGG sound file:", err)
			return
		}
	} else if strings.HasSuffix(file, ".wav") {
		fmt.Println("Unsupported sound file format")
		return
	}

	err = speaker.Init(format.SampleRate, format.SampleRate.N(time.Second/10))
	if err != nil {
		fmt.Println("Error initializing speaker:", err)
		return
	}

	done := make(chan struct{})
	speaker.Play(volumeEffect)
	speaker.Play(beep.Seq(streamer, beep.Callback(func() {
		close(done)
	})))
	<-done
}

func listenForKeyPresses() {
	if err := keyboard.Open(); err != nil {
		panic(err)
	}
	defer func() {
		_ = keyboard.Close()
	}()

	fmt.Println("Press q to quit")
	for {
		char, key, err := keyboard.GetKey()
		if err != nil {
			println(char) // for Go
			panic(err)
		}
		if key == keyboard.Key(0) { // 0 means key "q"
			os.Exit(0)
		}
		if key == keyboard.KeyArrowUp {
			volumeEffect.Volume += 0.1
			fmt.Printf("Volume increased: %.1f\n", volumeEffect.Volume)
			volumeEffect.Streamer = streamer
		}

		if key == keyboard.KeyArrowDown {
			volumeEffect.Volume -= 0.1
			fmt.Printf("Volume decreased: %.1f\n", volumeEffect.Volume)
			volumeEffect.Streamer = streamer
		}

	}
}
