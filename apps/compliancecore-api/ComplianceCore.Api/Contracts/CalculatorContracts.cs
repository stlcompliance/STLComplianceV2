namespace ComplianceCore.Api.Contracts;

public sealed record Title49CalculatorSummaryResponse(
    Guid TenantId,
    int TotalRequirements,
    int NumericThresholdCount,
    int RetentionDurationCount,
    int MixedCalculatorCount,
    int ReadyCount,
    int ReviewCount,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<Title49CalculatorItemResponse> Requirements);

public sealed record Title49CalculatorItemResponse(
    Guid FactRequirementId,
    string RequirementKey,
    string FactKey,
    string? PackKey,
    string? CitationKey,
    string SourceProduct,
    string SourceEntity,
    string ValueType,
    string Operator,
    string ExpectedValue,
    string RetentionPeriod,
    string CalculatorKind,
    decimal? ParsedNumericThreshold,
    int? ParsedRetentionDays,
    bool IsReady,
    DateTimeOffset UpdatedAt);

