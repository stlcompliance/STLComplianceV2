namespace Shared.Worker.Options;

public sealed class ComplianceCoreRuleChangeMonitorOptions
{
    public const string SectionName = "ComplianceCoreRuleChangeMonitor";

    public bool Enabled { get; set; } = true;

    public string ComplianceCoreBaseUrl { get; set; } = "http://localhost:5107";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 30;

    public int BatchSize { get; set; } = 100;

    public Guid? TenantId { get; set; }
}
