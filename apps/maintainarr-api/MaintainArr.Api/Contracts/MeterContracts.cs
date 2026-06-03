namespace MaintainArr.Api.Contracts;

public sealed record AssetMeterResponse(
    Guid AssetMeterId,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string MeterKey,
    string Name,
    string Description,
    string Unit,
    decimal BaselineReading,
    decimal CurrentReading,
    DateTimeOffset? LastReadingAt,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateAssetMeterRequest(
    string MeterKey,
    string Name,
    string Description,
    string Unit,
    decimal BaselineReading);

public sealed record MeterReadingResponse(
    Guid MeterReadingId,
    Guid AssetMeterId,
    Guid AssetId,
    decimal ReadingValue,
    decimal DeltaFromPrevious,
    DateTimeOffset ReadAt,
    Guid RecordedByUserId,
    string Notes,
    bool IsCorrection,
    DateTimeOffset CreatedAt);

public sealed record RecordMeterReadingRequest(
    decimal ReadingValue,
    DateTimeOffset? ReadAt,
    string Notes,
    bool IsCorrection);

public sealed record CorrectMeterReadingRequest(
    decimal ReadingValue,
    DateTimeOffset? ReadAt,
    string Reason);

public sealed record MeterPmForecastItem(
    Guid PmScheduleId,
    string ScheduleKey,
    string Name,
    string ScheduleMode,
    string DueStatus,
    decimal? NextDueAtUsage,
    decimal? IntervalUsage,
    decimal CurrentMeterReading,
    decimal? UsageUntilDue,
    bool IsDueFromUsage);

public sealed record MeterPmForecastResponse(
    Guid AssetMeterId,
    string MeterKey,
    string Unit,
    decimal CurrentReading,
    decimal? UsageVelocityPerDay,
    decimal? PredictedUsageUntilDue,
    decimal? PredictedDaysUntilDue,
    DateTimeOffset? PredictedDueAt,
    decimal ConfidenceScore,
    bool IsDueSoon,
    IReadOnlyList<MeterPmForecastItem> LinkedSchedules);

public sealed record MeterMissingReadingAlertResponse(
    Guid AssetMeterId,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string MeterKey,
    string MeterName,
    DateTimeOffset? LastReadingAt,
    int? DaysSinceLastReading,
    string Message);
