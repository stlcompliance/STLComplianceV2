namespace Shared.Worker.Options;

public sealed class MaintainArrPlatformEventProcessingOptions
{
    public const string SectionName = "MaintainArrPlatformEventProcessing";

    public bool Enabled { get; set; }

    public string MaintainArrBaseUrl { get; set; } = "http://localhost:5104";

    public string ServiceToken { get; set; } = string.Empty;

    public Guid? TenantId { get; set; }

    public int ScanIntervalMinutes { get; set; } = 5;

    public int BatchSize { get; set; } = 50;
}
