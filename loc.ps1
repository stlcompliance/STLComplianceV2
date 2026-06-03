param(
    [string]$Root = $PSScriptRoot,
    [switch]$Detailed
)

$ErrorActionPreference = 'Stop'

$rootPath = (Resolve-Path -LiteralPath $Root).Path

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
    '.py', '.sql',
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
    '.png', '.jpg', '.jpeg', '.gif', '.webp', '.ico', '.svg',
    '.pdf', '.doc', '.docx', '.xls', '.xlsx', '.ppt', '.pptx',
    '.mp3', '.mp4', '.mov', '.avi', '.wav',
    '.pyc', '.cache', '.lock', '.log'
) | ForEach-Object { [void]$excludedExtensions.Add($_) }

$excludedFileNamePatterns = @(
    '\.g\.(cs|ts|tsx|js|jsx)$',
    '\.generated\.',
    '\.designer\.cs$',
    '\.min\.(css|js)$',
    '\.bundle\.(css|js)$',
    '^package-lock\.json$',
    '^yarn\.lock$',
    '^pnpm-lock\.yaml$',
    '^\.tmp-'
)

function Test-ExcludedPath {
    param([System.IO.FileSystemInfo]$Item)

    $relativePath = [System.IO.Path]::GetRelativePath($rootPath, $Item.FullName)
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

function Get-CommentStyle {
    param([string]$Extension)

    switch ($Extension.ToLowerInvariant()) {
        { $_ -in '.cs', '.ts', '.tsx', '.js', '.jsx', '.mjs', '.cjs', '.css', '.scss', '.sass', '.less', '.java', '.go', '.rs', '.php' } { return 'slash' }
        { $_ -in '.ps1', '.psm1', '.psd1', '.py', '.sh', '.bash', '.yaml', '.yml' } { return 'hash' }
        { $_ -in '.html', '.htm', '.xml', '.config', '.csproj', '.props', '.targets' } { return 'xml' }
        default { return 'none' }
    }
}

function Test-CommentLine {
    param(
        [string]$TrimmedLine,
        [string]$Style
    )

    if ($TrimmedLine.Length -eq 0) {
        return $false
    }

    switch ($Style) {
        'slash' { return $TrimmedLine.StartsWith('//') -or $TrimmedLine.StartsWith('/*') -or $TrimmedLine.StartsWith('*') -or $TrimmedLine.StartsWith('*/') }
        'hash' { return $TrimmedLine.StartsWith('#') }
        'xml' { return $TrimmedLine.StartsWith('<!--') -or $TrimmedLine.StartsWith('-->') }
        default { return $false }
    }
}

$fileResults = New-Object System.Collections.Generic.List[object]

Get-ChildItem -LiteralPath $rootPath -Recurse -File -Force |
    Where-Object { Test-SourceFile -File $_ } |
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

        $relativePath = [System.IO.Path]::GetRelativePath($rootPath, $file.FullName)
        $topDirectory = ($relativePath -split '[\\/]')[0]
        $style = Get-CommentStyle -Extension $extension

        $totalLines = 0
        $blankLines = 0
        $commentLines = 0

        foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
            $totalLines++
            $trimmed = $line.Trim()

            if ($trimmed.Length -eq 0) {
                $blankLines++
            } elseif (Test-CommentLine -TrimmedLine $trimmed -Style $style) {
                $commentLines++
            }
        }

        $fileResults.Add([pscustomobject]@{
            Path = $relativePath
            Extension = $extension
            TopDirectory = $topDirectory
            Lines = $totalLines
            Blank = $blankLines
            Comment = $commentLines
            Code = $totalLines - $blankLines - $commentLines
        })
    }

$summary = [pscustomobject]@{
    Files = $fileResults.Count
    Lines = ($fileResults | Measure-Object -Property Lines -Sum).Sum
    Code = ($fileResults | Measure-Object -Property Code -Sum).Sum
    Blank = ($fileResults | Measure-Object -Property Blank -Sum).Sum
    Comment = ($fileResults | Measure-Object -Property Comment -Sum).Sum
}

Write-Host "LOC summary for $rootPath"
Write-Host ""
$summary | Format-Table -AutoSize

Write-Host ""
Write-Host "By extension"
$fileResults |
    Group-Object Extension |
    ForEach-Object {
        [pscustomobject]@{
            Extension = $_.Name
            Files = $_.Count
            Lines = ($_.Group | Measure-Object -Property Lines -Sum).Sum
            Code = ($_.Group | Measure-Object -Property Code -Sum).Sum
            Blank = ($_.Group | Measure-Object -Property Blank -Sum).Sum
            Comment = ($_.Group | Measure-Object -Property Comment -Sum).Sum
        }
    } |
    Sort-Object Code -Descending |
    Format-Table -AutoSize

Write-Host ""
Write-Host "By top-level path"
$fileResults |
    Group-Object TopDirectory |
    ForEach-Object {
        [pscustomobject]@{
            Path = $_.Name
            Files = $_.Count
            Lines = ($_.Group | Measure-Object -Property Lines -Sum).Sum
            Code = ($_.Group | Measure-Object -Property Code -Sum).Sum
            Blank = ($_.Group | Measure-Object -Property Blank -Sum).Sum
            Comment = ($_.Group | Measure-Object -Property Comment -Sum).Sum
        }
    } |
    Sort-Object Code -Descending |
    Format-Table -AutoSize

if ($Detailed) {
    Write-Host ""
    Write-Host "Largest files"
    $fileResults |
        Sort-Object Code -Descending |
        Select-Object -First 50 |
        Format-Table Path, Lines, Code, Blank, Comment -AutoSize
}
