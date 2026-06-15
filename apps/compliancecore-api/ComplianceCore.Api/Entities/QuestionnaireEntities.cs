using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class QuestionnaireRun : IHasTenant
{
    public Guid QuestionnaireRunId { get; set; }

    public Guid TenantId { get; set; }

    public string ProductKey { get; set; } = string.Empty;

    public string WorkflowKey { get; set; } = string.Empty;

    public string SubjectType { get; set; } = string.Empty;

    public string SubjectId { get; set; } = string.Empty;

    public string SourceRecordId { get; set; } = string.Empty;

    public string SourceEntity { get; set; } = string.Empty;

    public string SourceRecordContextJson { get; set; } = "{}";

    public string KnownFactsJson { get; set; } = "{}";

    public string TemplateKey { get; set; } = string.Empty;

    public string Status { get; set; } = QuestionnaireRunStatuses.Draft;

    public string SummaryJson { get; set; } = "{}";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    public DateTimeOffset? SubmittedAt { get; set; }
}

public sealed class QuestionnaireAnswer : IHasTenant
{
    public Guid QuestionnaireAnswerId { get; set; }

    public Guid TenantId { get; set; }

    public Guid QuestionnaireRunId { get; set; }

    public string QuestionKey { get; set; } = string.Empty;

    public string QuestionLabel { get; set; } = string.Empty;

    public string SectionKey { get; set; } = string.Empty;

    public string SectionLabel { get; set; } = string.Empty;

    public string AnswerKind { get; set; } = string.Empty;

    public string SelectedOptionKey { get; set; } = string.Empty;

    public string AnswerText { get; set; } = string.Empty;

    public string DocumentUrl { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string FileHash { get; set; } = string.Empty;

    public string NormalizedFactKey { get; set; } = string.Empty;

    public string NormalizedFactValue { get; set; } = string.Empty;

    public string NormalizedFactValueType { get; set; } = FactValueTypes.String;

    public string SourceProduct { get; set; } = string.Empty;

    public string WorkflowKey { get; set; } = string.Empty;

    public string SubjectType { get; set; } = string.Empty;

    public string SubjectId { get; set; } = string.Empty;

    public string SourceRecordId { get; set; } = string.Empty;

    public string ReviewStatus { get; set; } = QuestionnaireReviewStatuses.Confirmed;

    public decimal Confidence { get; set; } = 1m;

    public DateTimeOffset EffectiveAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid? EvidenceReferenceId { get; set; }

    public string? EvidenceId { get; set; }

    public string SourceContextJson { get; set; } = "{}";

    public QuestionnaireRun? Run { get; set; }

    public EvidenceReference? EvidenceReference { get; set; }
}

public static class QuestionnaireRunStatuses
{
    public const string Draft = "draft";
    public const string Resolved = "resolved";
    public const string Submitted = "submitted";
    public const string NeedsReview = "needs_review";
    public const string Blocked = "blocked";
}

public static class QuestionnaireReviewStatuses
{
    public const string Confirmed = "confirmed";
    public const string Unknown = "unknown";
    public const string Conflict = "conflict";
    public const string Deferred = "deferred";
    public const string EvidencePending = "evidence_pending";
}
