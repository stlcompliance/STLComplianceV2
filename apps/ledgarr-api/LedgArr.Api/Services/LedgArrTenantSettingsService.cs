using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using LedgArr.Api.Data;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace LedgArr.Api.Services;

public sealed class LedgArrTenantSettingsService(LedgArrDbContext db)
{
    private const string ProductKey = "ledgarr";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };

    private static readonly IReadOnlyDictionary<string, SectionDefinition> SectionDefinitions =
        new Dictionary<string, SectionDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["generalLedger"] = new("General Ledger", "Configure base accounting mode, currency, period close, and core control accounts.", typeof(GeneralLedgerSettingsSection), LedgArrSettingsPermissions.SettingsView, LedgArrSettingsPermissions.SettingsManage),
            ["legalEntities"] = new("Legal Entities", "Configure LedgArr-owned financial legal entities and intercompany rules without modeling governing bodies.", typeof(LegalEntitySettingsSection), LedgArrSettingsPermissions.LegalEntitiesView, LedgArrSettingsPermissions.LegalEntitiesManage),
            ["chartOfAccounts"] = new("Chart Of Accounts", "Define account numbering, approval, and default account mappings for LedgArr-owned accounts.", typeof(ChartOfAccountsSettingsSection), LedgArrSettingsPermissions.ChartOfAccountsView, LedgArrSettingsPermissions.ChartOfAccountsManage),
            ["dimensions"] = new("Dimensions", "Control required financial dimensions and cross-product reference mapping behavior.", typeof(DimensionSettingsSection), LedgArrSettingsPermissions.SettingsView, LedgArrSettingsPermissions.SettingsManage),
            ["postingSources"] = new("Posting Sources", "Enable or hold finance packets from source products while preserving source-of-truth ownership.", typeof(PostingSourceSettingsSection), LedgArrSettingsPermissions.PostingRulesView, LedgArrSettingsPermissions.PostingRulesManage),
            ["accountsPayable"] = new("Accounts Payable", "Control AP validation, approvals, evidence, and control account behavior.", typeof(AccountsPayableSettingsSection), LedgArrSettingsPermissions.SettingsView, LedgArrSettingsPermissions.SettingsManage),
            ["accountsReceivable"] = new("Accounts Receivable", "Control AR numbering, approvals, credit behavior, and portal exposure.", typeof(AccountsReceivableSettingsSection), LedgArrSettingsPermissions.SettingsView, LedgArrSettingsPermissions.SettingsManage),
            ["inventoryAccounting"] = new("Inventory Accounting", "Configure valuation, capitalization, and financial approval rules for inventory accounting.", typeof(InventoryAccountingSettingsSection), LedgArrSettingsPermissions.SettingsView, LedgArrSettingsPermissions.SettingsManage),
            ["fixedAssets"] = new("Fixed Assets", "Configure capitalization, depreciation, and MaintainArr asset accounting behavior.", typeof(FixedAssetSettingsSection), LedgArrSettingsPermissions.SettingsView, LedgArrSettingsPermissions.SettingsManage),
            ["tax"] = new("Tax", "Configure tax accounting, provider mode, filing ownership, and RecordArr evidence references.", typeof(TaxSettingsSection), LedgArrSettingsPermissions.SettingsView, LedgArrSettingsPermissions.SettingsManage),
            ["banking"] = new("Banking", "Configure cash, reconciliation, payment methods, and bank export/import behavior.", typeof(BankingSettingsSection), LedgArrSettingsPermissions.SettingsView, LedgArrSettingsPermissions.SettingsManage),
            ["intercompany"] = new("Intercompany", "Configure due to / due from behavior, settlement rules, and elimination references.", typeof(IntercompanySettingsSection), LedgArrSettingsPermissions.SettingsView, LedgArrSettingsPermissions.SettingsManage),
            ["approvals"] = new("Approvals", "Configure approval policies, segregation of duties, and required reason-code categories.", typeof(ApprovalSettingsSection), LedgArrSettingsPermissions.SettingsView, LedgArrSettingsPermissions.SettingsManage),
            ["close"] = new("Close", "Configure monthly close, reopen, audit packet export, and manual posting evidence requirements.", typeof(CloseSettingsSection), LedgArrSettingsPermissions.PeriodCloseView, LedgArrSettingsPermissions.PeriodCloseManage),
            ["integrations"] = new("Integrations", "Configure LedgArr operating mode and external ERP integration behavior.", typeof(IntegrationSettingsSection), LedgArrSettingsPermissions.IntegrationsView, LedgArrSettingsPermissions.IntegrationsManage),
            ["reporting"] = new("Reporting", "Configure financial reporting defaults and profitability reporting toggles.", typeof(ReportingSettingsSection), LedgArrSettingsPermissions.SettingsView, LedgArrSettingsPermissions.SettingsManage),
            ["evidence"] = new("Evidence", "Configure RecordArr document retention and attachment requirements for financial execution.", typeof(EvidenceSettingsSection), LedgArrSettingsPermissions.SettingsView, LedgArrSettingsPermissions.SettingsManage),
        };

    private static readonly IReadOnlyDictionary<string, HashSet<string>> HighImpactFields =
        new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["generalLedger"] = new(StringComparer.OrdinalIgnoreCase) { "accountingBasis", "baseCurrency", "fiscalYearStartMonth", "periodCloseMode", "allowPostingToClosedPeriods" },
            ["legalEntities"] = new(StringComparer.OrdinalIgnoreCase) { "requireLegalEntityOnEveryPosting", "intercompanyEnabled" },
            ["postingSources"] = new(StringComparer.OrdinalIgnoreCase) { "inventoryValuationMethod" },
            ["inventoryAccounting"] = new(StringComparer.OrdinalIgnoreCase) { "inventoryValuationMethod" },
            ["tax"] = new(StringComparer.OrdinalIgnoreCase) { "taxCalculationMode" },
            ["banking"] = new(StringComparer.OrdinalIgnoreCase) { "paymentBatchApprovalThreshold" },
            ["intercompany"] = new(StringComparer.OrdinalIgnoreCase) { "intercompanyEnabled", "intercompanyMarkupPercent" },
            ["approvals"] = new(StringComparer.OrdinalIgnoreCase) { "requireApprovalForManualJournals", "requireApprovalForPeriodClose" },
            ["close"] = new(StringComparer.OrdinalIgnoreCase) { "allowReopenPeriod", "immutableAuditLogEnabled", "manualPostingEvidenceThreshold" },
            ["integrations"] = new(StringComparer.OrdinalIgnoreCase) { "ledgarrOperatingMode", "externalErpProvider" },
            ["evidence"] = new(StringComparer.OrdinalIgnoreCase) { "requireAttachmentForManualJournalAboveThreshold", "manualJournalAttachmentThreshold" },
        };

    public async Task<LedgArrTenantSettingsEnvelope> GetTenantSettingsAsync(Guid tenantId, ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        EnsureCanViewAll(principal);
        await EnsureDefaultsExistAsync(tenantId, cancellationToken);
        var sections = new List<LedgArrTenantSettingsSectionDto>();
        foreach (var sectionKey in SectionDefinitions.Keys)
        {
            if (!CanViewSection(principal, sectionKey))
            {
                continue;
            }

            sections.Add(await BuildSectionDtoAsync(tenantId, sectionKey, cancellationToken));
        }

        return new LedgArrTenantSettingsEnvelope(1, sections);
    }

    public async Task<LedgArrTenantSettingsSectionDto> GetTenantSettingsSectionAsync(Guid tenantId, string sectionKey, ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        EnsureCanViewSection(principal, sectionKey);
        await EnsureDefaultsExistAsync(tenantId, cancellationToken);
        return await BuildSectionDtoAsync(tenantId, sectionKey, cancellationToken);
    }

    public async Task<LedgArrTenantSettingsOptionsResponse> GetOptionsAsync(Guid tenantId, ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        EnsureCanViewAll(principal);
        await EnsureDefaultsExistAsync(tenantId, cancellationToken);

        var accounts = await db.GLAccounts
            .Where(account => account.TenantId == tenantId && account.Status == "active")
            .OrderBy(account => account.AccountCode)
            .Select(account => new SettingsOptionReference(account.AccountCode, $"{account.AccountCode} - {account.Name}", "ledgarr", "glAccount", account.AccountCode, account.Status))
            .ToListAsync(cancellationToken);

        var legalEntities = await db.FinancialLegalEntities
            .Where(entity => entity.TenantId == tenantId && entity.Status == "active")
            .OrderBy(entity => entity.EntityCode)
            .Select(entity => new SettingsOptionReference(entity.EntityCode, $"{entity.EntityCode} - {entity.DisplayName}", "ledgarr", "financialLegalEntity", entity.EntityCode, entity.Status))
            .ToListAsync(cancellationToken);

        var currencies = await db.Currencies
            .OrderBy(currency => currency.CurrencyCode)
            .Select(currency => new NamedOption(currency.CurrencyCode, $"{currency.CurrencyCode} - {currency.DisplayName}"))
            .ToListAsync(cancellationToken);

        if (currencies.Count == 0)
        {
            currencies.Add(new NamedOption("USD", "USD - US Dollar"));
        }

        var sourceProducts = new[]
        {
            new NamedOption("maintainarr", "MaintainArr"),
            new NamedOption("supplyarr", "SupplyArr"),
            new NamedOption("loadarr", "LoadArr"),
            new NamedOption("routarr", "RoutArr"),
            new NamedOption("ordarr", "OrdArr"),
            new NamedOption("customarr", "CustomArr"),
            new NamedOption("staffarr", "StaffArr"),
            new NamedOption("recordarr", "RecordArr"),
        };

        var crossProductReferences = await db.FinancialPacketSourceRefs
            .Where(reference => reference.TenantId == tenantId && reference.ProductKey != ProductKey)
            .OrderBy(reference => reference.ProductKey)
            .ThenBy(reference => reference.SourceRecordType)
            .ThenBy(reference => reference.SourceRecordDisplayName)
            .Select(reference => new SettingsOptionReference(
                $"{reference.ProductKey}|{reference.SourceRecordType}|{reference.SourceRecordId}",
                $"{reference.SourceRecordDisplayName} ({reference.ProductKey}:{reference.SourceRecordType})",
                reference.ProductKey,
                reference.SourceRecordType,
                reference.SourceRecordId,
                "active"))
            .Distinct()
            .ToListAsync(cancellationToken);

        return new LedgArrTenantSettingsOptionsResponse(
            accounts,
            legalEntities,
            currencies,
            sourceProducts,
            SectionDefinitions.Keys.OrderBy(key => key).Select(key => new NamedOption(key, SectionDefinitions[key].DisplayName)).ToArray(),
            crossProductReferences);
    }

    public async Task<LedgArrTenantSettingsValidationResponse> ValidateTenantSettingsSectionAsync(Guid tenantId, string sectionKey, LedgArrTenantSettingsUpdateRequest request, ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        EnsureCanManageSection(principal, sectionKey);
        await EnsureDefaultsExistAsync(tenantId, cancellationToken);
        return await ValidateSectionCoreAsync(tenantId, NormalizeSectionKey(sectionKey), request, cancellationToken);
    }

    public async Task<LedgArrTenantSettingsSectionDto> UpdateTenantSettingsSectionAsync(Guid tenantId, string sectionKey, LedgArrTenantSettingsUpdateRequest request, ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        sectionKey = NormalizeSectionKey(sectionKey);
        EnsureCanManageSection(principal, sectionKey);
        await EnsureDefaultsExistAsync(tenantId, cancellationToken);

        var validation = await ValidateSectionCoreAsync(tenantId, sectionKey, request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new StlApiException("ledgarr.settings.validation_failed", "LedgArr tenant settings validation failed.", 400);
        }

        var definition = SectionDefinitions[sectionKey];
        var entity = await db.LedgArrTenantSettingSections.FirstOrDefaultAsync(
            row => row.TenantId == tenantId && row.SectionKey == sectionKey,
            cancellationToken);

        if (entity is not null
            && !string.IsNullOrWhiteSpace(request.ExpectedRowVersion)
            && !string.Equals(entity.RowVersion, request.ExpectedRowVersion, StringComparison.Ordinal))
        {
            throw new StlApiException("ledgarr.settings.conflict", "LedgArr tenant settings were modified by another administrator. Refresh and retry.", 409);
        }

        var actorPersonId = principal.GetPersonId();
        var currentJson = entity?.SettingsJson;
        var currentValue = entity is null
            ? CreateDefaultSectionObject(sectionKey)
            : DeserializeSectionValue(definition.SectionType, entity.SettingsJson);
        var requestedValue = DeserializeSectionValue(definition.SectionType, request.Value);
        var diff = BuildDiff(currentValue, requestedValue);
        var changedFieldKeys = diff.Keys.ToArray();
        var requiresReason = changedFieldKeys.Any(key => IsHighImpactField(sectionKey, key));
        if (requiresReason && string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new StlApiException("ledgarr.settings.reason_required", "A change reason is required for high-impact LedgArr settings updates.", 400);
        }

        var nextJson = JsonSerializer.Serialize(requestedValue, definition.SectionType, JsonOptions);
        if (entity is null)
        {
            entity = new LedgArrTenantSettingSection
            {
                TenantId = tenantId,
                SectionKey = sectionKey,
                SettingsVersion = 1,
                SettingsJson = nextJson,
                CreatedByPersonId = actorPersonId,
                UpdatedByPersonId = actorPersonId,
                RowVersion = Guid.NewGuid().ToString("N"),
            };
            db.LedgArrTenantSettingSections.Add(entity);
        }
        else
        {
            entity.SettingsVersion = 1;
            entity.SettingsJson = nextJson;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            entity.UpdatedByPersonId = actorPersonId;
            entity.RowVersion = Guid.NewGuid().ToString("N");
        }

        db.LedgArrTenantSettingsAudits.Add(new LedgArrTenantSettingsAudit
        {
            TenantId = tenantId,
            SectionKey = sectionKey,
            ChangedByPersonId = actorPersonId,
            ChangeReason = request.Reason,
            BeforeJson = currentJson,
            AfterJson = nextJson,
            DiffJson = JsonSerializer.Serialize(diff, JsonOptions),
            CorrelationId = principal.FindFirstValue(StlClaimTypes.CorrelationId),
        });

        db.FinancialAuditEvents.Add(new FinancialAuditEvent
        {
            TenantId = tenantId,
            Action = requiresReason ? "ledgarr.tenantSettings.highImpactSettingChanged" : "ledgarr.tenantSettings.sectionUpdated",
            TargetType = "tenant_settings_section",
            TargetId = sectionKey,
            ActorId = actorPersonId.ToString("D"),
            Summary = $"Updated LedgArr tenant settings section {sectionKey}.",
            Reason = request.Reason,
        });

        await db.SaveChangesAsync(cancellationToken);
        return await BuildSectionDtoAsync(tenantId, sectionKey, cancellationToken);
    }

    public async Task<LedgArrTenantSettingsSectionDto> ResetTenantSettingsSectionToDefaultAsync(Guid tenantId, string sectionKey, ClaimsPrincipal principal, string? reason, CancellationToken cancellationToken = default)
    {
        sectionKey = NormalizeSectionKey(sectionKey);
        EnsureCanManageSection(principal, sectionKey);
        await EnsureDefaultsExistAsync(tenantId, cancellationToken);

        var definition = SectionDefinitions[sectionKey];
        var actorPersonId = principal.GetPersonId();
        var entity = await db.LedgArrTenantSettingSections.FirstOrDefaultAsync(
            row => row.TenantId == tenantId && row.SectionKey == sectionKey,
            cancellationToken);
        var previousJson = entity?.SettingsJson;
        var defaultValue = CreateDefaultSectionObject(sectionKey);
        var defaultJson = JsonSerializer.Serialize(defaultValue, definition.SectionType, JsonOptions);
        var diff = entity is null
            ? new Dictionary<string, SettingDiffItem>(StringComparer.OrdinalIgnoreCase)
            : BuildDiff(DeserializeSectionValue(definition.SectionType, entity.SettingsJson), defaultValue);

        if (entity is null)
        {
            entity = new LedgArrTenantSettingSection
            {
                TenantId = tenantId,
                SectionKey = sectionKey,
                SettingsVersion = 1,
                SettingsJson = defaultJson,
                CreatedByPersonId = actorPersonId,
                UpdatedByPersonId = actorPersonId,
                RowVersion = Guid.NewGuid().ToString("N"),
            };
            db.LedgArrTenantSettingSections.Add(entity);
        }
        else
        {
            entity.SettingsJson = defaultJson;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            entity.UpdatedByPersonId = actorPersonId;
            entity.RowVersion = Guid.NewGuid().ToString("N");
        }

        db.LedgArrTenantSettingsAudits.Add(new LedgArrTenantSettingsAudit
        {
            TenantId = tenantId,
            SectionKey = sectionKey,
            ChangedByPersonId = actorPersonId,
            ChangeReason = string.IsNullOrWhiteSpace(reason) ? "Reset to LedgArr defaults." : reason.Trim(),
            BeforeJson = previousJson,
            AfterJson = defaultJson,
            DiffJson = JsonSerializer.Serialize(diff, JsonOptions),
            CorrelationId = principal.FindFirstValue(StlClaimTypes.CorrelationId),
        });

        db.FinancialAuditEvents.Add(new FinancialAuditEvent
        {
            TenantId = tenantId,
            Action = "ledgarr.tenantSettings.sectionReset",
            TargetType = "tenant_settings_section",
            TargetId = sectionKey,
            ActorId = actorPersonId.ToString("D"),
            Summary = $"Reset LedgArr tenant settings section {sectionKey} to defaults.",
            Reason = reason,
        });

        await db.SaveChangesAsync(cancellationToken);
        return await BuildSectionDtoAsync(tenantId, sectionKey, cancellationToken);
    }

    public async Task<LedgArrTenantSettingsAuditResponse> GetTenantSettingsAuditAsync(Guid tenantId, string sectionKey, ClaimsPrincipal principal, int skip, int take, CancellationToken cancellationToken = default)
    {
        sectionKey = NormalizeSectionKey(sectionKey);
        EnsureCanViewSection(principal, sectionKey);
        skip = Math.Max(0, skip);
        take = Math.Clamp(take, 1, 100);

        var items = await db.LedgArrTenantSettingsAudits
            .Where(item => item.TenantId == tenantId && item.SectionKey == sectionKey)
            .OrderByDescending(item => item.ChangedAtUtc)
            .Skip(skip)
            .Take(take)
            .Select(item => new LedgArrTenantSettingsAuditItem(
                item.Id,
                item.SectionKey,
                item.ChangedAtUtc,
                item.ChangedByPersonId.ToString("D"),
                item.ChangeReason,
                item.DiffJson,
                item.CorrelationId))
            .ToListAsync(cancellationToken);

        return new LedgArrTenantSettingsAuditResponse(items);
    }

    public LedgArrEffectiveSettingsSnapshot GetDefaultTenantSettings() =>
        new(
            (GeneralLedgerSettingsSection)CreateDefaultSectionObject("generalLedger"),
            (LegalEntitySettingsSection)CreateDefaultSectionObject("legalEntities"),
            (ChartOfAccountsSettingsSection)CreateDefaultSectionObject("chartOfAccounts"),
            (DimensionSettingsSection)CreateDefaultSectionObject("dimensions"),
            (PostingSourceSettingsSection)CreateDefaultSectionObject("postingSources"),
            (AccountsPayableSettingsSection)CreateDefaultSectionObject("accountsPayable"),
            (AccountsReceivableSettingsSection)CreateDefaultSectionObject("accountsReceivable"),
            (InventoryAccountingSettingsSection)CreateDefaultSectionObject("inventoryAccounting"),
            (FixedAssetSettingsSection)CreateDefaultSectionObject("fixedAssets"),
            (TaxSettingsSection)CreateDefaultSectionObject("tax"),
            (BankingSettingsSection)CreateDefaultSectionObject("banking"),
            (IntercompanySettingsSection)CreateDefaultSectionObject("intercompany"),
            (ApprovalSettingsSection)CreateDefaultSectionObject("approvals"),
            (CloseSettingsSection)CreateDefaultSectionObject("close"),
            (IntegrationSettingsSection)CreateDefaultSectionObject("integrations"),
            (ReportingSettingsSection)CreateDefaultSectionObject("reporting"),
            (EvidenceSettingsSection)CreateDefaultSectionObject("evidence"));

    public async Task<LedgArrEffectiveSettingsSnapshot> GetEffectiveSettingsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        await EnsureDefaultsExistAsync(tenantId, cancellationToken);
        return new LedgArrEffectiveSettingsSnapshot(
            await GetEffectiveSectionAsync<GeneralLedgerSettingsSection>(tenantId, "generalLedger", cancellationToken),
            await GetEffectiveSectionAsync<LegalEntitySettingsSection>(tenantId, "legalEntities", cancellationToken),
            await GetEffectiveSectionAsync<ChartOfAccountsSettingsSection>(tenantId, "chartOfAccounts", cancellationToken),
            await GetEffectiveSectionAsync<DimensionSettingsSection>(tenantId, "dimensions", cancellationToken),
            await GetEffectiveSectionAsync<PostingSourceSettingsSection>(tenantId, "postingSources", cancellationToken),
            await GetEffectiveSectionAsync<AccountsPayableSettingsSection>(tenantId, "accountsPayable", cancellationToken),
            await GetEffectiveSectionAsync<AccountsReceivableSettingsSection>(tenantId, "accountsReceivable", cancellationToken),
            await GetEffectiveSectionAsync<InventoryAccountingSettingsSection>(tenantId, "inventoryAccounting", cancellationToken),
            await GetEffectiveSectionAsync<FixedAssetSettingsSection>(tenantId, "fixedAssets", cancellationToken),
            await GetEffectiveSectionAsync<TaxSettingsSection>(tenantId, "tax", cancellationToken),
            await GetEffectiveSectionAsync<BankingSettingsSection>(tenantId, "banking", cancellationToken),
            await GetEffectiveSectionAsync<IntercompanySettingsSection>(tenantId, "intercompany", cancellationToken),
            await GetEffectiveSectionAsync<ApprovalSettingsSection>(tenantId, "approvals", cancellationToken),
            await GetEffectiveSectionAsync<CloseSettingsSection>(tenantId, "close", cancellationToken),
            await GetEffectiveSectionAsync<IntegrationSettingsSection>(tenantId, "integrations", cancellationToken),
            await GetEffectiveSectionAsync<ReportingSettingsSection>(tenantId, "reporting", cancellationToken),
            await GetEffectiveSectionAsync<EvidenceSettingsSection>(tenantId, "evidence", cancellationToken));
    }

    public static bool IsSourceProductPostingEnabled(LedgArrEffectiveSettingsSnapshot settings, string sourceProductKey) =>
        sourceProductKey.ToLowerInvariant() switch
        {
            "maintainarr" => settings.PostingSources.MaintainArr.PostWorkOrderLaborCosts
                || settings.PostingSources.MaintainArr.PostPartsConsumption
                || settings.PostingSources.MaintainArr.PostOutsideVendorRepairInvoices,
            "supplyarr" => settings.PostingSources.SupplyArr.PostPurchaseOrders || settings.PostingSources.SupplyArr.PostVendorBills,
            "loadarr" => settings.PostingSources.LoadArr.PostInventoryReceipts
                || settings.PostingSources.LoadArr.PostInventoryAdjustments
                || settings.PostingSources.LoadArr.PostInventoryTransfers,
            "routarr" => settings.PostingSources.RoutArr.PostFreightAccruals || settings.PostingSources.RoutArr.PostCarrierPayables,
            "ordarr" => settings.PostingSources.OrdArr.PostCustomerInvoices || settings.PostingSources.OrdArr.PostRevenue,
            "staffarr" => settings.PostingSources.StaffArr.PostPayrollExportPackets || settings.PostingSources.StaffArr.PostExpenseReimbursements,
            "customarr" => true,
            _ => true,
        };

    private async Task<TSection> GetEffectiveSectionAsync<TSection>(Guid tenantId, string sectionKey, CancellationToken cancellationToken)
        where TSection : class
    {
        var definition = SectionDefinitions[sectionKey];
        var row = await db.LedgArrTenantSettingSections.FirstOrDefaultAsync(
            item => item.TenantId == tenantId && item.SectionKey == sectionKey,
            cancellationToken);
        if (row is null)
        {
            return (TSection)CreateDefaultSectionObject(sectionKey);
        }

        return (TSection)DeserializeSectionValue(definition.SectionType, row.SettingsJson);
    }

    private async Task EnsureDefaultsExistAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        if (!await db.Currencies.AnyAsync(currency => currency.CurrencyCode == "USD", cancellationToken))
        {
            db.Currencies.Add(new Currency { CurrencyCode = "USD", DisplayName = "US Dollar", MinorUnits = 2 });
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<LedgArrTenantSettingsSectionDto> BuildSectionDtoAsync(Guid tenantId, string sectionKey, CancellationToken cancellationToken)
    {
        var definition = SectionDefinitions[sectionKey];
        var row = await db.LedgArrTenantSettingSections.FirstOrDefaultAsync(
            item => item.TenantId == tenantId && item.SectionKey == sectionKey,
            cancellationToken);
        var value = row is null
            ? CreateDefaultSectionObject(sectionKey)
            : DeserializeSectionValue(definition.SectionType, row.SettingsJson);
        return new LedgArrTenantSettingsSectionDto(
            sectionKey,
            definition.DisplayName,
            definition.Description,
            row?.RowVersion ?? "default",
            value,
            HighImpactFields.TryGetValue(sectionKey, out var highImpactFields) ? highImpactFields.OrderBy(item => item).ToArray() : []);
    }

    private async Task<LedgArrTenantSettingsValidationResponse> ValidateSectionCoreAsync(Guid tenantId, string sectionKey, LedgArrTenantSettingsUpdateRequest request, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var definition = SectionDefinitions[sectionKey];
        object typedValue;
        try
        {
            typedValue = DeserializeSectionValue(definition.SectionType, request.Value);
        }
        catch (Exception ex)
        {
            return new LedgArrTenantSettingsValidationResponse(false, new Dictionary<string, string[]>
            {
                ["section"] = [$"Unable to parse {sectionKey} settings payload: {ex.Message}"],
            });
        }

        var accounts = await db.GLAccounts.Where(account => account.TenantId == tenantId && account.Status == "active").ToListAsync(cancellationToken);
        var entities = await db.FinancialLegalEntities.Where(entity => entity.TenantId == tenantId && entity.Status == "active").ToListAsync(cancellationToken);
        var supportedCurrencies = await db.Currencies.Select(currency => currency.CurrencyCode).ToListAsync(cancellationToken);
        if (supportedCurrencies.Count == 0)
        {
            supportedCurrencies.Add("USD");
        }

        var validator = new SectionValidator(errors, accounts, entities, supportedCurrencies);
        switch (typedValue)
        {
            case GeneralLedgerSettingsSection gl:
                ValidateGeneralLedger(gl, validator);
                break;
            case LegalEntitySettingsSection legal:
                ValidateLegalEntities(legal, validator);
                break;
            case ChartOfAccountsSettingsSection chart:
                ValidateChartOfAccounts(chart, validator);
                break;
            case DimensionSettingsSection dimensions:
                ValidateDimensions(dimensions, validator);
                break;
            case PostingSourceSettingsSection postingSources:
                ValidatePostingSources(postingSources, validator);
                break;
            case AccountsPayableSettingsSection ap:
                ValidateAccountsPayable(ap, validator);
                break;
            case AccountsReceivableSettingsSection ar:
                ValidateAccountsReceivable(ar, validator);
                break;
            case InventoryAccountingSettingsSection inventory:
                ValidateInventoryAccounting(inventory, validator);
                break;
            case FixedAssetSettingsSection fixedAssets:
                ValidateFixedAssets(fixedAssets, validator);
                break;
            case TaxSettingsSection tax:
                ValidateTax(tax, validator);
                break;
            case BankingSettingsSection banking:
                ValidateBanking(banking, validator);
                break;
            case IntercompanySettingsSection intercompany:
                ValidateIntercompany(intercompany, validator);
                break;
            case ApprovalSettingsSection approvals:
                ValidateApprovals(approvals, validator);
                break;
            case CloseSettingsSection close:
                ValidateClose(close, validator);
                break;
            case IntegrationSettingsSection integrations:
                ValidateIntegrations(integrations, validator);
                break;
            case ReportingSettingsSection reporting:
                ValidateReporting(reporting, validator);
                break;
            case EvidenceSettingsSection evidence:
                ValidateEvidence(evidence, validator);
                break;
        }

        return new LedgArrTenantSettingsValidationResponse(errors.Count == 0, errors);
    }

    private static void ValidateGeneralLedger(GeneralLedgerSettingsSection section, SectionValidator validator)
    {
        validator.Enum(nameof(section.AccountingBasis), section.AccountingBasis, "accrual", "cash", "hybrid");
        validator.Currency(nameof(section.BaseCurrency), section.BaseCurrency);
        validator.Currency(nameof(section.ReportingCurrency), section.ReportingCurrency);
        validator.Enum(nameof(section.ExchangeRateProvider), section.ExchangeRateProvider, "manual", "ecb", "xe", "none");
        validator.Enum(nameof(section.ExchangeRateLockPolicy), section.ExchangeRateLockPolicy, "allowChanges", "lockOnPreview", "lockOnPost");
        validator.IntRange(nameof(section.FiscalYearStartMonth), section.FiscalYearStartMonth, 1, 12);
        validator.Enum(nameof(section.FiscalCalendarType), section.FiscalCalendarType, "calendarMonth", "fourFourFive", "custom");
        validator.Enum(nameof(section.PeriodCloseMode), section.PeriodCloseMode, "softCloseThenHardClose", "hardCloseOnly", "manual");
        validator.Account(nameof(section.DefaultRetainedEarningsAccountRef), section.DefaultRetainedEarningsAccountRef);
        validator.Account(nameof(section.DefaultSuspenseAccountRef), section.DefaultSuspenseAccountRef);
        validator.Account(nameof(section.DefaultRoundingAccountRef), section.DefaultRoundingAccountRef);
        validator.Account(nameof(section.DefaultCurrencyGainAccountRef), section.DefaultCurrencyGainAccountRef);
        validator.Account(nameof(section.DefaultCurrencyLossAccountRef), section.DefaultCurrencyLossAccountRef);
    }

    private static void ValidateLegalEntities(LegalEntitySettingsSection section, SectionValidator validator)
    {
        validator.LegalEntity(nameof(section.DefaultLegalEntityRef), section.DefaultLegalEntityRef);
        validator.Enum(nameof(section.IntercompanyBalancingMode), section.IntercompanyBalancingMode, "dueToDueFrom", "clearing", "manual");
        validator.LegalEntity(nameof(section.ConsolidationEliminationEntityRef), section.ConsolidationEliminationEntityRef);
        validator.Currency(nameof(section.ConsolidationCurrency), section.ConsolidationCurrency);
        validator.Account(nameof(section.DueToAccountRef), section.DueToAccountRef);
        validator.Account(nameof(section.DueFromAccountRef), section.DueFromAccountRef);
    }

    private static void ValidateChartOfAccounts(ChartOfAccountsSettingsSection section, SectionValidator validator)
    {
        validator.Enum(nameof(section.ChartOfAccountsTemplate), section.ChartOfAccountsTemplate, "stlStandard", "manufacturing", "logistics", "service");
        validator.Enum(nameof(section.AccountNumberingScheme), section.AccountNumberingScheme, "numeric", "segmented");
        validator.IntRange(nameof(section.AccountNumberLength), section.AccountNumberLength, 3, 32);
        validator.Account(nameof(section.DefaultInventoryAssetAccountRef), section.DefaultInventoryAssetAccountRef);
        validator.Account(nameof(section.DefaultApControlAccountRef), section.DefaultApControlAccountRef);
        validator.Account(nameof(section.DefaultArControlAccountRef), section.DefaultArControlAccountRef);
        validator.Account(nameof(section.DefaultPayrollLiabilityAccountRef), section.DefaultPayrollLiabilityAccountRef);
        validator.Account(nameof(section.DefaultSalesTaxPayableAccountRef), section.DefaultSalesTaxPayableAccountRef);
        validator.Account(nameof(section.DefaultUseTaxPayableAccountRef), section.DefaultUseTaxPayableAccountRef);
        validator.Account(nameof(section.DefaultFreightAccrualAccountRef), section.DefaultFreightAccrualAccountRef);
        validator.Account(nameof(section.DefaultWorkOrderExpenseAccountRef), section.DefaultWorkOrderExpenseAccountRef);
        validator.Account(nameof(section.DefaultPartsExpenseAccountRef), section.DefaultPartsExpenseAccountRef);
        validator.Account(nameof(section.DefaultFuelExpenseAccountRef), section.DefaultFuelExpenseAccountRef);
        validator.Account(nameof(section.DefaultMaintenanceLaborExpenseAccountRef), section.DefaultMaintenanceLaborExpenseAccountRef);
        validator.Account(nameof(section.DefaultWarrantyRecoveryAccountRef), section.DefaultWarrantyRecoveryAccountRef);
        validator.Account(nameof(section.DefaultCustomerDepositsAccountRef), section.DefaultCustomerDepositsAccountRef);
        validator.Account(nameof(section.DefaultVendorDepositsAccountRef), section.DefaultVendorDepositsAccountRef);
    }

    private static void ValidateDimensions(DimensionSettingsSection section, SectionValidator validator)
    {
        validator.ReferenceList(nameof(section.FallbackDimensions), section.FallbackDimensions);
        validator.ReferenceList(nameof(section.CrossProductDimensionMappingRules), section.CrossProductDimensionMappingRules.SelectMany(rule => rule.ReferenceValues));
        validator.EnumList(nameof(section.EnabledDimensions), section.EnabledDimensions,
            "legalEntity", "costCenter", "department", "site", "location", "customer", "vendor", "order", "purchaseOrder", "workOrder", "asset", "route", "trip", "load", "warehouse", "project", "productLine", "serviceLine", "person", "equipmentClass", "revenueCategory", "expenseCategory");
    }

    private static void ValidatePostingSources(PostingSourceSettingsSection section, SectionValidator validator)
    {
        validator.Account(nameof(section.MaintainArr.DefaultRepairExpenseAccountRef), section.MaintainArr.DefaultRepairExpenseAccountRef);
        validator.Account(nameof(section.MaintainArr.DefaultPmExpenseAccountRef), section.MaintainArr.DefaultPmExpenseAccountRef);
        validator.Account(nameof(section.MaintainArr.DefaultPartsExpenseAccountRef), section.MaintainArr.DefaultPartsExpenseAccountRef);
        validator.Account(nameof(section.MaintainArr.DefaultWarrantyRecoveryAccountRef), section.MaintainArr.DefaultWarrantyRecoveryAccountRef);
        validator.Account(nameof(section.SupplyArr.DefaultApAccountRef), section.SupplyArr.DefaultApAccountRef);
        validator.Account(nameof(section.SupplyArr.DefaultPurchaseAccrualAccountRef), section.SupplyArr.DefaultPurchaseAccrualAccountRef);
        validator.Account(nameof(section.SupplyArr.DefaultVendorDepositAccountRef), section.SupplyArr.DefaultVendorDepositAccountRef);
        validator.Account(nameof(section.SupplyArr.DefaultPriceVarianceAccountRef), section.SupplyArr.DefaultPriceVarianceAccountRef);
        validator.Enum(nameof(section.LoadArr.InventoryValuationMethod), section.LoadArr.InventoryValuationMethod, "fifo", "weightedAverage", "standardCost", "specificIdentification");
        validator.Account(nameof(section.LoadArr.DefaultInventoryAssetAccountRef), section.LoadArr.DefaultInventoryAssetAccountRef);
        validator.Account(nameof(section.LoadArr.DefaultInventoryVarianceAccountRef), section.LoadArr.DefaultInventoryVarianceAccountRef);
        validator.Account(nameof(section.LoadArr.DefaultReceivingAccrualAccountRef), section.LoadArr.DefaultReceivingAccrualAccountRef);
        validator.Account(nameof(section.RoutArr.DefaultFreightExpenseAccountRef), section.RoutArr.DefaultFreightExpenseAccountRef);
        validator.Account(nameof(section.RoutArr.DefaultFreightRevenueAccountRef), section.RoutArr.DefaultFreightRevenueAccountRef);
        validator.Account(nameof(section.RoutArr.DefaultAccessorialRevenueAccountRef), section.RoutArr.DefaultAccessorialRevenueAccountRef);
        validator.Account(nameof(section.RoutArr.DefaultDetentionRevenueAccountRef), section.RoutArr.DefaultDetentionRevenueAccountRef);
        validator.Account(nameof(section.OrdArr.DefaultArAccountRef), section.OrdArr.DefaultArAccountRef);
        validator.Account(nameof(section.OrdArr.DefaultRevenueAccountRef), section.OrdArr.DefaultRevenueAccountRef);
        validator.Account(nameof(section.OrdArr.DefaultDiscountAccountRef), section.OrdArr.DefaultDiscountAccountRef);
        validator.Account(nameof(section.OrdArr.DefaultReturnsAccountRef), section.OrdArr.DefaultReturnsAccountRef);
        validator.Account(nameof(section.StaffArr.DefaultPayrollLiabilityAccountRef), section.StaffArr.DefaultPayrollLiabilityAccountRef);
        validator.Account(nameof(section.StaffArr.DefaultWageExpenseAccountRef), section.StaffArr.DefaultWageExpenseAccountRef);
        validator.Account(nameof(section.StaffArr.DefaultReimbursementClearingAccountRef), section.StaffArr.DefaultReimbursementClearingAccountRef);
    }

    private static void ValidateAccountsPayable(AccountsPayableSettingsSection section, SectionValidator validator)
    {
        validator.Percent(nameof(section.MatchTolerancePercent), section.MatchTolerancePercent, 0m, 100m);
        validator.NonNegative(nameof(section.MatchToleranceAmount), section.MatchToleranceAmount);
        validator.NonNegative(nameof(section.DefaultBillApprovalThreshold), section.DefaultBillApprovalThreshold);
        validator.Account(nameof(section.DefaultApControlAccountRef), section.DefaultApControlAccountRef);
        validator.Account(nameof(section.DefaultPurchaseAccrualAccountRef), section.DefaultPurchaseAccrualAccountRef);
        validator.Account(nameof(section.DefaultTaxPayableAccountRef), section.DefaultTaxPayableAccountRef);
    }

    private static void ValidateAccountsReceivable(AccountsReceivableSettingsSection section, SectionValidator validator)
    {
        validator.Enum(nameof(section.RevenueRecognitionMode), section.RevenueRecognitionMode, "pointInTime", "overTime", "manual");
        validator.Enum(nameof(section.CreditLimitEnforcement), section.CreditLimitEnforcement, "none", "warn", "block");
        validator.Enum(nameof(section.CreditHoldEnforcement), section.CreditHoldEnforcement, "none", "warn", "block");
        validator.Account(nameof(section.DefaultArAccountRef), section.DefaultArAccountRef);
        validator.Account(nameof(section.DefaultRevenueAccountRef), section.DefaultRevenueAccountRef);
        validator.Account(nameof(section.DefaultDeferredRevenueAccountRef), section.DefaultDeferredRevenueAccountRef);
        validator.Account(nameof(section.DefaultDiscountAccountRef), section.DefaultDiscountAccountRef);
        validator.Account(nameof(section.DefaultWriteOffAccountRef), section.DefaultWriteOffAccountRef);
        validator.Account(nameof(section.DefaultBadDebtAccountRef), section.DefaultBadDebtAccountRef);
    }

    private static void ValidateInventoryAccounting(InventoryAccountingSettingsSection section, SectionValidator validator)
    {
        validator.Enum(nameof(section.InventoryValuationMethod), section.InventoryValuationMethod, "fifo", "weightedAverage", "standardCost", "specificIdentification");
        validator.NonNegative(nameof(section.InventoryAdjustmentApprovalThreshold), section.InventoryAdjustmentApprovalThreshold);
        validator.Account(nameof(section.DefaultInventoryAssetAccountRef), section.DefaultInventoryAssetAccountRef);
        validator.Account(nameof(section.DefaultInventoryAdjustmentAccountRef), section.DefaultInventoryAdjustmentAccountRef);
        validator.Account(nameof(section.DefaultShrinkAccountRef), section.DefaultShrinkAccountRef);
        validator.Account(nameof(section.DefaultDamageAccountRef), section.DefaultDamageAccountRef);
        validator.Account(nameof(section.DefaultObsoleteInventoryAccountRef), section.DefaultObsoleteInventoryAccountRef);
        validator.Account(nameof(section.DefaultLandedCostClearingAccountRef), section.DefaultLandedCostClearingAccountRef);
    }

    private static void ValidateFixedAssets(FixedAssetSettingsSection section, SectionValidator validator)
    {
        validator.NonNegative(nameof(section.CapitalizationThreshold), section.CapitalizationThreshold);
        validator.Account(nameof(section.DefaultFixedAssetAccountRef), section.DefaultFixedAssetAccountRef);
        validator.Account(nameof(section.DefaultAccumulatedDepreciationAccountRef), section.DefaultAccumulatedDepreciationAccountRef);
        validator.Account(nameof(section.DefaultDepreciationExpenseAccountRef), section.DefaultDepreciationExpenseAccountRef);
        validator.Account(nameof(section.DefaultGainLossOnDisposalAccountRef), section.DefaultGainLossOnDisposalAccountRef);
        validator.EnumList(nameof(section.EnabledDepreciationMethods), section.EnabledDepreciationMethods, "straightLine", "doubleDecliningBalance", "unitsOfProduction");
    }

    private static void ValidateTax(TaxSettingsSection section, SectionValidator validator)
    {
        validator.Enum(nameof(section.TaxCalculationMode), section.TaxCalculationMode, "manual", "internalTable", "externalProvider");
        validator.Enum(nameof(section.TaxRoundingMethod), section.TaxRoundingMethod, "nearest", "up", "down");
        validator.Enum(nameof(section.TaxPostingDatePolicy), section.TaxPostingDatePolicy, "transactionDate", "invoiceDate", "periodEnd");
        validator.Account(nameof(section.DefaultSalesTaxPayableAccountRef), section.DefaultSalesTaxPayableAccountRef);
        validator.Account(nameof(section.DefaultUseTaxPayableAccountRef), section.DefaultUseTaxPayableAccountRef);
        validator.Account(nameof(section.DefaultVatReceivableAccountRef), section.DefaultVatReceivableAccountRef);
        validator.Account(nameof(section.DefaultVatPayableAccountRef), section.DefaultVatPayableAccountRef);
        validator.Reference(nameof(section.TaxFilingResponsibilityOwnerRef), section.TaxFilingResponsibilityOwnerRef);
        validator.Reference(nameof(section.TaxEvidenceStorageLocationRef), section.TaxEvidenceStorageLocationRef, allowLedgArrOwned: false);
    }

    private static void ValidateBanking(BankingSettingsSection section, SectionValidator validator)
    {
        validator.NonNegative(nameof(section.PaymentBatchApprovalThreshold), section.PaymentBatchApprovalThreshold);
        validator.NonNegative(nameof(section.RequireDualApprovalAboveThreshold), section.RequireDualApprovalAboveThreshold);
        validator.Account(nameof(section.DefaultCashAccountRef), section.DefaultCashAccountRef);
        validator.Account(nameof(section.DefaultUndepositedFundsAccountRef), section.DefaultUndepositedFundsAccountRef);
        validator.Account(nameof(section.DefaultPaymentClearingAccountRef), section.DefaultPaymentClearingAccountRef);
    }

    private static void ValidateIntercompany(IntercompanySettingsSection section, SectionValidator validator)
    {
        validator.Percent(nameof(section.IntercompanyMarkupPercent), section.IntercompanyMarkupPercent, 0m, 100m);
        validator.Enum(nameof(section.IntercompanySettlementFrequency), section.IntercompanySettlementFrequency, "daily", "weekly", "monthly", "manual");
        validator.LegalEntity(nameof(section.EliminationEntityRef), section.EliminationEntityRef);
        validator.ReferenceList(nameof(section.DueToDueFromAccountMappings), section.DueToDueFromAccountMappings);
        validator.ReferenceList(nameof(section.EliminationAccountMappings), section.EliminationAccountMappings);
    }

    private static void ValidateApprovals(ApprovalSettingsSection section, SectionValidator validator)
    {
        validator.IntRange(nameof(section.ApprovalTimeoutHours), section.ApprovalTimeoutHours, 1, 720);
        validator.EnumList(nameof(section.RequiredReasonCodes), section.RequiredReasonCodes, "void", "reversal", "writeOff", "closeOverride", "paymentHoldRelease", "creditHoldOverride", "manualJournal");
    }

    private static void ValidateClose(CloseSettingsSection section, SectionValidator validator)
    {
        validator.NonNegative(nameof(section.ManualPostingEvidenceThreshold), section.ManualPostingEvidenceThreshold);
        validator.EnumList(nameof(section.CloseTasksByModule), section.CloseTasksByModule, "gl", "ap", "ar", "inventory", "fixedAssets", "banking", "tax", "payrollExport", "intercompany");
        validator.Enum(nameof(section.RetentionPolicy), section.RetentionPolicy, "standard", "extended", "permanent");
    }

    private static void ValidateIntegrations(IntegrationSettingsSection section, SectionValidator validator)
    {
        validator.Enum(nameof(section.LedgarrOperatingMode), section.LedgarrOperatingMode, "systemOfRecord", "subledgerOnly", "externalErpMirror", "disabled");
        validator.Enum(nameof(section.ExternalErpProvider), section.ExternalErpProvider, "quickbooks", "netsuite", "sage", "xero", "microsoftDynamics", "sap", "custom", "none");
        validator.Enum(nameof(section.SyncDirection), section.SyncDirection, "outboundOnly", "inboundOnly", "bidirectional", "none");
        validator.Enum(nameof(section.PostingBatchMode), section.PostingBatchMode, "realTime", "scheduled", "manualReview");
    }

    private static void ValidateReporting(ReportingSettingsSection section, SectionValidator validator)
    {
        validator.Currency(nameof(section.DefaultReportingCurrency), section.DefaultReportingCurrency);
        validator.Enum(nameof(section.DefaultFinancialStatementBasis), section.DefaultFinancialStatementBasis, "accrual", "cash", "hybrid");
        validator.Enum(nameof(section.DefaultComparisonPeriod), section.DefaultComparisonPeriod, "priorMonth", "priorQuarter", "priorYear");
    }

    private static void ValidateEvidence(EvidenceSettingsSection section, SectionValidator validator)
    {
        validator.NonNegative(nameof(section.ManualJournalAttachmentThreshold), section.ManualJournalAttachmentThreshold);
        validator.Enum(nameof(section.FinancialDocumentRetentionPeriod), section.FinancialDocumentRetentionPeriod, "standard", "extended", "permanent");
        validator.ReferenceList(nameof(section.RecordArrFolderMapping), section.RecordArrFolderMapping);
    }

    private static object CreateDefaultSectionObject(string sectionKey) => sectionKey switch
    {
        "generalLedger" => new GeneralLedgerSettingsSection(),
        "legalEntities" => new LegalEntitySettingsSection(),
        "chartOfAccounts" => new ChartOfAccountsSettingsSection(),
        "dimensions" => new DimensionSettingsSection(),
        "postingSources" => new PostingSourceSettingsSection(),
        "accountsPayable" => new AccountsPayableSettingsSection(),
        "accountsReceivable" => new AccountsReceivableSettingsSection(),
        "inventoryAccounting" => new InventoryAccountingSettingsSection(),
        "fixedAssets" => new FixedAssetSettingsSection(),
        "tax" => new TaxSettingsSection(),
        "banking" => new BankingSettingsSection(),
        "intercompany" => new IntercompanySettingsSection(),
        "approvals" => new ApprovalSettingsSection(),
        "close" => new CloseSettingsSection(),
        "integrations" => new IntegrationSettingsSection(),
        "reporting" => new ReportingSettingsSection(),
        "evidence" => new EvidenceSettingsSection(),
        _ => throw new StlApiException("ledgarr.settings.section_unknown", $"Unknown LedgArr settings section '{sectionKey}'.", 404),
    };

    private static object DeserializeSectionValue(Type sectionType, JsonElement value) =>
        JsonSerializer.Deserialize(value.GetRawText(), sectionType, JsonOptions)
        ?? Activator.CreateInstance(sectionType)
        ?? throw new InvalidOperationException($"Unable to create {sectionType.Name}.");

    private static object DeserializeSectionValue(Type sectionType, string json) =>
        JsonSerializer.Deserialize(json, sectionType, JsonOptions)
        ?? Activator.CreateInstance(sectionType)
        ?? throw new InvalidOperationException($"Unable to create {sectionType.Name}.");

    private static Dictionary<string, SettingDiffItem> BuildDiff(object before, object after)
    {
        var beforeNode = JsonNode.Parse(JsonSerializer.Serialize(before, JsonOptions))?.AsObject() ?? new JsonObject();
        var afterNode = JsonNode.Parse(JsonSerializer.Serialize(after, JsonOptions))?.AsObject() ?? new JsonObject();
        var keys = beforeNode.Select(item => item.Key)
            .Union(afterNode.Select(item => item.Key), StringComparer.OrdinalIgnoreCase);
        var diff = new Dictionary<string, SettingDiffItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in keys)
        {
            var beforeJson = beforeNode[key]?.ToJsonString() ?? "null";
            var afterJson = afterNode[key]?.ToJsonString() ?? "null";
            if (!string.Equals(beforeJson, afterJson, StringComparison.Ordinal))
            {
                diff[key] = new SettingDiffItem(beforeNode[key], afterNode[key]);
            }
        }

        return diff;
    }

    private static bool IsHighImpactField(string sectionKey, string fieldKey) =>
        HighImpactFields.TryGetValue(sectionKey, out var fields) && fields.Contains(fieldKey);

    private static string NormalizeSectionKey(string sectionKey)
    {
        if (!SectionDefinitions.ContainsKey(sectionKey))
        {
            throw new StlApiException("ledgarr.settings.section_unknown", $"Unknown LedgArr settings section '{sectionKey}'.", 404);
        }

        return SectionDefinitions.Keys.First(key => string.Equals(key, sectionKey, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasPermission(ClaimsPrincipal principal, string permissionKey)
    {
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        if (string.Equals(principal.GetTenantRoleKey(), "tenant_admin", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return principal.Claims.Any(claim =>
            (string.Equals(claim.Type, "permissions", StringComparison.OrdinalIgnoreCase)
             || string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase)
             || string.Equals(claim.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase))
            && claim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(item => string.Equals(item, permissionKey, StringComparison.OrdinalIgnoreCase)));
    }

    private static bool CanViewSection(ClaimsPrincipal principal, string sectionKey)
    {
        var definition = SectionDefinitions[NormalizeSectionKey(sectionKey)];
        return HasPermission(principal, LedgArrSettingsPermissions.SettingsView)
            && HasPermission(principal, definition.ViewPermission);
    }

    private static void EnsureCanViewAll(ClaimsPrincipal principal)
    {
        if (!HasPermission(principal, LedgArrSettingsPermissions.SettingsView))
        {
            throw new StlApiException("ledgarr.settings.forbidden", "LedgArr settings view permission is required.", 403);
        }
    }

    private static void EnsureCanViewSection(ClaimsPrincipal principal, string sectionKey)
    {
        if (!CanViewSection(principal, sectionKey))
        {
            throw new StlApiException("ledgarr.settings.forbidden", $"Viewing LedgArr settings section '{sectionKey}' is not permitted.", 403);
        }
    }

    private static void EnsureCanManageSection(ClaimsPrincipal principal, string sectionKey)
    {
        var definition = SectionDefinitions[NormalizeSectionKey(sectionKey)];
        if (!HasPermission(principal, LedgArrSettingsPermissions.SettingsManage)
            || !HasPermission(principal, definition.ManagePermission))
        {
            throw new StlApiException("ledgarr.settings.forbidden", $"Managing LedgArr settings section '{sectionKey}' is not permitted.", 403);
        }
    }

    private sealed record SectionDefinition(string DisplayName, string Description, Type SectionType, string ViewPermission, string ManagePermission);

    private sealed class SectionValidator(
        IDictionary<string, string[]> errors,
        IReadOnlyList<GLAccount> accounts,
        IReadOnlyList<FinancialLegalEntity> entities,
        IReadOnlyList<string> currencies)
    {
        public void Enum(string fieldKey, string? value, params string[] allowed)
        {
            if (!string.IsNullOrWhiteSpace(value) && !allowed.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                errors[fieldKey] = [$"'{value}' is not an allowed value for {fieldKey}."];
            }
        }

        public void EnumList(string fieldKey, IEnumerable<string> values, params string[] allowed)
        {
            var invalid = values.Where(value => !allowed.Contains(value, StringComparer.OrdinalIgnoreCase)).ToArray();
            if (invalid.Length > 0)
            {
                errors[fieldKey] = [$"Unsupported values: {string.Join(", ", invalid)}."];
            }
        }

        public void Currency(string fieldKey, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value) && !currencies.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                errors[fieldKey] = [$"'{value}' is not a supported currency code."];
            }
        }

        public void Percent(string fieldKey, decimal value, decimal minimum, decimal maximum)
        {
            if (value < minimum || value > maximum)
            {
                errors[fieldKey] = [$"{fieldKey} must be between {minimum} and {maximum}."];
            }
        }

        public void IntRange(string fieldKey, int value, int minimum, int maximum)
        {
            if (value < minimum || value > maximum)
            {
                errors[fieldKey] = [$"{fieldKey} must be between {minimum} and {maximum}."];
            }
        }

        public void NonNegative(string fieldKey, decimal value)
        {
            if (value < 0)
            {
                errors[fieldKey] = [$"{fieldKey} must be non-negative."];
            }
        }

        public void Account(string fieldKey, string? accountCode)
        {
            if (string.IsNullOrWhiteSpace(accountCode))
            {
                return;
            }

            if (!accounts.Any(account => string.Equals(account.AccountCode, accountCode, StringComparison.OrdinalIgnoreCase)))
            {
                errors[fieldKey] = [$"Account '{accountCode}' does not exist as an active LedgArr GL account."];
            }
        }

        public void LegalEntity(string fieldKey, string? entityCode)
        {
            if (string.IsNullOrWhiteSpace(entityCode))
            {
                return;
            }

            var entity = entities.FirstOrDefault(item => string.Equals(item.EntityCode, entityCode, StringComparison.OrdinalIgnoreCase));
            if (entity is null)
            {
                errors[fieldKey] = [$"Legal entity '{entityCode}' does not exist as an active LedgArr legal entity."];
                return;
            }

            if (LedgArrStore.LooksLikeGoverningBody(entity.EntityCode, entity.DisplayName, entity.EntityType, null))
            {
                errors[fieldKey] = [$"'{entityCode}' cannot be used because LedgArr legal entities are not Compliance Core governing bodies."];
            }
        }

        public void Reference(string fieldKey, PublicReferenceSetting? reference, bool allowLedgArrOwned = true)
        {
            if (reference is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(reference.ProductKey)
                || string.IsNullOrWhiteSpace(reference.ObjectType)
                || string.IsNullOrWhiteSpace(reference.PublicKey)
                || string.IsNullOrWhiteSpace(reference.DisplayName))
            {
                errors[fieldKey] = ["Cross-product references must include productKey, objectType, publicKey, and displayName."];
                return;
            }

            if (!allowLedgArrOwned && string.Equals(reference.ProductKey, ProductKey, StringComparison.OrdinalIgnoreCase))
            {
                errors[fieldKey] = ["This field must reference the owning product, not a LedgArr-owned record."];
            }

            if (string.Equals(reference.ProductKey, "compliancecore", StringComparison.OrdinalIgnoreCase)
                && string.Equals(reference.ObjectType, "governingBody", StringComparison.OrdinalIgnoreCase))
            {
                errors[fieldKey] = ["Compliance Core governing bodies cannot be used as LedgArr legal entities or accounting references."];
            }
        }

        public void ReferenceList(string fieldKey, IEnumerable<PublicReferenceSetting?> references)
        {
            var invalid = references.Where(reference =>
                reference is not null
                && (string.IsNullOrWhiteSpace(reference.ProductKey)
                    || string.IsNullOrWhiteSpace(reference.ObjectType)
                    || string.IsNullOrWhiteSpace(reference.PublicKey)
                    || string.IsNullOrWhiteSpace(reference.DisplayName)))
                .ToArray();
            if (invalid.Length > 0)
            {
                errors[fieldKey] = ["Every cross-product reference must use an owning-product key, object type, public key, and display label."];
            }
        }
    }
}

public static class LedgArrSettingsPermissions
{
    public const string SettingsView = "ledgarr.settings.view";
    public const string SettingsManage = "ledgarr.settings.manage";
    public const string LegalEntitiesView = "ledgarr.legalEntities.view";
    public const string LegalEntitiesManage = "ledgarr.legalEntities.manage";
    public const string ChartOfAccountsView = "ledgarr.chartOfAccounts.view";
    public const string ChartOfAccountsManage = "ledgarr.chartOfAccounts.manage";
    public const string PostingRulesView = "ledgarr.postingRules.view";
    public const string PostingRulesManage = "ledgarr.postingRules.manage";
    public const string PeriodCloseView = "ledgarr.periodClose.view";
    public const string PeriodCloseManage = "ledgarr.periodClose.manage";
    public const string IntegrationsView = "ledgarr.integrations.view";
    public const string IntegrationsManage = "ledgarr.integrations.manage";
}

public sealed record LedgArrTenantSettingsEnvelope(int SettingsVersion, IReadOnlyList<LedgArrTenantSettingsSectionDto> Sections);

public sealed record LedgArrTenantSettingsSectionDto(
    string SectionKey,
    string DisplayName,
    string Description,
    string RowVersion,
    object Value,
    IReadOnlyList<string> HighImpactFields);

public sealed record LedgArrTenantSettingsUpdateRequest(JsonElement Value, string? ExpectedRowVersion, string? Reason);

public sealed record LedgArrTenantSettingsValidationResponse(bool IsValid, IReadOnlyDictionary<string, string[]> Errors);

public sealed record LedgArrTenantSettingsAuditResponse(IReadOnlyList<LedgArrTenantSettingsAuditItem> Items);

public sealed record LedgArrTenantSettingsAuditItem(
    Guid AuditId,
    string SectionKey,
    DateTimeOffset ChangedAtUtc,
    string ChangedByPersonId,
    string? ChangeReason,
    string? DiffJson,
    string? CorrelationId);

public sealed record LedgArrTenantSettingsOptionsResponse(
    IReadOnlyList<SettingsOptionReference> Accounts,
    IReadOnlyList<SettingsOptionReference> LegalEntities,
    IReadOnlyList<NamedOption> Currencies,
    IReadOnlyList<NamedOption> SourceProducts,
    IReadOnlyList<NamedOption> Sections,
    IReadOnlyList<SettingsOptionReference> CrossProductReferences);

public sealed record SettingsOptionReference(string Value, string Label, string ProductKey, string ObjectType, string PublicKey, string Status);

public sealed record NamedOption(string Value, string Label);

public sealed record SettingDiffItem(object? Before, object? After);

public sealed record LedgArrEffectiveSettingsSnapshot(
    GeneralLedgerSettingsSection GeneralLedger,
    LegalEntitySettingsSection LegalEntities,
    ChartOfAccountsSettingsSection ChartOfAccounts,
    DimensionSettingsSection Dimensions,
    PostingSourceSettingsSection PostingSources,
    AccountsPayableSettingsSection AccountsPayable,
    AccountsReceivableSettingsSection AccountsReceivable,
    InventoryAccountingSettingsSection InventoryAccounting,
    FixedAssetSettingsSection FixedAssets,
    TaxSettingsSection Tax,
    BankingSettingsSection Banking,
    IntercompanySettingsSection Intercompany,
    ApprovalSettingsSection Approvals,
    CloseSettingsSection Close,
    IntegrationSettingsSection Integrations,
    ReportingSettingsSection Reporting,
    EvidenceSettingsSection Evidence);

public sealed class PublicReferenceSetting
{
    public string ProductKey { get; set; } = string.Empty;
    public string ObjectType { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class CrossProductDimensionRuleSetting
{
    public string TransactionType { get; set; } = string.Empty;
    public string SourceProductKey { get; set; } = string.Empty;
    public List<PublicReferenceSetting> ReferenceValues { get; set; } = [];
}

public sealed class GeneralLedgerSettingsSection
{
    public string AccountingBasis { get; set; } = "accrual";
    public string BaseCurrency { get; set; } = "USD";
    public string ReportingCurrency { get; set; } = "USD";
    public bool MultiCurrencyEnabled { get; set; }
    public string ExchangeRateProvider { get; set; } = "manual";
    public string ExchangeRateLockPolicy { get; set; } = "lockOnPost";
    public int FiscalYearStartMonth { get; set; } = 1;
    public string FiscalCalendarType { get; set; } = "calendarMonth";
    public string PeriodCloseMode { get; set; } = "softCloseThenHardClose";
    public bool AllowPostingToClosedPeriods { get; set; }
    public bool RequireCloseChecklistBeforePeriodLock { get; set; } = true;
    public bool RequireJournalApproval { get; set; } = true;
    public bool RequireJournalReversalReason { get; set; } = true;
    public bool AllowUnbalancedDrafts { get; set; }
    public bool AutoNumberJournals { get; set; } = true;
    public string JournalNumberingFormat { get; set; } = "JRNL-{yyyy}-{seq}";
    public string? DefaultRetainedEarningsAccountRef { get; set; }
    public string? DefaultSuspenseAccountRef { get; set; }
    public string? DefaultRoundingAccountRef { get; set; }
    public string? DefaultCurrencyGainAccountRef { get; set; }
    public string? DefaultCurrencyLossAccountRef { get; set; }
}

public sealed class LegalEntitySettingsSection
{
    public bool MultiEntityEnabled { get; set; }
    public string? DefaultLegalEntityRef { get; set; }
    public string LegalEntityCodeFormat { get; set; } = "{ENTITY}-{NNN}";
    public List<string> RequiredLegalEntityFields { get; set; } = ["displayName", "entityType", "baseCurrencyCode"];
    public List<string> SupportedLegalEntityTypes { get; set; } = ["company", "division", "branch"];
    public bool RequireLegalEntityOnEveryPosting { get; set; } = true;
    public bool RequireLegalEntityOnCustomerInvoices { get; set; } = true;
    public bool RequireLegalEntityOnVendorBills { get; set; } = true;
    public bool RequireLegalEntityOnPayrollExportPackets { get; set; } = true;
    public bool AllowSharedChartOfAccountsAcrossEntities { get; set; } = true;
    public bool AllowEntitySpecificChartOverrides { get; set; }
    public bool IntercompanyEnabled { get; set; }
    public string IntercompanyBalancingMode { get; set; } = "dueToDueFrom";
    public string? DueToAccountRef { get; set; }
    public string? DueFromAccountRef { get; set; }
    public bool ConsolidationEnabled { get; set; }
    public string ConsolidationCurrency { get; set; } = "USD";
    public string? ConsolidationEliminationEntityRef { get; set; }
}

public sealed class ChartOfAccountsSettingsSection
{
    public string ChartOfAccountsTemplate { get; set; } = "stlStandard";
    public bool AllowTenantDefinedAccounts { get; set; } = true;
    public bool RequireAccountApprovalBeforeUse { get; set; } = true;
    public string AccountNumberingScheme { get; set; } = "numeric";
    public int AccountNumberLength { get; set; } = 4;
    public string SegmentSeparator { get; set; } = "-";
    public bool AccountSegmentsEnabled { get; set; }
    public List<string> RequiredSegmentStructure { get; set; } = [];
    public bool AllowInactiveAccountsInReports { get; set; }
    public bool PreventPostingToParentAccounts { get; set; } = true;
    public bool RequireAccountCategory { get; set; } = true;
    public bool RequireNormalBalance { get; set; } = true;
    public bool RequireFinancialStatementClassification { get; set; } = true;
    public string? DefaultInventoryAssetAccountRef { get; set; } = "1200";
    public string? DefaultApControlAccountRef { get; set; } = "2000";
    public string? DefaultArControlAccountRef { get; set; } = "1100";
    public string? DefaultPayrollLiabilityAccountRef { get; set; }
    public string? DefaultSalesTaxPayableAccountRef { get; set; } = "2200";
    public string? DefaultUseTaxPayableAccountRef { get; set; }
    public string? DefaultFreightAccrualAccountRef { get; set; } = "2100";
    public string? DefaultWorkOrderExpenseAccountRef { get; set; } = "5100";
    public string? DefaultPartsExpenseAccountRef { get; set; } = "5000";
    public string? DefaultFuelExpenseAccountRef { get; set; }
    public string? DefaultMaintenanceLaborExpenseAccountRef { get; set; } = "5100";
    public string? DefaultWarrantyRecoveryAccountRef { get; set; }
    public string? DefaultCustomerDepositsAccountRef { get; set; }
    public string? DefaultVendorDepositsAccountRef { get; set; }
}

public sealed class DimensionSettingsSection
{
    public List<string> EnabledDimensions { get; set; } = ["legalEntity", "department", "site"];
    public Dictionary<string, List<string>> RequiredDimensionsByTransactionType { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, List<string>> InheritedDimensionsBySourceProduct { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public bool AllowDimensionOverridesBeforePosting { get; set; } = true;
    public bool DimensionOverrideRequiresApproval { get; set; } = true;
    public bool MissingDimensionsBlockPosting { get; set; } = true;
    public List<PublicReferenceSetting> FallbackDimensions { get; set; } = [];
    public List<CrossProductDimensionRuleSetting> CrossProductDimensionMappingRules { get; set; } = [];
}

public sealed class PostingSourceSettingsSection
{
    public MaintainArrPostingSettings MaintainArr { get; set; } = new();
    public SupplyArrPostingSettings SupplyArr { get; set; } = new();
    public LoadArrPostingSettings LoadArr { get; set; } = new();
    public RoutArrPostingSettings RoutArr { get; set; } = new();
    public OrdArrPostingSettings OrdArr { get; set; } = new();
    public CustomArrPostingSettings CustomArr { get; set; } = new();
    public StaffArrPostingSettings StaffArr { get; set; } = new();
}

public sealed class MaintainArrPostingSettings
{
    public bool PostWorkOrderLaborCosts { get; set; } = true;
    public bool PostPartsConsumption { get; set; } = true;
    public bool PostOutsideVendorRepairInvoices { get; set; } = true;
    public bool CapitalizeEligibleAssetWork { get; set; } = true;
    public bool RequireAssetReferenceForCapitalization { get; set; } = true;
    public string? DefaultRepairExpenseAccountRef { get; set; } = "5100";
    public string? DefaultPmExpenseAccountRef { get; set; } = "5100";
    public string? DefaultPartsExpenseAccountRef { get; set; } = "5000";
    public string? DefaultWarrantyRecoveryAccountRef { get; set; }
}

public sealed class SupplyArrPostingSettings
{
    public bool PostPurchaseOrders { get; set; } = true;
    public bool PostVendorBills { get; set; } = true;
    public bool RequirePoMatchBeforeApPosting { get; set; } = true;
    public string? DefaultApAccountRef { get; set; } = "2000";
    public string? DefaultPurchaseAccrualAccountRef { get; set; } = "2100";
    public string? DefaultVendorDepositAccountRef { get; set; }
    public string? DefaultPriceVarianceAccountRef { get; set; } = "5200";
}

public sealed class LoadArrPostingSettings
{
    public bool PostInventoryReceipts { get; set; } = true;
    public bool PostInventoryAdjustments { get; set; } = true;
    public bool PostInventoryTransfers { get; set; } = true;
    public bool PostShrinkDamageAdjustments { get; set; } = true;
    public string InventoryValuationMethod { get; set; } = "fifo";
    public string? DefaultInventoryAssetAccountRef { get; set; } = "1200";
    public string? DefaultInventoryVarianceAccountRef { get; set; } = "5200";
    public string? DefaultReceivingAccrualAccountRef { get; set; } = "2100";
}

public sealed class RoutArrPostingSettings
{
    public bool PostFreightAccruals { get; set; } = true;
    public bool PostCarrierPayables { get; set; } = true;
    public bool PostAccessorials { get; set; } = true;
    public bool PostDetentionCharges { get; set; } = true;
    public bool PostLayoverCharges { get; set; } = true;
    public bool AllocateFreightToOrdersLoadsCustomers { get; set; } = true;
    public string? DefaultFreightExpenseAccountRef { get; set; } = "5000";
    public string? DefaultFreightRevenueAccountRef { get; set; } = "4000";
    public string? DefaultAccessorialRevenueAccountRef { get; set; } = "4000";
    public string? DefaultDetentionRevenueAccountRef { get; set; } = "4000";
}

public sealed class OrdArrPostingSettings
{
    public bool PostCustomerInvoices { get; set; } = true;
    public bool PostRevenue { get; set; } = true;
    public bool PostDiscounts { get; set; } = true;
    public bool PostReturnsCredits { get; set; } = true;
    public string? DefaultArAccountRef { get; set; } = "1100";
    public string? DefaultRevenueAccountRef { get; set; } = "4000";
    public string? DefaultDiscountAccountRef { get; set; }
    public string? DefaultReturnsAccountRef { get; set; }
}

public sealed class CustomArrPostingSettings
{
    public string CustomerFinancialReferenceMode { get; set; } = "requiredReference";
    public bool CreditHoldVisibility { get; set; } = true;
    public bool CustomerAccountBalanceVisibility { get; set; } = true;
    public bool PortalPaymentStatusExposure { get; set; } = true;
    public string DefaultCustomerPaymentTerms { get; set; } = "Net 30";
}

public sealed class StaffArrPostingSettings
{
    public bool PostPayrollExportPackets { get; set; } = true;
    public bool PostExpenseReimbursements { get; set; } = true;
    public bool RequirePersonDimensionOnLaborPostings { get; set; } = true;
    public string? DefaultPayrollLiabilityAccountRef { get; set; }
    public string? DefaultWageExpenseAccountRef { get; set; } = "5000";
    public string? DefaultReimbursementClearingAccountRef { get; set; }
}

public sealed class AccountsPayableSettingsSection
{
    public bool ApEnabled { get; set; } = true;
    public bool RequireVendorReferenceFromSupplyArr { get; set; } = true;
    public bool RequireLegalEntityOnVendorBills { get; set; } = true;
    public bool RequireBillApproval { get; set; } = true;
    public decimal DefaultBillApprovalThreshold { get; set; }
    public List<string> ApprovalRoutingRules { get; set; } = [];
    public bool TwoWayMatchEnabled { get; set; } = true;
    public bool ThreeWayMatchEnabled { get; set; }
    public decimal MatchToleranceAmount { get; set; }
    public decimal MatchTolerancePercent { get; set; } = 5m;
    public bool BlockDuplicateInvoiceNumbersPerVendor { get; set; } = true;
    public bool RequireInvoiceAttachment { get; set; } = true;
    public bool RequirePaymentTerms { get; set; } = true;
    public bool RequireTaxClassificationWhereApplicable { get; set; } = true;
    public string? DefaultApControlAccountRef { get; set; } = "2000";
    public string? DefaultPurchaseAccrualAccountRef { get; set; } = "2100";
    public string? DefaultTaxPayableAccountRef { get; set; } = "2200";
    public bool PaymentBatchApprovalRequired { get; set; } = true;
    public bool AllowPartialPayments { get; set; } = true;
    public bool AllowScheduledPayments { get; set; } = true;
    public bool AllowEarlyPaymentDiscounts { get; set; } = true;
}

public sealed class AccountsReceivableSettingsSection
{
    public bool ArEnabled { get; set; } = true;
    public bool RequireCustomerReferenceFromCustomArr { get; set; } = true;
    public bool RequireLegalEntityOnCustomerInvoices { get; set; } = true;
    public string InvoiceNumberingFormat { get; set; } = "INV-{yyyy}-{seq}";
    public string CreditMemoNumberingFormat { get; set; } = "CRM-{yyyy}-{seq}";
    public string DefaultPaymentTerms { get; set; } = "Net 30";
    public bool AllowCustomerSpecificTerms { get; set; } = true;
    public bool RequireInvoiceApproval { get; set; } = true;
    public bool AllowInvoiceDraftEditsAfterApproval { get; set; }
    public bool AllowInvoiceVoids { get; set; } = true;
    public bool RequireVoidReason { get; set; } = true;
    public string RevenueRecognitionMode { get; set; } = "pointInTime";
    public string? DefaultArAccountRef { get; set; } = "1100";
    public string? DefaultRevenueAccountRef { get; set; } = "4000";
    public string? DefaultDeferredRevenueAccountRef { get; set; }
    public string? DefaultDiscountAccountRef { get; set; }
    public string? DefaultWriteOffAccountRef { get; set; }
    public string? DefaultBadDebtAccountRef { get; set; }
    public string CreditLimitEnforcement { get; set; } = "warn";
    public string CreditHoldEnforcement { get; set; } = "block";
    public bool CreditHoldOverrideApproval { get; set; } = true;
    public bool CustomerStatementGenerationEnabled { get; set; } = true;
    public bool PortalBalanceVisibilityEnabled { get; set; } = true;
    public bool PortalInvoiceVisibilityEnabled { get; set; } = true;
    public bool PortalPaymentStatusVisibilityEnabled { get; set; } = true;
}

public sealed class InventoryAccountingSettingsSection
{
    public bool InventoryAccountingEnabled { get; set; } = true;
    public string InventoryValuationMethod { get; set; } = "fifo";
    public bool AllowNegativeInventoryValuation { get; set; }
    public bool RequireReceiptBeforeInvoiceMatch { get; set; } = true;
    public bool CapitalizeFreightIntoInventory { get; set; } = true;
    public bool CapitalizeTaxIntoInventory { get; set; }
    public bool CapitalizeHandlingIntoInventory { get; set; }
    public string? DefaultInventoryAssetAccountRef { get; set; } = "1200";
    public string? DefaultInventoryAdjustmentAccountRef { get; set; } = "5200";
    public string? DefaultShrinkAccountRef { get; set; } = "5200";
    public string? DefaultDamageAccountRef { get; set; } = "5200";
    public string? DefaultObsoleteInventoryAccountRef { get; set; }
    public string? DefaultLandedCostClearingAccountRef { get; set; } = "2300";
    public bool RequireFinancialApprovalForInventoryAdjustmentAboveThreshold { get; set; } = true;
    public decimal InventoryAdjustmentApprovalThreshold { get; set; }
    public bool RequireReasonCodeForInventoryWriteOff { get; set; } = true;
}

public sealed class FixedAssetSettingsSection
{
    public bool FixedAssetAccountingEnabled { get; set; } = true;
    public bool AllowMaintainArrAssetsToBecomeFinancialAssets { get; set; } = true;
    public bool RequireCapitalizationApproval { get; set; } = true;
    public decimal CapitalizationThreshold { get; set; }
    public string? DefaultFixedAssetAccountRef { get; set; } = "1500";
    public string? DefaultAccumulatedDepreciationAccountRef { get; set; } = "1590";
    public string? DefaultDepreciationExpenseAccountRef { get; set; } = "5300";
    public string? DefaultGainLossOnDisposalAccountRef { get; set; }
    public List<string> EnabledDepreciationMethods { get; set; } = ["straightLine"];
    public Dictionary<string, int> DefaultUsefulLifeByAssetClass { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public bool RequireDisposalApproval { get; set; } = true;
    public bool RequireDisposalReason { get; set; } = true;
    public bool AllowAssetImpairmentPostings { get; set; }
    public bool RequireSupportingDocumentForCapitalization { get; set; } = true;
    public bool RequireLegalEntityForAllFinancialAssets { get; set; } = true;
}

public sealed class TaxSettingsSection
{
    public bool SalesTaxEnabled { get; set; } = true;
    public bool UseTaxEnabled { get; set; }
    public bool VatGstEnabled { get; set; }
    public string TaxCalculationMode { get; set; } = "manual";
    public string TaxProviderIntegration { get; set; } = "none";
    public string TaxJurisdictionSource { get; set; } = "tenantConfig";
    public bool RequireTaxCodeOnTaxableTransactions { get; set; } = true;
    public bool AllowTaxExemptionCertificates { get; set; } = true;
    public bool RequireExemptionCertificateValidation { get; set; } = true;
    public string? DefaultSalesTaxPayableAccountRef { get; set; } = "2200";
    public string? DefaultUseTaxPayableAccountRef { get; set; }
    public string? DefaultVatReceivableAccountRef { get; set; }
    public string? DefaultVatPayableAccountRef { get; set; }
    public string TaxRoundingMethod { get; set; } = "nearest";
    public string TaxPostingDatePolicy { get; set; } = "transactionDate";
    public bool TaxFilingCalendarEnabled { get; set; } = true;
    public PublicReferenceSetting? TaxFilingResponsibilityOwnerRef { get; set; }
    public PublicReferenceSetting? TaxEvidenceStorageLocationRef { get; set; }
}

public sealed class BankingSettingsSection
{
    public bool BankingEnabled { get; set; }
    public bool BankAccountApprovalRequired { get; set; } = true;
    public string? DefaultCashAccountRef { get; set; } = "1000";
    public string? DefaultUndepositedFundsAccountRef { get; set; }
    public string? DefaultPaymentClearingAccountRef { get; set; } = "2300";
    public bool BankReconciliationEnabled { get; set; } = true;
    public bool RequireReconciliationApproval { get; set; } = true;
    public List<string> EnabledPaymentMethods { get; set; } = ["ach", "check"];
    public string CheckNumberingFormat { get; set; } = "CHK-{yyyy}-{seq}";
    public bool PositivePayExportEnabled { get; set; }
    public decimal PaymentBatchApprovalThreshold { get; set; }
    public decimal RequireDualApprovalAboveThreshold { get; set; }
    public bool AllowCustomerPaymentImport { get; set; } = true;
    public bool AllowVendorPaymentExport { get; set; } = true;
    public string BankFeedProvider { get; set; } = "none";
    public string BankStatementImportFormat { get; set; } = "csv";
}

public sealed class IntercompanySettingsSection
{
    public bool IntercompanyEnabled { get; set; }
    public bool AutoCreateBalancingEntries { get; set; } = true;
    public bool RequireIntercompanyAgreementReference { get; set; } = true;
    public List<PublicReferenceSetting> DueToDueFromAccountMappings { get; set; } = [];
    public string IntercompanySettlementFrequency { get; set; } = "monthly";
    public bool IntercompanyMarkupEnabled { get; set; }
    public decimal IntercompanyMarkupPercent { get; set; }
    public bool RequireApprovalForIntercompanyPostings { get; set; } = true;
    public bool AllowCrossEntityInventoryTransferAccounting { get; set; }
    public bool AllowCrossEntityLaborAllocation { get; set; }
    public bool AllowCrossEntityAssetUseAllocation { get; set; }
    public bool ConsolidationEliminationEnabled { get; set; }
    public string? EliminationEntityRef { get; set; }
    public List<PublicReferenceSetting> EliminationAccountMappings { get; set; } = [];
}

public sealed class ApprovalSettingsSection
{
    public bool RequireApprovalForManualJournals { get; set; } = true;
    public bool RequireApprovalForApBills { get; set; } = true;
    public bool RequireApprovalForArInvoices { get; set; } = true;
    public bool RequireApprovalForCreditMemos { get; set; } = true;
    public bool RequireApprovalForPayments { get; set; } = true;
    public bool RequireApprovalForWriteOffs { get; set; } = true;
    public bool RequireApprovalForInventoryValuationAdjustments { get; set; } = true;
    public bool RequireApprovalForFixedAssetCapitalization { get; set; } = true;
    public bool RequireApprovalForDepreciationOverrides { get; set; } = true;
    public bool RequireApprovalForPeriodClose { get; set; } = true;
    public bool SegregationOfDutiesEnforced { get; set; } = true;
    public bool PreventCreatorFromApprovingOwnTransaction { get; set; } = true;
    public bool ApprovalDelegationEnabled { get; set; } = true;
    public bool ApprovalEscalationEnabled { get; set; } = true;
    public int ApprovalTimeoutHours { get; set; } = 48;
    public List<string> RequiredReasonCodes { get; set; } = ["void", "reversal", "writeOff", "closeOverride", "paymentHoldRelease", "creditHoldOverride", "manualJournal"];
}

public sealed class CloseSettingsSection
{
    public bool MonthlyCloseEnabled { get; set; } = true;
    public bool CloseChecklistRequired { get; set; } = true;
    public List<string> CloseTasksByModule { get; set; } = ["gl", "ap", "ar", "inventory", "fixedAssets", "banking", "tax", "payrollExport", "intercompany"];
    public bool RequireAllSubledgersBalancedBeforeClose { get; set; } = true;
    public bool RequireBankReconciliationsBeforeClose { get; set; } = true;
    public bool RequireInventoryValuationLockBeforeClose { get; set; } = true;
    public bool RequireDepreciationRunBeforeClose { get; set; } = true;
    public bool AllowSoftClose { get; set; } = true;
    public bool AllowHardClose { get; set; } = true;
    public bool LockPostingAfterHardClose { get; set; } = true;
    public bool AllowReopenPeriod { get; set; }
    public bool ReopenRequiresApproval { get; set; } = true;
    public bool ReopenRequiresReason { get; set; } = true;
    public bool ImmutableAuditLogEnabled { get; set; } = true;
    public bool ExportAuditPacketToRecordArr { get; set; } = true;
    public string RetentionPolicy { get; set; } = "standard";
    public bool AuditEvidenceRequiredForManualPostingsAboveThreshold { get; set; } = true;
    public decimal ManualPostingEvidenceThreshold { get; set; }
}

public sealed class IntegrationSettingsSection
{
    public string LedgarrOperatingMode { get; set; } = "systemOfRecord";
    public string ExternalErpProvider { get; set; } = "none";
    public string SyncDirection { get; set; } = "outboundOnly";
    public string SyncFrequency { get; set; } = "hourly";
    public string PostingBatchMode { get; set; } = "scheduled";
    public bool ExternalAccountMappingRequired { get; set; } = true;
    public bool ExternalEntityMappingRequired { get; set; } = true;
    public bool ExternalCustomerMappingRequired { get; set; } = true;
    public bool ExternalVendorMappingRequired { get; set; } = true;
    public bool ExternalItemAccountMappingRequired { get; set; } = true;
    public bool RetryFailedPostings { get; set; } = true;
    public bool HoldFailedPostingsForReview { get; set; } = true;
    public bool AllowManualReExport { get; set; } = true;
    public bool RequireReconciliationAgainstExternalErp { get; set; } = true;
}

public sealed class ReportingSettingsSection
{
    public string DefaultReportingCurrency { get; set; } = "USD";
    public string DefaultFinancialStatementBasis { get; set; } = "accrual";
    public bool ConsolidatedReportingEnabled { get; set; }
    public bool EntityLevelReportingEnabled { get; set; } = true;
    public bool SiteLevelReportingEnabled { get; set; } = true;
    public bool DepartmentLevelReportingEnabled { get; set; } = true;
    public bool CostCenterReportingEnabled { get; set; } = true;
    public bool CustomerProfitabilityEnabled { get; set; } = true;
    public bool VendorSpendReportingEnabled { get; set; } = true;
    public bool AssetCostReportingEnabled { get; set; } = true;
    public bool RouteTripProfitabilityEnabled { get; set; } = true;
    public bool OrderProfitabilityEnabled { get; set; } = true;
    public bool InventoryValuationReportingEnabled { get; set; } = true;
    public bool ClosePackageGenerationEnabled { get; set; } = true;
    public bool ManagementPackageGenerationEnabled { get; set; } = true;
    public string DefaultReportCalendar { get; set; } = "fiscal";
    public string DefaultComparisonPeriod { get; set; } = "priorMonth";
    public string DefaultBudgetVersion { get; set; } = "working";
    public bool BudgetingEnabled { get; set; } = true;
    public bool ForecastingEnabled { get; set; } = true;
}

public sealed class EvidenceSettingsSection
{
    public bool StoreFinancialDocumentsInRecordArr { get; set; } = true;
    public bool RequireAttachmentForApBill { get; set; } = true;
    public bool RequireAttachmentForManualJournalAboveThreshold { get; set; } = true;
    public decimal ManualJournalAttachmentThreshold { get; set; }
    public bool RequireAttachmentForPaymentBatch { get; set; } = true;
    public bool RequireAttachmentForTaxFiling { get; set; } = true;
    public bool RequireAttachmentForFixedAssetCapitalization { get; set; } = true;
    public bool RequireAttachmentForPeriodClose { get; set; } = true;
    public List<PublicReferenceSetting> RecordArrFolderMapping { get; set; } = [];
    public string FinancialDocumentRetentionPeriod { get; set; } = "standard";
    public bool AllowSourceProductAttachmentsToSatisfyEvidenceRequirement { get; set; } = true;
    public bool RequireImmutableFinalizedDocumentCopyAfterPosting { get; set; } = true;
}
