param(
    [string]$Root = $PSScriptRoot,
    [switch]$Detailed
)

$ErrorActionPreference = 'Stop'

$rootPath = (Resolve-Path -LiteralPath $Root).Path

function Get-RelativePath {
    param(
        [string]$BasePath,
        [string]$TargetPath
    )

    if (-not $BasePath.EndsWith([System.IO.Path]::DirectorySeparatorChar)) {
        $BasePath += [System.IO.Path]::DirectorySeparatorChar
    }

    $baseUri = [System.Uri]::new($BasePath)
    $targetUri = [System.Uri]::new($TargetPath)

    return [System.Uri]::UnescapeDataString(
        $baseUri.MakeRelativeUri($targetUri).ToString()
    ).Replace('/', [System.IO.Path]::DirectorySeparatorChar)
}

$excludedDirectories = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

@(
    # source control / editor state
    '.git', '.hg', '.svn',
    '.vs', '.idea', '.cursor', '.vscode',

    # JS / frontend dependencies and build output
    'node_modules', 'bower_components', 'jspm_packages',
    '.pnpm-store', '.yarn',
    '.next', '.nuxt', '.svelte-kit',
    '.turbo', '.vite', '.parcel-cache',
    'dist', 'build', 'out', 'coverage',

    # .NET / test / artifact output
    'bin', 'obj', 'TestResults', 'artifacts',

    # Python / common caches
    '__pycache__',
    '.pytest_cache',
    '.mypy_cache',
    '.ruff_cache',
    '.cache',
    '.venv',
    'venv'
) | ForEach-Object {
    [void]$excludedDirectories.Add($_)
}

$binaryExtensions = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

@(
    # executables / compiled output
    '.exe', '.dll', '.pdb', '.so', '.dylib', '.class', '.jar', '.war', '.ear',
    '.o', '.a', '.lib', '.wasm', '.pyc',

    # archives / packages
    '.zip', '.tar', '.gz', '.tgz', '.bz2', '.xz', '.7z', '.rar',
    '.nupkg', '.snupkg', '.vsix', '.msi',

    # images
    '.png', '.jpg', '.jpeg', '.gif', '.webp', '.ico', '.bmp', '.tif', '.tiff',

    # documents
    '.pdf', '.doc', '.docx', '.xls', '.xlsx', '.ppt', '.pptx',

    # media
    '.mp3', '.mp4', '.mov', '.avi', '.wav', '.flac', '.m4a', '.webm', '.mkv',

    # fonts / databases / cert bundles
    '.ttf', '.otf', '.woff', '.woff2', '.eot',
    '.db', '.sqlite', '.sqlite3',
    '.pfx', '.p12'
) | ForEach-Object {
    [void]$binaryExtensions.Add($_)
}

function Test-ExcludedPath {
    param([System.IO.FileSystemInfo]$Item)

    $relativePath = Get-RelativePath -BasePath $rootPath -TargetPath $Item.FullName
    $segments = $relativePath -split '[\\/]'

    foreach ($segment in $segments) {
        if ($excludedDirectories.Contains($segment)) {
            return $true
        }
    }

    return $false
}

function Test-BinaryFile {
    param([System.IO.FileInfo]$File)

    if ($binaryExtensions.Contains($File.Extension)) {
        return $true
    }

    $stream = $null

    try {
        $stream = [System.IO.File]::Open(
            $File.FullName,
            [System.IO.FileMode]::Open,
            [System.IO.FileAccess]::Read,
            [System.IO.FileShare]::ReadWrite
        )

        $buffer = New-Object byte[] ([Math]::Min(8192, [int]$stream.Length))
        $bytesRead = $stream.Read($buffer, 0, $buffer.Length)

        for ($i = 0; $i -lt $bytesRead; $i++) {
            if ($buffer[$i] -eq 0) {
                return $true
            }
        }

        return $false
    }
    catch {
        return $true
    }
    finally {
        if ($stream) {
            $stream.Dispose()
        }
    }
}

