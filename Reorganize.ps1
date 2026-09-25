# ─────────────────────────────────────────────
#  Ragnar – Reorganise namespaces & folders
#  Run from: C:\Users\jp325\source\Ragnar
# ─────────────────────────────────────────────
$ErrorActionPreference = 'Stop'
$root   = $PWD
$abst   = Join-Path $root 'Ragnar.Abstractions'
$core   = Join-Path $root 'Ragnar.Core'
$embed  = Join-Path $root 'Ragnar.Embedding'
$main   = Join-Path $root 'Ragnar'
$plugins = Join-Path $root 'Ragnar.Plugins'
$tests  = Join-Path $root 'Ragnar.UnitTests'

function Move-File {
    param([[string]]$src, [[string]]$dst)
    if (-not (Test-Path $src)) { Write-Host "  SKIP (not found): $src" -ForegroundColor DarkYellow; return }
    $dstDir = Split-Path $dst -Parent
    if (-not (Test-Path $dstDir)) { New-Item -ItemType Directory -Path $dstDir -Force | Out-Null }
    if (Test-Path $dst) { Write-Host "  SKIP (exists):    $dst" -ForegroundColor DarkYellow; return }
    Move-Item -Path $src -Destination $dst
    Write-Host "  MOVED:  $src → $dst" -ForegroundColor Green
}

function Fix-Namespace {
    param([[string]]$file, [[string]]$newNs)
    if (-not (Test-Path $file)) { return }
    $content = Get-Content $file -Raw
    # Replace the first "namespace X" line (block-scoped) with the new one
    $content = $content -replace 'namespace\s+[[\w.]]+\s*\{?', "namespace $newNs {"
    # Also handle file-scoped: "namespace X;"
    $content = $content -replace "namespace\s+[[\w.]]+\s*;", "namespace $newNs;"
    [[System.IO.File]]::WriteAllText($file, $content)
    Write-Host "  NS:     $file → $newNs" -ForegroundColor Cyan
}

# ── Phase 2: Interfaces → Abstractions.Contracts ──
Write-Host "`n═══ Phase 2: Move interfaces to Ragnar.Abstractions ═══" -ForegroundColor Yellow
$abstContracts = Join-Path $abst 'Contracts'
if (-not (Test-Path $abstContracts)) { New-Item -ItemType Directory -Path $abstContracts -Force | Out-Null }

Move-File (Join-Path $main 'Contracts\IApplicationHeader.cs') (Join-Path $abstContracts 'IApplicationHeader.cs')
Fix-Namespace (Join-Path $abstContracts 'IApplicationHeader.cs') 'Ragnar.Abstractions.Contracts'

Move-File (Join-Path $main 'Output\IOutputFormatter.cs') (Join-Path $abstContracts 'IOutputFormatter.cs')
Fix-Namespace (Join-Path $abstContracts 'IOutputFormatter.cs') 'Ragnar.Abstractions.Contracts'

Move-File (Join-Path $plugins 'IQuestionSource.cs') (Join-Path $abstContracts 'IQuestionSource.cs')
Fix-Namespace (Join-Path $abstContracts 'IQuestionSource.cs') 'Ragnar.Abstractions.Contracts'

# IPathResolver / IWriter / IResponseWriter – move if they exist as separate files
@(
    'IPathResolver.cs',
    'IWriter.cs',
    'IResponseWriter.cs',
    'IClock.cs',
    'IFileParser.cs',
    'IFileParseFactory.cs',
    'IOutputWriter.cs',
    'IPipelineStage.cs'
) | ForEach-Object {
    $name = $_
    $src  = Join-Path $main "Output\$name"
    if (-not (Test-Path $src)) { $src = Join-Path $main "Contracts\$name" }
    if (-not (Test-Path $src)) { $src = Join-Path $main "$name" }
    Move-File $src (Join-Path $abstContracts $name)
    Fix-Namespace (Join-Path $abstContracts $name) 'Ragnar.Abstractions.Contracts'
}

# ── Phase 3: Models & Validators → Core ──
Write-Host "`n═══ Phase 3: Move models & validators to Ragnar.Core ═══" -ForegroundColor Yellow
$coreModels     = Join-Path $core 'Models'
$coreValidation = Join-Path $core 'Validation'
$coreText       = Join-Path $core 'Text'
@( $coreModels, $coreValidation, $coreText ) | ForEach-Object {
    if (-not (Test-Path $_)) { New-Item -ItemType Directory -Path $_ -Force | Out-Null }
}

