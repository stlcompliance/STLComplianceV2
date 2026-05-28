param(
    [ValidateSet(
        "api-health-liveness",
        "api-health-ready",
        "nexarr-platform-health",
        "nexarr-auth-me",
        "product-auth-handoff-me",
        "trainarr-qualification-check",
        "routarr-dispatch-workflow-gate",
        "all")]
    [string]$Scenario = "all",
    [int]$Vus = 5,
    [string]$Duration = "30s",
    [string]$OutputDirectory = "",
    [switch]$SkipHealthGate,
    [switch]$SkipSloValidation
)

$ErrorActionPreference = "Stop"

$Catalog = @(
    @{ Product = "nexarr"; SourceEnv = "RENDER_STAGING_NEXARR_API_URL"; TargetEnv = "STL_NEXARR_BASE_URL" },
    @{ Product = "staffarr"; SourceEnv = "RENDER_STAGING_STAFFARR_API_URL"; TargetEnv = "STL_STAFFARR_BASE_URL" },
    @{ Product = "trainarr"; SourceEnv = "RENDER_STAGING_TRAINARR_API_URL"; TargetEnv = "STL_TRAINARR_BASE_URL" },
    @{ Product = "maintainarr"; SourceEnv = "RENDER_STAGING_MAINTAINARR_API_URL"; TargetEnv = "STL_MAINTAINARR_BASE_URL" },
    @{ Product = "routarr"; SourceEnv = "RENDER_STAGING_ROUTARR_API_URL"; TargetEnv = "STL_ROUTARR_BASE_URL" },
    @{ Product = "supplyarr"; SourceEnv = "RENDER_STAGING_SUPPLYARR_API_URL"; TargetEnv = "STL_SUPPLYARR_BASE_URL" },
    @{ Product = "compliancecore"; SourceEnv = "RENDER_STAGING_COMPLIANCECORE_API_URL"; TargetEnv = "STL_COMPLIANCECORE_BASE_URL" }
)

function Get-ScriptRoot {
    return Split-Path -Parent $MyInvocation.MyCommand.Path
}

function Normalize-BaseUrl {
    param([string]$Raw)

    $trimmed = $Raw.Trim().TrimEnd('/')
    if ($trimmed -match '^https?://') {
        return $trimmed
    }

    return "https://$trimmed"
}

function Resolve-StagingEndpoints {
    $missing = @()
    foreach ($entry in $Catalog) {
        $raw = [Environment]::GetEnvironmentVariable($entry.SourceEnv)
        if ([string]::IsNullOrWhiteSpace($raw)) {
            $missing += $entry.SourceEnv
            continue
        }

        $normalized = Normalize-BaseUrl $raw
        Set-Item -Path "Env:$($entry.TargetEnv)" -Value $normalized
    }

    if ($missing.Count -gt 0) {
        throw "Missing staging API URL environment variables: $($missing -join ', ')"
    }
}

function Test-StagingHealth {
    foreach ($entry in $Catalog) {
        $baseUrl = [Environment]::GetEnvironmentVariable($entry.TargetEnv)
        $healthUrl = "$($baseUrl.TrimEnd('/'))/health"
        try {
            $response = Invoke-WebRequest -Uri $healthUrl -Method Get -TimeoutSec 15 -UseBasicParsing
            if ($response.StatusCode -lt 200 -or $response.StatusCode -ge 300) {
                throw "Non-success status $($response.StatusCode) from $healthUrl"
            }
            Write-Host "Healthy: $healthUrl"
        }
        catch {
            throw "Staging health gate failed for $($entry.Product) at $healthUrl : $($_.Exception.Message)"
        }
    }
}

$scriptRoot = Get-ScriptRoot
$loadTestScript = Join-Path $scriptRoot "load-test-run.ps1"

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = $env:RENDER_STAGING_LOAD_OUTPUT_DIRECTORY
}
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $env:TEMP "stl-render-staging-load-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
}

Resolve-StagingEndpoints
$env:STL_LOAD_SLO_PROFILE = "product-owner"

if (-not $SkipHealthGate) {
    Write-Host "Running Render staging API health gate..."
    Test-StagingHealth
}

$loadArgs = @{
    Scenario = $Scenario
    Vus = $Vus
    Duration = $Duration
    OutputDirectory = $OutputDirectory
}
if ($SkipSloValidation) {
    $loadArgs.SkipSloValidation = $true
}

Write-Host "Starting Render staging load soak (product-owner SLO profile)..."
& $loadTestScript @loadArgs

Write-Host ""
Write-Host "Render staging load soak completed. Summaries in $OutputDirectory"
