# WARP setup tutorial

Everything you need to get WARP running: the Unity project, a server on a Linux vserver, and one-click deploys from the Unity Editor.

The feature list and release process are in [README.md](README.md). This file covers the setup. (The README's "How2Build" section still says Unity 2019.4. The project now uses **Unity 6000.3.17f1**.)

## 1. Prerequisites

- **Unity 6000.3.17f1**, installed through Unity Hub with these modules:
  - Windows Build Support (for the client)
  - **Linux Dedicated Server Build Support** (for the server)
- Windows 10/11 with the **OpenSSH Client** (installed by default; check under Settings > Apps > Optional features).
- A Linux vserver you can log into with ssh, with `unzip` installed.
- Guild Wars 2, to play the client.

## 2. Open the project

Open `warp_unity/` in Unity Hub. The other folders at the git root are separate projects:

| Folder | What it is |
|---|---|
| `warp_unity/` | Main Unity project (client and server) |
| `warp_utility/` | C# library (overlay, input hooks, MumbleLink). Its DLLs go into `warp_unity/Assets/external`. |
| `warp_starter/` | WPF app that starts WARP when GW2 runs |
| `warp_patcher/` | Downloads the WARP zip and starts the starter |

To test locally without a server, use the `WarpManager` options in `Assets/scenes/main_scene.unity`: set `m_warpMode` to `HOST`, and optionally enable `m_bUseMinaData` so GW2 doesn't need to be running.

## 3. Set up the SSH key

Deploys use key-based ssh login, so no password is needed. Do this once.

### 3.1 Create a key (Windows, PowerShell)

```powershell
ssh-keygen -t ed25519
```

Press Enter to accept the default path (`C:\Users\<you>\.ssh\id_ed25519`). Use a passphrase if you like. Section 3.3 covers how that works with deploys.

This creates two files:
- `id_ed25519`: the **private** key. Never share it or commit it.
- `id_ed25519.pub`: the **public** key. This is the one that goes on the server.

### 3.2 Put the public key on the server

Run this on Windows (PowerShell). It copies the public key to the server and asks for your server password one last time. The first time you connect to a server, ssh also asks whether to trust it. Type `yes`. The deploy can't answer that question itself, so it has to happen here once.

```powershell
Get-Content $env:USERPROFILE\.ssh\id_ed25519.pub | ssh -p 22 USER@HOST "mkdir -p ~/.ssh && chmod 700 ~/.ssh && cat >> ~/.ssh/authorized_keys && chmod 600 ~/.ssh/authorized_keys"
```

Test it. This must print `key-works` without asking for a password:

```powershell
ssh -o BatchMode=yes -p 22 USER@HOST echo key-works
```

### 3.3 Passphrase and ssh-agent (only if the key has a passphrase)

The deploy runs without a console, so ssh can't ask for a passphrase. Load the key into the Windows ssh-agent once instead.

In an **admin** PowerShell:

```powershell
Get-Service ssh-agent | Set-Service -StartupType Automatic; Start-Service ssh-agent
```

In a normal PowerShell (enter the passphrase once):

```powershell
ssh-add $env:USERPROFILE\.ssh\id_ed25519
```

The agent keeps the key across reboots. If the key isn't loaded, the deploy fails with "Permission denied" instead of hanging.

## 4. Set up the server

Pick a folder on the server for WARP, for example `/home/YOURUSER/warp`. Upload [start_server.sh](warp_unity/Assets/server_files/start_server.sh) into it (via scp or sftp).

```powershell
scp -P 22 warp_unity\Assets\server_files\start_server.sh USER@HOST:/home/YOURUSER/warp/
```

On the server, start it inside `screen` (or `tmux`) so it keeps running after you disconnect:

```bash
cd /home/YOURUSER/warp
chmod u+x start_server.sh
screen -S warp
./start_server.sh
```

Detach with `Ctrl+A`, then `D`. Reattach later with `screen -r warp`.

The script loops forever. If a `SERVER_LINUX.zip` exists, it unzips it (with `-o`, so old files are overwritten) and deletes the zip. Then it runs `warp_server.x86_64 -batchmode`. When the server exits, it waits 10 s and starts again.

If the script has Windows line endings (CRLF), it fails on Linux. Fix it on the server with `sed -i 's/\r$//' start_server.sh`.

## 5. Configure the deploy

1. Copy `warp_unity/Tools/deploy_config.example.json` to `warp_unity/Tools/deploy_config.json`. It is gitignored and stays on your machine.
2. Fill it in:

```json
{
  "host": "your.server.example",
  "port": 22,
  "user": "warp",
  "keyFile": "C:\\Users\\<you>\\.ssh\\id_ed25519",
  "remoteDir": "/home/YOURUSER/warp"
}
```

| Field | Meaning |
|---|---|
| `host` | Domain or IP of the vserver |
| `port` | ssh port (22 unless you changed it) |
| `user` | Login name on the server |
| `keyFile` | Path to the **private** key. Use double backslashes in JSON. |
| `remoteDir` | Folder on the server that holds `start_server.sh` |

## 6. Build and deploy the server

In the Unity Editor, use the **Warp > Server** menu:

| Item | What it does |
|---|---|
| Build Linux Server | Builds a Linux dedicated server to `Build/Server/warp_server.x86_64` (enabled scenes only) |
| Deploy Last Build | Zips `Build/Server`, uploads it, and restarts the server |
| Build and Deploy | Both of the above in one go |

How a deploy works:
1. `Tools/deploy_server.ps1` zips the build with `tar`.
2. It uploads the zip as `SERVER_LINUX.zip.part`, then renames it to `SERVER_LINUX.zip`. This way `start_server.sh` never sees a half-uploaded file.
3. It kills the running server over ssh.
4. `start_server.sh` sees the zip, unzips it and starts the new build after about 10 s.

You can also run the script by hand:

```powershell
powershell -ExecutionPolicy Bypass -File warp_unity\Tools\deploy_server.ps1 -BuildDir warp_unity\Build\Server
```

**Version numbers:** client and server builds must have the same version number, because the server tells the client its version on connect.

## 7. Build and run the client

- Set the server's IP or domain in the `network_manager` object ("Network Address" field).
- Build for Windows standalone in the Editor.
- Run the build and Guild Wars 2.

## Troubleshooting

| Problem | Likely cause |
|---|---|
| `Host key verification failed` during deploy | You never connected to the server from this PC. Run `ssh USER@HOST` once and type `yes` (see 3.2) |
| `Permission denied (publickey)` during deploy | Public key isn't in `authorized_keys`, or the key has a passphrase and isn't loaded (`ssh-add`, see 3.3) |
| `Missing ...deploy_config.json` | Do step 5 |
| `No warp_server.x86_64 in ...` | Run **Build Linux Server** first |
| Build fails with a missing module error | Install Linux Dedicated Server Build Support in Unity Hub |
| Server doesn't restart after deploy | `start_server.sh` isn't running on the server (check `screen -r warp`) |
| `server was not running` in the deploy output | Harmless. The new build is picked up the next time the loop starts. |
| `bad interpreter` / `\r` errors on the server | CRLF line endings in `start_server.sh` (see section 4) |
