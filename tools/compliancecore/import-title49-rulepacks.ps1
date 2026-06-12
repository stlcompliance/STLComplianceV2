param(
    [Parameter(Mandatory = $true)]
    [string]$ComplianceCoreBaseUrl,

    [Parameter(Mandatory = $true)]
    [string]$AccessToken,

    [string]$RulePackRoot = "",

    [switch]$DryRun,

    [switch]$UpdateExistingFacts
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RulePackRoot)) {
    $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $RepoRoot = Resolve-Path (Join-Path $ScriptDir "..\..")
    $RulePackRoot = Join-Path $RepoRoot "root\rulepack\title49"
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
    @{ Key = "dot_part40"; Label = "DOT Part 40 Drug and Alcohol Testing Procedures"; Description = "Procedures for transportation workplace drug and alcohol testing programs." },
    @{ Key = "dot_title49_metadata"; Label = "DOT Title 49 Citation Metadata"; Description = "General Title 49 citation metadata program." },
    @{ Key = "fmcsa_fmcsr"; Label = "FMCSA Federal Motor Carrier Safety Regulations"; Description = "Federal motor carrier safety regulations for motor carrier operations." },
    @{ Key = "fmcsa_passenger"; Label = "FMCSA Passenger Carrier Operations"; Description = "Passenger carrier operational reference program." },
    @{ Key = "fmcsa_household_goods"; Label = "FMCSA Household Goods Consumer Protection"; Description = "Household goods consumer protection reference program." },
    @{ Key = "fmcsa_intermodal"; Label = "FMCSA Intermodal Equipment Provider"; Description = "Intermodal equipment provider reference program." },
    @{ Key = "phmsa_hmr"; Label = "PHMSA Hazardous Materials Regulations"; Description = "Hazardous Materials Regulations in 49 CFR parts 171 through 180." },
    @{ Key = "phmsa_pipeline"; Label = "PHMSA Pipeline Safety"; Description = "Pipeline safety reference program." },
    @{ Key = "fra_rail"; Label = "FRA Rail Safety"; Description = "Federal Railroad Administration safety reference program." },
    @{ Key = "fta_transit"; Label = "FTA Transit Safety"; Description = "Federal Transit Administration safety reference program." },
    @{ Key = "nhtsa_vehicle"; Label = "NHTSA Vehicle Standards"; Description = "National Highway Traffic Safety Administration vehicle standards reference program." },
    @{ Key = "tsa_security"; Label = "TSA Transportation Security"; Description = "Transportation Security Administration security reference program." },
    @{ Key = "stb_surface"; Label = "Surface Transportation Board"; Description = "Surface Transportation Board surface transportation reference program." }
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
    $Body = $Bodies | Where-Object { $_.bodyKey -eq "dot" } | Select-Object -First 1
    if ($null -eq $Body) {
        if ($DryRun) {
            Write-Host "DRY RUN: would create governing body dot"
        } else {
            $Body = Invoke-ComplianceJson -Method Post -Path "/api/governing-bodies" -Body @{
                bodyKey = "dot"
                label = "U.S. Department of Transportation"
                description = "Federal transportation safety and compliance authority."
            }
        }
    }

    if ($DryRun -and $null -eq $Body) {
        Write-Host "DRY RUN: skipping jurisdiction/program creation because dot body does not exist yet."
        return
    }

    $Jurisdictions = Invoke-ComplianceJson -Method Get -Path "/api/jurisdictions"
    $Jurisdiction = $Jurisdictions | Where-Object { $_.jurisdictionKey -eq "us_federal" } | Select-Object -First 1
    if ($null -eq $Jurisdiction) {
        if ($DryRun) {
            Write-Host "DRY RUN: would create jurisdiction us_federal"
        } else {
            $Jurisdiction = Invoke-ComplianceJson -Method Post -Path "/api/jurisdictions" -Body @{
                governingBodyId = $Body.governingBodyId
                jurisdictionKey = "us_federal"
                label = "United States Federal"
                description = "Federal jurisdiction for Title 49 transportation regulations."
            }
        }
    }

    if ($DryRun -and $null -eq $Jurisdiction) {
        Write-Host "DRY RUN: skipping program creation because us_federal jurisdiction does not exist yet."
        return
    }

    $ExistingPrograms = Invoke-ComplianceJson -Method Get -Path "/api/regulatory-programs"
    foreach ($Program in $Programs) {
        $Existing = $ExistingPrograms | Where-Object { $_.programKey -eq $Program.Key } | Select-Object -First 1
        if ($null -ne $Existing) {
            continue
        }

        if ($DryRun) {
            Write-Host "DRY RUN: would create regulatory program $($Program.Key)"
        } else {
            [void](Invoke-ComplianceJson -Method Post -Path "/api/regulatory-programs" -Body @{
                jurisdictionId = $Jurisdiction.jurisdictionId
                programKey = $Program.Key
                label = $Program.Label
                description = $Program.Description
            })
        }
    }
}

function Ensure-FactDefinitions {
    $FactRows = Get-ChildItem -Path $RulePackRoot -Recurse -Filter "rule_fact_requirements.csv" |
        ForEach-Object { Import-Csv -Path $_.FullName } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_.fact_key) }

    $GroupedFacts = $FactRows | Group-Object -Property fact_key
    $ExistingFacts = Invoke-ComplianceJson -Method Get -Path "/api/v1/facts"
    foreach ($Group in $GroupedFacts) {
        $First = $Group.Group | Select-Object -First 1
        $Existing = $ExistingFacts | Where-Object { $_.factKey -eq $First.fact_key } | Select-Object -First 1
        if ($null -eq $Existing) {
            $ValueType = if ([string]::IsNullOrWhiteSpace($First.value_type)) { "boolean" } else { $First.value_type }
            if ($DryRun) {
                Write-Host "DRY RUN: would create fact $($First.fact_key)"
            } else {
                [void](Invoke-ComplianceJson -Method Post -Path "/api/v1/facts" -Body @{
                    factKey = $First.fact_key
                    label = $First.label
                    description = $First.description
                    valueType = $ValueType
                })
            }
            continue
        }

        if ($UpdateExistingFacts) {
            $ValueType = if ([string]::IsNullOrWhiteSpace($First.value_type)) { "boolean" } else { $First.value_type }
            if ($DryRun) {
                Write-Host "DRY RUN: would update fact $($First.fact_key)"
            } else {
                [void](Invoke-ComplianceJson -Method Patch -Path "/api/v1/facts/$($First.fact_key)" -Body @{
                    label = $First.label
                    description = $First.description
                    valueType = $ValueType
                    isActive = $true
                })
            }
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
Ensure-FactDefinitions
Import-RulePackBundles

if ($DryRun) {
    Write-Host "Title 49 dry-run validation completed."
} else {
    Write-Host "Title 49 rule-pack import completed."
}
