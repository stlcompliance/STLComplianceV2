namespace Shared.Worker.Options;

public sealed class SupplyArrAvailabilitySnapshotOptions
{
    public const string SectionName = "SupplyArrAvailabilitySnapshot";

    public bool Enabled { get; set; } = true;

    public string SupplyArrBaseUrl { get; set; } = "http://localhost:5106";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 60;

    public int BatchSize { get; set; } = 100;

    public int StalenessHours { get; set; } = 24;

    public Guid? TenantId { get; set; }
}
