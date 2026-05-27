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

    public const string Date = "date";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        String,
        Boolean,
        Number,
        Date
    };
}
