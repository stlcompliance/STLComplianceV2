using System.Security.Claims;
using LedgArr.Api.Data;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace LedgArr.Api.Services;

public sealed class LedgArrStore(LedgArrDbContext db)
{
    private const string ProductKey = "ledgarr";
    private static readonly StringComparer IgnoreCase = StringComparer.OrdinalIgnoreCase;

    public LedgArrSessionBootstrapResponse BuildSession(
        string userId,
        string personId,
        string tenantId,
        string tenantRoleKey,
        bool isPlatformAdmin,
        IEnumerable<string> entitlements) =>
        new(userId, personId, tenantId, $"session-{userId}", tenantRoleKey, isPlatformAdmin, ProductKey, true, entitlements.ToArray());

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
        ChangePeriodStatusAsync(principal, id, "closed", reason ?? "Period close workflow completed.", cancellationToken);

    public Task<FiscalPeriod?> ReopenFiscalPeriodAsync(ClaimsPrincipal principal, Guid id, string? reason, CancellationToken cancellationToken = default) =>
        ChangePeriodStatusAsync(principal, id, "open", reason ?? "Authorized reopen workflow completed.", cancellationToken);

    public Task<FiscalPeriod?> LockFiscalPeriodAsync(ClaimsPrincipal principal, Guid id, string? reason, CancellationToken cancellationToken = default) =>
        ChangePeriodStatusAsync(principal, id, "locked", reason ?? "Period lock workflow completed.", cancellationToken);

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
        await db.SaveChangesAsync(cancellationToken);
        return await BuildJournalResponseAsync(reversal, cancellationToken);
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

    public async Task<IReadOnlyList<AgingBucketResponse>> GetARAgingAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var invoices = await db.CustomerInvoices.Where(i => i.TenantId == tenantId && (i.Status == "issued" || i.Status == "posted")).ToListAsync(cancellationToken);
        return BuildAging(invoices.Select(i => (i.DueDate, i.TotalAmount)), today);
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

    private async Task<FiscalPeriod?> ChangePeriodStatusAsync(ClaimsPrincipal principal, Guid id, string status, string reason, CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        var period = await db.FiscalPeriods.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id, cancellationToken);
        if (period is null)
        {
            return null;
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
            Reason = reason,
        });
        await AddAuditAsync(tenantId, principal, $"period.{status}", "fiscal_period", period.Id.ToString(), $"Period {period.PeriodKey} moved to {status}.", reason, cancellationToken);
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

    private async Task<IReadOnlyList<PacketIssueDraft>> ValidatePacketAsync(Guid tenantId, FinancialPacket packet, FinancialPacketIngestRequest request, CancellationToken cancellationToken)
    {
        var issues = new List<PacketIssueDraft>();
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

        if (request.Lines.Any(line => line.TotalAmount < 0 && !AllowsNegativePacketLine(packet.PacketType)))
        {
            issues.Add(new("line_direction_invalid", "Packet line amount direction is invalid for this packet type.", "blocked"));
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
        if (!principal.HasProductEntitlement(ProductKey))
        {
            throw new StlApiException("ledgarr.not_entitled", "Active LedgArr entitlement is required.", 403);
        }

        return principal.GetTenantId();
    }

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
    bool HasLedgArrEntitlement,
    IReadOnlyList<string> Entitlements);

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
    IReadOnlyList<string> Entitlements,
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

public sealed record BudgetCheckRequest(string AccountCode, decimal Amount, string? DimensionSummary);

public sealed record BudgetCheckResponse(string Decision, string Message, decimal BudgetAmount, decimal ActualAmount, decimal ProjectedAmount);

public sealed record CreateExternalFinanceSystemRequest(string SystemKey, string DisplayName, string? Mode);

public sealed record CreateExternalPostingBatchRequest(Guid ExternalFinanceSystemId, IReadOnlyList<Guid> JournalEntryIds);

public sealed record TrialBalanceResponse(IReadOnlyList<TrialBalanceRowResponse> Rows, decimal TotalDebits, decimal TotalCredits);

public sealed record TrialBalanceRowResponse(string AccountCode, string AccountName, decimal Debits, decimal Credits, decimal Balance);

public sealed record ReportSummaryResponse(string ReportKey, DateTimeOffset GeneratedAt, decimal TotalDebits, decimal TotalCredits, int MappingExceptionCount);

public sealed record WeightedAverageInput(decimal Quantity, decimal UnitCost);

public sealed record FifoLayerInput(Guid LayerId, DateOnly LayerDate, decimal QuantityRemaining, decimal UnitCost);

public sealed record FifoLayerConsumption(Guid LayerId, decimal Quantity, decimal UnitCost, decimal Amount);

public sealed record FifoConsumptionResult(IReadOnlyList<FifoLayerConsumption> Layers, decimal TotalAmount);
