#Requires -Version 5.1
<#
.SYNOPSIS
    Reorganizes Ragnar.UnitTests into a consistent folder layout
    that mirrors the production source structure.

.DESCRIPTION
    Moves test files into category-based subfolders under Ragnar.UnitTests\
    and removes any now-empty root-level test .cs files.
    Works with SDK-style .csproj (auto-globbing), so no project file edits needed.

.EXAMPLE
    .\ReorganizeTests.ps1 -DryRun        # preview moves without touching disk
    .\ReorganizeTests.ps1               # actually move the files
#>

[CmdletBinding(SupportsShouldProcess)]
param(
    [switch]$DryRun
)

# ─── Paths ────────────────────────────────────────────────────────────────────
$root = Split-Path -Parent $MyInvocation.MyCommand.Path   # repo root
$testProject = Join-Path $root "Ragnar.UnitTests"

if (-not (Test-Path $testProject)) {
    Write-Error "Test project not found at $testProject"
    exit 1
}

Write-Host "`n=== Ragnar.UnitTests – Folder Reorganisation ===" -ForegroundColor Cyan
Write-Host "Target : $testProject`n" -ForegroundColor Cyan

# ─── Desired layout ───────────────────────────────────────────────────────────
# Maps a flat or misplaced .cs file (relative to test project) → new subfolder
#
#  Current (flat or scattered)          →  Target subfolder
#  ─────────────────────────────────     ────────────────
#  ResponseWriterTests.cs               →  Output\
#  StopwatchExtensionsTests.cs          →  Extensions\
#  Stages\AskQuestionsStageTests.cs     →  Stages\        (already correct)
#  Parsing\CsvRecordParserTests.cs      →  Parsing\       (already correct)
#  Validation\OllamaOptionsValidatorTests.cs → Validation\ (already correct)
#  VectorStore\VectorStoreBuilderTests.cs   → VectorStore\ (already correct)
#  Extensions\StringExtensionsTests.cs  →  Extensions\    (already correct)
#  Services\OllamaClientFactoryTests.cs →  Services\      (already correct)
#
# The two files that need moving are at the project root.

$moves = @(
    # source (relative to $testProject)         destination (relative to $testProject)
    [PSCustomObject]@{ Source = "ResponseWriterTests.cs";              Destination = "Output\ResponseWriterTests.cs" }
    [PSCustomObject]@{ Source = "StopwatchExtensionsTests.cs";         Destination = "Extensions\StopwatchExtensionsTests.cs" }
)

# ─── Ensure target folders exist ──────────────────────────────────────────────
$foldersToCreate = @("Output", "Extensions", "Stages", "Parsing", "Validation", "VectorStore", "Services")
foreach ($f in $foldersToCreate) {
    $dir = Join-Path $testProject $f
    if (-not (Test-Path $dir)) {
        if ($DryRun) {
            Write-Host "  [DRY] Would create folder : $dir" -ForegroundColor Yellow
        } else {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
            Write-Host "  Created folder            : $f" -ForegroundColor Green
        }
    }
}

# ─── Perform moves ────────────────────────────────────────────────────────────
foreach ($m in $moves) {
    $src = Join-Path $testProject $m.Source
    $dst = Join-Path $testProject $m.Destination

    if (-not (Test-Path $src)) {
        Write-Host "  SKIP  (not found)           : $($m.Source)" -ForegroundColor DarkGray
        continue
    }

    # If already in the right place, skip
    if ((Test-Path $dst)) {
        Write-Host "  SKIP  (already in place)  : $($m.Destination)" -ForegroundColor DarkGray
        continue
    }

    if ($DryRun) {
        Write-Host "  [DRY] $($m.Source)  →  $($m.Destination)" -ForegroundColor Yellow
    } else {
        Move-Item -Path $src -Destination $dst -Force
        Write-Host "  Moved: $($m.Source)  →  $($m.Destination)" -ForegroundColor Green
    }
}

# ─── Clean up: remove any .cs files left at project root (besides .csproj/.props) ─
$rootCsFiles = Get-ChildItem -Path $testProject -Filter "*.cs" -File
if ($rootCsFiles.Count -gt 0) {
    Write-Host "`n  WARNING – .cs files still at project root:" -ForegroundColor Red
    foreach ($f in $rootCsFiles) {
        Write-Host "    $($f.Name)" -ForegroundColor Red
    }
} else {
    Write-Host "`n  ✔ No stray .cs files at project root." -ForegroundColor Green
}

# ─── Final tree ───────────────────────────────────────────────────────────────
Write-Host "`n=== Resulting layout ===" -ForegroundColor Cyan
Get-ChildItem -Path $testProject -Filter "*.cs" -Recurse |
    Sort-Object FullName |
    ForEach-Object {
        $rel = $_.FullName.Substring($testProject.Length + 1)
        Write-Host "  $rel"
    }

Write-Host "`nDone." -ForegroundColor Cyan