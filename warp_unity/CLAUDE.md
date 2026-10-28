# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository layout

The git root is `C:\Data\warp_os_repo` (one level above this folder). It holds four separate projects that make up WARP, a Guild Wars 2 roleplaying overlay:

- `warp_unity/` – the main Unity project (client **and** server). This is where nearly all work happens.
- `warp_utility/` – legacy C# class library (overlay, mouse hook, MumbleLink, mostly copied from Blish HUD). No longer used: the parts WARP needs were ported into `warp_unity/Assets/modules/native` (`WarpNative` asmdef, unsafe code enabled).
- `warp_starter/` – small WPF app that launches WARP once `Gw2-64` is running.
- `warp_patcher/` – downloads the WARP zip from a hardcoded URL, replaces the old folder and starts the starter.

`README.md` at the git root has the long-form documentation (feature list, release process, code conventions). Its "How2Build" section mentions Unity 2019.4; that is outdated. The project is on **Unity 6000.3.17f1** (`ProjectSettings/ProjectVersion.txt`).

## Building and running

There is no CLI build/test/lint setup. Everything runs through the Unity Editor.

- **Client**: build for Windows standalone from the Editor.
- **Server**: menu **Warp > Server > Build Linux Server** (`Assets/server_files/Editor/ServerDeploy.cs`). It builds a Linux Dedicated Server player to `Build/Server/warp_server.x86_64` using the enabled scenes. **Build and Deploy** also runs `Tools/deploy_server.ps1`, which zips the build, uploads it with scp and restarts it on the vserver.
- Deploy settings live in `Tools/deploy_config.json` (copy `deploy_config.example.json`; needs the OpenSSH client and key-based login).
- On the server, `Assets/server_files/start_server.sh` loops forever: it unzips a new `SERVER_LINUX.zip` if present, then runs `warp_server.x86_64 -batchmode`. The server exits and restarts on its own to pick up updates.
- Client and server builds must have the **same version number**, because the server tells the client its version on connect and the client uses it to decide whether to patch.
- `Assets/modules/testing/TestScript.cs` and `Assets/modules/networking/test/` hold test helpers. There is no automated test suite. Test by playing in the Editor.

### Testing in the Editor

`WarpManager` (`Assets/modules/managers/WarpManager.cs`) is the entry point on the scene object in `Assets/scenes/main_scene.unity`. Its inspector options drive local testing:

- `m_warpMode`: `HOST` (client + server in the Editor, connects to localhost), `CLIENT`, or `SERVER`.
- `m_bConnectToLocal`: a client connects to localhost instead of the real server.
- `m_bUseMinaData`: simulates minimal Mumble data (name and position) so GW2 doesn't need to be running.
- `m_bUsePTRPort`: uses port 7778 (test server).

## Architecture

**One project, two roles.** WARP works like a small MMO built on Mirror networking. The same code base runs as a Windows client and as a headless Linux dedicated server. Scripts use `isServer`/`isClient` checks and `#if UNITY_EDITOR` blocks for role-specific behaviour. Client and server must be the same build.

**No local input or camera.** The player's position and camera come from the GW2 MumbleLink API (`modules/gameintegration/MumbleManager.cs`, backed by `warp_utility.dll`), not from WASD or the mouse. WARP renders over GW2 as a transparent overlay (`modules/transparentwindow`).

**Input** (`modules/input`): the overlay window never has focus, so a mouse hook feeds simulated clicks into Unity's input system. Keyboard hooks don't work well, so clicking a text field raycasts and actually focuses the WARP window to type. Sending text to GW2 chat works by putting it in the clipboard and simulating Enter, Ctrl+V, Enter. Keep that use minimal (ArenaNet tolerates it only in that limited form).

**Code layout.** Features are separate folders under `Assets/modules/` (char_sheet, account_sheet, aspects, map_manager, wikimap, treasure_hunt, settings, localization, etc.). Cross-cutting pieces are:

