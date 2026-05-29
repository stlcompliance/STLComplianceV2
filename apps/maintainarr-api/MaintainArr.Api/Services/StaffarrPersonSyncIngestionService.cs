using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class StaffarrPersonSyncIngestionService(
    MaintainArrDbContext db,
    TechnicianRefService technicianRefService)
{
    public async Task<IngestStaffarrPersonSyncResponse> IngestAsync(
        IngestStaffarrPersonSyncRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var personId = request.StaffarrPersonId.ToString("D");
        var correlationId = NormalizeCorrelationId(request.CorrelationId, request);

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            var replay = await db.StaffPersonRefs
                .AsNoTracking()
                .AnyAsync(
                    x => x.TenantId == request.TenantId && x.SourceCorrelationId == correlationId,
                    cancellationToken);
            if (replay)
            {
                var existing = await db.StaffPersonRefs
                    .AsNoTracking()
                    .Where(x => x.TenantId == request.TenantId && x.StaffarrPersonId == personId)
                    .Select(x => new { x.DisplayNameSnapshot })
                    .FirstOrDefaultAsync(cancellationToken);
                return new IngestStaffarrPersonSyncResponse(
                    personId,
                    existing?.DisplayNameSnapshot ?? request.DisplayName.Trim(),
                    true);
            }
        }

        var upserted = await technicianRefService.UpsertAsync(
            request.TenantId,
            null,
            new UpsertTechnicianRefRequest(
                personId,
                request.DisplayName.Trim(),
                request.EmploymentStatus.Trim().ToLowerInvariant(),
                request.PrimarySite?.Trim(),
                request.OccurredAt,
                correlationId),
            cancellationToken);

        return new IngestStaffarrPersonSyncResponse(
            upserted.PersonId,
            upserted.DisplayName,
            false);
    }

    private static void ValidateRequest(IngestStaffarrPersonSyncRequest request)
    {
        if (request.TenantId == Guid.Empty)
        {
            throw new StlApiException(
                "staffarr_person_sync.validation",
                "TenantId is required.",
                400);
        }

        if (request.StaffarrPersonId == Guid.Empty)
        {
            throw new StlApiException(
                "staffarr_person_sync.validation",
                "StaffarrPersonId is required.",
                400);
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            throw new StlApiException(
                "staffarr_person_sync.validation",
                "DisplayName is required.",
                400);
        }

        if (string.IsNullOrWhiteSpace(request.EmploymentStatus))
        {
            throw new StlApiException(
                "staffarr_person_sync.validation",
                "EmploymentStatus is required.",
                400);
        }

        if (string.IsNullOrWhiteSpace(request.EventType))
        {
            throw new StlApiException(
                "staffarr_person_sync.validation",
                "EventType is required.",
                400);
        }
    }

    private static string? NormalizeCorrelationId(string? correlationId, IngestStaffarrPersonSyncRequest request)
    {
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.Trim();
        }

        return $"{request.EventType.Trim().ToLowerInvariant()}:{request.StaffarrPersonId:D}:{request.OccurredAt:O}";
    }
}
