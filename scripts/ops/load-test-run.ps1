param(
    [ValidateSet("api-health-liveness", "api-health-ready", "nexarr-platform-health", "nexarr-auth-me", "product-auth-handoff-me", "all")]
    [string]$Scenario = "all",
    [int]$Vus = 5,
    [string]$Duration = "30s",
    [string]$OutputDirectory = "",
    [switch]$SkipSloValidation
)

$ErrorActionPreference = "Stop"

function Resolve-K6Path {
    $k6 = Get-Command k6 -ErrorAction SilentlyContinue
    if ($null -eq $k6) {
        throw "k6 is not on PATH. Install from https://k6.io/docs/get-started/installation/"
    }
    return $k6.Source
}

function Get-RepoRoot {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    return (Resolve-Path (Join-Path $scriptDir "../..")).Path
}

function Get-ScenarioScript {
    param([string]$ScenarioKey)
    return Join-Path $RepoRoot "tests/load-k6/scenarios/$ScenarioKey.js"
}

function Invoke-K6Scenario {
    param(
        [string]$ScenarioKey,
        [string]$SummaryPath
    )

    $scriptPath = Get-ScenarioScript -ScenarioKey $ScenarioKey
    if (-not (Test-Path $scriptPath)) {
        throw "Missing k6 script: $scriptPath"
    }

    Write-Host "Running k6 scenario '$ScenarioKey' -> $SummaryPath"
    $env:STL_LOAD_VUS = [string]$Vus
    $env:STL_LOAD_DURATION = $Duration

    & $K6Path run $scriptPath --summary-export $SummaryPath
    if ($LASTEXITCODE -ne 0) {
        throw "k6 exited with code $LASTEXITCODE for scenario '$ScenarioKey'"
    }
}

function Test-SloEvaluation {
    param(
        [string]$ScenarioKey,
        [string]$SummaryPath
    )

    $testProject = Join-Path $RepoRoot "tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj"
    $env:LOAD_SCENARIO_KEY = $ScenarioKey
    $env:LOAD_SUMMARY_PATH = $SummaryPath
    dotnet test $testProject -c Release --no-build `
        --filter "FullyQualifiedName~Evaluate_summary_file_from_environment" `
        --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        throw "SLO evaluation failed for scenario '$ScenarioKey'"
    }
}

$RepoRoot = Get-RepoRoot
$K6Path = Resolve-K6Path

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $env:TEMP "stl-load-k6-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
}
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

$scenarios = @()
switch ($Scenario) {
    "all" {
        $scenarios = @(
            "api-health-liveness",
            "api-health-ready",
            "nexarr-platform-health",
            "nexarr-auth-me",
            "product-auth-handoff-me"
        )
    }
    default {
        $scenarios = @($Scenario)
    }
}

Write-Host "Building Load.Tests for SLO validation..."
dotnet build (Join-Path $RepoRoot "tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj") -c Release -v q

$results = @()
foreach ($scenarioKey in $scenarios) {
    $summaryPath = Join-Path $OutputDirectory "$scenarioKey-summary.json"
    Invoke-K6Scenario -ScenarioKey $scenarioKey -SummaryPath $summaryPath

    if (-not $SkipSloValidation) {
        Test-SloEvaluation -ScenarioKey $scenarioKey -SummaryPath $summaryPath
    }

    $results += [pscustomobject]@{
        Scenario = $scenarioKey
        Summary = $summaryPath
    }
}

Write-Host ""
Write-Host "Load test harness completed for $($results.Count) scenario(s)."
$results | Format-Table -AutoSize
