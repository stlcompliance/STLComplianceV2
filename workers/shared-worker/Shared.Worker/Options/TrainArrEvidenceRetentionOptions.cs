namespace Shared.Worker.Options;

public sealed class TrainArrEvidenceRetentionOptions
{
    public const string SectionName = "TrainArrEvidenceRetention";

    public bool Enabled { get; set; } = true;

    public string TrainArrBaseUrl { get; set; } = "http://localhost:5103";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 60;

    public int BatchSize { get; set; } = 50;

    public Guid? TenantId { get; set; }
}
