namespace Shared.Worker.Options;

public sealed class NexArrCompanionNotificationDispatchOptions
{
    public const string SectionName = "NexArrCompanionNotificationDispatch";

    public bool Enabled { get; set; } = true;

    public string NexArrBaseUrl { get; set; } = "http://localhost:5101";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 2;

    public int BatchSize { get; set; } = 25;

    public Guid? TenantId { get; set; }
}
