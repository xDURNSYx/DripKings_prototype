# Run from the Unity project root on Windows. Requires Unity 6000.3.2f1 and Windows build support.
# Example: .\Tools\Verify-Mvp.ps1 -UnityExe "C:\Program Files\Unity\Hub\Editor\6000.3.2f1\Editor\Unity.exe"
[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$UnityExe)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if (!(Test-Path -LiteralPath $UnityExe -PathType Leaf)) { throw "Unity.exe not found: $UnityExe" }
foreach ($folder in @('Assets','Packages','ProjectSettings')) {
    if (!(Test-Path (Join-Path $root $folder))) { throw "Not a Unity project root: missing $folder" }
}
# Refuse to generate scenes or builds over an uncommitted working tree.
# Git must be available; this script is intentionally for a checked-out repository.
$git = Get-Command git -ErrorAction SilentlyContinue
if (-not $git) { throw 'Git is required for the clean-working-tree safety check.' }
& git -C $root rev-parse --is-inside-work-tree *> $null
if ($LASTEXITCODE -ne 0) { throw 'Project is not inside a Git working tree.' }
$changes = @(& git -C $root status --porcelain --untracked-files=normal)
if ($LASTEXITCODE -ne 0) { throw 'Could not inspect Git working tree.' }
if ($changes.Count -gt 0) {
    throw 'Working tree has modified or untracked files. Commit/stash them or use a clean worktree before running verification.'
}
$logs = Join-Path $root ('MvpVerification_' + (Get-Date -Format 'yyyyMMdd_HHmmss'))
New-Item -ItemType Directory -Path $logs -ErrorAction Stop | Out-Null
function Invoke-UnityStep([string]$name,[string]$method,[string]$marker) {
    $log = Join-Path $logs ($name + '.log')
    & $UnityExe -batchmode -nographics -quit -projectPath $root -executeMethod $method -logFile $log
    $exit = $LASTEXITCODE
    if ($exit -ne 0) { throw "$name failed (exit $exit). Inspect $log" }
    if (!(Test-Path -LiteralPath $log)) { throw "$name produced no log: $log" }
    if ($marker -and !(Select-String -Path $log -Pattern $marker -SimpleMatch -Quiet)) {
        throw "$name did not log required marker '$marker'. Inspect $log"
    }
    Write-Host "$name: exit 0; log $log"
}
Push-Location $root
try {
    Invoke-UnityStep 'smoke' 'DripKings.EditorTools.MvpSmokeTests.Run' 'DRIP_KINGS_MVP_SMOKE_PASS: 6 assertions passed'
    $scene = Join-Path $root 'Assets\DripKings_MVP_Test.unity'
    if (!(Test-Path -LiteralPath $scene)) {
        Invoke-UnityStep 'scene' 'DripKings.EditorTools.CreateMvpTestScene.Create' 'Drip Kings test scene saved'
        if (!(Test-Path -LiteralPath $scene)) { throw "Scene generation returned success but scene is missing" }
    } else {
        Write-Host 'Existing MVP scene preserved; inspect it manually before trusting the build.'
    }
    if (Test-Path (Join-Path $root 'Builds\MVP')) {
        throw 'Builds\MVP exists. Preserve or rename it before running this build.'
    }
    Invoke-UnityStep 'build' 'DripKings.EditorTools.MvpWindowsBuild.Build' 'DRIP_KINGS_MVP_BUILD_PASS'
    if (!(Test-Path (Join-Path $root 'Builds\MVP\DripKings.exe'))) {
        throw 'Build reported success but DripKings.exe is missing'
    }
    Write-Host 'Automated checks and build completed. Manual driving, rewards, persistence and restart remain unverified.'
} finally { Pop-Location; Write-Host "Logs: $logs" }
