namespace ComplianceCore.Api.Contracts;

public sealed record ComplianceKeyResponse(
    Guid ComplianceKeyId,
    string Key,
    string Label,
    string Category,
    string Description,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record CreateComplianceKeyRequest(
    string Key,
    string Label,
    string Category,
    string Description);

public sealed record MaterialKeyResponse(
    Guid MaterialKeyId,
    string Key,
    string Label,
    string Category,
    string Description,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record CreateMaterialKeyRequest(
    string Key,
    string Label,
    string Category,
    string Description);
