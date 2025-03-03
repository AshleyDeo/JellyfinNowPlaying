The Rainmeter plugin for Jellyfin Music

## Install
* Install correct version of JellyfinNowPlaying
* Newtonsoft.Json.dll required in C:\Program Files\Rainmeter\
## Setup
**API Key from Jellyfin required**

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

Measure Types are:

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
- `Volume`: From 0 to 100 ^
- `State` ^: 0 for stopped, 1 for playing, and 2 for paused
- `Rating` ^: Favorite
- `Repeat`: 0 if repeat/loop track is off, 1 if repeating one track, 2 if repeating all.
- `Shuffle` ^: 0 if shuffle is off, 1 if on.
^ : Depends on if client reports to server
## Not working
 *Jellyfin Api remote controls not working (yet?)*
* `SupportsRemoteControl` ^: 0 or 1 if the current media supports remote media control buttons.
* Play/Pause/other Media controls:. Bangs for basic Windows Media controls work if client supports it
* Artist Backdrop/Logo: downloads but loading into skin crashes

*Uses monstercat visualizer skins*