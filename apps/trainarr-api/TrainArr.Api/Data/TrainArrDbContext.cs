using Microsoft.EntityFrameworkCore;

using TrainArr.Api.Entities;

using STLCompliance.Shared.Data;



namespace TrainArr.Api.Data;



public sealed class TrainArrDbContext(DbContextOptions<TrainArrDbContext> options) : PlatformDbContext(options)

{

    public DbSet<CertificationPublication> CertificationPublications => Set<CertificationPublication>();



    public DbSet<StaffarrIncidentRemediation> StaffarrIncidentRemediations => Set<StaffarrIncidentRemediation>();



    public DbSet<TrainingDefinition> TrainingDefinitions => Set<TrainingDefinition>();

    public DbSet<TrainingDefinitionStep> TrainingDefinitionSteps => Set<TrainingDefinitionStep>();

    public DbSet<TrainingDefinitionCompletionRule> TrainingDefinitionCompletionRules =>
        Set<TrainingDefinitionCompletionRule>();

    public DbSet<TrainingDefinitionStepBranch> TrainingDefinitionStepBranches =>
        Set<TrainingDefinitionStepBranch>();

    public DbSet<TrainingAssignmentStepProgress> TrainingAssignmentStepProgress =>
        Set<TrainingAssignmentStepProgress>();



    public DbSet<TrainingAssignment> TrainingAssignments => Set<TrainingAssignment>();

    public DbSet<TrainingAssignmentMaterialDemandLine> TrainingAssignmentMaterialDemandLines =>
        Set<TrainingAssignmentMaterialDemandLine>();

    public DbSet<TrainingAssignmentMaterialDemandStatusEvent> TrainingAssignmentMaterialDemandStatusEvents =>
        Set<TrainingAssignmentMaterialDemandStatusEvent>();



    public DbSet<TrainingProgram> TrainingPrograms => Set<TrainingProgram>();



    public DbSet<TrainingProgramDefinition> TrainingProgramDefinitions => Set<TrainingProgramDefinition>();

    public DbSet<TrainingProgramVersion> TrainingProgramVersions => Set<TrainingProgramVersion>();

    public DbSet<TrainingProgramVersionDefinition> TrainingProgramVersionDefinitions =>
        Set<TrainingProgramVersionDefinition>();

    public DbSet<TrainingMatrixEntry> TrainingMatrixEntries => Set<TrainingMatrixEntry>();

    public DbSet<TrainingApplicabilityProfile> TrainingApplicabilityProfiles => Set<TrainingApplicabilityProfile>();

    public DbSet<TrainingRequirement> TrainingRequirements => Set<TrainingRequirement>();

    public DbSet<TrainingEvidence> TrainingEvidence => Set<TrainingEvidence>();



    public DbSet<TrainingEvaluation> TrainingEvaluations => Set<TrainingEvaluation>();

    public DbSet<TrainingEvaluationRevision> TrainingEvaluationRevisions => Set<TrainingEvaluationRevision>();



    public DbSet<TrainingSignoff> TrainingSignoffs => Set<TrainingSignoff>();

    public DbSet<QualificationIssue> QualificationIssues => Set<QualificationIssue>();

    public DbSet<QualificationCheckRecord> QualificationCheckRecords => Set<QualificationCheckRecord>();

    public DbSet<TrainingCitationAttachment> TrainingCitationAttachments => Set<TrainingCitationAttachment>();

    public DbSet<TrainingRulePackRequirement> TrainingRulePackRequirements => Set<TrainingRulePackRequirement>();



    public DbSet<TenantTrainingNotificationSettings> TenantTrainingNotificationSettings =>
        Set<TenantTrainingNotificationSettings>();

    public DbSet<TenantAssignmentDueReminderSettings> TenantAssignmentDueReminderSettings =>
        Set<TenantAssignmentDueReminderSettings>();

    public DbSet<AssignmentDueReminderRun> AssignmentDueReminderRuns => Set<AssignmentDueReminderRun>();

    public DbSet<TenantAssignmentEscalationSettings> TenantAssignmentEscalationSettings =>
        Set<TenantAssignmentEscalationSettings>();

