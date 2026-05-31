using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class ProductGateResponse : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid WorkflowGateCheckResultId { get; set; }

    public string SourceProduct { get; set; } = string.Empty;

    public string ResponseOutcome { get; set; } = string.Empty;

    public string? ResponseCode { get; set; }

    public string? ResponseMessage { get; set; }

    public string ResponsePayloadJson { get; set; } = "{}";

    public DateTimeOffset RespondedAt { get; set; }

    public WorkflowGateCheckResult? WorkflowGateCheckResult { get; set; }
}
