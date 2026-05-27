using Microsoft.EntityFrameworkCore;

using TrainArr.Api.Entities;

using STLCompliance.Shared.Data;



namespace TrainArr.Api.Data;



public sealed class TrainArrDbContext(DbContextOptions<TrainArrDbContext> options) : PlatformDbContext(options)

{

    public DbSet<CertificationPublication> CertificationPublications => Set<CertificationPublication>();



    public DbSet<StaffarrIncidentRemediation> StaffarrIncidentRemediations => Set<StaffarrIncidentRemediation>();



    public DbSet<TrainingDefinition> TrainingDefinitions => Set<TrainingDefinition>();



    public DbSet<TrainingAssignment> TrainingAssignments => Set<TrainingAssignment>();



    public DbSet<TrainingProgram> TrainingPrograms => Set<TrainingProgram>();



    public DbSet<TrainingProgramDefinition> TrainingProgramDefinitions => Set<TrainingProgramDefinition>();



    public DbSet<TrainingEvidence> TrainingEvidence => Set<TrainingEvidence>();



    public DbSet<TrainingEvaluation> TrainingEvaluations => Set<TrainingEvaluation>();



    public DbSet<TrainingSignoff> TrainingSignoffs => Set<TrainingSignoff>();

    public DbSet<QualificationIssue> QualificationIssues => Set<QualificationIssue>();

    public DbSet<TrainingCitationAttachment> TrainingCitationAttachments => Set<TrainingCitationAttachment>();

    public DbSet<TrainingRulePackRequirement> TrainingRulePackRequirements => Set<TrainingRulePackRequirement>();



    public DbSet<TrainArrAuditEvent> AuditEvents => Set<TrainArrAuditEvent>();



    protected override void OnModelCreating(ModelBuilder modelBuilder)

    {

        base.OnModelCreating(modelBuilder);



        modelBuilder.Entity<CertificationPublication>(entity =>

        {

            entity.ToTable("trainarr_certification_publications");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.QualificationKey).HasMaxLength(128).IsRequired();

            entity.Property(x => x.QualificationName).HasMaxLength(128).IsRequired();

            entity.Property(x => x.PublicationType).HasMaxLength(64).IsRequired();

            entity.Property(x => x.BlockerType).HasMaxLength(64).IsRequired();

            entity.Property(x => x.Message).HasMaxLength(1024).IsRequired();

            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();

            entity.HasIndex(x => x.TenantId);

            entity.HasIndex(x => new { x.TenantId, x.StaffarrPersonId, x.PublishedAt });

        });



