namespace TrainArr.Api.Contracts;

public sealed record CreateTrainingApplicabilityProfileRequest(
    string Label,
    string ScopeType,
    string ScopeKey,
    string? Description,
    string? SourceProduct,
    string? SourceRecordId);

public sealed record UpdateTrainingApplicabilityProfileRequest(
    string Label,
    string? Description);

public sealed record TrainingApplicabilityProfileResponse(
    Guid ApplicabilityProfileId,
    string ProfileKey,
    string Label,
    string? Description,
    string ScopeType,
    string ScopeKey,
    string? SourceProduct,
    string? SourceRecordId,
    DateTimeOffset? SourceUpdatedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
