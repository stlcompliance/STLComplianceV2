using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class FactSource : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid FactDefinitionId { get; set; }

    public string SourceKey { get; set; } = string.Empty;

    public string SourceType { get; set; } = FactSourceTypes.StaticConfig;

    public string Label { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional owning product key (e.g. staffarr, trainarr) for product-bound sources.
    /// </summary>
    public string? ProductKey { get; set; }

    /// <summary>
    /// Optional external reference within the product (API path, entity type, etc.).
    /// </summary>
    public string? ProductReference { get; set; }

    /// <summary>
    /// JSON configuration. For static_config: typed value fields matching the fact definition value type.
    /// </summary>
    public string ConfigJson { get; set; } = "{}";

    public int Priority { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public FactDefinition? FactDefinition { get; set; }
}

public static class FactSourceTypes
{
    public const string StaticConfig = "static_config";

    /// <summary>
    /// Product API sources resolve from caller context, background sync cache, or configured fetch paths.
    /// </summary>
    public const string ProductApi = "product_api";

    /// <summary>
    /// Rebuildable mirror rows ingested from owning product APIs (e.g. SupplyArr procurement facts).
    /// </summary>
    public const string ProductMirror = "product_mirror";

    /// <summary>
    /// Generated report sources resolve from a report definition plus included event classes.
    /// </summary>
    public const string ReportGenerated = "report_generated";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        StaticConfig,
        ProductApi,
        ProductMirror,
        ReportGenerated,
    };
}
