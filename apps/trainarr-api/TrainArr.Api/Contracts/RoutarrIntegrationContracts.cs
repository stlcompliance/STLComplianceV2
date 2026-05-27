namespace TrainArr.Api.Contracts;

public sealed record RoutarrQualificationCheckRequest(
    Guid TenantId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string? RulePackKey,
    IReadOnlyDictionary<string, string>? Context);
