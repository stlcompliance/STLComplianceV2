namespace NexArr.Api.Entities;

public sealed class ProductCallbackAllowlistEntry
{
    public Guid Id { get; set; }

    public string ProductKey { get; set; } = string.Empty;

    public Guid? TenantId { get; set; }

    public string UrlPattern { get; set; } = string.Empty;

    public string PatternType { get; set; } = CallbackPatternTypes.Origin;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ModifiedAt { get; set; }

    public ProductCatalogItem Product { get; set; } = null!;

    public Tenant? Tenant { get; set; }
}

public static class CallbackPatternTypes
{
    public const string Origin = "origin";

    public const string Prefix = "prefix";
}
