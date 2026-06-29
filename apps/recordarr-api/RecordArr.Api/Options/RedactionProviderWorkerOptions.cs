namespace RecordArr.Api.Options;

public sealed class RedactionProviderWorkerOptions
{
    public const string SectionName = "RedactionProviderWorker";

    public bool Enabled { get; set; }

    public string[] TenantIds { get; set; } = [];

    public string RequestedByPersonId { get; set; } = "recordarr-redaction-provider-worker";

    public string? ManifestPath { get; set; }

    public int PollIntervalSeconds { get; set; } = 300;
}
