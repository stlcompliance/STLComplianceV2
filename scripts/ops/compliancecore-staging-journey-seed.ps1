param(
    [string]$NexArrBaseUrl = "",
    [string]$ComplianceCoreBaseUrl = ""
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

if ([string]::IsNullOrWhiteSpace($ComplianceCoreBaseUrl)) {
    $ComplianceCoreBaseUrl = $env:RENDER_STAGING_COMPLIANCECORE_API_URL
}
if ([string]::IsNullOrWhiteSpace($ComplianceCoreBaseUrl)) {
    $ComplianceCoreBaseUrl = $env:STL_COMPLIANCECORE_BASE_URL
}
if ([string]::IsNullOrWhiteSpace($ComplianceCoreBaseUrl)) {
    $ComplianceCoreBaseUrl = "http://localhost:5107"
}

$NexArrBaseUrl = Normalize-BaseUrl $NexArrBaseUrl
$ComplianceCoreBaseUrl = Normalize-BaseUrl $ComplianceCoreBaseUrl

$email = if ($env:STL_LOAD_DEMO_EMAIL) { $env:STL_LOAD_DEMO_EMAIL } else { "admin@demo.stl" }
$password = if ($env:STL_LOAD_DEMO_PASSWORD) { $env:STL_LOAD_DEMO_PASSWORD } else { "ChangeMe!Demo2026" }
$tenantId = if ($env:STL_LOAD_DEMO_TENANT_ID) { $env:STL_LOAD_DEMO_TENANT_ID } else { "11111111-1111-1111-1111-111111111101" }

Write-Host "Logging into NexArr at $NexArrBaseUrl"
$loginBody = @{ email = $email; password = $password; tenantId = $tenantId } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Method Post -Uri "$NexArrBaseUrl/api/auth/login" -ContentType "application/json" -Body $loginBody
$nexarrToken = $loginResponse.accessToken

Write-Host "Creating Compliance Core handoff"
$handoffBody = @{ productKey = "compliancecore"; callbackUrl = $null } | ConvertTo-Json
$handoffResponse = Invoke-RestMethod -Method Post -Uri "$NexArrBaseUrl/api/launch/handoff" `
    -ContentType "application/json" `
    -Headers @{ Authorization = "Bearer $nexarrToken" } `
    -Body $handoffBody

Write-Host "Redeeming Compliance Core session at $ComplianceCoreBaseUrl"
$redeemBody = @{ handoffCode = $handoffResponse.handoffCode } | ConvertTo-Json
$redeemResponse = Invoke-RestMethod -Method Post -Uri "$ComplianceCoreBaseUrl/api/auth/handoff/redeem" `
    -ContentType "application/json" `
    -Body $redeemBody

Write-Host "Seeding load-test journey fixtures"
$seedResponse = Invoke-RestMethod -Method Post -Uri "$ComplianceCoreBaseUrl/api/load-test-journey/seed" `
    -ContentType "application/json" `
    -Headers @{ Authorization = "Bearer $($redeemResponse.accessToken)" } `
    -Body "{}"

$seedResponse | ConvertTo-Json -Depth 5
if ($seedResponse.rulePackId) {
    $rulePackId = $seedResponse.rulePackId.ToString()
    $env:STL_LOAD_JOURNEY_RULE_PACK_ID = $rulePackId
    if ($env:GITHUB_ENV) {
        Add-Content -Path $env:GITHUB_ENV -Value "STL_LOAD_JOURNEY_RULE_PACK_ID=$rulePackId"
    }
    Write-Host "Set STL_LOAD_JOURNEY_RULE_PACK_ID=$rulePackId"
}
Write-Host "Compliance Core load-test journey seed completed."
