# jammer - Play songs from cli with youtube and soundcloud support

![banner](https://raw.githubusercontent.com/jooapa/jammer/main/.github/img/jammer-banner.gif)
*In this example I fist open the PvZ playlist from SoundCloud. Then I delete some songs at the start using the `delete`. Then I save the current playlist to name `plant`. After exiting i use the inline cli-playlist tool `jammer playlist play plant` this will play the plant playlist.*
## Usage

```bash
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

_when using soundcloud or youtube links dont forget to use `https://` at the start._

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

- **MP3:** MPEG Audio Layer III
- **OGG:** Ogg Vorbis
- **WAV:** Waveform Audio File Format
- **FLAC:** Free Lossless Audio Codec
- **AAC:** Advanced Audio Coding
- **WMA:** Windows Media Audio
- **MP4:** MPEG-4
- **JAMMER:** Jammer playlist
- **FOLDER:** Folder / Directory

### Controls

| key | action |
|  --------  |  -------  |
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
| `h` | show/hide help |
| `c` | show/hide settings |
| `f` | show/hide playlist view |
| `n` | next song in playlist |
| `p` | previous song in playlist |
| `Delete` | delete current song from playlist |
| `F2` | show playlist options |
| `tab` | show CMD help screen|
| `0` | goto start of the song|
| `9` | goto end of the song|

### Install

Github latest [Release](https://github.com/jooapa/signal-jammer/releases/latest)

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

You can also use `-d` flag that will add logs to to local jammer folder.

### Build/Run yourself

```bash

dotnet run -- "path/to/song.mp3" ..
```

```bash
dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true
```

```bash
build.bat
```

you can also use `change_version.py` to change the version of the app.

```bash
python change_version.py [version]
```

![image](https://raw.githubusercontent.com/jooapa/jammer/main/jammer_HQ.png)
