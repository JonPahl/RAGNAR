#!/usr/bin/env pwsh
#Requires -Version 7.3
# Reorganise Ragnar solution folders and namespaces.
# Run from the solution root directory.

$ErrorActionPreference = 'Stop'
$Root = $PSScriptRoot          # C:\Users\jp325\source\Ragnar
$Src  = Join-Path $Root 'src'
$Test = Join-Path $Root 'tests'

Write-Host "=== Ragnar reorganisation ===" -ForegroundColor Cyan

# ──────────────────────────────────────────────
# 0. Safety: verify we are in the right place
# ──────────────────────────────────────────────
if (-not (Test-Path (Join-Path $Root 'Ragnar.Abstractions'))) {
    throw "Ragnar.Abstractions not found under '$Root'. Run from the solution root."
}

# ──────────────────────────────────────────────
# 1. Create src/ and tests/ top-level dirs
# ──────────────────────────────────────────────
New-Item -ItemType Directory -Path $Src  -Force | Out-Null
New-Item -ItemType Directory -Path $Test -Force | Out-Null

# ──────────────────────────────────────────────
# 2. Move solution-level projects into src/
# ──────────────────────────────────────────────
$prodProjects = @(
    'Ragnar',
    'Ragnar.Abstractions',
    'Ragnar.Core',
    'Ragnar.Embedding',
    'Ragnar.Questions'
)
foreach ($p in $prodProjects) {
    $from = Join-Path $Root $p
    $to   = Join-Path $Src  $pcls
    if (Test-Path $from) {
        Write-Host "  src/$p" -ForegroundColor DarkGray
        Move-Item $from $to
    }
}

# ──────────────────────────────────────────────
# 3. Move test projects into tests/
# ──────────────────────────────────────────────
$testProjects = @(
    'Tests\Ragnar.UnitTests',
    'Tests\Ragnar.IntegrationTests'
)
foreach ($p in $testProjects) {
    $from = Join-Path $Root $p
    $name = (Split-Path $p -Leaf)
    $to   = Join-Path $Test $name
    if (Test-Path $from) {
        Write-Host "  tests/$name" -ForegroundColor DarkGray
        Move-Item $from $to
    }
}

# ──────────────────────────────────────────────
# 4. Ragnar.Abstractions – group interfaces by domain
# ──────────────────────────────────────────────
$abs = Join-Path $Src 'Ragnar.Abstractions'

# Create domain folders
$absIo  = Join-Path $abs 'IO'
$absEmb = Join-Path $abs 'Embedding'
$absRend = Join-Path $abs 'Rendering'
foreach ($d in @($absIo, $absEmb, $absRend)) {
    New-Item -ItemType Directory -Path $d -Force | Out-Null
}

# IO interfaces
$ioInterfaces = @(
    'IFileParser.cs',
    'IOutputFormatter.cs',
    'IOutputWriter.cs',
    'IPathResolver.cs',
    'IQuestionSource.cs',
    'IRecordParser.cs',
    'IWriter.cs'
)
foreach ($f in $ioInterfaces) {
    # Search both root and Contracts/
    $candidates = @(
        (Join-Path $abs $f),
        (Join-Path $abs "Contracts\$f")
    ) | Where-Object { Test-Path $_ }
    foreach ($c in $candidates) {
        Move-Item $c (Join-Path $absIo $f) -Force
        Write-Host "  Abstractions/IO/$f"
    }
}

# Embedding interfaces
$embInterfaces = @(
    'IOllamaClientFactory.cs',
    'IQdrantClient.cs',
    'IQdrantPointFactory.cs',
    'IVectorSearchService.cs'
)
foreach ($f in $embInterfaces) {
    $candidates = @(
        (Join-Path $abs $f),
        (Join-Path $abs "Contracts\$f")
    ) | Where-Object { Test-Path $_ }
    foreach ($c in $candidates) {
        Move-Item $c (Join-Path $absEmb $f) -Force
        Write-Host "  Abstractions/Embedding/$f"
    }
}

