# jammer - Play songs from cli with youtube and soundcloud support

![banner](https://raw.githubusercontent.com/jooapa/jammer/main/.github/img/jammer-banner.gif)

<br>

*In this example I first open the PvZ playlist from SoundCloud. Then I delete some songs at the start using the `delete`. Then I save the current playlist to name `plant`. After exiting i use the inline cli-playlist tool `jammer playlist play plant` this will play the plant playlist.*

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

- **FLAC:** Free Lossless Audio Codec
- **AAC:** Advanced Audio Coding
- **WMA:** Windows Media Audio
- **JAMMER:** Jammer playlist
- **FOLDER:** Folder/Directory (to support playing all audio files within a folder)
- **MP3:** MPEG Audio Layer III
- **OGG:** Ogg Vorbis
- **WAV:** Waveform Audio File Format
- **MP2:** MPEG Audio Layer II
- **MP1:** MPEG Audio Layer I
- **AIFF:** Audio Interchange File Format
- **M2A:** MPEG-1 Audio Layer II
- **MPA:** MPEG Audio
- **M1A:** MPEG-1 Audio Layer I
- **MPG:** MPEG-1 Audio Layer III
- **MPEG:** Moving Picture Experts Group
- **AIF:** Audio Interchange File Format
- **MP3PRO:** MPEG Audio Layer III PRO
- **BWF:** Broadcast Wave Format
- **MUS:** Finale Music Score
- **MOD:** Tracker Module
- **MO3:** Compressed MOD format
- **S3M:** ScreamTracker 3 Module
- **XM:** FastTracker 2 Extended Module
- **IT:** Impulse Tracker Module
- **MTM:** MultiTracker Module
- **UMX:** Unreal Music Package
- **MDZ:** Compressed MMD3 Module
- **S3Z:** Compressed S3M Module
- **ITZ:** Impulse Tracker 2 Module
- **XMZ:** Compressed XM Module
- 

### Controls

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

### Playlist Options

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
