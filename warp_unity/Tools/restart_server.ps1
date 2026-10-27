# Restarts the WARP server on the vserver without uploading anything.
# Needs the OpenSSH client (ships with Windows 10/11) and key-based login.
# Settings come from Tools/deploy_config.json (copy deploy_config.example.json).
$ErrorActionPreference = 'Stop'

$configPath = Join-Path $PSScriptRoot 'deploy_config.json'
if (-not (Test-Path $configPath)) {
    throw "Missing $configPath - copy deploy_config.example.json to deploy_config.json and fill it in."
}
$cfg = Get-Content $configPath -Raw | ConvertFrom-Json

# BatchMode: never prompt (Unity has no console); fails fast if the key needs a passphrase and isn't in ssh-agent.
$sshArgs = @('-o', 'BatchMode=yes', '-p', $cfg.port)
if ($cfg.keyFile) {
    $sshArgs += @('-i', $cfg.keyFile)
}
$target = "$($cfg.user)@$($cfg.host)"

Write-Host "Restarting server on $target ..."
# start_server.sh starts the server again ~10 s after the process ends, so killing it is enough.
# The [w] keeps pkill from matching this very command line.
ssh @sshArgs $target "pkill -f '[w]arp_server.x86_64' || echo 'server was not running'"
if ($LASTEXITCODE -ne 0) { throw 'remote step failed' }

Write-Host 'Done. The server is back in ~10 s.'
