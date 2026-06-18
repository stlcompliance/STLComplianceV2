using Microsoft.EntityFrameworkCore;

namespace LedgArr.Api.Data;

public sealed class PayrollCalendar
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid LegalEntityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Frequency { get; set; } = "biweekly";
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    public DateOnly PayDate { get; set; }
    public DateOnly CutoffDate { get; set; }
    public string Timezone { get; set; } = "UTC";
    public string Status { get; set; } = "active";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PayrollCodeMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid LegalEntityId { get; set; }
    public string StaffArrPayCodeRef { get; set; } = string.Empty;
    public string? PayrollProviderRef { get; set; }
    public string ProviderEarningCode { get; set; } = string.Empty;
    public string? ProviderDeductionCode { get; set; }
    public string GlAccountRef { get; set; } = string.Empty;
    public string? CostCenterRef { get; set; }
    public string? DepartmentRef { get; set; }
    public string? TaxableTreatmentSnapshot { get; set; }
    public bool Active { get; set; } = true;
    public DateOnly EffectiveStartDate { get; set; }
    public DateOnly? EffectiveEndDate { get; set; }
}

public sealed class PayrollBatch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid LegalEntityId { get; set; }
    public Guid PayrollCalendarId { get; set; }
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    public DateOnly PayDate { get; set; }
    public string Status { get; set; } = "draft";
    public Guid? SourceStaffArrSnapshotId { get; set; }
    public string? SourceSnapshotHash { get; set; }
    public int TotalWorkers { get; set; }
    public decimal TotalHours { get; set; }
    public decimal? TotalGrossEstimate { get; set; }
    public string ExportProvider { get; set; } = "generic_csv";
    public DateTimeOffset? ExportedAt { get; set; }
    public Guid? ApprovedByPersonId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? CorrectionReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PayrollBatchLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PayrollBatchId { get; set; }
    public Guid PersonId { get; set; }
    public string WorkerNumber { get; set; } = string.Empty;
    public Guid LegalEntityId { get; set; }
    public Guid PayrollCalendarId { get; set; }
    public string PayCodeRef { get; set; } = string.Empty;
    public string ProviderEarningCode { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal? RateSnapshot { get; set; }
    public decimal? GrossEstimate { get; set; }
    public string AllocationSnapshot { get; set; } = string.Empty;
    public string SourceTimesheetPeriodRef { get; set; } = string.Empty;
    public string SourceTimeEntryRefs { get; set; } = string.Empty;
    public string ValidationStatus { get; set; } = "pending";
}

public sealed class PayrollExportPacket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PayrollBatchId { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public string ExportFormat { get; set; } = string.Empty;
    public string? FileRef { get; set; }
    public string PayloadHash { get; set; } = string.Empty;
    public Guid ExportedByPersonId { get; set; }
    public DateTimeOffset ExportedAt { get; set; } = DateTimeOffset.UtcNow;
    public string ProviderResponseStatus { get; set; } = "pending";
    public string? ProviderResponseRef { get; set; }
    public string? Errors { get; set; }
    public string ReplayProtectionKey { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PayrollJournalSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PayrollBatchId { get; set; }
    public Guid LegalEntityId { get; set; }
    public string GlAccountRef { get; set; } = string.Empty;
    public string? CostCenterRef { get; set; }
    public string? DepartmentRef { get; set; }
    public string ProductKey { get; set; } = string.Empty;
    public string CostObjectType { get; set; } = string.Empty;
    public string CostObjectRef { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string SourcePayrollBatchLineRefs { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public static class LedgArrPayrollModelConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PayrollCalendar>(entity =>
        {
            entity.HasIndex(x => new { x.TenantId, x.LegalEntityId, x.Name }).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Frequency).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Timezone).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
        });

        modelBuilder.Entity<PayrollCodeMapping>(entity =>
        {
            entity.HasIndex(x => new { x.TenantId, x.LegalEntityId, x.StaffArrPayCodeRef, x.ProviderEarningCode }).IsUnique();
            entity.Property(x => x.StaffArrPayCodeRef).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PayrollProviderRef).HasMaxLength(64);
            entity.Property(x => x.ProviderEarningCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ProviderDeductionCode).HasMaxLength(64);
            entity.Property(x => x.GlAccountRef).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CostCenterRef).HasMaxLength(128);
            entity.Property(x => x.DepartmentRef).HasMaxLength(128);
            entity.Property(x => x.TaxableTreatmentSnapshot).HasMaxLength(1024);
        });

        modelBuilder.Entity<PayrollBatch>(entity =>
        {
            entity.HasIndex(x => new { x.TenantId, x.LegalEntityId, x.PayrollCalendarId, x.PeriodStartDate, x.PeriodEndDate }).IsUnique();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceSnapshotHash).HasMaxLength(128);
            entity.Property(x => x.ExportProvider).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CorrectionReason).HasMaxLength(2048);
        });

        modelBuilder.Entity<PayrollBatchLine>(entity =>
        {
            entity.HasIndex(x => new { x.TenantId, x.PayrollBatchId, x.PersonId, x.PayCodeRef, x.SourceTimesheetPeriodRef });
            entity.Property(x => x.WorkerNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PayCodeRef).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ProviderEarningCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AllocationSnapshot).HasColumnType("text").IsRequired();
            entity.Property(x => x.SourceTimesheetPeriodRef).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SourceTimeEntryRefs).HasColumnType("text").IsRequired();
            entity.Property(x => x.ValidationStatus).HasMaxLength(32).IsRequired();
        });

        modelBuilder.Entity<PayrollExportPacket>(entity =>
        {
            entity.HasIndex(x => new { x.TenantId, x.PayrollBatchId, x.PayloadHash }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ReplayProtectionKey }).IsUnique();
            entity.Property(x => x.ProviderKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExportFormat).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FileRef).HasMaxLength(512);
            entity.Property(x => x.PayloadHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ProviderResponseStatus).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ProviderResponseRef).HasMaxLength(256);
            entity.Property(x => x.Errors).HasColumnType("text");
            entity.Property(x => x.ReplayProtectionKey).HasMaxLength(128).IsRequired();
        });

        modelBuilder.Entity<PayrollJournalSnapshot>(entity =>
        {
            entity.HasIndex(x => new { x.TenantId, x.PayrollBatchId });
            entity.Property(x => x.GlAccountRef).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CostCenterRef).HasMaxLength(128);
            entity.Property(x => x.DepartmentRef).HasMaxLength(128);
            entity.Property(x => x.ProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CostObjectType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CostObjectRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.SourcePayrollBatchLineRefs).HasColumnType("text").IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
        });
    }
}
