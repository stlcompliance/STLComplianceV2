namespace NexArr.Api.Entities;

public sealed class ProductCatalogItem
{
    public string ProductKey { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
