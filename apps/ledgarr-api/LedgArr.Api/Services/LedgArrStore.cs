using System.Security.Claims;
using LedgArr.Api.Data;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace LedgArr.Api.Services;

public sealed class LedgArrStore(LedgArrDbContext db, LedgArrTenantSettingsService? tenantSettingsService = null)
{
    private const string ProductKey = "ledgarr";
    private static readonly StringComparer IgnoreCase = StringComparer.OrdinalIgnoreCase;
    private readonly LedgArrTenantSettingsService _tenantSettingsService = tenantSettingsService ?? new LedgArrTenantSettingsService(db);

    public LedgArrSessionBootstrapResponse BuildSession(
        string userId,
        string personId,
        string tenantId,
        string tenantRoleKey,
        bool isPlatformAdmin,
        IEnumerable<string> launchableProductKeys) =>
        new(userId, personId, tenantId, $"session-{userId}", tenantRoleKey, isPlatformAdmin, ProductKey, true, launchableProductKeys.ToArray());

    public async Task<LedgArrDashboardResponse> GetDashboardAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureBootstrapAsync(tenantId, cancellationToken);

        var packets = await db.FinancialPackets.Where(p => p.TenantId == tenantId).ToListAsync(cancellationToken);
        var journals = await db.JournalEntries.Where(j => j.TenantId == tenantId).ToListAsync(cancellationToken);
        var vendorBills = await db.VendorBills.Where(b => b.TenantId == tenantId).ToListAsync(cancellationToken);
        var invoices = await db.CustomerInvoices.Where(i => i.TenantId == tenantId).ToListAsync(cancellationToken);
        var periods = await db.FiscalPeriods.Where(p => p.TenantId == tenantId).ToListAsync(cancellationToken);
        var audit = await db.FinancialAuditEvents
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.OccurredAt)
            .Take(10)
            .Select(e => new LedgArrActivityResponse(e.Id.ToString(), e.Action, e.TargetType, e.TargetId, e.Summary, e.OccurredAt))
            .ToListAsync(cancellationToken);

        return new LedgArrDashboardResponse(
            DateTimeOffset.UtcNow,
            await db.FinancialLegalEntities.CountAsync(e => e.TenantId == tenantId && e.Status == "active", cancellationToken),
            packets.Count(p => p.Status is "received" or "needs_mapping" or "validation_failed"),
            journals.Count(j => j.Status == "posted"),
            periods.Count(p => p.Status == "open"),
            periods.Count(p => p.Status == "closed"),
            vendorBills.Count(b => b.Status is not "paid" and not "voided"),
            invoices.Count(i => i.Status is "issued" or "posted"),
            journals.Where(j => j.Status == "posted").Sum(j => j.TotalDebits),
            vendorBills.Where(b => b.Status is not "paid" and not "voided").Sum(b => b.TotalAmount),
            invoices.Where(i => i.Status is "issued" or "posted").Sum(i => i.TotalAmount),
            audit);
    }

    public async Task<IReadOnlyList<FinancialLegalEntity>> ListFinancialLegalEntitiesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureBootstrapAsync(tenantId, cancellationToken);
        return await db.FinancialLegalEntities
            .Where(e => e.TenantId == tenantId)
            .OrderBy(e => e.EntityCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FinancialLegalEntityRegistration>> ListFinancialLegalEntityRegistrationsAsync(
        ClaimsPrincipal principal,
        Guid? financialLegalEntityId = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureBootstrapAsync(tenantId, cancellationToken);

        var query = db.FinancialLegalEntityRegistrations.Where(registration => registration.TenantId == tenantId);
        if (financialLegalEntityId.HasValue)
        {
            query = query.Where(registration => registration.FinancialLegalEntityId == financialLegalEntityId.Value);
        }

        return await query
            .OrderBy(registration => registration.JurisdictionLabel)
            .ThenBy(registration => registration.RegistrationType)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FinancialLegalEntityAddressSnapshot>> ListFinancialLegalEntityAddressSnapshotsAsync(
        ClaimsPrincipal principal,
        Guid? financialLegalEntityId = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureBootstrapAsync(tenantId, cancellationToken);

        var query = db.FinancialLegalEntityAddressSnapshots.Where(snapshot => snapshot.TenantId == tenantId);
        if (financialLegalEntityId.HasValue)
        {
            query = query.Where(snapshot => snapshot.FinancialLegalEntityId == financialLegalEntityId.Value);
        }

        return await query
            .OrderBy(snapshot => snapshot.SnapshotLabel)
            .ThenBy(snapshot => snapshot.AddressLine1)
            .ToListAsync(cancellationToken);
    }

    public async Task<FinancialLegalEntity?> GetFinancialLegalEntityAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        return await db.FinancialLegalEntities.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id, cancellationToken);
    }

    public async Task<FinancialLegalEntity> CreateFinancialLegalEntityAsync(
        ClaimsPrincipal principal,
        CreateFinancialLegalEntityRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        RejectGoverningBodyLikeEntity(request.EntityCode, request.DisplayName, request.EntityType, request.SourceRef);

        var entity = new FinancialLegalEntity
        {
            TenantId = tenantId,
            EntityCode = NormalizeRequired(request.EntityCode, "Entity code is required.").ToUpperInvariant(),
            DisplayName = NormalizeRequired(request.DisplayName, "Financial Legal Entity display name is required."),
            EntityType = NormalizeOptional(request.EntityType, "company"),
            BaseCurrencyCode = NormalizeOptional(request.BaseCurrencyCode, "USD").ToUpperInvariant(),
            StaffArrLocationRefId = request.StaffArrLocationRefId,
            SnapshotLabel = request.SnapshotLabel,
        };

        db.FinancialLegalEntities.Add(entity);
        await AddAuditAsync(tenantId, principal, "financial_legal_entity.created", "financial_legal_entity", entity.Id.ToString(), $"Created Financial Legal Entity {entity.DisplayName}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<FinancialLegalEntity?> UpdateFinancialLegalEntityAsync(
        ClaimsPrincipal principal,
        Guid id,
        CreateFinancialLegalEntityRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        RejectGoverningBodyLikeEntity(request.EntityCode, request.DisplayName, request.EntityType, request.SourceRef);

        var entity = await db.FinancialLegalEntities.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.EntityCode = NormalizeRequired(request.EntityCode, "Entity code is required.").ToUpperInvariant();
        entity.DisplayName = NormalizeRequired(request.DisplayName, "Financial Legal Entity display name is required.");
        entity.EntityType = NormalizeOptional(request.EntityType, "company");
        entity.BaseCurrencyCode = NormalizeOptional(request.BaseCurrencyCode, "USD").ToUpperInvariant();
        entity.StaffArrLocationRefId = request.StaffArrLocationRefId;
        entity.SnapshotLabel = request.SnapshotLabel;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await AddAuditAsync(tenantId, principal, "financial_legal_entity.updated", "financial_legal_entity", entity.Id.ToString(), $"Updated Financial Legal Entity {entity.DisplayName}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<FinancialLegalEntity?> DeactivateFinancialLegalEntityAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var entity = await db.FinancialLegalEntities.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Status = "inactive";
        entity.DeactivatedAt = DateTimeOffset.UtcNow;
        await AddAuditAsync(tenantId, principal, "financial_legal_entity.deactivated", "financial_legal_entity", entity.Id.ToString(), $"Deactivated Financial Legal Entity {entity.DisplayName}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<IReadOnlyList<FiscalCalendar>> ListFiscalCalendarsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureBootstrapAsync(tenantId, cancellationToken);
        return await db.FiscalCalendars.Where(c => c.TenantId == tenantId).OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public async Task<FiscalCalendar> CreateFiscalCalendarAsync(ClaimsPrincipal principal, CreateFiscalCalendarRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var calendar = new FiscalCalendar
        {
            TenantId = tenantId,
            Name = NormalizeRequired(request.Name, "Fiscal calendar name is required."),
            FiscalYearStartMonth = request.FiscalYearStartMonth is >= 1 and <= 12 ? request.FiscalYearStartMonth : 1,
            BaseCurrencyCode = NormalizeOptional(request.BaseCurrencyCode, "USD").ToUpperInvariant(),
        };

        db.FiscalCalendars.Add(calendar);
        await AddAuditAsync(tenantId, principal, "fiscal_calendar.created", "fiscal_calendar", calendar.Id.ToString(), $"Created fiscal calendar {calendar.Name}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return calendar;
    }

    public async Task<IReadOnlyList<FiscalPeriod>> ListFiscalPeriodsAsync(ClaimsPrincipal principal, string? status, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureBootstrapAsync(tenantId, cancellationToken);
        var query = db.FiscalPeriods.Where(p => p.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status == status.Trim().ToLowerInvariant());
        }

        return await query.OrderByDescending(p => p.StartDate).ToListAsync(cancellationToken);
    }

    public async Task<FiscalPeriod> CreateFiscalPeriodAsync(ClaimsPrincipal principal, CreateFiscalPeriodRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var period = new FiscalPeriod
        {
            TenantId = tenantId,
            FiscalCalendarId = request.FiscalCalendarId,
            FinancialLegalEntityId = request.FinancialLegalEntityId,
            PeriodKey = NormalizeRequired(request.PeriodKey, "Fiscal period key is required."),
            Name = NormalizeRequired(request.Name, "Fiscal period name is required."),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "open",
        };

        db.FiscalPeriods.Add(period);
        await AddAuditAsync(tenantId, principal, "fiscal_period.created", "fiscal_period", period.Id.ToString(), $"Created fiscal period {period.PeriodKey}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return period;
    }

    public Task<FiscalPeriod?> CloseFiscalPeriodAsync(ClaimsPrincipal principal, Guid id, string? reason, CancellationToken cancellationToken = default) =>
        ChangePeriodStatusAsync(principal, id, "closed", reason, cancellationToken);

    public Task<FiscalPeriod?> ReopenFiscalPeriodAsync(ClaimsPrincipal principal, Guid id, string? reason, CancellationToken cancellationToken = default) =>
        ChangePeriodStatusAsync(principal, id, "open", reason, cancellationToken);

    public Task<FiscalPeriod?> LockFiscalPeriodAsync(ClaimsPrincipal principal, Guid id, string? reason, CancellationToken cancellationToken = default) =>
        ChangePeriodStatusAsync(principal, id, "locked", reason, cancellationToken);

    public async Task<IReadOnlyList<ChartOfAccounts>> ListChartsOfAccountsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureBootstrapAsync(tenantId, cancellationToken);
        return await db.ChartsOfAccounts.Where(c => c.TenantId == tenantId).OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public async Task<ChartOfAccounts> CreateChartOfAccountsAsync(ClaimsPrincipal principal, CreateChartOfAccountsRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var chart = new ChartOfAccounts
        {
            TenantId = tenantId,
            FinancialLegalEntityId = request.FinancialLegalEntityId,
            Name = NormalizeRequired(request.Name, "Chart of accounts name is required."),
        };

        db.ChartsOfAccounts.Add(chart);
        await AddAuditAsync(tenantId, principal, "chart_of_accounts.created", "chart_of_accounts", chart.Id.ToString(), $"Created chart of accounts {chart.Name}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return chart;
    }

    public async Task<IReadOnlyList<GLAccount>> ListGLAccountsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureBootstrapAsync(tenantId, cancellationToken);
        return await db.GLAccounts.Where(a => a.TenantId == tenantId).OrderBy(a => a.AccountCode).ToListAsync(cancellationToken);
    }

    public async Task<GLAccount> CreateGLAccountAsync(ClaimsPrincipal principal, CreateGLAccountRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var chartId = request.ChartOfAccountsId ?? await EnsureDefaultChartAsync(tenantId, cancellationToken);
        var account = new GLAccount
        {
            TenantId = tenantId,
            ChartOfAccountsId = chartId,
            AccountCode = NormalizeRequired(request.AccountCode, "GL account code is required."),
            Name = NormalizeRequired(request.Name, "GL account name is required."),
            AccountType = NormalizeOptional(request.AccountType, "expense"),
            Category = NormalizeOptional(request.Category, "operating"),
            NormalBalance = NormalizeOptional(request.NormalBalance, request.AccountType is "liability" or "equity" or "revenue" ? "credit" : "debit"),
        };

        db.GLAccounts.Add(account);
        await AddAuditAsync(tenantId, principal, "gl_account.created", "gl_account", account.Id.ToString(), $"Created GL account {account.AccountCode}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<GLAccount?> UpdateGLAccountAsync(ClaimsPrincipal principal, Guid id, CreateGLAccountRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var account = await db.GLAccounts.FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id, cancellationToken);
        if (account is null)
        {
            return null;
        }

        account.AccountCode = NormalizeRequired(request.AccountCode, "GL account code is required.");
        account.Name = NormalizeRequired(request.Name, "GL account name is required.");
        account.AccountType = NormalizeOptional(request.AccountType, account.AccountType);
        account.Category = NormalizeOptional(request.Category, account.Category);
        account.NormalBalance = NormalizeOptional(request.NormalBalance, account.NormalBalance);
        await AddAuditAsync(tenantId, principal, "gl_account.updated", "gl_account", account.Id.ToString(), $"Updated GL account {account.AccountCode}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<GLAccount?> DeactivateGLAccountAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var account = await db.GLAccounts.FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id, cancellationToken);
        if (account is null)
        {
            return null;
        }

        account.Status = "inactive";
        await AddAuditAsync(tenantId, principal, "gl_account.deactivated", "gl_account", account.Id.ToString(), $"Deactivated GL account {account.AccountCode}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<IReadOnlyList<FinancialDimensionType>> ListDimensionTypesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        return await db.FinancialDimensionTypes.Where(d => d.TenantId == tenantId).OrderBy(d => d.DimensionKey).ToListAsync(cancellationToken);
    }

    public async Task<FinancialDimensionType> CreateDimensionTypeAsync(ClaimsPrincipal principal, CreateDimensionTypeRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var type = new FinancialDimensionType
        {
            TenantId = tenantId,
            DimensionKey = NormalizeRequired(request.DimensionKey, "Dimension key is required.").ToLowerInvariant(),
            DisplayName = NormalizeRequired(request.DisplayName, "Dimension display name is required."),
        };

        db.FinancialDimensionTypes.Add(type);
        await AddAuditAsync(tenantId, principal, "dimension_type.created", "financial_dimension_type", type.Id.ToString(), $"Created dimension type {type.DimensionKey}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return type;
    }

    public async Task<FinancialDimensionValue> CreateDimensionValueAsync(ClaimsPrincipal principal, CreateDimensionValueRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var value = new FinancialDimensionValue
        {
            TenantId = tenantId,
            DimensionTypeId = request.DimensionTypeId,
            ValueKey = NormalizeRequired(request.ValueKey, "Dimension value key is required.").ToLowerInvariant(),
            DisplayName = NormalizeRequired(request.DisplayName, "Dimension value display name is required."),
            SourceProductKey = request.SourceRef?.ProductKey,
            SourceRecordType = request.SourceRef?.ObjectType,
            SourceRecordId = request.SourceRef?.ObjectId,
        };

        db.FinancialDimensionValues.Add(value);
        await AddAuditAsync(tenantId, principal, "dimension_value.created", "financial_dimension_value", value.Id.ToString(), $"Created dimension value {value.DisplayName}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return value;
    }

    public async Task<DimensionResolveResponse> ResolveDimensionsAsync(ClaimsPrincipal principal, DimensionResolveRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var sourceRefs = request.SourceRefs ?? [];
        var values = await db.FinancialDimensionValues
            .Where(v => v.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var resolved = values
            .Where(value => sourceRefs.Any(source =>
                string.Equals(source.ProductKey, value.SourceProductKey, StringComparison.OrdinalIgnoreCase)
                && string.Equals(source.ObjectType, value.SourceRecordType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(source.ObjectId, value.SourceRecordId, StringComparison.OrdinalIgnoreCase)))
            .Select(value => new DimensionResolvedValueResponse(value.Id, value.ValueKey, value.DisplayName))
            .ToArray();

        return new DimensionResolveResponse(resolved, resolved.Length == sourceRefs.Count);
    }

    public async Task<IReadOnlyList<FinancialPacket>> ListFinancialPacketsAsync(ClaimsPrincipal principal, string? status, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var query = db.FinancialPackets.Where(p => p.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status == status.Trim().ToLowerInvariant());
        }

        return await query.OrderByDescending(p => p.ReceivedAt).ToListAsync(cancellationToken);
    }

    public async Task<FinancialPacketDetailResponse?> GetFinancialPacketAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var packet = await db.FinancialPackets.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id, cancellationToken);
        if (packet is null)
        {
            return null;
        }

        return await BuildPacketDetailAsync(packet, cancellationToken);
    }

    public async Task<FinancialPacketDetailResponse> IngestFinancialPacketAsync(
        ClaimsPrincipal principal,
        FinancialPacketIngestRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            throw new StlApiException("ledgarr.packet.idempotency_required", "Every FinancialPacket must include an idempotencyKey.", 400);
        }

        var sourceProductKey = NormalizeRequired(request.SourceProductKey, "sourceProductKey is required.").ToLowerInvariant();
        var existingKey = await db.FinancialPacketIdempotencyKeys.FirstOrDefaultAsync(key =>
            key.TenantId == tenantId
            && key.SourceProductKey == sourceProductKey
            && key.SourceEventId == request.SourceEventId
            && key.SourceEventVersion == request.SourceEventVersion,
            cancellationToken);

        if (existingKey is not null)
        {
            var existing = await db.FinancialPackets.SingleAsync(packet => packet.Id == existingKey.FinancialPacketId, cancellationToken);
            return await BuildPacketDetailAsync(existing, cancellationToken);
        }

        var packet = new FinancialPacket
        {
            TenantId = tenantId,
            FinancialLegalEntityId = request.FinancialLegalEntityId,
            SourceProductKey = sourceProductKey,
            SourceEventId = NormalizeRequired(request.SourceEventId, "sourceEventId is required."),
            SourceEventVersion = request.SourceEventVersion <= 0 ? 1 : request.SourceEventVersion,
            SourceRecordType = NormalizeRequired(request.SourceRecordType, "sourceRecordType is required."),
            SourceRecordId = NormalizeRequired(request.SourceRecordId, "sourceRecordId is required."),
            SourceRecordDisplayName = NormalizeOptional(request.SourceRecordDisplayName, request.SourceRecordId),
            SourceOccurredAt = request.SourceOccurredAt,
            PacketType = NormalizeRequired(request.PacketType, "packetType is required.").ToLowerInvariant(),
            PacketSubType = NormalizeOptional(request.PacketSubType, string.Empty).ToLowerInvariant(),
            AccountingDate = request.AccountingDate,
            TransactionCurrency = NormalizeOptional(request.TransactionCurrency, "USD").ToUpperInvariant(),
            SourceAmount = request.SourceAmount,
            SourceTaxAmount = request.SourceTaxAmount,
            SourceTotalAmount = request.SourceTotalAmount,
            IdempotencyKey = request.IdempotencyKey.Trim(),
        };

        var issues = await ValidatePacketAsync(tenantId, packet, request, cancellationToken);
        packet.Status = issues.Count > 0
            ? "validation_failed"
            : packet.FinancialLegalEntityId is null ? "needs_mapping" : "received";

        db.FinancialPackets.Add(packet);
        db.FinancialPacketIdempotencyKeys.Add(new FinancialPacketIdempotencyKey
        {
            TenantId = tenantId,
            SourceProductKey = packet.SourceProductKey,
            SourceEventId = packet.SourceEventId,
            SourceEventVersion = packet.SourceEventVersion,
            IdempotencyKey = packet.IdempotencyKey,
            FinancialPacketId = packet.Id,
        });

        foreach (var line in request.Lines.OrderBy(line => line.LineNumber))
        {
            db.FinancialPacketLines.Add(new FinancialPacketLine
            {
                TenantId = tenantId,
                FinancialPacketId = packet.Id,
                LineNumber = line.LineNumber,
                SourceLineId = NormalizeOptional(line.SourceLineId, line.LineNumber.ToString()),
                LineType = NormalizeOptional(line.LineType, "item"),
                ItemRefId = line.ItemRef?.ObjectId,
                VendorRefId = line.VendorRef?.ObjectId,
                CustomerRefId = line.CustomerRef?.ObjectId,
                AssetRefId = line.AssetRef?.ObjectId,
                WorkOrderRefId = line.WorkOrderRef?.ObjectId,
                OrderRefId = line.OrderRef?.ObjectId,
                ShipmentRefId = line.ShipmentRef?.ObjectId,
                TripRefId = line.TripRef?.ObjectId,
                LocationRefId = line.LocationRef?.ObjectId,
                Quantity = line.Quantity,
                UnitOfMeasure = NormalizeOptional(line.UnitOfMeasure, string.Empty),
                UnitCost = line.UnitCost,
                ExtendedAmount = line.ExtendedAmount,
                TaxAmount = line.TaxAmount,
                TotalAmount = line.TotalAmount,
                DimensionSummary = FlattenHints(line.DimensionHints),
                CostBehavior = line.CostBehavior,
                CapitalizationHint = line.CapitalizationHint,
                BillableHint = line.BillableHint,
            });
        }

        foreach (var sourceRef in request.SourceRefs)
        {
            db.FinancialPacketSourceRefs.Add(new FinancialPacketSourceRef
            {
                TenantId = tenantId,
                FinancialPacketId = packet.Id,
                ProductKey = NormalizeRequired(sourceRef.ProductKey, "sourceRefs.productKey is required.").ToLowerInvariant(),
                SourceRecordType = NormalizeRequired(sourceRef.SourceRecordType, "sourceRefs.sourceRecordType is required."),
                SourceRecordId = NormalizeRequired(sourceRef.SourceRecordId, "sourceRefs.sourceRecordId is required."),
                SourceRecordDisplayName = NormalizeOptional(sourceRef.SourceRecordDisplayName, sourceRef.SourceRecordId),
                SourceEventId = sourceRef.SourceEventId,
                SourceVersion = sourceRef.SourceVersion,
                SnapshotLabel = NormalizeOptional(sourceRef.SnapshotLabel, sourceRef.SourceRecordDisplayName ?? sourceRef.SourceRecordId),
            });
        }

        foreach (var issue in issues)
        {
            db.FinancialPacketValidationIssues.Add(new FinancialPacketValidationIssue
            {
                TenantId = tenantId,
                FinancialPacketId = packet.Id,
                Code = issue.Code,
                Message = issue.Message,
                Severity = issue.Severity,
            });
        }

        db.FinancialPacketStatusHistory.Add(new FinancialPacketStatusHistory
        {
            TenantId = tenantId,
            FinancialPacketId = packet.Id,
            Status = packet.Status,
            Summary = issues.Count > 0 ? "Financial packet received with validation issues." : "Financial packet received.",
        });

        await AddAuditAsync(tenantId, principal, "packet.received", "financial_packet", packet.Id.ToString(), $"Received {packet.PacketType} packet from {packet.SourceProductKey}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await BuildPacketDetailAsync(packet, cancellationToken);
    }

    public async Task<FinancialPacketDetailResponse?> MapFinancialPacketAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var packet = await db.FinancialPackets.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id, cancellationToken);
        if (packet is null)
        {
            return null;
        }

        if (packet.Status == "validation_failed")
        {
            throw new StlApiException("ledgarr.packet.validation_failed", "Validation issues must be resolved before mapping.", 409);
        }

        if (packet.FinancialLegalEntityId is null)
        {
            throw new StlApiException("ledgarr.packet.financial_legal_entity_missing", "Financial Legal Entity is required before mapping.", 409);
        }

        packet.Status = "mapped";
        db.FinancialPacketMappingResults.Add(new FinancialPacketMappingResult
        {
            TenantId = tenantId,
            FinancialPacketId = packet.Id,
            Summary = "Resolved Financial Legal Entity, account, and dimension mappings.",
        });
        await AddPacketStatusAsync(tenantId, packet.Id, "mapped", "Financial packet mapped.", cancellationToken);
        await AddAuditAsync(tenantId, principal, "packet.mapped", "financial_packet", packet.Id.ToString(), "Mapped financial packet.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await BuildPacketDetailAsync(packet, cancellationToken);
    }

    public async Task<PostingPreviewResponse?> CreatePostingPreviewForPacketAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var packet = await db.FinancialPackets.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id, cancellationToken);
        if (packet is null)
        {
            return null;
        }

        var preview = await CreatePostingPreviewCoreAsync(principal, tenantId, packet, cancellationToken);
        packet.Status = "preview_ready";
        await AddPacketStatusAsync(tenantId, packet.Id, "preview_ready", "Posting preview generated.", cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await BuildPostingPreviewResponseAsync(preview, cancellationToken);
    }

    public async Task<PostingPreviewResponse> CreateAdHocPostingPreviewAsync(ClaimsPrincipal principal, PostingPreviewRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var packet = new FinancialPacket
        {
            TenantId = tenantId,
            FinancialLegalEntityId = request.FinancialLegalEntityId,
            SourceProductKey = ProductKey,
            SourceEventId = $"manual-preview-{Guid.NewGuid():N}",
            SourceEventVersion = 1,
            SourceRecordType = "manual_adjustment",
            SourceRecordId = $"manual-{Guid.NewGuid():N}",
            SourceRecordDisplayName = request.Description,
            PacketType = "manual_adjustment",
            AccountingDate = request.AccountingDate,
            SourceAmount = request.Amount,
            SourceTotalAmount = request.Amount,
            TransactionCurrency = NormalizeOptional(request.CurrencyCode, "USD"),
            IdempotencyKey = $"manual-preview-{Guid.NewGuid():N}",
            Status = "preview_ready",
        };

        db.FinancialPackets.Add(packet);
        var preview = await CreatePostingPreviewCoreAsync(principal, tenantId, packet, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await BuildPostingPreviewResponseAsync(preview, cancellationToken);
    }

    public async Task<PostingPreviewResponse?> ApprovePostingPreviewAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var preview = await db.PostingPreviews.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id, cancellationToken);
        if (preview is null)
        {
            return null;
        }

        preview.Status = "approved";
        preview.ApprovedAt = DateTimeOffset.UtcNow;
        await AddAuditAsync(tenantId, principal, "posting_preview.approved", "posting_preview", preview.Id.ToString(), "Approved posting preview.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await BuildPostingPreviewResponseAsync(preview, cancellationToken);
    }

    public async Task<JournalEntryResponse?> PostPostingPreviewAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var preview = await db.PostingPreviews.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id, cancellationToken);
        if (preview is null)
        {
            return null;
        }

        if (preview.Status != "approved")
        {
            throw new StlApiException("ledgarr.preview.not_approved", "Posting preview must be approved before posting.", 409);
        }

        var lines = await db.PostingPreviewLines.Where(l => l.PostingPreviewId == preview.Id).OrderBy(l => l.LineNumber).ToListAsync(cancellationToken);
        var packet = preview.FinancialPacketId is null
            ? null
            : await db.FinancialPackets.FirstOrDefaultAsync(p => p.Id == preview.FinancialPacketId && p.TenantId == tenantId, cancellationToken);
        var financialLegalEntityId = packet?.FinancialLegalEntityId
            ?? await ResolveDefaultFinancialLegalEntityIdAsync(tenantId, cancellationToken);
        var accountingDate = packet?.AccountingDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var period = await ResolveOpenPeriodAsync(tenantId, financialLegalEntityId, accountingDate, cancellationToken);
        var journal = await CreateJournalEntryFromLinesAsync(
            principal,
            tenantId,
            financialLegalEntityId,
            period.Id,
            accountingDate,
            packet is null ? "Manual posting preview" : $"Packet {packet.SourceRecordDisplayName}",
            lines.Select(line => new CreateJournalLineRequest(line.GLAccountId, line.Debit, line.Credit, line.Memo, line.DimensionSummary)).ToArray(),
            sourcePacketId: packet?.Id,
            sourceDocumentType: preview.SourceDocumentType,
            sourceDocumentId: preview.SourceDocumentId,
            cancellationToken);

        await PostJournalCoreAsync(principal, journal, cancellationToken);
        preview.Status = "posted";
        preview.PostedAt = DateTimeOffset.UtcNow;
        if (packet is not null)
        {
            packet.Status = "posted";
            packet.PostedJournalEntryId = journal.Id;
            db.FinancialPacketPostingResults.Add(new FinancialPacketPostingResult
            {
                TenantId = tenantId,
                FinancialPacketId = packet.Id,
                JournalEntryId = journal.Id,
                Status = "posted",
                Summary = "Packet posted to the general ledger.",
            });
            await AddPacketStatusAsync(tenantId, packet.Id, "posted", "Financial packet posted.", cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        return await BuildJournalResponseAsync(journal, cancellationToken);
    }

    public async Task<IReadOnlyList<BillableEventSummaryResponse>> ListBillableEventsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var entities = await db.FinancialLegalEntities
            .Where(entity => entity.TenantId == tenantId)
            .ToDictionaryAsync(entity => entity.Id, cancellationToken);
        var events = await db.BillableEvents
            .Where(item => item.TenantId == tenantId)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync(cancellationToken);

        return events
            .Select(item => new BillableEventSummaryResponse(
                item.Id,
                item.FinancialPacketId,
                item.FinancialLegalEntityId,
                item.FinancialLegalEntityId is { } entityId && entities.TryGetValue(entityId, out var entity) ? entity.DisplayName : "Unassigned entity",
                item.EventNumber,
                item.SourceProductKey,
                item.SourceRecordDisplayName,
                item.ChargeType,
                item.CustomerRefId,
                item.CustomerDisplayName,
                item.Amount,
                item.CurrencyCode,
                item.ApprovalStatus,
                item.InvoiceStatus,
                item.HoldReason,
                item.ExceptionReason,
                item.AccountingDate))
            .ToArray();
    }

    public async Task<BillableEvent> CreateBillableEventFromPacketAsync(ClaimsPrincipal principal, Guid packetId, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var existing = await db.BillableEvents.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.FinancialPacketId == packetId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var packet = await db.FinancialPackets.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == packetId, cancellationToken)
            ?? throw new StlApiException("ledgarr.billing.packet_missing", "Financial packet could not be resolved for billing intake.", 404);
        if (!IsBillableSourceProduct(packet.SourceProductKey))
        {
            throw new StlApiException("ledgarr.billing.source_not_billable", "This source product does not support LedgArr billing intake.", 409);
        }

        var firstLine = await db.FinancialPacketLines
            .Where(line => line.TenantId == tenantId && line.FinancialPacketId == packetId)
            .OrderBy(line => line.LineNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var chargeType = NormalizeOptional(firstLine?.BillableHint, packet.PacketType).ToLowerInvariant();
        var customerRefId = firstLine?.CustomerRefId;
        var billableEvent = new BillableEvent
        {
            TenantId = tenantId,
            FinancialPacketId = packet.Id,
            FinancialLegalEntityId = packet.FinancialLegalEntityId,
            EventNumber = await GenerateBillableEventNumberAsync(tenantId, cancellationToken),
            SourceProductKey = packet.SourceProductKey,
            SourceRecordDisplayName = packet.SourceRecordDisplayName,
            ChargeType = chargeType,
            CustomerRefId = customerRefId,
            CustomerDisplayName = string.IsNullOrWhiteSpace(customerRefId) ? "Customer reference required" : customerRefId,
            Amount = packet.SourceTotalAmount,
            CurrencyCode = packet.TransactionCurrency,
            AccountingDate = packet.AccountingDate,
            ExceptionReason = string.IsNullOrWhiteSpace(customerRefId) ? "customer_ref_missing" : null,
            CreatedByPersonId = principal.GetPersonId(),
        };

        db.BillableEvents.Add(billableEvent);
        await AddAuditAsync(
            tenantId,
            principal,
            "billable_event.created",
            "billable_event",
            billableEvent.Id.ToString(),
            $"Created billable event {billableEvent.EventNumber} from packet {packet.SourceRecordDisplayName}.",
            cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return billableEvent;
    }

    public async Task<BillableEvent?> ApproveBillableEventAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var billableEvent = await db.BillableEvents.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Id == id, cancellationToken);
        if (billableEvent is null)
        {
            return null;
        }

        billableEvent.ApprovalStatus = "approved";
        billableEvent.HoldReason = null;
        await AddAuditAsync(tenantId, principal, "billable_event.approved", "billable_event", billableEvent.Id.ToString(), $"Approved billable event {billableEvent.EventNumber}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return billableEvent;
    }

    public async Task<BillableEvent?> HoldBillableEventAsync(ClaimsPrincipal principal, Guid id, string? reason, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var billableEvent = await db.BillableEvents.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Id == id, cancellationToken);
        if (billableEvent is null)
        {
            return null;
        }

        var normalizedReason = NormalizeRequired(reason, "A hold reason is required for billable events.");
        billableEvent.ApprovalStatus = "held";
        billableEvent.HoldReason = normalizedReason;
        await AddAuditAsync(tenantId, principal, "billable_event.held", "billable_event", billableEvent.Id.ToString(), $"Placed billable event {billableEvent.EventNumber} on hold.", normalizedReason, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return billableEvent;
    }

    public async Task<BillableEvent?> GenerateInvoiceDraftForBillableEventAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var billableEvent = await db.BillableEvents.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Id == id, cancellationToken);
        if (billableEvent is null)
        {
            return null;
        }

        if (!IgnoreCase.Equals(billableEvent.ApprovalStatus, "approved"))
        {
            throw new StlApiException("ledgarr.billing.not_approved", "Billable events must be approved before invoice draft generation.", 409);
        }

        if (string.IsNullOrWhiteSpace(billableEvent.CustomerRefId))
        {
            throw new StlApiException("ledgarr.billing.customer_ref_missing", "Customer reference is required before invoice draft generation.", 409);
        }

        billableEvent.InvoiceStatus = "draft_generated";
        await AddAuditAsync(tenantId, principal, "billable_event.invoice_draft_generated", "billable_event", billableEvent.Id.ToString(), $"Generated invoice draft signal for billable event {billableEvent.EventNumber}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return billableEvent;
    }

    public async Task<IReadOnlyList<JournalEntryResponse>> ListJournalsAsync(ClaimsPrincipal principal, string? status, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var query = db.JournalEntries.Where(j => j.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(j => j.Status == status.Trim().ToLowerInvariant());
        }

        var journals = await query.OrderByDescending(j => j.CreatedAt).ToListAsync(cancellationToken);
        var responses = new List<JournalEntryResponse>(journals.Count);
        foreach (var journal in journals)
        {
            responses.Add(await BuildJournalResponseAsync(journal, cancellationToken));
        }

        return responses;
    }

    public async Task<IReadOnlyList<ApprovalPolicySummaryResponse>> ListApprovalPoliciesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureBootstrapAsync(tenantId, cancellationToken);

        var policies = await db.ApprovalPolicies
            .Where(policy => policy.TenantId == tenantId)
            .OrderBy(policy => policy.PolicyKey)
            .ToListAsync(cancellationToken);
        var policyIds = policies.Select(policy => policy.Id).ToArray();
        var steps = await db.ApprovalSteps
            .Where(step => step.TenantId == tenantId && policyIds.Contains(step.ApprovalPolicyId))
            .OrderBy(step => step.StepNumber)
            .ToListAsync(cancellationToken);
        var stepsByPolicyId = steps
            .GroupBy(step => step.ApprovalPolicyId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<ApprovalStepSummaryResponse>)group
                    .Select(step => new ApprovalStepSummaryResponse(step.StepNumber, step.RequiredPermissionKey))
                    .ToArray());

        return policies
            .Select(policy => new ApprovalPolicySummaryResponse(
                policy.Id,
                policy.PolicyKey,
                policy.AppliesTo,
                policy.RequiresApproval,
                stepsByPolicyId.TryGetValue(policy.Id, out var policySteps) ? policySteps : Array.Empty<ApprovalStepSummaryResponse>()))
            .ToArray();
    }

    public async Task<IReadOnlyList<SegregationOfDutiesRuleResponse>> ListSegregationOfDutiesRulesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureBootstrapAsync(tenantId, cancellationToken);

        return await db.SegregationOfDutiesRules
            .Where(rule => rule.TenantId == tenantId)
            .OrderBy(rule => rule.RuleKey)
            .Select(rule => new SegregationOfDutiesRuleResponse(
                rule.Id,
                rule.RuleKey,
                rule.IncompatibleActions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FinancialAuditEventResponse>> ListFinancialAuditEventsAsync(
        ClaimsPrincipal principal,
        string? targetType = null,
        int take = 25,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureBootstrapAsync(tenantId, cancellationToken);

        var query = db.FinancialAuditEvents.Where(entry => entry.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(targetType))
        {
            query = query.Where(entry => entry.TargetType == targetType.Trim().ToLowerInvariant());
        }

        return await query
            .OrderByDescending(entry => entry.OccurredAt)
            .Take(Math.Clamp(take, 1, 100))
            .Select(entry => new FinancialAuditEventResponse(
                entry.Id,
                entry.ProductKey,
                entry.Action,
                entry.TargetType,
                entry.TargetId,
                entry.ActorId,
                entry.Summary,
                entry.Reason,
                entry.OccurredAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JournalAttachmentRefResponse>> ListJournalAttachmentRefsAsync(
        ClaimsPrincipal principal,
        Guid? journalEntryId = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureBootstrapAsync(tenantId, cancellationToken);

        var query =
            from attachment in db.JournalAttachmentRefs
            join journal in db.JournalEntries on attachment.JournalEntryId equals journal.Id
            where attachment.TenantId == tenantId && journal.TenantId == tenantId
            select new { attachment, journal };

        if (journalEntryId.HasValue)
        {
            query = query.Where(item => item.attachment.JournalEntryId == journalEntryId.Value);
        }

        return await query
            .OrderByDescending(item => item.journal.AccountingDate)
            .ThenBy(item => item.journal.JournalNumber)
            .ThenBy(item => item.attachment.DisplayName)
            .Select(item => new JournalAttachmentRefResponse(
                item.attachment.Id,
                item.attachment.JournalEntryId,
                item.journal.JournalNumber,
                item.attachment.RecordArrDocumentId,
                item.attachment.DisplayName))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JournalAuditTrailResponse>> ListJournalAuditTrailsAsync(
        ClaimsPrincipal principal,
        Guid? journalEntryId = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureBootstrapAsync(tenantId, cancellationToken);

        var query =
            from trail in db.JournalAuditTrails
            join journal in db.JournalEntries on trail.JournalEntryId equals journal.Id
            where trail.TenantId == tenantId && journal.TenantId == tenantId
            select new { trail, journal };

        if (journalEntryId.HasValue)
        {
            query = query.Where(item => item.trail.JournalEntryId == journalEntryId.Value);
        }

        return await query
            .OrderByDescending(item => item.trail.OccurredAt)
            .Select(item => new JournalAuditTrailResponse(
                item.trail.Id,
                item.trail.JournalEntryId,
                item.journal.JournalNumber,
                item.trail.Action,
                item.trail.ActorId,
                item.trail.Summary,
                item.trail.OccurredAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<JournalEntryResponse?> GetJournalAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var journal = await db.JournalEntries.FirstOrDefaultAsync(j => j.TenantId == tenantId && j.Id == id, cancellationToken);
        return journal is null ? null : await BuildJournalResponseAsync(journal, cancellationToken);
    }

    public async Task<JournalEntryResponse> CreateJournalAsync(ClaimsPrincipal principal, CreateJournalRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var period = await ResolveOpenPeriodAsync(tenantId, request.FinancialLegalEntityId, request.AccountingDate, cancellationToken);
        var journal = await CreateJournalEntryFromLinesAsync(
            principal,
            tenantId,
            request.FinancialLegalEntityId,
            period.Id,
            request.AccountingDate,
            NormalizeRequired(request.Description, "Journal description is required."),
            request.Lines,
            null,
            "manual_journal",
            null,
            cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await BuildJournalResponseAsync(journal, cancellationToken);
    }

    public async Task<JournalEntryResponse?> SubmitJournalAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var journal = await ChangeJournalStatusAsync(principal, id, "submitted", "journal.submitted", cancellationToken);
        return journal is null ? null : await BuildJournalResponseAsync(journal, cancellationToken);
    }

    public async Task<JournalEntryResponse?> ApproveJournalAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var journal = await ChangeJournalStatusAsync(principal, id, "approved", "journal.approved", cancellationToken);
        return journal is null ? null : await BuildJournalResponseAsync(journal, cancellationToken);
    }

    public async Task<JournalEntryResponse?> PostJournalAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var journal = await db.JournalEntries.FirstOrDefaultAsync(j => j.TenantId == tenantId && j.Id == id, cancellationToken);
        if (journal is null)
        {
            return null;
        }

        await PostJournalCoreAsync(principal, journal, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await BuildJournalResponseAsync(journal, cancellationToken);
    }

    public async Task<JournalEntryResponse?> ReverseJournalAsync(ClaimsPrincipal principal, Guid id, ReverseJournalRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var original = await db.JournalEntries.FirstOrDefaultAsync(j => j.TenantId == tenantId && j.Id == id, cancellationToken);
        if (original is null)
        {
            return null;
        }

        if (original.Status != "posted")
        {
            throw new StlApiException("ledgarr.journal.not_posted", "Only posted journal entries can be reversed.", 409);
        }

        var originalLines = await db.JournalLines.Where(l => l.JournalEntryId == original.Id).OrderBy(l => l.LineNumber).ToListAsync(cancellationToken);
        var accountingDate = request.AccountingDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var period = await ResolveOpenPeriodAsync(tenantId, original.FinancialLegalEntityId, accountingDate, cancellationToken);
        var reversal = await CreateJournalEntryFromLinesAsync(
            principal,
            tenantId,
            original.FinancialLegalEntityId,
            period.Id,
            accountingDate,
            $"Reversal of {original.JournalNumber}: {request.Reason}",
            originalLines.Select(line => new CreateJournalLineRequest(line.GLAccountId, line.Credit, line.Debit, $"Reversal: {line.Memo}", line.DimensionSummary)).ToArray(),
            null,
            "journal_reversal",
            original.Id,
            cancellationToken);
        reversal.ReversalOfJournalEntryId = original.Id;
        await PostJournalCoreAsync(principal, reversal, cancellationToken);
        original.Status = "reversed";
        db.JournalEntryReversals.Add(new JournalEntryReversal
        {
            TenantId = tenantId,
            OriginalJournalEntryId = original.Id,
            ReversalJournalEntryId = reversal.Id,
            Reason = NormalizeRequired(request.Reason, "Reversal reason is required."),
        });
        await AddAuditAsync(tenantId, principal, "journal.reversed", "journal_entry", original.Id.ToString(), $"Reversed journal {original.JournalNumber}.", request.Reason, cancellationToken);
        await AddJournalAuditTrailAsync(tenantId, original.Id, principal, "journal.reversed", $"Reversed journal {original.JournalNumber}.", cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await BuildJournalResponseAsync(reversal, cancellationToken);
    }

    public async Task<JournalAttachmentRefResponse?> CreateJournalAttachmentRefAsync(
        ClaimsPrincipal principal,
        Guid journalEntryId,
        CreateJournalAttachmentRefRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var journal = await db.JournalEntries.FirstOrDefaultAsync(
            entry => entry.TenantId == tenantId && entry.Id == journalEntryId,
            cancellationToken);
        if (journal is null)
        {
            return null;
        }

        var recordArrDocumentId = NormalizeRequired(request.RecordArrDocumentId, "RecordArr document id is required.");
        var displayName = NormalizeRequired(request.DisplayName, "Attachment display name is required.");
        var existing = await db.JournalAttachmentRefs.FirstOrDefaultAsync(
            attachment => attachment.TenantId == tenantId
                && attachment.JournalEntryId == journalEntryId
                && attachment.RecordArrDocumentId == recordArrDocumentId,
            cancellationToken);
        if (existing is not null)
        {
            return new JournalAttachmentRefResponse(
                existing.Id,
                existing.JournalEntryId,
                journal.JournalNumber,
                existing.RecordArrDocumentId,
                existing.DisplayName);
        }

        var attachment = new JournalAttachmentRef
        {
            TenantId = tenantId,
            JournalEntryId = journalEntryId,
            RecordArrDocumentId = recordArrDocumentId,
            DisplayName = displayName,
        };
        db.JournalAttachmentRefs.Add(attachment);

        var summary = $"Linked RecordArr document {displayName} to journal {journal.JournalNumber}.";
        await AddJournalAuditTrailAsync(tenantId, journalEntryId, principal, "journal.attachment_linked", summary, cancellationToken);
        await AddAuditAsync(tenantId, principal, "journal.attachment_linked", "journal_entry", journalEntryId.ToString(), summary, cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new JournalAttachmentRefResponse(
            attachment.Id,
            attachment.JournalEntryId,
            journal.JournalNumber,
            attachment.RecordArrDocumentId,
            attachment.DisplayName);
    }

    public async Task<VendorBill> CreateVendorBillAsync(ClaimsPrincipal principal, CreateVendorBillRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureFinancialLegalEntityExistsAsync(tenantId, request.FinancialLegalEntityId, cancellationToken);
        if (!string.Equals(request.VendorRef.ProductKey, "supplyarr", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("ledgarr.vendor_bill.vendor_source_invalid", "Vendor bills must reference SupplyArr-owned vendor records.", 400);
        }

        var bill = new VendorBill
        {
            TenantId = tenantId,
            FinancialLegalEntityId = request.FinancialLegalEntityId,
            VendorRefProductKey = "supplyarr",
            VendorRefId = request.VendorRef.ObjectId,
            VendorDisplayName = NormalizeRequired(request.VendorDisplayName, "Vendor display name is required."),
            VendorInvoiceNumber = NormalizeRequired(request.VendorInvoiceNumber, "Vendor invoice number is required."),
            BillDate = request.BillDate,
            DueDate = request.DueDate,
            CurrencyCode = NormalizeOptional(request.CurrencyCode, "USD"),
            Subtotal = request.Subtotal,
            TaxAmount = request.TaxAmount,
            TotalAmount = request.TotalAmount,
        };

        db.VendorBills.Add(bill);
        foreach (var line in request.Lines)
        {
            db.VendorBillLines.Add(new VendorBillLine
            {
                TenantId = tenantId,
                VendorBillId = bill.Id,
                LineNumber = line.LineNumber,
                Description = NormalizeOptional(line.Description, "Vendor bill line"),
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                Amount = line.Amount,
            });
        }

        await AddAuditAsync(tenantId, principal, "vendor_bill.created", "vendor_bill", bill.Id.ToString(), $"Created AP bill {bill.VendorInvoiceNumber}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return bill;
    }

    public async Task<IReadOnlyList<VendorBill>> ListVendorBillsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        return await db.VendorBills.Where(b => b.TenantId == tenantId).OrderByDescending(b => b.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<VendorBill?> MatchVendorBillAsync(ClaimsPrincipal principal, Guid id, MatchVendorBillRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var bill = await db.VendorBills.FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == id, cancellationToken);
        if (bill is null)
        {
            return null;
        }

        var variance = bill.TotalAmount - request.ExpectedAmount;
        bill.MatchStatus = Math.Abs(variance) <= request.AllowedVariance
            ? "matched"
            : "variance_open";
        db.VendorBillMatches.Add(new VendorBillMatch
        {
            TenantId = tenantId,
            VendorBillId = bill.Id,
            SourceProductKey = request.SourceRef.ProductKey,
            SourceRecordType = request.SourceRef.ObjectType,
            SourceRecordId = request.SourceRef.ObjectId,
            ExpectedAmount = request.ExpectedAmount,
            ActualAmount = bill.TotalAmount,
            VarianceAmount = variance,
            Status = bill.MatchStatus,
        });

        if (bill.MatchStatus == "variance_open")
        {
            db.VendorBillVariances.Add(new VendorBillVariance
            {
                TenantId = tenantId,
                VendorBillId = bill.Id,
                VarianceCode = "amount_variance",
                VarianceAmount = variance,
            });
        }

        await AddAuditAsync(tenantId, principal, "vendor_bill.matched", "vendor_bill", bill.Id.ToString(), $"Matched AP bill with status {bill.MatchStatus}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return bill;
    }

    public async Task<VendorBill?> ApproveVendorBillAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var bill = await db.VendorBills.FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == id, cancellationToken);
        if (bill is null)
        {
            return null;
        }

        if (bill.MatchStatus == "variance_open")
        {
            throw new StlApiException("ledgarr.vendor_bill.unresolved_variance", "AP bill cannot be approved while match variance is unresolved.", 409);
        }

        bill.Status = "approved";
        db.VendorBillApprovals.Add(new VendorBillApproval
        {
            TenantId = tenantId,
            VendorBillId = bill.Id,
            Decision = "approved",
            ActorPersonId = principal.GetPersonId().ToString(),
        });
        await AddAuditAsync(tenantId, principal, "vendor_bill.approved", "vendor_bill", bill.Id.ToString(), $"Approved AP bill {bill.VendorInvoiceNumber}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return bill;
    }

    public async Task<VendorBill?> PostVendorBillAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var bill = await db.VendorBills.FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == id, cancellationToken);
        if (bill is null)
        {
            return null;
        }

        if (bill.Status != "approved" || bill.MatchStatus == "variance_open")
        {
            throw new StlApiException("ledgarr.vendor_bill.not_ready", "AP bill requires approval and resolved match variance before posting.", 409);
        }

        var period = await ResolveOpenPeriodAsync(tenantId, bill.FinancialLegalEntityId, bill.BillDate, cancellationToken);
        var expense = await GetOrCreateAccountAsync(tenantId, "5000", "Operating Expense", "expense", "debit", cancellationToken);
        var ap = await GetOrCreateAccountAsync(tenantId, "2000", "Accounts Payable", "liability", "credit", cancellationToken);
        var journal = await CreateJournalEntryFromLinesAsync(
            principal,
            tenantId,
            bill.FinancialLegalEntityId,
            period.Id,
            bill.BillDate,
            $"AP bill {bill.VendorInvoiceNumber}",
            [
                new CreateJournalLineRequest(expense.Id, bill.TotalAmount, 0, $"AP expense {bill.VendorDisplayName}", null),
                new CreateJournalLineRequest(ap.Id, 0, bill.TotalAmount, $"AP liability {bill.VendorDisplayName}", null),
            ],
            null,
            "vendor_bill",
            bill.Id,
            cancellationToken);
        await PostJournalCoreAsync(principal, journal, cancellationToken);
        bill.Status = "posted";
        bill.PostedJournalEntryId = journal.Id;
        await AddAuditAsync(tenantId, principal, "vendor_bill.posted", "vendor_bill", bill.Id.ToString(), $"Posted AP bill {bill.VendorInvoiceNumber}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return bill;
    }

    public async Task<IReadOnlyList<AgingBucketResponse>> GetAPAgingAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var bills = await db.VendorBills.Where(b => b.TenantId == tenantId && b.Status != "paid" && b.Status != "voided").ToListAsync(cancellationToken);
        return BuildAging(bills.Select(b => (b.DueDate, b.TotalAmount)), today);
    }

    public async Task<PaymentRun> CreatePaymentRunAsync(ClaimsPrincipal principal, PaymentRunRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var bills = await db.VendorBills.Where(b => b.TenantId == tenantId && request.VendorBillIds.Contains(b.Id)).ToListAsync(cancellationToken);
        var run = new PaymentRun
        {
            TenantId = tenantId,
            PaymentRunNumber = await NextNumberAsync(tenantId, "PAYRUN", "PAYRUN", cancellationToken),
            TotalAmount = bills.Sum(b => b.TotalAmount),
        };
        db.PaymentRuns.Add(run);
        await AddAuditAsync(tenantId, principal, "payment_run.created", "payment_run", run.Id.ToString(), $"Created payment run {run.PaymentRunNumber}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return run;
    }

    public async Task<IReadOnlyList<PaymentRunSummaryResponse>> ListPaymentRunsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var runs = await db.PaymentRuns
            .Where(run => run.TenantId == tenantId)
            .OrderByDescending(run => run.PaymentRunNumber)
            .ToListAsync(cancellationToken);
        var exports = await db.PaymentExportBatches
            .Where(batch => batch.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return runs
            .Select(run =>
            {
                var latestExport = exports
                    .Where(batch => batch.PaymentRunId == run.Id)
                    .OrderByDescending(batch => batch.CreatedAt)
                    .FirstOrDefault();
                return new PaymentRunSummaryResponse(
                    run.Id,
                    run.PaymentRunNumber,
                    run.Status,
                    run.TotalAmount,
                    latestExport?.CreatedAt,
                    latestExport?.Status);
            })
            .ToArray();
    }

    public async Task<PaymentRun?> ApprovePaymentRunAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var run = await db.PaymentRuns.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id, cancellationToken);
        if (run is null)
        {
            return null;
        }

        run.Status = "approved";
        await db.SaveChangesAsync(cancellationToken);
        return run;
    }

    public async Task<PaymentExportBatch?> ExportPaymentRunAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var run = await db.PaymentRuns.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id, cancellationToken);
        if (run is null)
        {
            return null;
        }

        if (run.Status != "approved")
        {
            throw new StlApiException("ledgarr.payment_run.not_approved", "Payment run must be approved before export.", 409);
        }

        run.Status = "exported";
        var export = new PaymentExportBatch { TenantId = tenantId, PaymentRunId = run.Id, Status = "exported" };
        db.PaymentExportBatches.Add(export);
        await AddAuditAsync(tenantId, principal, "payment_run.exported", "payment_run", run.Id.ToString(), $"Exported payment run {run.PaymentRunNumber}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return export;
    }

    public async Task<CustomerInvoice> CreateCustomerInvoiceAsync(ClaimsPrincipal principal, CreateCustomerInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureFinancialLegalEntityExistsAsync(tenantId, request.FinancialLegalEntityId, cancellationToken);
        if (!string.Equals(request.CustomerRef.ProductKey, "customarr", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("ledgarr.customer_invoice.customer_source_invalid", "Customer invoices must reference CustomArr-owned customer records.", 400);
        }

        var invoice = new CustomerInvoice
        {
            TenantId = tenantId,
            FinancialLegalEntityId = request.FinancialLegalEntityId,
            CustomerRefProductKey = "customarr",
            CustomerRefId = request.CustomerRef.ObjectId,
            CustomerDisplayName = NormalizeRequired(request.CustomerDisplayName, "Customer display name is required."),
            InvoiceNumber = string.IsNullOrWhiteSpace(request.InvoiceNumber)
                ? await NextNumberAsync(tenantId, "ARINV", "INV", cancellationToken)
                : request.InvoiceNumber.Trim(),
            InvoiceDate = request.InvoiceDate,
            DueDate = request.DueDate,
            CurrencyCode = NormalizeOptional(request.CurrencyCode, "USD"),
            Subtotal = request.Subtotal,
            TaxAmount = request.TaxAmount,
            TotalAmount = request.TotalAmount,
        };

        db.CustomerInvoices.Add(invoice);
        foreach (var line in request.Lines)
        {
            db.CustomerInvoiceLines.Add(new CustomerInvoiceLine
            {
                TenantId = tenantId,
                CustomerInvoiceId = invoice.Id,
                LineNumber = line.LineNumber,
                Description = NormalizeOptional(line.Description, "Customer invoice line"),
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                Amount = line.Amount,
            });
        }

        await AddAuditAsync(tenantId, principal, "customer_invoice.created", "customer_invoice", invoice.Id.ToString(), $"Created AR invoice {invoice.InvoiceNumber}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return invoice;
    }

    public async Task<IReadOnlyList<CustomerInvoice>> ListCustomerInvoicesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        return await db.CustomerInvoices.Where(i => i.TenantId == tenantId).OrderByDescending(i => i.InvoiceDate).ToListAsync(cancellationToken);
    }

    public async Task<CustomerInvoice?> ApproveCustomerInvoiceAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default) =>
        await ChangeInvoiceStatusAsync(principal, id, "approved", "customer_invoice.approved", cancellationToken);

    public async Task<CustomerInvoice?> IssueCustomerInvoiceAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default) =>
        await ChangeInvoiceStatusAsync(principal, id, "issued", "customer_invoice.issued", cancellationToken);

    public async Task<CustomerInvoice?> PostCustomerInvoiceAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var invoice = await db.CustomerInvoices.FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == id, cancellationToken);
        if (invoice is null)
        {
            return null;
        }

        if (invoice.Status is not "issued" and not "approved")
        {
            throw new StlApiException("ledgarr.customer_invoice.not_issued", "Customer invoice must be approved or issued before posting.", 409);
        }

        var period = await ResolveOpenPeriodAsync(tenantId, invoice.FinancialLegalEntityId, invoice.InvoiceDate, cancellationToken);
        var ar = await GetOrCreateAccountAsync(tenantId, "1100", "Accounts Receivable", "asset", "debit", cancellationToken);
        var revenue = await GetOrCreateAccountAsync(tenantId, "4000", "Revenue", "revenue", "credit", cancellationToken);
        var tax = await GetOrCreateAccountAsync(tenantId, "2200", "Tax Payable", "liability", "credit", cancellationToken);
        var lines = new List<CreateJournalLineRequest>
        {
            new(ar.Id, invoice.TotalAmount, 0, $"AR invoice {invoice.InvoiceNumber}", null),
            new(revenue.Id, 0, invoice.Subtotal, $"Revenue {invoice.CustomerDisplayName}", null),
        };
        if (invoice.TaxAmount != 0)
        {
            lines.Add(new CreateJournalLineRequest(tax.Id, 0, invoice.TaxAmount, "Sales tax payable", null));
        }

        var journal = await CreateJournalEntryFromLinesAsync(principal, tenantId, invoice.FinancialLegalEntityId, period.Id, invoice.InvoiceDate, $"AR invoice {invoice.InvoiceNumber}", lines, null, "customer_invoice", invoice.Id, cancellationToken);
        await PostJournalCoreAsync(principal, journal, cancellationToken);
        invoice.Status = "posted";
        invoice.PostedJournalEntryId = journal.Id;
        await AddAuditAsync(tenantId, principal, "customer_invoice.posted", "customer_invoice", invoice.Id.ToString(), $"Posted AR invoice {invoice.InvoiceNumber}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return invoice;
    }

    public async Task<CustomerPayment> CreateCustomerPaymentAsync(ClaimsPrincipal principal, CreateCustomerPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var payment = new CustomerPayment
        {
            TenantId = tenantId,
            FinancialLegalEntityId = request.FinancialLegalEntityId,
            CustomerRefId = request.CustomerRef.ObjectId,
            PaymentNumber = await NextNumberAsync(tenantId, "ARPAY", "PAY", cancellationToken),
            Amount = request.Amount,
        };
        db.CustomerPayments.Add(payment);
        foreach (var application in request.Applications)
        {
            db.CustomerPaymentApplications.Add(new CustomerPaymentApplication
            {
                TenantId = tenantId,
                CustomerPaymentId = payment.Id,
                CustomerInvoiceId = application.CustomerInvoiceId,
                AppliedAmount = application.AppliedAmount,
            });
        }

        await AddAuditAsync(tenantId, principal, "customer_payment.recorded", "customer_payment", payment.Id.ToString(), $"Recorded customer payment {payment.PaymentNumber}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<IReadOnlyList<CustomerPaymentSummaryResponse>> ListCustomerPaymentsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var payments = await db.CustomerPayments
            .Where(payment => payment.TenantId == tenantId)
            .OrderByDescending(payment => payment.PaymentNumber)
            .ToListAsync(cancellationToken);
        var applications = await db.CustomerPaymentApplications
            .Where(application => application.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return payments
            .Select(payment => new CustomerPaymentSummaryResponse(
                payment.Id,
                payment.FinancialLegalEntityId,
                payment.CustomerRefId,
                payment.PaymentNumber,
                payment.Amount,
                payment.Status,
                applications.Where(application => application.CustomerPaymentId == payment.Id).Sum(application => application.AppliedAmount)))
            .ToArray();
    }

    public async Task<IReadOnlyList<AgingBucketResponse>> GetARAgingAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var invoices = await db.CustomerInvoices.Where(i => i.TenantId == tenantId && (i.Status == "issued" || i.Status == "posted")).ToListAsync(cancellationToken);
        return BuildAging(invoices.Select(i => (i.DueDate, i.TotalAmount)), today);
    }

    public async Task<IReadOnlyList<BankAccountSummaryResponse>> ListBankAccountsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var accounts = await db.BankAccounts
            .Where(account => account.TenantId == tenantId)
            .OrderBy(account => account.AccountDisplayName)
            .ToListAsync(cancellationToken);
        var legalEntities = await db.FinancialLegalEntities
            .Where(entity => entity.TenantId == tenantId)
            .ToDictionaryAsync(entity => entity.Id, cancellationToken);
        var glAccounts = await db.GLAccounts
            .Where(account => account.TenantId == tenantId)
            .ToDictionaryAsync(account => account.Id, cancellationToken);

        return accounts
            .Select(account => new BankAccountSummaryResponse(
                account.Id,
                account.FinancialLegalEntityId,
                legalEntities.TryGetValue(account.FinancialLegalEntityId, out var entity) ? entity.DisplayName : "Unknown entity",
                account.BankName,
                account.AccountDisplayName,
                account.AccountType,
                account.MaskedAccountNumber,
                account.CurrencyCode,
                account.GLCashAccountId,
                glAccounts.TryGetValue(account.GLCashAccountId, out var glAccount) ? glAccount.AccountCode : "unknown",
                account.Status,
                account.ReconciliationEnabled))
            .ToArray();
    }

    public async Task<BankAccount> CreateBankAccountAsync(ClaimsPrincipal principal, CreateBankAccountRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureFinancialLegalEntityExistsAsync(tenantId, request.FinancialLegalEntityId, cancellationToken);
        var glAccount = await db.GLAccounts.FirstOrDefaultAsync(account => account.TenantId == tenantId && account.Id == request.GLCashAccountId, cancellationToken);
        if (glAccount is null)
        {
            throw new StlApiException("ledgarr.bank_account.cash_account_missing", "The selected GL cash account could not be found.", 404);
        }

        var account = new BankAccount
        {
            TenantId = tenantId,
            FinancialLegalEntityId = request.FinancialLegalEntityId,
            BankName = NormalizeRequired(request.BankName, "Bank name is required."),
            AccountDisplayName = NormalizeRequired(request.AccountDisplayName, "Account display name is required."),
            AccountType = NormalizeOptional(request.AccountType, "checking"),
            MaskedAccountNumber = NormalizeRequired(request.MaskedAccountNumber, "Masked account number is required."),
            CurrencyCode = NormalizeOptional(request.CurrencyCode, "USD").ToUpperInvariant(),
            GLCashAccountId = request.GLCashAccountId,
            ReconciliationEnabled = request.ReconciliationEnabled,
        };
        db.BankAccounts.Add(account);
        await AddAuditAsync(tenantId, principal, "bank_account.created", "bank_account", account.Id.ToString(), $"Created bank account {account.AccountDisplayName}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<IReadOnlyList<BankTransactionSummaryResponse>> ListBankTransactionsAsync(ClaimsPrincipal principal, Guid? bankAccountId, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var query = db.BankTransactions.Where(transaction => transaction.TenantId == tenantId);
        if (bankAccountId.HasValue)
        {
            query = query.Where(transaction => transaction.BankAccountId == bankAccountId.Value);
        }

        var transactions = await query
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Amount)
            .ToListAsync(cancellationToken);
        var accounts = await db.BankAccounts
            .Where(account => account.TenantId == tenantId)
            .ToDictionaryAsync(account => account.Id, cancellationToken);

        return transactions
            .Select(transaction => new BankTransactionSummaryResponse(
                transaction.Id,
                transaction.BankAccountId,
                accounts.TryGetValue(transaction.BankAccountId, out var account) ? account.AccountDisplayName : "Unknown account",
                transaction.TransactionDate,
                transaction.Description,
                transaction.Amount,
                transaction.Direction,
                transaction.SourceType,
                transaction.MatchStatus,
                transaction.PurchaseOrderRefProductKey,
                transaction.PurchaseOrderRefType,
                transaction.PurchaseOrderRefId,
                transaction.PurchaseOrderRefDisplayName,
                transaction.PurchaseOrderApprovedAmountSnapshot,
                transaction.PurchaseOrderVarianceAmount,
                transaction.PurchaseOrderAmountStatus,
                transaction.ReconciliationStatus,
                transaction.ReconciliationId))
            .ToArray();
    }

    public async Task<BankTransactionSummaryResponse> CreateBankTransactionAsync(ClaimsPrincipal principal, CreateBankTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var account = await db.BankAccounts.FirstOrDefaultAsync(bankAccount => bankAccount.TenantId == tenantId && bankAccount.Id == request.BankAccountId, cancellationToken);
        if (account is null)
        {
            throw new StlApiException("ledgarr.bank_transaction.account_missing", "The selected bank account could not be found.", 404);
        }

        if (request.PurchaseOrderRef is not null)
        {
            if (!string.Equals(request.PurchaseOrderRef.ProductKey, "supplyarr", StringComparison.OrdinalIgnoreCase))
            {
                throw new StlApiException("ledgarr.bank_transaction.purchase_order_ref_product_invalid", "Purchase order references must come from SupplyArr.", 400);
            }

            if (!string.Equals(request.PurchaseOrderRef.ObjectType, "purchase_order", StringComparison.OrdinalIgnoreCase))
            {
                throw new StlApiException("ledgarr.bank_transaction.purchase_order_ref_type_invalid", "Purchase order references must use the purchase_order object type.", 400);
            }
        }
        else if (request.PurchaseOrderApprovedAmountSnapshot.HasValue)
        {
            throw new StlApiException("ledgarr.bank_transaction.purchase_order_amount_requires_ref", "A purchase order reference is required before an approved amount snapshot can be recorded.", 400);
        }

        if (request.PurchaseOrderApprovedAmountSnapshot.HasValue && request.PurchaseOrderApprovedAmountSnapshot.Value < 0)
        {
            throw new StlApiException("ledgarr.bank_transaction.purchase_order_amount_invalid", "Purchase order approved amount snapshot cannot be negative.", 400);
        }

        var comparableAmount = Math.Abs(request.Amount);
        var poAmountStatus = "not_applicable";
        decimal? poVarianceAmount = null;
        if (request.PurchaseOrderRef is not null)
        {
            poAmountStatus = request.PurchaseOrderApprovedAmountSnapshot.HasValue ? "matched" : "reference_only";
            if (request.PurchaseOrderApprovedAmountSnapshot.HasValue)
            {
                poVarianceAmount = Math.Round(comparableAmount - request.PurchaseOrderApprovedAmountSnapshot.Value, 2, MidpointRounding.AwayFromZero);
                poAmountStatus = poVarianceAmount == 0m ? "matched" : "variance_open";
            }
        }

        var transaction = new BankTransaction
        {
            TenantId = tenantId,
            BankAccountId = request.BankAccountId,
            TransactionDate = request.TransactionDate,
            Description = NormalizeRequired(request.Description, "Transaction description is required."),
            Amount = request.Amount,
            Direction = NormalizeOptional(request.Direction, request.Amount < 0 ? "credit" : "debit"),
            SourceType = NormalizeOptional(request.SourceType, "manual"),
            MatchStatus = NormalizeOptional(request.MatchStatus, "unmatched"),
            PurchaseOrderRefProductKey = request.PurchaseOrderRef?.ProductKey,
            PurchaseOrderRefType = request.PurchaseOrderRef?.ObjectType,
            PurchaseOrderRefId = request.PurchaseOrderRef?.ObjectId,
            PurchaseOrderRefDisplayName = request.PurchaseOrderRef?.ObjectNumber,
            PurchaseOrderApprovedAmountSnapshot = request.PurchaseOrderApprovedAmountSnapshot,
            PurchaseOrderVarianceAmount = poVarianceAmount,
            PurchaseOrderAmountStatus = poAmountStatus,
        };
        db.BankTransactions.Add(transaction);
        await AddAuditAsync(tenantId, principal, "bank_transaction.recorded", "bank_transaction", transaction.Id.ToString(), $"Recorded bank transaction for {account.AccountDisplayName}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return new BankTransactionSummaryResponse(
            transaction.Id,
            transaction.BankAccountId,
            account.AccountDisplayName,
            transaction.TransactionDate,
            transaction.Description,
            transaction.Amount,
            transaction.Direction,
            transaction.SourceType,
            transaction.MatchStatus,
            transaction.PurchaseOrderRefProductKey,
            transaction.PurchaseOrderRefType,
            transaction.PurchaseOrderRefId,
            transaction.PurchaseOrderRefDisplayName,
            transaction.PurchaseOrderApprovedAmountSnapshot,
            transaction.PurchaseOrderVarianceAmount,
            transaction.PurchaseOrderAmountStatus,
            transaction.ReconciliationStatus,
            transaction.ReconciliationId);
    }

    public async Task<BankTransaction?> MatchBankTransactionAsync(ClaimsPrincipal principal, Guid id, MatchBankTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var transaction = await db.BankTransactions.FirstOrDefaultAsync(bankTransaction => bankTransaction.TenantId == tenantId && bankTransaction.Id == id, cancellationToken);
        if (transaction is null)
        {
            return null;
        }

        transaction.MatchStatus = "matched";
        transaction.MatchedLedgArrTransactionType = NormalizeRequired(request.MatchedLedgArrTransactionType, "Matched transaction type is required.");
        transaction.MatchedLedgArrTransactionId = NormalizeRequired(request.MatchedLedgArrTransactionId, "Matched transaction id is required.");
        await AddAuditAsync(tenantId, principal, "bank_transaction.matched", "bank_transaction", transaction.Id.ToString(), "Matched bank transaction to LedgArr activity.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    public async Task<IReadOnlyList<BankReconciliationSummaryResponse>> ListBankReconciliationsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var reconciliations = await db.BankReconciliations
            .Where(reconciliation => reconciliation.TenantId == tenantId)
            .OrderByDescending(reconciliation => reconciliation.StatementDate)
            .ToListAsync(cancellationToken);
        var accounts = await db.BankAccounts
            .Where(account => account.TenantId == tenantId)
            .ToDictionaryAsync(account => account.Id, cancellationToken);

        return reconciliations
            .Select(reconciliation => new BankReconciliationSummaryResponse(
                reconciliation.Id,
                reconciliation.BankAccountId,
                accounts.TryGetValue(reconciliation.BankAccountId, out var account) ? account.AccountDisplayName : "Unknown account",
                reconciliation.PeriodStartDate,
                reconciliation.PeriodEndDate,
                reconciliation.BeginningBalance,
                reconciliation.EndingBalance,
                reconciliation.StatementDate,
                reconciliation.ClearedTransactionTotal,
                reconciliation.AdjustmentTotal,
                reconciliation.MatchedTransactionCount,
                reconciliation.ExceptionCount,
                reconciliation.ApprovalStatus,
                reconciliation.LockStatus,
                reconciliation.Status))
            .ToArray();
    }

    public async Task<BankReconciliation> CreateBankReconciliationAsync(ClaimsPrincipal principal, CreateBankReconciliationRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var account = await db.BankAccounts.FirstOrDefaultAsync(bankAccount => bankAccount.TenantId == tenantId && bankAccount.Id == request.BankAccountId, cancellationToken);
        if (account is null)
        {
            throw new StlApiException("ledgarr.bank_reconciliation.account_missing", "The selected bank account could not be found.", 404);
        }

        var transactions = await db.BankTransactions
            .Where(transaction => transaction.TenantId == tenantId && request.BankTransactionIds.Contains(transaction.Id))
            .ToListAsync(cancellationToken);
        var clearedTotal = transactions.Sum(transaction => transaction.Amount);
        var difference = request.BeginningBalance + clearedTotal + request.AdjustmentTotal - request.EndingBalance;
        var reconciliation = new BankReconciliation
        {
            TenantId = tenantId,
            BankAccountId = request.BankAccountId,
            PeriodStartDate = request.PeriodStartDate,
            PeriodEndDate = request.PeriodEndDate,
            BeginningBalance = request.BeginningBalance,
            EndingBalance = request.EndingBalance,
            StatementDate = request.StatementDate,
            ClearedTransactionTotal = clearedTotal,
            AdjustmentTotal = request.AdjustmentTotal,
            MatchedTransactionCount = transactions.Count,
            ExceptionCount = Math.Round(difference, 2, MidpointRounding.AwayFromZero) == 0 ? 0 : 1,
            Status = Math.Round(difference, 2, MidpointRounding.AwayFromZero) == 0 ? "balanced" : "exceptions_open",
        };
        db.BankReconciliations.Add(reconciliation);
        foreach (var transaction in transactions)
        {
            transaction.ReconciliationStatus = "reconciled";
            transaction.ReconciliationId = reconciliation.Id;
        }

        await AddAuditAsync(tenantId, principal, "bank_reconciliation.created", "bank_reconciliation", reconciliation.Id.ToString(), $"Created bank reconciliation for {account.AccountDisplayName}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return reconciliation;
    }

    public async Task<BankReconciliation?> ApproveBankReconciliationAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var reconciliation = await db.BankReconciliations.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Id == id, cancellationToken);
        if (reconciliation is null)
        {
            return null;
        }

        if (reconciliation.ExceptionCount > 0)
        {
            throw new StlApiException("ledgarr.bank_reconciliation.unbalanced", "Bank reconciliation cannot be approved while exceptions remain.", 409);
        }

        reconciliation.ApprovalStatus = "approved";
        reconciliation.Status = "approved";
        reconciliation.ApprovedAt = DateTimeOffset.UtcNow;
        await AddAuditAsync(tenantId, principal, "bank_reconciliation.approved", "bank_reconciliation", reconciliation.Id.ToString(), "Approved bank reconciliation.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return reconciliation;
    }

    public async Task<BankReconciliation?> LockBankReconciliationAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var reconciliation = await db.BankReconciliations.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Id == id, cancellationToken);
        if (reconciliation is null)
        {
            return null;
        }

        if (reconciliation.ApprovalStatus != "approved")
        {
            throw new StlApiException("ledgarr.bank_reconciliation.not_approved", "Bank reconciliation must be approved before it can be locked.", 409);
        }

        reconciliation.LockStatus = "locked";
        reconciliation.Status = "locked";
        reconciliation.LockedAt = DateTimeOffset.UtcNow;
        await AddAuditAsync(tenantId, principal, "bank_reconciliation.locked", "bank_reconciliation", reconciliation.Id.ToString(), "Locked bank reconciliation.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return reconciliation;
    }

    public async Task<InventoryValuationMovement> RevalueInventoryAsync(ClaimsPrincipal principal, InventoryRevalueRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var movement = new InventoryValuationMovement
        {
            TenantId = tenantId,
            FinancialLegalEntityId = request.FinancialLegalEntityId,
            ItemRefId = NormalizeRequired(request.ItemRef.ObjectId, "Item reference is required."),
            MovementType = NormalizeOptional(request.MovementType, "revalue"),
            CostMethod = NormalizeOptional(request.CostMethod, "fifo"),
            Quantity = request.Quantity,
            UnitCost = request.UnitCost,
            ExtendedAmount = request.Quantity * request.UnitCost,
        };
        db.InventoryValuationMovements.Add(movement);
        await AddAuditAsync(tenantId, principal, "inventory_valuation.updated", "inventory_valuation_movement", movement.Id.ToString(), $"Recorded inventory valuation movement for {movement.ItemRefId}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return movement;
    }

    public async Task<IReadOnlyList<InventoryCostLayer>> ListInventoryCostLayersAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        return await db.InventoryCostLayers.Where(l => l.TenantId == tenantId).OrderBy(l => l.LayerDate).ToListAsync(cancellationToken);
    }

    public async Task<FixedAssetFinancialRecord> CapitalizeFixedAssetAsync(ClaimsPrincipal principal, CapitalizeAssetRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var asset = new FixedAssetFinancialRecord
        {
            TenantId = tenantId,
            FinancialLegalEntityId = request.FinancialLegalEntityId,
            MaintainArrAssetRefId = NormalizeRequired(request.MaintainArrAssetRef.ObjectId, "MaintainArr asset reference is required."),
            AssetNumber = await NextNumberAsync(tenantId, "FA", "FA", cancellationToken),
            AssetClass = NormalizeOptional(request.AssetClass, "equipment"),
            InServiceDate = request.InServiceDate,
            CapitalizedCost = request.CapitalizedCost,
            BookValue = request.CapitalizedCost,
            DepreciationMethod = NormalizeOptional(request.DepreciationMethod, "straight_line"),
            UsefulLifeMonths = request.UsefulLifeMonths,
            SalvageValue = request.SalvageValue,
        };
        db.FixedAssetFinancialRecords.Add(asset);

        foreach (var schedule in GenerateStraightLineSchedule(asset, tenantId))
        {
            db.AssetDepreciationSchedules.Add(schedule);
        }

        await AddAuditAsync(tenantId, principal, "fixed_asset.capitalized", "fixed_asset_financial_record", asset.Id.ToString(), $"Capitalized fixed asset {asset.AssetNumber}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return asset;
    }

    public async Task<IReadOnlyList<FixedAssetSummaryResponse>> ListFixedAssetsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var assets = await db.FixedAssetFinancialRecords
            .Where(asset => asset.TenantId == tenantId)
            .OrderBy(asset => asset.AssetNumber)
            .ToListAsync(cancellationToken);
        var schedules = await db.AssetDepreciationSchedules
            .Where(schedule => schedule.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return assets
            .Select(asset =>
            {
                var nextSchedule = schedules
                    .Where(schedule => schedule.FixedAssetFinancialRecordId == asset.Id && schedule.Status != "posted")
                    .OrderBy(schedule => schedule.SequenceNumber)
                    .FirstOrDefault();
                var remaining = schedules.Count(schedule => schedule.FixedAssetFinancialRecordId == asset.Id && schedule.Status != "posted");
                return new FixedAssetSummaryResponse(
                    asset.Id,
                    asset.FinancialLegalEntityId,
                    asset.MaintainArrAssetRefId,
                    asset.AssetNumber,
                    asset.AssetClass,
                    asset.InServiceDate,
                    asset.CapitalizedCost,
                    asset.BookValue,
                    asset.DepreciationMethod,
                    asset.UsefulLifeMonths,
                    asset.SalvageValue,
                    asset.Status,
                    nextSchedule?.DepreciationDate,
                    remaining);
            })
            .ToArray();
    }

    public async Task<IReadOnlyList<AssetDepreciationSchedule>> ListFixedAssetDepreciationSchedulesAsync(
        ClaimsPrincipal principal,
        Guid fixedAssetId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        return await db.AssetDepreciationSchedules
            .Where(schedule => schedule.TenantId == tenantId && schedule.FixedAssetFinancialRecordId == fixedAssetId)
            .OrderBy(schedule => schedule.SequenceNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<Budget> CreateBudgetAsync(ClaimsPrincipal principal, CreateBudgetRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var budget = new Budget
        {
            TenantId = tenantId,
            FinancialLegalEntityId = request.FinancialLegalEntityId,
            BudgetNumber = await NextNumberAsync(tenantId, "BUD", "BUD", cancellationToken),
            Name = NormalizeRequired(request.Name, "Budget name is required."),
            Status = "approved",
        };
        db.Budgets.Add(budget);
        foreach (var line in request.Lines)
        {
            db.BudgetLines.Add(new BudgetLine
            {
                TenantId = tenantId,
                BudgetId = budget.Id,
                AccountCode = line.AccountCode,
                DimensionSummary = line.DimensionSummary,
                Amount = line.Amount,
                WarningThresholdPercent = line.WarningThresholdPercent,
                BlockThresholdPercent = line.BlockThresholdPercent,
            });
        }

        await AddAuditAsync(tenantId, principal, "budget.approved", "budget", budget.Id.ToString(), $"Created and approved budget {budget.BudgetNumber}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return budget;
    }

    public async Task<IReadOnlyList<BudgetSummaryResponse>> ListBudgetsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var budgets = await db.Budgets
            .Where(budget => budget.TenantId == tenantId)
            .OrderByDescending(budget => budget.BudgetNumber)
            .ToListAsync(cancellationToken);
        var lines = await db.BudgetLines
            .Where(line => line.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return budgets
            .Select(budget =>
            {
                var budgetLines = lines.Where(line => line.BudgetId == budget.Id).ToArray();
                return new BudgetSummaryResponse(
                    budget.Id,
                    budget.FinancialLegalEntityId,
                    budget.BudgetNumber,
                    budget.Name,
                    budget.Status,
                    budgetLines.Length,
                    budgetLines.Sum(line => line.Amount));
            })
            .ToArray();
    }

    public async Task<IReadOnlyList<FinancialProjectSummaryResponse>> ListFinancialProjectsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureBootstrapAsync(tenantId, cancellationToken);

        var projects = await db.FinancialProjects
            .Where(project => project.TenantId == tenantId)
            .OrderBy(project => project.ProjectCode)
            .ToListAsync(cancellationToken);

        var taskCounts = await db.FinancialProjectTasks
            .Where(task => task.TenantId == tenantId)
            .GroupBy(task => task.FinancialProjectId)
            .Select(group => new { FinancialProjectId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.FinancialProjectId, item => item.Count, cancellationToken);

        var budgetAmounts = await db.ProjectBudgets
            .Where(budget => budget.TenantId == tenantId)
            .GroupBy(budget => budget.FinancialProjectId)
            .Select(group => new { FinancialProjectId = group.Key, Amount = group.Sum(item => item.Amount) })
            .ToDictionaryAsync(item => item.FinancialProjectId, item => item.Amount, cancellationToken);

        var actualAmounts = await db.ProjectActualCosts
            .Where(cost => cost.TenantId == tenantId)
            .GroupBy(cost => cost.FinancialProjectId)
            .Select(group => new { FinancialProjectId = group.Key, Amount = group.Sum(item => item.Amount) })
            .ToDictionaryAsync(item => item.FinancialProjectId, item => item.Amount, cancellationToken);

        var committedAmounts = await db.ProjectCommittedCosts
            .Where(cost => cost.TenantId == tenantId)
            .GroupBy(cost => cost.FinancialProjectId)
            .Select(group => new { FinancialProjectId = group.Key, Amount = group.Sum(item => item.Amount) })
            .ToDictionaryAsync(item => item.FinancialProjectId, item => item.Amount, cancellationToken);

        var allocatedAmounts = await db.ProjectCostAllocations
            .Where(allocation => allocation.TenantId == tenantId)
            .GroupBy(allocation => allocation.FinancialProjectId)
            .Select(group => new { FinancialProjectId = group.Key, Amount = group.Sum(item => item.Amount) })
            .ToDictionaryAsync(item => item.FinancialProjectId, item => item.Amount, cancellationToken);

        var billingStatuses = await db.ProjectBillingStatuses
            .Where(status => status.TenantId == tenantId)
            .GroupBy(status => status.FinancialProjectId)
            .Select(group => group.OrderByDescending(item => item.Id).First())
            .ToDictionaryAsync(item => item.FinancialProjectId, item => item.Status, cancellationToken);

        var entityNames = await db.FinancialLegalEntities
            .Where(entity => entity.TenantId == tenantId)
            .ToDictionaryAsync(entity => entity.Id, entity => entity.DisplayName, cancellationToken);

        return projects
            .Select(project => new FinancialProjectSummaryResponse(
                project.Id,
                project.FinancialLegalEntityId,
                entityNames.GetValueOrDefault(project.FinancialLegalEntityId, "Unknown legal entity"),
                project.ProjectCode,
                project.Name,
                project.Status,
                budgetAmounts.GetValueOrDefault(project.Id, 0m),
                actualAmounts.GetValueOrDefault(project.Id, 0m),
                committedAmounts.GetValueOrDefault(project.Id, 0m),
                allocatedAmounts.GetValueOrDefault(project.Id, 0m),
                billingStatuses.GetValueOrDefault(project.Id, "not_billable"),
                taskCounts.GetValueOrDefault(project.Id, 0)))
            .ToArray();
    }

    public async Task<BudgetCheckResponse> CheckBudgetAsync(ClaimsPrincipal principal, BudgetCheckRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var line = await db.BudgetLines
            .Where(l => l.TenantId == tenantId && l.AccountCode == request.AccountCode)
            .OrderByDescending(l => l.Amount)
            .FirstOrDefaultAsync(cancellationToken);

        if (line is null)
        {
            return new BudgetCheckResponse("approval_required", "No approved budget line exists for this account.", 0, request.Amount, request.Amount);
        }

        var actual = await db.JournalLines
            .Where(l => l.TenantId == tenantId && l.AccountCode == line.AccountCode)
            .SumAsync(l => l.Debit - l.Credit, cancellationToken);
        var projected = actual + request.Amount;
        var ratio = line.Amount == 0 ? 1 : projected / line.Amount;
        var decision = ratio > line.BlockThresholdPercent
            ? "blocked"
            : ratio > line.WarningThresholdPercent ? "warning" : "allowed";
        var message = decision switch
        {
            "blocked" => "Budget threshold would be exceeded.",
            "warning" => "Budget warning threshold would be exceeded.",
            _ => "Budget check passed.",
        };

        if (decision != "allowed")
        {
            await AddAuditAsync(tenantId, principal, "budget.threshold_exceeded", "budget_line", line.Id.ToString(), message, cancellationToken: cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        return new BudgetCheckResponse(decision, message, line.Amount, actual, projected);
    }

    public async Task<ExternalFinanceSystem> CreateExternalFinanceSystemAsync(ClaimsPrincipal principal, CreateExternalFinanceSystemRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var system = new ExternalFinanceSystem
        {
            TenantId = tenantId,
            SystemKey = NormalizeRequired(request.SystemKey, "External system key is required.").ToLowerInvariant(),
            DisplayName = NormalizeRequired(request.DisplayName, "External system display name is required."),
            Mode = NormalizeOptional(request.Mode, "export_only"),
        };
        db.ExternalFinanceSystems.Add(system);
        await AddAuditAsync(tenantId, principal, "external_finance_system.created", "external_finance_system", system.Id.ToString(), $"Created external finance system {system.DisplayName}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return system;
    }

    public async Task<IReadOnlyList<ExternalFinanceSystem>> ListExternalFinanceSystemsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        return await db.ExternalFinanceSystems
            .Where(system => system.TenantId == tenantId)
            .OrderBy(system => system.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<ExternalPostingBatch> CreateExternalPostingBatchAsync(ClaimsPrincipal principal, CreateExternalPostingBatchRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var journals = await db.JournalEntries.Where(j => j.TenantId == tenantId && request.JournalEntryIds.Contains(j.Id)).ToListAsync(cancellationToken);
        if (journals.Count != request.JournalEntryIds.Count)
        {
            throw new StlApiException("ledgarr.external_export.journal_missing", "One or more journal entries could not be found.", 400);
        }

        if (journals.Any(j => j.Status != "posted"))
        {
            throw new StlApiException("ledgarr.external_export.unposted_journal", "External export batch cannot contain unposted or failed entries.", 409);
        }

        var batch = new ExternalPostingBatch
        {
            TenantId = tenantId,
            ExternalFinanceSystemId = request.ExternalFinanceSystemId,
            BatchNumber = await NextNumberAsync(tenantId, "EXT", "EXT", cancellationToken),
            JournalEntryIdsCsv = string.Join(',', request.JournalEntryIds),
        };
        db.ExternalPostingBatches.Add(batch);
        await AddAuditAsync(tenantId, principal, "external_export.created", "external_posting_batch", batch.Id.ToString(), $"Created external posting batch {batch.BatchNumber}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return batch;
    }

    public async Task<ExternalPostingBatch?> ExportExternalPostingBatchAsync(ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var batch = await db.ExternalPostingBatches.FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Id == id, cancellationToken);
        if (batch is null)
        {
            return null;
        }

        batch.Status = "sent";
        batch.ExportedAt = DateTimeOffset.UtcNow;
        db.ExternalPostingResults.Add(new ExternalPostingResult
        {
            TenantId = tenantId,
            ExternalPostingBatchId = batch.Id,
            Status = "sent",
            Message = "Export batch sent through LedgArr integration bridge.",
        });
        await AddAuditAsync(tenantId, principal, "external_export.sent", "external_posting_batch", batch.Id.ToString(), $"Sent external posting batch {batch.BatchNumber}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return batch;
    }

    public async Task<IReadOnlyList<ExternalPostingBatchSummaryResponse>> ListExternalPostingBatchesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var batches = await db.ExternalPostingBatches
            .Where(batch => batch.TenantId == tenantId)
            .OrderByDescending(batch => batch.CreatedAt)
            .ToListAsync(cancellationToken);
        var systems = await db.ExternalFinanceSystems
            .Where(system => system.TenantId == tenantId)
            .ToDictionaryAsync(system => system.Id, cancellationToken);

        return batches
            .Select(batch => new ExternalPostingBatchSummaryResponse(
                batch.Id,
                batch.ExternalFinanceSystemId,
                systems.TryGetValue(batch.ExternalFinanceSystemId, out var system) ? system.DisplayName : "Unknown system",
                batch.BatchNumber,
                batch.Status,
                batch.CreatedAt,
                batch.ExportedAt,
                string.IsNullOrWhiteSpace(batch.JournalEntryIdsCsv)
                    ? 0
                    : batch.JournalEntryIdsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length))
            .ToArray();
    }

    public async Task<IReadOnlyList<TaxCode>> ListTaxCodesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        return await db.TaxCodes
            .Where(code => code.TenantId == tenantId)
            .OrderBy(code => code.TaxCodeKey)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaxCode> CreateTaxCodeAsync(ClaimsPrincipal principal, CreateTaxCodeRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var taxCode = new TaxCode
        {
            TenantId = tenantId,
            TaxCodeKey = NormalizeRequired(request.TaxCodeKey, "Tax code key is required.").ToUpperInvariant(),
            DisplayName = NormalizeRequired(request.DisplayName, "Tax code display name is required."),
            Status = NormalizeOptional(request.Status, "active").ToLowerInvariant(),
        };
        db.TaxCodes.Add(taxCode);
        await AddAuditAsync(tenantId, principal, "tax_code.created", "tax_code", taxCode.Id.ToString(), $"Created tax code {taxCode.TaxCodeKey}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return taxCode;
    }

    public async Task<IReadOnlyList<TaxAdjustmentSummaryResponse>> ListTaxAdjustmentsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var taxCodes = await db.TaxCodes
            .Where(code => code.TenantId == tenantId)
            .ToDictionaryAsync(code => code.Id, cancellationToken);
        var entities = await db.FinancialLegalEntities
            .Where(entity => entity.TenantId == tenantId)
            .ToDictionaryAsync(entity => entity.Id, cancellationToken);
        var adjustments = await db.TaxAdjustments
            .Where(adjustment => adjustment.TenantId == tenantId)
            .OrderByDescending(adjustment => adjustment.AdjustmentDate)
            .ThenByDescending(adjustment => adjustment.CreatedAt)
            .ToListAsync(cancellationToken);

        return adjustments
            .Select(adjustment => new TaxAdjustmentSummaryResponse(
                adjustment.Id,
                adjustment.FinancialLegalEntityId,
                entities.TryGetValue(adjustment.FinancialLegalEntityId, out var entity) ? entity.DisplayName : "Unknown entity",
                adjustment.TaxCodeId,
                taxCodes.TryGetValue(adjustment.TaxCodeId, out var code) ? code.TaxCodeKey : "unknown",
                taxCodes.TryGetValue(adjustment.TaxCodeId, out code) ? code.DisplayName : "Unknown tax code",
                adjustment.AdjustmentNumber,
                adjustment.AdjustmentDate,
                adjustment.Amount,
                adjustment.CurrencyCode,
                adjustment.Reason,
                adjustment.Status))
            .ToArray();
    }

    public async Task<TaxAdjustment> CreateTaxAdjustmentAsync(ClaimsPrincipal principal, CreateTaxAdjustmentRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureFinancialLegalEntityExistsAsync(tenantId, request.FinancialLegalEntityId, cancellationToken);
        await ResolveOpenPeriodAsync(tenantId, request.FinancialLegalEntityId, request.AdjustmentDate, cancellationToken);

        var taxCode = await db.TaxCodes.FirstOrDefaultAsync(
            code => code.TenantId == tenantId && code.Id == request.TaxCodeId,
            cancellationToken);
        if (taxCode is null)
        {
            throw new StlApiException("ledgarr.tax_code_missing", "Tax code could not be resolved in LedgArr.", 400);
        }

        if (!IgnoreCase.Equals(taxCode.Status, "active"))
        {
            throw new StlApiException("ledgarr.tax_code_inactive", "Only active tax codes can be used for tax adjustments.", 409);
        }

        var adjustment = new TaxAdjustment
        {
            TenantId = tenantId,
            FinancialLegalEntityId = request.FinancialLegalEntityId,
            TaxCodeId = request.TaxCodeId,
            AdjustmentNumber = await GenerateTaxAdjustmentNumberAsync(tenantId, cancellationToken),
            AdjustmentDate = request.AdjustmentDate,
            Amount = request.Amount,
            CurrencyCode = NormalizeOptional(request.CurrencyCode, "USD").ToUpperInvariant(),
            Reason = NormalizeRequired(request.Reason, "Tax adjustment reason is required."),
            Status = NormalizeOptional(request.Status, "posted").ToLowerInvariant(),
            CreatedByPersonId = principal.GetPersonId(),
        };

        db.TaxAdjustments.Add(adjustment);
        await AddAuditAsync(
            tenantId,
            principal,
            "tax_adjustment.created",
            "tax_adjustment",
            adjustment.Id.ToString(),
            $"Created tax adjustment {adjustment.AdjustmentNumber} for {taxCode.TaxCodeKey}.",
            adjustment.Reason,
            cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return adjustment;
    }

    public async Task<IReadOnlyList<TaxLiabilitySummaryResponse>> ListTaxLiabilitySummariesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var adjustments = await db.TaxAdjustments
            .Where(adjustment => adjustment.TenantId == tenantId)
            .ToListAsync(cancellationToken);
        var taxCodes = await db.TaxCodes
            .Where(code => code.TenantId == tenantId)
            .ToDictionaryAsync(code => code.Id, cancellationToken);
        var entities = await db.FinancialLegalEntities
            .Where(entity => entity.TenantId == tenantId)
            .ToDictionaryAsync(entity => entity.Id, cancellationToken);

        return adjustments
            .GroupBy(adjustment => new { adjustment.FinancialLegalEntityId, adjustment.TaxCodeId, adjustment.CurrencyCode })
            .Select(group =>
            {
                entities.TryGetValue(group.Key.FinancialLegalEntityId, out var entity);
                taxCodes.TryGetValue(group.Key.TaxCodeId, out var code);
                return new TaxLiabilitySummaryResponse(
                    group.Key.FinancialLegalEntityId,
                    entity?.DisplayName ?? "Unknown entity",
                    group.Key.TaxCodeId,
                    code?.TaxCodeKey ?? "unknown",
                    code?.DisplayName ?? "Unknown tax code",
                    group.Key.CurrencyCode,
                    group.Sum(adjustment => adjustment.Amount),
                    group.Count(),
                    group.Max(adjustment => adjustment.AdjustmentDate));
            })
            .OrderBy(summary => summary.FinancialLegalEntityDisplayName)
            .ThenBy(summary => summary.TaxCodeKey)
            .ToArray();
    }

    public async Task<IReadOnlyList<FinancialLegalEntityRelationshipSummaryResponse>> ListFinancialLegalEntityRelationshipsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var entities = await db.FinancialLegalEntities
            .Where(entity => entity.TenantId == tenantId)
            .ToDictionaryAsync(entity => entity.Id, cancellationToken);
        var relationships = await db.FinancialLegalEntityRelationships
            .Where(relationship => relationship.TenantId == tenantId)
            .OrderBy(relationship => relationship.RelationshipType)
            .ToListAsync(cancellationToken);

        return relationships
            .Select(relationship => new FinancialLegalEntityRelationshipSummaryResponse(
                relationship.Id,
                relationship.ParentFinancialLegalEntityId,
                entities.TryGetValue(relationship.ParentFinancialLegalEntityId, out var parent) ? parent.DisplayName : "Unknown parent",
                relationship.ChildFinancialLegalEntityId,
                entities.TryGetValue(relationship.ChildFinancialLegalEntityId, out var child) ? child.DisplayName : "Unknown child",
                relationship.RelationshipType,
                relationship.OwnershipPercentage,
                relationship.Status))
            .ToArray();
    }

    public async Task<FinancialLegalEntityRelationship> CreateFinancialLegalEntityRelationshipAsync(
        ClaimsPrincipal principal,
        CreateFinancialLegalEntityRelationshipRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureFinancialLegalEntityExistsAsync(tenantId, request.ParentFinancialLegalEntityId, cancellationToken);
        await EnsureFinancialLegalEntityExistsAsync(tenantId, request.ChildFinancialLegalEntityId, cancellationToken);
        if (request.ParentFinancialLegalEntityId == request.ChildFinancialLegalEntityId)
        {
            throw new StlApiException("ledgarr.intercompany.self_reference_forbidden", "Parent and child financial legal entities must be different.", 400);
        }

        var relationship = new FinancialLegalEntityRelationship
        {
            TenantId = tenantId,
            ParentFinancialLegalEntityId = request.ParentFinancialLegalEntityId,
            ChildFinancialLegalEntityId = request.ChildFinancialLegalEntityId,
            RelationshipType = NormalizeOptional(request.RelationshipType, "intercompany"),
            OwnershipPercentage = request.OwnershipPercentage,
            Status = NormalizeOptional(request.Status, "active").ToLowerInvariant(),
        };
        db.FinancialLegalEntityRelationships.Add(relationship);
        await AddAuditAsync(tenantId, principal, "financial_legal_entity_relationship.created", "financial_legal_entity_relationship", relationship.Id.ToString(), "Created financial legal entity relationship.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return relationship;
    }

    public async Task<IReadOnlyList<IntercompanyTransactionSummaryResponse>> ListIntercompanyTransactionsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var entities = await db.FinancialLegalEntities
            .Where(entity => entity.TenantId == tenantId)
            .ToDictionaryAsync(entity => entity.Id, cancellationToken);
        var transactions = await db.IntercompanyTransactions
            .Where(transaction => transaction.TenantId == tenantId)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.CreatedAt)
            .ToListAsync(cancellationToken);

        return transactions
            .Select(transaction => new IntercompanyTransactionSummaryResponse(
                transaction.Id,
                transaction.RelationshipId,
                transaction.FromFinancialLegalEntityId,
                entities.TryGetValue(transaction.FromFinancialLegalEntityId, out var fromEntity) ? fromEntity.DisplayName : "Unknown source entity",
                transaction.ToFinancialLegalEntityId,
                entities.TryGetValue(transaction.ToFinancialLegalEntityId, out var toEntity) ? toEntity.DisplayName : "Unknown destination entity",
                transaction.TransactionNumber,
                transaction.TransactionDate,
                transaction.DueDate,
                transaction.Amount,
                transaction.CurrencyCode,
                transaction.Description,
                transaction.TransactionType,
                transaction.Status,
                transaction.SettlementStatus))
            .ToArray();
    }

    public async Task<IntercompanyTransaction> CreateIntercompanyTransactionAsync(
        ClaimsPrincipal principal,
        CreateIntercompanyTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        if (request.FromFinancialLegalEntityId == request.ToFinancialLegalEntityId)
        {
            throw new StlApiException("ledgarr.intercompany.self_reference_forbidden", "Intercompany transactions require two different financial legal entities.", 400);
        }

        await EnsureFinancialLegalEntityExistsAsync(tenantId, request.FromFinancialLegalEntityId, cancellationToken);
        await EnsureFinancialLegalEntityExistsAsync(tenantId, request.ToFinancialLegalEntityId, cancellationToken);
        await ResolveOpenPeriodAsync(tenantId, request.FromFinancialLegalEntityId, request.TransactionDate, cancellationToken);
        await ResolveOpenPeriodAsync(tenantId, request.ToFinancialLegalEntityId, request.TransactionDate, cancellationToken);

        var relationship = await db.FinancialLegalEntityRelationships.FirstOrDefaultAsync(
            item => item.TenantId == tenantId
                && item.Id == request.RelationshipId,
            cancellationToken);
        if (relationship is null)
        {
            throw new StlApiException("ledgarr.intercompany.relationship_missing", "Intercompany relationship could not be resolved in LedgArr.", 400);
        }

        var relationshipMatchesDirection =
            relationship.ParentFinancialLegalEntityId == request.FromFinancialLegalEntityId
            && relationship.ChildFinancialLegalEntityId == request.ToFinancialLegalEntityId;
        var relationshipMatchesReverse =
            relationship.ParentFinancialLegalEntityId == request.ToFinancialLegalEntityId
            && relationship.ChildFinancialLegalEntityId == request.FromFinancialLegalEntityId;
        if (!relationshipMatchesDirection && !relationshipMatchesReverse)
        {
            throw new StlApiException("ledgarr.intercompany.relationship_entity_mismatch", "Relationship entities do not match the requested intercompany transaction.", 409);
        }

        if (!IgnoreCase.Equals(relationship.Status, "active"))
        {
            throw new StlApiException("ledgarr.intercompany.relationship_inactive", "Only active intercompany relationships may be used for transactions.", 409);
        }

        var transaction = new IntercompanyTransaction
        {
            TenantId = tenantId,
            RelationshipId = relationship.Id,
            FromFinancialLegalEntityId = request.FromFinancialLegalEntityId,
            ToFinancialLegalEntityId = request.ToFinancialLegalEntityId,
            TransactionNumber = await GenerateIntercompanyTransactionNumberAsync(tenantId, cancellationToken),
            TransactionDate = request.TransactionDate,
            DueDate = request.DueDate,
            Amount = request.Amount,
            CurrencyCode = NormalizeOptional(request.CurrencyCode, "USD").ToUpperInvariant(),
            Description = NormalizeRequired(request.Description, "Intercompany transaction description is required."),
            TransactionType = NormalizeOptional(request.TransactionType, "due_to_due_from").ToLowerInvariant(),
            Status = NormalizeOptional(request.Status, "posted").ToLowerInvariant(),
            SettlementStatus = NormalizeOptional(request.SettlementStatus, "open").ToLowerInvariant(),
            CreatedByPersonId = principal.GetPersonId(),
        };

        db.IntercompanyTransactions.Add(transaction);
        await AddAuditAsync(
            tenantId,
            principal,
            "intercompany_transaction.created",
            "intercompany_transaction",
            transaction.Id.ToString(),
            $"Created intercompany transaction {transaction.TransactionNumber}.",
            transaction.Description,
            cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    public async Task<IntercompanyTransaction?> SettleIntercompanyTransactionAsync(
        ClaimsPrincipal principal,
        Guid id,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var transaction = await db.IntercompanyTransactions.FirstOrDefaultAsync(
            item => item.TenantId == tenantId && item.Id == id,
            cancellationToken);
        if (transaction is null)
        {
            return null;
        }

        var normalizedReason = NormalizeRequired(reason, "A reason is required before settling an intercompany transaction.");
        if (IgnoreCase.Equals(transaction.SettlementStatus, "settled"))
        {
            throw new StlApiException("ledgarr.intercompany.already_settled", "Intercompany transaction has already been settled.", 409);
        }

        transaction.SettlementStatus = "settled";
        transaction.Status = "settled";
        transaction.SettledAt = DateTimeOffset.UtcNow;
        await AddAuditAsync(
            tenantId,
            principal,
            "intercompany_transaction.settled",
            "intercompany_transaction",
            transaction.Id.ToString(),
            $"Settled intercompany transaction {transaction.TransactionNumber}.",
            normalizedReason,
            cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    public async Task<IReadOnlyList<IntercompanyBalanceSummaryResponse>> ListIntercompanyBalanceSummariesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var entities = await db.FinancialLegalEntities
            .Where(entity => entity.TenantId == tenantId)
            .ToDictionaryAsync(entity => entity.Id, cancellationToken);
        var openTransactions = await db.IntercompanyTransactions
            .Where(transaction => transaction.TenantId == tenantId && transaction.SettlementStatus != "settled")
            .ToListAsync(cancellationToken);

        return openTransactions
            .GroupBy(transaction => new
            {
                transaction.FromFinancialLegalEntityId,
                transaction.ToFinancialLegalEntityId,
                transaction.CurrencyCode,
            })
            .Select(group => new IntercompanyBalanceSummaryResponse(
                group.Key.FromFinancialLegalEntityId,
                entities.TryGetValue(group.Key.FromFinancialLegalEntityId, out var fromEntity) ? fromEntity.DisplayName : "Unknown source entity",
                group.Key.ToFinancialLegalEntityId,
                entities.TryGetValue(group.Key.ToFinancialLegalEntityId, out var toEntity) ? toEntity.DisplayName : "Unknown destination entity",
                group.Key.CurrencyCode,
                group.Sum(transaction => transaction.Amount),
                group.Count(),
                group.Min(transaction => transaction.TransactionDate),
                group.Max(transaction => transaction.TransactionDate)))
            .OrderBy(summary => summary.FromFinancialLegalEntityDisplayName)
            .ThenBy(summary => summary.ToFinancialLegalEntityDisplayName)
            .ToArray();
    }

    public async Task<TrialBalanceResponse> GetTrialBalanceAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var lines = await db.JournalLines.Where(l => l.TenantId == tenantId).ToListAsync(cancellationToken);
        var accounts = await db.GLAccounts.Where(a => a.TenantId == tenantId).ToDictionaryAsync(a => a.Id, cancellationToken);
        var rows = lines
            .GroupBy(line => line.GLAccountId)
            .Select(group =>
            {
                accounts.TryGetValue(group.Key, out var account);
                var debit = group.Sum(l => l.Debit);
                var credit = group.Sum(l => l.Credit);
                return new TrialBalanceRowResponse(account?.AccountCode ?? "unknown", account?.Name ?? "Unknown", debit, credit, debit - credit);
            })
            .OrderBy(row => row.AccountCode)
            .ToArray();

        return new TrialBalanceResponse(rows, rows.Sum(r => r.Debits), rows.Sum(r => r.Credits));
    }

    public async Task<ReportSummaryResponse> GetReportSummaryAsync(ClaimsPrincipal principal, string reportKey, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var trialBalance = await GetTrialBalanceAsync(principal, cancellationToken);
        var packetExceptions = await db.FinancialPackets.CountAsync(p => p.TenantId == tenantId && (p.Status == "validation_failed" || p.Status == "needs_mapping"), cancellationToken);
        return new ReportSummaryResponse(reportKey, DateTimeOffset.UtcNow, trialBalance.TotalDebits, trialBalance.TotalCredits, packetExceptions);
    }

    public static decimal CalculateWeightedAverageCost(IEnumerable<WeightedAverageInput> inputs)
    {
        var material = inputs.ToArray();
        var totalQuantity = material.Sum(i => i.Quantity);
        if (totalQuantity == 0)
        {
            return 0;
        }

        return Math.Round(material.Sum(i => i.Quantity * i.UnitCost) / totalQuantity, 2, MidpointRounding.AwayFromZero);
    }

    public static FifoConsumptionResult CalculateFifoConsumption(IEnumerable<FifoLayerInput> layers, decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        var remaining = quantity;
        var consumed = new List<FifoLayerConsumption>();
        foreach (var layer in layers.OrderBy(layer => layer.LayerDate))
        {
            if (remaining <= 0)
            {
                break;
            }

            var take = Math.Min(remaining, layer.QuantityRemaining);
            if (take <= 0)
            {
                continue;
            }

            consumed.Add(new FifoLayerConsumption(layer.LayerId, take, layer.UnitCost, take * layer.UnitCost));
            remaining -= take;
        }

        if (remaining > 0)
        {
            throw new StlApiException("ledgarr.inventory.insufficient_fifo_layers", "FIFO layer consumption cannot be completed because available quantity is insufficient.", 409);
        }

        return new FifoConsumptionResult(consumed, consumed.Sum(c => c.Amount));
    }

    private async Task<FiscalPeriod?> ChangePeriodStatusAsync(ClaimsPrincipal principal, Guid id, string status, string? reason, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        var period = await db.FiscalPeriods.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id, cancellationToken);
        if (period is null)
        {
            return null;
        }

        var normalizedReason = NormalizeRequired(reason, "A reason is required for period close, reopen, and lock actions.");

        if (status == "closed" && period.Status != "open")
        {
            throw new StlApiException("ledgarr.period.close_requires_open", "Only open fiscal periods can be closed.", 409);
        }

        if (status == "open" && period.Status is not ("closed" or "locked"))
        {
            throw new StlApiException("ledgarr.period.reopen_requires_closed_or_locked", "Only closed or locked fiscal periods can be reopened.", 409);
        }

        if (status == "locked" && period.Status != "closed")
        {
            throw new StlApiException("ledgarr.period.lock_requires_closed", "Only closed fiscal periods can be locked.", 409);
        }

        period.Status = status;
        if (status == "closed")
        {
            period.ClosedAt = DateTimeOffset.UtcNow;
        }
        else if (status == "locked")
        {
            period.LockedAt = DateTimeOffset.UtcNow;
        }

        db.PeriodLockAudits.Add(new PeriodLockAudit
        {
            TenantId = tenantId,
            FiscalPeriodId = period.Id,
            Action = status,
            ActorId = principal.GetPersonId().ToString(),
            Reason = normalizedReason,
        });
        await AddAuditAsync(tenantId, principal, $"period.{status}", "fiscal_period", period.Id.ToString(), $"Period {period.PeriodKey} moved to {status}.", normalizedReason, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return period;
    }

    private async Task<PostingPreview> CreatePostingPreviewCoreAsync(ClaimsPrincipal principal, Guid tenantId, FinancialPacket packet, CancellationToken cancellationToken)
    {
        if (packet.FinancialLegalEntityId is null)
        {
            throw new StlApiException("ledgarr.packet.financial_legal_entity_missing", "Financial packet must resolve to one Financial Legal Entity before preview.", 409);
        }

        await EnsureFinancialLegalEntityExistsAsync(tenantId, packet.FinancialLegalEntityId.Value, cancellationToken);
        var preview = new PostingPreview
        {
            TenantId = tenantId,
            FinancialPacketId = packet.Id,
            SourceDocumentType = "financial_packet",
            SourceDocumentId = packet.Id,
        };
        db.PostingPreviews.Add(preview);
        var postingLines = await BuildPostingLinesAsync(tenantId, packet, cancellationToken);
        var lineNumber = 1;
        foreach (var postingLine in postingLines)
        {
            db.PostingPreviewLines.Add(new PostingPreviewLine
            {
                TenantId = tenantId,
                PostingPreviewId = preview.Id,
                LineNumber = lineNumber++,
                GLAccountId = postingLine.Account.Id,
                AccountCode = postingLine.Account.AccountCode,
                Debit = postingLine.Debit,
                Credit = postingLine.Credit,
                Memo = postingLine.Memo,
                DimensionSummary = postingLine.DimensionSummary,
            });
        }

        preview.TotalDebits = postingLines.Sum(line => line.Debit);
        preview.TotalCredits = postingLines.Sum(line => line.Credit);
        ValidateBalanced(preview.TotalDebits, preview.TotalCredits);
        await AddAuditAsync(tenantId, principal, "posting_preview.created", "posting_preview", preview.Id.ToString(), "Created posting preview.", cancellationToken: cancellationToken);
        return preview;
    }

    private async Task<IReadOnlyList<PostingLineDraft>> BuildPostingLinesAsync(Guid tenantId, FinancialPacket packet, CancellationToken cancellationToken)
    {
        var amount = packet.SourceAmount != 0 ? packet.SourceAmount : packet.SourceTotalAmount - packet.SourceTaxAmount;
        var tax = packet.SourceTaxAmount;
        var total = packet.SourceTotalAmount != 0 ? packet.SourceTotalAmount : amount + tax;

        async Task<GLAccount> Account(string code, string name, string type, string normal) =>
            await GetOrCreateAccountAsync(tenantId, code, name, type, normal, cancellationToken);

        return packet.PacketType switch
        {
            "customer_invoice" or "customer_order_invoice_request" or "shipment_revenue" => await BuildCustomerInvoicePostingAsync(),
            "customer_payment" => await Pair("1000", "Cash", "asset", "debit", "1100", "Accounts Receivable", "asset", "debit", "Customer payment", total, debitFirst: true),
            "vendor_payment" => await Pair("2000", "Accounts Payable", "liability", "credit", "1000", "Cash", "asset", "debit", "Vendor payment", total, debitFirst: true),
            "inventory_receipt_valuation" or "receiving_accrual" => await Pair("1200", "Inventory", "asset", "debit", "2100", "Receiving Accrual", "liability", "credit", "Inventory receipt valuation", total, debitFirst: true),
            "inventory_adjustment_valuation" or "inventory_scrap_valuation" => await Pair("5200", "Inventory Variance Expense", "expense", "debit", "1200", "Inventory", "asset", "debit", "Inventory adjustment valuation", total, debitFirst: true),
            "asset_capitalization" => await Pair("1500", "Fixed Assets", "asset", "debit", "2300", "Asset Clearing", "liability", "credit", "Asset capitalization", total, debitFirst: true),
            "asset_depreciation" => await Pair("5300", "Depreciation Expense", "expense", "debit", "1590", "Accumulated Depreciation", "asset", "credit", "Asset depreciation", total, debitFirst: true),
            "work_order_cost" or "maintenance_vendor_service" => await Pair("5100", "Maintenance Expense", "expense", "debit", "2300", "Cost Clearing", "liability", "credit", "Maintenance cost", total, debitFirst: true),
            _ => await Pair("5000", "Operating Expense", "expense", "debit", "2000", "Accounts Payable", "liability", "credit", $"{packet.PacketType} posting", total, debitFirst: true),
        };

        async Task<IReadOnlyList<PostingLineDraft>> BuildCustomerInvoicePostingAsync()
        {
            var ar = await Account("1100", "Accounts Receivable", "asset", "debit");
            var revenue = await Account("4000", "Revenue", "revenue", "credit");
            var taxPayable = await Account("2200", "Tax Payable", "liability", "credit");
            var lines = new List<PostingLineDraft>
            {
                new(ar, total, 0, "Customer invoice receivable", null),
                new(revenue, 0, amount, "Customer invoice revenue", null),
            };
            if (tax != 0)
            {
                lines.Add(new PostingLineDraft(taxPayable, 0, tax, "Tax liability", null));
            }

            return lines;
        }

        async Task<IReadOnlyList<PostingLineDraft>> Pair(
            string debitCode,
            string debitName,
            string debitType,
            string debitNormal,
            string creditCode,
            string creditName,
            string creditType,
            string creditNormal,
            string memo,
            decimal pairAmount,
            bool debitFirst)
        {
            var debitAccount = await Account(debitCode, debitName, debitType, debitNormal);
            var creditAccount = await Account(creditCode, creditName, creditType, creditNormal);
            var first = new PostingLineDraft(debitAccount, pairAmount, 0, memo, null);
            var second = new PostingLineDraft(creditAccount, 0, pairAmount, memo, null);
            return debitFirst ? [first, second] : [second, first];
        }
    }

    private async Task<JournalEntry> CreateJournalEntryFromLinesAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        Guid financialLegalEntityId,
        Guid fiscalPeriodId,
        DateOnly accountingDate,
        string description,
        IReadOnlyList<CreateJournalLineRequest> lines,
        Guid? sourcePacketId,
        string sourceDocumentType,
        Guid? sourceDocumentId,
        CancellationToken cancellationToken)
    {
        if (lines.Count < 2)
        {
            throw new StlApiException("ledgarr.journal.minimum_lines", "Every JournalEntry must have at least two JournalLines.", 400);
        }

        ValidateBalanced(lines.Sum(l => l.Debit), lines.Sum(l => l.Credit));
        var journal = new JournalEntry
        {
            TenantId = tenantId,
            FinancialLegalEntityId = financialLegalEntityId,
            FiscalPeriodId = fiscalPeriodId,
            JournalNumber = await NextNumberAsync(tenantId, "JE", "JE", cancellationToken),
            FinancialPacketId = sourcePacketId,
            SourceDocumentType = sourceDocumentType,
            SourceDocumentId = sourceDocumentId,
            AccountingDate = accountingDate,
            Description = description,
            TotalDebits = lines.Sum(l => l.Debit),
            TotalCredits = lines.Sum(l => l.Credit),
        };
        db.JournalEntries.Add(journal);

        var lineNumber = 1;
        foreach (var line in lines)
        {
            var account = await db.GLAccounts.FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == line.GLAccountId, cancellationToken)
                ?? throw new StlApiException("ledgarr.journal.account_missing", "Journal line references a GL account that does not exist in LedgArr.", 400);
            db.JournalLines.Add(new JournalLine
            {
                TenantId = tenantId,
                JournalEntryId = journal.Id,
                LineNumber = lineNumber++,
                GLAccountId = account.Id,
                AccountCode = account.AccountCode,
                Debit = line.Debit,
                Credit = line.Credit,
                Memo = line.Memo,
                DimensionSummary = line.DimensionSummary,
            });
        }

        await AddAuditAsync(tenantId, principal, "journal.created", "journal_entry", journal.Id.ToString(), $"Created journal {journal.JournalNumber}.", cancellationToken: cancellationToken);
        await AddJournalAuditTrailAsync(tenantId, journal.Id, principal, "journal.created", $"Created journal {journal.JournalNumber}.", cancellationToken);
        return journal;
    }

    private async Task PostJournalCoreAsync(ClaimsPrincipal principal, JournalEntry journal, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        if (journal.PostedAt is not null || journal.Status == "posted")
        {
            throw new StlApiException("ledgarr.journal.posted_immutable", "Posted journal entries are immutable; use reversal or adjustment entries.", 409);
        }

        var persistedLines = await db.JournalLines.Where(l => l.JournalEntryId == journal.Id).ToListAsync(cancellationToken);
        var pendingLines = db.ChangeTracker.Entries<JournalLine>()
            .Where(entry => entry.State == EntityState.Added && entry.Entity.JournalEntryId == journal.Id)
            .Select(entry => entry.Entity);
        var lines = persistedLines
            .Concat(pendingLines)
            .DistinctBy(line => line.Id)
            .ToList();
        if (lines.Count < 2)
        {
            throw new StlApiException("ledgarr.journal.minimum_lines", "Every JournalEntry must have at least two JournalLines.", 400);
        }

        ValidateBalanced(lines.Sum(l => l.Debit), lines.Sum(l => l.Credit));
        var period = await db.FiscalPeriods.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == journal.FiscalPeriodId, cancellationToken)
            ?? throw new StlApiException("ledgarr.period.missing", "Journal posting requires a fiscal period.", 400);
        if (period.Status == "closed")
        {
            throw new StlApiException("ledgarr.period.closed", "Closed periods reject normal postings.", 409);
        }

        if (period.Status == "locked")
        {
            throw new StlApiException("ledgarr.period.locked", "Locked periods reject all postings except authorized reopening workflow.", 409);
        }

        if (period.StartDate > journal.AccountingDate || period.EndDate < journal.AccountingDate)
        {
            throw new StlApiException("ledgarr.period.date_mismatch", "Journal accounting date must fall within the fiscal period.", 400);
        }

        journal.Status = "posted";
        journal.PostedAt = DateTimeOffset.UtcNow;
        await AddAuditAsync(tenantId, principal, "journal.posted", "journal_entry", journal.Id.ToString(), $"Posted journal {journal.JournalNumber}.", cancellationToken: cancellationToken);
        await AddJournalAuditTrailAsync(tenantId, journal.Id, principal, "journal.posted", $"Posted journal {journal.JournalNumber}.", cancellationToken);
    }

    private async Task<JournalEntry?> ChangeJournalStatusAsync(ClaimsPrincipal principal, Guid id, string status, string auditAction, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        var journal = await db.JournalEntries.FirstOrDefaultAsync(j => j.TenantId == tenantId && j.Id == id, cancellationToken);
        if (journal is null)
        {
            return null;
        }

        if (journal.PostedAt is not null)
        {
            throw new StlApiException("ledgarr.journal.posted_immutable", "Posted journal entries are immutable; use reversal or adjustment entries.", 409);
        }

        journal.Status = status;
        if (status == "approved")
        {
            db.JournalApprovals.Add(new JournalApproval
            {
                TenantId = tenantId,
                JournalEntryId = journal.Id,
                Decision = "approved",
                ActorPersonId = principal.GetPersonId().ToString(),
            });
        }

        await AddAuditAsync(tenantId, principal, auditAction, "journal_entry", journal.Id.ToString(), $"Journal {journal.JournalNumber} moved to {status}.", cancellationToken: cancellationToken);
        await AddJournalAuditTrailAsync(tenantId, journal.Id, principal, auditAction, $"Journal {journal.JournalNumber} moved to {status}.", cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return journal;
    }

    private async Task<CustomerInvoice?> ChangeInvoiceStatusAsync(ClaimsPrincipal principal, Guid id, string status, string auditAction, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        var invoice = await db.CustomerInvoices.FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == id, cancellationToken);
        if (invoice is null)
        {
            return null;
        }

        invoice.Status = status;
        if (status == "approved")
        {
            db.CustomerInvoiceApprovals.Add(new CustomerInvoiceApproval
            {
                TenantId = tenantId,
                CustomerInvoiceId = invoice.Id,
                Decision = "approved",
                ActorPersonId = principal.GetPersonId().ToString(),
            });
        }

        await AddAuditAsync(tenantId, principal, auditAction, "customer_invoice", invoice.Id.ToString(), $"Customer invoice {invoice.InvoiceNumber} moved to {status}.", cancellationToken: cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return invoice;
    }

    private async Task<FiscalPeriod> ResolveOpenPeriodAsync(Guid tenantId, Guid financialLegalEntityId, DateOnly accountingDate, CancellationToken cancellationToken)
    {
        var entity = await EnsureFinancialLegalEntityExistsAsync(tenantId, financialLegalEntityId, cancellationToken);
        var period = await db.FiscalPeriods
            .Where(p => p.TenantId == tenantId
                && p.FiscalCalendarId == entity.FiscalCalendarId
                && p.StartDate <= accountingDate
                && p.EndDate >= accountingDate)
            .OrderByDescending(p => p.FinancialLegalEntityId == financialLegalEntityId)
            .FirstOrDefaultAsync(cancellationToken);

        if (period is null)
        {
            throw new StlApiException("ledgarr.period.missing", "Posting requires a fiscal period for the accounting date.", 400);
        }

        if (period.Status == "closed")
        {
            throw new StlApiException("ledgarr.period.closed", "Closed periods reject normal postings.", 409);
        }

        if (period.Status == "locked")
        {
            throw new StlApiException("ledgarr.period.locked", "Locked periods reject all postings except authorized reopening workflow.", 409);
        }

        return period;
    }

    private async Task<FinancialLegalEntity> EnsureFinancialLegalEntityExistsAsync(Guid tenantId, Guid id, CancellationToken cancellationToken) =>
        await db.FinancialLegalEntities.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id && e.Status == "active", cancellationToken)
            ?? throw new StlApiException("ledgarr.financial_legal_entity_missing", "Financial Legal Entity could not be resolved in LedgArr.", 400);

    private async Task<Guid> ResolveDefaultFinancialLegalEntityIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        await EnsureBootstrapAsync(tenantId, cancellationToken);
        return await db.FinancialLegalEntities
            .Where(e => e.TenantId == tenantId && e.Status == "active")
            .OrderBy(e => e.EntityCode)
            .Select(e => e.Id)
            .FirstAsync(cancellationToken);
    }

    private async Task<string> GenerateTaxAdjustmentNumberAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var existingCount = await db.TaxAdjustments.CountAsync(adjustment => adjustment.TenantId == tenantId, cancellationToken);
        return $"TAX-ADJ-{existingCount + 1:0000}";
    }

    private async Task<string> GenerateIntercompanyTransactionNumberAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var existingCount = await db.IntercompanyTransactions.CountAsync(transaction => transaction.TenantId == tenantId, cancellationToken);
        return $"IC-{existingCount + 1:0000}";
    }

    private async Task<string> GenerateBillableEventNumberAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var existingCount = await db.BillableEvents.CountAsync(item => item.TenantId == tenantId, cancellationToken);
        return $"BILL-{existingCount + 1:0000}";
    }

    private static bool IsBillableSourceProduct(string sourceProductKey) =>
        sourceProductKey is "ordarr" or "customarr" or "routarr" or "loadarr" or "maintainarr" or "assurarr";

    private async Task<IReadOnlyList<PacketIssueDraft>> ValidatePacketAsync(Guid tenantId, FinancialPacket packet, FinancialPacketIngestRequest request, CancellationToken cancellationToken)
    {
        var issues = new List<PacketIssueDraft>();
        var settings = await _tenantSettingsService.GetEffectiveSettingsAsync(tenantId, cancellationToken);
        if (request.Lines.Count == 0)
        {
            issues.Add(new("lines_required", "FinancialPacket requires at least one packet line.", "blocked"));
        }

        if (request.SourceRefs.Any(sourceRef =>
            string.IsNullOrWhiteSpace(sourceRef.ProductKey)
            || string.IsNullOrWhiteSpace(sourceRef.SourceRecordType)
            || string.IsNullOrWhiteSpace(sourceRef.SourceRecordId)))
        {
            issues.Add(new("source_ref_required", "Every source reference must include productKey, sourceRecordType, and sourceRecordId.", "blocked"));
        }

        if (packet.FinancialLegalEntityId is not null)
        {
            var entity = await db.FinancialLegalEntities.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == packet.FinancialLegalEntityId, cancellationToken);
            if (entity is null)
            {
                issues.Add(new("financial_legal_entity_missing", "Financial Legal Entity does not exist in LedgArr.", "blocked"));
            }
            else if (LooksLikeGoverningBody(entity.EntityCode, entity.DisplayName, entity.EntityType, null))
            {
                issues.Add(new("governing_body_not_financial_entity", "Compliance Core governing bodies cannot be used as LedgArr Financial Legal Entities.", "blocked"));
            }
        }
        else if (settings.LegalEntities.RequireLegalEntityOnEveryPosting)
        {
            issues.Add(new("missingLegalEntity", "LedgArr tenant settings require a legal entity on every posting packet.", "blocked"));
        }

        if (request.Lines.Any(line => line.TotalAmount < 0 && !AllowsNegativePacketLine(packet.PacketType)))
        {
            issues.Add(new("line_direction_invalid", "Packet line amount direction is invalid for this packet type.", "blocked"));
        }

        if (!LedgArrTenantSettingsService.IsSourceProductPostingEnabled(settings, packet.SourceProductKey))
        {
            issues.Add(new("sourceProductPostingDisabled", $"Posting from source product '{packet.SourceProductKey}' is disabled in LedgArr tenant settings.", "blocked"));
        }

        if (settings.Integrations.LedgarrOperatingMode == "disabled")
        {
            issues.Add(new("ledgarrDisabled", "LedgArr tenant settings currently disable packet posting.", "blocked"));
        }

        if (settings.Integrations.LedgarrOperatingMode == "externalErpMirror" && packet.PacketType is not "customer_invoice" and not "vendor_bill")
        {
            issues.Add(new("externalErpMirrorModeHold", "This packet type must be held for review while LedgArr runs in external ERP mirror mode.", "blocked"));
        }

        if (settings.Dimensions.MissingDimensionsBlockPosting)
        {
            var requiredDimensions = settings.Dimensions.RequiredDimensionsByTransactionType.TryGetValue(packet.PacketType, out var configured)
                ? configured
                : [];
            var missingDimensions = requiredDimensions
                .Where(required => !request.Lines.Any(line => line.DimensionHints?.ContainsKey(required) == true)
                                   && !(request.DimensionHints?.ContainsKey(required) == true))
                .ToArray();
            if (missingDimensions.Length > 0)
            {
                issues.Add(new("missingRequiredDimension", $"Missing required dimensions for packet type '{packet.PacketType}': {string.Join(", ", missingDimensions)}.", "blocked"));
            }
        }

        if (settings.Evidence.RequireAttachmentForManualJournalAboveThreshold
            && packet.PacketType == "manual_adjustment"
            && packet.SourceTotalAmount >= settings.Evidence.ManualJournalAttachmentThreshold
            && (request.DocumentRefs is null || request.DocumentRefs.Count == 0))
        {
            issues.Add(new("evidenceRequired", "Manual adjustment packets at or above the configured threshold require a supporting RecordArr document reference.", "blocked"));
        }

        return issues;
    }

    private async Task<FinancialPacketDetailResponse> BuildPacketDetailAsync(FinancialPacket packet, CancellationToken cancellationToken)
    {
        var lines = await db.FinancialPacketLines.Where(l => l.FinancialPacketId == packet.Id).OrderBy(l => l.LineNumber).ToListAsync(cancellationToken);
        var refs = await db.FinancialPacketSourceRefs.Where(r => r.FinancialPacketId == packet.Id).ToListAsync(cancellationToken);
        var issues = await db.FinancialPacketValidationIssues.Where(i => i.FinancialPacketId == packet.Id).ToListAsync(cancellationToken);
        return new FinancialPacketDetailResponse(packet, lines, refs, issues);
    }

    private async Task<PostingPreviewResponse> BuildPostingPreviewResponseAsync(PostingPreview preview, CancellationToken cancellationToken)
    {
        var lines = await db.PostingPreviewLines.Where(l => l.PostingPreviewId == preview.Id).OrderBy(l => l.LineNumber).ToListAsync(cancellationToken);
        return new PostingPreviewResponse(preview, lines);
    }

    private async Task<JournalEntryResponse> BuildJournalResponseAsync(JournalEntry journal, CancellationToken cancellationToken)
    {
        var lines = await db.JournalLines.Where(l => l.JournalEntryId == journal.Id).OrderBy(l => l.LineNumber).ToListAsync(cancellationToken);
        return new JournalEntryResponse(journal, lines);
    }

    private async Task AddPacketStatusAsync(Guid tenantId, Guid packetId, string status, string summary, CancellationToken cancellationToken)
    {
        db.FinancialPacketStatusHistory.Add(new FinancialPacketStatusHistory
        {
            TenantId = tenantId,
            FinancialPacketId = packetId,
            Status = status,
            Summary = summary,
        });
        await Task.CompletedTask;
    }

    private async Task AddAuditAsync(
        Guid tenantId,
        ClaimsPrincipal principal,
        string action,
        string targetType,
        string targetId,
        string summary,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        db.FinancialAuditEvents.Add(new FinancialAuditEvent
        {
            TenantId = tenantId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            ActorId = principal.Identity?.IsAuthenticated == true ? principal.GetPersonId().ToString() : "system",
            Summary = summary,
            Reason = reason,
        });
        await Task.CompletedTask;
    }

    private async Task AddJournalAuditTrailAsync(
        Guid tenantId,
        Guid journalEntryId,
        ClaimsPrincipal principal,
        string action,
        string summary,
        CancellationToken cancellationToken = default)
    {
        db.JournalAuditTrails.Add(new JournalAuditTrail
        {
            TenantId = tenantId,
            JournalEntryId = journalEntryId,
            Action = action,
            ActorId = principal.Identity?.IsAuthenticated == true ? principal.GetPersonId().ToString() : "system",
            Summary = summary,
        });
        await Task.CompletedTask;
    }

    private async Task EnsureBootstrapAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        if (!await db.TenantFinancialProfiles.AnyAsync(p => p.TenantId == tenantId, cancellationToken))
        {
            db.TenantFinancialProfiles.Add(new TenantFinancialProfile { TenantId = tenantId });
        }

        if (!await db.Currencies.AnyAsync(c => c.CurrencyCode == "USD", cancellationToken))
        {
            db.Currencies.Add(new Currency { CurrencyCode = "USD", DisplayName = "US Dollar", MinorUnits = 2 });
        }

        var calendar = await db.FiscalCalendars.FirstOrDefaultAsync(c => c.TenantId == tenantId, cancellationToken);
        if (calendar is null)
        {
            calendar = new FiscalCalendar { TenantId = tenantId, Name = "Standard calendar", FiscalYearStartMonth = 1 };
            db.FiscalCalendars.Add(calendar);
        }

        var entity = await db.FinancialLegalEntities.FirstOrDefaultAsync(e => e.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            entity = new FinancialLegalEntity
            {
                TenantId = tenantId,
                EntityCode = "STL",
                DisplayName = "STL Operating Company",
                EntityType = "company",
                FiscalCalendarId = calendar.Id,
                BaseCurrencyCode = "USD",
                SnapshotLabel = "LedgArr bootstrap accounting entity",
            };
            db.FinancialLegalEntities.Add(entity);
        }
        else if (entity.FiscalCalendarId is null)
        {
            entity.FiscalCalendarId = calendar.Id;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentPeriodKey = $"{today.Year:D4}-{today.Month:D2}";
        if (!await db.FiscalPeriods.AnyAsync(p => p.TenantId == tenantId && p.FiscalCalendarId == calendar.Id && p.PeriodKey == currentPeriodKey, cancellationToken))
        {
            var start = new DateOnly(today.Year, today.Month, 1);
            db.FiscalPeriods.Add(new FiscalPeriod
            {
                TenantId = tenantId,
                FiscalCalendarId = calendar.Id,
                FinancialLegalEntityId = entity.Id,
                PeriodKey = currentPeriodKey,
                Name = $"{today:yyyy MMMM}",
                StartDate = start,
                EndDate = start.AddMonths(1).AddDays(-1),
            });
        }

        await EnsureDefaultChartAsync(tenantId, cancellationToken);
        await EnsureDefaultControlFrameworkAsync(tenantId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid> EnsureDefaultChartAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var chart = await db.ChartsOfAccounts.FirstOrDefaultAsync(c => c.TenantId == tenantId, cancellationToken);
        if (chart is null)
        {
            chart = new ChartOfAccounts { TenantId = tenantId, Name = "Standard chart of accounts" };
            db.ChartsOfAccounts.Add(chart);
        }

        foreach (var seed in DefaultAccounts)
        {
            if (!await db.GLAccounts.AnyAsync(a => a.TenantId == tenantId && a.AccountCode == seed.Code, cancellationToken))
            {
                db.GLAccounts.Add(new GLAccount
                {
                    TenantId = tenantId,
                    ChartOfAccountsId = chart.Id,
                    AccountCode = seed.Code,
                    Name = seed.Name,
                    AccountType = seed.Type,
                    NormalBalance = seed.NormalBalance,
                    Category = seed.Category,
                });
            }
        }

        return chart.Id;
    }

    private async Task EnsureDefaultControlFrameworkAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        if (!await db.ApprovalPolicies.AnyAsync(policy => policy.TenantId == tenantId, cancellationToken))
        {
            var journalApproval = new ApprovalPolicy
            {
                TenantId = tenantId,
                PolicyKey = "journalApproval",
                AppliesTo = "journal_entry",
                RequiresApproval = true,
            };
            var vendorPaymentApproval = new ApprovalPolicy
            {
                TenantId = tenantId,
                PolicyKey = "vendorPaymentApproval",
                AppliesTo = "payment_run",
                RequiresApproval = true,
            };
            var bankAccountChangeApproval = new ApprovalPolicy
            {
                TenantId = tenantId,
                PolicyKey = "bankAccountChangeApproval",
                AppliesTo = "bank_account",
                RequiresApproval = true,
            };
            var customerCreditOverrideApproval = new ApprovalPolicy
            {
                TenantId = tenantId,
                PolicyKey = "customerCreditOverrideApproval",
                AppliesTo = "customer_credit_override",
                RequiresApproval = true,
            };
            var periodLockControl = new ApprovalPolicy
            {
                TenantId = tenantId,
                PolicyKey = "periodLockControl",
                AppliesTo = "fiscal_period_lock",
                RequiresApproval = true,
            };

            db.ApprovalPolicies.AddRange(
                journalApproval,
                vendorPaymentApproval,
                bankAccountChangeApproval,
                customerCreditOverrideApproval,
                periodLockControl);
            db.ApprovalSteps.AddRange(
                new ApprovalStep
                {
                    TenantId = tenantId,
                    ApprovalPolicyId = journalApproval.Id,
                    StepNumber = 1,
                    RequiredPermissionKey = "ledgarr.journals.submit",
                },
                new ApprovalStep
                {
                    TenantId = tenantId,
                    ApprovalPolicyId = journalApproval.Id,
                    StepNumber = 2,
                    RequiredPermissionKey = "ledgarr.journals.approve",
                },
                new ApprovalStep
                {
                    TenantId = tenantId,
                    ApprovalPolicyId = vendorPaymentApproval.Id,
                    StepNumber = 1,
                    RequiredPermissionKey = "ledgarr.ap.paymentRuns.review",
                },
                new ApprovalStep
                {
                    TenantId = tenantId,
                    ApprovalPolicyId = vendorPaymentApproval.Id,
                    StepNumber = 2,
                    RequiredPermissionKey = "ledgarr.ap.paymentRuns.approve",
                },
                new ApprovalStep
                {
                    TenantId = tenantId,
                    ApprovalPolicyId = bankAccountChangeApproval.Id,
                    StepNumber = 1,
                    RequiredPermissionKey = "ledgarr.banking.accounts.manage",
                },
                new ApprovalStep
                {
                    TenantId = tenantId,
                    ApprovalPolicyId = bankAccountChangeApproval.Id,
                    StepNumber = 2,
                    RequiredPermissionKey = "ledgarr.periodClose.manage",
                },
                new ApprovalStep
                {
                    TenantId = tenantId,
                    ApprovalPolicyId = customerCreditOverrideApproval.Id,
                    StepNumber = 1,
                    RequiredPermissionKey = "ledgarr.accountsReceivable.credit.review",
                },
                new ApprovalStep
                {
                    TenantId = tenantId,
                    ApprovalPolicyId = customerCreditOverrideApproval.Id,
                    StepNumber = 2,
                    RequiredPermissionKey = "ledgarr.accountsReceivable.credit.approve",
                },
                new ApprovalStep
                {
                    TenantId = tenantId,
                    ApprovalPolicyId = periodLockControl.Id,
                    StepNumber = 1,
                    RequiredPermissionKey = "ledgarr.periodClose.manage",
                });
        }

        if (!await db.SegregationOfDutiesRules.AnyAsync(rule => rule.TenantId == tenantId, cancellationToken))
        {
            db.SegregationOfDutiesRules.AddRange(
                new SegregationOfDutiesRule
                {
                    TenantId = tenantId,
                    RuleKey = "journalCreatorVsApprover",
                    IncompatibleActions = "journal_create,journal_approve",
                },
                new SegregationOfDutiesRule
                {
                    TenantId = tenantId,
                    RuleKey = "vendorMasterVsPaymentRelease",
                    IncompatibleActions = "vendor_change,payment_export",
                },
                new SegregationOfDutiesRule
                {
                    TenantId = tenantId,
                    RuleKey = "bankAccountMaintenanceVsReconciliationLock",
                    IncompatibleActions = "bank_account_change,bank_reconciliation_lock",
                },
                new SegregationOfDutiesRule
                {
                    TenantId = tenantId,
                    RuleKey = "creditOverrideVsCashApplication",
                    IncompatibleActions = "credit_override,customer_cash_application",
                });
        }
    }

    private async Task<GLAccount> GetOrCreateAccountAsync(Guid tenantId, string code, string name, string type, string normalBalance, CancellationToken cancellationToken)
    {
        var existing = await db.GLAccounts.FirstOrDefaultAsync(a => a.TenantId == tenantId && a.AccountCode == code, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var chartId = await EnsureDefaultChartAsync(tenantId, cancellationToken);
        var account = new GLAccount
        {
            TenantId = tenantId,
            ChartOfAccountsId = chartId,
            AccountCode = code,
            Name = name,
            AccountType = type,
            NormalBalance = normalBalance,
            Category = "system",
        };
        db.GLAccounts.Add(account);
        return account;
    }

    private async Task<string> NextNumberAsync(Guid tenantId, string sequenceKey, string prefix, CancellationToken cancellationToken)
    {
        var sequence = await db.NumberingSequences.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.SequenceKey == sequenceKey, cancellationToken);
        if (sequence is null)
        {
            sequence = new NumberingSequence { TenantId = tenantId, SequenceKey = sequenceKey, Prefix = prefix, NextNumber = 1 };
            db.NumberingSequences.Add(sequence);
        }

        var value = $"{sequence.Prefix}-{DateTimeOffset.UtcNow:yyyy}-{sequence.NextNumber:D5}";
        sequence.NextNumber += 1;
        return value;
    }

    private static IReadOnlyList<AgingBucketResponse> BuildAging(IEnumerable<(DateOnly DueDate, decimal Amount)> items, DateOnly today)
    {
        var buckets = new Dictionary<string, decimal>(IgnoreCase)
        {
            ["current"] = 0,
            ["1_30"] = 0,
            ["31_60"] = 0,
            ["61_90"] = 0,
            ["90_plus"] = 0,
        };

        foreach (var (dueDate, amount) in items)
        {
            var days = today.DayNumber - dueDate.DayNumber;
            var bucket = days <= 0
                ? "current"
                : days <= 30 ? "1_30"
                : days <= 60 ? "31_60"
                : days <= 90 ? "61_90" : "90_plus";
            buckets[bucket] += amount;
        }

        return buckets.Select(kvp => new AgingBucketResponse(kvp.Key, kvp.Value)).ToArray();
    }

    private static IReadOnlyList<AssetDepreciationSchedule> GenerateStraightLineSchedule(FixedAssetFinancialRecord asset, Guid tenantId)
    {
        if (asset.UsefulLifeMonths <= 0)
        {
            throw new StlApiException("ledgarr.asset.useful_life_required", "Useful life months must be greater than zero.", 400);
        }

        var depreciable = asset.CapitalizedCost - asset.SalvageValue;
        var monthly = Math.Round(depreciable / asset.UsefulLifeMonths, 2, MidpointRounding.AwayFromZero);
        var accumulated = 0m;
        var schedules = new List<AssetDepreciationSchedule>();
        for (var i = 1; i <= asset.UsefulLifeMonths; i++)
        {
            var amount = i == asset.UsefulLifeMonths ? depreciable - accumulated : monthly;
            accumulated += amount;
            schedules.Add(new AssetDepreciationSchedule
            {
                TenantId = tenantId,
                FixedAssetFinancialRecordId = asset.Id,
                SequenceNumber = i,
                DepreciationDate = asset.InServiceDate.AddMonths(i),
                DepreciationAmount = amount,
                AccumulatedDepreciation = accumulated,
            });
        }

        return schedules;
    }

    private static void ValidateBalanced(decimal debits, decimal credits)
    {
        if (debits <= 0 || credits <= 0 || Math.Round(debits - credits, 2) != 0)
        {
            throw new StlApiException("ledgarr.posting.unbalanced", "Financial postings must be balanced before posting.", 400);
        }
    }

    private static bool AllowsNegativePacketLine(string packetType) =>
        packetType is "vendor_credit" or "customer_credit" or "inventory_adjustment_valuation" or "manual_adjustment";

    private static string FlattenHints(IReadOnlyDictionary<string, string>? hints) =>
        hints is null || hints.Count == 0
            ? string.Empty
            : string.Join("; ", hints.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}"));

    private static string NormalizeRequired(string? value, string message)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new StlApiException("ledgarr.validation.required", message, 400);
        }

        return trimmed;
    }

    private static string NormalizeOptional(string? value, string fallback)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? fallback : trimmed;
    }

    private static void RejectGoverningBodyLikeEntity(string code, string displayName, string? entityType, StlProductObjectReference? sourceRef)
    {
        if (LooksLikeGoverningBody(code, displayName, entityType, sourceRef))
        {
            throw new StlApiException(
                "ledgarr.financial_legal_entity.governing_body_forbidden",
                "LedgArr FinancialLegalEntity records must not represent Compliance Core governing bodies, regulators, agencies, or statutory authorities.",
                400);
        }
    }

    public static bool LooksLikeGoverningBody(string code, string displayName, string? entityType, StlProductObjectReference? sourceRef)
    {
        if (sourceRef is not null && string.Equals(sourceRef.ProductKey, "compliancecore", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var haystack = $"{code} {displayName} {entityType}".ToUpperInvariant();
        var blockedTokens = new[]
        {
            "FMCSA",
            "OSHA",
            "MSHA",
            "EPA",
            "DOT",
            "FDA",
            "GOVERNING BODY",
            "REGULATOR",
            "REGULATORY AGENCY",
            "STATE AGENCY",
            "STANDARDS BODY",
            "STATUTORY AUTHORITY",
        };
        return blockedTokens.Any(token => haystack.Contains(token, StringComparison.Ordinal));
    }

    private Guid EnsureEntitled(ClaimsPrincipal principal)
    {
        var tenantId = principal.GetTenantId();

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "ledgarr_admin",
                "controller",
                "accountant",
                "ap_clerk",
                "ar_billing_clerk",
                "cash_treasury_user",
                "cost_accountant",
                "fixed_asset_accountant",
                "project_accountant",
                "finance_planner",
                "tax_administrator",
                "integration_administrator"))
        {
            return tenantId;
        }

        throw new StlApiException(
            "ledgarr.forbidden",
            "LedgArr access requires a finance or LedgArr role.",
            403);
    }

    private static bool MatchesRole(string roleKey, params string[] expectedRoleKeys) =>
        expectedRoleKeys.Any(expected => string.Equals(roleKey, expected, StringComparison.OrdinalIgnoreCase));

    private sealed record PostingLineDraft(GLAccount Account, decimal Debit, decimal Credit, string Memo, string? DimensionSummary);

    private sealed record PacketIssueDraft(string Code, string Message, string Severity);

    private sealed record DefaultAccountSeed(string Code, string Name, string Type, string NormalBalance, string Category);

    private static readonly IReadOnlyList<DefaultAccountSeed> DefaultAccounts =
    [
        new("1000", "Cash", "asset", "debit", "current"),
        new("1100", "Accounts Receivable", "asset", "debit", "current"),
        new("1200", "Inventory", "asset", "debit", "current"),
        new("1500", "Fixed Assets", "asset", "debit", "long_lived"),
        new("1590", "Accumulated Depreciation", "asset", "credit", "contra_asset"),
        new("2000", "Accounts Payable", "liability", "credit", "current"),
        new("2100", "Receiving Accrual", "liability", "credit", "current"),
        new("2200", "Tax Payable", "liability", "credit", "current"),
        new("2300", "Cost Clearing", "liability", "credit", "current"),
        new("4000", "Revenue", "revenue", "credit", "operating"),
        new("5000", "Operating Expense", "expense", "debit", "operating"),
        new("5100", "Maintenance Expense", "expense", "debit", "operating"),
        new("5200", "Inventory Variance Expense", "expense", "debit", "operating"),
        new("5300", "Depreciation Expense", "expense", "debit", "operating"),
    ];
}

public sealed record LedgArrSessionBootstrapResponse(
    string UserId,
    string PersonId,
    string TenantId,
    string SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    string ProductKey,
    bool HasLedgArrAccess,
    IReadOnlyList<string> LaunchableProductKeys);

public sealed record LedgArrHandoffSessionResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string UserId,
    string PersonId,
    string Email,
    string DisplayName,
    string TenantId,
    string TenantSlug,
    string TenantDisplayName,
    string SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    IReadOnlyList<string> LaunchableProductKeys,
    string ThemePreference,
    string? CallbackUrl);

public sealed record LedgArrDashboardResponse(
    DateTimeOffset GeneratedAt,
    int FinancialLegalEntityCount,
    int OpenPacketCount,
    int PostedJournalCount,
    int OpenPeriodCount,
    int ClosedPeriodCount,
    int OpenVendorBillCount,
    int OpenCustomerInvoiceCount,
    decimal PostedDebitVolume,
    decimal OpenApAmount,
    decimal OpenArAmount,
    IReadOnlyList<LedgArrActivityResponse> RecentActivity);

public sealed record LedgArrActivityResponse(
    string ActivityId,
    string Action,
    string TargetType,
    string TargetId,
    string Summary,
    DateTimeOffset OccurredAt);

public sealed record FinancialAuditEventResponse(
    Guid Id,
    string ProductKey,
    string Action,
    string TargetType,
    string TargetId,
    string ActorId,
    string Summary,
    string? Reason,
    DateTimeOffset OccurredAt);

public sealed record CreateFinancialLegalEntityRequest(
    string EntityCode,
    string DisplayName,
    string? EntityType,
    string? BaseCurrencyCode,
    string? StaffArrLocationRefId,
    string? SnapshotLabel,
    StlProductObjectReference? SourceRef = null);

public sealed record CreateFiscalCalendarRequest(string Name, int FiscalYearStartMonth, string? BaseCurrencyCode);

public sealed record CreateFiscalPeriodRequest(
    Guid FiscalCalendarId,
    Guid? FinancialLegalEntityId,
    string PeriodKey,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate);

public sealed record PeriodActionRequest(string? Reason);

public sealed record CreateChartOfAccountsRequest(string Name, Guid? FinancialLegalEntityId);

public sealed record CreateGLAccountRequest(
    Guid? ChartOfAccountsId,
    string AccountCode,
    string Name,
    string? AccountType,
    string? Category,
    string? NormalBalance);

public sealed record CreateDimensionTypeRequest(string DimensionKey, string DisplayName);

public sealed record CreateDimensionValueRequest(
    Guid DimensionTypeId,
    string ValueKey,
    string DisplayName,
    StlProductObjectReference? SourceRef);

public sealed record DimensionResolveRequest(IReadOnlyList<StlProductObjectReference>? SourceRefs);

public sealed record DimensionResolvedValueResponse(Guid DimensionValueId, string ValueKey, string DisplayName);

public sealed record DimensionResolveResponse(IReadOnlyList<DimensionResolvedValueResponse> Values, bool FullyResolved);

public sealed record FinancialPacketIngestRequest(
    Guid? FinancialLegalEntityId,
    string SourceProductKey,
    string SourceEventId,
    int SourceEventVersion,
    string SourceRecordType,
    string SourceRecordId,
    string? SourceRecordDisplayName,
    DateTimeOffset SourceOccurredAt,
    string PacketType,
    string? PacketSubType,
    DateOnly AccountingDate,
    string? TransactionCurrency,
    decimal SourceAmount,
    decimal SourceTaxAmount,
    decimal SourceTotalAmount,
    IReadOnlyList<FinancialPacketLineRequest> Lines,
    IReadOnlyList<FinancialPacketSourceRefRequest> SourceRefs,
    IReadOnlyDictionary<string, string>? DimensionHints,
    IReadOnlyList<StlProductObjectReference>? DocumentRefs,
    IReadOnlyDictionary<string, string>? ApprovalHints,
    string IdempotencyKey);

public sealed record FinancialPacketLineRequest(
    int LineNumber,
    string? SourceLineId,
    string? LineType,
    StlProductObjectReference? ItemRef,
    StlProductObjectReference? VendorRef,
    StlProductObjectReference? CustomerRef,
    StlProductObjectReference? AssetRef,
    StlProductObjectReference? WorkOrderRef,
    StlProductObjectReference? OrderRef,
    StlProductObjectReference? ShipmentRef,
    StlProductObjectReference? TripRef,
    StlProductObjectReference? LocationRef,
    decimal Quantity,
    string? UnitOfMeasure,
    decimal UnitCost,
    decimal ExtendedAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    IReadOnlyDictionary<string, string>? DimensionHints,
    string? CostBehavior,
    string? CapitalizationHint,
    string? BillableHint);

public sealed record FinancialPacketSourceRefRequest(
    string ProductKey,
    string SourceRecordType,
    string SourceRecordId,
    string? SourceRecordDisplayName,
    string? SourceEventId,
    int? SourceVersion,
    string? SnapshotLabel);

public sealed record FinancialPacketDetailResponse(
    FinancialPacket Packet,
    IReadOnlyList<FinancialPacketLine> Lines,
    IReadOnlyList<FinancialPacketSourceRef> SourceRefs,
    IReadOnlyList<FinancialPacketValidationIssue> ValidationIssues);

public sealed record BillableEventActionRequest(string? Reason);

public sealed record BillableEventSummaryResponse(
    Guid Id,
    Guid FinancialPacketId,
    Guid? FinancialLegalEntityId,
    string FinancialLegalEntityDisplayName,
    string EventNumber,
    string SourceProductKey,
    string SourceRecordDisplayName,
    string ChargeType,
    string? CustomerRefId,
    string CustomerDisplayName,
    decimal Amount,
    string CurrencyCode,
    string ApprovalStatus,
    string InvoiceStatus,
    string? HoldReason,
    string? ExceptionReason,
    DateOnly AccountingDate);

public sealed record PostingPreviewRequest(
    Guid FinancialLegalEntityId,
    DateOnly AccountingDate,
    string Description,
    decimal Amount,
    string? CurrencyCode);

public sealed record PostingPreviewResponse(PostingPreview Preview, IReadOnlyList<PostingPreviewLine> Lines);

public sealed record CreateJournalRequest(
    Guid FinancialLegalEntityId,
    DateOnly AccountingDate,
    string Description,
    IReadOnlyList<CreateJournalLineRequest> Lines);

public sealed record CreateJournalLineRequest(
    Guid GLAccountId,
    decimal Debit,
    decimal Credit,
    string Memo,
    string? DimensionSummary);

public sealed record ReverseJournalRequest(string Reason, DateOnly? AccountingDate);

public sealed record JournalEntryResponse(JournalEntry Journal, IReadOnlyList<JournalLine> Lines);

public sealed record CreateJournalAttachmentRefRequest(string RecordArrDocumentId, string DisplayName);

public sealed record JournalAttachmentRefResponse(
    Guid Id,
    Guid JournalEntryId,
    string JournalNumber,
    string RecordArrDocumentId,
    string DisplayName);

public sealed record JournalAuditTrailResponse(
    Guid Id,
    Guid JournalEntryId,
    string JournalNumber,
    string Action,
    string ActorId,
    string Summary,
    DateTimeOffset OccurredAt);

public sealed record ApprovalPolicySummaryResponse(
    Guid Id,
    string PolicyKey,
    string AppliesTo,
    bool RequiresApproval,
    IReadOnlyList<ApprovalStepSummaryResponse> Steps);

public sealed record ApprovalStepSummaryResponse(int StepNumber, string RequiredPermissionKey);

public sealed record SegregationOfDutiesRuleResponse(
    Guid Id,
    string RuleKey,
    IReadOnlyList<string> IncompatibleActions);

public sealed record CreateVendorBillRequest(
    Guid FinancialLegalEntityId,
    StlProductObjectReference VendorRef,
    string VendorDisplayName,
    string VendorInvoiceNumber,
    DateOnly BillDate,
    DateOnly DueDate,
    string? CurrencyCode,
    decimal Subtotal,
    decimal TaxAmount,
    decimal TotalAmount,
    IReadOnlyList<VendorBillLineRequest> Lines);

public sealed record VendorBillLineRequest(int LineNumber, string? Description, decimal Quantity, decimal UnitCost, decimal Amount);

public sealed record MatchVendorBillRequest(StlProductObjectReference SourceRef, decimal ExpectedAmount, decimal AllowedVariance);

public sealed record PaymentRunRequest(IReadOnlyList<Guid> VendorBillIds);

public sealed record PaymentRunSummaryResponse(
    Guid Id,
    string PaymentRunNumber,
    string Status,
    decimal TotalAmount,
    DateTimeOffset? LatestExportedAt,
    string? LatestExportStatus);

public sealed record CreateCustomerInvoiceRequest(
    Guid FinancialLegalEntityId,
    StlProductObjectReference CustomerRef,
    string CustomerDisplayName,
    string? InvoiceNumber,
    DateOnly InvoiceDate,
    DateOnly DueDate,
    string? CurrencyCode,
    decimal Subtotal,
    decimal TaxAmount,
    decimal TotalAmount,
    IReadOnlyList<CustomerInvoiceLineRequest> Lines);

public sealed record CustomerInvoiceLineRequest(int LineNumber, string? Description, decimal Quantity, decimal UnitPrice, decimal Amount);

public sealed record CreateCustomerPaymentRequest(
    Guid FinancialLegalEntityId,
    StlProductObjectReference CustomerRef,
    decimal Amount,
    IReadOnlyList<CustomerPaymentApplicationRequest> Applications);

public sealed record CustomerPaymentApplicationRequest(Guid CustomerInvoiceId, decimal AppliedAmount);

public sealed record CustomerPaymentSummaryResponse(
    Guid Id,
    Guid FinancialLegalEntityId,
    string CustomerRefId,
    string PaymentNumber,
    decimal Amount,
    string Status,
    decimal AppliedAmount);

public sealed record CreateBankAccountRequest(
    Guid FinancialLegalEntityId,
    string BankName,
    string AccountDisplayName,
    string? AccountType,
    string MaskedAccountNumber,
    string? CurrencyCode,
    Guid GLCashAccountId,
    bool ReconciliationEnabled);

public sealed record BankAccountSummaryResponse(
    Guid Id,
    Guid FinancialLegalEntityId,
    string FinancialLegalEntityDisplayName,
    string BankName,
    string AccountDisplayName,
    string AccountType,
    string MaskedAccountNumber,
    string CurrencyCode,
    Guid GLCashAccountId,
    string GLCashAccountCode,
    string Status,
    bool ReconciliationEnabled);

public sealed record CreateBankTransactionRequest(
    Guid BankAccountId,
    DateOnly TransactionDate,
    string Description,
    decimal Amount,
    string? Direction,
    string? SourceType,
    string? MatchStatus,
    StlProductObjectReference? PurchaseOrderRef,
    decimal? PurchaseOrderApprovedAmountSnapshot);

public sealed record MatchBankTransactionRequest(string MatchedLedgArrTransactionType, string MatchedLedgArrTransactionId);

public sealed record BankTransactionSummaryResponse(
    Guid Id,
    Guid BankAccountId,
    string BankAccountDisplayName,
    DateOnly TransactionDate,
    string Description,
    decimal Amount,
    string Direction,
    string SourceType,
    string MatchStatus,
    string? PurchaseOrderRefProductKey,
    string? PurchaseOrderRefType,
    string? PurchaseOrderRefId,
    string? PurchaseOrderRefDisplayName,
    decimal? PurchaseOrderApprovedAmountSnapshot,
    decimal? PurchaseOrderVarianceAmount,
    string PurchaseOrderAmountStatus,
    string ReconciliationStatus,
    Guid? ReconciliationId);

public sealed record CreateBankReconciliationRequest(
    Guid BankAccountId,
    DateOnly PeriodStartDate,
    DateOnly PeriodEndDate,
    decimal BeginningBalance,
    decimal EndingBalance,
    DateOnly StatementDate,
    decimal AdjustmentTotal,
    IReadOnlyList<Guid> BankTransactionIds);

public sealed record BankReconciliationSummaryResponse(
    Guid Id,
    Guid BankAccountId,
    string BankAccountDisplayName,
    DateOnly PeriodStartDate,
    DateOnly PeriodEndDate,
    decimal BeginningBalance,
    decimal EndingBalance,
    DateOnly StatementDate,
    decimal ClearedTransactionTotal,
    decimal AdjustmentTotal,
    int MatchedTransactionCount,
    int ExceptionCount,
    string ApprovalStatus,
    string LockStatus,
    string Status);

public sealed record AgingBucketResponse(string Bucket, decimal Amount);

public sealed record InventoryRevalueRequest(
    Guid FinancialLegalEntityId,
    StlProductObjectReference ItemRef,
    string? MovementType,
    string? CostMethod,
    decimal Quantity,
    decimal UnitCost);

public sealed record CapitalizeAssetRequest(
    Guid FinancialLegalEntityId,
    StlProductObjectReference MaintainArrAssetRef,
    string? AssetClass,
    DateOnly InServiceDate,
    decimal CapitalizedCost,
    string? DepreciationMethod,
    int UsefulLifeMonths,
    decimal SalvageValue);

public sealed record FixedAssetSummaryResponse(
    Guid Id,
    Guid FinancialLegalEntityId,
    string MaintainArrAssetRefId,
    string AssetNumber,
    string AssetClass,
    DateOnly InServiceDate,
    decimal CapitalizedCost,
    decimal BookValue,
    string DepreciationMethod,
    int UsefulLifeMonths,
    decimal SalvageValue,
    string Status,
    DateOnly? NextDepreciationDate,
    int RemainingScheduleCount);

public sealed record CreateBudgetRequest(
    Guid FinancialLegalEntityId,
    string Name,
    IReadOnlyList<CreateBudgetLineRequest> Lines);

public sealed record CreateBudgetLineRequest(
    string AccountCode,
    string? DimensionSummary,
    decimal Amount,
    decimal WarningThresholdPercent,
    decimal BlockThresholdPercent);

public sealed record BudgetSummaryResponse(
    Guid Id,
    Guid FinancialLegalEntityId,
    string BudgetNumber,
    string Name,
    string Status,
    int LineCount,
    decimal TotalBudgetAmount);

public sealed record FinancialProjectSummaryResponse(
    Guid Id,
    Guid FinancialLegalEntityId,
    string FinancialLegalEntityDisplayName,
    string ProjectCode,
    string Name,
    string Status,
    decimal BudgetAmount,
    decimal ActualCostAmount,
    decimal CommittedCostAmount,
    decimal AllocatedCostAmount,
    string BillingStatus,
    int TaskCount);

public sealed record BudgetCheckRequest(string AccountCode, decimal Amount, string? DimensionSummary);

public sealed record BudgetCheckResponse(string Decision, string Message, decimal BudgetAmount, decimal ActualAmount, decimal ProjectedAmount);

public sealed record CreateTaxCodeRequest(string TaxCodeKey, string DisplayName, string? Status);
public sealed record CreateTaxAdjustmentRequest(Guid FinancialLegalEntityId, Guid TaxCodeId, DateOnly AdjustmentDate, decimal Amount, string? CurrencyCode, string Reason, string? Status);
public sealed record TaxAdjustmentSummaryResponse(
    Guid Id,
    Guid FinancialLegalEntityId,
    string FinancialLegalEntityDisplayName,
    Guid TaxCodeId,
    string TaxCodeKey,
    string TaxCodeDisplayName,
    string AdjustmentNumber,
    DateOnly AdjustmentDate,
    decimal Amount,
    string CurrencyCode,
    string Reason,
    string Status);
public sealed record TaxLiabilitySummaryResponse(
    Guid FinancialLegalEntityId,
    string FinancialLegalEntityDisplayName,
    Guid TaxCodeId,
    string TaxCodeKey,
    string TaxCodeDisplayName,
    string CurrencyCode,
    decimal LiabilityAmount,
    int AdjustmentCount,
    DateOnly LatestAdjustmentDate);

public sealed record CreateExternalFinanceSystemRequest(string SystemKey, string DisplayName, string? Mode);

public sealed record CreateExternalPostingBatchRequest(Guid ExternalFinanceSystemId, IReadOnlyList<Guid> JournalEntryIds);

public sealed record ExternalPostingBatchSummaryResponse(
    Guid Id,
    Guid ExternalFinanceSystemId,
    string ExternalFinanceSystemDisplayName,
    string BatchNumber,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExportedAt,
    int JournalCount);

public sealed record CreateFinancialLegalEntityRelationshipRequest(
    Guid ParentFinancialLegalEntityId,
    Guid ChildFinancialLegalEntityId,
    string? RelationshipType,
    decimal OwnershipPercentage,
    string? Status);

public sealed record FinancialLegalEntityRelationshipSummaryResponse(
    Guid Id,
    Guid ParentFinancialLegalEntityId,
    string ParentDisplayName,
    Guid ChildFinancialLegalEntityId,
    string ChildDisplayName,
    string RelationshipType,
    decimal OwnershipPercentage,
    string Status);

public sealed record CreateIntercompanyTransactionRequest(
    Guid RelationshipId,
    Guid FromFinancialLegalEntityId,
    Guid ToFinancialLegalEntityId,
    DateOnly TransactionDate,
    DateOnly? DueDate,
    decimal Amount,
    string? CurrencyCode,
    string Description,
    string? TransactionType,
    string? Status,
    string? SettlementStatus);

public sealed record IntercompanySettlementRequest(string Reason);

public sealed record IntercompanyTransactionSummaryResponse(
    Guid Id,
    Guid RelationshipId,
    Guid FromFinancialLegalEntityId,
    string FromFinancialLegalEntityDisplayName,
    Guid ToFinancialLegalEntityId,
    string ToFinancialLegalEntityDisplayName,
    string TransactionNumber,
    DateOnly TransactionDate,
    DateOnly? DueDate,
    decimal Amount,
    string CurrencyCode,
    string Description,
    string TransactionType,
    string Status,
    string SettlementStatus);

public sealed record IntercompanyBalanceSummaryResponse(
    Guid FromFinancialLegalEntityId,
    string FromFinancialLegalEntityDisplayName,
    Guid ToFinancialLegalEntityId,
    string ToFinancialLegalEntityDisplayName,
    string CurrencyCode,
    decimal OpenAmount,
    int OpenTransactionCount,
    DateOnly OldestTransactionDate,
    DateOnly LatestTransactionDate);

public sealed record TrialBalanceResponse(IReadOnlyList<TrialBalanceRowResponse> Rows, decimal TotalDebits, decimal TotalCredits);

public sealed record TrialBalanceRowResponse(string AccountCode, string AccountName, decimal Debits, decimal Credits, decimal Balance);

public sealed record ReportSummaryResponse(string ReportKey, DateTimeOffset GeneratedAt, decimal TotalDebits, decimal TotalCredits, int MappingExceptionCount);

public sealed record WeightedAverageInput(decimal Quantity, decimal UnitCost);

public sealed record FifoLayerInput(Guid LayerId, DateOnly LayerDate, decimal QuantityRemaining, decimal UnitCost);

public sealed record FifoLayerConsumption(Guid LayerId, decimal Quantity, decimal UnitCost, decimal Amount);

public sealed record FifoConsumptionResult(IReadOnlyList<FifoLayerConsumption> Layers, decimal TotalAmount);
