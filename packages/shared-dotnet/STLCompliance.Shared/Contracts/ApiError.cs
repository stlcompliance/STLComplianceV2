namespace STLCompliance.Shared.Contracts;

public sealed record ApiError(
    Guid ErrorId,
    string Code,
    string Message,
    object? Details,
    Guid CorrelationId);