        modelBuilder.Entity<StaffarrIncidentRemediation>(entity =>

        {

            entity.ToTable("trainarr_staffarr_incident_remediations");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ReasonCategoryKey).HasMaxLength(64).IsRequired();

            entity.Property(x => x.Severity).HasMaxLength(32).IsRequired();

            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();

            entity.Property(x => x.Description).HasMaxLength(4096).IsRequired();

            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();

            entity.HasIndex(x => x.TenantId);

            entity.HasIndex(x => new { x.TenantId, x.StaffarrIncidentId }).IsUnique();

            entity.HasIndex(x => new { x.TenantId, x.StaffarrPersonId, x.CreatedAt });

        });



        modelBuilder.Entity<TrainingDefinition>(entity =>

        {

            entity.ToTable("trainarr_training_definitions");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DefinitionKey).HasMaxLength(64).IsRequired();

            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();

            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();

            entity.Property(x => x.QualificationKey).HasMaxLength(128).IsRequired();

            entity.Property(x => x.QualificationName).HasMaxLength(128).IsRequired();

            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();

            entity.HasIndex(x => x.TenantId);

            entity.HasIndex(x => new { x.TenantId, x.DefinitionKey }).IsUnique();

        });



        modelBuilder.Entity<TrainingAssignment>(entity =>

        {

            entity.ToTable("trainarr_training_assignments");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.AssignmentReason).HasMaxLength(64).IsRequired();

            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();

            entity.HasIndex(x => x.TenantId);

            entity.HasIndex(x => new { x.TenantId, x.StaffarrPersonId, x.CreatedAt });

            entity.HasIndex(x => new { x.TenantId, x.StaffarrIncidentRemediationId });

            entity.HasOne(x => x.TrainingDefinition)

                .WithMany()

                .HasForeignKey(x => x.TrainingDefinitionId)

                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.StaffarrIncidentRemediation)

                .WithMany()

                .HasForeignKey(x => x.StaffarrIncidentRemediationId)

                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.QualificationIssue)

                .WithOne(x => x.TrainingAssignment)

                .HasForeignKey<QualificationIssue>(x => x.TrainingAssignmentId)

                .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<QualificationIssue>(entity =>
        {
            entity.ToTable("trainarr_qualification_issues");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.QualificationKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.QualificationName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.LifecycleReason).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ExpiresAt });
            entity.HasIndex(x => new { x.TenantId, x.TrainingAssignmentId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.GrantPublicationId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.LifecyclePublicationId })
                .IsUnique()
                .HasFilter("\"LifecyclePublicationId\" IS NOT NULL");
        });



        modelBuilder.Entity<TrainingProgram>(entity =>

        {

            entity.ToTable("trainarr_training_programs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ProgramKey).HasMaxLength(64).IsRequired();

            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();

            entity.Property(x => x.Description).HasMaxLength(2048).IsRequired();

            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();

            entity.HasIndex(x => x.TenantId);

            entity.HasIndex(x => new { x.TenantId, x.ProgramKey }).IsUnique();

        });



        modelBuilder.Entity<TrainingProgramDefinition>(entity =>

        {

            entity.ToTable("trainarr_training_program_definitions");

            entity.HasKey(x => new { x.TrainingProgramId, x.TrainingDefinitionId });

            entity.HasOne(x => x.TrainingProgram)

                .WithMany(x => x.ProgramDefinitions)

                .HasForeignKey(x => x.TrainingProgramId)

                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.TrainingDefinition)

                .WithMany()

                .HasForeignKey(x => x.TrainingDefinitionId)

                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.TrainingProgramId, x.SortOrder });

        });



        modelBuilder.Entity<TrainingEvaluation>(entity =>

        {

            entity.ToTable("trainarr_training_evaluations");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Result).HasMaxLength(32).IsRequired();

            entity.Property(x => x.Notes).HasMaxLength(2048);

            entity.HasIndex(x => x.TenantId);

            entity.HasIndex(x => new { x.TenantId, x.TrainingAssignmentId }).IsUnique();

            entity.HasOne(x => x.TrainingAssignment)

                .WithOne(x => x.Evaluation)

                .HasForeignKey<TrainingEvaluation>(x => x.TrainingAssignmentId)

                .OnDelete(DeleteBehavior.Cascade);

        });



        modelBuilder.Entity<TrainingSignoff>(entity =>

        {

            entity.ToTable("trainarr_training_signoffs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.SignoffRole).HasMaxLength(32).IsRequired();

            entity.Property(x => x.Notes).HasMaxLength(1024);

            entity.HasIndex(x => x.TenantId);

            entity.HasIndex(x => new { x.TenantId, x.TrainingAssignmentId, x.SignoffRole }).IsUnique();

            entity.HasOne(x => x.TrainingAssignment)

                .WithMany(x => x.Signoffs)

                .HasForeignKey(x => x.TrainingAssignmentId)

                .OnDelete(DeleteBehavior.Cascade);

        });



        modelBuilder.Entity<TrainingEvidence>(entity =>

        {

            entity.ToTable("trainarr_training_evidence");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.EvidenceTypeKey).HasMaxLength(64).IsRequired();

            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();

            entity.Property(x => x.ContentType).HasMaxLength(128).IsRequired();

            entity.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();

            entity.Property(x => x.Notes).HasMaxLength(1024);

            entity.HasIndex(x => x.TenantId);

            entity.HasIndex(x => new { x.TenantId, x.TrainingAssignmentId, x.CreatedAt });

            entity.HasOne(x => x.TrainingAssignment)

                .WithMany(x => x.EvidenceRecords)

                .HasForeignKey(x => x.TrainingAssignmentId)

                .OnDelete(DeleteBehavior.Cascade);

        });



        modelBuilder.Entity<TrainingCitationAttachment>(entity =>
        {
            entity.ToTable("trainarr_training_citation_attachments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CitationKey).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId, x.CitationKey });
            entity.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId, x.ComplianceCoreCitationId })
                .IsUnique();
        });

        modelBuilder.Entity<TrainingRulePackRequirement>(entity =>
        {
            entity.ToTable("trainarr_training_rule_pack_requirements");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RulePackKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.KnownStatus).HasMaxLength(32);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RulePackKey });
            entity.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId, x.RulePackKey }).IsUnique();
        });

        modelBuilder.Entity<TrainArrAuditEvent>(entity =>

        {

            entity.ToTable("trainarr_audit_events");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Action).HasMaxLength(128).IsRequired();

            entity.Property(x => x.TargetType).HasMaxLength(64).IsRequired();

            entity.Property(x => x.TargetId).HasMaxLength(128);

            entity.Property(x => x.Result).HasMaxLength(32).IsRequired();

            entity.Property(x => x.ReasonCode).HasMaxLength(64);

            entity.HasIndex(x => x.TenantId);

            entity.HasIndex(x => x.OccurredAt);

        });

    }

}


