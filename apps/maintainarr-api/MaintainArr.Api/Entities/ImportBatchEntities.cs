using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class MaintainArrImportBatch : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ImportType { get; set; } = string.Empty;

    public string Phase { get; set; } = string.Empty;

    public bool DryRun { get; set; }

    public string Status { get; set; } = string.Empty;

    public int TotalRows { get; set; }

    public int SuccessCount { get; set; }

    public int ErrorCount { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}

public static class MaintainArrImportTypes
{
    public const string Assets = "assets";
}

public static class MaintainArrImportPhases
{
    public const string Validate = "validate";

    public const string Commit = "commit";
}

public static class MaintainArrImportBatchStatuses
{
    public const string Completed = "completed";

    public const string Partial = "partial";

    public const string Failed = "failed";
}
