using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Entities;

public sealed class TimekeepingProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public string WorkerNumber { get; set; } = string.Empty;
    public string? DefaultLegalEntityRef { get; set; }
    public string? DefaultSiteRef { get; set; }
    public string? DefaultDepartmentRef { get; set; }
    public string? DefaultPositionRef { get; set; }
    public Guid? DefaultSupervisorPersonId { get; set; }
    public Guid? PayPolicyId { get; set; }
    public string PayrollEligibilityStatus { get; set; } = "eligible";
    public string TimeEntryMode { get; set; } = "hybrid";
    public bool OvertimeEligible { get; set; } = true;
    public bool RequiresMealBreakAttestation { get; set; }
    public bool RequiresEndOfShiftAttestation { get; set; }
    public bool AllowMobileClock { get; set; } = true;
    public bool AllowKioskClock { get; set; } = true;
    public bool AllowManualCorrections { get; set; } = true;
    public Guid? DefaultLaborAllocationTemplateId { get; set; }
    public DateOnly EffectiveStartDate { get; set; }
    public DateOnly? EffectiveEndDate { get; set; }
    public string Status { get; set; } = "active";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PayPolicy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string JurisdictionRefs { get; set; } = string.Empty;
    public string? RoundingPolicy { get; set; }
    public string? MealBreakPolicy { get; set; }
    public string? RestBreakPolicy { get; set; }
    public string? OvertimePolicy { get; set; }
    public string? DoubleTimePolicy { get; set; }
    public string? HolidayPolicy { get; set; }
    public string? ShiftDifferentialPolicy { get; set; }
    public string? TravelTimePolicy { get; set; }
    public string? StandbyCalloutPolicy { get; set; }
    public string? ApprovalPolicy { get; set; }
    public string? CorrectionPolicy { get; set; }
    public string? AttestationPolicy { get; set; }
    public string? ComplianceRulepackRefs { get; set; }
    public DateOnly EffectiveStartDate { get; set; }
    public DateOnly? EffectiveEndDate { get; set; }
    public string Status { get; set; } = "active";
}

public sealed class PayCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = "worked";
    public bool CountsTowardWorkedHours { get; set; } = true;
    public bool CountsTowardOvertimeBase { get; set; } = true;
    public bool RequiresAllocation { get; set; }
    public bool RequiresApproval { get; set; } = true;
    public bool RequiresReason { get; set; }
    public bool Active { get; set; } = true;
    public DateOnly EffectiveStartDate { get; set; }
    public DateOnly? EffectiveEndDate { get; set; }
}

