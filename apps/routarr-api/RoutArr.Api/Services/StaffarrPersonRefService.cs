using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class StaffarrPersonRefService(
    RoutArrDbContext db,
    IRoutArrAuditService audit)
{
    public async Task<StaffarrPersonRefListResponse> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var items = await db.StaffarrPersonRefs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.DisplayName)
            .Select(x => new StaffarrPersonRefResponse(x.PersonId, x.DisplayName, x.MirroredAt))
            .ToListAsync(cancellationToken);

        return new StaffarrPersonRefListResponse(items);
    }

    public async Task<StaffarrPersonRefResponse> UpsertAsync(
        Guid tenantId,
        Guid? actorUserId,
        UpsertStaffarrPersonRefRequest request,
        CancellationToken cancellationToken = default)
    {
        var personId = ValidatePersonId(request.PersonId);
        var displayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? personId
            : request.DisplayName.Trim();

        if (displayName.Length > 256)
        {
            throw new StlApiException(
                "staffarr_person_ref.display_name_too_long",
                "Display name must be 256 characters or fewer.",
                400);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = await db.StaffarrPersonRefs
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PersonId == personId, cancellationToken);

        if (entity is null)
        {
            entity = new StaffarrPersonRef
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PersonId = personId,
            };
            db.StaffarrPersonRefs.Add(entity);
        }

        entity.DisplayName = displayName;
        entity.SourceUpdatedAt = request.SourceUpdatedAt ?? now;
        entity.MirroredAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "staffarr_person_ref.upsert",
            tenantId,
            actorUserId,
            "staffarr_person_ref",
            personId,
            displayName,
            cancellationToken: cancellationToken);

        return new StaffarrPersonRefResponse(entity.PersonId, entity.DisplayName, entity.MirroredAt);
    }

    public async Task UpsertFromAssignmentAsync(
        Guid tenantId,
        Guid? actorUserId,
        string personId,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            var exists = await db.StaffarrPersonRefs
                .AnyAsync(x => x.TenantId == tenantId && x.PersonId == personId, cancellationToken);
            if (exists)
            {
                return;
            }

            displayName = personId;
        }

        await UpsertAsync(
            tenantId,
            actorUserId,
            new UpsertStaffarrPersonRefRequest(personId, displayName, null),
            cancellationToken);
    }

    private static string ValidatePersonId(string? personId)
    {
        var normalized = personId?.Trim() ?? string.Empty;
        if (normalized.Length is < 8 or > 128)
        {
            throw new StlApiException(
                "staffarr_person_ref.invalid_person_id",
                "Person id must be between 8 and 128 characters.",
                400);
        }

        return normalized;
    }
}
