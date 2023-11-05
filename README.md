# signal jammer 
## play songs from cli

### usage
```
jammer [song] 
jammer soundcloud.com/username/track-name 
jammer start 
```

### encoding
jammer can play
- mp3
- wav
- ogg
- flac

### controls

| key | action |
|  --------  |  -------  |
| `space` | play/pause |
| `q` | quit |
| `→` | forward 5s |
| `←` | backward 5s |
| `↑` | volume up |
| `↓` | volume down |
| `m` | mute/unmute |
| `L` | toggle loop |

### install
Github latest [Release](https://github.com/jooapa/signal-jammer/releases/latest)

### build
```
dotnet build --configuration Release
```
### run
```
dotnet run -- "path/to/song.mp3"
```