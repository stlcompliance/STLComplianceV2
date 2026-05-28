namespace Shared.Worker.Options;

public sealed class SupplyArrDemandProcessingOptions
{
    public const string SectionName = "SupplyArrDemandProcessing";

    public bool Enabled { get; set; } = true;

    public string SupplyArrBaseUrl { get; set; } = "http://localhost:5106";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 30;

    public int BatchSize { get; set; } = 50;

    public int StalenessHours { get; set; } = 4;

    public Guid? TenantId { get; set; }

}
