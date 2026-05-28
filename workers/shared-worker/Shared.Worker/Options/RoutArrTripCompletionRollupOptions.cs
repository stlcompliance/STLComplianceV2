namespace Shared.Worker.Options;

public sealed class RoutArrTripCompletionRollupOptions
{
    public const string SectionName = "RoutArrTripCompletionRollup";

    public bool Enabled { get; set; } = true;

    public string RoutArrBaseUrl { get; set; } = "http://localhost:5105";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 30;

    public int BatchSize { get; set; } = 50;

    public int StalenessHours { get; set; } = 1;

    public Guid? TenantId { get; set; }
}
