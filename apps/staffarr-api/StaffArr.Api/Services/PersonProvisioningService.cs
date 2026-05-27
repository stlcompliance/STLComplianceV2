using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;

namespace StaffArr.Api.Services;

public sealed class PersonProvisioningService(StaffArrDbContext db)
{
    public async Task<StaffPerson> EnsurePersonAsync(
        Guid tenantId,
        Guid externalUserId,
        string email,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        var existing = await db.People.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.ExternalUserId == externalUserId,
            cancellationToken);
        if (existing is not null)
        {
            if (!string.Equals(existing.PrimaryEmail, email, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(existing.DisplayName, displayName, StringComparison.Ordinal))
            {
                existing.PrimaryEmail = email.Trim();
                existing.DisplayName = displayName.Trim();
                existing.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
            }

            return existing;
        }

        var (givenName, familyName) = SplitName(displayName);
        var person = new StaffPerson
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExternalUserId = externalUserId,
            GivenName = givenName,
            FamilyName = familyName,
            DisplayName = displayName.Trim(),
            PrimaryEmail = email.Trim(),
            EmploymentStatus = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.People.Add(person);
        await db.SaveChangesAsync(cancellationToken);
        return person;
    }

    private static (string GivenName, string FamilyName) SplitName(string displayName)
    {
        var parts = displayName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return ("Unknown", "User");
        }

        if (parts.Length == 1)
        {
            return (parts[0], "User");
        }

        return (parts[0], string.Join(' ', parts.Skip(1)));
    }
}
