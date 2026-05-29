namespace Shared.Worker.Options;

public sealed class MaintainArrTechnicianRefRefreshOptions
{
    public const string SectionName = "MaintainArrTechnicianRefRefresh";

    public bool Enabled { get; set; } = true;

    public string MaintainArrBaseUrl { get; set; } = "http://localhost:5104";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 60;

    public int BatchSize { get; set; } = 50;

    public int StaleAfterHours { get; set; } = 24;

    public Guid? TenantId { get; set; }
}