- `modules/managers/` – `WarpManager`, `ClientManager`, `ServerManager`.
- `modules/networking/` – `WarpNetworkManager`, the `Player` and `Character` network objects, `WarpInterestManagment` (proximity-based visibility, tuned by `server_settings.txt` values such as visibility range and tick rate), and `requestables/` for on-demand data requests such as profile sheets.
- `modules/database/Database.cs` – SQLite (`Database.sqlite`, with `Database_debug.sqlite` in the Editor). The server uses the DB mostly as a persistence dump. After login everything is held in RAM. Server DB backups run on an interval (`fBackupDBInterval`).
- `modules/_data/` – serializable data classes (`CharacterSheet`, `AccountSheet`).

**Networking vocabulary.** The code uses Mirror's `[Command]` (client to server), `[ClientRpc]`/`[TargetRpc]` (server to client) and `[SyncVar]`. `Player` is the central networked class.

**Localization** uses Unity Localization (`Assets/localization`), connected to a Google sheet for translators.

**Third-party / legacy code**: `Assets/Mirror`, `Assets/NaughtyAttributes` (inspector attributes such as `[ShowIf]`), `Assets/TextMesh Pro` and `Assets/old_project` are imported or legacy. Don't refactor them. `Assets/modules/native` is Blish HUD-derived WinAPI code (keeps its `Blish_HUD` / `SuMumbleLinkGW2` namespaces and style). `Assets/external` holds `AsyncClipboardService.dll`, used for the clipboard. `Assets/scenes/unused` is dead.

**Scripting define symbols** (`ProjectSettings/ProjectSettings.asset`) include many `MIRROR_*_OR_NEWER` entries and `EDGEGAP_PLUGIN_SERVERS`. Unity regenerates them, so don't edit them by hand.

## Code conventions

The author uses simplified Hungarian notation (see the README). Follow it in new code:

- A type prefix goes before the name: `iPlayer` (int index), `strPlayerName` (string), `fVisibilityRange` (float), `bFlag`, `v3Pos`, `textPlayerName` (a TMP_Text), `arVersions` (array).
- Parameters start with `_`. Member variables sometimes use `m_`, e.g. `m_warpMode`, `m_bAutoStart`.
- Class names also carry a prefix (except `void`).

Additional style rules:

- Keep changes small and purposeful. No drive-by renames, reformats or refactors of unrelated code.
- Types are declared in the global namespace; no file-scoped namespaces.
- Never use `var`; always write the explicit type.
- Use target-typed `new()` when the type is already declared on the left (`MyClass obj = new();`, `List<int> list = new();`).
- One type per file, with the file name matching the primary type. Small helper classes, abstract base classes or enums closely tied to the main type may share its file.
- For null checks prefer the implicit bool check (`if (!Player.Local)`) over `== null` / `!= null`.
- Use `/// <summary>` sparingly, only for public API whose behaviour isn't obvious from the name. Use inline `//` comments for non-obvious logic, `// TODO-...` and `// REFACTOR: ...`. No comments that just restate the code.
- LINQ is fine for filtering, sorting and ordering collections.
- Use nested `[System.Serializable]` classes for structured Inspector data, and `List<T>` for serialized collections.
- Store coroutines in a field so they can be stopped and restarted safely (`if (m_coroutine != null) StopCoroutine(m_coroutine);`).
- Prefer UnityEvents for cross-system, Inspector-visible communication. Use plain C# `event Action` for tighter, code-only contracts.

## Working notes

- Generated or noisy paths: `Library/`, `Temp/`, `obj/`, `Logs/`, `UserSettings/`, `*.csproj` and `*.sln(x)`. Don't hand-edit or review these.
- Every asset has a `.meta` file. Keep it with the asset when moving, renaming or deleting.
- Unity-side introspection (console logs, scene or camera capture, running editor commands) is available through the `unity-mcp` tools when the Editor is open.
