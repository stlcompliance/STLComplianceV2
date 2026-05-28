namespace SupplyArr.Api.Contracts;

public sealed record ForgivingSearchResultItemResponse(
    string EntityType,
    Guid EntityId,
    string PrimaryKey,
    string Title,
    string Subtitle,
    string DeepLinkPath,
    int MatchScore);

public sealed record ForgivingSearchResponse(
    string Query,
    string NormalizedQuery,
    int TotalCount,
    IReadOnlyList<ForgivingSearchResultItemResponse> Results);
