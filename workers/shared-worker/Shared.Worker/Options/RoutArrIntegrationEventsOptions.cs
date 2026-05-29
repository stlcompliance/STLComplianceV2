namespace Shared.Worker.Options;

public sealed class RoutArrIntegrationEventsOptions
{
    public const string SectionName = "RoutArrIntegrationEvents";

    public bool Enabled { get; set; } = true;

    public string RoutArrBaseUrl { get; set; } = "http://localhost:5105";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 15;

    public int BatchSize { get; set; } = 50;

    public Guid? TenantId { get; set; }
}
