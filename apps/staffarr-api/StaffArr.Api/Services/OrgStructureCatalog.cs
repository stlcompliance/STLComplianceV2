using STLCompliance.Shared.Contracts;

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

    public static readonly string[] LocationTypes =
    [
        "site",
        "building",
        "warehouse",
        "dock",
        "room",
        "yard",
        "parts_room",
        "staging_area",
        "quarantine_area",
        "inspection_hold",
        "receiving_staging",
        "putaway_queue",
        "maintenance_handoff",
        "service_counter",
        "technician_pickup",
        "service_truck",
        "shelf",
        "bin",
        "parking_area",
        "work_cell",
        "production_line",
        "office",
        "training_room",
        "break_room",
        "restricted_area",
        "company",
        "division",
        "region",
        "business_unit",
        "cost_center",
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

    public static readonly string[] LocationStatuses =
    [
        "planned",
        "active",
        "inactive",
        "restricted",
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

    public static readonly string[] AllowedProductUsages =
    [
        "maintainarr",
        "loadarr",
        "routarr",
        "trainarr",
        "staffarr",
        "compliancecore",
        "all"
    ];

    public static bool IsStructuralType(string unitType) =>
        unitType is "company" or "division" or "region" or "business_unit" or "cost_center" or "other";

    public static bool IsAssignablePlacementType(string unitType) =>
        unitType is "site" or "department" or "team" or "position";

    public static bool IsSelectableOrgUnitStatus(string status) =>
        status is "planned" or "active";

    public static bool IsSelectableAssignmentStatus(string status) =>
        status is "planned" or "active";

    public static bool IsSelectableLocationStatus(string status) =>
        status is "planned" or "active" or "restricted";

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

    public static string BuildStableCode(string prefix, Guid id) =>
        $"{prefix}-{id:N}".ToUpperInvariant();

    public static string NormalizeCode(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length == 0)
        {
            throw new StlApiException("org_unit.validation", "Code is required.", 400);
        }

        var buffer = new char[normalized.Length];
        var bufferLength = 0;
        var previousWasSeparator = false;

        foreach (var character in normalized)
        {
            var isAllowed = character is >= 'A' and <= 'Z'
                or >= '0' and <= '9'
                or '-'
                or '_';

            if (!isAllowed)
            {
                if (previousWasSeparator || bufferLength == 0)
                {
                    continue;
                }

                buffer[bufferLength++] = '-';
                previousWasSeparator = true;
                continue;
            }

            buffer[bufferLength++] = character;
            previousWasSeparator = character is '-' or '_';
        }

        var result = new string(buffer[..bufferLength]).Trim('-', '_');
        if (string.IsNullOrWhiteSpace(result))
        {
            throw new StlApiException("org_unit.validation", "Code must contain at least one alphanumeric character.", 400);
        }

        return result.Length > 64 ? result[..64] : result;
    }
}
