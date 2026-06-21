namespace StaffArr.Api.Contracts;

public sealed record EmploymentApplicationFieldOptionRequest(
    string Value,
    string Label);

public sealed record EmploymentApplicationFieldRequest(
    string FieldKey,
    string Label,
    string Control,
    bool Required,
    string MappingMode,
    string? TargetFieldKey,
    string? HelpText = null,
    string? Placeholder = null,
    IReadOnlyList<EmploymentApplicationFieldOptionRequest>? Options = null);

public sealed record EmploymentApplicationTemplateCreateRequest(
    string TemplateKey,
    string TemplateName,
    string Title,
    string Subtitle,
    string SubmitLabel,
    DateTimeOffset? PublicLinkExpiresAt,
    IReadOnlyList<EmploymentApplicationFieldRequest> Fields);

public sealed record EmploymentApplicationTemplateUpsertRequest(
    string TemplateName,
    string Title,
    string Subtitle,
    string SubmitLabel,
    DateTimeOffset? PublicLinkExpiresAt,
    IReadOnlyList<EmploymentApplicationFieldRequest> Fields);

public sealed record EmploymentApplicationTemplateFieldResponse(
    string FieldKey,
    string Label,
    string Control,
    bool Required,
    string MappingMode,
    string? TargetFieldKey,
    string? HelpText,
    string? Placeholder,
    IReadOnlyList<EmploymentApplicationFieldOptionRequest> Options);

public sealed record EmploymentApplicationTemplateResponse(
    Guid EmploymentApplicationTemplateId,
    string TemplateKey,
    string TemplateName,
    string Title,
    string Subtitle,
    string SubmitLabel,
    int Version,
    string Status,
    string PublicToken,
    DateTimeOffset? PublicLinkExpiresAt,
    IReadOnlyList<EmploymentApplicationTemplateFieldResponse> Fields,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? PublishedAt,
    DateTimeOffset? RetiredAt);

public sealed record EmploymentApplicationSubmissionResponse(
    Guid EmploymentApplicationSubmissionId,
    Guid? CreatedPersonId,
    Guid? CreatedCandidateId,
    Guid? RecruitingRequisitionId,
    string Status,
    string ApplicantDisplayName,
    string ApplicantEmail,
    string TemplateKey,
    int TemplateVersion,
    DateTimeOffset SubmittedAt,
    IReadOnlyDictionary<string, string?> CreateRequestValues,
    IReadOnlyDictionary<string, string?> EventualProfileValues);

public sealed record EmploymentApplicationSubmissionListItemResponse(
    Guid EmploymentApplicationSubmissionId,
    Guid? CreatedPersonId,
    Guid? CreatedCandidateId,
    Guid? RecruitingRequisitionId,
    string Status,
    string ApplicantDisplayName,
    string ApplicantEmail,
    string TemplateKey,
    int TemplateVersion,
    DateTimeOffset SubmittedAt);

public sealed record PublicEmploymentApplicationResponse(
    Guid EmploymentApplicationTemplateId,
    string TemplateKey,
    string TemplateName,
    string Title,
    string Subtitle,
    string SubmitLabel,
    int Version,
    IReadOnlyList<EmploymentApplicationTemplateFieldResponse> Fields,
    DateTimeOffset PublicLinkExpiresAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record SubmitEmploymentApplicationRequest(
    IReadOnlyDictionary<string, string?> Answers);
