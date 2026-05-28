using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

/// <summary>
/// Rebuildable local mirror of Compliance Core regulatory/compliance keys linked to MaintainArr subjects.
/// </summary>
public sealed class ComplianceRegulatoryKeyMirror : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>inspection_template | asset_type | pm_program</summary>
    public string SubjectType { get; set; } = string.Empty;

    public Guid SubjectId { get; set; }

    public string ComplianceKey { get; set; } = string.Empty;

    public string? MaterialKey { get; set; }

    public string? RegulatoryCitationKey { get; set; }

    public string SourceProduct { get; set; } = ComplianceRegulatoryKeyMirrorSources.ComplianceCore;

    public string SourceRecordKey { get; set; } = string.Empty;

    public DateTimeOffset SourceUpdatedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public static class ComplianceRegulatoryKeyMirrorSources
{
    public const string ComplianceCore = "compliancecore";
}

public static class ComplianceRegulatoryKeyMirrorSubjectTypes
{
    public const string InspectionTemplate = "inspection_template";

    public const string AssetType = "asset_type";

    public const string PmProgram = "pm_program";
}
