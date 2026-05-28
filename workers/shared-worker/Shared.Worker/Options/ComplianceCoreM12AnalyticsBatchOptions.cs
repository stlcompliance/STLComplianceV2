namespace Shared.Worker.Options;

public sealed class ComplianceCoreM12AnalyticsBatchOptions
{
    public const string SectionName = "ComplianceCoreM12AnalyticsBatch";

    public bool Enabled { get; set; } = true;

    public string ComplianceCoreBaseUrl { get; set; } = "http://localhost:5107";

    public string ServiceToken { get; set; } = "";

    public int ScanIntervalMinutes { get; set; } = 60;

    public int BatchSize { get; set; } = 25;

    public int IntervalHours { get; set; } = 24;

    public Guid? TenantId { get; set; }
}
