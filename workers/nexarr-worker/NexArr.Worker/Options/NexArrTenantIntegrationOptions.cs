namespace NexArr.Worker.Options;

public sealed class NexArrTenantIntegrationOptions
{
    public const string SectionName = "NexArrTenantIntegrations";

    public bool Enabled { get; set; } = true;

    public string NexArrBaseUrl { get; set; } = "http://localhost:5101";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 5;

    public int BatchSize { get; set; } = 25;
}
