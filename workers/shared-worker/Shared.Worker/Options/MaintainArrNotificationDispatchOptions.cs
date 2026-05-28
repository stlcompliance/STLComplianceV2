namespace Shared.Worker.Options;

public sealed class MaintainArrNotificationDispatchOptions
{
    public const string SectionName = "MaintainArrNotificationDispatch";

    public bool Enabled { get; set; } = true;

    public string MaintainArrBaseUrl { get; set; } = "http://localhost:5104";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 15;

    public int BatchSize { get; set; } = 50;

    public Guid? TenantId { get; set; }
}