# Rendering interfaces
$rendInterfaces = @(
    'IChatPromptProvider.cs',
    'ITableBuilder.cs',
    'ITableRenderer.cs'
)
foreach ($f in $rendInterfaces) {
    $candidates = @(
        (Join-Path $abs $f),
        (Join-Path $abs "Contracts\$f")
    ) | Where-Object { Test-Path $_ }
    foreach ($c in $candidates) {
        Move-Item $c (Join-Path $absRend $f) -Force
        Write-Host "  Abstractions/Rendering/$f"
    }
}

# Remove empty Contracts folder if it exists
$contracts = Join-Path $abs 'Contracts'
if ((Test-Path $contracts) -and (-not (Get-ChildItem $contracts -File))) {
    Remove-Item $contracts -Force
    Write-Host "  Removed empty Abstractions/Contracts/"
}

# ──────────────────────────────────────────────
# 5. Ragnar.Core – tidy sub-folders
# ──────────────────────────────────────────────
$core = Join-Path $Src 'Ragnar.Core'

# Ensure Models/ holds domain POCOs
$coreModels = Join-Path $core 'Models'
New-Item -ItemType Directory -Path $coreModels -Force | Out-Null

# Move QuestionExtensions.cs into Models/ (it's a model extension)
$qExt = Join-Path $core 'Models\QuestionExtensions.cs'
if (-not (Test-Path $qExt)) {
    $found = Get-ChildItem $core -Recurse -Filter 'QuestionExtensions.cs' | Select-Object -First 1
    if ($found) { Move-Item $found.FullName $qExt -Force; Write-Host "  Core/Models/QuestionExtensions.cs" }
}

# Move EmbeddingOptions.cs to Options/ (if it's in Models/)
$embOpt = Join-Path $core 'Options\EmbeddingOptions.cs'
$embOptModel = Join-Path $core 'Models\EmbeddingOptions.cs'
if ((Test-Path $embOptModel) -and (-not (Test-Path $embOpt))) {
    Move-Item $embOptModel $embOpt -Force
    Write-Host "  Core/Options/EmbeddingOptions.cs"
}

# ──────────────────────────────────────────────
# 6. Ragnar.Embedding – separate VectorStore from Parsers
# ──────────────────────────────────────────────
$emb = Join-Path $Src 'Ragnar.Embedding'
$embVec = Join-Path $emb 'VectorStore'
$embCli = Join-Path $emb 'Clients'
New-Item -ItemType Directory -Path $embVec -Force | Out-Null
New-Item -ItemType Directory -Path $embCli -Force | Out-Null

# PointStructFactory → VectorStore/
$psf = Join-Path $emb 'Parsers\PointStructFactory.cs'
if (Test-Path $psf) {
    Move-Item $psf (Join-Path $embVec 'PointStructFactory.cs') -Force
    Write-Host "  Embedding/VectorStore/PointStructFactory.cs"
}

# OllamaClientFactory → Clients/ (if present at root)
$ocf = Get-ChildItem $emb -Recurse -Filter 'OllamaClientFactory.cs' | Select-Object -First 1
if ($ocf -and ($ocf.DirectoryName -ne $embCli)) {
    Move-Item $ocf.FullName (Join-Path $embCli 'OllamaClientFactory.cs') -Force
    Write-Host "  Embedding/Clients/OllamaClientFactory.cs"
}

# ──────────────────────────────────────────────
# 7. Ragnar.Questions – rename Questions/ → Sources/
# ──────────────────────────────────────────────
$q = Join-Path $Src 'Ragnar.Questions'
$qSrc = Join-Path $q 'Sources'
New-Item -ItemType Directory -Path $qSrc -Force | Out-Null

$qOld = Join-Path $q 'Questions'
if (Test-Path $qOld) {
    Get-ChildItem $qOld -File | ForEach-Object {
        Move-Item $_.FullName (Join-Path $qSrc $_.Name) -Force
        Write-Host "  Questions/Sources/$($_.Name)"
    }
    Remove-Item $qOld -Force
}

