namespace RecordArr.Api.Options;

public sealed class ObjectStoreReconciliationWorkerOptions
{
    public const string SectionName = "ObjectStoreReconciliationWorker";

    public bool Enabled { get; set; }

    public string[] TenantIds { get; set; } = [];

    public string RequestedByPersonId { get; set; } = "recordarr-object-store-worker";

    public string? InventoryManifestPath { get; set; }

    public int PollIntervalSeconds { get; set; } = 300;
}
