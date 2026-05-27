namespace NexArr.Api.Options;

public sealed class StlLaunchOptions
{
    public const string SectionName = "Launch";

    public int HandoffLifetimeMinutes { get; set; } = 5;

    public Dictionary<string, ProductLaunchUrlOptions> Products { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class ProductLaunchUrlOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public string LaunchPath { get; set; } = "/";
}