# ──────────────────────────────────────────────
# 8. Ragnar (main app) – de-dupe, reorganise
# ──────────────────────────────────────────────
$main = Join-Path $Src 'Ragnar'

# 8a. Remove duplicate AssemblyExtensions.cs
$asmFiles = Get-ChildItem $main -Recurse -Filter 'AssemblyExtensions.cs'
if ($asmFiles.Count -gt 1) {
    $keep = $asmFiles[0]   # keep first one found
    $asmFiles | Select-Object -Skip 1 | ForEach-Object {
        Remove-Item $_.FullName -Force
        Write-Host "  REMOVED duplicate: $($_.FullName)"
    }
    # Normalise: ensure it's in Branding/
    $target = Join-Path $main 'Branding\AssemblyExtensions.cs'
    if ((Resolve-Path $keep.FullName) -ne (Resolve-Path $target -ErrorAction SilentlyContinue)) {
        Move-Item $keep.FullName $target -Force
    }
    Write-Host "  Branding/AssemblyExtensions.cs (single copy)"
}

# 8b. SummarizePromptProvider → Prompts/
$prompts = Join-Path $main 'Prompts'
New-Item -ItemType Directory -Path $prompts -Force | Out-Null
$summarize = Join-Path $main 'Services\SummarizePromptProvider.cs'
if (Test-Path $summarize) {
    Move-Item $summarize (Join-Path $prompts 'SummarizePromptProvider.cs') -Force
    Write-Host "  Prompts/SummarizePromptProvider.cs"
}

# 8c. Ensure Stages sub-folders exist
$stagesEmb = Join-Path $main 'Stages\Embedding'
$stagesQ   = Join-Path $main 'Stages\Questions'
New-Item -ItemType Directory -Path $stagesEmb -Force | Out-Null
New-Item -ItemType Directory -Path $stagesQ   -Force | Out-Null

# Move ParsingStage into Stages/Embedding/
$ps = Join-Path $main 'Stages\ParsingStage.cs'
if (Test-Path $ps) {
    Move-Item $ps (Join-Path $stagesEmb 'ParsingStage.cs') -Force
    Write-Host "  Stages/Embedding/ParsingStage.cs"
}

# 8d. Remove empty Services/ if it exists
$srv = Join-Path $main 'Services'
if ((Test-Path $srv) -and (-not (Get-ChildItem $srv -File))) {
    Remove-Item $srv -Force
}

# ──────────────────────────────────────────────
# 9. Unit Tests – mirror production structure
# ──────────────────────────────────────────────
$ut = Join-Path $Test 'Ragnar.UnitTests'

# Create mirror folders
$utFolders = @(
    'Core\Extensions',
    'Core\Options',
    'Core\Rendering',
    'Embedding\Parsers',
    'Embedding\VectorStore',
    'Questions\Filters',
    'Questions\Sources',
    'Output',
    'Stages\Embedding',
    'Stages\Questions'
)
foreach ($f in $utFolders) {
    New-Item -ItemType Directory -Path (Join-Path $ut $f) -Force | Out-Null
}

# Move specific test files
$testMoves = @(
    @{ From = 'XmlCommentFilterStrategyTests.cs'; To = 'Questions\Filters\XmlCommentFilterStrategyTests.cs' }
    @{ From = 'Core\Extensions\StringExtensionsTests.cs'; To = 'Core\Extensions\StringExtensionsTests.cs' }
    @{ From = 'ApplicationOptionsValidationTests.cs'; To = 'Core\Options\ApplicationOptionsValidationTests.cs' }
    @{ From = 'Providers\CsvFileQuestionProviderTests.cs'; To = 'Questions\Sources\CsvQuestionSourceTests.cs' }
    @{ From = 'Embedding\VectorStore\VectorStoreBuilderTests.cs'; To = 'Embedding\VectorStore\VectorStoreBuilderTests.cs' }
)
foreach ($m in $testMoves) {
    $from = Join-Path $ut $m.From
    $to   = Join-Path $ut $m.To
    if ((Test-Path $from) -and (-not (Test-Path $to))) {
        Move-Item $from $to -Force
        Write-Host "  UnitTests/$($m.To)"
    }
}

