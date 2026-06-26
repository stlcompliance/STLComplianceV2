namespace Shared.Worker.Options;

public sealed class NexArrLaunchDestinationReconciliationOptions
{
    public const string SectionName = "NexArrLaunchDestinationReconciliation";
    public const string CompatibilityLegacySectionName = "NexArrEntitlementReconciliation";

    public bool Enabled { get; set; } = true;

    public string NexArrBaseUrl { get; set; } = "http://localhost:5101";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 30;

    public int BatchSize { get; set; } = 50;
}
