using Microsoft.Extensions.DependencyInjection;
using ComplianceCore.Api.Services;
using MaintainArr.Api.Services;
using NexArr.Api.Services;
using RoutArr.Api.Services;
using StaffArr.Api.Services;
using SupplyArr.Api.Services;
using TrainArr.Api.Services;

namespace STLCompliance.E2E.Support;

internal static class E2EAccessTokenHelper
{
    public static string StaffArr(
        IServiceProvider services,
        Guid tenantId,
        Guid userId,
        Guid personId,
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin") =>
        Mint<StaffArrTokenService>(services, tenantId, userId, personId, entitlements, tenantRoleKey);

    public static string MaintainArr(
        IServiceProvider services,
        Guid tenantId,
        Guid userId,
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin") =>
        Mint<MaintainArrTokenService>(services, tenantId, userId, userId, entitlements, tenantRoleKey);

    public static string RoutArr(
        IServiceProvider services,
        Guid tenantId,
        Guid userId,
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin") =>
        Mint<RoutArrTokenService>(services, tenantId, userId, userId, entitlements, tenantRoleKey);

    public static string TrainArr(
        IServiceProvider services,
        Guid tenantId,
        Guid userId,
        Guid personId,
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin") =>
        Mint<TrainArrTokenService>(services, tenantId, userId, personId, entitlements, tenantRoleKey);

    public static string ComplianceCore(
        IServiceProvider services,
        Guid tenantId,
        Guid userId,
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "compliance_admin") =>
        Mint<ComplianceCoreTokenService>(services, tenantId, userId, userId, entitlements, tenantRoleKey);

    public static string SupplyArr(
        IServiceProvider services,
        Guid tenantId,
        Guid userId,
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin") =>
        Mint<SupplyArrTokenService>(services, tenantId, userId, userId, entitlements, tenantRoleKey);

    private static string Mint<TTokenService>(
        IServiceProvider services,
        Guid tenantId,
        Guid userId,
        Guid personId,
        IReadOnlyList<string> entitlements,
        string tenantRoleKey)
        where TTokenService : notnull
    {
        using var scope = services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TTokenService>();
        var (accessToken, _) = tokenService switch
        {
            StaffArrTokenService staff => staff.CreateAccessToken(
                userId,
                personId,
                E2ETenants.TenantAAdminEmail,
                "E2E Isolation Admin",
                tenantId,
                Guid.NewGuid(),
                tenantRoleKey,
                entitlements,
                isPlatformAdmin: false),
            MaintainArrTokenService maintain => maintain.CreateAccessToken(
                userId,
                personId,
                E2ETenants.TenantAAdminEmail,
                "E2E Isolation Admin",
                tenantId,
                Guid.NewGuid(),
                tenantRoleKey,
                entitlements,
                isPlatformAdmin: false),
            RoutArrTokenService rout => rout.CreateAccessToken(
                userId,
                personId,
                E2ETenants.TenantAAdminEmail,
                "E2E Isolation Admin",
                tenantId,
                Guid.NewGuid(),
                tenantRoleKey,
                entitlements,
                isPlatformAdmin: false),
            TrainArrTokenService train => train.CreateAccessToken(
                userId,
                personId,
                E2ETenants.TenantAAdminEmail,
                "E2E Isolation Admin",
                tenantId,
                Guid.NewGuid(),
                tenantRoleKey,
                entitlements,
                isPlatformAdmin: false),
            ComplianceCoreTokenService compliance => compliance.CreateAccessToken(
                userId,
                personId,
                E2ETenants.TenantAAdminEmail,
                "E2E Isolation Admin",
                tenantId,
                Guid.NewGuid(),
                tenantRoleKey,
                entitlements,
                isPlatformAdmin: false),
            SupplyArrTokenService supply => supply.CreateAccessToken(
                userId,
                personId,
                E2ETenants.TenantAAdminEmail,
                "E2E Isolation Admin",
                tenantId,
                Guid.NewGuid(),
                tenantRoleKey,
                entitlements,
                isPlatformAdmin: false),
            _ => throw new InvalidOperationException($"Unsupported token service type {typeof(TTokenService).Name}.")
        };
        return accessToken;
    }
}