# Remove empty legacy folders
foreach ($legacy in @('Providers')) {
    $lp = Join-Path $ut $legacy
    if ((Test-Path $lp) -and (-not (Get-ChildItem $lp -File))) {
        Remove-Item $lp -Force
    }
}

# ──────────────────────────────────────────────
# 10. Integration Tests – tidy
# ──────────────────────────────────────────────
$it = Join-Path $Test 'Ragnar.IntegrationTests'
$itOut = Join-Path $it 'Output'
New-Item -ItemType Directory -Path $itOut -Force | Out-Null
$rwt = Join-Path $it 'ResponseWriterIntegrationTests.cs'
if ((Test-Path $rwt) -and (-not (Test-Path (Join-Path $itOut 'ResponseWriterIntegrationTests.cs')))) {
    Move-Item $rwt (Join-Path $itOut 'ResponseWriterIntegrationTests.cs') -Force
    Write-Host "  IntegrationTests/Output/ResponseWriterIntegrationTests.cs"
}

# ──────────────────────────────────────────────
# 11. Update namespaces in .cs files (bulk sed)
# ──────────────────────────────────────────────
$namespaceMap = [ordered]@{
    'Ragnar.Abstractions'            = 'Ragnar.Abstractions'          # base unchanged
    'Ragnar.Abstractions.Contracts'  = 'Ragnar.Abstractions.IO'       # contracts → domain
    'Ragnar.Core.Text'               = 'Ragnar.Core.Text'
    'Ragnar.Core.Validation'         = 'Ragnar.Core.Validation'
    'Ragnar.Core.Rendering'          = 'Ragnar.Core.Rendering'
    'Ragnar.Core.Models'             = 'Ragnar.Core.Models'
    'Ragnar.Core.Options'            = 'Ragnar.Core.Options'
    'Ragnar.Embedding.Parsers'       = 'Ragnar.Embedding.Parsers'
    'Ragnar.Questions.Questions'     = 'Ragnar.Questions.Sources'
    'Ragnar.Services'                = 'Ragnar.Prompts'
}

# Update namespace declarations in moved files
$allCs = Get-ChildItem $Src -Recurse -Filter '*.cs'
foreach ($file in $allCs) {
    $content = Get-Content $file.FullName -Raw
    $newContent = $content

    # Fix Contracts → IO (only for files now in IO/)
    if ($file.FullName -match '\\IO\\' -or $file.FullName -match '/IO/') {
        $newContent = $newContent -replace
            'namespace Ragnar\.Abstractions\.Contracts',
            'namespace Ragnar.Abstractions.IO'
    }
    # Fix Questions → Sources
    if ($file.FullName -match '\\Sources\\' -or $file.FullName -match '/Sources/') {
        $newContent = $newContent -replace
            'namespace Ragnar\.Questions\.Questions',
            'namespace Ragnar.Questions.Sources'
    }
    # Fix Services → Prompts
    if ($file.FullName -match '\\Prompts\\' -or $file.FullName -match '/Prompts/') {
        $newContent = $newContent -replace
            'namespace Ragnar\.Services',
            'namespace Ragnar.Prompts'
    }
    # Fix VectorStore
    if ($file.FullName -match '\\VectorStore\\' -or $file.FullName -match '/VectorStore/') {
        $newContent = $newContent -replace
            'namespace Ragnar\.Embedding\.Parsers\b(?!\.)',
            'namespace Ragnar.Embedding.VectorStore'
    }

    if ($newContent -ne $content) {
        Set-Content $file.FullName -Value $newContent -NoNewline
        Write-Host "  ns updated: $($file.FullName)"
    }
}

