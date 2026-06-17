namespace NexArr.Api.Options;

public sealed class ProductDatabaseNukeOptions
{
    public const string SectionName = "ProductDatabaseNuke";

    public bool IsEnabled { get; set; } = true;

    public string ConfirmationPhrase { get; set; } = "NUKE PRODUCT DATA";

    public int CommandTimeoutSeconds { get; set; } = 120;

    public bool AllowLocalDatabaseNameFallback { get; set; } = true;

    public Dictionary<string, ProductDatabaseNukeTargetOptions> ProductDatabases { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed class ProductDatabaseNukeTargetOptions
{
    public string? ConnectionString { get; set; }
}
