namespace STLCompliance.Shared.Operations;

public sealed record LedgArrPermissionCatalogItem(
    string PermissionKey,
    string Label,
    string? Description,
    string Scope,
    string Sensitivity,
    string Status = "active");

public static class StlLedgArrPermissionCatalog
{
    public static IReadOnlyList<LedgArrPermissionCatalogItem> All { get; } =
    [
        P("ledgarr.settings.view", "View LedgArr Settings", "View LedgArr tenant settings and administrative accounting configuration.", "tenant", "sensitive"),
        P("ledgarr.settings.manage", "Manage LedgArr Settings", "Update LedgArr tenant settings and accounting configuration.", "tenant", "critical"),
        P("ledgarr.legalEntities.view", "View Financial Legal Entity Settings", "View LedgArr legal-entity configuration inside tenant settings.", "tenant", "sensitive"),
        P("ledgarr.legalEntities.manage", "Manage Financial Legal Entity Settings", "Update LedgArr legal-entity configuration inside tenant settings.", "tenant", "critical"),
        P("ledgarr.chartOfAccounts.view", "View Chart Of Accounts Settings", "View LedgArr chart-of-accounts configuration inside tenant settings.", "tenant", "sensitive"),
        P("ledgarr.chartOfAccounts.manage", "Manage Chart Of Accounts Settings", "Update LedgArr chart-of-accounts configuration inside tenant settings.", "tenant", "critical"),
        P("ledgarr.postingRules.view", "View Posting Source Settings", "View LedgArr posting-source and posting-rule settings.", "tenant", "sensitive"),
        P("ledgarr.postingRules.manage", "Manage Posting Source Settings", "Update LedgArr posting-source and posting-rule settings.", "tenant", "critical"),
        P("ledgarr.periodClose.view", "View Period Close Settings", "View LedgArr close and audit configuration.", "tenant", "sensitive"),
        P("ledgarr.periodClose.manage", "Manage Period Close Settings", "Update LedgArr close and audit configuration.", "tenant", "critical"),
        P("ledgarr.integrations.view", "View Integration Settings", "View LedgArr external ERP and integration settings.", "tenant", "sensitive"),
        P("ledgarr.integrations.manage", "Manage Integration Settings", "Update LedgArr external ERP and integration settings.", "tenant", "critical"),
    ];

    private static LedgArrPermissionCatalogItem P(
        string permissionKey,
        string label,
        string description,
        string scope,
        string sensitivity,
        string status = "active") =>
        new(permissionKey, label, description, scope, sensitivity, status);
}
