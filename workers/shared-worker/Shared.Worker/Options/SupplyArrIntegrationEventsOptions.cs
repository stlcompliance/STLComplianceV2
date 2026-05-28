namespace Shared.Worker.Options;

public sealed class SupplyArrIntegrationEventsOptions
{
    public const string SectionName = "SupplyArrIntegrationEvents";

    public bool Enabled { get; set; } = true;

    public string SupplyArrBaseUrl { get; set; } = "http://localhost:5106";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 15;

    public int BatchSize { get; set; } = 50;

    public Guid? TenantId { get; set; }
}
