namespace Shared.Worker.Options;

public sealed class NexArrTenantLifecycleOptions
{
    public const string SectionName = "NexArrTenantLifecycle";

    public bool Enabled { get; set; } = true;

    public string NexArrBaseUrl { get; set; } = "http://localhost:5101";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 60;

    public int BatchSize { get; set; } = 25;
}
