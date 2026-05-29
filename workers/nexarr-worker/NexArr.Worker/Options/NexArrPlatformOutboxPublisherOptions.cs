namespace NexArr.Worker.Options;

public sealed class NexArrPlatformOutboxPublisherOptions
{
    public const string SectionName = "NexArrPlatformOutboxPublisher";

    public bool Enabled { get; set; } = true;

    public string NexArrBaseUrl { get; set; } = "http://localhost:5101";

    public string ServiceToken { get; set; } = string.Empty;

    public int ScanIntervalMinutes { get; set; } = 1;

    public int BatchSize { get; set; } = 50;
}
