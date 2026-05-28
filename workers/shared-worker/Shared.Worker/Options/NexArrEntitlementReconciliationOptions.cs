namespace Shared.Worker.Options;

public sealed class NexArrEntitlementReconciliationOptions
{
    public const string SectionName = "NexArrEntitlementReconciliation";

    public bool Enabled { get; set; } = true;

    public string NexArrBaseUrl { get; set; } = "http://localhost:5101";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 30;

    public int BatchSize { get; set; } = 50;
}
