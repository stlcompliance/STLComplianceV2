using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class PersonExportPresetService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    private static readonly HashSet<string> AllowedEmploymentStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "active", "inactive", "terminated" };

    private static readonly HashSet<string> AllowedPresetKeys =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "all-people",
            "active-workforce",
            "inactive-records",
            "terminated-records",
            "active-at-org-unit",
        };

    public async Task<PersonExportPresetResponse?> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var preset = await db.TenantPersonExportPresets
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new PersonExportPresetResponse(
                x.EmploymentStatus,
                x.OrgUnitId,
                x.PresetKey,
                x.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return preset;
    }

    public async Task<PersonExportPresetResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertPersonExportPresetRequest request,
        CancellationToken cancellationToken = default)
    {
        var employmentStatus = NormalizeEmploymentStatus(request.EmploymentStatus);
        var orgUnitId = request.OrgUnitId;
        var presetKey = NormalizePresetKey(request.PresetKey);

        if (orgUnitId is Guid requestedOrgUnitId)
        {
            var orgUnitExists = await db.OrgUnits.AnyAsync(
                x => x.TenantId == tenantId && x.Id == requestedOrgUnitId && x.Status == "active",
                cancellationToken);
            if (!orgUnitExists)
            {
                throw new StlApiException(
                    "people.export_preset.org_unit_not_found",
                    "Primary org unit was not found for this tenant.",
                    404);
            }
        }

        if (string.Equals(presetKey, "active-at-org-unit", StringComparison.OrdinalIgnoreCase)
            && orgUnitId is null)
        {
            throw new StlApiException(
                "people.export_preset.org_unit_required",
                "Org unit is required for the active-at-org-unit preset.",
                400);
        }

        var entity = await db.TenantPersonExportPresets
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantPersonExportPreset
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantPersonExportPresets.Add(entity);
        }

        entity.EmploymentStatus = employmentStatus;
        entity.OrgUnitId = orgUnitId;
        entity.PresetKey = presetKey;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "person.export_preset.update",
            tenantId,
            actorUserId,
            "tenant_person_export_preset",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return new PersonExportPresetResponse(
            entity.EmploymentStatus,
            entity.OrgUnitId,
            entity.PresetKey,
            entity.UpdatedAt);
    }

    private static string? NormalizeEmploymentStatus(string? employmentStatus)
    {
        if (string.IsNullOrWhiteSpace(employmentStatus))
        {
            return null;
        }

        var normalized = employmentStatus.Trim().ToLowerInvariant();
        if (!AllowedEmploymentStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "people.export_preset.invalid_employment_status",
                "Employment status must be active, inactive, or terminated.",
                400);
        }

        return normalized;
    }

    private static string? NormalizePresetKey(string? presetKey)
    {
        if (string.IsNullOrWhiteSpace(presetKey))
        {
            return null;
        }

        var normalized = presetKey.Trim().ToLowerInvariant();
        if (!AllowedPresetKeys.Contains(normalized))
        {
            throw new StlApiException(
                "people.export_preset.invalid_preset_key",
                "Preset key is not recognized.",
                400);
        }

        return normalized;
    }
}
