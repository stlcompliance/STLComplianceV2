param(
    [string]$NexArrBaseUrl = "",
    [string]$RoutArrBaseUrl = ""
)

$ErrorActionPreference = "Stop"

function Normalize-BaseUrl {
    param([string]$Raw)
    $trimmed = $Raw.Trim().TrimEnd('/')
    if ($trimmed -match '^https?://') {
        return $trimmed
    }
    return "https://$trimmed"
}

if ([string]::IsNullOrWhiteSpace($NexArrBaseUrl)) {
    $NexArrBaseUrl = $env:RENDER_STAGING_NEXARR_API_URL
}
if ([string]::IsNullOrWhiteSpace($NexArrBaseUrl)) {
    $NexArrBaseUrl = $env:STL_NEXARR_BASE_URL
}
if ([string]::IsNullOrWhiteSpace($NexArrBaseUrl)) {
    $NexArrBaseUrl = "http://localhost:5101"
}

if ([string]::IsNullOrWhiteSpace($RoutArrBaseUrl)) {
    $RoutArrBaseUrl = $env:RENDER_STAGING_ROUTARR_API_URL
}
if ([string]::IsNullOrWhiteSpace($RoutArrBaseUrl)) {
    $RoutArrBaseUrl = $env:STL_ROUTARR_BASE_URL
}
if ([string]::IsNullOrWhiteSpace($RoutArrBaseUrl)) {
    $RoutArrBaseUrl = "http://localhost:5105"
}

$NexArrBaseUrl = Normalize-BaseUrl $NexArrBaseUrl
$RoutArrBaseUrl = Normalize-BaseUrl $RoutArrBaseUrl

$email = if ($env:STL_LOAD_DEMO_EMAIL) { $env:STL_LOAD_DEMO_EMAIL } else { "admin@demo.stl" }
$password = if ($env:STL_LOAD_DEMO_PASSWORD) { $env:STL_LOAD_DEMO_PASSWORD } else { "ChangeMe!Demo2026" }
$tenantId = if ($env:STL_LOAD_DEMO_TENANT_ID) { $env:STL_LOAD_DEMO_TENANT_ID } else { "11111111-1111-1111-1111-111111111101" }

Write-Host "Logging into NexArr at $NexArrBaseUrl"
$loginBody = @{ email = $email; password = $password; tenantId = $tenantId } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Method Post -Uri "$NexArrBaseUrl/api/auth/login" -ContentType "application/json" -Body $loginBody
$nexarrToken = $loginResponse.accessToken

Write-Host "Creating RoutArr handoff"
$handoffBody = @{ productKey = "routarr"; callbackUrl = $null } | ConvertTo-Json
$handoffResponse = Invoke-RestMethod -Method Post -Uri "$NexArrBaseUrl/api/launch/handoff" `
    -ContentType "application/json" `
    -Headers @{ Authorization = "Bearer $nexarrToken" } `
    -Body $handoffBody

Write-Host "Redeeming RoutArr session at $RoutArrBaseUrl"
$redeemBody = @{ handoffCode = $handoffResponse.handoffCode } | ConvertTo-Json
$redeemResponse = Invoke-RestMethod -Method Post -Uri "$RoutArrBaseUrl/api/auth/handoff/redeem" `
    -ContentType "application/json" `
    -Body $redeemBody

Write-Host "Seeding RoutArr load-test journey dispatch trip mirror"
$seedResponse = Invoke-RestMethod -Method Post -Uri "$RoutArrBaseUrl/api/load-test-journey/seed" `
    -ContentType "application/json" `
    -Headers @{ Authorization = "Bearer $($redeemResponse.accessToken)" } `
    -Body "{}"

$seedResponse | ConvertTo-Json -Depth 5
Write-Host "RoutArr load-test journey dispatch trip mirror seed completed."
