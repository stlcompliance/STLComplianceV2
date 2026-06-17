namespace STLCompliance.Shared.Integration;

public sealed record CrossProductReference(
    string OwnerProductKey,
    string ReferenceType,
    string ReferenceId,
    string DisplayLabelSnapshot,
    string? SecondaryLabelSnapshot = null,
    string? StatusSnapshot = null,
    string? OwnerVersion = null,
    string CreatedVia = "selected");

public sealed record ReferenceTypeDescriptor(
    string OwnerProductKey,
    string ReferenceType,
    string Label,
    bool CanSearch = true,
    bool CanQuickCreate = false,
    string? QuickCreatePermission = null,
    string? Description = null);

public sealed record ReferenceSearchRequest(
    string ReferenceType,
    string? Query = null,
    int Limit = 25,
    IReadOnlyDictionary<string, string>? Filters = null);

public sealed record ReferenceSearchResponse(
    IReadOnlyList<ReferenceSummaryResponse> Results);

public sealed record ReferenceSummaryResponse(
    string OwnerProductKey,
    string ReferenceType,
    string ReferenceId,
    string DisplayLabel,
    string? SecondaryLabel = null,
    string? Status = null,
    string? OwnerVersion = null,
    string? DetailPath = null,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record QuickCreateOptionDescriptor(
    string Value,
    string Label);

public sealed record QuickCreateFieldDescriptor(
    string Key,
    string Label,
    string FieldType,
    bool Required = false,
    string? Placeholder = null,
    string? DefaultValue = null,
    IReadOnlyList<QuickCreateOptionDescriptor>? Options = null);

public sealed record QuickCreateSchemaResponse(
    string OwnerProductKey,
    string ReferenceType,
    bool Allowed,
    string ManagedByLabel,
    string? PermissionKey = null,
    string? DisabledReason = null,
    IReadOnlyList<QuickCreateFieldDescriptor>? Fields = null);

public sealed record QuickCreateRequest(
    string ReferenceType,
    IReadOnlyDictionary<string, string> Values,
    string? DuplicateDisposition = null);

public sealed record DuplicateCandidateResponse(
    string ReferenceId,
    string DisplayLabel,
    string? SecondaryLabel = null,
    string? Status = null,
    string MatchReason = "",
    decimal Confidence = 0);

public sealed record QuickCreateResponse(
    CrossProductReference? Reference,
    IReadOnlyList<DuplicateCandidateResponse> DuplicateCandidates,
    bool Created,
    string? ReviewStatus = null,
    string? Message = null);

public static class CrossProductReferenceExtensions
{
    public static CrossProductReference ToCrossProductReference(
        this ReferenceSummaryResponse summary,
        string createdVia = "selected") =>
        new(
            summary.OwnerProductKey,
            summary.ReferenceType,
            summary.ReferenceId,
            summary.DisplayLabel,
            summary.SecondaryLabel,
            summary.Status,
            summary.OwnerVersion,
            createdVia);
}
