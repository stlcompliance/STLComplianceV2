using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class ReadinessOverrideService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    public async Task<ReadinessOverrideResponse> GrantOverrideAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        GrantReadinessOverrideRequest request,
        CancellationToken cancellationToken = default)
    {
        var personExists = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.Id == personId,
            cancellationToken);
        if (!personExists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        var reason = NormalizeReason(request.Reason);
        ValidateExpiresAt(request.ExpiresAt);

        var activeOverrides = await db.PersonReadinessOverrides
            .Where(x => x.TenantId == tenantId && x.PersonId == personId && x.Status == "active")
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var existing in activeOverrides)
        {
            existing.Status = "cleared";
            existing.ClearedAt = now;
            existing.ClearedByUserId = actorUserId;
            existing.UpdatedAt = now;
        }

        var entity = new PersonReadinessOverride
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = personId,
            Status = "active",
            Reason = reason,
            GrantedAt = now,
            ExpiresAt = request.ExpiresAt,
            GrantedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.PersonReadinessOverrides.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "readiness_override.grant",
            tenantId,
            actorUserId,
            "person_readiness_override",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    public async Task<ReadinessOverrideResponse> ClearOverrideAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var personExists = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.Id == personId,
            cancellationToken);
        if (!personExists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        var activeOverride = await db.PersonReadinessOverrides
            .Where(x => x.TenantId == tenantId && x.PersonId == personId && x.Status == "active")
            .OrderByDescending(x => x.GrantedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeOverride is null)
        {
            throw new StlApiException("readiness_override.not_found", "No active readiness override exists for this person.", 404);
        }

        var now = DateTimeOffset.UtcNow;
        activeOverride.Status = "cleared";
        activeOverride.ClearedAt = now;
        activeOverride.ClearedByUserId = actorUserId;
        activeOverride.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "readiness_override.clear",
            tenantId,
            actorUserId,
            "person_readiness_override",
            activeOverride.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapResponse(activeOverride);
    }

    public async Task<ReadinessOverrideResponse> ClearOverrideByIdAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid overrideId,
        CancellationToken cancellationToken = default)
    {
        var activeOverride = await db.PersonReadinessOverrides
            .Where(x => x.TenantId == tenantId && x.Id == overrideId)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeOverride is null)
        {
            throw new StlApiException("readiness_override.not_found", "Readiness override was not found.", 404);
        }

        if (!string.Equals(activeOverride.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "readiness_override.not_active",
                "Readiness override is not active.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        activeOverride.Status = "cleared";
        activeOverride.ClearedAt = now;
        activeOverride.ClearedByUserId = actorUserId;
        activeOverride.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "readiness_override.clear",
            tenantId,
            actorUserId,
            "person_readiness_override",
            activeOverride.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapResponse(activeOverride);
    }

    public async Task<PersonReadinessOverride?> GetEffectiveActiveOverrideAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var candidate = await db.PersonReadinessOverrides
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId && x.Status == "active")
            .OrderByDescending(x => x.GrantedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (candidate is null)
        {
            return null;
        }

        if (candidate.ExpiresAt is DateTimeOffset expiresAt && expiresAt <= now)
        {
            return null;
        }

        return candidate;
    }

    private static string NormalizeReason(string reason)
    {
        var trimmed = reason.Trim();
        if (trimmed.Length < 8)
        {
            throw new StlApiException(
                "readiness_override.validation",
                "Override reason must be at least 8 characters.",
                400);
        }

        if (trimmed.Length > 1024)
        {
            throw new StlApiException(
                "readiness_override.validation",
                "Override reason must be 1024 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static void ValidateExpiresAt(DateTimeOffset? expiresAt)
    {
        if (expiresAt is null)
        {
            return;
        }

        if (expiresAt <= DateTimeOffset.UtcNow)
        {
            throw new StlApiException(
                "readiness_override.validation",
                "Override expiration must be in the future.",
                400);
        }
    }

    private static ReadinessOverrideResponse MapResponse(PersonReadinessOverride entity) =>
        new(
            entity.Id,
            entity.PersonId,
            entity.Status,
            entity.Reason,
            entity.GrantedAt,
            entity.ExpiresAt,
            entity.GrantedByUserId,
            entity.ClearedAt,
            entity.ClearedByUserId);
}
