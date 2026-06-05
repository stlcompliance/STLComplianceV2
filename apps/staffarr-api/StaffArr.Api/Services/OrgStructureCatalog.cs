namespace StaffArr.Api.Services;

internal static class OrgStructureCatalog
{
    public static readonly string[] OrgUnitTypes =
    [
        "company",
        "division",
        "region",
        "business_unit",
        "cost_center",
        "site",
        "department",
        "team",
        "position",
        "other"
    ];

    public static readonly string[] OrgUnitStatuses =
    [
        "planned",
        "active",
        "inactive",
        "archived"
    ];

    public static readonly string[] AssignmentStatuses =
    [
        "planned",
        "active",
        "ended",
        "canceled"
    ];

    public static readonly string[] SiteTypes =
    [
        "office",
        "warehouse",
        "plant",
        "shop",
        "yard",
        "terminal",
        "customer_embedded",
        "mixed",
        "other"
    ];

    public static readonly string[] TeamTypes =
    [
        "operational",
        "maintenance",
        "warehouse",
        "dispatch",
        "safety",
        "quality",
        "training",
        "admin",
        "project",
        "emergency_response"
    ];

    public static bool IsStructuralType(string unitType) =>
        unitType is "company" or "division" or "region" or "business_unit" or "cost_center" or "other";

    public static bool IsAssignablePlacementType(string unitType) =>
        unitType is "site" or "department" or "team" or "position";

    public static bool IsSelectableOrgUnitStatus(string status) =>
        status is "planned" or "active";

    public static bool IsSelectableAssignmentStatus(string status) =>
        status is "planned" or "active";

    public static bool IsAllowedParentType(string unitType, string? parentType)
    {
        if (parentType is null)
        {
            return unitType is "company" or "site" or "division" or "region" or "business_unit" or "cost_center" or "other";
        }

        if (IsStructuralType(unitType))
        {
            return IsStructuralType(parentType);
        }

        return unitType switch
        {
            "site" => IsStructuralType(parentType),
            "department" => parentType == "site",
            "team" => parentType == "department",
            "position" => parentType == "team",
            _ => false
        };
    }
}
