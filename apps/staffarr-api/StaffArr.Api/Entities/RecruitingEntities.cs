using Microsoft.EntityFrameworkCore;

namespace StaffArr.Api.Entities;

public sealed class RecruitingRequisition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string RequisitionNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string JobCode { get; set; } = string.Empty;
    public string JobFamily { get; set; } = string.Empty;
    public string? DepartmentRef { get; set; }
    public string? SiteRef { get; set; }
    public string? LocationRef { get; set; }
    public Guid? HiringManagerPersonId { get; set; }
    public Guid? RecruiterPersonId { get; set; }
    public string Status { get; set; } = "draft";
    public int HeadcountRequested { get; set; } = 1;
    public int FilledCount { get; set; }
    public DateOnly? OpenDate { get; set; }
    public DateOnly? TargetStartDate { get; set; }
    public string? SourceProductKey { get; set; }
    public string? SourceRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class RecruitingCandidate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid? RecruitingRequisitionId { get; set; }
    public Guid? EmploymentApplicationSubmissionId { get; set; }
    public Guid? PersonId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string? CandidatePhone { get; set; }
    public string SourceType { get; set; } = "application";
    public string Stage { get; set; } = "applied";
    public string Status { get; set; } = "active";
    public string? BackgroundCheckStatus { get; set; }
    public string? DrugScreenStatus { get; set; }
    public string? PhysicalStatus { get; set; }
    public string? OfferStatus { get; set; }
    public decimal? Score { get; set; }
    public string? Notes { get; set; }
    public string? SourceProductKey { get; set; }
    public string? SourceRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class RecruitingInterviewStage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid RecruitingCandidateId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string Status { get; set; } = "scheduled";
    public DateTimeOffset? ScheduledAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid? InterviewerPersonId { get; set; }
    public decimal? Score { get; set; }
    public string? Recommendation { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class RecruitingOffer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid RecruitingCandidateId { get; set; }
    public string Status { get; set; } = "draft";
    public string Title { get; set; } = string.Empty;
    public string PayBasis { get; set; } = "salary";
    public decimal? AnnualSalary { get; set; }
    public decimal? HourlyRate { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? ApprovedByPersonId { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? DeclinedAt { get; set; }
    public string? Notes { get; set; }
    public string? SourceProductKey { get; set; }
    public string? SourceRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public static class StaffArrRecruitingModelConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecruitingRequisition>(entity =>
        {
            entity.ToTable("staffarr_recruiting_requisitions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequisitionNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.JobCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.JobFamily).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DepartmentRef).HasMaxLength(256);
            entity.Property(x => x.SiteRef).HasMaxLength(256);
            entity.Property(x => x.LocationRef).HasMaxLength(256);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProductKey).HasMaxLength(64);
            entity.Property(x => x.SourceRef).HasMaxLength(256);
            entity.HasIndex(x => new { x.TenantId, x.RequisitionNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.HiringManagerPersonId);
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.RecruiterPersonId);
        });

        modelBuilder.Entity<RecruitingCandidate>(entity =>
        {
            entity.ToTable("staffarr_recruiting_candidates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CandidateName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CandidateEmail).HasMaxLength(320).IsRequired();
            entity.Property(x => x.CandidatePhone).HasMaxLength(32);
            entity.Property(x => x.SourceType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Stage).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.BackgroundCheckStatus).HasMaxLength(32);
            entity.Property(x => x.DrugScreenStatus).HasMaxLength(32);
            entity.Property(x => x.PhysicalStatus).HasMaxLength(32);
            entity.Property(x => x.OfferStatus).HasMaxLength(32);
            entity.Property(x => x.Notes).HasMaxLength(2048);
            entity.Property(x => x.SourceProductKey).HasMaxLength(64);
            entity.Property(x => x.SourceRef).HasMaxLength(256);
            entity.HasIndex(x => new { x.TenantId, x.RecruitingRequisitionId, x.Stage });
            entity.HasOne<RecruitingRequisition>().WithMany().HasForeignKey(x => x.RecruitingRequisitionId);
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
            entity.HasOne<EmploymentApplicationSubmission>().WithMany().HasForeignKey(x => x.EmploymentApplicationSubmissionId);
        });

        modelBuilder.Entity<RecruitingInterviewStage>(entity =>
        {
            entity.ToTable("staffarr_recruiting_interview_stages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StageName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Recommendation).HasMaxLength(64);
            entity.Property(x => x.Notes).HasMaxLength(2048);
            entity.HasIndex(x => new { x.TenantId, x.RecruitingCandidateId, x.CreatedAt });
            entity.HasOne<RecruitingCandidate>().WithMany().HasForeignKey(x => x.RecruitingCandidateId);
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.InterviewerPersonId);
        });

        modelBuilder.Entity<RecruitingOffer>(entity =>
        {
            entity.ToTable("staffarr_recruiting_offers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PayBasis).HasMaxLength(32).IsRequired();
            entity.Property(x => x.AnnualSalary).HasPrecision(18, 2);
            entity.Property(x => x.HourlyRate).HasPrecision(18, 4);
            entity.Property(x => x.Notes).HasMaxLength(2048);
            entity.Property(x => x.SourceProductKey).HasMaxLength(64);
            entity.Property(x => x.SourceRef).HasMaxLength(256);
            entity.HasIndex(x => new { x.TenantId, x.RecruitingCandidateId, x.Status });
            entity.HasOne<RecruitingCandidate>().WithMany().HasForeignKey(x => x.RecruitingCandidateId);
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.ApprovedByPersonId);
        });
    }
}