Move-File (Join-Path $main 'Models\EmbeddingData.cs') (Join-Path $coreModels 'EmbeddingData.cs')
Fix-Namespace (Join-Path $coreModels 'EmbeddingData.cs') 'Ragnar.Core.Models'

Move-File (Join-Path $main 'Validator\FileLoadOptionsValidator.cs') (Join-Path $coreValidation 'FileLoadOptionsValidator.cs')
Fix-Namespace (Join-Path $coreValidation 'FileLoadOptionsValidator.cs') 'Ragnar.Core.Validation'

Move-File (Join-Path $main 'Validator\OllamaOptionsValidator.cs') (Join-Path $coreValidation 'OllamaOptionsValidator.cs')
Fix-Namespace (Join-Path $coreValidation 'OllamaOptionsValidator.cs') 'Ragnar.Core.Validation'

Move-File (Join-Path $embed 'StringExtensions.cs') (Join-Path $coreText 'StringExtensions.cs')
Fix-Namespace (Join-Path $coreText 'StringExtensions.cs') 'Ragnar.Core.Text'

# ── Phase 4: Clean up Embedding ──
Write-Host "`n═══ Phase 4: Reorganise Ragnar.Embedding ═══" -ForegroundColor Yellow
$embedParsers   = Join-Path $embed 'Parsers'
$embedVector    = Join-Path $embed 'VectorStore'
@( $embedParsers, $embedVector ) | ForEach-Object {
    if (-not (Test-Path $_)) { New-Item -ItemType Directory -Path $_ -Force | Out-Null }
}

Move-File (Join-Path $embed 'UnitOfWork\BaseFileParser.cs') (Join-Path $embedParsers 'BaseFileParser.cs')
Fix-Namespace (Join-Path $embedParsers 'BaseFileParser.cs') 'Ragnar.Embedding.Parsers'

# Move any other parsers found in UnitOfWork
Get-ChildItem (Join-Path $embed 'UnitOfWork') -Filter '*.cs' -ErrorAction SilentlyContinue | ForEach-Object {
    Move-File $_.FullName (Join-Path $embedParsers $_.Name)
    Fix-Namespace (Join-Path $embedParsers $_.Name) 'Ragnar.Embedding.Parsers'
}

Move-File (Join-Path $embed 'VectorStoreBuilder.cs') (Join-Path $embedVector 'VectorStoreBuilder.cs')
Fix-Namespace (Join-Path $embedVector 'VectorStoreBuilder.cs') 'Ragnar.Embedding.VectorStore'

# Remove empty UnitOfWork folder
$uw = Join-Path $embed 'UnitOfWork'
if ((Test-Path $uw) -and -not (Get-ChildItem $uw -ErrorAction SilentlyContinue)) {
    Remove-Item $uw
    Write-Host "  REMOVED empty folder: $uw" -ForegroundColor DarkYellow
}

# ── Phase 5: Main project housekeeping ──
Write-Host "`n═══ Phase 5: Reorganise main Ragnar project ═══" -ForegroundColor Yellow
$mainPipeline = Join-Path $main 'Pipeline'
if (-not (Test-Path $mainPipeline)) { New-Item -ItemType Directory -Path $mainPipeline -Force | Out-Null }

Move-File (Join-Path $main 'PipelineRunner.cs') (Join-Path $mainPipeline 'PipelineRunner.cs')
Fix-Namespace (Join-Path $mainPipeline 'PipelineRunner.cs') 'Ragnar.Pipeline'

# Remove empty Contracts / Validator / Models folders if they're empty
@( 'Contracts', 'Validator', 'Models' ) | ForEach-Object {
    $dir = Join-Path $main $_
    if ((Test-Path $dir) -and -not (Get-ChildItem $dir -ErrorAction SilentlyContinue)) {
        Remove-Item $dir
        Write-Host "  REMOVED empty folder: $dir" -ForegroundColor DarkYellow
    }
}

# ── Phase 6: Reorganise Unit Tests ──
Write-Host "`n═══ Phase 6: Reorganise Ragnar.UnitTests ═══" -ForegroundColor Yellow

