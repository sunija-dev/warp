
# WARP - The Guild Wars 2 roleplaying overlay

[![WARP Trailer](https://github.com/sunija-dev/warp/assets/10253311/49e69526-81a1-463c-8d09-5c1b9bc578aa)](https://www.youtube.com/watch?v=wbPDP9DxHtg)

WARP is the unofficial roleplaying overlay for Guild Wars 2 for Windows. Its main features are

- See roleplayers on the map
- Easily to switch to their shard
- Write/read roleplay profiles

[>> Download <<](https://gw2warp.com/)

[Website](https://gw2warp.com/) / [Discord](https://discord.gg/GQB4RKCFwc)

*--- This is an early release of the repository. So you'll find many unfinished notes here. ---*

## What can I do with this repository?
Can I…

*…make a roleplaying overlay for another game?*

Yes! Many games support the MumbleAPI, which is the core piece to make WARP work. Though, don’t forget to replace the GW2 graphics, because ArenaNet would not allow their usage despite for GW2 fan projects.

*…make an overlay for GW2 with it?*

Yes, BUT a [BlishHUD](https://github.com/blish-hud/Blish-HUD) module would be the better way to do that. It is also written in C#, though it does not use Unity. I still highly recommend it, since you’ll save yourself from fixing my (slightly shaky) overlay code. And the installation will be wayyyy easier for your users.

*…add features to WARP?*

Kinda. WARP uses a dedicated server. When you edit the code, it won’t work with the WARP server anymore. You will have to host your own server and then you can basically make your own roleplaying overlay. If you intend on doing that, I’d recommend reaching out to me on Discord (username: sunija), so maybe I can help you.

*…slap in some 3D graphics from the asset store and make my own roleplaying MMO?*

Surprisingly, yes. WARP works like a full indie-MMO, though without the graphics and input (which are currently coming from the GW2 game that runs next to it). It definitely needs work, but I guess it’s not the worst project to start from, if you want to do something simple.

*…copy some scripts 1:1 and use them in my commercial game?*

Yes. This repo uses the very permissive MIT license.

*…copy the graphics and use them in my game?*

NO. The graphics belong to ArenaNet and are not covered by the license.

## License
The WARP code is released under the MIT license, unless noted otherwise (e.g. for external code). If you find code that is not properly credited, please let me know.

Artworks/graphic assets from Guild Wars 2 belong to ©2024 ArenaNet, LLC and are not covered by the MIT license.

## Technology
Engine: [Unity](https://unity.com/), C#

Overlay: [BlishHUD](https://github.com/blish-hud/Blish-HUD) / [CodeMonkey](https://www.youtube.com/watch?v=RqgsGaMPZTw)

Networking: [Mirror](https://mirror-networking.com/), with a “client + dedicated server” architecture

Server: Ubuntu vServer

## History
2020 Mar - German beta

2021 Aug - International beta, new design

2021 Dec - 1.0 release

2024 Mar - Open sourced

## Known bugs
- Disappearing on alt-tab
- Window sometimes off-screen

# Documentation
## How2Build
- Download
- Install Unity 2019.4.24f1
- Run server
  - Build WARP for Linux (headless)
  - Put Linux build on server (via ftp, eg. filezilla)
  - Put server\_files on server
  - Connect to your server (via ssh)
- Run client
  - Build WARP for Windows
  - Run start\_server.sh
  - Run your windows build
  - Run Guild Wars 2

Full build (with patcher, starter, utility):

- Open warp\_patcher and build it
- Open warp\_starter and build it
- Open warp\_utility and build it
- Put the libraries of warp\_utility in warp\_unity/Assets/external
- Open warp\_unity (as shown above) and build it

## Releasing a new version
- (You'll have to run it on your server)
- Unity: set the version number (in the build settings)
- Set the IP/domain of your server in the network_manager object in the "Network Address" field
- Make a windows build
- Throw the warp\_starter files in the folder of the build
- Zip it
- Upload the zip to the web path hardcoded in the patcher (atm weltenrast.de/warp/recore/WARP.zip)
- Make a linux build (needs to have same version number)
- Zip it, upload it to your linux server, unzip it
- Run it with the start_server.sh script (you find it in the project files)
- (TODO: Add remaining steps)
- (make a test build)
  - don’t add starter
  
## Server
- Ubuntu vServer
- 2 cores
- 4 gb ram
- 120gb harddisk
- Location: Germany (WARP is not very ping dependant, so it still works around the world)
- → Can be rented for less than 5€/month

## Let’s go through running WARP…
1. To explain how everything works, let’s talk through using WARP and what happens in the background.
1. Go to website https://gw2warp.com/  (wordpress, but I recommend bootstrap templates. They are free and don’t need maintenance)
1. Run the patcher

   1. (image of patcher)
   1. The patcher will always
      1. Download the WARP build from a hardcoded address (atm <https://weltenrast.de/data/recore/WARP.zip>)
      1. Delete the WARP folder next to it, if there is one (to remove the old version)
      1. Unzip the warp folder next to it
      1. Start the warp\_starter.exe in the folder
      1. (Yes, that means if people run the warp\_patcher to start WARP, it will always re-download itself. Happened some times)
1. The warp\_starter
   1. The WARP starter
1. (Setup process)


## Projects Overview
### <a name="_eksgihj4ysyv"></a>WARP Patcher
The patcher is a simple “patcher”. It just download the new version from a hardcoded location, deletes the old warp folder, unzips the new one and starts it.

It’s main purpose is (as with any patcher, duh) to download WARP for the first time, and can be used to download new versions later on. Since the patcher itself doesn’t check if there is actually a new version, WARP only starts it if there is actually one.

Patcher is a single .exe file, all libraries build in. Didn’t want a .zip because unzipping is tedious and already hard for some common users. It is its own program and not part of the main executable, because it has to delete the old WARP files, and an executable cannot delete itself (I think…?).

### WARP Starter
The WARP starter is a tiny program that starts WARP once GW2 is started.

It is necessary, because there’s no other way to only start WARP when GW2 is running. E.g. BlishHUD gets around this problem by just not closing itself when GW2 is closed - which is fine because it’s really lightweight in its sleep mode. But Unity isn’t as light-weight, so I wanted to close the main WARP program as often as possible.

The starter basically just checks every 2 seconds if there is a process that is called Gw2-64. If there is, and there is no warp process, it’ll start WARP. (Thinking about it, I think atm there’s a bug where the starter starts WARP in the account login screen, but WARP doesn’t recognize that window and closes itself, causing an annoying endless loop. 🤔)

The starter also causes the taskbar icon in the bottom right, from which the starter can be closed.

### WARP Utility
WARP Utility is a c# library that provides code to handle parts of the overlaying, the MouseHook (=Input) and access MumbleLink.

This code cannot be part of the Unity project, because Unity doesn’t allow to import the System.Drawings library. And it might contain some nuget libraries (for the mousehook?), of which I didn’t know how to get them into Unity. 🤔 Getting this code into Unity would make working with it easier. On the other hand… once it is built, you rarely have to touch it.

This codebase is a horrible mess of copied-together code from BlishHUD (for overlaying and mouse/keyboard hooks), and half of the methods are maybe not called anymore because I was able to move them to Unity.

### WARP Unity
This is the main project, which contains what you see ingame.

The project basically works like a full MMO with a dedicated server. Since it is build with Mirror, it uses the same project as client and server. When you build it for Windows it acts as a client, when you build it for headless linux it acts as server.

In a normal MMO you would move your character/camera with WASD+Mouse. In WARP you move your character/camera by moving them in GW2. WARP reads your new position/camPosition from the MumbleAPI that GW2 implements. So, WARP actually renders from the same position as GW2. Which means you could also put 3D objects in the world and they would render perfectly fine (though only as overlay, and not actually as part of the world. Kinda like X-Ray vision.).

## Code Convention
1. Simplified hungarian notation that I learned at game studios
   1. Put class abbreviation in front of var name. E.g. iPlayer for int, textName for TMP\_Text.
   1. Also before classes (except void, duh)
   1. \_ in front of parameter vars.
   1. Sometimes I used m\_ for member vars.
1. Hungarian doesn’t have the best rep, but I found it helps a lot. Especially because you often have multiple vars for one concept. E.g.
   1. Player = player class
   1. iPlayer = index of the player in a list
   1. strPlayerName = string, player name
   1. textPlayerName = text field that displays the player name
1. Also, you can see what’s your code (= very likely to contain bugs) and build-in code (= less likely to contain bugs)
1. And it’s harder to mess up variable names completely, even if you don’t have much time to think about them.


## Translator Guideline
(Not super relevant, but might be interesting to see.)

Thank you for volunteering to translate WARP! :)

1. **General**
   1. **This is voluntary.**
      Whenever new translations are needed, I’ll inform you about it. If you have time to translate: That’s awesome! If not, that’s also fine. Just tell me, so I can plan with it. ;)
   1. **Credits**
      Please write to me how you want to be credited on the “about”-page (e.g. your Discord name, your GW2 tag, both, …).
1. **Guidelines**
   1. **Informal language.**
      “Du” instead of “Sie”, “tu” instead of “vous”. It’s a game after all. ;)
   1. **Inclusive language.**
      Whenever possible, address all genders.
   1. **Short and understandable.**
      Don’t have to translate 1:1, rather keep it as short and understandable as possible. Except…
   1. **Exact wording for GW2 options/buttons.**
      E.g. the “Windowed Fullscreen” option should be named *exactly* as in GW2. That makes it a lot easier for players to find them.
   1. **Don’t force it. ;)**
      If an English word is the common term for sth in your language, just keep it. No need to translate “login” if that’s the common word for it.
   1. **No slurs.**
      Obviously. ;) But also not if you are doing a temporary translation that is not intended for publication. If you put it in the sheet, it might appear like that ingame.
1. **Tech Tips**
   1. **Remove spaces from links/code.**
      </ color > → </color>
   1. **Automatic translation.**
      =googletranslate([click english field], “en”, “fr”)


## Features
1. Overlaying
   1. How to make overlay window
   1. Mouse hook
   1. Select on input field (because keyhook didn’t work)
   1. On Win10 is really easy, even in fullscreen now
1. Inputs
   1. Via mousehook, because overlay should not have focus. Mousehook gets mouse positions and clicks and simulates them (which is possible with unity’s new input system)
   1. …but keyboard hook didn’t work out so well. Would be especially hard with special characters in french, german, etc. So instead, when you click on a text field it actually focuses WARP and you type directly in it. That’s done with a raycast that checks if you’re hovering a text field. That’s also why it is suggested to turn on the “play music if window is not focussed” setting in GW2 - otherwise the music stops when you click in a text field.
   1. The keyboard hook can simulate keyboard strokes though. That is used to put text in the chat (e.g., to send invites to players). In that case, it puts the text in the clipboard, presses Enter to open the chat, presses ctrl+v to paste it in, and then enter again to send it.
   1. Definitely do not overuse that functionality. ANet seems to be fine with that limited use, but using that tech to build spambots (or just regular bots) will definitely get you (rightfully) banned.
1. Database
   1. Sqlite
   1. SqliteBrowser
   1. Easy to use sql library
   1. It is basically only used to dump information. Once you log in, everything is loaded to RAM. Even with the old version of WARP that had 1000+ items flying around, I’d load them all to RAM because I feared the DB would be too slow. Is that correct? I don’t know.
1. Networking
   1. Mirror
   1. Client/Server in same project
   1. Player class is most important
   1. Explain Cmd, RPC, SyncVar
   1. Needs to be same build
1. MumbleAPI
   1. Actually a library for mumble (= open source teamspeak = discord). Allows positional audio (= you hear people louder if they are close). Many games have it (Gw2, WoW, minecraft, …)= Games can implement this API to share your position to mumble. But can also be read from other games. Thus it’s the basis for a lot of overlays.
   1. MumbleDebugLog
1. GW2 API + Login
1. GW2 style ui
   1. Scraping
1. Patching
   1. Server tells client it’s version with a network message. If WARP has an oldver
1. Localization
   1. Uses unity’s localization system
   1. Can be connected to google doc. Send it people to localize (name localization people)
1. Icon Selector
   1. Linq
   1. Scraped icons
1. Character/Account sheets, Aspects, CloseCharsList
   1. Show on hover/mini charlist. Proximity displaying
   1. Saved to db
   1. Aspects are basically entries there
1. Notes
1. Character List
   1. Synced independently from sheets, because no proximity.
   1. If you wondered why you couldn’t check out profiles there… that’s it. I’d have to make a special request to get the sheet. I started that with “RequestableObjects”
1. IP changer
1. Admin commands
   1. Needs to be admin in db but still needs pw, because I don’t trust myself. I guess that’s 2 factor
   1. Not much there, could be more
1. Feedback / Reports
1. MapMarkers
1. WikiMap
1. Settings
1. TreasureHunt
1. Unfinished
   1. Dice
   1. EmoteBoard
   1. Spots

## Feature Ideas (Not added)
1. WARP Chat
1. Items (again)
   1. aspects
   1. placing
   1. finding
   1. “plots”
   1. aspects
   1. item managment (my items, show in world, etc)
   1. inventory (+bank)
   1. feedback (how often picked up, likes,...)
   1. combination & logic (conditions)
   1. delete after some time
1. See people you played with
1. Set status
1. Make better overview on sheet
1. Thankuuuus
1. Beginner Tutorial (like on how2rp.com)
   1. Master Guide
   1. Leveling Guide
1. RP-Finder
   1. Classifieds (+ Sorting)
   1. Account pages
   1. Filter
   1. Suggestions
   1. Rumors
   1. Guild/Group ads
   1. (Ads in Loading screens)
   1. Group-Chats…?
   1. “Last played with”
1. Community
   1. Worldchat?
   1. Post small infos about your RP
   1. Anecdotes (+ Likes), share funny or epic moments
1. Personalization
   1. add quick buttons to windows
1. Housing
   1. display objects
   1. housing zone
   1. show effects, dead people, ...
1. RP-Tipps
   1. warp tutorial
   1. rp tips
   1. character creation gothrough
   1. notes about your character
1. Debugging feature?
1. Feedback
   1. start page
   1. write down diary (+ auto screenshots?)
1. XP
   1. Likes
   1. Shop
   1. Boost-Zones
   1. XP bar (and fx)
1. Calendar
1. npc sheets (maybe if we spawn npcs…?)
