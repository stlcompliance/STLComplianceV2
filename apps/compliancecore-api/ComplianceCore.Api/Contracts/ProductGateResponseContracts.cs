namespace ComplianceCore.Api.Contracts;

public sealed record CreateProductGateResponseRequest(
    Guid TenantId,
    Guid CheckResultId,
    string ResponseOutcome,
    string? ResponseCode = null,
    string? ResponseMessage = null,
    IReadOnlyDictionary<string, string>? ResponsePayload = null);

public sealed record ProductGateResponseItemResponse(
    Guid ResponseId,
    Guid TenantId,
    Guid CheckResultId,
    string SourceProduct,
    string ResponseOutcome,
    string? ResponseCode,
    string? ResponseMessage,
    IReadOnlyDictionary<string, string> ResponsePayload,
    DateTimeOffset RespondedAt);

public sealed record ProductGateResponseListResponse(
    Guid CheckResultId,
    int Count,
    IReadOnlyList<ProductGateResponseItemResponse> Items);
