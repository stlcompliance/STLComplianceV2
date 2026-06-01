using System.Text.Json;
using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class CatalogDefinition : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Owner { get; set; } = "maintainarr";
    public string Scope { get; set; } = "tenant";
    public bool IsSystem { get; set; } = true;
    public bool IsTenantExtendable { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CatalogOption : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CatalogId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public Guid? ParentOptionId { get; set; }
    public string MetadataJson { get; set; } = "{}";
    public bool IsSystem { get; set; } = true;
    public bool IsTenantSpecific { get; set; }
    public Guid? OptionTenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CatalogOptionDependency : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CatalogOptionId { get; set; }
    public string DependsOnCatalogKey { get; set; } = string.Empty;
    public string DependsOnOptionKey { get; set; } = string.Empty;
    public string RuleJson { get; set; } = "{}";
}

public sealed class FieldsetDefinition : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string EntityType { get; set; } = "asset";
    public string Purpose { get; set; } = "create";
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class FieldsetField : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid FieldsetId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    public string ControlType { get; set; } = "text";
    public bool Required { get; set; }
    public string? CatalogKey { get; set; }
    public string? ReferenceKey { get; set; }
    public string SourceType { get; set; } = "maintainarr_catalog";
    public string SourceOfTruth { get; set; } = "MaintainArr";
    public int SortOrder { get; set; }
    public string SectionKey { get; set; } = "core";
    public string DependencyJson { get; set; } = "{}";
    public string ValidationJson { get; set; } = "{}";
    public string DefaultValueJson { get; set; } = "null";
    public string VisibilityJson { get; set; } = "{}";
    public bool AllowCustom { get; set; }
    public bool CustomRequiresApproval { get; set; }
    public bool DrivesLogic { get; set; }
    public bool DrivesInspectionBranching { get; set; }
    public bool DrivesPMApplicability { get; set; }
    public bool DrivesCompliance { get; set; }
    public bool DrivesReporting { get; set; }
    public bool DrivesReadiness { get; set; }
}

public sealed class PendingCatalogValue : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string CatalogKey { get; set; } = string.Empty;
    public string ProposedKey { get; set; } = string.Empty;
    public string ProposedLabel { get; set; } = string.Empty;
    public string ProposedByPersonId { get; set; } = string.Empty;
    public string SourceEntityType { get; set; } = string.Empty;
    public string SourceEntityId { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public string? ReviewedByPersonId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ReferenceCacheEntry : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string SourceOfTruth { get; set; } = string.Empty;
    public string ReferenceKey { get; set; } = string.Empty;
    public string ExternalKey { get; set; } = string.Empty;
    public string? ExternalId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MetadataJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset LastSyncedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetCustomFieldValue : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AssetId { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string ValueJson { get; set; } = "null";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetSpec : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AssetId { get; set; }
    public string SpecKey { get; set; } = string.Empty;
    public string ValueJson { get; set; } = "null";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetComponent : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AssetId { get; set; }
    public string ComponentKey { get; set; } = string.Empty;
    public string ValueJson { get; set; } = "null";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetDocumentRef : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AssetId { get; set; }
    public string DocumentTypeKey { get; set; } = string.Empty;
    public string ExternalDocumentId { get; set; } = string.Empty;
    public string StatusKey { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetComplianceState : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AssetId { get; set; }
    public string GoverningBodyKeysJson { get; set; } = "[]";
    public string RulepackApplicabilityKeysJson { get; set; } = "[]";
    public string ComplianceCategoryKeysJson { get; set; } = "[]";
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetStatusHistory : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AssetId { get; set; }
    public string StatusFieldKey { get; set; } = string.Empty;
    public string StatusValueKey { get; set; } = string.Empty;
    public string? ChangedByPersonId { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class AssetLocationHistory : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AssetId { get; set; }
    public string? SiteId { get; set; }
    public string? HomeLocationId { get; set; }
    public string? CurrentLocationId { get; set; }
    public string? Yard { get; set; }
    public string? Bay { get; set; }
    public string? ParkingSpot { get; set; }
    public string? ChangedByPersonId { get; set; }
    public DateTimeOffset EffectiveAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class AssetAssignmentHistory : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AssetId { get; set; }
    public string AssignmentFieldKey { get; set; } = string.Empty;
    public string PersonId { get; set; } = string.Empty;
    public string? ChangedByPersonId { get; set; }
    public DateTimeOffset EffectiveAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class AssetReadinessState : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AssetId { get; set; }
    public string ReadinessStatusKey { get; set; } = "blocked";
    public string OperationalStatusKey { get; set; } = "unknown";
    public string AvailabilityStatusKey { get; set; } = "unavailable";
    public string? Basis { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetExternalMapping : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AssetId { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string ExternalEntityType { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string? ExternalKey { get; set; }
    public string? MetadataJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
