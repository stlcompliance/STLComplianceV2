using System.ComponentModel.DataAnnotations.Schema;
using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

[NotMapped]
public abstract class ProductObjectReferenceBase : IHasTenant
{
    public Guid ReferenceId { get; set; }

    public Guid TenantId { get; set; }

    public string SourceProduct { get; set; } = string.Empty;

    public string ObjectKind { get; set; } = string.Empty;

    public string ExternalRecordId { get; set; } = string.Empty;

    public string StableKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    public DateTimeOffset? LastSeenAt { get; set; }

    public string MetadataJson { get; set; } = "{}";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ExternalObjectReference : ProductObjectReferenceBase;

public sealed class DocumentReference : ProductObjectReferenceBase;

public sealed class MaterialReference : ProductObjectReferenceBase;

public sealed class PartReference : ProductObjectReferenceBase;

public sealed class SystemReference : ProductObjectReferenceBase;

public sealed class AssetReference : ProductObjectReferenceBase;
