# Jammer - Play songs from cli with youtube and soundcloud support

![banner](https://raw.githubusercontent.com/jooapa/jammer/main/.github/img/jammer-banner.gif)

<br>

*In this example I first open the PvZ playlist from SoundCloud. Then I delete some songs at the start using the `delete`. Then I save the current playlist to name `plant`. After exiting i use the inline cli-playlist tool `jammer playlist play plant` this will play the plant playlist.*

## Introduction

- Jammer is a simple CLI music player that supports playing songs from local files, youtube and soundcloud. For `Windows`, `Linux` and `OSX`.
- The player doesn't stream the songs but downloads them to local storage.
- It can also make these playlists that can be saved and loaded later.
- The jammer folder is located in the user's home directory and contains the downloaded songs, playlists and settings.
- It uses [Bass](https://www.un4seen.com/) for playing the songs, [SoundCloudExplode](https://github.com/jerry08/SoundCloudExplode), [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) for downloading the songs and [Spectre.Console](https://github.com/spectreconsole/spectre.console) for the UI.

## Install/Update

### Install

Github latest [Release](https://github.com/jooapa/signal-jammer/releases/latest)

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
jammer selfdestruct   deletes jammer
jammer start          opens jammer folder
jammer --help         show help
jammer -d             debug mode
jammer --version      show version
```

*when using soundcloud or youtube links dont forget to use **`https://`** at the start.*

```bash
jammer playlist play <name>
jammer playlist create <name>
jammer playlist delete <name>
jammer playlist add <name> <song> ...
jammer playlist remove <name> <song> ...
jammer playlist show <name>
jammer playlist list
```

### Supported formats

Jammer **supports** the following audio formats: ***.mp3***, ***.ogg***, ***.wav***, ***.mp2***, ***.mp1***, ***.aiff***, ***.m2a***, ***.mpa***, ***.m1a***, ***.mpg***, ***.mpeg***, ***.aif***, ***.mp3pro***, ***.bwf***, ***.mus***, ***.mod***, ***.mo3***, ***.s3m***, ***.xm***, ***.it***, ***.mtm***, ***.umx***, ***.mdz***, ***.s3z***, ***.itz***, ***.xmz***

- **JAMMER** Jammer playlist
- **FOLDER** Folder/Directory (support playing all audio files within a folder)
- **YOUTUBE** Youtube video
- **SOUNDCLOUD** Soundcloud song/playlist

### Player Controls

| key | action |
|  --------  |  -------  |
| `h` | show/hide help |
| `c` | show/hide settings |
| `f` | show/hide playlist view |
|    |    |
| `space` | play/pause |
| `q` | quit |
| `→` | forward |
| `←` | backward |
| `↑` | volume up |
| `↓` | volume down |
| `m` | mute/unmute |
| `L` | toggle loop |
| `s` | toggle shuffle |
| `r` | play in random song |
| `n` | next song in playlist |
| `p` | previous song in playlist |
| `Delete` | delete current song from playlist |
| `F2` | show playlist options |
| `tab` | show CMD help screen|
| `0` | goto start of the song|
| `9` | goto end of the song|

### Playlist Controls

| key | action |
| ------ | ----------- |
| `shift + A`| Add song to playlist |
| `shift + D`| Show songs in other playlist |
| `shift + F`| List all playlists |
| `shift + O`| Play other playlist |
| `shift + S`| Save playlist |
| `shift + alt + S`| Save as |
| `alt + S`| Shuffle playlist |
| `shift + P`| Play song(s) |

### Example usage

```bash
jammer "path/to/song.mp3" ./another/song.aac C:\Users\user\jammer\playlists\playlist.jammer "path/to/folder"
```

```bash
jammer https://soundcloud.com/angry-birds-2009-2014/haunted-hogs https://soundcloud.com/cohen-campbell-823175156/sets/angry-birds-epic
```

```bash
jammer https://www.youtube.com/watch?v=4zjFDTIROhQ
```

You can also use `-d` flag that will add logs to current folder.

#### Random info

You can also stack multiple jammer playlists inside another jammer playlists.

### Build / Run yourself

When running the app you need to have the right **bass library** in the **same folder** as the **executable**. The libary is **included** in the **libs folder**. You can also download the library from [un4seen](http://www.un4seen.com/).

```bash

dotnet run -- "path/to/song.mp3" ..
```

#### Build

##### Windows

```bash
dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true
```

##### Linux

```bash
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true
```

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

[ ] When playing song, and opening a new playlist, the song doesn't change
[ ] Pressing  `0`, doesn't do anything
[ ] Download bar
[ ] Fix playlist cmd
[ ] Fix time bar not going to new line when 0:-01

### Incoming Features

[ ] Add more audio formats
[ ] Maybe use curses for UI
[ ] Linux AppImage
