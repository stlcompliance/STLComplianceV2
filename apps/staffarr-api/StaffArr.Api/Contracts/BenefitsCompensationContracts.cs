namespace StaffArr.Api.Contracts;

public sealed record BenefitEnrollmentResponse(
    Guid Id,
    Guid PersonId,
    string BenefitType,
    string PlanName,
    string BenefitClass,
    string CoverageLevel,
    string EligibilityStatus,
    string EnrollmentStatus,
    string CarrierExportStatus,
    string? CarrierMemberId,
    string? CarrierGroupId,
    DateOnly EffectiveStartDate,
    DateOnly? EffectiveEndDate,
    DateOnly? OpenEnrollmentYear,
    string? SourceProductKey,
    string? SourceRef,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpsertBenefitEnrollmentRequest(
    Guid PersonId,
    string BenefitType,
    string PlanName,
    string BenefitClass,
    string CoverageLevel,
    string EligibilityStatus,
    string EnrollmentStatus,
    string CarrierExportStatus,
    string? CarrierMemberId,
    string? CarrierGroupId,
    DateOnly EffectiveStartDate,
    DateOnly? EffectiveEndDate,
    DateOnly? OpenEnrollmentYear,
    string? SourceProductKey,
    string? SourceRef);

public sealed record BenefitDependentResponse(
    Guid Id,
    Guid PersonId,
    string FirstName,
    string LastName,
    string Relationship,
    DateOnly? DateOfBirth,
    bool IsStudent,
    bool IsDisabled,
    string CoverageStatus,
    DateOnly? CoverageStartDate,
    DateOnly? CoverageEndDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpsertBenefitDependentRequest(
    Guid PersonId,
    string FirstName,
    string LastName,
    string Relationship,
    DateOnly? DateOfBirth,
    bool IsStudent,
    bool IsDisabled,
    string CoverageStatus,
    DateOnly? CoverageStartDate,
    DateOnly? CoverageEndDate);

public sealed record BenefitBeneficiaryResponse(
    Guid Id,
    Guid PersonId,
    string FirstName,
    string LastName,
    string Relationship,
    decimal AllocationPercent,
    string? DesignationType,
    string Status,
    DateOnly? EffectiveStartDate,
    DateOnly? EffectiveEndDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpsertBenefitBeneficiaryRequest(
    Guid PersonId,
    string FirstName,
    string LastName,
    string Relationship,
    decimal AllocationPercent,
    string? DesignationType,
    string Status,
    DateOnly? EffectiveStartDate,
    DateOnly? EffectiveEndDate);

public sealed record CompensationProfileResponse(
    Guid Id,
    Guid PersonId,
    string PayBasis,
    string PayGrade,
    string PayBand,
    string? StepProgression,
    decimal? BaseRate,
    decimal? AnnualSalary,
    string CurrencyCode,
    bool OvertimeEligible,
    bool ShiftDifferentialEligible,
    bool BonusEligible,
    bool AllowanceEligible,
    string Status,
    DateOnly EffectiveStartDate,
    DateOnly? EffectiveEndDate,
    string? SourceProductKey,
    string? SourceRef,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpsertCompensationProfileRequest(
    Guid PersonId,
    string PayBasis,
    string PayGrade,
    string PayBand,
    string? StepProgression,
    decimal? BaseRate,
    decimal? AnnualSalary,
    string CurrencyCode,
    bool OvertimeEligible,
    bool ShiftDifferentialEligible,
    bool BonusEligible,
    bool AllowanceEligible,
    string Status,
    DateOnly EffectiveStartDate,
    DateOnly? EffectiveEndDate,
    string? SourceProductKey,
    string? SourceRef);

public sealed record CompensationChangeRequestResponse(
    Guid Id,
    Guid PersonId,
    string RequestType,
    string Status,
    string ReasonCode,
    string ReasonText,
    string OldSnapshot,
    string NewSnapshot,
    Guid? RequestedByPersonId,
    Guid? ApprovedByPersonId,
    DateTimeOffset? ReviewedAt,
    DateOnly? EffectiveDate,
    string? SourceProductKey,
    string? SourceRef,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateCompensationChangeRequestRequest(
    Guid PersonId,
    string RequestType,
    string Status,
    string ReasonCode,
    string ReasonText,
    string OldSnapshot,
    string NewSnapshot,
    Guid? RequestedByPersonId,
    DateOnly? EffectiveDate,
    string? SourceProductKey,
    string? SourceRef);

public sealed record ReviewCompensationChangeRequest(string Status, string? ReviewedByNotes = null);