public sealed class ClockEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public string SourceProductKey { get; set; } = string.Empty;
    public string SourceDeviceType { get; set; } = string.Empty;
    public string? SourceDeviceId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTimeOffset EventTimestamp { get; set; }
    public DateTimeOffset CapturedTimestamp { get; set; } = DateTimeOffset.UtcNow;
    public string Timezone { get; set; } = "UTC";
    public string? GeoPoint { get; set; }
    public string? SiteRef { get; set; }
    public string? LocationRef { get; set; }
    public string? SourceRef { get; set; }
    public Guid? EnteredByPersonId { get; set; }
    public string? Notes { get; set; }
    public string AnomalyFlagsCsv { get; set; } = string.Empty;
    public string ImmutableAuditHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class WorkSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public DateOnly SessionDate { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public string Timezone { get; set; } = "UTC";
    public string Status { get; set; } = "draft";
    public string SourceType { get; set; } = "manual";
    public string PrimarySourceProductKey { get; set; } = "staffarr";
    public string? PrimarySourceRef { get; set; }
    public string? SiteRef { get; set; }
    public string? LocationRef { get; set; }
    public Guid? SupervisorPersonId { get; set; }
    public string AnomalyFlagsCsv { get; set; } = string.Empty;
    public int CalculatedDurationMinutes { get; set; }
    public int PaidDurationMinutes { get; set; }
    public int UnpaidBreakMinutes { get; set; }
    public bool RequiresReview { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class LeaveRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string Timezone { get; set; } = "UTC";
    public bool IsIntermittent { get; set; }
    public bool IsPaid { get; set; } = true;
    public string Status { get; set; } = "requested";
    public Guid? RequestedByPersonId { get; set; }
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid? ApprovedByPersonId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public string? Reason { get; set; }
    public string PayrollLockStatus { get; set; } = "unlocked";
    public string? SourceProductKey { get; set; }
    public string? SourceRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class AttendanceEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = "low";
    public int PointValue { get; set; }
    public string Status { get; set; } = "open";
    public string? Notes { get; set; }
    public string SourceProductKey { get; set; } = "staffarr";
    public string? SourceRef { get; set; }
    public Guid? RelatedLeaveRequestId { get; set; }
    public Guid? RelatedTimesheetPeriodId { get; set; }
    public Guid? ReviewedByPersonId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class AvailabilityBlock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public string AvailabilityType { get; set; } = string.Empty;
    public string DayOfWeekMaskCsv { get; set; } = string.Empty;
    public TimeOnly StartLocalTime { get; set; }
    public TimeOnly EndLocalTime { get; set; }
    public string Timezone { get; set; } = "UTC";
    public DateOnly EffectiveStartDate { get; set; }
    public DateOnly? EffectiveEndDate { get; set; }
    public string Status { get; set; } = "active";
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class TimesheetPeriod
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public string PayrollCalendarRef { get; set; } = string.Empty;
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    public string Status { get; set; } = "open";
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? ApprovedByPersonId { get; set; }
    public DateTimeOffset? PayrollReadyAt { get; set; }
    public DateTimeOffset? ExportedAt { get; set; }
    public int TotalWorkedMinutes { get; set; }
    public int TotalPaidMinutes { get; set; }
    public int TotalUnpaidMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public int ExceptionCount { get; set; }
    public string AttestationStatus { get; set; } = "pending";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class TimeEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public Guid? WorkSessionId { get; set; }
    public Guid TimesheetPeriodId { get; set; }
    public DateOnly EntryDate { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public Guid PayCodeId { get; set; }
    public Guid? PayPolicyId { get; set; }
    public string Classification { get; set; } = "regular";
    public string SourceProductKey { get; set; } = "staffarr";
    public string? SourceRef { get; set; }
    public string SourceConfidence { get; set; } = "manual";
    public string? Description { get; set; }
    public bool RequiresApproval { get; set; } = true;
    public string ApprovalStatus { get; set; } = "pending";
    public string PayrollLockStatus { get; set; } = "unlocked";
    public Guid? CreatedByPersonId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class LaborAllocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid TimeEntryId { get; set; }
    public decimal AllocationPercent { get; set; }
    public int AllocationMinutes { get; set; }
    public string ProductKey { get; set; } = string.Empty;
    public string CostObjectType { get; set; } = string.Empty;
    public string CostObjectRef { get; set; } = string.Empty;
    public string LegalEntityRef { get; set; } = string.Empty;
    public string SiteRef { get; set; } = string.Empty;
    public string DepartmentRef { get; set; } = string.Empty;
    public string? CustomerRef { get; set; }
    public string? OrderRef { get; set; }
    public string? AssetRef { get; set; }
    public string? WorkOrderRef { get; set; }
    public string? TripRef { get; set; }
    public string? RouteRef { get; set; }
    public string? WarehouseTaskRef { get; set; }
    public string? TrainingSessionRef { get; set; }
    public string? QualityCaseRef { get; set; }
    public string? ProjectRef { get; set; }
    public string? GlDimensionSnapshot { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class TimeException
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public Guid TimesheetPeriodId { get; set; }
    public Guid? WorkSessionId { get; set; }
    public Guid? TimeEntryId { get; set; }
    public string Severity { get; set; } = "warning";
    public string ExceptionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string SourceProductKey { get; set; } = "staffarr";
    public string? SourceRef { get; set; }
    public string ResolutionStatus { get; set; } = "open";
    public Guid? ResolvedByPersonId { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class TimeCorrection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public Guid RequestedByPersonId { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public string ReasonText { get; set; } = string.Empty;
    public string OldSnapshot { get; set; } = string.Empty;
    public string NewSnapshot { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = "pending";
    public Guid? ApprovedByPersonId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class TimeAttestation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public Guid TimesheetPeriodId { get; set; }
    public string AttestationType { get; set; } = string.Empty;
    public string StatementText { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public DateTimeOffset AttestedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid AttestedByPersonId { get; set; }
    public string SourceDeviceType { get; set; } = string.Empty;
    public string SourceProductKey { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class LaborEvidenceInboxItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string SourceProductKey { get; set; } = string.Empty;
    public string SourceEntityType { get; set; } = string.Empty;
    public string SourceEntityId { get; set; } = string.Empty;
    public Guid PersonId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string? SuggestedPayCodeKey { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public int? DurationMinutes { get; set; }
    public string Timezone { get; set; } = "UTC";
    public string? SiteRef { get; set; }
    public string? LocationRef { get; set; }
    public string? LegalEntityRef { get; set; }
    public string CostObjectRefsJson { get; set; } = "[]";
    public string Confidence { get; set; } = "estimated";
    public string? Notes { get; set; }
    public DateTimeOffset EmittedAt { get; set; }
    public string Status { get; set; } = "received";
    public Guid? TimeEntryId { get; set; }
    public Guid? TimesheetPeriodId { get; set; }
    public bool ConflictDetected { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public static class StaffArrTimekeepingModelConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TimekeepingProfile>(entity =>
        {
            entity.ToTable("staffarr_timekeeping_profiles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.WorkerNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DefaultLegalEntityRef).HasMaxLength(256);
            entity.Property(x => x.DefaultSiteRef).HasMaxLength(256);
            entity.Property(x => x.DefaultDepartmentRef).HasMaxLength(256);
            entity.Property(x => x.DefaultPositionRef).HasMaxLength(256);
            entity.Property(x => x.PayrollEligibilityStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.TimeEntryMode).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.PersonId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.WorkerNumber }).IsUnique();
        });

        modelBuilder.Entity<PayPolicy>(entity =>
        {
            entity.ToTable("staffarr_pay_policies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024);
            entity.Property(x => x.JurisdictionRefs).HasColumnType("text").IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Name });
        });

        modelBuilder.Entity<PayCode>(entity =>
        {
            entity.ToTable("staffarr_pay_codes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        });

        modelBuilder.Entity<ClockEvent>(entity =>
        {
            entity.ToTable("staffarr_clock_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceDeviceType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceDeviceId).HasMaxLength(128);
            entity.Property(x => x.EventType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Timezone).HasMaxLength(64).IsRequired();
            entity.Property(x => x.GeoPoint).HasMaxLength(128);
            entity.Property(x => x.SiteRef).HasMaxLength(256);
            entity.Property(x => x.LocationRef).HasMaxLength(256);
            entity.Property(x => x.SourceRef).HasMaxLength(256);
            entity.Property(x => x.Notes).HasMaxLength(2048);
            entity.Property(x => x.AnomalyFlagsCsv).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ImmutableAuditHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.EventTimestamp });
        });

        modelBuilder.Entity<WorkSession>(entity =>
        {
            entity.ToTable("staffarr_work_sessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Timezone).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.PrimarySourceProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PrimarySourceRef).HasMaxLength(256);
            entity.Property(x => x.SiteRef).HasMaxLength(256);
            entity.Property(x => x.LocationRef).HasMaxLength(256);
            entity.Property(x => x.AnomalyFlagsCsv).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.SessionDate });
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.ToTable("staffarr_leave_requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LeaveType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Timezone).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ReviewNotes).HasMaxLength(2048);
            entity.Property(x => x.Reason).HasMaxLength(2048);
            entity.Property(x => x.PayrollLockStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProductKey).HasMaxLength(64);
            entity.Property(x => x.SourceRef).HasMaxLength(256);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.StartDate, x.EndDate });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<AttendanceEvent>(entity =>
        {
            entity.ToTable("staffarr_attendance_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Severity).HasMaxLength(16).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(2048);
            entity.Property(x => x.SourceProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceRef).HasMaxLength(256);
            entity.Property(x => x.ResolutionNotes).HasMaxLength(2048);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.OccurredAt });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.EventType, x.Status });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
            entity.HasOne<LeaveRequest>().WithMany().HasForeignKey(x => x.RelatedLeaveRequestId);
            entity.HasOne<TimesheetPeriod>().WithMany().HasForeignKey(x => x.RelatedTimesheetPeriodId);
        });

        modelBuilder.Entity<AvailabilityBlock>(entity =>
        {
            entity.ToTable("staffarr_availability_blocks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AvailabilityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DayOfWeekMaskCsv).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Timezone).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(2048);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.EffectiveStartDate, x.EffectiveEndDate });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<TimesheetPeriod>(entity =>
        {
            entity.ToTable("staffarr_timesheet_periods");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PayrollCalendarRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.AttestationStatus).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.PeriodStartDate, x.PeriodEndDate }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.PayrollReadyAt });
        });

        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.ToTable("staffarr_time_entries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Classification).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceRef).HasMaxLength(256);
            entity.Property(x => x.SourceConfidence).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2048);
            entity.Property(x => x.ApprovalStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.PayrollLockStatus).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.EntryDate });
            entity.HasIndex(x => new { x.TenantId, x.TimesheetPeriodId });
        });

        modelBuilder.Entity<LaborAllocation>(entity =>
        {
            entity.ToTable("staffarr_labor_allocations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AllocationPercent).HasPrecision(5, 2);
            entity.Property(x => x.ProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CostObjectType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CostObjectRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.LegalEntityRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.SiteRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DepartmentRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CustomerRef).HasMaxLength(256);
            entity.Property(x => x.OrderRef).HasMaxLength(256);
            entity.Property(x => x.AssetRef).HasMaxLength(256);
            entity.Property(x => x.WorkOrderRef).HasMaxLength(256);
            entity.Property(x => x.TripRef).HasMaxLength(256);
            entity.Property(x => x.RouteRef).HasMaxLength(256);
            entity.Property(x => x.WarehouseTaskRef).HasMaxLength(256);
            entity.Property(x => x.TrainingSessionRef).HasMaxLength(256);
            entity.Property(x => x.QualityCaseRef).HasMaxLength(256);
            entity.Property(x => x.ProjectRef).HasMaxLength(256);
            entity.Property(x => x.GlDimensionSnapshot).HasMaxLength(2048);
            entity.HasIndex(x => new { x.TenantId, x.TimeEntryId });
        });

        modelBuilder.Entity<TimeException>(entity =>
        {
            entity.ToTable("staffarr_time_exceptions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Severity).HasMaxLength(16).IsRequired();
            entity.Property(x => x.ExceptionType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.SourceProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceRef).HasMaxLength(256);
            entity.Property(x => x.ResolutionStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ResolutionNotes).HasMaxLength(2048);
            entity.HasIndex(x => new { x.TenantId, x.TimesheetPeriodId, x.ResolutionStatus });
        });

        modelBuilder.Entity<TimeCorrection>(entity =>
        {
            entity.ToTable("staffarr_time_corrections");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TargetType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ReasonText).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.OldSnapshot).HasColumnType("text").IsRequired();
            entity.Property(x => x.NewSnapshot).HasColumnType("text").IsRequired();
            entity.Property(x => x.ApprovalStatus).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.CreatedAt });
        });

        modelBuilder.Entity<TimeAttestation>(entity =>
        {
            entity.ToTable("staffarr_time_attestations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AttestationType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.StatementText).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.Response).HasMaxLength(512).IsRequired();
            entity.Property(x => x.SourceDeviceType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceProductKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.TimesheetPeriodId, x.AttestationType });
        });

        modelBuilder.Entity<LaborEvidenceInboxItem>(entity =>
        {
            entity.ToTable("staffarr_labor_evidence_inbox");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.SourceProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceEntityId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ActivityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SuggestedPayCodeKey).HasMaxLength(64);
            entity.Property(x => x.Timezone).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SiteRef).HasMaxLength(256);
            entity.Property(x => x.LocationRef).HasMaxLength(256);
            entity.Property(x => x.LegalEntityRef).HasMaxLength(256);
            entity.Property(x => x.CostObjectRefsJson).HasColumnType("text").IsRequired();
            entity.Property(x => x.Confidence).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(2048);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.EmittedAt });
        });
    }
}