# Core/Extensions
$coreTestExt = Join-Path $tests 'Core\Extensions'
if (-not (Test-Path $coreTestExt)) { New-Item -ItemType Directory -Path $coreTestExt -Force | Out-Null }

Move-File (Join-Path $tests 'Extensions\StringExtensionsTests.cs') (Join-Path $coreTestExt 'StringExtensionsTests.cs')
Fix-Namespace (Join-Path $coreTestExt 'StringExtensionsTests.cs') 'Ragnar.UnitTests.Core.Extensions'

Move-File (Join-Path $tests 'StopwatchExtensionsTests.cs') (Join-Path $coreTestExt 'StopwatchExtensionsTests.cs')
Fix-Namespace (Join-Path $coreTestExt 'StopwatchExtensionsTests.cs') 'Ragnar.UnitTests.Core.Extensions'

# Core/Validation
$coreTestVal = Join-Path $tests 'Core\Validation'
if (-not (Test-Path $coreTestVal)) { New-Item -ItemType Directory -Path $coreTestVal -Force | Out-Null }

Move-File (Join-Path $tests 'Validation\OllamaOptionsValidatorTests.cs') (Join-Path $coreTestVal 'OllamaOptionsValidatorTests.cs')
Fix-Namespace (Join-Path $coreTestVal 'OllamaOptionsValidatorTests.cs') 'Ragnar.UnitTests.Core.Validation'

Move-File (Join-Path $tests 'Validation\FileLoadOptionsValidatorTests.cs') (Join-Path $coreTestVal 'FileLoadOptionsValidatorTests.cs')
Fix-Namespace (Join-Path $coreTestVal 'FileLoadOptionsValidatorTests.cs') 'Ragnar.UnitTests.Core.Validation'

# Embedding/Parsers
$embedTestParsers = Join-Path $tests 'Embedding\Parsers'
if (-not (Test-Path $embedTestParsers)) { New-Item -ItemType Directory -Path $embedTestParsers -Force | Out-Null }

Move-File (Join-Path $tests 'Parsing\CsvRecordParserTests.cs') (Join-Path $embedTestParsers 'CsvRecordParserTests.cs')
Fix-Namespace (Join-Path $embedTestParsers 'CsvRecordParserTests.cs') 'Ragnar.UnitTests.Embedding.Parsers'

# Embedding/VectorStore
$embedTestVs = Join-Path $tests 'Embedding\VectorStore'
if (-not (Test-Path $embedTestVs)) { New-Item -ItemType Directory -Path $embedTestVs -Force | Out-Null }

Move-File (Join-Path $tests 'VectorStore\VectorStoreBuilderTests.cs') (Join-Path $embedTestVs 'VectorStoreBuilderTests.cs')
Fix-Namespace (Join-Path $embedTestVs 'VectorStoreBuilderTests.cs') 'Ragnar.UnitTests.Embedding.VectorStore'

# Output
$testOutput = Join-Path $tests 'Output'
if (-not (Test-Path $testOutput)) { New-Item -ItemType Directory -Path $testOutput -Force | Out-Null }

Move-File (Join-Path $tests 'ResponseWriterTests.cs') (Join-Path $testOutput 'ResponseWriterTests.cs')
Fix-Namespace (Join-Path $testOutput 'ResponseWriterTests.cs') 'Ragnar.UnitTests.Output'

# Remove empty test folders
@( 'Extensions', 'Validation', 'Parsing', 'VectorStore' ) | ForEach-Object {
    $dir = Join-Path $tests $_
    if ((Test-Path $dir) -and -not (Get-ChildItem $dir -ErrorAction SilentlyContinue)) {
        Remove-Item $dir
        Write-Host "  REMOVED empty test folder: $dir" -ForegroundColor DarkYellow
    }
}

# ── Done ──
Write-Host "`n═══════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  Reorganisation complete. Next steps:" -ForegroundColor Green
Write-Host "  1. Update <ProjectReference> entries in .csproj files" -ForegroundColor Green
Write-Host "  2. dotnet restore && dotnet build" -ForegroundColor Green
Write-Host "  3. Fix any missing 'using' directives" -ForegroundColor Green
Write-Host "  4. dotnet test" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green
