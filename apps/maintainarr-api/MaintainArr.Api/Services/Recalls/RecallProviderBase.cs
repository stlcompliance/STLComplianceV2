using MaintainArr.Api.Contracts;

namespace MaintainArr.Api.Services.Recalls;

public abstract class RecallProviderBase
{
    public abstract string ProviderKey { get; }

    public abstract string DisplayName { get; }

    public abstract string SourceOfTruth { get; }

    public abstract bool Enabled { get; }

    public abstract Task<RecallProviderHealthResponse> HealthCheckAsync(CancellationToken cancellationToken = default);

    public abstract Task<IReadOnlyList<RecallCampaignSeed>> FindCampaignsByVehicleAsync(
        Guid tenantId,
        int modelYear,
        string make,
        string model,
        CancellationToken cancellationToken = default);

    public abstract Task<RecallCampaignSeed?> FindCampaignByCampaignNumberAsync(
        Guid tenantId,
        string campaignNumber,
        CancellationToken cancellationToken = default);
}

public sealed record RecallCampaignSeed(
    string SourceProvider,
    string SourceType,
    string? SourceProviderRecordId,
    string? NhtsaCampaignNumber,
    string? NhtsaActionNumber,
    string? ManufacturerCampaignNumber,
    string? CampaignTitle,
    string Manufacturer,
    string Component,
    string? ReportReceivedDate,
    string? CampaignStartDate,
    string? CampaignEndDate,
    string CampaignStatus,
    int? PotentialUnitsAffected,
    string Summary,
    string Consequence,
    string Remedy,
    string Notes,
    bool ParkIt,
    bool ParkOutside,
    bool OverTheAirUpdate,
    string RecallType,
    string? SourceUrl,
    DateTimeOffset? FetchedAt,
    string? SourceRawJson,
    IReadOnlyList<RecallCampaignApplicabilityRequest> Applicability);

public sealed record RecallProviderCaseSeed(
    string MatchBasis,
    string MatchConfidence,
    decimal? MatchScore,
    string Status,
    string ReadinessImpact,
    string Reason,
    string VerificationStatus,
    string? VerificationSource,
    string? VerificationMethod,
    string? ProviderRawJson,
    DateTimeOffset? ExpiresAt);
