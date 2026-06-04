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
    public DbSet<AssurArrVerificationPlan> VerificationPlans => Set<AssurArrVerificationPlan>();
    public DbSet<AssurArrQualityAudit> QualityAudits => Set<AssurArrQualityAudit>();
    public DbSet<AssurArrQualityAuditChecklist> QualityAuditChecklists => Set<AssurArrQualityAuditChecklist>();
    public DbSet<AssurArrQualityAuditChecklistItem> QualityAuditChecklistItems => Set<AssurArrQualityAuditChecklistItem>();
    public DbSet<AssurArrAuditFinding> AuditFindings => Set<AssurArrAuditFinding>();
    public DbSet<AssurArrQualityStatusSnapshot> QualityStatusSnapshots => Set<AssurArrQualityStatusSnapshot>();
    public DbSet<AssurArrQualityScorecard> QualityScorecards => Set<AssurArrQualityScorecard>();
    public DbSet<AssurArrQualityReview> QualityReviews => Set<AssurArrQualityReview>();
    public DbSet<AssurArrQualityRelease> QualityReleases => Set<AssurArrQualityRelease>();
    public DbSet<AssurArrContainmentAction> ContainmentActions => Set<AssurArrContainmentAction>();
    public DbSet<AssurArrDisposition> Dispositions => Set<AssurArrDisposition>();
    public DbSet<AssurArrSupplierQualityIssue> SupplierQualityIssues => Set<AssurArrSupplierQualityIssue>();
    public DbSet<AssurArrCustomerComplaintQualityCase> CustomerComplaintQualityCases => Set<AssurArrCustomerComplaintQualityCase>();
    public DbSet<AssurArrTimelineEvent> TimelineEvents => Set<AssurArrTimelineEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureRecord<AssurArrNonconformance>(modelBuilder, "assurarr_nonconformances");
        ConfigureRecord<AssurArrQualityHold>(modelBuilder, "assurarr_quality_holds");
        ConfigureRecord<AssurArrCapa>(modelBuilder, "assurarr_capas");
        ConfigureCapaAction(modelBuilder);
        ConfigureVerificationPlan(modelBuilder);
        ConfigureRecord<AssurArrQualityAudit>(modelBuilder, "assurarr_quality_audits");
        ConfigureChecklist(modelBuilder);
        ConfigureChecklistItem(modelBuilder);
        ConfigureRecord<AssurArrAuditFinding>(modelBuilder, "assurarr_audit_findings");
        ConfigureRecord<AssurArrQualityStatusSnapshot>(modelBuilder, "assurarr_quality_status_snapshots");
        ConfigureRecord<AssurArrQualityScorecard>(modelBuilder, "assurarr_quality_scorecards");
        ConfigureReview(modelBuilder);
        ConfigureRelease(modelBuilder);
        ConfigureContainmentAction(modelBuilder);
        ConfigureDisposition(modelBuilder);
        ConfigureSupplierQualityIssue(modelBuilder);
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
