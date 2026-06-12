namespace StaffArr.Api.Contracts;

public sealed record StaffArrFieldOptionResponse(
    string Value,
    string Label,
    string? Hint,
    string Owner,
    string SourceOfTruth);

public sealed record StaffArrFieldDefinitionResponse(
    string Key,
    string Label,
    string Control,
    bool Required,
    string Owner,
    string SourceOfTruth,
    IReadOnlyList<StaffArrFieldOptionResponse> Options);

public sealed record StaffArrFieldsetResponse(
    string Key,
    string Label,
    string EntityType,
    string Purpose,
    IReadOnlyList<StaffArrFieldDefinitionResponse> Fields);
