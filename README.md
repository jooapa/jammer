# signal jammer 

![jammer](https://user-images.githubusercontent.com/16443111/132134202-5b0b9b9a-4b0a-4b0d-8b0a-9b0b0b0b0b0b.png)
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