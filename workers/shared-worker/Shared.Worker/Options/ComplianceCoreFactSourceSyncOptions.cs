namespace Shared.Worker.Options;

public sealed class ComplianceCoreFactSourceSyncOptions
{
    public const string SectionName = "ComplianceCoreFactSourceSync";

    public bool Enabled { get; set; } = true;

    public string ComplianceCoreBaseUrl { get; set; } = "http://localhost:5107";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 15;

    public int IntervalMinutes { get; set; } = 60;

    public int BatchSize { get; set; } = 50;

    public Guid? TenantId { get; set; }
}