# Also fix in test projects
$testCs = Get-ChildItem $Test -Recurse -Filter '*.cs'
foreach ($file in $testCs) {
    $content = Get-Content $file.FullName -Raw
    $newContent = $content -replace
        'namespace Ragnar\.Questions\.Questions',
        'namespace Ragnar.Questions.Sources'
    $newContent = $newContent -replace
        'namespace Ragnar\.Services\b',
        'namespace Ragnar.Prompts'
    if ($newContent -ne $content) {
        Set-Content $file.FullName -Value $newContent -NoNewline
        Write-Host "  test ns updated: $($file.FullName)"
    }
}

# ──────────────────────────────────────────────
# 12. Update .csproj ProjectReference paths
# ──────────────────────────────────────────────
$allCsproj = Get-ChildItem $Src, $Test -Recurse -Filter '*.csproj'
foreach ($proj in $allCsproj) {
    $content = Get-Content $proj.FullName -Raw
    $newContent = $content

    # Fix relative paths: ../../Ragnar.Abstractions → ../../src/Ragnar.Abstractions
    # (adjust based on actual relative depth)
    $newContent = $newContent -replace
        'Include="\.\.\\?\.\.\\?Ragnar\.Abstractions"',
        'Include="../../src/Ragnar.Abstractions"'
    $newContent = $newContent -replace
        'Include="\.\.\\?\.\.\\?Ragnar\.Core"',
        'Include="../../src/Ragnar.Core"'
    $newContent = $newContent -replace
        'Include="\.\.\\?\.\.\\?Ragnar\.Embedding"',
        'Include="../../src/Ragnar.Embedding"'
    $newContent = $newContent -replace
        'Include="\.\.\\?\.\.\\?Ragnar\.Questions"',
        'Include="../../src/Ragnar.Questions"'
    $newContent = $newContent -replace
        'Include="\.\.\\?\.\.\\?Ragnar\\?"',
        'Include="../../src/Ragnar"'

    if ($newContent -ne $content) {
        Set-Content $proj.FullName -Value $newContent -NoNewline
        Write-Host "  refs updated: $($proj.Name)"
    }
}

# ──────────────────────────────────────────────
# 13. Update .sln project paths (if .sln exists)
# ──────────────────────────────────────────────
$sln = Get-ChildItem $Root -Filter '*.sln' | Select-Object -First 1
if ($sln) {
    $content = Get-Content $sln.FullName -Raw
    $newContent = $content
    # Example: Ragnar.Abstractions\ → src\Ragnar.Abstractions\
    $newContent = $newContent -replace 'Ragnar\.Abstractions\\', 'src\Ragnar.Abstractions\'
    $newContent = $newContent -replace 'Ragnar\.Core\\',           'src\Ragnar.Core\'
    $newContent = $newContent -replace 'Ragnar\.Embedding\\',      'src\Ragnar.Embedding\'
    $newContent = $newContent -replace 'Ragnar\.Questions\\',      'src\Ragnar.Questions\'
    $newContent = $newContent -replace '(?<![\w.])Ragnar\\',       'src\Ragnar\'
    $newContent = $newContent -replace 'Tests\\Ragnar\.UnitTests\\',         'tests\Ragnar.UnitTests\'
    $newContent = $newContent -replace 'Tests\\Ragnar\.IntegrationTests\\',  'tests\Ragnar.IntegrationTests\'
    if ($newContent -ne $content) {
        Set-Content $sln.FullName -Value $newContent -NoNewline
        Write-Host "  sln updated: $($sln.Name)"
    }
}

# ──────────────────────────────────────────────
# 14. Build & verify
# ──────────────────────────────────────────────
Write-Host "`n=== Building solution ===" -ForegroundColor Cyan
dotnet build $sln.FullName 2>&1 | Tee-Object -FilePath (Join-Path $Root 'build-reorg.log')

Write-Host "`n=== Reorganisation complete ===" -ForegroundColor Green
Write-Host "Review build-reorg.log for any remaining errors."