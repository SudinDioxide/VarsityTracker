# PowerShell script to package the project for assignment submission
# Cleans bin, obj, and temp directories to keep the zip file lightweight and fast to extract.

$ProgressPreference = 'SilentlyContinue'

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($projectRoot)) {
    $projectRoot = (Get-Location).Path
}

$targetZip = Join-Path $projectRoot "Group1_VarsityTracker_Submission.zip"
Write-Host "Preparing submission package: $targetZip" -ForegroundColor Cyan

# Remove existing zip if present
if (Test-Path $targetZip) {
    Remove-Item $targetZip -Force
}

# Directories to exclude
$excludeDirs = @("bin", "obj", ".vs", ".git", ".vscode", "TestResults")

$tempDir = Join-Path $env:TEMP "VarsityTracker_Package"
if (Test-Path $tempDir) {
    Remove-Item $tempDir -Recurse -Force
}
New-Item -ItemType Directory -Path $tempDir | Out-Null

Write-Host "Copying project source files..." -ForegroundColor Yellow

$sourceFiles = Get-ChildItem -Path $projectRoot -Recurse | Where-Object {
    $item = $_
    $skip = $false
    foreach ($ex in $excludeDirs) {
        if ($item.FullName -match "[\\/]$ex([\\/]|$)") {
            $skip = $true
            break
        }
    }
    if ($item.FullName -eq $targetZip) { $skip = $true }
    -not $skip
}

foreach ($item in $sourceFiles) {
    $relPath = $item.FullName.Substring($projectRoot.Length).TrimStart('\', '/')
    $destPath = Join-Path $tempDir $relPath
    if ($item.PSIsContainer) {
        if (-not (Test-Path $destPath)) {
            New-Item -ItemType Directory -Path $destPath | Out-Null
        }
    } else {
        $parent = Split-Path $destPath
        if (-not (Test-Path $parent)) {
            New-Item -ItemType Directory -Path $parent | Out-Null
        }
        Copy-Item -Path $item.FullName -Destination $destPath -Force
    }
}

Write-Host "Compressing files into $targetZip..." -ForegroundColor Yellow
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $targetZip, [System.IO.Compression.CompressionLevel]::Optimal, $false)

Remove-Item $tempDir -Recurse -Force

if (Test-Path $targetZip) {
    $zipItem = Get-Item $targetZip
    $sizeMB = [math]::Round($zipItem.Length / 1MB, 2)
    Write-Host "Submission package generated successfully!" -ForegroundColor Green
    Write-Host "File: $($zipItem.FullName) ($sizeMB MB)" -ForegroundColor Green
} else {
    Write-Error "Failed to generate $targetZip"
}
