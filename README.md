# jammer 

![banner](https://raw.githubusercontent.com/jooapa/jammer/main/images/jammer_banner.gif)

## Play songs from cli with youtube and soundcloud support

### Usage
```bash
jammer [song] ... [song]
jammer soundcloud.com/username/track-name 
jammer soundcloud.com/username/sets/playlist-name
jammer youtube.com/watch?v=video-id
jammer start          // opens jammer folder
jammer playlist       // playlist controls
jammer selfdestruct   // deletes jammer
jammer start          // opens jammer folder
jammer --help         // show help
jamemr --version      // show version

```
```

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
| `F2` | show playlist options |


### Install
Github latest [Release](https://github.com/jooapa/signal-jammer/releases/latest)

### Build/Run yourself
```
dotnet build --configuration Release
```
```
dotnet run -- "path/to/song.mp3"
```
```
dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true
```

![image](https://raw.githubusercontent.com/jooapa/jammer/main/jammer_HQ.png)