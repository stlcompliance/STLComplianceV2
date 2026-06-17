namespace StaffArr.Api.Services;

public static class TenantAdminPermissionInheritanceRules
{
    public const string TenantAdminRoleKey = "tenant_admin";
    public const string TenantAdminSystemTemplateName = "Tenant Admin";

    public static bool IsTenantAdminRoleKey(string? roleKey) =>
        string.Equals(roleKey, TenantAdminRoleKey, StringComparison.OrdinalIgnoreCase);

    public static bool IsTenantAdminSystemTemplateName(string? roleName) =>
        string.Equals(roleName, TenantAdminSystemTemplateName, StringComparison.OrdinalIgnoreCase);

    public static bool IsPlatformAdminPermission(string? productKey, string? permissionKey)
    {
        if (string.IsNullOrWhiteSpace(permissionKey))
        {
            return false;
        }

        var key = permissionKey.Trim();
        var hasPlatformAdminSegment = key
            .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(segment =>
                segment.Equals("platform_admin", StringComparison.OrdinalIgnoreCase)
                || segment.Equals("platform-admin", StringComparison.OrdinalIgnoreCase)
                || segment.Equals("platformadmin", StringComparison.OrdinalIgnoreCase));

        if (hasPlatformAdminSegment)
        {
            return true;
        }

        return string.Equals(productKey, "nexarr", StringComparison.OrdinalIgnoreCase)
            && key.Contains(".platform.admin.", StringComparison.OrdinalIgnoreCase);
    }
}
