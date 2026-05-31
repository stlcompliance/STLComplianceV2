using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class ProductGateEventService(ComplianceCoreDbContext db)
{
    private static readonly string[] GateActions =
    [
        "product_gates.evaluate",
        ProductGateEvaluationService.EvidenceMissingEventAction,
        ProductGateEvaluationService.EvidenceStaleEventAction,
        "product_gates.response.recorded",
    ];

    public async Task<PagedResult<ProductGateEventResponse>> ListAsync(
        Guid tenantId,
        Guid? checkResultId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var resolvedPage = page is null or < 1 ? 1 : page.Value;
        var resolvedPageSize = pageSize switch
        {
            null or < 1 => 25,
            > 100 => 100,
            _ => pageSize.Value
        };

        var query = db.AuditEvents.AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.TargetType == "workflow_gate_check_result"
                && GateActions.Contains(x.Action));
        if (checkResultId is not null && checkResultId != Guid.Empty)
        {
            var checkResultIdText = checkResultId.Value.ToString();
            query = query.Where(x => x.TargetId == checkResultIdText);
        }

        if (from is not null)
        {
            query = query.Where(x => x.OccurredAt >= from.Value);
        }

        if (to is not null)
        {
            query = query.Where(x => x.OccurredAt <= to.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var rawItems = await query
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id)
            .Skip((resolvedPage - 1) * resolvedPageSize)
            .Take(resolvedPageSize)
            .ToListAsync(cancellationToken);
        var items = rawItems
            .Select(x => new ProductGateEventResponse(
                x.Id,
                x.TenantId,
                x.ActorUserId,
                x.Action,
                x.Result,
                x.ReasonCode,
                Guid.TryParse(x.TargetId, out var parsedCheckResultId) ? parsedCheckResultId : null,
                x.OccurredAt))
            .ToList();

        return new PagedResult<ProductGateEventResponse>(
            items,
            resolvedPage,
            resolvedPageSize,
            total,
            resolvedPage * resolvedPageSize < total);
    }
}
