using NexArr.Api.Services;

namespace STLCompliance.E2E.Support;

/// <summary>
/// Secondary tenant identifiers for cross-tenant isolation batteries.
/// Tenant A is the seeded demo tenant; Tenant B is synthetic for isolation proofs.
/// </summary>
internal static class E2ETenants
{
    public static readonly Guid TenantAId = PlatformSeeder.DemoTenantId;
    public static readonly Guid TenantBId = Guid.Parse("88888888-8888-8888-8888-888888888801");
    public static readonly Guid TenantBUserId = Guid.Parse("88888888-8888-8888-8888-888888888802");
    public static readonly Guid TenantBPersonId = Guid.Parse("88888888-8888-8888-8888-888888888803");

    public const string TenantAAdminEmail = PlatformSeeder.DemoAdminEmail;
    public const string TenantBAdminEmail = "tenant-b-admin@e2e.stl";
}
