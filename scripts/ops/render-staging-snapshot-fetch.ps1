param(
    [string]$OutputDirectory = "",
    [string[]]$Databases = @(),
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$Catalog = @(
    @{ Database = "nexarr"; Env = "RENDER_STAGING_NEXARR_DATABASE_URL" },
    @{ Database = "staffarr"; Env = "RENDER_STAGING_STAFFARR_DATABASE_URL" },
    @{ Database = "trainarr"; Env = "RENDER_STAGING_TRAINARR_DATABASE_URL" },
    @{ Database = "maintainarr"; Env = "RENDER_STAGING_MAINTAINARR_DATABASE_URL" },
    @{ Database = "routarr"; Env = "RENDER_STAGING_ROUTARR_DATABASE_URL" },
    @{ Database = "supplyarr"; Env = "RENDER_STAGING_SUPPLYARR_DATABASE_URL" },
    @{ Database = "compliancecore"; Env = "RENDER_STAGING_COMPLIANCECORE_DATABASE_URL" }
)

function Resolve-PgDumpPath {
    $pgDump = Get-Command pg_dump -ErrorAction SilentlyContinue
    if ($null -eq $pgDump) {
        throw "pg_dump is not on PATH. Install PostgreSQL client tools or download Render dashboard backups manually."
    }
    return $pgDump.Source
}

function Get-SelectedCatalogEntries {
    param([string[]]$SelectedDatabases)

    if ($SelectedDatabases.Count -eq 0) {
        return $Catalog
    }

    return $Catalog | Where-Object { $SelectedDatabases -contains $_.Database }
}

# Npgsql is not guaranteed on PATH for scripts; parse URI manually when needed.
function Parse-PostgresUri {
    param([string]$Uri)

    if ($Uri -match "^postgres(?:ql)?://([^:]+):([^@]+)@([^:/]+)(?::(\d+))?/([^?]+)") {
        return @{
            Username = $Matches[1]
            Password = [Uri]::UnescapeDataString($Matches[2])
            Host = $Matches[3]
            Port = if ($Matches[4]) { [int]$Matches[4] } else { 5432 }
            Database = $Matches[5]
        }
    }

    throw "Unable to parse PostgreSQL URI. Use a standard postgres://user:pass@host:port/db URL."
}

$PgDumpPath = Resolve-PgDumpPath

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = $env:RENDER_STAGING_SNAPSHOT_DIRECTORY
}
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $env:TEMP "stl-render-staging-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$entries = Get-SelectedCatalogEntries -SelectedDatabases $Databases

Write-Host "Render staging snapshot fetch"
Write-Host "  Output directory: $OutputDirectory"
Write-Host "  Databases: $($entries.Database -join ', ')"

$missing = @()
foreach ($entry in $entries) {
    $databaseUrl = [Environment]::GetEnvironmentVariable($entry.Env)
    if ([string]::IsNullOrWhiteSpace($databaseUrl)) {
        $missing += $entry.Env
    }
}

if ($missing.Count -gt 0) {
    throw "Missing staging database URL environment variables: $($missing -join ', ')"
}

foreach ($entry in $entries) {
    $databaseUrl = [Environment]::GetEnvironmentVariable($entry.Env)
    $parsed = Parse-PostgresUri -Uri $databaseUrl
    $backupPath = Join-Path $OutputDirectory "$($entry.Database).custom"

    Write-Host ""
    Write-Host "[$($entry.Database)] pg_dump -> $backupPath"

    if ($DryRun) {
        continue
    }

    $env:PGPASSWORD = $parsed.Password
    try {
        & $PgDumpPath -Fc -h $parsed.Host -p $parsed.Port -U $parsed.Username -d $parsed.Database -f $backupPath
        if ($LASTEXITCODE -ne 0) {
            throw "pg_dump failed for '$($entry.Database)'."
        }
    }
    finally {
        Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    }
}

Write-Host ""
if ($DryRun) {
    Write-Host "Render staging snapshot fetch dry run completed."
}
else {
    Write-Host "Render staging snapshot fetch completed for $($entries.Count) databases."
}