    public DbSet<AssignmentEscalationEvent> AssignmentEscalationEvents => Set<AssignmentEscalationEvent>();

    public DbSet<AssignmentEscalationRun> AssignmentEscalationRuns => Set<AssignmentEscalationRun>();

    public DbSet<TenantRecertificationSettings> TenantRecertificationSettings =>
        Set<TenantRecertificationSettings>();

    public DbSet<TenantQualificationRecalculationSettings> TenantQualificationRecalculationSettings =>
        Set<TenantQualificationRecalculationSettings>();

    public DbSet<QualificationRecalculationState> QualificationRecalculationStates =>
        Set<QualificationRecalculationState>();

    public DbSet<QualificationRecalculationRun> QualificationRecalculationRuns =>
        Set<QualificationRecalculationRun>();

    public DbSet<TenantRulePackImpactSettings> TenantRulePackImpactSettings =>
        Set<TenantRulePackImpactSettings>();

    public DbSet<RulePackImpactState> RulePackImpactStates =>
        Set<RulePackImpactState>();

    public DbSet<RulePackImpactRun> RulePackImpactRuns =>
        Set<RulePackImpactRun>();

    public DbSet<TenantEvidenceRetentionSettings> TenantEvidenceRetentionSettings =>
        Set<TenantEvidenceRetentionSettings>();

    public DbSet<EvidenceRetentionRun> EvidenceRetentionRuns =>
        Set<EvidenceRetentionRun>();

    public DbSet<TenantOrphanReferenceSettings> TenantOrphanReferenceSettings =>
        Set<TenantOrphanReferenceSettings>();

    public DbSet<OrphanReferenceFinding> OrphanReferenceFindings =>
        Set<OrphanReferenceFinding>();

    public DbSet<OrphanReferenceRun> OrphanReferenceRuns =>
        Set<OrphanReferenceRun>();

    public DbSet<RecertificationAssignmentRun> RecertificationAssignmentRuns =>
        Set<RecertificationAssignmentRun>();

    public DbSet<TrainingNotificationDispatch> TrainingNotificationDispatches =>
        Set<TrainingNotificationDispatch>();

    public DbSet<TenantIntegrationSettings> TenantIntegrationSettings =>
        Set<TenantIntegrationSettings>();

    public DbSet<TenantStaffarrPublicationSettings> TenantStaffarrPublicationSettings =>
        Set<TenantStaffarrPublicationSettings>();

    public DbSet<StaffarrPublicationDelivery> StaffarrPublicationDeliveries =>
        Set<StaffarrPublicationDelivery>();

    public DbSet<TenantEventProcessingSettings> TenantEventProcessingSettings =>
        Set<TenantEventProcessingSettings>();

    public DbSet<TrainingDomainEvent> TrainingDomainEvents => Set<TrainingDomainEvent>();

    public DbSet<PersonTrainingHistoryEntry> PersonTrainingHistoryEntries => Set<PersonTrainingHistoryEntry>();

    public DbSet<TrainArrAuditEvent> AuditEvents => Set<TrainArrAuditEvent>();

    public DbSet<AuditPackageGenerationJob> AuditPackageGenerationJobs => Set<AuditPackageGenerationJob>();



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

