using StaffArr.Api.Entities;

namespace StaffArr.Api.Services;

internal static class PersonCertificationEffectiveStatus
{
    public static string Resolve(PersonCertification entity)
    {
        if (string.Equals(entity.Status, "revoked", StringComparison.OrdinalIgnoreCase))
        {
            return "revoked";
        }

        if (string.Equals(entity.Status, "expired", StringComparison.OrdinalIgnoreCase))
        {
            return "expired";
        }

        if (entity.ExpiresAt is DateTimeOffset expiresAt && expiresAt <= DateTimeOffset.UtcNow)
        {
            return "expired";
        }

        return entity.Status;
    }
}
