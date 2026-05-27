namespace NexArr.Api.Entities;

public sealed class ServiceClient
{
    public Guid Id { get; set; }

    public string ClientKey { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string SourceProductKey { get; set; } = string.Empty;

    public string AllowedProductKeys { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ModifiedAt { get; set; }

    public ProductCatalogItem SourceProduct { get; set; } = null!;

    public ICollection<ServiceTokenRecord> Tokens { get; set; } = [];
}
