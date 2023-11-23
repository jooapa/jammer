# jammer 

![banner](https://raw.githubusercontent.com/jooapa/jammer/main/images/jammer_banner.gif)

## Play songs from cli with youtube and soundcloud support

### Usage
```
jammer [song] ... [song]
jammer soundcloud.com/username/track-name 
jammer soundcloud.com/username/sets/playlist-name
jammer youtube.com/watch?v=video-id
jammer start // opens jammer folder
jammer playlist // playlist controls
jammer selfdestruct // deletes jammer
```

### Supported formats
- mp3
- wav
- ogg
- flac

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

![image](https://raw.githubusercontent.com/jooapa/jammer/main/jammer.png)