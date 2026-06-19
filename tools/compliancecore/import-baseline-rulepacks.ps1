param(
    [Parameter(Mandatory = $true)]
    [string]$ComplianceCoreBaseUrl,

    [Parameter(Mandatory = $true)]
    [string]$AccessToken,

    [string]$RulePackRoot = "",

    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RulePackRoot)) {
    $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $RepoRoot = Resolve-Path (Join-Path $ScriptDir "..\..")
    $RulePackRoot = Join-Path $RepoRoot "root\rulepack\baseline"
}

$RulePackRoot = (Resolve-Path $RulePackRoot).Path
$BaseUrl = $ComplianceCoreBaseUrl.TrimEnd("/")
$Headers = @{ Authorization = "Bearer $AccessToken" }

$CsvNames = @(
    "controlled_vocabulary.csv",
    "vocabulary_aliases.csv",
    "compliance_keys.csv",
    "material_keys.csv",
    "rule_packs.csv",
    "rule_requirements.csv",
    "rule_fact_requirements.csv",
    "regulatory_mappings.csv",
    "sds_references.csv",
    "exception_exemptions.csv",
    "evidence_references.csv"
)

$Programs = @(
    @{ Key = "osha_general_industry"; BodyKey = "osha"; BodyLabel = "Occupational Safety and Health Administration"; JurisdictionKey = "us_federal_osha"; Label = "OSHA General Industry and Recordkeeping"; Description = "Federal OSHA recordkeeping and general industry baseline requirements." }
    @{ Key = "epa_environmental_spine"; BodyKey = "epa"; BodyLabel = "Environmental Protection Agency"; JurisdictionKey = "us_federal_epa"; Label = "EPA Environmental Spine"; Description = "Federal environmental waste, release, water, air, refrigerant, chemical, and tank baseline requirements." }
    @{ Key = "dol_employment_labor"; BodyKey = "dol"; BodyLabel = "U.S. Department of Labor"; JurisdictionKey = "us_federal_dol"; Label = "DOL Employment and Labor Baseline"; Description = "Federal wage, leave, notice, and labor recordkeeping baseline requirements." }
    @{ Key = "ftc_privacy_communications"; BodyKey = "ftc"; BodyLabel = "Federal Trade Commission"; JurisdictionKey = "us_federal_ftc"; Label = "FTC Privacy, Safeguards, and Communications"; Description = "Federal privacy, safeguards, telemarketing, and commercial email baseline requirements." }
    @{ Key = "federal_electronic_records"; BodyKey = "congress"; BodyLabel = "United States Congress"; JurisdictionKey = "us_federal_congress"; Label = "Electronic Records and Signatures"; Description = "E-SIGN and state UETA electronic records and signatures baseline requirements." }
    @{ Key = "state_business_authority"; BodyKey = "state_authorities"; BodyLabel = "State and Local Filing Authorities"; JurisdictionKey = "us_state_local_overlay"; Label = "State and Local Business Authority"; Description = "Jurisdiction-specific entity, licensing, permit, tax, UCC, consumer, and accessibility overlays." }
    @{ Key = "doj_accessibility"; BodyKey = "doj"; BodyLabel = "U.S. Department of Justice"; JurisdictionKey = "us_federal_doj"; Label = "ADA Public Accommodation Accessibility"; Description = "ADA Title III public accommodation and effective communication baseline requirements." }
    @{ Key = "trade_sanctions_supply_chain"; BodyKey = "trade_authorities"; BodyLabel = "U.S. Trade, Customs, and Sanctions Authorities"; JurisdictionKey = "us_federal_trade"; Label = "Trade, Customs, Sanctions, and Supply Chain"; Description = "Trade, customs, sanctions, forced-labor, and product compliance intake baseline requirements." }
)

