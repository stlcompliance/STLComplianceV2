using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class IntegrationPermissionCheckService(
    StaffArrDbContext db,
    PermissionProjectionService permissionProjectionService)
{
    public async Task<IntegrationPermissionCheckResponse> CheckAsync(
        Guid tenantId,
        Guid personId,
        IReadOnlyList<string> permissionKeys,
        CancellationToken cancellationToken = default)
    {
        var normalizedKeys = permissionKeys
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedKeys.Count == 0)
        {
            throw new StlApiException(
                "permission_check.validation",
                "At least one permissionKey query parameter is required.",
                400);
        }

        var person = await db.People.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == personId)
            .Select(x => new { x.Id, x.ExternalUserId, x.EmploymentStatus })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new StlApiException("people.not_found", "Person was not found.", 404);

        var isActive = string.Equals(person.EmploymentStatus, "active", StringComparison.OrdinalIgnoreCase);
        if (!isActive)
        {
            var inactiveChecks = normalizedKeys
                .Select(x => new IntegrationPermissionCheckItemResponse(x, false, []))
                .ToList();
            return new IntegrationPermissionCheckResponse(
                person.Id,
                person.ExternalUserId,
                false,
                DateTimeOffset.UtcNow,
                false,
                false,
                inactiveChecks);
        }

        var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
            tenantId,
            personId,
            cancellationToken);
        var checks = new List<IntegrationPermissionCheckItemResponse>();
        foreach (var key in normalizedKeys)
        {
            var matches = projection.Permissions
                .Where(x => string.Equals(x.PermissionKey, key, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.Sources.Select(source => new IntegrationPermissionCheckGrantResponse(
                    x.PermissionKey,
                    x.PermissionName,
                    x.ScopeType,
                    x.ScopeValue,
                    source.RoleKey,
                    source.RoleName)))
                .ToList();

            checks.Add(new IntegrationPermissionCheckItemResponse(
                key,
                matches.Count > 0,
                matches));
        }

        return new IntegrationPermissionCheckResponse(
            person.Id,
            person.ExternalUserId,
            true,
            projection.ComputedAt,
            checks.All(x => x.Granted),
            checks.Any(x => x.Granted),
            checks);
    }
}
