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
        $BasePath = $BasePath + [System.IO.Path]::DirectorySeparatorChar
    }

    $baseUri = [System.Uri]::new($BasePath)
    $targetUri = [System.Uri]::new($TargetPath)
    $relativeUri = $baseUri.MakeRelativeUri($targetUri)

    return [System.Uri]::UnescapeDataString($relativeUri.ToString()).Replace('/', [System.IO.Path]::DirectorySeparatorChar)
}

$excludedDirectories = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
@(
    '.git', '.github', '.vs', '.idea', '.cursor', '.vscode',
    '.buildcheck', '.e2e-preview-logs', '.next', '.nuxt', '.svelte-kit',
    'node_modules', 'bower_components',
    'bin', 'obj', 'dist', 'build', 'out', 'coverage', 'TestResults',
    'artifacts', 'branding',
    '__pycache__', '.pytest_cache', '.mypy_cache', '.ruff_cache',
    'trainarr-evidence', 'maintainarr-evidence'
) | ForEach-Object { [void]$excludedDirectories.Add($_) }

$includedExtensions = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
@(
    '.cs', '.csproj', '.sln', '.slnx', '.props', '.targets',
    '.ts', '.tsx', '.js', '.jsx', '.mjs', '.cjs',
    '.css', '.scss', '.sass', '.less',
    '.html', '.htm', '.razor',
    '.json', '.jsonc', '.yaml', '.yml', '.xml', '.config',
    '.ps1', '.psm1', '.psd1', '.sh', '.bash', '.cmd', '.bat',
    '.py', '.sql', '.svg',
    '.md', '.mdx', '.txt',
    '.dockerfile'
) | ForEach-Object { [void]$includedExtensions.Add($_) }

$includedExtensionlessFiles = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
@(
    'Dockerfile', 'Containerfile', 'Makefile', 'Procfile', '.env.example',
    '.gitignore', '.dockerignore', '.editorconfig', '.prettierrc', '.eslintrc'
) | ForEach-Object { [void]$includedExtensionlessFiles.Add($_) }

$excludedExtensions = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
@(
    '.dll', '.exe', '.pdb', '.so', '.dylib', '.class', '.jar',
    '.zip', '.tar', '.gz', '.tgz', '.7z', '.rar',
    '.png', '.jpg', '.jpeg', '.gif', '.webp', '.ico',
    '.pdf', '.doc', '.docx', '.xls', '.xlsx', '.ppt', '.pptx',
    '.mp3', '.mp4', '.mov', '.avi', '.wav',
    '.pyc', '.cache', '.lock', '.log'
) | ForEach-Object { [void]$excludedExtensions.Add($_) }

$excludedFileNamePatterns = @(
    '\.g\.(cs|ts|tsx|js|jsx)$',
    '\.generated\.',
    '\.openapi\.json$',
    '\.min\.(css|js)$',
    '\.bundle\.(css|js)$',
    '^package-lock\.json$',
    '^yarn\.lock$',
    '^pnpm-lock\.yaml$',
    '^\.tmp-'
)

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

function Test-SourceFile {
    param([System.IO.FileInfo]$File)

    if (Test-ExcludedPath -Item $File) {
        return $false
    }

    foreach ($pattern in $excludedFileNamePatterns) {
        if ($File.Name -match $pattern) {
            return $false
        }
    }

    $extension = if ($File.Name -ieq 'Dockerfile' -or $File.Name -ieq 'Containerfile') {
        '.dockerfile'
    } else {
        $File.Extension
    }

    if ($excludedExtensions.Contains($extension)) {
        return $false
    }

    if ($includedExtensions.Contains($extension) -or $includedExtensionlessFiles.Contains($File.Name)) {
        return $true
    }

    return $false
}

function Get-SourceFiles {
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
            if (Test-SourceFile -File $file) {
                $file
            }
        }
    }
}

function Test-BinaryFile {
    param([System.IO.FileInfo]$File)

    $stream = $null
    try {
        $stream = [System.IO.File]::Open($File.FullName, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
        $buffer = New-Object byte[] ([Math]::Min(4096, [int]$stream.Length))
        $bytesRead = $stream.Read($buffer, 0, $buffer.Length)

        for ($i = 0; $i -lt $bytesRead; $i++) {
            if ($buffer[$i] -eq 0) {
                return $true
            }
        }

        return $false
    } finally {
        if ($stream) {
            $stream.Dispose()
        }
    }
}


function ConvertTo-ProductName {
    param([string]$Slug)

    switch ($Slug.ToLowerInvariant()) {
        'compliancecore' { return 'ComplianceCore' }
        'loadarr' { return 'LoadArr' }
        'maintainarr' { return 'MaintainArr' }
        'nexarr' { return 'NexArr' }
        'recordarr' { return 'RecordArr' }
        'assurarr' { return 'AssurArr' }
        'routarr' { return 'RoutArr' }
        'staffarr' { return 'StaffArr' }
        'supplyarr' { return 'SupplyArr' }
        'trainarr' { return 'TrainArr' }
        'load' { return 'LoadArr' }
        'fieldcompanion' { return 'fieldcompanion' }
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

$fileResults = New-Object System.Collections.Generic.List[object]

Get-SourceFiles |
    ForEach-Object {
        $file = $_

        if (Test-BinaryFile -File $file) {
            return
        }

        $extension = if ($file.Name -ieq 'Dockerfile' -or $file.Name -ieq 'Containerfile') {
            '.dockerfile'
        } elseif ([string]::IsNullOrWhiteSpace($file.Extension)) {
            '<none>'
        } else {
            $file.Extension.ToLowerInvariant()
        }

        $relativePath = Get-RelativePath -BasePath $rootPath -TargetPath $file.FullName
        $product = Get-ProductName -RelativePath $relativePath

        $totalLines = 0

        foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
            $totalLines++
        }

        $fileResults.Add([pscustomobject]@{
            Product = $product
            Lines = $totalLines
        })
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
