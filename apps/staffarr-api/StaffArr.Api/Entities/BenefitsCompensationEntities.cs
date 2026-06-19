using Microsoft.EntityFrameworkCore;

namespace StaffArr.Api.Entities;

public sealed class BenefitEnrollment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public string BenefitType { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string BenefitClass { get; set; } = string.Empty;
    public string CoverageLevel { get; set; } = "employee";
    public string EligibilityStatus { get; set; } = "eligible";
    public string EnrollmentStatus { get; set; } = "pending";
    public string CarrierExportStatus { get; set; } = "not_exported";
    public string? CarrierMemberId { get; set; }
    public string? CarrierGroupId { get; set; }
    public DateOnly EffectiveStartDate { get; set; }
    public DateOnly? EffectiveEndDate { get; set; }
    public DateOnly? OpenEnrollmentYear { get; set; }
    public string? SourceProductKey { get; set; }
    public string? SourceRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class BenefitDependent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public bool IsStudent { get; set; }
    public bool IsDisabled { get; set; }
    public string CoverageStatus { get; set; } = "eligible";
    public DateOnly? CoverageStartDate { get; set; }
    public DateOnly? CoverageEndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class BenefitBeneficiary
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public decimal AllocationPercent { get; set; }
    public string? DesignationType { get; set; }
    public string Status { get; set; } = "active";
    public DateOnly? EffectiveStartDate { get; set; }
    public DateOnly? EffectiveEndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class CompensationProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public string PayBasis { get; set; } = "salary";
    public string PayGrade { get; set; } = string.Empty;
    public string PayBand { get; set; } = string.Empty;
    public string? StepProgression { get; set; }
    public decimal? BaseRate { get; set; }
    public decimal? AnnualSalary { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public bool OvertimeEligible { get; set; } = true;
    public bool ShiftDifferentialEligible { get; set; }
    public bool BonusEligible { get; set; }
    public bool AllowanceEligible { get; set; }
    public string Status { get; set; } = "active";
    public DateOnly EffectiveStartDate { get; set; }
    public DateOnly? EffectiveEndDate { get; set; }
    public string? SourceProductKey { get; set; }
    public string? SourceRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class CompensationChangeRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public string ReasonCode { get; set; } = string.Empty;
    public string ReasonText { get; set; } = string.Empty;
    public string OldSnapshot { get; set; } = string.Empty;
    public string NewSnapshot { get; set; } = string.Empty;
    public Guid? RequestedByPersonId { get; set; }
    public Guid? ApprovedByPersonId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateOnly? EffectiveDate { get; set; }
    public string? SourceProductKey { get; set; }
    public string? SourceRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public static class StaffArrBenefitsCompensationModelConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BenefitEnrollment>(entity =>
        {
            entity.ToTable("staffarr_benefit_enrollments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BenefitType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PlanName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.BenefitClass).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CoverageLevel).HasMaxLength(32).IsRequired();
            entity.Property(x => x.EligibilityStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.EnrollmentStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CarrierExportStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CarrierMemberId).HasMaxLength(128);
            entity.Property(x => x.CarrierGroupId).HasMaxLength(128);
            entity.Property(x => x.SourceProductKey).HasMaxLength(64);
            entity.Property(x => x.SourceRef).HasMaxLength(256);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.EnrollmentStatus });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.BenefitType, x.EnrollmentStatus });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<BenefitDependent>(entity =>
        {
            entity.ToTable("staffarr_benefit_dependents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Relationship).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CoverageStatus).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Relationship });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<BenefitBeneficiary>(entity =>
        {
            entity.ToTable("staffarr_benefit_beneficiaries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Relationship).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AllocationPercent).HasPrecision(5, 2);
            entity.Property(x => x.DesignationType).HasMaxLength(64);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<CompensationProfile>(entity =>
        {
            entity.ToTable("staffarr_compensation_profiles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PayBasis).HasMaxLength(32).IsRequired();
            entity.Property(x => x.PayGrade).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PayBand).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StepProgression).HasMaxLength(64);
            entity.Property(x => x.BaseRate).HasPrecision(18, 4);
            entity.Property(x => x.AnnualSalary).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(8).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProductKey).HasMaxLength(64);
            entity.Property(x => x.SourceRef).HasMaxLength(256);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.EffectiveStartDate, x.EffectiveEndDate });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<CompensationChangeRequest>(entity =>
        {
            entity.ToTable("staffarr_compensation_change_requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequestType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ReasonText).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.OldSnapshot).HasMaxLength(4096).IsRequired();
            entity.Property(x => x.NewSnapshot).HasMaxLength(4096).IsRequired();
            entity.Property(x => x.SourceProductKey).HasMaxLength(64);
            entity.Property(x => x.SourceRef).HasMaxLength(256);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.RequestType, x.CreatedAt });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
        });
    }
}
