<#
.SYNOPSIS
  Create a GitHub Release (and optionally create/push tag and upload assets).

.DESCRIPTION
  - Uses gh CLI if available; otherwise falls back to GitHub REST API via GITHUB_TOKEN.
  - Can auto-create an annotated git tag when missing and push it to origin.
  - Reads release notes from a file (e.g., docs/releases/v1.1.1.md).

.PARAMETER Tag
  The git tag to release (e.g., v1.1.1).

.PARAMETER Title
  Release title. Defaults to "PolarNet <Tag>".

.PARAMETER NotesFile
  Path to Markdown file with release notes. If omitted, tries docs/releases/<Tag>.md.

.PARAMETER Assets
  One or more file paths or glob patterns to upload as release assets (e.g., src/bin/Release/*.nupkg).

.PARAMETER Repo
  Repository in owner/name format. If omitted, derived from `git remote get-url origin`.

.PARAMETER Draft
  Create the release as a draft.

.PARAMETER Prerelease
  Mark the release as a prerelease.

.PARAMETER SkipTagPush
  Do not push the tag to origin (useful when tag already exists upstream).

.EXAMPLE
  ./scripts/publish-github-release.ps1 -Tag v1.1.1 -NotesFile ./docs/releases/v1.1.1.md -Assets "./src/bin/Release/*.nupkg","./src/bin/Release/*.snupkg"

.EXAMPLE
  ./scripts/publish-github-release.ps1 -Tag v1.1.1 -Draft -Prerelease

.NOTES
  - For REST API mode, set GITHUB_TOKEN environment variable with repo:write permissions.
  - PowerShell 7+ recommended.
#>

[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)][string]$Tag,
  [string]$Title,
  [string]$NotesFile,
  [string[]]$Assets,
  [string]$Repo,
  [switch]$Draft,
  [switch]$Prerelease,
  [switch]$SkipTagPush
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Write-Info($msg) { Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Write-Warn($msg) { Write-Host "[WARN] $msg" -ForegroundColor Yellow }
function Write-Err ($msg) { Write-Host "[ERROR] $msg" -ForegroundColor Red }

function Get-RepoFromGitRemote {
  $remoteUrl = (git remote get-url origin) 2>$null
  if (-not $remoteUrl) { throw "Cannot determine git remote 'origin'. Use -Repo owner/name." }
  # Support https and ssh forms
  if ($remoteUrl -match 'github\.com[/:]([^/]+)/([^/.]+)') {
    return "$($Matches[1])/$($Matches[2])"
  }
  throw "Unrecognized remote URL: $remoteUrl. Provide -Repo owner/name."
}

function Ensure-TagExistsAndPush($tag) {
  $existing = git tag --list $tag
  if (-not $existing) {
    Write-Info "Creating annotated tag $tag"
    git tag -a $tag -m "Release $tag"
  } else {
    Write-Info "Tag $tag already exists locally"
  }
  if (-not $SkipTagPush) {
    Write-Info "Pushing tag $tag to origin"
    git push origin $tag
  } else {
    Write-Warn "SkipTagPush set; not pushing tag to origin"
  }
}

function Resolve-Assets([string[]]$patterns) {
  $files = @()
  foreach ($p in ($patterns | Where-Object { $_ -and $_.Trim() -ne '' })) {
    $resolved = Get-ChildItem -Path $p -File -ErrorAction SilentlyContinue
    if ($resolved) { $files += $resolved.FullName }
  }
  return $files | Select-Object -Unique
}

try {
  if (-not $Title) { $Title = "PolarNet $Tag" }

  if (-not $Repo) { $Repo = Get-RepoFromGitRemote }
  Write-Info "Using repository: $Repo"

  # Determine notes file
  if (-not $NotesFile) {
    $candidate = Join-Path (Join-Path (Get-Location) 'docs' ) (Join-Path 'releases' ("$Tag.md"))
    if (Test-Path $candidate) { $NotesFile = $candidate }
  }
  if (-not $NotesFile -or -not (Test-Path $NotesFile)) {
    throw "Release notes file not found. Provide -NotesFile. Tried: $NotesFile"
  }
  $Body = Get-Content -Raw -LiteralPath $NotesFile
  Write-Info "Using notes: $NotesFile"

  # Ensure tag exists and push
  Ensure-TagExistsAndPush -tag $Tag

  $gh = Get-Command gh -ErrorAction SilentlyContinue
  $assetFiles = if ($Assets) { Resolve-Assets $Assets } else { @() }

  if ($gh) {
    Write-Info "gh CLI detected; creating release via gh"
    $args = @('release','create', $Tag,
      '--title', $Title,
      '--notes-file', $NotesFile,
      '--repo', $Repo
    )
    if ($Draft) { $args += '--draft' }
    if ($Prerelease) { $args += '--prerelease' }
    if ($assetFiles.Count -gt 0) { $args += $assetFiles }

    Write-Info ("gh " + ($args -join ' '))
    gh @args | Out-Null
    Write-Info "Release $Tag created via gh."
    return
  }

  Write-Warn "gh CLI not found; falling back to GitHub REST API"
  if (-not $env:GITHUB_TOKEN) { throw "GITHUB_TOKEN not set. Set a token with repo scope." }

  $branch = (git rev-parse --abbrev-ref HEAD).Trim()
  $headers = @{
    'Authorization' = "Bearer $($env:GITHUB_TOKEN)"
    'Accept'        = 'application/vnd.github+json'
    'User-Agent'    = 'PolarNet-Release-Script'
  }
  $releaseBody = @{ tag_name = $Tag; name = $Title; body = $Body; draft = [bool]$Draft; prerelease = [bool]$Prerelease; target_commitish = $branch } | ConvertTo-Json -Depth 5
  $createUri = "https://api.github.com/repos/$Repo/releases"

  Write-Info "Creating release via REST API"
  $resp = Invoke-RestMethod -Method Post -Uri $createUri -Headers $headers -Body $releaseBody -ContentType 'application/json; charset=utf-8'
  $uploadUrlTemplate = $resp.upload_url  # e.g. https://uploads.github.com/.../assets{?name,label}
  if (-not $uploadUrlTemplate) { throw "No upload_url returned from GitHub." }

  if ($assetFiles.Count -gt 0) {
    foreach ($file in $assetFiles) {
      $name = [System.IO.Path]::GetFileName($file)
      $uploadUrl = $uploadUrlTemplate -replace '\{\?name,label\}', "?name=$([Uri]::EscapeDataString($name))"
      Write-Info "Uploading asset: $name"
      Invoke-WebRequest -Method Post -Uri $uploadUrl -Headers $headers -InFile $file -ContentType 'application/octet-stream' | Out-Null
    }
  }

  Write-Info "Release $Tag created via REST API."
}
catch {
  Write-Err $_
  exit 1
}
