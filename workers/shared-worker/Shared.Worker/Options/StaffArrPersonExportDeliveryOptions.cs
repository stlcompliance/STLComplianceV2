namespace Shared.Worker.Options;

public sealed class StaffArrPersonExportDeliveryOptions
{
    public const string SectionName = "StaffArrPersonExportDelivery";

    public bool Enabled { get; set; } = true;

    public string StaffArrBaseUrl { get; set; } = "http://localhost:5102";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 60;

    public int BatchSize { get; set; } = 10;

    public Guid? TenantId { get; set; }
}
