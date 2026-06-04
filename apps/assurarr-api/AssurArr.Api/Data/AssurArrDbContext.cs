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
}