function ConvertTo-ProductName {
    param([string]$Slug)

    switch ($Slug.ToLowerInvariant()) {
        'compliancecore' { return 'ComplianceCore' }
        'customarr' { return 'CustomArr' }
        'loadarr' { return 'LoadArr' }
        'maintainarr' { return 'MaintainArr' }
        'ledgarr' { return 'Ledgarr' }
        'nexarr' { return 'NexArr' }
        'recordarr' { return 'RecordArr' }
        'assurarr' { return 'AssurArr' }
        'routarr' { return 'RoutArr' }
        'staffarr' { return 'StaffArr' }
        'supplyarr' { return 'SupplyArr' }
        'trainarr' { return 'TrainArr' }
        'load' { return 'LoadArr' }
        'fieldcompanion' { return 'FieldCompanion' }
        'suite' { return 'Suite' }
        'stlcompliancesite' { return 'STLComplianceSite' }
        'shared' { return 'Shared' }
        'e2e' { return 'E2E' }
        'dr' { return 'DisasterRecovery' }
        'health' { return 'Health' }
        'openapi' { return 'OpenAPI' }
        'otel' { return 'OpenTelemetry' }
        default {
            if ([string]::IsNullOrWhiteSpace($Slug)) {
                return 'Workspace'
            }

            return (Get-Culture).TextInfo.ToTitleCase($Slug.ToLowerInvariant())
        }
    }
}

function Get-ProductName {
    param([string]$RelativePath)

    $segments = $RelativePath -split '[\\/]'

    if ($segments.Count -ge 2) {
        switch ($segments[0].ToLowerInvariant()) {
            'apps' {
                $slug = $segments[1] -replace '-(api|frontend)$', ''
                return ConvertTo-ProductName -Slug $slug
            }
            'workers' {
                $slug = $segments[1] -replace '-worker$', ''
                return ConvertTo-ProductName -Slug $slug
            }
            'tests' {
                if ($segments[1] -ieq 'e2e-playwright') {
                    return 'E2E'
                }

                if ($segments[1] -ieq 'load-k6') {
                    return 'LoadArr'
                }

                if ($segments[1] -match '^STLCompliance\.([^.]+)') {
                    return ConvertTo-ProductName -Slug $Matches[1]
                }

                return 'Tests'
            }
            'packages' {
                if ($segments[1] -match '^shared') {
                    return 'Shared'
                }

                return ConvertTo-ProductName -Slug $segments[1]
            }
            'root' {
                return 'Workspace'
            }
        }
    }

    return 'Workspace'
}

function Get-TextFiles {
    $pendingDirectories = [System.Collections.Generic.Stack[System.IO.DirectoryInfo]]::new()
    $pendingDirectories.Push([System.IO.DirectoryInfo]::new($rootPath))

    while ($pendingDirectories.Count -gt 0) {
        $directory = $pendingDirectories.Pop()

        foreach ($childDirectory in $directory.EnumerateDirectories()) {
            if (-not (Test-ExcludedPath -Item $childDirectory)) {
                $pendingDirectories.Push($childDirectory)
            }
        }

        foreach ($file in $directory.EnumerateFiles()) {
            if ((-not (Test-ExcludedPath -Item $file)) -and (-not (Test-BinaryFile -File $file))) {
                $file
            }
        }
    }
}

$fileResults = New-Object System.Collections.Generic.List[object]

Get-TextFiles | ForEach-Object {
    $file = $_
    $relativePath = Get-RelativePath -BasePath $rootPath -TargetPath $file.FullName
    $product = Get-ProductName -RelativePath $relativePath

    $totalLines = 0

    try {
        foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
            $totalLines++
        }
    }
    catch {
        return
    }

    $fileResults.Add([pscustomobject]@{
        Product = $product
        Files = 1
        Lines = $totalLines
    })

    if ($Detailed) {
        [pscustomobject]@{
            Product = $product
            Lines = $totalLines
            Path = $relativePath
        }
    }
}

$fileResults |
    Group-Object Product |
    ForEach-Object {
        [pscustomobject]@{
            Product = $_.Name
            Files = $_.Count
            Lines = [int](($_.Group | Measure-Object -Property Lines -Sum).Sum)
        }
    } |
    Sort-Object Lines -Descending |
    Format-Table -AutoSize

Write-Host "Total files: $($fileResults.Count)"
Write-Host "Total lines: $([int](($fileResults | Measure-Object -Property Lines -Sum).Sum))"