namespace RecordArr.Api.Options;

public sealed class RedactionOverlayReviewWorkerOptions
{
    public const string SectionName = "RedactionOverlayReviewWorker";

    public bool Enabled { get; set; }

    public string[] TenantIds { get; set; } = [];

    public string RequestedByPersonId { get; set; } = "recordarr-redaction-overlay-worker";

    public string? ManifestPath { get; set; }

    public int PollIntervalSeconds { get; set; } = 300;
}
