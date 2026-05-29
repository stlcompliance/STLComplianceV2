using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class TechnicianRefService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit)
{
    public async Task<TechnicianRefListResponse> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var items = await db.StaffPersonRefs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.DisplayNameSnapshot)
            .Select(x => new TechnicianRefResponse(
                x.StaffarrPersonId,
                x.DisplayNameSnapshot,
                x.ActiveStatusSnapshot,
                x.PrimarySiteSnapshot,
                x.LastSeenAt))
            .ToListAsync(cancellationToken);

        return new TechnicianRefListResponse(items);
    }

    public async Task<TechnicianRefResponse> UpsertAsync(
        Guid tenantId,
        Guid? actorUserId,
        UpsertTechnicianRefRequest request,
        CancellationToken cancellationToken = default)
    {
        var personId = ValidatePersonId(request.PersonId);
        var displayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? personId
            : request.DisplayName.Trim();

        if (displayName.Length > 256)
        {
            throw new StlApiException(
                "technician_ref.display_name_too_long",
                "Display name must be 256 characters or fewer.",
                400);
        }

        var activeStatus = NormalizeOptionalSnapshot(request.ActiveStatus, 64, "Active status");
        var primarySite = NormalizeOptionalSnapshot(request.PrimarySite, 128, "Primary site");
        var sourceCorrelationId = NormalizeOptionalSnapshot(request.SourceCorrelationId, 128, "Source correlation id");

        var now = DateTimeOffset.UtcNow;
        var entity = await db.StaffPersonRefs
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.StaffarrPersonId == personId, cancellationToken);

        if (entity is null)
        {
            entity = new MaintainArrStaffPersonRef
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                StaffarrPersonId = personId,
            };
            db.StaffPersonRefs.Add(entity);
        }

        entity.DisplayNameSnapshot = displayName;
        entity.ActiveStatusSnapshot = activeStatus;
        entity.PrimarySiteSnapshot = primarySite;
        entity.LastSeenAt = request.SourceUpdatedAt ?? now;
        entity.SourceCorrelationId = sourceCorrelationId;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "technician_ref.upsert",
            tenantId,
            actorUserId,
            "technician_ref",
            personId,
            displayName,
            cancellationToken: cancellationToken);

        return new TechnicianRefResponse(
            entity.StaffarrPersonId,
            entity.DisplayNameSnapshot,
            entity.ActiveStatusSnapshot,
            entity.PrimarySiteSnapshot,
            entity.LastSeenAt);
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
            var exists = await db.StaffPersonRefs
                .AnyAsync(x => x.TenantId == tenantId && x.StaffarrPersonId == personId.Trim(), cancellationToken);
            if (exists)
            {
                return;
            }

            displayName = personId.Trim();
        }

        await UpsertAsync(
            tenantId,
            actorUserId,
            new UpsertTechnicianRefRequest(personId, displayName, null, null, null, null),
            cancellationToken);
    }

    private static string ValidatePersonId(string? personId)
    {
        var normalized = personId?.Trim() ?? string.Empty;
        if (normalized.Length is < 8 or > 128)
        {
            throw new StlApiException(
                "technician_ref.invalid_person_id",
                "Person id must be between 8 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string? NormalizeOptionalSnapshot(string? value, int maxLength, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new StlApiException(
                "technician_ref.snapshot_too_long",
                $"{label} must be {maxLength} characters or fewer.",
                400);
        }

        return trimmed;
    }
}
