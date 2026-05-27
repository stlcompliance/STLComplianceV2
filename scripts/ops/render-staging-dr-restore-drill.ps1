param(
    [string]$BackupDirectory = "",
    [string[]]$Databases = @(),
    [switch]$SkipSnapshotFetch,
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

function Get-ScriptRoot {
    return Split-Path -Parent $MyInvocation.MyCommand.Path
}

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

    throw "Unable to parse PostgreSQL URI for staging drill target."
}

function Get-SelectedCatalogEntries {
    param([string[]]$SelectedDatabases)

    if ($SelectedDatabases.Count -eq 0) {
        return $Catalog
    }

    return $Catalog | Where-Object { $SelectedDatabases -contains $_.Database }
}

$scriptRoot = Get-ScriptRoot
$fetchScript = Join-Path $scriptRoot "render-staging-snapshot-fetch.ps1"
$drillScript = Join-Path $scriptRoot "dr-restore-drill.ps1"

if ([string]::IsNullOrWhiteSpace($BackupDirectory)) {
    $BackupDirectory = $env:RENDER_STAGING_SNAPSHOT_DIRECTORY
}
if ([string]::IsNullOrWhiteSpace($BackupDirectory)) {
    $BackupDirectory = Join-Path $env:TEMP "stl-render-staging-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
}

$entries = Get-SelectedCatalogEntries -SelectedDatabases $Databases

Write-Host "Render staging DR restore drill"
Write-Host "  Backup directory: $BackupDirectory"
Write-Host "  Databases: $($entries.Database -join ', ')"

if (-not $SkipSnapshotFetch) {
    if ($DryRun) {
        & $fetchScript -OutputDirectory $BackupDirectory -Databases $Databases -DryRun
    }
    else {
        & $fetchScript -OutputDirectory $BackupDirectory -Databases $Databases
    }
}

if ($DryRun) {
    Write-Host "Render staging DR restore drill dry run completed."
    exit 0
}

if (-not (Test-Path $BackupDirectory)) {
    throw "Backup directory not found: $BackupDirectory"
}

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
    $backupPath = Join-Path $BackupDirectory "$($entry.Database).custom"
    if (-not (Test-Path $backupPath)) {
        throw "Missing backup for '$($entry.Database)': $backupPath"
    }

    Write-Host ""
    Write-Host "[$($entry.Database)] restore drill on $($parsed.Host):$($parsed.Port)"

    & $drillScript `
        -PostgresHost $parsed.Host `
        -PostgresPort $parsed.Port `
        -PostgresUser $parsed.Username `
        -PostgresPassword $parsed.Password `
        -BackupDirectory $BackupDirectory `
        -Databases @($entry.Database)
}

Write-Host ""
Write-Host "Render staging DR restore drill passed for $($entries.Count) databases."
