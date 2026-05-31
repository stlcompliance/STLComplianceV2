namespace MaintainArr.Api.Options;

public sealed class ComplianceCoreClientOptions
{
    public const string SectionName = "ComplianceCore";

    public string BaseUrl { get; set; } = "http://localhost:5107";

    public string ServiceToken { get; set; } = string.Empty;

    public string WorkOrderActionKey { get; set; } = "can-perform-maintenance";

    public string? WorkOrderWorkflowKey { get; set; }

    public string WorkOrderActivityContextKey { get; set; } = "maintenance_work_order";

    public bool EmitWorkOrderFindings { get; set; }

    public string AssetReadinessActionKey { get; set; } = "can-dispatch-asset";

    public string? AssetReadinessWorkflowKey { get; set; }

    public string AssetReadinessActivityContextKey { get; set; } = "asset_readiness";

    public bool EmitAssetReadinessFindings { get; set; }
}
