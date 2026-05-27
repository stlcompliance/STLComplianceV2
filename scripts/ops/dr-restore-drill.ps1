param(
    [string]$PostgresHost = "localhost",
    [int]$PostgresPort = 5432,
    [string]$PostgresUser = "stl",
    [string]$PostgresPassword = "stl_dev_password",
    [Parameter(Mandatory = $true)]
    [string]$BackupDirectory,
    [string]$DrillSuffix = "_dr_restore_drill",
    [string[]]$Databases = @(),
    [string]$DockerContainerName = "",
    [switch]$KeepDrillDatabases,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$defaultDatabases = @(
    "nexarr",
    "staffarr",
    "trainarr",
    "maintainarr",
    "routarr",
    "supplyarr",
    "compliancecore"
)

$targetDatabases = if ($Databases.Count -gt 0) { $Databases } else { $defaultDatabases }

function Resolve-BackupPath {
    param([string]$DatabaseName)

    foreach ($extension in @(".custom", ".dump", ".sql")) {
        $candidate = Join-Path $BackupDirectory "$DatabaseName$extension"
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    throw "No backup found for '$DatabaseName' under '$BackupDirectory'."
}

function Invoke-PgCommand {
    param(
        [string[]]$Arguments,
        [string]$Database = "postgres"
    )

    if ($DockerContainerName) {
        $dockerArgs = @("exec", "-e", "PGPASSWORD=$PostgresPassword", $DockerContainerName) + $Arguments
        & docker @dockerArgs
        if ($LASTEXITCODE -ne 0) {
            throw "docker exec failed for: $($Arguments -join ' ')"
        }
        return
    }

    $env:PGPASSWORD = $PostgresPassword
    try {
        & psql -h $PostgresHost -p $PostgresPort -U $PostgresUser -d $Database -v ON_ERROR_STOP=1 @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "psql failed for: $($Arguments -join ' ')"
        }
    }
    finally {
        Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    }
}

function Invoke-PgRestore {
    param(
        [string]$BackupPath,
        [string]$TargetDatabase
    )

    if ($DockerContainerName) {
        $containerBackup = "/tmp/dr-restore-$([IO.Path]::GetFileName($BackupPath))"
        docker cp $BackupPath "${DockerContainerName}:$containerBackup"
        if ($LASTEXITCODE -ne 0) {
            throw "docker cp failed for backup '$BackupPath'."
        }

        $dockerArgs = @(
            "exec",
            "-e", "PGPASSWORD=$PostgresPassword",
            $DockerContainerName,
            "pg_restore",
            "--no-owner",
            "--no-privileges",
            "--dbname=$TargetDatabase",
            "-h", $PostgresHost,
            "-p", "$PostgresPort",
            "-U", $PostgresUser,
            $containerBackup
        )
        & docker @dockerArgs
        if ($LASTEXITCODE -ne 0) {
            throw "pg_restore failed for database '$TargetDatabase'."
        }

        docker exec $DockerContainerName rm -f $containerBackup | Out-Null
        return
    }

    $env:PGPASSWORD = $PostgresPassword
    try {
        & pg_restore --no-owner --no-privileges --dbname=$TargetDatabase -h $PostgresHost -p $PostgresPort -U $PostgresUser $BackupPath
        if ($LASTEXITCODE -ne 0) {
            throw "pg_restore failed for database '$TargetDatabase'."
        }
    }
    finally {
        Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    }
}

function Invoke-PgSqlFile {
    param(
        [string]$BackupPath,
        [string]$TargetDatabase
    )

    if ($DockerContainerName) {
        $containerBackup = "/tmp/dr-restore-$([IO.Path]::GetFileName($BackupPath))"
        docker cp $BackupPath "${DockerContainerName}:$containerBackup"
        if ($LASTEXITCODE -ne 0) {
            throw "docker cp failed for backup '$BackupPath'."
        }

        $dockerArgs = @(
            "exec",
            "-e", "PGPASSWORD=$PostgresPassword",
            $DockerContainerName,
            "psql",
            "-v", "ON_ERROR_STOP=1",
            "-h", $PostgresHost,
            "-p", "$PostgresPort",
            "-U", $PostgresUser,
            "-d", $TargetDatabase,
            "-f", $containerBackup
        )
        & docker @dockerArgs
        if ($LASTEXITCODE -ne 0) {
            throw "psql restore failed for database '$TargetDatabase'."
        }

        docker exec $DockerContainerName rm -f $containerBackup | Out-Null
        return
    }

    $env:PGPASSWORD = $PostgresPassword
    try {
        & psql -v ON_ERROR_STOP=1 -h $PostgresHost -p $PostgresPort -U $PostgresUser -d $TargetDatabase -f $BackupPath
        if ($LASTEXITCODE -ne 0) {
            throw "psql restore failed for database '$TargetDatabase'."
        }
    }
    finally {
        Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    }
}

function Test-RestoredDatabase {
    param([string]$TargetDatabase)

    $sql = @"
SELECT
  (SELECT COUNT(*) FROM "__EFMigrationsHistory") AS migration_count,
  (SELECT EXISTS (
     SELECT 1 FROM information_schema.tables
     WHERE table_schema = 'public' AND table_name = 'platform_metadata'
   )) AS platform_metadata_exists;
"@

    if ($DockerContainerName) {
        $dockerArgs = @(
            "exec",
            "-e", "PGPASSWORD=$PostgresPassword",
            $DockerContainerName,
            "psql",
            "-tA",
            "-F", ",",
            "-h", $PostgresHost,
            "-p", "$PostgresPort",
            "-U", $PostgresUser,
            "-d", $TargetDatabase,
            "-c", $sql
        )
        $output = (& docker @dockerArgs | Out-String).Trim()
    }
    else {
        $env:PGPASSWORD = $PostgresPassword
        try {
            $output = (& psql -tA -F "," -h $PostgresHost -p $PostgresPort -U $PostgresUser -d $TargetDatabase -c $sql | Out-String).Trim()
        }
        finally {
            Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
        }
    }

    if (-not $output) {
        throw "Validation query returned no output for '$TargetDatabase'."
    }

    $parts = $output.Split(",")
    $migrationCount = [int]$parts[0]
    $platformMetadataExists = [bool]::Parse($parts[1])

    if ($migrationCount -le 0) {
        throw "Validation failed for '$TargetDatabase': no EF migration history rows."
    }

    if (-not $platformMetadataExists) {
        throw "Validation failed for '$TargetDatabase': platform_metadata table missing."
    }

    return [pscustomobject]@{
        Database = $TargetDatabase
        MigrationHistoryCount = $migrationCount
        PlatformMetadataTableExists = $platformMetadataExists
    }
}

if (-not (Test-Path $BackupDirectory)) {
    throw "Backup directory not found: $BackupDirectory"
}

Write-Host "DR restore drill"
Write-Host "  Host: $PostgresHost`:$PostgresPort"
Write-Host "  Backup directory: $BackupDirectory"
Write-Host "  Drill suffix: $DrillSuffix"
Write-Host "  Databases: $($targetDatabases -join ', ')"
if ($DockerContainerName) {
    Write-Host "  Docker container: $DockerContainerName"
}
if ($DryRun) {
    Write-Host "  Mode: DRY RUN"
}

$results = @()
foreach ($database in $targetDatabases) {
    $backupPath = Resolve-BackupPath -DatabaseName $database
    $drillDatabase = "$database$DrillSuffix"
    Write-Host ""
    Write-Host "[$database] backup=$backupPath -> drill database=$drillDatabase"

    if ($DryRun) {
        continue
    }

    Invoke-PgCommand -Arguments @("-c", "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$drillDatabase' AND pid <> pg_backend_pid();") -Database "postgres"
    Invoke-PgCommand -Arguments @("-c", "DROP DATABASE IF EXISTS `"$drillDatabase`";") -Database "postgres"
    Invoke-PgCommand -Arguments @("-c", "CREATE DATABASE `"$drillDatabase`";") -Database "postgres"

    $extension = [IO.Path]::GetExtension($backupPath).ToLowerInvariant()
    if ($extension -eq ".sql") {
        Invoke-PgSqlFile -BackupPath $backupPath -TargetDatabase $drillDatabase
    }
    else {
        Invoke-PgRestore -BackupPath $backupPath -TargetDatabase $drillDatabase
    }

    $results += Test-RestoredDatabase -TargetDatabase $drillDatabase

    if (-not $KeepDrillDatabases) {
        Invoke-PgCommand -Arguments @("-c", "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$drillDatabase' AND pid <> pg_backend_pid();") -Database "postgres"
        Invoke-PgCommand -Arguments @("-c", "DROP DATABASE IF EXISTS `"$drillDatabase`";") -Database "postgres"
        Write-Host "[$database] cleaned up drill database '$drillDatabase'"
    }
}

Write-Host ""
if ($DryRun) {
    Write-Host "DR restore drill dry run completed for $($targetDatabases.Count) databases."
}
else {
    Write-Host "DR restore drill passed for $($results.Count) databases."
    $results | Format-Table -AutoSize
}
