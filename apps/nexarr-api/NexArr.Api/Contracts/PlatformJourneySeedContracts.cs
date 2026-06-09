namespace NexArr.Api.Contracts;

public sealed record JourneySeedTargetResponse(
    string ProductKey,
    string DisplayName,
    string Description,
    string SeedPath,
    string? BaseUrl,
    bool IsConfigured);

public sealed record JourneySeedResultResponse(
    string ProductKey,
    string DisplayName,
    string Description,
    string SeedPath,
    string? BaseUrl,
    bool IsConfigured,
    bool Succeeded,
    int StatusCode,
    string? ResponseBody,
    DateTimeOffset RequestedAt);
