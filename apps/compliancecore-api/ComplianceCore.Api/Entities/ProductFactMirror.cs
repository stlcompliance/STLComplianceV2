using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class ProductFactMirror : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string SourceProduct { get; set; } = string.Empty;

    public string FactKey { get; set; } = string.Empty;

    public string ScopeKey { get; set; } = "tenant";

    public string ValueType { get; set; } = FactValueTypes.String;

    public string? StringValue { get; set; }

    public bool? BooleanValue { get; set; }

    public decimal? NumberValue { get; set; }

    public DateOnly? DateValue { get; set; }

    public string SourceEntityType { get; set; } = string.Empty;

    public Guid? SourceEntityId { get; set; }

    public string SourceEventKind { get; set; } = string.Empty;

    public Guid SourcePublicationId { get; set; }

    public string IdempotencyKey { get; set; } = string.Empty;

    public DateTimeOffset PublishedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
