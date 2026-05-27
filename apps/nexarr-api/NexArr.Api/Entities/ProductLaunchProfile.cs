namespace NexArr.Api.Entities;

public sealed class ProductLaunchProfile
{
    public string ProductKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public string LaunchPath { get; set; } = "/";

    public bool IsActive { get; set; } = true;

    public DateTimeOffset ModifiedAt { get; set; }

    public ProductCatalogItem Product { get; set; } = null!;
}
