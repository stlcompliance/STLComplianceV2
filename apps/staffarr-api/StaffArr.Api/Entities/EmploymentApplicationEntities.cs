using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class EmploymentApplicationTemplate : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string TemplateKey { get; set; } = string.Empty;

    public string TemplateName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string SubmitLabel { get; set; } = "Submit application";

    public int Version { get; set; } = 1;

    public string Status { get; set; } = EmploymentApplicationTemplateStatuses.Draft;

    public string PublicToken { get; set; } = string.Empty;

    public DateTimeOffset? PublicLinkExpiresAt { get; set; }

    public string TemplateJson { get; set; } = string.Empty;

    public string? CreatedByPersonId { get; set; }

    public string? UpdatedByPersonId { get; set; }

    public string? PublishedByPersonId { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public string? RetiredByPersonId { get; set; }

    public DateTimeOffset? RetiredAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public static class EmploymentApplicationTemplateStatuses
{
    public const string Draft = "draft";
    public const string Published = "published";
    public const string Retired = "retired";
}

public sealed class EmploymentApplicationSubmission : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid EmploymentApplicationTemplateId { get; set; }

    public string TemplateKey { get; set; } = string.Empty;

    public int TemplateVersion { get; set; }

    public Guid? CreatedPersonId { get; set; }

    public string Status { get; set; } = "submitted";

    public string ApplicantDisplayName { get; set; } = string.Empty;

    public string ApplicantEmail { get; set; } = string.Empty;

    public string RawAnswersJson { get; set; } = string.Empty;

    public string CreateRequestJson { get; set; } = string.Empty;

    public string EventualProfileJson { get; set; } = string.Empty;

    public string? SourceIpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? ReviewerNotes { get; set; }

    public DateTimeOffset SubmittedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public EmploymentApplicationTemplate? Template { get; set; }
}
