namespace Shared.Worker.Options;

public sealed class SupplyArrProcurementCoordinationOptions
{
    public const string SectionName = "SupplyArrProcurementCoordination";

    public bool Enabled { get; set; } = true;

    public string SupplyArrBaseUrl { get; set; } = "http://localhost:5106";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 30;

    public int BatchSize { get; set; } = 50;

    public int StalenessHours { get; set; } = 2;

    public Guid? TenantId { get; set; }
}
