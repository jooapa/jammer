# <img src="https://raw.githubusercontent.com/jooapa/jammer/main/icons/trans_icon512x512.png" width="35px" align="left"> Jammer — light-weight CLI music player

![banner](.github/img/banner3.png)

## Introduction

Tired of opening up a browser or app to play music, and even then you can't
play local files or songs from different sites?

Jammer is a simple CLI music player that supports playing songs from your **local files**, **Youtube** and **Soundcloud**.

Compatible with *Windows*, *Linux*, *(most likely works on MacOS too, but i have no way to compile to mac)*

- The player **doesn't** stream the songs, but downloads them to local storage.
- The Jammer folder is located in the user's home directory and contains the
  downloaded songs, playlists, settings, keybinds, locales and effects modification.
- Jammer uses [Bass](https://www.un4seen.com/bass.html) for playing the songs and [ManagedBass](https://github.com/ManagedBass/ManagedBass) for being able to use it with C#, [SoundCloudExplode](https://github.com/jerry08/SoundCloudExplode), [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode), [yt-dlp](https://github.com/yt-dlp/yt-dlp) for downloading the songs and [Spectre.Console](https://github.com/spectreconsole/spectre.console) for the UI.

## Table of Contents

- [Install/Update](#installupdate)
- [Usage](#usage)
- [Supported formats](#supported-formats)
- [M3U and M3U8 Support](#m3u-and-m3u8-support)
- [MIDI support](#midi-support)
- [RSS](#rss)
- [Streams](#streams)
- [Soundcloud Client ID](#soundcloud-client-id)
- [Jammer Location](#jammer-location)
- [Environment Variables](#environment-variables)
- [Themes](#themes)
- [Visualizer](#visualizer)
- [Effects](#effects)
- [Language support](#language-support)
- [Developing](#developing)
- [Build / Run yourself](#build--run-yourself)
- [Known Issues](#known-issues)

## Install/Update

### Install

Github latest [Release](https://github.com/jooapa/signal-Jammer/releases/latest)
Linux version of Jammer requires fuse2. Ubuntu 22.02 or newer install `apt install libfuse2 ffmpeg`

### Update existing

```bash
jammer --update
```

## Usage

*when using **Soundcloud** or **Youtube** **links** do not forget to use **`https://`** at the start.*

```bash
# examples of how to use
jammer
jammer [song] ... [folder]
jammer https://soundcloud.com/username/track-name
jammer https://soundcloud.com/username/sets/playlist-name
jammer https://youtube.com/watch?v=video-id
jammer https://youtube.com/playlist?list=playlist-id
jammer https://raw.githubusercontent.com/jooapa/jammer/main/npc_music/616845.mp3
jammer https://raw.githubusercontent.com/jooapa/jammer/main/example/terraria.jammer
jammer https://anchor.fm/s/101ec0f34/podcast/rss
jammer "path/to/song.mp3"

jammer     --start        # opens jammer folder
jammer     --update       # checks for updates and installs
jammer -h, --help         # show help
jammer -D                 # debug mode
jammer -v, --version      # show version

## these commands are for the playlists in the <jammer/playlists> folder
jammer -p, --play       <name>                # play playlist
jammer -c, --create     <name>                # create playlist
jammer -d, --delete     <name>                # delete playlist
jammer -a, --add        <name> <song> ...     # add song to playlist
jammer -r, --remove     <name> <song> ...     # remove song from playlist
jammer -s, --show       <name>                # show songs in playlist
jammer -l, --list                             # list all playlists

jammer -f, --flush                            # deletes all the songs in songs folder
jammer -gp, --get-path                        # get the path to the <jammer/songs> folder
jammer -hm, --home                            # play all songs from the <jammer/songs> folder
jammer -so, --songs                           # open <jammer/songs> folder
```

#### Example of making a playlist in cli

```bash
jammer -c new_playlist
jammer -a new_playlist "https://www.youtube.com/playlist?list=PLnaJlq-zKc0WUXhwhSowwJdpe1fZumJzd"
jammer -p new_playlist
```

### Supported formats

Jammer **supports** the following audio formats: ***.mp3***, ***.ogg***, ***.wav***, ***.mp2***, ***.mp1***, ***.aiff***, ***.aif***, ***.mod***, ***.mo3***, ***.s3m***, ***.xm***, ***.it***, ***.aac***, ***.adts***, ***.mp4***, ***.m4a***, ***.m4b***, ***.mid***, ***.midi***, ***.rmi***, ***.kar***

- **JAMMER** Jammer playlist
- **FOLDER** Folder/Directory (support playing all audio files within a folder)
- **YOUTUBE** Youtube video/playlist
- **SOUNDCLOUD** Soundcloud song/playlist
- **RSS** RSS feed

### MIDI support

Jammer supports playing ***.mid***, ***.midi***, ***.rmi***, ***.kar*** files. To play, you need to have a SoundFont file ***.sf2***, ***.sf3***, ***.sfz***, ***sf2pack***

Here is one sf2 file you can use [ChoriumRevA.SF2](https://www.un4seen.com/download.php?x/ChoriumRevA), *This is BASS's recommended SoundFont file.*

To change the SoundFont file, press `G` (default keybind).

`Link to a soundFont by path`: This will link the SoundFont file by path. **This will not copy the SoundFont file to the <jammer/soundfonts>.**

`Import soundfont by path`: **This will copy the SoundFont file to the `<jammer/soundfonts>`.**

Will show all the SoundFont files in the `<jammer/soundfonts>` folder.

### RSS

Jammer supports playing audio from RSS feeds. You can add an RSS feed by the url. Then you can open the rss, and it will show all the audio files in the feed.

### Streams

Streams are filtered views of your Jammer playlists that allow you to play specific subsets of songs based on tags or properties.

Currently available stream:

#### Favorites Stream (`fav` / `favorites`)

The favorites stream plays only songs that have been marked as favorites using the `IsFavorite` tag.

**Usage:**
```bash
# Play favorites from a specific playlist
jammer -p playlist:fav
jammer -p epic:favorites

# Play favorites from a playlist file
jammer example.jammer:fav
```

**How to favorite songs:**
- Press `Ctrl + F` (default keybind) while playing a song to toggle its favorite status
- Favorited songs will be marked with a ★ symbol in the player interface

This allows you to curate your favorite tracks within any playlist and easily access them later without creating separate playlist files.

### Themes

You can create your own theme by pressing `T` (default keybind)

Select 'Create a New Theme' and write the theme's name. Go to `<jammer/themes>`, you should see `name.json`. It will contain all the information needed for creating a theme.

### Visualizer

You can change the visualizer style in custom Themes.
To change the visualizer settings, you can change the `Visualizer.ini` file in the root folder.

### Effects

- Reverb
- Echo
- Flanger
- Chorus
- Distortion
- Compressor
- Gargle
- Parametric Equalizer

These can be changed in the Effects.ini file in the jammer folder.

### Jammer Location

- **Windows**: `C:\Users\username\jammer`
- **Linux**: `~/jammer`

### Environment Variables

- `JAMMER_CONFIG_PATH`
- `JAMMER_SONGS_PATH`
- `JAMMER_PLAYLISTS_PATH`

### M3U and M3U8 Support

Jammer supports m3u and m3u8 playlists. You can play them but with pretty limited functionality.

m3u files can be played just by opening them with Jammer. But cannot be opened with the `--play`, `-p` command from the `<jammer/playlists>` folder. You can `Save as` (default keybind `Alt + S`) the m3u file, Thus creating a JAMMER playlist to `<jammer/playlists>` folder.

Starting the m3u or m3u8 file with `#EXTM3U` and example of the m3u of all the features that are supported.

```m3u
#EXTM3U

#EXTINF:0,Lady Gaga - Telephone ft. Beyoncé
https://www.youtube.com/watch?v=Zwnvgz3ey78
#EXTINF:0,Epic Music 
/home/user/epic music/epic_music.mp3

/tmp/secret_klinoff.mp3
```

## Language support

Translations may not be up-to-date

Currently supported languages:

- English
- Finnish (*[antonako1](https://github.com/antonako1)*)
- Portuguese (*[Natanaelfelixx](https://github.com/Natanaelfelixx)*)

Create new translation by copying already existing .ini file from /locales and translating it.

## Soundcloud Client ID

soundcloud every now and then changes the client id, which is not cool, so this allows change allows the user to change it :)
You can change the client id by going to the settings and changing the client id.

### how to get the id

- open up the [soundcloud.com](https://soundcloud.com/discover)
- open the inspect element -> Network tab
- start playing some random song
- you start to see some entries in the network tab. you should see some thing like `me?client_id=wDSKS1Bp8WmdlRPkZ7NQXGs67PMXl2Nd`

or you can use the automatic client id fetcher, that is located in the settings menu. *(Might crash on linux)*

## YouTube Download Backends

Jammer supports two different backends for downloading YouTube content:

### YoutubeExplode (Default)
The default backend that works out of the box without any additional setup.

### yt-dlp Backend
An alternative backend that uses [yt-dlp](https://github.com/yt-dlp/yt-dlp) for downloading YouTube content.

**Setup Requirements:**

**Windows:**
- Download `yt-dlp.exe` from the [official releases](https://github.com/yt-dlp/yt-dlp/releases)
- Place the `yt-dlp.exe` file in the same folder as `jammer.exe`

**Linux/macOS:**
- Install yt-dlp and ensure it's available in your system PATH:
```bash
# Using pip
pip install yt-dlp

# Or using your package manager
# Ubuntu/Debian
sudo apt install yt-dlp
```

**Switching Backends:**
You can change between backends by pressing `B` (default keybind) while Jammer is running.

## Star History

<a href="https://star-history.com/#jooapa/jammer&Date">
 <picture>
   <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/svg?repos=jooapa/jammer&type=Date&theme=dark" />
   <source media="(prefers-color-scheme: light)" srcset="https://api.star-history.com/svg?repos=jooapa/jammer&type=Date" />
   <img alt="Star History Chart" src="https://api.star-history.com/svg?repos=jooapa/jammer&type=Date" />
 </picture>
</a>

---

# Developing

## Build / Run yourself

Download the **BASS** and **BASS_AAC** libraries from the [un4seen](http://www.un4seen.com/bass.html) website or the libaries are included in the libs folder.

On **Linux**, you need to add the libraries to the $LD_LIBRARY_PATH.

```bash
export LD_LIBRARY_PATH=/path/to/your/library:$LD_LIBRARY_PATH
```

On **Windows**, you need to add the libraries to the executable folder.

## Install submodules

Jammer uses git submodules. To get the submodules, run this command in the root folder. 

```bash
git submodule update --init --recursive
```

### Run

```bash
dotnet run --project Jammer.CLI -- [args]
```

### Build

#### Windows

```bash
dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true -p:DefineConstants="CLI_UI" --self-contained
```

##### Linux

Add **BASS** and **BASS_AAC** libraries to the executable folder and to $LD_LIBRARY_PATH.

```bash
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true -p:UseForms=false -p:DefineConstants="CLI_UI" --self-contained
```

##### Linux AppImage release

AppImage requires fuse. To install fuse

```bash
sudo apt install libfuse2
```

To install appimagetool

```bash
wget https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage
chmod 700 appimagetool-x86_64.AppImage
```

To create AppImage run `build-appimage.sh`

or if you want to build it from usb

```bash
sh -c "$(wget -O- https://raw.githubusercontent.com/jooapa/jammer/main/usb-appimage.sh)"
```

##### Build script for NSIS installer

```shell
.\Jammer.CLI\buildcli.bat
```

you can use `update.py` to change the version of the app.

```bash
                 |-Major
                 ||--Minor
                 |||---Patch
python update.py 101
```

## Known Issues

Perfect app, no issues.
