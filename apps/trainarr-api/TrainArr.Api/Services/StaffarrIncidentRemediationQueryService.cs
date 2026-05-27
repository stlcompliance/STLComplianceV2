using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class StaffarrIncidentRemediationQueryService(TrainArrDbContext db)
{
    public async Task<IReadOnlyList<StaffarrIncidentRemediationResponse>> ListAsync(
        Guid tenantId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.StaffarrIncidentRemediations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status == normalizedStatus);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new StaffarrIncidentRemediationResponse(
                x.Id,
                x.TenantId,
                x.StaffarrIncidentId,
                x.StaffarrPersonId,
                x.ReasonCategoryKey,
                x.Status,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<StaffarrIncidentRemediationDetailResponse> GetAsync(
        Guid tenantId,
        Guid remediationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.StaffarrIncidentRemediations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == remediationId, cancellationToken);
        if (entity is null)
        {
            throw new StlApiException(
                "incident_remediations.not_found",
                "StaffArr incident remediation was not found.",
                404);
        }

        return new StaffarrIncidentRemediationDetailResponse(
            entity.Id,
            entity.TenantId,
            entity.StaffarrIncidentId,
            entity.StaffarrPersonId,
            entity.ReasonCategoryKey,
            entity.Severity,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.OccurredAt,
            entity.ReportedAt,
            entity.CreatedAt);
    }
}
