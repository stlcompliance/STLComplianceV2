using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;

namespace StaffArr.Api.Services;

public sealed class PersonProvisioningService(StaffArrDbContext db)
{
    public async Task<PersonProvisioningResult> EnsureProvisionedAsync(
        Guid tenantId,
        Guid externalUserId,
        string email,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();
        var normalizedDisplayName = displayName.Trim();

        var existing = await db.People.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.ExternalUserId == externalUserId,
            cancellationToken);
        if (existing is not null)
        {
            var wasUpdated = false;
            if (!string.Equals(existing.PrimaryEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(existing.DisplayName, normalizedDisplayName, StringComparison.Ordinal))
            {
                existing.PrimaryEmail = normalizedEmail;
                existing.DisplayName = normalizedDisplayName;
                existing.CanLoginSnapshot = true;
                existing.HasUserAccountSnapshot = true;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
                wasUpdated = true;
            }

            return new PersonProvisioningResult(existing, false, wasUpdated);
        }

        var (givenName, familyName) = SplitName(normalizedDisplayName);
        var person = new StaffPerson
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExternalUserId = externalUserId,
            GivenName = givenName,
            FamilyName = familyName,
            LegalFirstName = givenName,
            LegalLastName = familyName,
            DisplayName = normalizedDisplayName,
            PrimaryEmail = normalizedEmail,
            EmploymentStatus = "active",
            CanLoginSnapshot = true,
            HasUserAccountSnapshot = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.People.Add(person);
        await db.SaveChangesAsync(cancellationToken);
        return new PersonProvisioningResult(person, true, false);
    }

    public async Task<StaffPerson> EnsurePersonAsync(
        Guid tenantId,
        Guid externalUserId,
        string email,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        var result = await EnsureProvisionedAsync(
            tenantId,
            externalUserId,
            email,
            displayName,
            cancellationToken);
        return result.Person;
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

public sealed record PersonProvisioningResult(
    StaffPerson Person,
    bool WasCreated,
    bool WasUpdated);
