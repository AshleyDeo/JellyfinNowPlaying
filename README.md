The Rainmeter plugin for Jellyfin Music

## Install
### **Rainmeter Skin** (recommended)
1. Install rmskin.
2. Download Newtonsoft.Json.dll
3. Place Newtonsoft.Json.dll in C:\Program Files\Rainmeter
4. **BEFORE** loading skins <u>**Add Server URL** to variables.ini in JellyfinNowPlaying/@resources</u>
<br>**_Skin will crash Rainmeter if Server Url not working_**

### **DLL files only**
1. Download zip
2. Place correct version of JellyfinNowPlaying.dll in C:\Program Files\Rainmeter\Plugins
3. Place Newtonsoft.Json.dll in C:\Program Files\Rainmeter

## Setup
**API Key from Jellyfin required**
<br> Check if working by going to `YOUR_SERVER_URL/Sessions?ApiKey=YOUR_API_KEY`

```
[Variables]
JellyfinServer="YourServerUrl"
JellyfinApiKey=YourApiKey
JellyfinUsername=YourUsername

; This will return the path to the cover art.
[MeasureCover]
Measure=Plugin
Plugin=JellyfinNowPlaying
ServerUrl=#JellyfinServer#
ApiKey=#JellyfinApiKey#
UserName=#JellyfinUsername#
DefaultPath=#@#Images\nocover.png
Type=Cover
UpdateDivider = 5

[MeasurePosition]
Measure=Plugin
Plugin=JellyfinNowPlaying
UseInfo=MeasureCover ;Reuses login info from MeasureCover
Type=Position
UpdateDivider=5
```

Measure Types are:<br>
^ : Depends on if client reports to server

- `Status`: 0 for inactive and 1 for active.
- `Player`: Player Name (Jellyfin Web, Finamp, Feishin...)
- `Title`: Track title.
- `Artist`: Track artist.
- `Album`: Track album.
- `Cover`: Path to cover art.
- `CoverUrl`: URL for cover art from server.
- `Backdrop`: Path to artist backdrop art.
- `BackdropUrl`: URL for artist backdrop art from server.
- `Logo`: Path to artist logo art.
- `LogoUrl`: URL for artist logo from server.
- `Duration`: Total length of track formatted into HH:MM:SS string
- `DurationSeconds`: Total length of track in seconds.
- `Position`: Current position in track formatted into HH:MM:SS string
- `PositionSeconds`: Current position in track in seconds.
- `Remaining`: Remaining time of track formatted into HH:MM:SS string
- `RemainingSeconds`: Remaining time of track in seconds.
- `Progress`: Percentage of track completed.
- `Volume`^: From 0 to 100
- `State` ^: 0 for stopped, 1 for playing, and 2 for paused
- `Rating` ^: Favorite
- `Repeat`^: 0 if repeat/loop track is off, 1 if repeating one track, 2 if repeating all.
- `Shuffle` ^: 0 if shuffle is off, 1 if on.
## Not working
 *Jellyfin Api remote controls not working (yet?)*
* `SupportsRemoteControl` ^: 0 or 1 if the current media supports remote media control buttons.
* Play/Pause/other Media controls: Bangs for basic Windows Media controls work if client supports it
* Artist Backdrop/Logo: downloads but loading into skin crashes
* Skin crashes rainmeter if server is not valid
* Strings return 0 if null

## Fixes
Rainmeter freezes
1. Close using Task Manager
2. Open Rainmeter.ini in C:\Program Files\Rainmeter
3. Set `Active=0` under [JellyfinNowPlaying\Cover] and [JellyfinNowPlaying\Dashboard]
4. Check if variables.ini or anywhere where your server URL is correct


*Based monstercat-visualizer and WebNowPlayingRedux skins*