        modelBuilder.Entity<TrainingDefinitionStep>(entity =>
        {
            entity.ToTable("trainarr_training_definition_steps");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StepKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.StepType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ConfigJson).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TrainingDefinitionId, x.StepKey }).IsUnique();
            entity.HasOne(x => x.TrainingDefinition)
                .WithMany()
                .HasForeignKey(x => x.TrainingDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TrainingDefinitionCompletionRule>(entity =>
        {
            entity.ToTable("trainarr_training_definition_completion_rules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RuleKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RuleType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ConfigJson).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TrainingDefinitionId, x.RuleKey }).IsUnique();
            entity.HasOne(x => x.TrainingDefinition)
                .WithMany()
                .HasForeignKey(x => x.TrainingDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TrainingDefinitionStepBranch>(entity =>
        {
            entity.ToTable("trainarr_training_definition_step_branches");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BranchKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.BranchType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ConfigJson).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TrainingDefinitionStepId, x.BranchKey }).IsUnique();
            entity.HasOne(x => x.TrainingDefinitionStep)
                .WithMany()
                .HasForeignKey(x => x.TrainingDefinitionStepId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TrainingAssignmentStepProgress>(entity =>
        {
            entity.ToTable("trainarr_training_assignment_step_progress");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ResponseJson);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TrainingAssignmentId, x.TrainingDefinitionStepId }).IsUnique();
            entity.HasOne(x => x.TrainingAssignment)
                .WithMany()
                .HasForeignKey(x => x.TrainingAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.TrainingDefinitionStep)
                .WithMany()
                .HasForeignKey(x => x.TrainingDefinitionStepId)
                .OnDelete(DeleteBehavior.Restrict);
        });



        modelBuilder.Entity<TrainingAssignment>(entity =>

        {

            entity.ToTable("trainarr_training_assignments");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.AssignmentReason).HasMaxLength(64).IsRequired();

            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();

            entity.Property(x => x.StaffarrAcknowledgementStatus).HasMaxLength(32);

            entity.HasIndex(x => x.TenantId);

            entity.HasIndex(x => new { x.TenantId, x.StaffarrPersonId, x.CreatedAt });

            entity.HasIndex(x => new { x.TenantId, x.StaffarrIncidentRemediationId });

            entity.HasIndex(x => new { x.TenantId, x.SourceQualificationIssueId });

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

        modelBuilder.Entity<TrainingAssignmentMaterialDemandLine>(entity =>
        {
            entity.ToTable("trainarr_training_assignment_material_demand_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PartNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(256).IsRequired();
            entity.Property(x => x.QuantityRequested).HasPrecision(18, 4);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProcurementStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProcurementStatusMessage).HasMaxLength(512);
            entity.Property(x => x.QuantityReceived).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TrainingAssignmentId });
            entity.HasIndex(x => new { x.TenantId, x.TrainingAssignmentId, x.LineNumber });
            entity.HasIndex(x => new { x.TenantId, x.TrainarrPublicationId });
            entity.HasIndex(x => new { x.TenantId, x.ProcurementStatus });
            entity.HasOne(x => x.TrainingAssignment)
                .WithMany()
                .HasForeignKey(x => x.TrainingAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TrainingAssignmentMaterialDemandStatusEvent>(entity =>
        {
            entity.ToTable("trainarr_training_assignment_material_demand_status_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProcurementStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TrainarrPublicationId, x.OccurredAt });
            entity.HasIndex(x => new { x.TenantId, x.SupplyarrCallbackPublicationId }).IsUnique();
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

        modelBuilder.Entity<TrainingProgramVersion>(entity =>
        {
            entity.ToTable("trainarr_training_program_versions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2048).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TrainingProgramId, x.VersionNumber }).IsUnique();
            entity.HasOne(x => x.TrainingProgram)
                .WithMany()
                .HasForeignKey(x => x.TrainingProgramId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TrainingProgramVersionDefinition>(entity =>
        {
            entity.ToTable("trainarr_training_program_version_definitions");
            entity.HasKey(x => new { x.TrainingProgramVersionId, x.TrainingDefinitionId });
            entity.HasOne(x => x.TrainingProgramVersion)
                .WithMany(x => x.VersionDefinitions)
                .HasForeignKey(x => x.TrainingProgramVersionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.TrainingDefinition)
                .WithMany()
                .HasForeignKey(x => x.TrainingDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.TrainingProgramVersionId, x.SortOrder });
        });

        modelBuilder.Entity<TrainingMatrixEntry>(entity =>
        {
            entity.ToTable("trainarr_training_matrix_entries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ApplicabilityKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ApplicabilityLabel).HasMaxLength(128).IsRequired();
            entity.Property(x => x.RequirementLevel).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ApplicabilityKey, x.SortOrder });
            entity.HasOne(x => x.TrainingProgram)
                .WithMany()
                .HasForeignKey(x => x.TrainingProgramId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TrainingDefinition)
                .WithMany()
                .HasForeignKey(x => x.TrainingDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TrainingApplicabilityProfile>(entity =>
        {
            entity.ToTable("trainarr_training_applicability_profiles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProfileKey).HasMaxLength(96).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.ScopeType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ScopeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(32);
            entity.Property(x => x.SourceRecordId).HasMaxLength(64);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ProfileKey }).IsUnique();
        });

        modelBuilder.Entity<TrainingRequirement>(entity =>
        {
            entity.ToTable("trainarr_training_requirements");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequirementKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.RequirementSource).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceKey).HasMaxLength(128);
            entity.Property(x => x.RequirementLevel).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RequirementKey }).IsUnique();
            entity.HasOne(x => x.TrainingProgram)
                .WithMany()
                .HasForeignKey(x => x.TrainingProgramId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TrainingDefinition)
                .WithMany()
                .HasForeignKey(x => x.TrainingDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApplicabilityProfile)
                .WithMany()
                .HasForeignKey(x => x.ApplicabilityProfileId)
                .OnDelete(DeleteBehavior.SetNull);
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

        modelBuilder.Entity<TrainingEvaluationRevision>(entity =>
        {
            entity.ToTable("trainarr_training_evaluation_revisions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Result).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(2048);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TrainingAssignmentId, x.SupersededAt });
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

        modelBuilder.Entity<TenantTrainingNotificationSettings>(entity =>
        {
            entity.ToTable("trainarr_tenant_training_notification_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NotificationWebhookUrl).HasMaxLength(2048);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<TrainingNotificationDispatch>(entity =>
        {
            entity.ToTable("trainarr_training_notification_dispatches");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RelatedEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DispatchStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.WebhookHost).HasMaxLength(256);
            entity.Property(x => x.ErrorMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DispatchStatus, x.NextRetryAt, x.CreatedAt });
            entity.HasIndex(x => new
            {
                x.TenantId,
                x.EventKind,
                x.RelatedEntityType,
                x.RelatedEntityId,
            });
        });

        modelBuilder.Entity<TenantAssignmentDueReminderSettings>(entity =>
        {
            entity.ToTable("trainarr_tenant_assignment_due_reminder_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<AssignmentDueReminderRun>(entity =>
        {
            entity.ToTable("trainarr_assignment_due_reminder_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<TenantAssignmentEscalationSettings>(entity =>
        {
            entity.ToTable("trainarr_tenant_assignment_escalation_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<AssignmentEscalationEvent>(entity =>
        {
            entity.ToTable("trainarr_assignment_escalation_events");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TrainingAssignmentId, x.CreatedAt });
        });

        modelBuilder.Entity<AssignmentEscalationRun>(entity =>
        {
            entity.ToTable("trainarr_assignment_escalation_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<TenantRecertificationSettings>(entity =>
        {
            entity.ToTable("trainarr_tenant_recertification_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<RecertificationAssignmentRun>(entity =>
        {
            entity.ToTable("trainarr_recertification_assignment_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Outcome).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SkipReason).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.QualificationIssueId, x.Outcome });
            entity.HasIndex(x => new { x.TenantId, x.ProcessedAt });
        });

        modelBuilder.Entity<TenantQualificationRecalculationSettings>(entity =>
        {
            entity.ToTable("trainarr_tenant_qualification_recalculation_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<QualificationRecalculationState>(entity =>
        {
            entity.ToTable("trainarr_qualification_recalculation_states");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.QualificationKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Outcome).HasMaxLength(16).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.RulePackKey).HasMaxLength(128);
            entity.Property(x => x.PreviousOutcome).HasMaxLength(16);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.QualificationIssueId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ComputedAt });
        });

        modelBuilder.Entity<QualificationRecalculationRun>(entity =>
        {
            entity.ToTable("trainarr_qualification_recalculation_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Outcome).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CheckOutcome).HasMaxLength(16);
            entity.Property(x => x.SkipReason).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.QualificationIssueId });
            entity.HasIndex(x => new { x.TenantId, x.ProcessedAt });
        });

        modelBuilder.Entity<TenantRulePackImpactSettings>(entity =>
        {
            entity.ToTable("trainarr_tenant_rule_pack_impact_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<RulePackImpactState>(entity =>
        {
            entity.ToTable("trainarr_rule_pack_impact_states");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RulePackKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Triggers).HasMaxLength(512).IsRequired();
            entity.Property(x => x.BaselineStatus).HasMaxLength(32);
            entity.Property(x => x.CurrentStatus).HasMaxLength(32);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RulePackKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ComputedAt });
        });

        modelBuilder.Entity<RulePackImpactRun>(entity =>
        {
            entity.ToTable("trainarr_rule_pack_impact_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RulePackKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Outcome).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SkipReason).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RulePackKey });
            entity.HasIndex(x => new { x.TenantId, x.ProcessedAt });
        });

        modelBuilder.Entity<TenantEvidenceRetentionSettings>(entity =>
        {
            entity.ToTable("trainarr_tenant_evidence_retention_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<EvidenceRetentionRun>(entity =>
        {
            entity.ToTable("trainarr_evidence_retention_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Outcome).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SkipReason).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ProcessedAt });
        });

        modelBuilder.Entity<TenantOrphanReferenceSettings>(entity =>
        {
            entity.ToTable("trainarr_tenant_orphan_reference_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<OrphanReferenceFinding>(entity =>
        {
            entity.ToTable("trainarr_orphan_reference_findings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReferenceKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ReferenceKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.SampleSourceEntityType).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ReferenceKind, x.ReferenceKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.IsActive, x.LastDetectedAt });
        });

        modelBuilder.Entity<OrphanReferenceRun>(entity =>
        {
            entity.ToTable("trainarr_orphan_reference_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Outcome).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SkipReason).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ProcessedAt });
        });

        modelBuilder.Entity<TenantIntegrationSettings>(entity =>
        {
            entity.ToTable("trainarr_tenant_integration_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<TenantStaffarrPublicationSettings>(entity =>
        {
            entity.ToTable("trainarr_tenant_staffarr_publication_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<StaffarrPublicationDelivery>(entity =>
        {
            entity.ToTable("trainarr_staffarr_publication_deliveries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OperationKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PayloadJson).HasMaxLength(8192).IsRequired();
            entity.Property(x => x.DeliveryStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DeliveryStatus, x.NextRetryAt, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.CertificationPublicationId, x.OperationKind, x.DeliveryStatus });
        });

        modelBuilder.Entity<TenantEventProcessingSettings>(entity =>
        {
            entity.ToTable("trainarr_tenant_event_processing_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<TrainingDomainEvent>(entity =>
        {
            entity.ToTable("trainarr_training_domain_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.RelatedEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PayloadJson).HasMaxLength(8192).IsRequired();
            entity.Property(x => x.ProcessingStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ProcessingStatus, x.NextRetryAt, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.StaffarrPersonId, x.CreatedAt });
        });

        modelBuilder.Entity<PersonTrainingHistoryEntry>(entity =>
        {
            entity.ToTable("trainarr_person_training_history_entries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.RelatedEntityType).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SourceDomainEventId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.StaffarrPersonId, x.OccurredAt });
        });

        modelBuilder.Entity<QualificationCheckRecord>(entity =>
        {
            entity.ToTable("trainarr_qualification_check_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.QualificationKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Outcome).HasMaxLength(16).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.RulePackKey).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.StaffarrPersonId, x.CheckedAt });
            entity.HasIndex(x => new { x.TenantId, x.QualificationKey, x.CheckedAt });
            entity.HasIndex(x => new { x.TenantId, x.BatchId });
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

        modelBuilder.Entity<AuditPackageGenerationJob>(entity =>
        {
            entity.ToTable("trainarr_audit_package_generation_jobs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Format).HasMaxLength(16).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
            entity.Property(x => x.ArtifactZip);
            entity.Property(x => x.ArtifactJson);
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
            entity.HasIndex(x => x.CreatedAt);
        });

    }

}


