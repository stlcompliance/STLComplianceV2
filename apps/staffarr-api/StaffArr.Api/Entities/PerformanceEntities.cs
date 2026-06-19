using Microsoft.EntityFrameworkCore;

namespace StaffArr.Api.Entities;

public sealed class PerformanceReviewCycle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public string CycleName { get; set; } = string.Empty;
    public string CycleType { get; set; } = "annual";
    public string Status { get; set; } = "planned";
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly? SelfReviewDueDate { get; set; }
    public DateOnly? ManagerReviewDueDate { get; set; }
    public Guid? ManagerPersonId { get; set; }
    public DateTimeOffset? SelfReviewCompletedAt { get; set; }
    public DateTimeOffset? ManagerReviewCompletedAt { get; set; }
    public string? OverallRating { get; set; }
    public bool PromotionReady { get; set; }
    public bool SuccessionReady { get; set; }
    public DateTimeOffset? NextCheckInAt { get; set; }
    public string? Summary { get; set; }
    public string? DevelopmentPlan { get; set; }
    public string? SourceProductKey { get; set; }
    public string? SourceRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PerformanceGoal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public Guid? PerformanceReviewCycleId { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
    public string GoalType { get; set; } = "development";
    public string Status { get; set; } = "not_started";
    public string Priority { get; set; } = "medium";
    public decimal ProgressPercent { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? TargetDate { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid? OwnerPersonId { get; set; }
    public string? SuccessMetric { get; set; }
    public string? Summary { get; set; }
    public string? ResultSummary { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PerformanceCompetencyAssessment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public Guid? PerformanceReviewCycleId { get; set; }
    public string CompetencyKey { get; set; } = string.Empty;
    public string CompetencyName { get; set; } = string.Empty;
    public string ExpectedLevel { get; set; } = "proficient";
    public string CurrentLevel { get; set; } = "proficient";
    public string Rating { get; set; } = "meets";
    public string Status { get; set; } = "draft";
    public string? Notes { get; set; }
    public Guid? AssessedByPersonId { get; set; }
    public DateTimeOffset? AssessedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PerformanceFeedbackEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public Guid? PerformanceReviewCycleId { get; set; }
    public string FeedbackType { get; set; } = "manager_review";
    public string Visibility { get; set; } = "manager";
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Sentiment { get; set; }
    public Guid? AuthorPersonId { get; set; }
    public Guid? RelatedPersonId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PerformanceImprovementPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public DateOnly StartDate { get; set; }
    public DateOnly? TargetDate { get; set; }
    public string? CheckInCadence { get; set; }
    public DateTimeOffset? NextCheckInAt { get; set; }
    public Guid? ManagerPersonId { get; set; }
    public Guid? HrOwnerPersonId { get; set; }
    public string? Summary { get; set; }
    public string? Expectations { get; set; }
    public string? SuccessCriteria { get; set; }
    public string? SourceProductKey { get; set; }
    public string? SourceRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public static class StaffArrPerformanceModelConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PerformanceReviewCycle>(entity =>
        {
            entity.ToTable("staffarr_performance_review_cycles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CycleName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CycleType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.OverallRating).HasMaxLength(64);
            entity.Property(x => x.Summary).HasMaxLength(2048);
            entity.Property(x => x.DevelopmentPlan).HasMaxLength(4096);
            entity.Property(x => x.SourceProductKey).HasMaxLength(64);
            entity.Property(x => x.SourceRef).HasMaxLength(256);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.StartDate, x.EndDate });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<PerformanceGoal>(entity =>
        {
            entity.ToTable("staffarr_performance_goals");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.GoalTitle).HasMaxLength(200).IsRequired();
            entity.Property(x => x.GoalType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Priority).HasMaxLength(16).IsRequired();
            entity.Property(x => x.ProgressPercent).HasPrecision(5, 2);
            entity.Property(x => x.SuccessMetric).HasMaxLength(1024);
            entity.Property(x => x.Summary).HasMaxLength(2048);
            entity.Property(x => x.ResultSummary).HasMaxLength(2048);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.PerformanceReviewCycleId });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
            entity.HasOne<PerformanceReviewCycle>().WithMany().HasForeignKey(x => x.PerformanceReviewCycleId);
        });

        modelBuilder.Entity<PerformanceCompetencyAssessment>(entity =>
        {
            entity.ToTable("staffarr_performance_competency_assessments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CompetencyKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CompetencyName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ExpectedLevel).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CurrentLevel).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Rating).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(2048);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.PerformanceReviewCycleId, x.CompetencyKey }).IsUnique();
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
            entity.HasOne<PerformanceReviewCycle>().WithMany().HasForeignKey(x => x.PerformanceReviewCycleId);
        });

        modelBuilder.Entity<PerformanceFeedbackEntry>(entity =>
        {
            entity.ToTable("staffarr_performance_feedback_entries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FeedbackType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Visibility).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Body).HasMaxLength(4096).IsRequired();
            entity.Property(x => x.Sentiment).HasMaxLength(32);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.PerformanceReviewCycleId });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
            entity.HasOne<PerformanceReviewCycle>().WithMany().HasForeignKey(x => x.PerformanceReviewCycleId);
        });

        modelBuilder.Entity<PerformanceImprovementPlan>(entity =>
        {
            entity.ToTable("staffarr_performance_improvement_plans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PlanName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CheckInCadence).HasMaxLength(64);
            entity.Property(x => x.Summary).HasMaxLength(2048);
            entity.Property(x => x.Expectations).HasMaxLength(4096);
            entity.Property(x => x.SuccessCriteria).HasMaxLength(4096);
            entity.Property(x => x.SourceProductKey).HasMaxLength(64);
            entity.Property(x => x.SourceRef).HasMaxLength(256);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.StartDate, x.TargetDate });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
        });
    }
}
