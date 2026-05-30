using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class FactDefinition : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string FactKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ValueType { get; set; } = FactValueTypes.String;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public static class FactValueTypes
{
    public const string String = "string";

    public const string Boolean = "boolean";

    public const string Number = "number";

    public const string Integer = "integer";

    public const string Date = "date";

    public const string DateTime = "datetime";

    public const string Enum = "enum";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        String,
        Boolean,
        Number,
        Integer,
        Date,
        DateTime,
        Enum
    };
}

public static class FactRequirementOperators
{
    public const string Equal = "equals";

    public const string AllTrue = "all_true";

    public const string Exists = "exists";

    public const string NotEmpty = "not_empty";

    public const string Current = "current";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Equal,
        AllTrue,
        Exists,
        NotEmpty,
        Current
    };
}

public static class FactRequirementEvidenceKinds
{
    public const string ProductRecord = "product_record";

    public const string DocumentRecord = "document_record";

    public const string SystemFact = "system_fact";

    public const string DerivedFact = "derived_fact";

    public const string ExternalRegistry = "external_registry";

    public const string InspectionRecord = "inspection_record";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ProductRecord,
        DocumentRecord,
        SystemFact,
        DerivedFact,
        ExternalRegistry,
        InspectionRecord
    };
}

public static class FactRequirementFailureSeverities
{
    public const string Info = "info";

    public const string Minor = "minor";

    public const string Major = "major";

    public const string Critical = "critical";

    public const string AutomaticFailure = "automatic_failure";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Info,
        Minor,
        Major,
        Critical,
        AutomaticFailure
    };
}

public static class ComplianceCoreProductKeys
{
    public const string StaffArr = "StaffArr";

    public const string TrainArr = "TrainArr";

    public const string MaintainArr = "MaintainArr";

    public const string RoutArr = "RoutArr";

    public const string SupplyArr = "SupplyArr";

    public const string ComplianceCore = "ComplianceCore";

    public static readonly IReadOnlyDictionary<string, string> Canonical = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [StaffArr] = StaffArr,
        [TrainArr] = TrainArr,
        [MaintainArr] = MaintainArr,
        [RoutArr] = RoutArr,
        [SupplyArr] = SupplyArr,
        [ComplianceCore] = ComplianceCore
    };
}
