using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class RecallCampaign : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string SourceProvider { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;

    public string? SourceProviderRecordId { get; set; }

    public string? NhtsaCampaignNumber { get; set; }

    public string? NhtsaActionNumber { get; set; }

    public string? ManufacturerCampaignNumber { get; set; }

    public string? CampaignTitle { get; set; }

    public string Manufacturer { get; set; } = string.Empty;

    public string Component { get; set; } = string.Empty;

    public string? ReportReceivedDate { get; set; }

    public string? CampaignStartDate { get; set; }

    public string? CampaignEndDate { get; set; }

    public string CampaignStatus { get; set; } = "unknown";

    public int? PotentialUnitsAffected { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string Consequence { get; set; } = string.Empty;

    public string Remedy { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public bool ParkIt { get; set; }

    public bool ParkOutside { get; set; }

    public bool OverTheAirUpdate { get; set; }

    public string RecallType { get; set; } = "unknown";

    public string? SourceRawJson { get; set; }

    public string? SourceUrl { get; set; }

    public DateTimeOffset? FetchedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<RecallCampaignApplicability> Applicabilities { get; set; } = [];

    public ICollection<AssetRecallCase> AssetCases { get; set; } = [];
}

public sealed class RecallCampaignApplicability
{
    public Guid Id { get; set; }

    public Guid RecallCampaignId { get; set; }

    public int? ModelYear { get; set; }

    public string? Make { get; set; }

    public string? Model { get; set; }

    public string? AssetClass { get; set; }

    public string? AssetType { get; set; }

    public string? BodyClass { get; set; }

    public string? VehicleType { get; set; }

    public string? FuelType { get; set; }

    public string? EngineFamily { get; set; }

    public string? EngineManufacturer { get; set; }

    public string? ComponentCategory { get; set; }

    public string? TireBrand { get; set; }

    public string? TireLine { get; set; }

    public string? TireSize { get; set; }

    public string? EquipmentMake { get; set; }

    public string? EquipmentModel { get; set; }

    public string? SerialRangeStart { get; set; }

    public string? SerialRangeEnd { get; set; }

    public DateTimeOffset? ProductionStartDate { get; set; }

    public DateTimeOffset? ProductionEndDate { get; set; }

    public string? Notes { get; set; }

    public string? SourceRawJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public RecallCampaign RecallCampaign { get; set; } = null!;
}

public sealed class AssetRecallCase : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public Guid RecallCampaignId { get; set; }

    public string MatchBasis { get; set; } = "unknown";

    public string MatchConfidence { get; set; } = "low";

    public decimal? MatchScore { get; set; }

    public string Status { get; set; } = "potential_match";

    public string ReadinessImpact { get; set; } = "advisory";

    public string Reason { get; set; } = string.Empty;

    public DateTimeOffset DetectedAt { get; set; }

    public DateTimeOffset? LastRefreshedAt { get; set; }

    public string? DismissedByPersonId { get; set; }

    public DateTimeOffset? DismissedAt { get; set; }

    public string? DismissalReason { get; set; }

    public string? VerificationSource { get; set; }

    public string? VerificationMethod { get; set; }

    public string VerificationStatus { get; set; } = "unknown";

    public string? VerifiedByPersonId { get; set; }

    public DateTimeOffset? VerifiedAt { get; set; }

    public Guid? EvidenceDocumentId { get; set; }

    public string? EvidenceUrl { get; set; }

    public string? EvidenceText { get; set; }

    public string? ProviderRawJson { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public Guid? WorkOrderId { get; set; }

    public Guid? InspectionRunId { get; set; }

    public Guid? DefectId { get; set; }

    public Guid? ReadinessHoldId { get; set; }

    public string ActionType { get; set; } = "note_only";

    public string ActionStatus { get; set; } = "planned";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Asset Asset { get; set; } = null!;

    public RecallCampaign RecallCampaign { get; set; } = null!;
}

public sealed class RecallAuditLogEntry : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? AssetId { get; set; }

    public Guid? RecallCampaignId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? PreviousStatus { get; set; }

    public string? NewStatus { get; set; }

    public string? PersonId { get; set; }

    public Guid? ServiceClientId { get; set; }

    public string? DetailsJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class RecallMakeModelAlias
{
    public Guid Id { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string RawMake { get; set; } = string.Empty;

    public string RawModel { get; set; } = string.Empty;

    public string NormalizedMake { get; set; } = string.Empty;

    public string NormalizedModel { get; set; } = string.Empty;

    public double Confidence { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