function Invoke-ComplianceJson {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Method,

        [Parameter(Mandatory = $true)]
        [string]$Path,

        [object]$Body = $null
    )

    $Uri = "$BaseUrl$Path"
    if ($null -eq $Body) {
        return Invoke-RestMethod -Method $Method -Uri $Uri -Headers $Headers
    }

    return Invoke-RestMethod -Method $Method -Uri $Uri -Headers $Headers -ContentType "application/json" -Body ($Body | ConvertTo-Json -Depth 20)
}

function Ensure-RegulatorySpine {
    $Bodies = Invoke-ComplianceJson -Method Get -Path "/api/governing-bodies"
    $Jurisdictions = Invoke-ComplianceJson -Method Get -Path "/api/jurisdictions"
    $ExistingPrograms = Invoke-ComplianceJson -Method Get -Path "/api/regulatory-programs"

    foreach ($Program in $Programs) {
        $Body = $Bodies | Where-Object { $_.bodyKey -eq $Program.BodyKey } | Select-Object -First 1
        if ($null -eq $Body) {
            if ($DryRun) {
                Write-Host "DRY RUN: would create governing body $($Program.BodyKey)"
                continue
            }
            $Body = Invoke-ComplianceJson -Method Post -Path "/api/governing-bodies" -Body @{
                bodyKey = $Program.BodyKey
                label = $Program.BodyLabel
                description = "Baseline rulepack governing body."
            }
            $Bodies += $Body
        }

        $Jurisdiction = $Jurisdictions | Where-Object { $_.jurisdictionKey -eq $Program.JurisdictionKey } | Select-Object -First 1
        if ($null -eq $Jurisdiction) {
            if ($DryRun) {
                Write-Host "DRY RUN: would create jurisdiction $($Program.JurisdictionKey)"
                continue
            }
            $Jurisdiction = Invoke-ComplianceJson -Method Post -Path "/api/jurisdictions" -Body @{
                governingBodyId = $Body.governingBodyId
                jurisdictionKey = $Program.JurisdictionKey
                label = $Program.Label
                description = $Program.Description
            }
            $Jurisdictions += $Jurisdiction
        }

        $Existing = $ExistingPrograms | Where-Object { $_.programKey -eq $Program.Key } | Select-Object -First 1
        if ($null -ne $Existing) {
            continue
        }

        if ($DryRun) {
            Write-Host "DRY RUN: would create regulatory program $($Program.Key)"
        } else {
            $Created = Invoke-ComplianceJson -Method Post -Path "/api/regulatory-programs" -Body @{
                jurisdictionId = $Jurisdiction.jurisdictionId
                programKey = $Program.Key
                label = $Program.Label
                description = $Program.Description
            }
            $ExistingPrograms += $Created
        }
    }
}

function Import-RulePackBundles {
    $Endpoint = if ($DryRun) { "/api/v1/rule-pack-imports/validate" } else { "/api/v1/rule-pack-imports/publish-draft" }
    foreach ($PackDir in Get-ChildItem -Path $RulePackRoot -Directory | Sort-Object Name) {
        $Form = @{}
        $Index = 0
        foreach ($CsvName in $CsvNames) {
            $Path = Join-Path $PackDir.FullName $CsvName
            if (-not (Test-Path $Path)) {
                throw "Missing $CsvName in $($PackDir.FullName)"
            }
            $Form["file$Index"] = Get-Item $Path
            $Index++
        }

        Write-Host "Importing $($PackDir.Name) via $Endpoint"
        $Result = Invoke-RestMethod -Method Post -Uri "$BaseUrl$Endpoint" -Headers $Headers -Form $Form
        if ($Result.result.issues.Count -gt 0) {
            $Result.result.issues | ConvertTo-Json -Depth 10
            throw "Import validation failed for $($PackDir.Name)"
        }
    }
}

Ensure-RegulatorySpine
Import-RulePackBundles

if ($DryRun) {
    Write-Host "Baseline rule-pack dry-run validation completed."
} else {
    Write-Host "Baseline rule-pack import completed."
}
