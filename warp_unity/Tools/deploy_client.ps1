# Zips a built client folder (WARP) and optionally uploads it via scp to the web server.
# Needs the OpenSSH client (ships with Windows 10/11) and key-based login for -Upload.
# Settings come from Tools/deploy_config.json (copy deploy_config.example.json).
param(
    [Parameter(Mandatory)][string]$BuildDir,
    [Parameter(Mandatory)][string]$ZipPath,
    [switch]$Upload
)

$ErrorActionPreference = 'Stop'

$BuildDir = $BuildDir.TrimEnd('\', '/')
if (-not (Test-Path (Join-Path $BuildDir 'warp.exe'))) {
    throw "No warp.exe in $BuildDir - build the client first."
}

Write-Host "Zipping $BuildDir ..."
if (Test-Path $ZipPath) { Remove-Item $ZipPath }
# The patcher extracts into its own folder and expects a top-level "WARP" folder inside the zip.
# tar.exe (bsdtar) writes zips with forward slashes, unlike Compress-Archive in Windows PowerShell 5.1
tar.exe -a -c -f $ZipPath --exclude '*DoNotShip*' -C (Split-Path $BuildDir -Parent) (Split-Path $BuildDir -Leaf)
if ($LASTEXITCODE -ne 0) { throw 'zip failed' }
$sizeMb = [math]::Round((Get-Item $ZipPath).Length / 1MB, 1)
Write-Host "Created $ZipPath ($sizeMb MB)"

if (-not $Upload) { return }

$configPath = Join-Path $PSScriptRoot 'deploy_config.json'
if (-not (Test-Path $configPath)) {
    throw "Missing $configPath - copy deploy_config.example.json to deploy_config.json and fill it in."
}
$cfg = Get-Content $configPath -Raw | ConvertFrom-Json

# BatchMode: never prompt (Unity has no console); fails fast if the key needs a passphrase and isn't in ssh-agent.
$sshArgs = @('-o', 'BatchMode=yes', '-p', $cfg.port)
$scpArgs = @('-o', 'BatchMode=yes', '-P', $cfg.port)
if ($cfg.keyFile) {
    $sshArgs += @('-i', $cfg.keyFile)
    $scpArgs += @('-i', $cfg.keyFile)
}
$target = "$($cfg.user)@$($cfg.host)"
$remoteZip = if ($cfg.clientRemotePath) { $cfg.clientRemotePath } else { '/var/www/vhosts/gw2warp.com/httpdocs/data/recore/WARP.zip' }

Write-Host "Uploading $sizeMb MB to ${target}:$remoteZip ..."
# Upload under a temp name and rename, so nobody downloads a half-uploaded zip.
scp @scpArgs $ZipPath "${target}:$remoteZip.part"
if ($LASTEXITCODE -ne 0) { throw 'upload failed' }
ssh @sshArgs $target "mv '$remoteZip.part' '$remoteZip'"
if ($LASTEXITCODE -ne 0) { throw 'remote rename failed' }

Write-Host 'Done. The new client is live.'
