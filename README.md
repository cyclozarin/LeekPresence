# LeekPresence

Since original game's Discord Rich Presence is not very interesting enough, this mod extends it with adding several new things:
- Large and small icons to the presence status
- Providing information about local player and whenether is camera filming or not (updated every 10 seconds)
- Abillity to invite friends through Discord (if they have LeekPresence too)
- Some more Rich Presence statuses such as if players just got back from the Old World and are watching the video or unfortunately losing the quota

## Configuration

As of now you can only change Discord App ID for abillity to change presence icons, but more config will probably be added in later updates.
(That is first mod that uses BepInEx config, so you need to change it in your mod manager's Config Manager or mod's cfg file in BepInEx config folder)

- Discord App ID (long)
  - Description: Determines Discord App ID for mod to use.
  - Default value: `1278755625123184681`
  
## Icon values for configuration

- Large icons
  - `mainmenu` - shows up in main menu and when crew got back from the old world
  - `lobby` - shows up when in non-started lobby
  - `surface` - shows up when in started lobby on surface
  - `factory`, `harbour`, `mines` - shows up in old world
  - `failedquota` - shows up when failing quota
  - `watchingvideo` - shows up in process of watching video
- Small icons
  - `cw` - shows up when virality isnt loaded
  - `virality` - shows up when virality is loaded
  - `alive`, `dead`, `everyoneisdead` - indicates whenether or not you're alive, dead or everyone has died
  
*P. S. Name is mispelled intentionally as the reference to LiveLeek, a Streamer's streaming platform as been officially stated by one of the cw devs Wilnyl.*
