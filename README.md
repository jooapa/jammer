# Jammer — Play songs in cli with youtube and soundcloud support

![banner](https://raw.githubusercontent.com/jooapa/jammer/main/.github/img/jammer-banner.gif)

<br>

*In this example I first open the PvZ playlist from SoundCloud. Then I delete
some songs at the start, by pressing the `delete` key. ~~Then I save the
current playlist to name `plant`~~ (**This is old, now you would press `shift +
alt + S` and type the name of the playlist.**). After exiting i use the inline
cli-playlist tool `jammer playlist play plant` this will play the plant
playlist.*

## Introduction

Tired of opening up a browser or app to play music, and even then you can't
play local files or the songs are in multiple places. Jammer is a simple CLI
music player that supports playing songs from **local files**, **Youtube** and
**Soundcloud**. For **`Windows`**, *`Linux`*.

***Jammer shines it best when using it as a playlist. That's why I created it,
for the playlist feature across different platforms***

- The player doesn't stream the songs, but downloads them to local storage.
- The Jammer folder is located in the user's home directory and contains the
  downloaded songs, playlists and settings.
- Jammer uses [Bass](https://www.un4seen.com/) for playing the songs and [ManagedBass](https://github.com/ManagedBass/ManagedBass) for being able to use it in .NET, [SoundCloudExplode](https://github.com/jerry08/SoundCloudExplode), [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) for downloading the songs and [Spectre.Console](https://github.com/spectreconsole/spectre.console) for the UI.

## Install/Update

### Install

Github latest [Release](https://github.com/jooapa/signal-jammer/releases/latest)
Linux version of jammer requires fuse2. Ubuntu 22.02 or newer install `apt
install libfuse2`.

### Update existing

```bash
jammer update
```

## Usage

```bash
jammer
jammer [song] ... [folder]
jammer https://soundcloud.com/username/track-name
jammer https://soundcloud.com/username/sets/playlist-name
jammer https://youtube.com/watch?v=video-id

jammer start          opens jammer folder
jammer update         checks for updates and installs
jammer playlist       playlist controls
jammer start          opens jammer folder
jammer --help -h      show help
jammer -d             debug mode
jammer --version      show version
```

*when using **soundcloud** or **youtube** **links** dont forget to use **`https://`** at the start.*

```bash
jammer -p, --play <name>
jammer -c, --create <name>
jammer -d, --delete <name>
jammer -a, --add <name> <song> ...
jammer -r, --remove <name> <song> ...
jammer -s, --show <name>
jammer -l, --list
```

### Supported formats

Jammer **supports** the following audio formats: ***.mp3***, ***.ogg***, ***.wav***, ***.mp2***, ***.mp1***, ***.aiff***, ***.m2a***, ***.mpa***, ***.m1a***, ***.mpg***, ***.mpeg***, ***.aif***, ***.mp3pro***, ***.bwf***, ***.mus***, ***.mod***, ***.mo3***, ***.s3m***, ***.xm***, ***.it***, ***.mtm***, ***.umx***, ***.mdz***, ***.s3z***, ***.itz***, ***.xmz***, ***.aac***, ***.adts***, ***.mp4***, ***.m4a***, ***.m4b***.

- **JAMMER** Jammer playlist
- **FOLDER** Folder/Directory (support playing all audio files within a folder)
- **YOUTUBE** Youtube video
- **SOUNDCLOUD** Soundcloud song/playlist

### Default Player Controls

| Key | Action |
|  --------  |  -------  |
| `H` | Show/hide help |
| `C` | Show/hide settings |
| `F` | Show/hide playlist view |
| `Shift + E` | Edit keybindings|
| `Shift + L` | Change language|
| `Space` | Play/pause |
| `Q` | Quit |
| `→` | Forward |
| `←` | Backward |
| `↑` | Volume up |
| `↓` | Volume down |
| `M` | Mute/unmute |
| `L` | Toggle loop |
| `S` | Toggle shuffle |
| `R` | Play in random song |
| `N` | Next song in playlist |
| `P` | Previous song in playlist |
| `Delete` | Delete current song from playlist |
| `F2` | Show playlist options |
| `Tab` | Show CMD help screen|
| `0` | Goto start of the song|
| `9` | Goto end of the song|

### Default Playlist Controls

| Key | Action |
| ------ | ----------- |
| `Shift + A`| Add song to playlist |
| `Shift + D`| Show songs in other playlist |
| `Shift + F`| List all playlists |
| `Shift + O`| Play other playlist |
| `Shift + S`| Save playlist |
| `Shift + Alt + S`| Save as |
| `Alt + S`| Shuffle playlist |
| `Shift + P`| Play song(s) |
| `Shift + B`| Redownload current song |

## Language support

Currently supported languages:

- English

- Finnish

Create new translation by copying already existing .ini file from /locales and translating it.

## Example usage

```bash
jammer "path/to/song.mp3" "path/to/folder" C:\Users\user\jammer\playlists\playlist.jammer
```

```bash
jammer https://soundcloud.com/angry-birds-2009-2014/haunted-hogs https://soundcloud.com/angrysausage/sets/undertale-toby-fox
```

```bash
jammer https://www.youtube.com/watch?v=4zjFDTIROhQ
```

You can also use `-d` flag that will add logs to current folder.

---

#### Random info

You can also stack multiple jammer playlists inside another jammer playlists.

---

## Build / Run yourself

Download the **BASS** and **BASS_AAC** libraries from the [un4seen](http://www.un4seen.com/bass.html) website. Add the **BASS** and **BASS_AAC** libraries to the folder and add it to $LD_LIBRARY_PATH.

```bash
export LD_LIBRARY_PATH=/path/to/your/library:$LD_LIBRARY_PATH
```

```bash

dotnet run -- "path/to/song.mp3" ..
```

### Build

##### Windows

```bash
dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true
```

##### Linux

```bash
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true
```

##### Linux AppImage release

AppImage requires fuse. To install fuse
```
sudo apt install libfuse2
```

To install appimagetool
```
wget https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage
chmod 700 appimagetool-x86_64.AppImage
```

To create AppImage run `build.sh`

##### Build script for NSIS installer

```bash
build.bat
```

you can also use `change_version.py` to change the version of the app.

```bash
python change_version.py [version]
```

![image](https://raw.githubusercontent.com/jooapa/jammer/main/jammer_HQ.png)

### Todo bug fixes

[x] When playing song, and opening a new playlist, the song doesn't change

[x] Pressing `0`, doesn't do anything

[x] Download bar

[x] Fix playlist cmd

[ ] Fix time bar not going to new line when 0:-01


### Incoming Features

[ ] Add more audio formats

[ ] Maybe use curses for UI

[ ] Better f mode

[ ] pg up/down you can scroll the playlist and modify the selected song

[ ] you can see whats downloaded from url


