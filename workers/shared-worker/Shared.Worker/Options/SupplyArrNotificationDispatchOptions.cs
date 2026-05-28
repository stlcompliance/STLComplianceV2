namespace Shared.Worker.Options;

public sealed class SupplyArrNotificationDispatchOptions
{
    public const string SectionName = "SupplyArrNotificationDispatch";

    public bool Enabled { get; set; } = true;

    public string SupplyArrBaseUrl { get; set; } = "http://localhost:5106";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 2;

    public int BatchSize { get; set; } = 25;

    public Guid? TenantId { get; set; }
}
