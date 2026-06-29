namespace RecordArr.Api.Options;

public sealed class AuditAnchorWorkerOptions
{
    public const string SectionName = "AuditAnchorWorker";

    public bool Enabled { get; set; }

    public string[] TenantIds { get; set; } = [];

    public string RequestedByPersonId { get; set; } = "recordarr-audit-anchor-worker";

    public string? ManifestPath { get; set; }

    public int PollIntervalSeconds { get; set; } = 3600;
}
