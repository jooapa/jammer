# Jammer — Play songs in cli with Youtube and Soundcloud support

![banner](https://raw.githubusercontent.com/jooapa/jammer/main/.github/img/jammer-banner.gif)

## Introduction

Tired of opening up a browser or app to play music, and even then you can't
play local files or songs from different sites?

Jammer is a simple CLI music player that supports playing songs from your**local files**, **Youtube** and **Soundcloud**. 

Compatible with **`Windows`**, *`Linux`*.

***Jammer shines its best when using it as a playlist. That's why I created it,
for the playlist feature across different platforms***

- The player doesn't stream the songs, but downloads them to local storage.
- The Jammer folder is located in the user's home directory and contains the
  downloaded songs, playlists, settings, keybinds and locales.
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
jammer https://youtube.com/playlist?list=playlist-id

jammer --start        opens jammer folder
jammer --update       checks for updates and installs
jammer --help -h      show help
jammer -d             debug mode
jammer --version      show version
```

*when using **Soundcloud** or **Youtube** **links** dont forget to use **`https://`** at the start.*

```bash
jammer -p, --play <name>                  play playlist
jammer -c, --create <name>                create playlist
jammer -d, --delete <name>                delete playlist
jammer -a, --add <name> <song> ...        add song to playlist
jammer -r, --remove <name> <song> ...     remove song from playlist
jammer -s, --show <name> 
jammer -l, --list                         list all playlists
jammer -f, --flush                        flushes all the songs in songs folder
jammer -sp, --set-path <path>, <default>  set path for songs folder
```

### Example usage

```bash
jammer "path/to/song.mp3" "path/to/folder" C:\Users\user\jammer\playlists\playlist.jammer
```

```bash
jammer https://soundcloud.com/angry-birds-2009-2014/haunted-hogs https://soundcloud.com/angrysausage/sets/undertale-toby-fox
```

```bash
jammer https://www.youtube.com/watch?v=4zjFDTIROhQ
```

#### Example of making a playlist in cli

```bash
jammer -c gd
jammer -a gd https://www.youtube.com/playlist?list=PLnaJlq-zKc0WUXhwhSowwJdpe1fZumJzd
jammer -p gd
```

*you can do same opening the `jammer` and pressing saving as by default keybinds `shift + alt + s` and after that `shift + a` to add the playlist by input*

You can also use `-d` flag that will add logs to current folder.

---

### Supported formats

Jammer **supports** the following audio formats: ***.mp3***, ***.ogg***, ***.wav***, ***.mp2***, ***.mp1***, ***.aiff***, ***.m2a***, ***.mpa***, ***.m1a***, ***.mpg***, ***.mpeg***, ***.aif***, ***.mp3pro***, ***.bwf***, ***.mus***, ***.mod***, ***.mo3***, ***.s3m***, ***.xm***, ***.it***, ***.mtm***, ***.umx***, ***.mdz***, ***.s3z***, ***.itz***, ***.xmz***, ***.aac***, ***.adts***, ***.mp4***, ***.m4a***, ***.m4b***.

- **JAMMER** Jammer playlist
- **FOLDER** Folder/Directory (support playing all audio files within a folder)
- **YOUTUBE** Youtube video/playlist
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

---

#### Random info

You can also stack multiple jammer playlists inside another jammer playlists.

---

## Build / Run yourself

Download the **BASS** and **BASS_AAC** libraries from the [un4seen](http://www.un4seen.com/bass.html) website.

```bash
export LD_LIBRARY_PATH=/path/to/your/library:$LD_LIBRARY_PATH
```

```bash

dotnet run -- "path/to/song.mp3" ..
```

### Build

##### Windows

Add **BASS** and **BASS_AAC** libraries to the executable folder

```bash
-p:UseForms -- When true, uses forms to add global key listener
cd JAMMER.CLI or JAMMER.ELECTRON
dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true -p:UseForms={true|false}
```


Electron UI

Requires installation of [Electron.net](https://github.com/ElectronNET/Electron.NET)

##### Linux

Add **BASS** and **BASS_AAC** libraries to the executable folder and to $LD_LIBRARY_PATH.

```bash
cd JAMMER.CLI or JAMMER.ELECTRON
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true -p:UseForms=false
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

To create AppImage run `build.sh`

##### Build script for NSIS installer

```bash
build.bat     { CLI | FORMS } { CLI | ELECTRON }

CLI - Only barebone CLI version
FORMS - Includes global key listeners for windows
========================
CLI - CLI version
ELECTRON - UI-Electron version

```



you can also use `change_version.py` to change the version of the app.

```bash
python change_version.py [version]
```

---

![image](https://raw.githubusercontent.com/jooapa/jammer/main/jammer_HQ.png)

---

### Todo bug fixes

- [x] When playing song, and opening a new playlist, the song doesn't change
- [x] Pressing `0`, doesn't do anything
- [x] Download bar
- [x] Fix playlist cmd
- [ ] Fix time bar not going to new line when 0:-01

### Incoming Features

- [ ] Add more audio formats
- [x] Better f mode
- [x] pg up/down you can scroll the playlist and modify the selected song
