# <img src="https://raw.githubusercontent.com/jooapa/jammer/main/icons/trans_icon512x512.png" width="35px" align="left"> v3.53

## What's Changed
### Features & Improvements
* feat: add synchronous resolution for Spotify import URIs in Play and SpotifyIntegration services by @jooapa
* feat: add GitHub Actions workflow for building and packaging Linux and Windows artifacts by @jooapa
* feat: enhance build script to support local machine target detection and all-target compilation by @jooapa
* feat: release notes template and script by @jooapa
* feat: Update file permissions for scripts and add Homebrew formula for Jammer by @jooapa
* feat: Handle empty playlists by stopping music playback and resetting state by @jooapa
* feat: Make Windows NSIS installer opt-in via --nsis flag by @jooapa
* feat: Add per-song pitch shift metadata by @jooapa
* feat: Add JRead arrow callbacks for numeric input stepping and update locales by @jooapa
* feat: Add per-song playback metadata editor with speed, reverse, trim, and inline effects by @jooapa
* feat: Add Python build script and remove PowerShell build script for cross-platform packaging by @jooapa
* feat: Refactor macOS packaging logic to support cross-packaging on Linux and improve library validation by @jooapa
* feat: Enhance AppImage packaging process with automatic appimagetool download and improved error handling by @jooapa
* feat: Implement JavaScript runtime resolution for yt-dlp and enhance audio format handling by @jooapa
* feat: Add Spotify integration for playlist import and metadata handling by @jooapa
* feat: Add Puppeteer self-test and progress output in #141 by @retroaalto
* Add centralized path management and update tests by @jooapa
* Improve SoundCloud track download process with error handling and retry logic by @jooapa
* Add quick playlist switcher by @jooapa
* Complete Finnish and Portuguese localization by @jooapa
* Revamp settings and cross-platform integrations by @jooapa

### Fixes
* fix(playlist): prevent BASS FileOpen errors and add warning handling for unplayable/age-restricted tracks by @jooapa
* fix: Improve error handling in Invoke-External and adjust argument order for macOS packaging by @jooapa
* fix: properly dispose TagLib.File resources with using statements in #140 by @retroaalto

## Notes
- Per-song metadata editor lets you customize speed, reverse, trim, and effects for each song in your playlist
- Spotify integration imports playlists by resolving tracks to YouTube/SoundCloud
- Quick playlist switcher: press Tab to quickly switch between playlists
- Settings UI now responds immediately to key actions
- SoundCloud downloads are more reliable with automatic retry logic
- Full Finnish (fi) and Brazilian Portuguese (pt-BR) translations
- Build system now uses Python instead of PowerShell for better cross-platform support
- Homebrew support: `brew tap jooapa/jammer https://github.com/jooapa/jammer && brew install jammer`

**Full Changelog**: https://github.com/jooapa/jammer/compare/3.52...3.53
