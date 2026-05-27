param(
    [switch]$BuildFrontends,
    [switch]$ApisOnly
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "../..")
Push-Location $repoRoot

try {
    $composeFiles = @("-f", "docker-compose.yml")
    if (-not $ApisOnly) {
        $composeFiles += @("-f", "docker-compose.e2e.yml", "--profile", "e2e")
    }

    $apiServices = @(
        "postgres",
        "nexarr-api",
        "staffarr-api",
        "trainarr-api",
        "maintainarr-api",
        "routarr-api",
        "supplyarr-api",
        "compliancecore-api"
    )

    if ($ApisOnly) {
        Write-Host "Starting API stack for browser E2E..."
        docker compose @composeFiles up -d --build @apiServices
    }
    elseif ($BuildFrontends) {
        Write-Host "Starting full E2E stack (APIs + dockerized Vite previews)..."
        docker compose @composeFiles up -d --build @apiServices `
            suite-frontend-e2e staffarr-frontend-e2e trainarr-frontend-e2e `
            compliancecore-frontend-e2e maintainarr-frontend-e2e supplyarr-frontend-e2e routarr-frontend-e2e
    }
    else {
        Write-Host "Starting APIs via compose; use e2e-frontends-preview.ps1 for host Vite previews..."
        docker compose -f docker-compose.yml up -d --build @apiServices
    }

    Write-Host "Waiting for NexArr health..."
    for ($i = 1; $i -le 60; $i++) {
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:5101/health" -UseBasicParsing -TimeoutSec 3
            if ($response.StatusCode -eq 200) {
                Write-Host "NexArr API healthy after $i attempt(s)."
                return
            }
        }
        catch {
            Start-Sleep -Seconds 5
        }
    }

    throw "Timed out waiting for NexArr API health on http://localhost:5101/health"
}
finally {
    Pop-Location
}
