# signal jammer 

![banner](https://raw.githubusercontent.com/jooapa/signal-jammer/412b81208a1b783fd0034d5a23b43a223c80e51b/images/banner.png)
## Play songs from cli

### Usage
```
jammer [song] ... [song]
jammer soundcloud.com/username/track-name 
jammer start // opens jammer folder
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
| `s` | shuffle |
| `h` | show/hide help |
| `c` | show/hide settings |


### Install
Github latest [Release](https://github.com/jooapa/signal-jammer/releases/latest)

### Build/Run yourself
```
dotnet build --configuration Release
```
```
dotnet run -- "path/to/song.mp3"
```