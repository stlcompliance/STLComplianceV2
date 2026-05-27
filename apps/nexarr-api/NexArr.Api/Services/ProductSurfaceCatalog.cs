using NexArr.Api.Contracts;

namespace NexArr.Api.Services;

/// <summary>
/// Server-driven suite navigation surfaces and permission hints per entitled product.
/// </summary>
public static class ProductSurfaceCatalog
{
    private static readonly HashSet<string> InSuiteProducts = new(StringComparer.OrdinalIgnoreCase)
    {
        "nexarr",
    };

    public static IReadOnlyList<NavigationSurfaceItem> BuildSurfaces(
        string productKey,
        bool hasProductEntitlement,
        bool isPlatformAdmin)
    {
        var normalized = productKey.Trim().ToLowerInvariant();
        var definitions = GetDefinitions(normalized);
        var surfaces = new List<NavigationSurfaceItem>(definitions.Count);

        foreach (var definition in definitions)
        {
            var enabled = definition switch
            {
                { RequiresPlatformAdmin: true } => hasProductEntitlement && isPlatformAdmin,
                _ => hasProductEntitlement,
            };

            string? hint = null;
            if (!enabled)
            {
                hint = definition.RequiresPlatformAdmin
                    ? "Requires platform administrator access."
                    : $"Requires active {normalized} entitlement.";
            }
            else if (definition.LaunchExternal)
            {
                hint = "Opens the dedicated product workspace via NexArr handoff.";
            }

            surfaces.Add(new NavigationSurfaceItem(
                definition.SurfaceKey,
                definition.Label,
                definition.RelativePath,
                definition.IconKey,
                definition.SortOrder,
                enabled,
                hint));
        }

        return surfaces;
    }

    public static bool IsInSuiteProduct(string productKey) =>
        InSuiteProducts.Contains(productKey.Trim());

    private static IReadOnlyList<SurfaceDefinition> GetDefinitions(string productKey) =>
        productKey switch
        {
            "nexarr" =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("identity", "Identity & access", "identity", "auth", 10),
                new("tenants", "Tenants", "tenants", "sites", 20, RequiresPlatformAdmin: true),
            ],
            "staffarr" =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("people", "People directory", "people", "staffarr", 10),
                new("readiness", "Readiness", "readiness", "activity", 20),
                new("launch", "Open StaffArr app", "launch", "staffarr", 90, LaunchExternal: true),
            ],
            "trainarr" =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("assignments", "Assignments", "assignments", "training", 10),
                new("qualifications", "Qualifications", "qualifications", "training", 20),
                new("launch", "Open TrainArr app", "launch", "training", 90, LaunchExternal: true),
            ],
            "maintainarr" =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("assets", "Assets", "assets", "facilities", 10),
                new("work-orders", "Work orders", "work-orders", "inspections", 20),
                new("launch", "Open MaintainArr app", "launch", "preventiveMaintenance", 90, LaunchExternal: true),
            ],
            "routarr" =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("dispatch", "Dispatch", "dispatch", "fleet", 10),
                new("routes", "Routes", "routes", "routarr", 20),
                new("launch", "Open RoutArr app", "launch", "fleet", 90, LaunchExternal: true),
            ],
            "supplyarr" =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("inventory", "Inventory", "inventory", "inventory", 10),
                new("procurement", "Procurement", "procurement", "warehouse", 20),
                new("launch", "Open SupplyArr app", "launch", "supplyarr", 90, LaunchExternal: true),
            ],
            "compliancecore" =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("vocabulary", "Vocabulary", "vocabulary", "database", 10),
                new("rules", "Rule packs", "rules", "complianceCore", 20),
                new("launch", "Open Compliance Core app", "launch", "complianceCore", 90, LaunchExternal: true),
            ],
            _ =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("launch", "Open product app", "launch", "dashboard", 90, LaunchExternal: true),
            ],
        };

    private sealed record SurfaceDefinition(
        string SurfaceKey,
        string Label,
        string RelativePath,
        string IconKey,
        int SortOrder,
        bool RequiresPlatformAdmin = false,
        bool LaunchExternal = false);
}
