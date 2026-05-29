using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

/// <summary>
/// Structured applicability scope for requirement mapping — StaffArr owns org/role truth; keys are local references.
/// </summary>
public sealed class TrainingApplicabilityProfile : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ProfileKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string ScopeType { get; set; } = "custom";

    public string ScopeKey { get; set; } = string.Empty;

    public string? SourceProduct { get; set; }

    public string? SourceRecordId { get; set; }

    public DateTimeOffset? SourceUpdatedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public static class TrainingApplicabilityScopeTypes
{
    public const string RoleTemplate = "role_template";
    public const string OrgUnit = "org_unit";
    public const string JobCode = "job_code";
    public const string Site = "site";
    public const string Custom = "custom";
}
