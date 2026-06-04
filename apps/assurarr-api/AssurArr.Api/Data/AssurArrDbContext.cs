using AssurArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace AssurArr.Api.Data;

public sealed class AssurArrDbContext(DbContextOptions<AssurArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<AssurArrNonconformance> Nonconformances => Set<AssurArrNonconformance>();
    public DbSet<AssurArrQualityHold> QualityHolds => Set<AssurArrQualityHold>();
    public DbSet<AssurArrCapa> Capas => Set<AssurArrCapa>();
    public DbSet<AssurArrCapaAction> CapaActions => Set<AssurArrCapaAction>();
    public DbSet<AssurArrCapaActionBlocker> CapaActionBlockers => Set<AssurArrCapaActionBlocker>();
    public DbSet<AssurArrVerificationPlan> VerificationPlans => Set<AssurArrVerificationPlan>();
    public DbSet<AssurArrEffectivenessVerification> EffectivenessVerifications => Set<AssurArrEffectivenessVerification>();
    public DbSet<AssurArrQualityAudit> QualityAudits => Set<AssurArrQualityAudit>();
    public DbSet<AssurArrQualityAuditChecklist> QualityAuditChecklists => Set<AssurArrQualityAuditChecklist>();
    public DbSet<AssurArrQualityAuditChecklistItem> QualityAuditChecklistItems => Set<AssurArrQualityAuditChecklistItem>();
    public DbSet<AssurArrAuditFinding> AuditFindings => Set<AssurArrAuditFinding>();
    public DbSet<AssurArrRootCauseAnalysis> RootCauseAnalyses => Set<AssurArrRootCauseAnalysis>();
    public DbSet<AssurArrQualityStatusSnapshot> QualityStatusSnapshots => Set<AssurArrQualityStatusSnapshot>();
    public DbSet<AssurArrQualityScorecard> QualityScorecards => Set<AssurArrQualityScorecard>();
    public DbSet<AssurArrQualityMetric> QualityMetrics => Set<AssurArrQualityMetric>();
    public DbSet<AssurArrQualityRiskProfile> QualityRiskProfiles => Set<AssurArrQualityRiskProfile>();
    public DbSet<AssurArrQualityReview> QualityReviews => Set<AssurArrQualityReview>();
    public DbSet<AssurArrQualityRelease> QualityReleases => Set<AssurArrQualityRelease>();
    public DbSet<AssurArrContainmentAction> ContainmentActions => Set<AssurArrContainmentAction>();
    public DbSet<AssurArrDisposition> Dispositions => Set<AssurArrDisposition>();
    public DbSet<AssurArrSupplierQualityIssue> SupplierQualityIssues => Set<AssurArrSupplierQualityIssue>();
    public DbSet<AssurArrSupplierCorrectiveActionRequest> SupplierCorrectiveActionRequests => Set<AssurArrSupplierCorrectiveActionRequest>();
    public DbSet<AssurArrCustomerComplaintQualityCase> CustomerComplaintQualityCases => Set<AssurArrCustomerComplaintQualityCase>();
    public DbSet<AssurArrTimelineEvent> TimelineEvents => Set<AssurArrTimelineEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureNonconformance(modelBuilder);
        ConfigureQualityHold(modelBuilder);
        ConfigureRecord<AssurArrCapa>(modelBuilder, "assurarr_capas");
        ConfigureCapaAction(modelBuilder);
        ConfigureCapaActionBlocker(modelBuilder);
        ConfigureVerificationPlan(modelBuilder);
        ConfigureEffectivenessVerification(modelBuilder);
        ConfigureRecord<AssurArrQualityAudit>(modelBuilder, "assurarr_quality_audits");
        ConfigureChecklist(modelBuilder);
        ConfigureChecklistItem(modelBuilder);
        ConfigureRecord<AssurArrAuditFinding>(modelBuilder, "assurarr_audit_findings");
        ConfigureRootCauseAnalysis(modelBuilder);
        ConfigureRecord<AssurArrQualityStatusSnapshot>(modelBuilder, "assurarr_quality_status_snapshots");
        ConfigureRecord<AssurArrQualityScorecard>(modelBuilder, "assurarr_quality_scorecards");
        ConfigureMetric(modelBuilder);
        ConfigureRiskProfile(modelBuilder);
        ConfigureReview(modelBuilder);
        ConfigureRelease(modelBuilder);
        ConfigureContainmentAction(modelBuilder);
        ConfigureDisposition(modelBuilder);
        ConfigureSupplierQualityIssue(modelBuilder);
        ConfigureSupplierCorrectiveActionRequest(modelBuilder);
        ConfigureCustomerComplaintQualityCase(modelBuilder);

        modelBuilder.Entity<AssurArrTimelineEvent>(entity =>
        {
            entity.ToTable("assurarr_timeline_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.SubjectType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SubjectId).IsRequired();
            entity.Property(x => x.EventType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Details).HasMaxLength(2048);
            entity.Property(x => x.OccurredAt).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.SubjectType, x.SubjectId });
            entity.HasIndex(x => new { x.TenantId, x.OccurredAt });
        });
    }

    private static void ConfigureRecord<T>(ModelBuilder modelBuilder, string tableName)
        where T : class
    {
        modelBuilder.Entity<T>(entity =>
        {
            entity.ToTable(tableName);
            entity.HasKey("Id");
            entity.Property<Guid>("TenantId").IsRequired();
            entity.Property<string>("Number").HasMaxLength(64).IsRequired();
            entity.Property<string>("Title").HasMaxLength(256).IsRequired();
            entity.Property<string>("Description").HasMaxLength(4000);
            entity.Property<string>("Severity").HasMaxLength(32).IsRequired();
            entity.Property<string>("Status").HasMaxLength(32).IsRequired();
            entity.Property<string?>("SourceProduct").HasMaxLength(64);
            entity.Property<string?>("SourceObjectRef").HasMaxLength(256);
            entity.Property<string[]>("AffectedObjectRefs").HasColumnType("text[]").IsRequired();
            entity.Property<Guid?>("OwnerPersonId");
            entity.Property<string[]>("RecordRefs").HasColumnType("text[]").IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt");
            entity.Property<DateTimeOffset>("UpdatedAt");
            entity.Property<DateTimeOffset?>("ClosedAt");
            entity.Property<Guid?>("ClosedByPersonId");
            entity.Property<string?>("ClosureSummary").HasMaxLength(4000);
            entity.HasIndex("TenantId");
            entity.HasIndex("TenantId", "Number").IsUnique();
            entity.HasIndex("TenantId", "Status");
        });
    }

    private static void ConfigureNonconformance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrNonconformance>(entity =>
        {
            entity.ToTable("assurarr_nonconformances");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Severity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64);
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256);
            entity.Property(x => x.AffectedObjectRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.OwnerPersonId);
            entity.Property(x => x.RecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.BlockerRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.ClosedAt);
            entity.Property(x => x.ClosedByPersonId);
            entity.Property(x => x.ClosureSummary).HasMaxLength(4000);
            entity.Property(x => x.NonconformanceType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CustomerImpact).HasMaxLength(4000);
            entity.Property(x => x.SupplierImpact).HasMaxLength(4000);
            entity.Property(x => x.SafetyImpact).HasMaxLength(4000);
            entity.Property(x => x.ComplianceImpact).HasMaxLength(4000);
            entity.Property(x => x.RecurrenceFlag).IsRequired();
            entity.Property(x => x.RepeatOfNonconformanceRef).HasMaxLength(256);
            entity.Property(x => x.RootCauseRef).HasMaxLength(256);
            entity.Property(x => x.DueAt);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status });
        });
    }

    private static void ConfigureQualityHold(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrQualityHold>(entity =>
        {
            entity.ToTable("assurarr_quality_holds");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Severity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64);
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256);
            entity.Property(x => x.AffectedObjectRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.OwnerPersonId);
            entity.Property(x => x.RecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.ClosedAt);
            entity.Property(x => x.ClosedByPersonId);
            entity.Property(x => x.ClosureSummary).HasMaxLength(4000);
            entity.Property(x => x.HoldType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.HoldScope).HasMaxLength(64).IsRequired();
            entity.Property(x => x.HoldReason).HasMaxLength(4000);
            entity.Property(x => x.ReleaseReason).HasMaxLength(4000);
            entity.Property(x => x.RejectionReason).HasMaxLength(4000);
            entity.Property(x => x.ConditionalReleaseTerms).HasMaxLength(4000);
            entity.Property(x => x.ReleaseRequirements).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.ReleaseApprovalRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.QuantityHeld);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(64);
            entity.Property(x => x.LotNumber).HasMaxLength(128);
            entity.Property(x => x.SerialNumber).HasMaxLength(128);
            entity.Property(x => x.PlacedAt);
            entity.Property(x => x.PlacedByPersonId);
            entity.Property(x => x.ReleasedAt);
            entity.Property(x => x.ReleasedByPersonId);
            entity.Property(x => x.RejectedAt);
            entity.Property(x => x.RejectedByPersonId);
            entity.Property(x => x.ExpiresAt);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status });
        });
    }

    private static void ConfigureReview(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrQualityReview>(entity =>
        {
            entity.ToTable("assurarr_quality_reviews");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Severity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64);
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256);
            entity.Property(x => x.AffectedObjectRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.OwnerPersonId);
            entity.Property(x => x.RecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.ClosedAt);
            entity.Property(x => x.ClosedByPersonId);
            entity.Property(x => x.ClosureSummary).HasMaxLength(4000);
            entity.Property(x => x.ReviewType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceReviewRef).HasMaxLength(256);
            entity.Property(x => x.DecisionReason).HasMaxLength(4000);
            entity.Property(x => x.RequiredEvidenceRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.SubmittedEvidenceRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status });
        });
    }

    private static void ConfigureMetric(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrQualityMetric>(entity =>
        {
            entity.ToTable("assurarr_quality_metrics");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.ScorecardId).IsRequired();
            entity.Property(x => x.MetricKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Category).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Value).HasPrecision(18, 4);
            entity.Property(x => x.Numerator).HasPrecision(18, 4);
            entity.Property(x => x.Denominator).HasPrecision(18, 4);
            entity.Property(x => x.Unit).HasMaxLength(64);
            entity.Property(x => x.TargetValue).HasPrecision(18, 4);
            entity.Property(x => x.WarningThreshold).HasPrecision(18, 4);
            entity.Property(x => x.CriticalThreshold).HasPrecision(18, 4);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProductRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.ScorecardId });
            entity.HasIndex(x => new { x.TenantId, x.MetricKey });
        });
    }

    private static void ConfigureRiskProfile(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrQualityRiskProfile>(entity =>
        {
            entity.ToTable("assurarr_quality_risk_profiles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.TargetType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TargetRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.RiskLevel).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RiskFactors).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.OpenIssueCount).IsRequired();
            entity.Property(x => x.RepeatIssueCount).IsRequired();
            entity.Property(x => x.CriticalIssueCount).IsRequired();
            entity.Property(x => x.LastIncidentAt);
            entity.Property(x => x.MitigationActions).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.ReviewedAt);
            entity.Property(x => x.ReviewedByPersonId);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.TargetType, x.TargetRef }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RiskLevel });
        });
    }

    private static void ConfigureCapaAction(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrCapaAction>(entity =>
        {
            entity.ToTable("assurarr_capa_actions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CapaId).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ActionType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AssignedPersonId);
            entity.Property(x => x.AssignedTeamRef).HasMaxLength(256);
            entity.Property(x => x.SourceProductActionRef).HasMaxLength(256);
            entity.Property(x => x.TargetProduct).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TargetObjectRef).HasMaxLength(256);
            entity.Property(x => x.DueAt);
            entity.Property(x => x.StartedAt);
            entity.Property(x => x.CompletedAt);
            entity.Property(x => x.CompletedByPersonId);
            entity.Property(x => x.VerificationRequired).IsRequired();
            entity.Property(x => x.VerifiedAt);
            entity.Property(x => x.VerifiedByPersonId);
            entity.Property(x => x.EvidenceRecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.BlockerRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.CapaId });
            entity.HasIndex(x => new { x.TenantId, x.Status });
        });
    }

    private static void ConfigureCapaActionBlocker(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrCapaActionBlocker>(entity =>
        {
            entity.ToTable("assurarr_capa_action_blockers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CapaActionId).IsRequired();
            entity.Property(x => x.BlockerType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64);
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256);
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.ResolvedAt);
            entity.Property(x => x.ResolvedByPersonId);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.CapaActionId });
            entity.HasIndex(x => new { x.TenantId, x.Status });
        });
    }

    private static void ConfigureVerificationPlan(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrVerificationPlan>(entity =>
        {
            entity.ToTable("assurarr_verification_plans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CapaId).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.VerificationType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SuccessCriteria).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.SampleSize);
            entity.Property(x => x.ObservationPeriodDays);
            entity.Property(x => x.RequiredEvidenceTypes).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.ResponsiblePersonId);
            entity.Property(x => x.PlannedVerificationAt);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.CapaId });
            entity.HasIndex(x => new { x.TenantId, x.Status });
        });
    }

    private static void ConfigureEffectivenessVerification(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrEffectivenessVerification>(entity =>
        {
            entity.ToTable("assurarr_effectiveness_verifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CapaId).IsRequired();
            entity.Property(x => x.VerificationPlanId);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.PerformedByPersonId);
            entity.Property(x => x.PerformedAt);
            entity.Property(x => x.ResultSummary).HasMaxLength(4000);
            entity.Property(x => x.EvidenceRecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.MetricResults).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.RecurrenceFound).IsRequired();
            entity.Property(x => x.FollowUpRequired).IsRequired();
            entity.Property(x => x.ReopenedCapaRef).HasMaxLength(256);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.CapaId });
            entity.HasIndex(x => new { x.TenantId, x.Status });
        });
    }

    private static void ConfigureChecklist(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrQualityAuditChecklist>(entity =>
        {
            entity.ToTable("assurarr_quality_audit_checklists");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AuditId).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ItemRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.ClosedAt);
            entity.Property(x => x.ClosedByPersonId);
            entity.Property(x => x.ClosureSummary).HasMaxLength(4000);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.AuditId });
            entity.HasIndex(x => new { x.TenantId, x.Status });
        });
    }

    private static void ConfigureChecklistItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrQualityAuditChecklistItem>(entity =>
        {
            entity.ToTable("assurarr_quality_audit_checklist_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ChecklistId).IsRequired();
            entity.Property(x => x.Sequence).IsRequired();
            entity.Property(x => x.Prompt).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.HelpText).HasMaxLength(4000);
            entity.Property(x => x.RequirementRef).HasMaxLength(256);
            entity.Property(x => x.ResponseType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Required).IsRequired();
            entity.Property(x => x.ResponseValue).HasMaxLength(4000);
            entity.Property(x => x.Result).HasMaxLength(64);
            entity.Property(x => x.FindingCreated).IsRequired();
            entity.Property(x => x.FindingRef).HasMaxLength(256);
            entity.Property(x => x.EvidenceRecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.AnsweredAt);
            entity.Property(x => x.AnsweredByPersonId);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ChecklistId });
            entity.HasIndex(x => new { x.TenantId, x.Sequence });
        });
    }

    private static void ConfigureRootCauseAnalysis(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrRootCauseAnalysis>(entity =>
        {
            entity.ToTable("assurarr_root_cause_analyses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.NonconformanceId).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Method).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PrimaryCauseCategory).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64);
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256);
            entity.Property(x => x.AffectedObjectRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.OwnerPersonId);
            entity.Property(x => x.RecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.RootCauseSummary).HasMaxLength(4000);
            entity.Property(x => x.ContributingFactors).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.AnalyzedByPersonId);
            entity.Property(x => x.CompletedAt);
            entity.Property(x => x.EvidenceRecordRefs).HasColumnType("text[]").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.NonconformanceId });
            entity.HasIndex(x => new { x.TenantId, x.Status });
        });
    }

    private static void ConfigureRelease(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrQualityRelease>(entity =>
        {
            entity.ToTable("assurarr_quality_releases");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Severity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64);
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256);
            entity.Property(x => x.AffectedObjectRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.OwnerPersonId);
            entity.Property(x => x.RecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.ClosedAt);
            entity.Property(x => x.ClosedByPersonId);
            entity.Property(x => x.ClosureSummary).HasMaxLength(4000);
            entity.Property(x => x.HoldRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ReleaseType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Conditions).HasMaxLength(4000);
            entity.Property(x => x.EvidenceRecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.HoldRef });
        });
    }

    private static void ConfigureContainmentAction(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrContainmentAction>(entity =>
        {
            entity.ToTable("assurarr_containment_actions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Severity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64);
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256);
            entity.Property(x => x.AffectedObjectRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.NonconformanceRef).HasMaxLength(256);
            entity.Property(x => x.ActionType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AssignedTeamRef).HasMaxLength(256);
            entity.Property(x => x.SourceProductActionRef).HasMaxLength(256);
            entity.Property(x => x.DueAt);
            entity.Property(x => x.StartedAt);
            entity.Property(x => x.CompletedAt);
            entity.Property(x => x.CompletedByPersonId);
            entity.Property(x => x.VerificationRequired).IsRequired();
            entity.Property(x => x.VerifiedByPersonId);
            entity.Property(x => x.VerifiedAt);
            entity.Property(x => x.EvidenceRecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.ClosedAt);
            entity.Property(x => x.ClosedByPersonId);
            entity.Property(x => x.ClosureSummary).HasMaxLength(4000);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.NonconformanceRef });
        });
    }

    private static void ConfigureDisposition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrDisposition>(entity =>
        {
            entity.ToTable("assurarr_dispositions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Severity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64);
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256);
            entity.Property(x => x.AffectedObjectRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.NonconformanceRef).HasMaxLength(256);
            entity.Property(x => x.DispositionType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DecisionAt);
            entity.Property(x => x.ApprovedByPersonId);
            entity.Property(x => x.ApprovedAt);
            entity.Property(x => x.Rationale).HasMaxLength(4000);
            entity.Property(x => x.RequiredActions).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.ExecutionProduct).HasMaxLength(64);
            entity.Property(x => x.ExecutionObjectRef).HasMaxLength(256);
            entity.Property(x => x.EvidenceRecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.ClosedAt);
            entity.Property(x => x.ClosedByPersonId);
            entity.Property(x => x.ClosureSummary).HasMaxLength(4000);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.NonconformanceRef });
        });
    }

    private static void ConfigureSupplierQualityIssue(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrSupplierQualityIssue>(entity =>
        {
            entity.ToTable("assurarr_supplier_quality_issues");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Severity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64);
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256);
            entity.Property(x => x.AffectedReceiptRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.AffectedPurchaseOrderRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.AffectedItemRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.SupplierRef).HasMaxLength(256);
            entity.Property(x => x.NonconformanceRef).HasMaxLength(256);
            entity.Property(x => x.ScarRef).HasMaxLength(256);
            entity.Property(x => x.HoldRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.RecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.ClosedAt);
            entity.Property(x => x.ClosedByPersonId);
            entity.Property(x => x.ClosureSummary).HasMaxLength(4000);
            entity.Property(x => x.IssueType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OpenedAt);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.SupplierRef });
        });
    }

    private static void ConfigureSupplierCorrectiveActionRequest(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrSupplierCorrectiveActionRequest>(entity =>
        {
            entity.ToTable("assurarr_supplier_corrective_action_requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Severity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64);
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256);
            entity.Property(x => x.AffectedObjectRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.SupplierRef).HasMaxLength(256);
            entity.Property(x => x.SourceNonconformanceRef).HasMaxLength(256);
            entity.Property(x => x.SourceCapaRef).HasMaxLength(256);
            entity.Property(x => x.RequestedByPersonId);
            entity.Property(x => x.RequestedAt);
            entity.Property(x => x.SupplierDueAt);
            entity.Property(x => x.SupplierResponseRecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.ReviewPersonId);
            entity.Property(x => x.ReviewedAt);
            entity.Property(x => x.ReviewDecision).HasMaxLength(128);
            entity.Property(x => x.FollowUpCapaRef).HasMaxLength(256);
            entity.Property(x => x.RecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.ClosedAt);
            entity.Property(x => x.ClosedByPersonId);
            entity.Property(x => x.ClosureSummary).HasMaxLength(4000);
            entity.Property(x => x.OwnerPersonId);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.SupplierRef });
            entity.HasIndex(x => new { x.TenantId, x.SourceNonconformanceRef });
        });
    }

    private static void ConfigureCustomerComplaintQualityCase(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssurArrCustomerComplaintQualityCase>(entity =>
        {
            entity.ToTable("assurarr_customer_complaint_quality_cases");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Severity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64);
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256);
            entity.Property(x => x.AffectedOrderRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.AffectedShipmentRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.AffectedItemRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.AffectedAssetRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.CustomerRef).HasMaxLength(256);
            entity.Property(x => x.CustomerContactSnapshot).HasMaxLength(4000);
            entity.Property(x => x.CustomerLocationRef).HasMaxLength(256);
            entity.Property(x => x.NonconformanceRef).HasMaxLength(256);
            entity.Property(x => x.HoldRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.CapaRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.CustomerResponseRecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.RecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.ClosedAt);
            entity.Property(x => x.ClosedByPersonId);
            entity.Property(x => x.ClosureSummary).HasMaxLength(4000);
            entity.Property(x => x.ComplaintType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ReceivedAt);
            entity.Property(x => x.ReceivedByPersonId);
            entity.Property(x => x.CustomerResponseDueAt);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Number }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.CustomerRef });
        });
    }
}
