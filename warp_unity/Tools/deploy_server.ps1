# Zips a built Linux server, uploads it via scp and restarts it on the vserver.
# Needs the OpenSSH client (ships with Windows 10/11) and key-based login.
# Settings come from Tools/deploy_config.json (copy deploy_config.example.json).
param(
    [Parameter(Mandatory)][string]$BuildDir
)

$ErrorActionPreference = 'Stop'

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
$zip = Join-Path ([IO.Path]::GetTempPath()) 'SERVER_LINUX.zip'

if (-not (Test-Path (Join-Path $BuildDir 'warp_server.x86_64'))) {
    throw "No warp_server.x86_64 in $BuildDir - build the Linux server first."
}

Write-Host "Zipping $BuildDir ..."
if (Test-Path $zip) { Remove-Item $zip }
# tar.exe (bsdtar) writes zips with forward slashes, unlike Compress-Archive in Windows PowerShell 5.1
tar.exe -a -c -f $zip --exclude '*DoNotShip*' -C $BuildDir .
if ($LASTEXITCODE -ne 0) { throw 'zip failed' }

Write-Host "Uploading $([math]::Round((Get-Item $zip).Length / 1MB, 1)) MB to $target ..."
# Upload under a temp name and rename, so start_server.sh never sees a half-uploaded zip.
scp @scpArgs $zip "${target}:$($cfg.remoteDir)/SERVER_LINUX.zip.part"
if ($LASTEXITCODE -ne 0) { throw 'upload failed' }

Write-Host 'Restarting server ...'
# start_server.sh unzips SERVER_LINUX.zip whenever the server process ends, so killing it is enough.
# The [w] keeps pkill from matching this very command line.
$remote = "cd '$($cfg.remoteDir)' && mv SERVER_LINUX.zip.part SERVER_LINUX.zip && (pkill -f '[w]arp_server.x86_64' || echo 'server was not running')"
ssh @sshArgs $target $remote
if ($LASTEXITCODE -ne 0) { throw 'remote step failed' }

Remove-Item $zip
Write-Host 'Done. The server restarts in ~10 s with the new build.'
