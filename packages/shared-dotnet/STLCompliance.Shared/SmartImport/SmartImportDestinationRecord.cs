namespace STLCompliance.Shared.SmartImport;

public sealed class SmartImportDestinationRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ActorPersonId { get; set; }
    public Guid ApprovedByPersonId { get; set; }
    public Guid ImportBatchId { get; set; }
    public Guid CommitPlanId { get; set; }
    public Guid CommitStepId { get; set; }
    public string DestinationProduct { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public string? RecordArrSourceRecordId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = "committed";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
