using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Contracts;

public sealed record PersonalTrainingDashboardResponse(
    Guid StaffarrPersonId,
    DateTimeOffset GeneratedAt,
    PersonalTrainingDashboardSummary Summary,
    IReadOnlyList<TrainingAssignmentSummaryResponse> AssignedTraining,
    IReadOnlyList<QualificationIssueResponse> Qualifications,
    FieldInboxResponse FieldInbox,
    IReadOnlyList<PersonTrainingHistoryEntryItem> RecentHistory);

public sealed record PersonalTrainingDashboardSummary(
    int ActiveAssignmentCount,
    int CompletedAssignmentCount,
    int OverdueAssignmentCount,
    int QualificationCount,
    int ExpiringQualificationCount,
    int FieldInboxCount,
    int RecentHistoryCount);
