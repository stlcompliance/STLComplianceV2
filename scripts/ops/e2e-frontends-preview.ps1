param(
    [string]$HostAddress = "127.0.0.1"
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "../..")

$frontends = @(
    @{ App = "suite-frontend"; Port = 5174; Env = @{ VITE_NEXARR_PROXY_TARGET = "http://127.0.0.1:5101" } },
    @{ App = "staffarr-frontend"; Port = 5175; Env = @{ VITE_STAFFARR_PROXY_TARGET = "http://127.0.0.1:5102" } },
    @{ App = "trainarr-frontend"; Port = 5176; Env = @{ VITE_TRAINARR_PROXY_TARGET = "http://127.0.0.1:5103" } },
    @{ App = "compliancecore-frontend"; Port = 5177; Env = @{ VITE_COMPLIANCECORE_PROXY_TARGET = "http://127.0.0.1:5107" } },
    @{ App = "maintainarr-frontend"; Port = 5178; Env = @{ VITE_MAINTAINARR_PROXY_TARGET = "http://127.0.0.1:5104" } },
    @{ App = "supplyarr-frontend"; Port = 5179; Env = @{ VITE_SUPPLYARR_PROXY_TARGET = "http://127.0.0.1:5106" } },
    @{ App = "routarr-frontend"; Port = 5180; Env = @{ VITE_ROUTARR_PROXY_TARGET = "http://127.0.0.1:5105" } },
    @{
        App = "companion-frontend"
        Port = 5181
        Env = @{
            VITE_NEXARR_PROXY_TARGET = "http://127.0.0.1:5101"
            VITE_TRAINARR_FRONTEND_BASE = "http://127.0.0.1:5176"
            VITE_STAFFARR_FRONTEND_BASE = "http://127.0.0.1:5175"
            VITE_MAINTAINARR_FRONTEND_BASE = "http://127.0.0.1:5178"
            VITE_ROUTARR_FRONTEND_BASE = "http://127.0.0.1:5180"
            VITE_SUPPLYARR_FRONTEND_BASE = "http://127.0.0.1:5179"
            VITE_COMPLIANCECORE_FRONTEND_BASE = "http://127.0.0.1:5177"
        }
    }
)

$logDir = Join-Path $repoRoot ".e2e-preview-logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

foreach ($frontend in $frontends) {
    $appPath = Join-Path $repoRoot "apps/$($frontend.App)"
    Write-Host "Building $($frontend.App)..."
    Push-Location $appPath
    try {
        npm ci --silent
        npm run build --silent
    }
    finally {
        Pop-Location
    }

    $logFile = Join-Path $logDir "$($frontend.App).log"
    Write-Host "Starting preview $($frontend.App) on $($frontend.Port) (log: $logFile)"

    $envPairs = $frontend.Env.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }
    $envPrefix = $envPairs -join " "
    $command = "cd `"$appPath`"; $envPrefix npm run preview -- --host $HostAddress --port $($frontend.Port)"
    Start-Process -FilePath "powershell" -ArgumentList @("-NoProfile", "-Command", $command) -WindowStyle Hidden `
        -RedirectStandardOutput $logFile -RedirectStandardError $logFile
}

Write-Host "Waiting for suite frontend on http://${HostAddress}:5174 ..."
for ($i = 1; $i -le 30; $i++) {
    try {
        $response = Invoke-WebRequest -Uri "http://${HostAddress}:5174/" -UseBasicParsing -TimeoutSec 3
        if ($response.StatusCode -eq 200) {
            Write-Host "Suite frontend reachable."
            break
        }
    }
    catch {
        Start-Sleep -Seconds 2
    }
}

Write-Host "Preview processes started. Logs: $logDir"
