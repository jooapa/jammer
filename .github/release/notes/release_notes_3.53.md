# <img src="https://raw.githubusercontent.com/jooapa/jammer/main/icons/trans_icon512x512.png" width="35px" align="left"> v3.53

## What's Changed
* feat: Per-song playback metadata editor with speed, reverse, trim, inline effects, and pitch shift
* feat: Spotify playlist import and metadata handling
* feat: Quick playlist switcher (Tab key)
* feat: Settings UI responsiveness improvements with immediate redraw
* feat: Improved SoundCloud track downloads with error handling and retry logic
* feat: Complete Finnish and Portuguese localization
* feat: Centralized path management
* feat: Python build script replacing PowerShell for cross-platform packaging
* feat: Homebrew formula for macOS and Linux installation
* fix: Properly dispose TagLib.File resources
* fix: Handle empty playlists by stopping playback and resetting state
* fix: Improved macOS packaging and cross-compilation support

## Notes
- Per-song metadata editor lets you customize speed, reverse, trim, and effects for each song in your playlist
- Spotify integration imports playlists by resolving tracks to YouTube/SoundCloud
- Quick playlist switcher: press Tab to quickly switch between playlists
- Settings UI now responds immediately to key actions
- SoundCloud downloads are more reliable with automatic retry logic
- Full Finnish (fi) and Brazilian Portuguese (pt-BR) translations
- Build system now uses Python instead of PowerShell for better cross-platform support
- Homebrew support: `brew tap jooapa/jammer && brew install jammer`

**Full Changelog**: https://github.com/jooapa/jammer/compare/3.52...3.53
