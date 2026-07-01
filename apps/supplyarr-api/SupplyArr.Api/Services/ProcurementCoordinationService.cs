using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class ProcurementCoordinationService(SupplyArrDbContext db)
{
    public async Task<ProcurementCoordinationDashboardResponse> GetDashboardAsync(
        Guid tenantId,
        string? coordinationStage,
        bool? activeOnly,
        CancellationToken cancellationToken = default)
    {
        var records = await db.ProcurementCoordinationRecords
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        IEnumerable<ProcurementCoordinationRecord> filtered = records;
        if (activeOnly == true)
        {
            filtered = filtered.Where(x => !x.IsTerminal);
        }

        if (!string.IsNullOrWhiteSpace(coordinationStage))
        {
            var normalizedStage = coordinationStage.Trim().ToLowerInvariant();
            filtered = filtered.Where(x =>
                string.Equals(x.CoordinationStage, normalizedStage, StringComparison.OrdinalIgnoreCase));
        }

        var items = filtered
            .Select(x =>
            {
                var isMaterialized = !ProcurementCoordinationRules.IsStale(
                    x.ComputedAt,
                    DateTimeOffset.UtcNow,
                    ProcurementCoordinationRules.DefaultReadStalenessHours);
                return ProcurementCoordinationWorkerService.MapSummary(x, isMaterialized);
            })
            .ToList();

        var stageCounts = records
            .GroupBy(x => x.CoordinationStage)
            .Select(x => new ProcurementCoordinationStageSummaryResponse(x.Key, x.Count()))
            .OrderBy(x => x.CoordinationStage)
            .ToList();

        return new ProcurementCoordinationDashboardResponse(
            records.Count(x => !x.IsTerminal),
            records.Count(x => x.IsTerminal),
            stageCounts,
            items);
    }

    public async Task<ProcurementCoordinationDetailResponse> GetAsync(
        Guid tenantId,
        string subjectType,
        Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        var normalizedSubjectType = subjectType.Trim().ToLowerInvariant();
        var record = await db.ProcurementCoordinationRecords
            .AsNoTracking()
            .Include(x => x.Events)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.SubjectType == normalizedSubjectType
                    && x.SubjectId == subjectId,
                cancellationToken);

        if (record is not null)
        {
            var isMaterialized = !ProcurementCoordinationRules.IsStale(
                record.ComputedAt,
                DateTimeOffset.UtcNow,
                ProcurementCoordinationRules.DefaultReadStalenessHours);

            return new ProcurementCoordinationDetailResponse(
                ProcurementCoordinationWorkerService.MapSummary(record, isMaterialized),
                record.Events
                    .OrderBy(x => x.SequenceNumber)
                    .Select(ProcurementCoordinationWorkerService.MapEvent)
                    .ToList());
        }

        return await BuildLiveDetailAsync(tenantId, normalizedSubjectType, subjectId, cancellationToken);
    }

    private async Task<ProcurementCoordinationDetailResponse> BuildLiveDetailAsync(
        Guid tenantId,
        string subjectType,
        Guid subjectId,
        CancellationToken cancellationToken)
    {
        var asOfUtc = DateTimeOffset.UtcNow;

        if (string.Equals(subjectType, ProcurementCoordinationSubjectTypes.PurchaseRequest, StringComparison.OrdinalIgnoreCase))
        {
            var purchaseRequest = await db.PurchaseRequests
                .AsNoTracking()
                .Include(x => x.Lines)
                .Include(x => x.Supplier)
                    .ThenInclude(x => x!.ParentSupplier)
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == subjectId, cancellationToken)
                ?? throw new STLCompliance.Shared.Contracts.StlApiException(
                    "procurement_coordination.purchase_request.not_found",
                    "Purchase request was not found.",
                    404);

            var hasOpenPurchaseOrder = await db.PurchaseOrders.AsNoTracking().AnyAsync(
                x => x.TenantId == tenantId
                    && x.PurchaseRequestId == subjectId
                    && (PurchaseOrderStatuses.Open.Contains(x.Status)
                        || string.Equals(x.Status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase)),
                cancellationToken);

            var computation = ProcurementCoordinationBuilder.BuildFromPurchaseRequest(
                purchaseRequest,
                hasOpenPurchaseOrder,
                asOfUtc);

            return new ProcurementCoordinationDetailResponse(
                computation.Summary with { IsMaterialized = false },
                computation.Events);
        }

        if (string.Equals(subjectType, ProcurementCoordinationSubjectTypes.PurchaseOrder, StringComparison.OrdinalIgnoreCase))
        {
            var purchaseOrder = await db.PurchaseOrders
                .AsNoTracking()
                .Include(x => x.Lines)
                .Include(x => x.Supplier)
                    .ThenInclude(x => x!.ParentSupplier)
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == subjectId, cancellationToken)
                ?? throw new STLCompliance.Shared.Contracts.StlApiException(
                    "procurement_coordination.purchase_order.not_found",
                    "Purchase order was not found.",
                    404);

            var computation = ProcurementCoordinationBuilder.BuildFromPurchaseOrder(purchaseOrder, asOfUtc);
            return new ProcurementCoordinationDetailResponse(
                computation.Summary with { IsMaterialized = false },
                computation.Events);
        }

        throw new STLCompliance.Shared.Contracts.StlApiException(
            "procurement_coordination.subject_type.invalid",
            "Coordination subject type is invalid.",
            400);
    }
}

