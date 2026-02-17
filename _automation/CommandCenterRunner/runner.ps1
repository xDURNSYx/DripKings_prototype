param(
  [string]$BaseUrl = "http://3.134.181.62:8080",
  [int]$UnityTimeoutSec = 3600
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function New-RunId {
  (Get-Date -Format "yyyyMMdd_HHmmss") + "_" + ([Guid]::NewGuid().ToString("N").Substring(0,8))
}

function Write-Log([string]$msg) {
  $ts = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
  Write-Host "[$ts] $msg"
}

function Get-AuthHeader {
  $tok = $env:COMMAND_CENTER_TOKEN
  if ([string]::IsNullOrWhiteSpace($tok)) { throw "COMMAND_CENTER_TOKEN env var is missing or empty." }
  $tok = $tok.Trim()
  return @{ "Authorization" = ("Bearer " + $tok) }
}

function Safe-Json($obj) {
  $obj | ConvertTo-Json -Depth 10 -Compress
}

function Read-TextTail([string]$path, [int]$maxLines = 200) {
  if (!(Test-Path $path)) { return $null }
  try { (Get-Content -Path $path -Tail $maxLines -ErrorAction Stop) -join "`n" } catch { $null }
}

function Invoke-ApiGet([string]$url) {
  try {
    return Invoke-RestMethod -Method GET -Uri $url -Headers (Get-AuthHeader) -TimeoutSec 30
  } catch {
    if ($_.Exception.Response -and $_.Exception.Response.StatusCode.value__ -eq 204) { return $null }
    throw
  }
}

function Invoke-ApiPostJson([string]$url, $bodyObj) {
  $headers = Get-AuthHeader
  $headers["Content-Type"] = "application/json"
  $json = ($bodyObj | ConvertTo-Json -Depth 10 -Compress)
  return Invoke-RestMethod -Method POST -Uri $url -Headers $headers -Body $json -TimeoutSec 60
}

function Start-UnityBuild([string]$projectPath, [string]$unityExe, [string]$logFile, [string]$buildName) {
  $args = @(
    "-batchmode",
    "-quit",
    "-projectPath", "`"$projectPath`"",
    "-executeMethod", "BuildScript.BuildWindows",
    "-logFile", "`"$logFile`"",
    "-buildName", "`"$buildName`""
  )
  Write-Log ("UNITY CMD: `"" + $unityExe + "`" " + ($args -join " "))
  $p = Start-Process -FilePath $unityExe -ArgumentList ($args -join " ") -PassThru -NoNewWindow
  $sw = [System.Diagnostics.Stopwatch]::StartNew()
  $exited = $false
  try { $exited = $p.WaitForExit($UnityTimeoutSec * 1000) } catch { $exited = $false }
  if (-not $exited) {
    Write-Log ("Unity timed out after " + $UnityTimeoutSec + " seconds. Killing process.")
    try { Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue } catch {}
    Get-Process AssetImportWorker* -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    return @{ timedOut = $true; exitCode = 124; durationSec = [Math]::Round($sw.Elapsed.TotalSeconds, 2) }
  }
  $sw.Stop()
  return @{ timedOut = $false; exitCode = $p.ExitCode; durationSec = [Math]::Round($sw.Elapsed.TotalSeconds, 2) }
}

# MAIN
$runId = New-RunId
$projectPath = "C:\Users\cmhcm\DripKings_prototype"
$unityExe = "C:\Program Files\Unity\Hub\Editor\6000.3.2f1\Editor\Unity.exe"
$artifactRoot = Join-Path $projectPath "_automation\artifacts"
$logDir = Join-Path $artifactRoot "Logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
Write-Log "RUN_ID=$runId"
$health = Invoke-RestMethod -Method GET -Uri "$BaseUrl/healthz" -TimeoutSec 10
Write-Log ("HEALTH: " + (Safe-Json $health))
$job = Invoke-ApiGet "$BaseUrl/jobs/next"
if ($null -eq $job -or (($job -is [string]) -and [string]::IsNullOrWhiteSpace($job))) {
  Write-Log "No job available. Exiting."
  exit 0
}
Write-Log ("JOB: " + (Safe-Json $job))
$jobId = $null
foreach ($k in @("id","job_id","jobId")) {
  if ($job.PSObject.Properties.Name -contains $k) {
    $jobId = [string]$job.$k
    if (-not [string]::IsNullOrWhiteSpace($jobId)) { break }
  }
}
if ([string]::IsNullOrWhiteSpace($jobId)) { throw "Job object has no id field (id/job_id/jobId)." }
$buildName = "DripKings"
if ($job.PSObject.Properties.Name -contains "payload" -and $job.payload) {
  if ($job.payload.PSObject.Properties.Name -contains "buildName" -and $job.payload.buildName) {
    $buildName = [string]$job.payload.buildName
  }
}
$unityLog = Join-Path $logDir ("unity_" + $jobId + "_" + $runId + ".log")
$result = Start-UnityBuild -projectPath $projectPath -unityExe $unityExe -logFile $unityLog -buildName $buildName
$tail = Read-TextTail -path $unityLog -maxLines 200
$status = "completed"
if ($result.timedOut -or $result.exitCode -ne 0) { $status = "failed" }
$body = @{ status=$status; runId=$runId; unity=@{ exe=$unityExe; timeoutSec=$UnityTimeoutSec; logFile=$unityLog; exitCode=$result.exitCode; timedOut=$result.timedOut; durationSec=$result.durationSec }; logTail=$tail }
Write-Log "POST /jobs/$jobId/complete"
Invoke-ApiPostJson "$BaseUrl/jobs/$jobId/complete" $body | Out-Null
if ($status -ne "completed") { Write-Log ("JOB FAILED (exitCode=" + $result.exitCode + ", timedOut=" + $result.timedOut + ")"); exit 1 }
Write-Log "JOB COMPLETED OK"
exit 0
