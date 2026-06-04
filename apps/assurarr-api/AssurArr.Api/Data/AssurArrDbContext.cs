using AssurArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace AssurArr.Api.Data;

public sealed class AssurArrDbContext(DbContextOptions<AssurArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<AssurArrNonconformance> Nonconformances => Set<AssurArrNonconformance>();
    public DbSet<AssurArrQualityHold> QualityHolds => Set<AssurArrQualityHold>();
    public DbSet<AssurArrCapa> Capas => Set<AssurArrCapa>();
    public DbSet<AssurArrQualityAudit> QualityAudits => Set<AssurArrQualityAudit>();
    public DbSet<AssurArrAuditFinding> AuditFindings => Set<AssurArrAuditFinding>();
    public DbSet<AssurArrQualityStatusSnapshot> QualityStatusSnapshots => Set<AssurArrQualityStatusSnapshot>();
    public DbSet<AssurArrQualityScorecard> QualityScorecards => Set<AssurArrQualityScorecard>();
    public DbSet<AssurArrQualityReview> QualityReviews => Set<AssurArrQualityReview>();
    public DbSet<AssurArrQualityRelease> QualityReleases => Set<AssurArrQualityRelease>();
    public DbSet<AssurArrTimelineEvent> TimelineEvents => Set<AssurArrTimelineEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureRecord<AssurArrNonconformance>(modelBuilder, "assurarr_nonconformances");
        ConfigureRecord<AssurArrQualityHold>(modelBuilder, "assurarr_quality_holds");
        ConfigureRecord<AssurArrCapa>(modelBuilder, "assurarr_capas");
        ConfigureRecord<AssurArrQualityAudit>(modelBuilder, "assurarr_quality_audits");
        ConfigureRecord<AssurArrAuditFinding>(modelBuilder, "assurarr_audit_findings");
        ConfigureRecord<AssurArrQualityStatusSnapshot>(modelBuilder, "assurarr_quality_status_snapshots");
        ConfigureRecord<AssurArrQualityScorecard>(modelBuilder, "assurarr_quality_scorecards");
        ConfigureReview(modelBuilder);
        ConfigureRelease(modelBuilder);

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
}
