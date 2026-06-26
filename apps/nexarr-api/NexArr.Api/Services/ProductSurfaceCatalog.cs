using NexArr.Api.Contracts;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

/// <summary>
/// Server-driven suite navigation surfaces and availability-aware permission hints per product.
/// </summary>
public static class ProductSurfaceCatalog
{
    private static readonly HashSet<string> InSuiteProducts = new(StringComparer.OrdinalIgnoreCase)
    {
        "nexarr",
    };

    public static IReadOnlyList<NavigationSurfaceItem> BuildSurfaces(
        string productKey,
        string productStatus,
        bool hasProductAvailability,
        bool isPlatformAdmin)
    {
        var normalized = ProductKeyAliases.Normalize(productKey);
        var isLaunchable = !string.Equals(productStatus.Trim(), "worker", StringComparison.OrdinalIgnoreCase);
        var definitions = GetDefinitions(normalized, isLaunchable);
        var surfaces = new List<NavigationSurfaceItem>(definitions.Count);

        foreach (var definition in definitions)
        {
            var enabled = definition switch
            {
                { RequiresPlatformAdmin: true } => hasProductAvailability && isPlatformAdmin,
                _ => hasProductAvailability,
            };

            string? hint = null;
            if (!enabled)
            {
                hint = definition.RequiresPlatformAdmin
                    ? "Requires platform administrator access."
                    : $"Requires {normalized} access in the current tenant context.";
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

    private static IReadOnlyList<SurfaceDefinition> GetDefinitions(
        string productKey,
        bool isLaunchable) =>
        productKey switch
        {
            "nexarr" =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("identity", "Identity & access", "identity", "auth", 10),
                new("tenants", "Tenants", "tenants", "sites", 20, RequiresPlatformAdmin: true),
                new("integrations", "Integrations", "integrations", "settings", 30),
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
                new("parties", "Parties", "parties", "sites", 10),
                new("catalog", "Catalog", "catalog", "supplyarr", 20),
                new("purchasing", "Purchasing", "purchasing", "warehouse", 30),
                new("pricing", "Pricing", "pricing", "database", 40),
                new("planning", "Planning", "planning", "activity", 50),
                new("readiness", "Readiness", "readiness", "activity", 60),
                new("launch", "Open SupplyArr app", "launch", "supplyarr", 90, LaunchExternal: true),
            ],
            "customarr" =>
            [
                new("launch", "Open CustomArr app", "launch", "users", 90, LaunchExternal: true),
            ],
            "ordarr" =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("orders", "Orders", "orders", "ordarr", 10),
                new("requests", "Requests", "requests", "ordarr", 20),
                new("handoffs", "Handoffs", "handoffs", "activity", 30),
                new("completion", "Completion packets", "completion", "documents", 40),
                new("launch", "Open OrdArr app", "launch", "ordarr", 90, LaunchExternal: true),
            ],
            "loadarr" =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("inventory", "Inventory", "inventory", "warehouse", 10),
                new("receiving", "Receiving", "receiving", "warehouse", 20),
                new("launch", "Open LoadArr app", "launch", "loadarr", 90, LaunchExternal: true),
            ],
            "recordarr" =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("records", "Records", "records", "documents", 10),
                new("capture", "Capture", "capture", "activity", 20),
                new("documents", "Documents", "documents", "documents", 30),
                new("packages", "Packages", "packages", "warehouse", 40),
                new("retention", "Retention", "retention", "warning", 50),
                new("holds", "Holds", "holds", "shield", 60),
                new("access", "Access", "access", "auth", 70),
                new("launch", "Open RecordArr app", "launch", "settings", 90, LaunchExternal: true),
            ],
            "reportarr" =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("dashboards", "Dashboards", "dashboards", "reportarr", 10),
                new("reports", "Reports", "reports", "reportarr", 20),
                new("alerts", "Alerts", "alerts", "warning", 30),
                new("launch", "Open ReportArr app", "launch", "reportarr", 90, LaunchExternal: true),
            ],
            "assurarr" =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("cases", "Cases", "cases", "assurarr", 10),
                new("capa", "CAPA", "capa", "shield", 20),
                new("launch", "Open AssurArr app", "launch", "assurarr", 90, LaunchExternal: true),
            ],
            "fieldcompanion" =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("inbox", "Inbox", "inbox", "inbox", 10),
                new("tasks", "Tasks", "tasks", "activity", 20),
                new("capture", "Capture", "capture", "camera", 30),
                new("launch", "Open Field Companion app", "launch", "fieldcompanion", 90, LaunchExternal: true),
            ],
            "compliancecore" =>
            [
                new("overview", "Overview", "", "dashboard", 0, RequiresPlatformAdmin: true),
                new("vocabulary", "Vocabulary", "vocabulary", "database", 10, RequiresPlatformAdmin: true),
                new("rules", "Rule packs", "rules", "complianceCore", 20, RequiresPlatformAdmin: true),
                new("launch", "Open Compliance Core app", "launch", "complianceCore", 90, RequiresPlatformAdmin: true, LaunchExternal: true),
            ],
            _ when isLaunchable =>
            [
                new("overview", "Overview", "", "dashboard", 0),
                new("launch", "Open product app", "launch", "dashboard", 90, LaunchExternal: true),
            ],
            _ =>
            [
                new("overview", "Overview", "", "dashboard", 0),
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
