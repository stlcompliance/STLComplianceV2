param(
    [string[]]$ApiBaseUrls = @(
        "http://localhost:5101",
        "http://localhost:5102",
        "http://localhost:5103",
        "http://localhost:5104",
        "http://localhost:5105",
        "http://localhost:5106",
        "http://localhost:5107"
    ),
    [switch]$RequireOtelEnabled
)

$ErrorActionPreference = "Stop"

function Test-ObservabilityEndpoint {
    param([string]$BaseUrl)

    $uri = "$($BaseUrl.TrimEnd('/'))/health/observability"
    Write-Host "Checking $uri"

    try {
        $response = Invoke-RestMethod -Uri $uri -Method Get -TimeoutSec 15
    }
    catch {
        throw "Observability probe failed for $BaseUrl`: $($_.Exception.Message)"
    }

    if ($null -eq $response.otelEnabled) {
        throw "Observability payload missing otelEnabled for $BaseUrl"
    }

    if ($RequireOtelEnabled -and -not $response.otelEnabled) {
        throw "OTEL is disabled on $BaseUrl but -RequireOtelEnabled was set."
    }

    if ($response.otelEnabled) {
        if ([string]::IsNullOrWhiteSpace($response.serviceName)) {
            throw "OTEL enabled but serviceName missing on $BaseUrl"
        }

        if ($response.exporter -eq "otlp" -and -not $response.otlpEndpointConfigured) {
            throw "OTEL exporter reported otlp without endpoint configuration on $BaseUrl"
        }

        if ($response.meters -notcontains "STLCompliance.Platform") {
            throw "Expected STLCompliance.Platform meter on $BaseUrl"
        }
    }

    return [pscustomobject]@{
        BaseUrl = $BaseUrl
        OtelEnabled = [bool]$response.otelEnabled
        ServiceName = [string]$response.serviceName
        Exporter = [string]$response.exporter
    }
}

$results = @()
foreach ($baseUrl in $ApiBaseUrls) {
    $results += Test-ObservabilityEndpoint -BaseUrl $baseUrl
}

Write-Host ""
Write-Host "OTEL smoke checks passed for $($results.Count) APIs."
$results | Format-Table -AutoSize
