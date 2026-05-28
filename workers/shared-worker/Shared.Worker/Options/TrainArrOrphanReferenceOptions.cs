namespace Shared.Worker.Options;

public sealed class TrainArrOrphanReferenceOptions
{
    public const string SectionName = "TrainArrOrphanReference";

    public bool Enabled { get; set; } = true;

    public string TrainArrBaseUrl { get; set; } = "http://localhost:5103";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 120;

    public int BatchSize { get; set; } = 10;

    public int StalenessHours { get; set; } = 24;

    public Guid? TenantId { get; set; }
}
