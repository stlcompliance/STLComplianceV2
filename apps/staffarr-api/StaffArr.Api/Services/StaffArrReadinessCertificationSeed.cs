using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;

namespace StaffArr.Api.Services;

public static class StaffArrReadinessCertificationSeed
{
    private static readonly (string Key, string Name, string Description, int? ValidityDays)[] BaselineDefinitions =
    [
        (
            "readiness.safety_orientation",
            "Safety Orientation",
            "Baseline safety orientation required for workforce readiness.",
            365),
        (
            "readiness.hazmat_awareness",
            "HazMat Awareness",
            "Hazardous materials awareness certification used by readiness gates.",
            365),
        (
            "readiness.equipment_operator",
            "Equipment Operator Qualification",
            "Operator qualification baseline referenced by dispatch and maintenance readiness.",
            730)
    ];

    public static async Task EnsureBaselineDefinitionsAsync(
        StaffArrDbContext db,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var existingKeys = await db.CertificationDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.CertificationKey)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var definition in BaselineDefinitions)
        {
            if (existingKeys.Contains(definition.Key, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            db.CertificationDefinitions.Add(new CertificationDefinition
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CertificationKey = definition.Key,
                Name = definition.Name,
                Description = definition.Description,
                Category = "readiness",
                DefaultValidityDays = definition.ValidityDays,
                Status = "active",
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
