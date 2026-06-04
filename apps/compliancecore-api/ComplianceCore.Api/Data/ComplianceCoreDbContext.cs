using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ComplianceCore.Api.Entities;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Data;

public sealed class ComplianceCoreDbContext(DbContextOptions<ComplianceCoreDbContext> options)
    : PlatformDbContext(options)
{
    public DbSet<VocabularyType> VocabularyTypes => Set<VocabularyType>();

    public DbSet<VocabularyTerm> VocabularyTerms => Set<VocabularyTerm>();

    public DbSet<VocabularyAlias> VocabularyAliases => Set<VocabularyAlias>();

    public DbSet<ComplianceKey> ComplianceKeys => Set<ComplianceKey>();

    public DbSet<MaterialKey> MaterialKeys => Set<MaterialKey>();

    public DbSet<ComplianceCoreAuditEvent> AuditEvents => Set<ComplianceCoreAuditEvent>();

    public DbSet<GoverningBody> GoverningBodies => Set<GoverningBody>();

    public DbSet<Jurisdiction> Jurisdictions => Set<Jurisdiction>();

    public DbSet<RegulatoryProgram> RegulatoryPrograms => Set<RegulatoryProgram>();

    public DbSet<RulePack> RulePacks => Set<RulePack>();

    public DbSet<RegulatoryCitation> RegulatoryCitations => Set<RegulatoryCitation>();

    public DbSet<FactDefinition> FactDefinitions => Set<FactDefinition>();

    public DbSet<FactRequirement> FactRequirements => Set<FactRequirement>();

    public DbSet<EvidenceReference> EvidenceReferences => Set<EvidenceReference>();

    public DbSet<FactAssertion> FactAssertions => Set<FactAssertion>();

    public DbSet<AuditTrace> AuditTraces => Set<AuditTrace>();

    public DbSet<RegulatoryMapping> RegulatoryMappings => Set<RegulatoryMapping>();

    public DbSet<RuleEvaluationRun> RuleEvaluationRuns => Set<RuleEvaluationRun>();

    public DbSet<RuleTestCase> RuleTestCases => Set<RuleTestCase>();

    public DbSet<ScheduledRuleEvaluationRun> ScheduledRuleEvaluationRuns => Set<ScheduledRuleEvaluationRun>();

    public DbSet<FactSource> FactSources => Set<FactSource>();

    public DbSet<ProductFactMirror> ProductFactMirrors => Set<ProductFactMirror>();

    public DbSet<ComplianceFinding> ComplianceFindings => Set<ComplianceFinding>();

    public DbSet<WorkflowGateDefinition> WorkflowGateDefinitions => Set<WorkflowGateDefinition>();

    public DbSet<WorkflowGateCheckResult> WorkflowGateCheckResults => Set<WorkflowGateCheckResult>();

    public DbSet<ProductGateResponse> ProductGateResponses => Set<ProductGateResponse>();

    public DbSet<SdsReference> SdsReferences => Set<SdsReference>();

    public DbSet<HazComReference> HazComReferences => Set<HazComReference>();

    public DbSet<AuditPackageGenerationJob> AuditPackageGenerationJobs => Set<AuditPackageGenerationJob>();

    public DbSet<SourceIngestionBatch> SourceIngestionBatches => Set<SourceIngestionBatch>();

    public DbSet<SourceIngestionJob> SourceIngestionJobs => Set<SourceIngestionJob>();

    public DbSet<RuleChangeEvent> RuleChangeEvents => Set<RuleChangeEvent>();

    public DbSet<RulePackMonitorSnapshot> RulePackMonitorSnapshots => Set<RulePackMonitorSnapshot>();

    public DbSet<RuleChangeScanRun> RuleChangeScanRuns => Set<RuleChangeScanRun>();

    public DbSet<RiskScoreRun> RiskScoreRuns => Set<RiskScoreRun>();

    public DbSet<RiskScore> RiskScores => Set<RiskScore>();

    public DbSet<MissingEvidenceWarningRun> MissingEvidenceWarningRuns => Set<MissingEvidenceWarningRun>();

    public DbSet<MissingEvidenceWarning> MissingEvidenceWarnings => Set<MissingEvidenceWarning>();

    public DbSet<ControlEffectivenessRun> ControlEffectivenessRuns => Set<ControlEffectivenessRun>();

    public DbSet<ControlEffectivenessRecord> ControlEffectivenessRecords => Set<ControlEffectivenessRecord>();

    public DbSet<ReadinessForecastRun> ReadinessForecastRuns => Set<ReadinessForecastRun>();

    public DbSet<ReadinessForecast> ReadinessForecasts => Set<ReadinessForecast>();

    public DbSet<TenantM12AnalyticsWorkerSettings> TenantM12AnalyticsWorkerSettings =>
        Set<TenantM12AnalyticsWorkerSettings>();

    public DbSet<M12AnalyticsBatchRun> M12AnalyticsBatchRuns => Set<M12AnalyticsBatchRun>();

    public DbSet<TenantFactSourceSyncWorkerSettings> TenantFactSourceSyncWorkerSettings =>
        Set<TenantFactSourceSyncWorkerSettings>();

    public DbSet<FactSourceSyncStatus> FactSourceSyncStatuses => Set<FactSourceSyncStatus>();

    public DbSet<ComplianceWaiver> ComplianceWaivers => Set<ComplianceWaiver>();

    public DbSet<ComplianceExceptionExemption> ComplianceExceptionExemptions => Set<ComplianceExceptionExemption>();

    public DbSet<ImportSession> ImportSessions => Set<ImportSession>();

    public DbSet<ImportSessionSourceFile> ImportSessionSourceFiles => Set<ImportSessionSourceFile>();

    public DbSet<ImportStagedRulePack> ImportStagedRulePacks => Set<ImportStagedRulePack>();

    public DbSet<ImportStagedRuleRequirement> ImportStagedRuleRequirements => Set<ImportStagedRuleRequirement>();

    public DbSet<ImportStagedFactRequirement> ImportStagedFactRequirements => Set<ImportStagedFactRequirement>();

    public DbSet<ImportStagedRegulatoryMapping> ImportStagedRegulatoryMappings => Set<ImportStagedRegulatoryMapping>();

    public DbSet<ImportStagedControlledVocabulary> ImportStagedControlledVocabulary => Set<ImportStagedControlledVocabulary>();

    public DbSet<ImportStagedVocabularyAlias> ImportStagedVocabularyAliases => Set<ImportStagedVocabularyAlias>();

    public DbSet<ImportStagedComplianceKey> ImportStagedComplianceKeys => Set<ImportStagedComplianceKey>();

    public DbSet<ImportStagedMaterialKey> ImportStagedMaterialKeys => Set<ImportStagedMaterialKey>();

    public DbSet<ImportStagedSdsReference> ImportStagedSdsReferences => Set<ImportStagedSdsReference>();

    public DbSet<ImportStagedEvidenceReference> ImportStagedEvidenceReferences => Set<ImportStagedEvidenceReference>();

    public DbSet<ImportStagedExceptionExemption> ImportStagedExceptionExemptions => Set<ImportStagedExceptionExemption>();

    public DbSet<ImportStagedMappingCandidate> ImportStagedMappingCandidates => Set<ImportStagedMappingCandidate>();

    public DbSet<ImportStagedMappingDecision> ImportStagedMappingDecisions => Set<ImportStagedMappingDecision>();

    public DbSet<ComplianceEvidenceOptionGroup> ComplianceEvidenceOptionGroups => Set<ComplianceEvidenceOptionGroup>();

    public DbSet<ComplianceEvidenceOption> ComplianceEvidenceOptions => Set<ComplianceEvidenceOption>();

    public DbSet<ExternalObjectReference> ExternalObjectReferences => Set<ExternalObjectReference>();

    public DbSet<DocumentReference> DocumentReferences => Set<DocumentReference>();

    public DbSet<MaterialReference> MaterialReferences => Set<MaterialReference>();

    public DbSet<PartReference> PartReferences => Set<PartReference>();

    public DbSet<SystemReference> SystemReferences => Set<SystemReference>();

    public DbSet<AssetReference> AssetReferences => Set<AssetReference>();

    public DbSet<TheoreticalSituation> TheoreticalSituations => Set<TheoreticalSituation>();

    public DbSet<TheoreticalSituationContext> TheoreticalSituationContexts => Set<TheoreticalSituationContext>();

    public DbSet<TheoreticalApplicabilityResult> TheoreticalApplicabilityResults => Set<TheoreticalApplicabilityResult>();

    public DbSet<TheoreticalSituationFact> TheoreticalSituationFacts => Set<TheoreticalSituationFact>();

    public DbSet<TheoreticalSituationIncident> TheoreticalSituationIncidents => Set<TheoreticalSituationIncident>();

    public DbSet<TheoreticalSituationEvaluation> TheoreticalSituationEvaluations => Set<TheoreticalSituationEvaluation>();

    public DbSet<TheoreticalSituationEvaluationDetail> TheoreticalSituationEvaluationDetails => Set<TheoreticalSituationEvaluationDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<VocabularyType>(entity =>
        {
            entity.ToTable("compliancecore_vocabulary_types");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TypeKey).IsUnique();
            entity.HasIndex(x => x.SortOrder);
        });

        modelBuilder.Entity<VocabularyTerm>(entity =>
        {
            entity.ToTable("compliancecore_vocabulary_terms");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TermKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.VocabularyTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TermKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.VocabularyTypeKey });
        });

        modelBuilder.Entity<VocabularyAlias>(entity =>
        {
            entity.ToTable("compliancecore_vocabulary_aliases");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AliasText).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.VocabularyTermId, x.AliasText }).IsUnique();
            entity.HasOne(x => x.VocabularyTerm)
                .WithMany()
                .HasForeignKey(x => x.VocabularyTermId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ComplianceKey>(entity =>
        {
            entity.ToTable("compliancecore_compliance_keys");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        });

        modelBuilder.Entity<MaterialKey>(entity =>
        {
            entity.ToTable("compliancecore_material_keys");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        });

        modelBuilder.Entity<ComplianceCoreAuditEvent>(entity =>
        {
            entity.ToTable("compliancecore_audit_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasMaxLength(128).IsRequired();
            entity.Property(x => x.TargetType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TargetId).HasMaxLength(128);
            entity.Property(x => x.Result).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(64);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.OccurredAt);
        });

        modelBuilder.Entity<GoverningBody>(entity =>
        {
            entity.ToTable("compliancecore_governing_bodies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BodyKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.BodyKey }).IsUnique();
        });

        modelBuilder.Entity<Jurisdiction>(entity =>
        {
            entity.ToTable("compliancecore_jurisdictions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.JurisdictionKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.GoverningBodyId);
            entity.HasIndex(x => new { x.TenantId, x.JurisdictionKey }).IsUnique();
            entity.HasOne(x => x.GoverningBody)
                .WithMany()
                .HasForeignKey(x => x.GoverningBodyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RegulatoryProgram>(entity =>
        {
            entity.ToTable("compliancecore_regulatory_programs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProgramKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.JurisdictionId);
            entity.HasIndex(x => new { x.TenantId, x.ProgramKey }).IsUnique();
            entity.HasOne(x => x.Jurisdiction)
                .WithMany()
                .HasForeignKey(x => x.JurisdictionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RulePack>(entity =>
        {
            entity.ToTable("compliancecore_rule_packs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PackKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RuleContentJson).HasColumnType("jsonb");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.RegulatoryProgramId);
            entity.HasIndex(x => new { x.TenantId, x.PackKey, x.VersionNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.LastScheduledEvaluationAt });
            entity.HasOne(x => x.RegulatoryProgram)
                .WithMany()
                .HasForeignKey(x => x.RegulatoryProgramId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ScheduledRuleEvaluationRun>(entity =>
        {
            entity.ToTable("compliancecore_scheduled_rule_evaluation_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.StartedAt);
        });

        modelBuilder.Entity<RegulatoryCitation>(entity =>
        {
            entity.ToTable("compliancecore_regulatory_citations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CitationKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SourceReference).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.RegulatoryProgramId);
            entity.HasIndex(x => x.RulePackId);
            entity.HasIndex(x => new { x.TenantId, x.CitationKey, x.VersionNumber }).IsUnique();
            entity.HasOne(x => x.RegulatoryProgram)
                .WithMany()
                .HasForeignKey(x => x.RegulatoryProgramId)
                .HasConstraintName("FK_compliancecore_regulatory_citations_program")
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RulePack)
                .WithMany()
                .HasForeignKey(x => x.RulePackId)
                .HasConstraintName("FK_compliancecore_regulatory_citations_rule_pack")
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SupersedesCitation)
                .WithMany()
                .HasForeignKey(x => x.SupersedesCitationId)
                .HasConstraintName("FK_compliancecore_regulatory_citations_supersedes")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FactDefinition>(entity =>
        {
            entity.ToTable("compliancecore_fact_definitions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FactKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ValueType).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.FactKey }).IsUnique();
        });

        modelBuilder.Entity<FactRequirement>(entity =>
        {
            entity.ToTable("compliancecore_fact_requirements");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequirementKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ApplicabilityKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SourceEntity).HasMaxLength(256).IsRequired();
            entity.Property(x => x.SourceFieldOrRecordType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ValueType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Operator).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ExpectedValue).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.EvidenceKind).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RequiredDocumentType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.RetentionPeriod).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AuditQuestion).HasMaxLength(512).IsRequired();
            entity.Property(x => x.FailureSeverity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.OverridePermission).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.FactDefinitionId);
            entity.HasIndex(x => x.RulePackId);
            entity.HasIndex(x => x.CitationId);
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct });
            entity.HasIndex(x => new { x.TenantId, x.SourceEntity });
            entity.HasIndex(x => new { x.TenantId, x.RequirementKey }).IsUnique();
            entity.HasOne(x => x.FactDefinition)
                .WithMany()
                .HasForeignKey(x => x.FactDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RulePack)
                .WithMany()
                .HasForeignKey(x => x.RulePackId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Citation)
                .WithMany()
                .HasForeignKey(x => x.CitationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EvidenceReference>(entity =>
        {
            entity.ToTable("compliancecore_evidence_references");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EvidenceId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.FactKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceEntity).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SourceRecordId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SourceField).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DocumentType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DocumentUrl).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
            entity.Property(x => x.FileHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ReviewStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.EvidenceId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.FactKey });
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct, x.SourceEntity });
        });

        modelBuilder.Entity<FactAssertion>(entity =>
        {
            entity.ToTable("compliancecore_fact_assertions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FactKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SubjectKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SubjectId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Value).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ValueType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceRecordId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EvidenceId).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.FactKey, x.SubjectKind, x.SubjectId });
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct });
            entity.HasOne(x => x.EvidenceReference)
                .WithMany()
                .HasForeignKey(x => x.EvidenceReferenceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditTrace>(entity =>
        {
            entity.ToTable("compliancecore_audit_traces");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AuditTraceId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PackKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FactKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CitationKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SubjectKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SubjectId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EvaluatedValue).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ExpectedValue).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Operator).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Result).HasMaxLength(32).IsRequired();
            entity.Property(x => x.FailureSeverity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.OverrideReason).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ClaimedExceptionExemptionKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ClaimedExceptionExemptionType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExceptionExemptionLegalBasis).HasMaxLength(512).IsRequired();
            entity.Property(x => x.ExceptionExemptionProofKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ExceptionExemptionScopeResult).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExceptionExemptionEffectiveResult).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ResultBeforeException).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ResultAfterException).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FinalComplianceResult).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AuditTraceId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PackKey });
            entity.HasIndex(x => new { x.TenantId, x.FactKey });
            entity.HasIndex(x => new { x.TenantId, x.CitationKey });
            entity.HasIndex(x => new { x.TenantId, x.ClaimedExceptionExemptionKey });
        });

        modelBuilder.Entity<RegulatoryMapping>(entity =>
        {
            entity.ToTable("compliancecore_regulatory_mappings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MappingKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.TargetKind).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.RegulatoryProgramId);
            entity.HasIndex(x => x.RulePackId);
            entity.HasIndex(x => x.CitationId);
            entity.HasIndex(x => x.FactDefinitionId);
            entity.HasIndex(x => x.ComplianceKeyId);
            entity.HasIndex(x => x.MaterialKeyId);
            entity.HasIndex(x => new { x.TenantId, x.MappingKey }).IsUnique();
            entity.HasOne(x => x.RegulatoryProgram)
                .WithMany()
                .HasForeignKey(x => x.RegulatoryProgramId)
                .HasConstraintName("FK_compliancecore_regulatory_mappings_program")
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RulePack)
                .WithMany()
                .HasForeignKey(x => x.RulePackId)
                .HasConstraintName("FK_compliancecore_regulatory_mappings_rule_pack")
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Citation)
                .WithMany()
                .HasForeignKey(x => x.CitationId)
                .HasConstraintName("FK_compliancecore_regulatory_mappings_citation")
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.FactDefinition)
                .WithMany()
                .HasForeignKey(x => x.FactDefinitionId)
                .HasConstraintName("FK_compliancecore_regulatory_mappings_fact_definition")
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ComplianceKey)
                .WithMany()
                .HasForeignKey(x => x.ComplianceKeyId)
                .HasConstraintName("FK_compliancecore_regulatory_mappings_compliance_key")
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MaterialKey)
                .WithMany()
                .HasForeignKey(x => x.MaterialKeyId)
                .HasConstraintName("FK_compliancecore_regulatory_mappings_material_key")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FactSource>(entity =>
        {
            entity.ToTable("compliancecore_fact_sources");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ProductKey).HasMaxLength(32);
            entity.Property(x => x.ProductReference).HasMaxLength(256);
            entity.Property(x => x.ConfigJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.FactDefinitionId);
            entity.HasIndex(x => new { x.TenantId, x.SourceKey }).IsUnique();
            entity.HasOne(x => x.FactDefinition)
                .WithMany()
                .HasForeignKey(x => x.FactDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RiskScoreRun>(entity =>
        {
            entity.ToTable("compliancecore_risk_score_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.HighestRiskLevel).HasMaxLength(16).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.EvaluatedAt });
        });

        modelBuilder.Entity<RiskScore>(entity =>
        {
            entity.ToTable("compliancecore_risk_scores");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PackKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RiskLevel).HasMaxLength(16).IsRequired();
            entity.Property(x => x.RuleOutcome).HasMaxLength(16).IsRequired();
            entity.Property(x => x.EvaluationResult).HasMaxLength(16).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.RunId);
            entity.HasIndex(x => new { x.TenantId, x.ScopeKey, x.PackKey, x.EvaluatedAt });
            entity.HasOne(x => x.Run)
                .WithMany()
                .HasForeignKey(x => x.RunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MissingEvidenceWarningRun>(entity =>
        {
            entity.ToTable("compliancecore_missing_evidence_warning_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.HighestSeverity).HasMaxLength(16).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.EvaluatedAt });
        });

        modelBuilder.Entity<MissingEvidenceWarning>(entity =>
        {
            entity.ToTable("compliancecore_missing_evidence_warnings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PackKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FactKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.WarningType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Severity).HasMaxLength(16).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.RunId);
            entity.HasIndex(x => new { x.TenantId, x.ScopeKey, x.PackKey, x.Severity, x.EvaluatedAt });
            entity.HasOne(x => x.Run)
                .WithMany()
                .HasForeignKey(x => x.RunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ControlEffectivenessRun>(entity =>
        {
            entity.ToTable("compliancecore_control_effectiveness_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.LowestEffectivenessLevel).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.EvaluatedAt });
        });

        modelBuilder.Entity<ControlEffectivenessRecord>(entity =>
        {
            entity.ToTable("compliancecore_control_effectiveness_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PackKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EffectivenessLevel).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ControlStatus).HasMaxLength(16).IsRequired();
            entity.Property(x => x.RuleOutcome).HasMaxLength(16).IsRequired();
            entity.Property(x => x.EvaluationResult).HasMaxLength(16).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.RunId);
            entity.HasIndex(x => new { x.TenantId, x.ScopeKey, x.PackKey, x.EvaluatedAt });
            entity.HasOne(x => x.Run)
                .WithMany()
                .HasForeignKey(x => x.RunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReadinessForecastRun>(entity =>
        {
            entity.ToTable("compliancecore_readiness_forecast_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ReadinessLevel).HasMaxLength(16).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ForecastedAt });
        });

        modelBuilder.Entity<ReadinessForecast>(entity =>
        {
            entity.ToTable("compliancecore_readiness_forecasts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PackKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ReadinessLevel).HasMaxLength(16).IsRequired();
            entity.Property(x => x.RiskLevel).HasMaxLength(16).IsRequired();
            entity.Property(x => x.EffectivenessLevel).HasMaxLength(32).IsRequired();
            entity.Property(x => x.HighestMissingEvidenceSeverity).HasMaxLength(16).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.RunId);
            entity.HasIndex(x => new { x.TenantId, x.ScopeKey, x.PackKey, x.ForecastedAt });
            entity.HasOne(x => x.Run)
                .WithMany()
                .HasForeignKey(x => x.RunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RuleChangeEvent>(entity =>
        {
            entity.ToTable("compliancecore_rule_change_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PackKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ProgramKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ChangeType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(512).IsRequired();
            entity.Property(x => x.FromStatus).HasMaxLength(32);
            entity.Property(x => x.ToStatus).HasMaxLength(32);
            entity.Property(x => x.PreviousContentHash).HasMaxLength(64);
            entity.Property(x => x.NewContentHash).HasMaxLength(64);
            entity.Property(x => x.Source).HasMaxLength(16).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PackKey, x.DetectedAt });
            entity.HasIndex(x => x.RulePackId);
            entity.HasOne(x => x.ScanRun)
                .WithMany()
                .HasForeignKey(x => x.ScanRunId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RulePackMonitorSnapshot>(entity =>
        {
            entity.ToTable("compliancecore_rule_pack_monitor_snapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PackKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ContentHash).HasMaxLength(64);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.RulePackId).IsUnique();
        });

        modelBuilder.Entity<RuleChangeScanRun>(entity =>
        {
            entity.ToTable("compliancecore_rule_change_scan_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(16).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.StartedAt);
        });

        modelBuilder.Entity<SourceIngestionBatch>(entity =>
        {
            entity.ToTable("compliancecore_source_ingestion_batches");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IngestionType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Phase).HasMaxLength(16).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(16).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(32);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IngestionType, x.CreatedAt });
        });

        modelBuilder.Entity<SourceIngestionJob>(entity =>
        {
            entity.ToTable("compliancecore_source_ingestion_jobs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.JobKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(16).IsRequired();
            entity.Property(x => x.EntityType).HasMaxLength(64);
            entity.Property(x => x.ErrorCode).HasMaxLength(64);
            entity.Property(x => x.Message).HasMaxLength(512);
            entity.HasIndex(x => x.BatchId);
            entity.HasOne(x => x.Batch)
                .WithMany(x => x.Jobs)
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductFactMirror>(entity =>
        {
            entity.ToTable("compliancecore_product_fact_mirrors");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceProduct).HasMaxLength(32).IsRequired();
            entity.Property(x => x.FactKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ScopeKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ValueType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.StringValue).HasMaxLength(512);
            entity.Property(x => x.SourceEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceEventKind).HasMaxLength(128).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct, x.FactKey, x.ScopeKey });
        });

        modelBuilder.Entity<RuleEvaluationRun>(entity =>
        {
            entity.ToTable("compliancecore_rule_evaluation_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.OverallResult).HasMaxLength(16).IsRequired();
            entity.Property(x => x.FactInputsJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.RuleResultsJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.AppliedWaiverKey).HasMaxLength(64);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.RulePackId);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.AppliedWaiverId);
            entity.HasOne(x => x.RulePack)
                .WithMany()
                .HasForeignKey(x => x.RulePackId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ComplianceFinding>(entity =>
        {
            entity.ToTable("compliancecore_findings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FindingKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Severity).HasMaxLength(16).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RuleKey).HasMaxLength(64);
            entity.Property(x => x.FactKey).HasMaxLength(64);
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.RulePackId);
            entity.HasIndex(x => x.RuleEvaluationRunId);
            entity.HasIndex(x => new { x.TenantId, x.FindingKey });
            entity.HasIndex(x => x.CreatedAt);
            entity.HasOne(x => x.RulePack)
                .WithMany()
                .HasForeignKey(x => x.RulePackId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RuleEvaluationRun)
                .WithMany()
                .HasForeignKey(x => x.RuleEvaluationRunId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkflowGateDefinition>(entity =>
        {
            entity.ToTable("compliancecore_workflow_gate_definitions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.GateKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.GateKey }).IsUnique();
            entity.HasIndex(x => x.RulePackId);
            entity.HasOne(x => x.RulePack)
                .WithMany()
                .HasForeignKey(x => x.RulePackId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkflowGateCheckResult>(entity =>
        {
            entity.ToTable("compliancecore_workflow_gate_check_results");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.GateKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Outcome).HasMaxLength(16).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ReasonsJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.ContextJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.AppliedWaiverKey).HasMaxLength(64);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.WorkflowGateDefinitionId);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.AppliedWaiverId);
            entity.HasOne(x => x.WorkflowGateDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowGateDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RuleEvaluationRun)
                .WithMany()
                .HasForeignKey(x => x.RuleEvaluationRunId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProductGateResponse>(entity =>
        {
            entity.ToTable("compliancecore_product_gate_responses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceProduct).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ResponseOutcome).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ResponseCode).HasMaxLength(64);
            entity.Property(x => x.ResponseMessage).HasMaxLength(1024);
            entity.Property(x => x.ResponsePayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.WorkflowGateCheckResultId);
            entity.HasIndex(x => new { x.TenantId, x.WorkflowGateCheckResultId, x.RespondedAt });
            entity.HasOne(x => x.WorkflowGateCheckResult)
                .WithMany()
                .HasForeignKey(x => x.WorkflowGateCheckResultId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SdsReference>(entity =>
        {
            entity.ToTable("compliancecore_sds_references");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SdsKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ProductName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Manufacturer).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DocumentUrl).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.MaterialKeyId);
            entity.HasIndex(x => new { x.TenantId, x.SdsKey }).IsUnique();
            entity.HasOne(x => x.MaterialKey)
                .WithMany()
                .HasForeignKey(x => x.MaterialKeyId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<HazComReference>(entity =>
        {
            entity.ToTable("compliancecore_hazcom_references");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.HazComKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.LinkedSdsKey).HasMaxLength(64);
            entity.Property(x => x.StaffarrSiteNameSnapshot).HasMaxLength(256).HasDefaultValue(string.Empty).IsRequired();
            entity.Property(x => x.LocationRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DocumentUrl).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.HazComKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.StaffarrSiteOrgUnitId });
        });

        modelBuilder.Entity<TenantM12AnalyticsWorkerSettings>(entity =>
        {
            entity.ToTable("compliancecore_tenant_m12_analytics_worker_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DefaultScopeKey).HasMaxLength(M12AnalyticsBatchRules.MaxScopeKeyLength).IsRequired();
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<TenantFactSourceSyncWorkerSettings>(entity =>
        {
            entity.ToTable("compliancecore_tenant_fact_source_sync_worker_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DefaultScopeKey).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<FactSourceSyncStatus>(entity =>
        {
            entity.ToTable("compliancecore_fact_source_sync_statuses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.HealthStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.LastErrorMessage).HasMaxLength(2000);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.FactSourceId).IsUnique();
            entity.HasOne(x => x.FactSource)
                .WithMany()
                .HasForeignKey(x => x.FactSourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<M12AnalyticsBatchRun>(entity =>
        {
            entity.ToTable("compliancecore_m12_analytics_batch_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ScopeKey).HasMaxLength(M12AnalyticsBatchRules.MaxScopeKeyLength).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
            entity.HasIndex(x => new { x.TenantId, x.StartedAt });
        });

        modelBuilder.Entity<AuditPackageGenerationJob>(entity =>
        {
            entity.ToTable("compliancecore_audit_package_generation_jobs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Format).HasMaxLength(16).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
            entity.Property(x => x.ArtifactZip);
            entity.Property(x => x.ArtifactJson);
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
            entity.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<ComplianceWaiver>(entity =>
        {
            entity.ToTable("compliancecore_waivers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.WaiverKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PackKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RuleKey).HasMaxLength(128);
            entity.Property(x => x.GateKey).HasMaxLength(128);
            entity.Property(x => x.SubjectScopeKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Explanation).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.WaiverKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ExpiresAt });
            entity.HasOne(x => x.RulePack)
                .WithMany()
                .HasForeignKey(x => x.RulePackId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RuleTestCase>(entity =>
        {
            entity.ToTable("compliancecore_rule_test_cases");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RuleKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TestKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ExpectedResult).HasMaxLength(16).IsRequired();
            entity.Property(x => x.FactsJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.RulePackId);
            entity.HasIndex(x => new { x.TenantId, x.RulePackId, x.TestKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RulePackId, x.RuleKey });
            entity.HasOne(x => x.RulePack)
                .WithMany()
                .HasForeignKey(x => x.RulePackId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ComplianceExceptionExemption>(entity =>
        {
            entity.ToTable("compliance_exception_exemption");
            entity.HasKey(x => x.ExceptionExemptionId);
            entity.Property(x => x.Key).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(64).IsRequired();
            entity.Property(x => x.GoverningBody).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ProgramKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PackKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CitationKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ApplicabilityKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AppliesToSubjectKind).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AppliesToSourceProduct).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AppliesToSourceEntity).HasMaxLength(256).IsRequired();
            entity.Property(x => x.EffectType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ConditionLogicJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.IssuingAuthority).HasMaxLength(256).IsRequired();
            entity.Property(x => x.AuthorizationNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ProgramKey, x.PackKey, x.CitationKey });
            entity.HasIndex(x => new { x.TenantId, x.Type });
            entity.HasIndex(x => new { x.TenantId, x.Active, x.ExpiresAt });
            entity.HasOne(x => x.RequiredEvidenceOptionGroup)
                .WithMany()
                .HasForeignKey(x => x.RequiredEvidenceOptionGroupId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ImportSession>(entity =>
        {
            entity.ToTable("compliancecore_import_sessions");
            entity.HasKey(x => x.ImportSessionId);
            entity.Property(x => x.SourceFilename).HasMaxLength(512).IsRequired();
            entity.Property(x => x.SourceHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ImportType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ValidationStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.MappingStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CommitStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(2000).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.SourceHash });
        });

        modelBuilder.Entity<ImportSessionSourceFile>(entity =>
        {
            entity.ToTable("compliancecore_import_session_source_files");
            entity.HasKey(x => x.ImportSessionSourceFileId);
            entity.Property(x => x.SourceFile).HasMaxLength(256).IsRequired();
            entity.Property(x => x.OriginalFilename).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Content).IsRequired();
            entity.Property(x => x.FileHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ValidationStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ValidationErrorsJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.ImportSessionId);
            entity.HasIndex(x => new { x.ImportSessionId, x.SourceFile }).IsUnique();
            entity.HasOne(x => x.ImportSession)
                .WithMany()
                .HasForeignKey(x => x.ImportSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        ConfigureStagedRow<ImportStagedRulePack>(modelBuilder, "compliancecore_import_staged_rule_packs");
        ConfigureStagedRow<ImportStagedRuleRequirement>(modelBuilder, "compliancecore_import_staged_rule_requirements");
        ConfigureStagedRow<ImportStagedFactRequirement>(modelBuilder, "compliancecore_import_staged_fact_requirements");
        ConfigureStagedRow<ImportStagedRegulatoryMapping>(modelBuilder, "compliancecore_import_staged_regulatory_mappings");
        ConfigureStagedRow<ImportStagedControlledVocabulary>(modelBuilder, "compliancecore_import_staged_controlled_vocabulary");
        ConfigureStagedRow<ImportStagedVocabularyAlias>(modelBuilder, "compliancecore_import_staged_vocabulary_aliases");
        ConfigureStagedRow<ImportStagedComplianceKey>(modelBuilder, "compliancecore_import_staged_compliance_keys");
        ConfigureStagedRow<ImportStagedMaterialKey>(modelBuilder, "compliancecore_import_staged_material_keys");
        ConfigureStagedRow<ImportStagedSdsReference>(modelBuilder, "compliancecore_import_staged_sds_references");
        ConfigureStagedRow<ImportStagedEvidenceReference>(modelBuilder, "compliancecore_import_staged_evidence_references");
        ConfigureStagedRow<ImportStagedExceptionExemption>(modelBuilder, "compliancecore_import_staged_exception_exemptions");

        modelBuilder.Entity<ImportStagedMappingCandidate>(entity =>
        {
            entity.ToTable("compliancecore_import_staged_mapping_candidates");
            entity.HasKey(x => x.MappingCandidateId);
            entity.Property(x => x.StagedSourceFile).HasMaxLength(256).IsRequired();
            entity.Property(x => x.SourceKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.SourceLabel).HasMaxLength(512).IsRequired();
            entity.Property(x => x.EvidenceOptionKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.EvidenceOptionLabel).HasMaxLength(512).IsRequired();
            entity.Property(x => x.OptionLogicGroup).HasMaxLength(32).IsRequired();
            entity.Property(x => x.TargetKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TargetId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.TargetKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.TargetLabel).HasMaxLength(512).IsRequired();
            entity.Property(x => x.ConfidenceScore).HasPrecision(5, 3);
            entity.Property(x => x.ConfidenceBand).HasMaxLength(32).IsRequired();
            entity.Property(x => x.MatchReasonsJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.RiskFlagsJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.ProposedAction).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.ImportSessionId);
            entity.HasIndex(x => x.StagedRowId);
            entity.HasIndex(x => new { x.ImportSessionId, x.ConfidenceBand });
            entity.HasOne(x => x.ImportSession)
                .WithMany()
                .HasForeignKey(x => x.ImportSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ImportStagedMappingDecision>(entity =>
        {
            entity.ToTable("compliancecore_import_staged_mapping_decisions");
            entity.HasKey(x => x.MappingDecisionId);
            entity.Property(x => x.Decision).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SelectedEvidenceOptionKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.SelectedTargetKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SelectedTargetId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.SelectedTargetKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CreateNewPayloadJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.EvidenceMappingPurpose).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExceptionExemptionKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ResidualRequirementsJson).HasColumnType("jsonb").IsRequired().HasDefaultValue("[]");
            entity.Property(x => x.OverrideReason).HasMaxLength(2000).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.ImportSessionId);
            entity.HasIndex(x => x.StagedRowId);
            entity.HasIndex(x => x.MappingCandidateId);
            entity.HasOne(x => x.ImportSession)
                .WithMany()
                .HasForeignKey(x => x.ImportSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.MappingCandidate)
                .WithMany()
                .HasForeignKey(x => x.MappingCandidateId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ComplianceEvidenceOptionGroup>(entity =>
        {
            entity.ToTable("compliancecore_evidence_option_groups");
            entity.HasKey(x => x.EvidenceOptionGroupId);
            entity.Property(x => x.RequirementKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.FactKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PackKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CitationKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.LogicType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ApplicabilityKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RequirementKey, x.FactKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PackKey });
            entity.HasIndex(x => new { x.TenantId, x.CitationKey });
        });

        modelBuilder.Entity<ComplianceEvidenceOption>(entity =>
        {
            entity.ToTable("compliancecore_evidence_options");
            entity.HasKey(x => x.EvidenceOptionId);
            entity.Property(x => x.OptionKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.OptionLabel).HasMaxLength(256).IsRequired();
            entity.Property(x => x.EvidenceKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TargetKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SourceEntity).HasMaxLength(256).IsRequired();
            entity.Property(x => x.SourceFieldOrRecordType).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DocumentTypeKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.MaterialKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PartKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SystemKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AssetKind).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ExternalRegistryKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.FactKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ConfidenceHint).HasPrecision(5, 3);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.EvidenceOptionGroupId);
            entity.HasIndex(x => new { x.TenantId, x.OptionKey }).IsUnique();
            entity.HasOne(x => x.EvidenceOptionGroup)
                .WithMany()
                .HasForeignKey(x => x.EvidenceOptionGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        ConfigureReference<ExternalObjectReference>(modelBuilder, "compliancecore_external_object_references");
        ConfigureReference<DocumentReference>(modelBuilder, "compliancecore_document_references");
        ConfigureReference<MaterialReference>(modelBuilder, "compliancecore_material_references");
        ConfigureReference<PartReference>(modelBuilder, "compliancecore_part_references");
        ConfigureReference<SystemReference>(modelBuilder, "compliancecore_system_references");
        ConfigureReference<AssetReference>(modelBuilder, "compliancecore_asset_references");

        modelBuilder.Entity<TheoreticalSituation>(entity =>
        {
            entity.ToTable("compliancecore_theoretical_situations");
            entity.HasKey(x => x.SituationId);
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.SituationKind).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.EvaluationMode).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SituationKind });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
        });

        modelBuilder.Entity<TheoreticalSituationContext>(entity =>
        {
            entity.ToTable("compliancecore_theoretical_situation_contexts");
            entity.HasKey(x => x.ContextId);
            entity.Property(x => x.ContextKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ContextLabel).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ContextValueKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ContextValueLabel).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ControlledVocabularyType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Confidence).HasPrecision(5, 3);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.SituationId);
            entity.HasIndex(x => new { x.SituationId, x.ContextKey }).IsUnique();
            entity.HasOne(x => x.Situation)
                .WithMany()
                .HasForeignKey(x => x.SituationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TheoreticalApplicabilityResult>(entity =>
        {
            entity.ToTable("compliancecore_theoretical_applicability_results");
            entity.HasKey(x => x.ApplicabilityResultId);
            entity.Property(x => x.ProgramKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PackKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CitationKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ApplicabilityScore).HasPrecision(5, 3);
            entity.Property(x => x.ApplicabilityBand).HasMaxLength(32).IsRequired();
            entity.Property(x => x.MatchReasonsJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.MissingContextJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.ExclusionReasonsJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.EdgeCaseReason).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.SituationId);
            entity.HasIndex(x => new { x.SituationId, x.ApplicabilityBand, x.UserVisiblePriority });
            entity.HasOne(x => x.Situation)
                .WithMany()
                .HasForeignKey(x => x.SituationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TheoreticalSituationFact>(entity =>
        {
            entity.ToTable("compliancecore_theoretical_situation_facts");
            entity.HasKey(x => x.SituationFactId);
            entity.Property(x => x.FactKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.RequirementKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CitationKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PackKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SimulatedValue).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ValueType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SimulatedState).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EvidenceOptionKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EvidenceKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TargetKind).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.SituationId);
            entity.HasIndex(x => new { x.SituationId, x.FactKey, x.RequirementKey, x.EvidenceOptionKey });
            entity.HasOne(x => x.Situation)
                .WithMany()
                .HasForeignKey(x => x.SituationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TheoreticalSituationIncident>(entity =>
        {
            entity.ToTable("compliancecore_theoretical_situation_incidents");
            entity.HasKey(x => x.SituationIncidentId);
            entity.Property(x => x.IncidentTypeKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SeverityKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.InvolvedSubjectKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.InvolvedSubjectState).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TriggerKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.TriggerValue).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ReportabilityState).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RemediationState).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.SituationId);
            entity.HasOne(x => x.Situation)
                .WithMany()
                .HasForeignKey(x => x.SituationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TheoreticalSituationEvaluation>(entity =>
        {
            entity.ToTable("compliancecore_theoretical_situation_evaluations");
            entity.HasKey(x => x.EvaluationId);
            entity.Property(x => x.Result).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.PrimaryProgramsJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.LikelyProgramsJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.EdgeCasesJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.SituationId);
            entity.HasIndex(x => new { x.SituationId, x.EvaluatedAt });
            entity.HasOne(x => x.Situation)
                .WithMany()
                .HasForeignKey(x => x.SituationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TheoreticalSituationEvaluationDetail>(entity =>
        {
            entity.ToTable("compliancecore_theoretical_situation_evaluation_details");
            entity.HasKey(x => x.DetailId);
            entity.Property(x => x.RequirementKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.FactKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CitationKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PackKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AuditQuestion).HasMaxLength(512).IsRequired();
            entity.Property(x => x.SimulatedState).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExpectedValue).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.ActualValue).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Operator).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Result).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FailureSeverity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.OverridePermission).HasMaxLength(128).IsRequired();
            entity.Property(x => x.NormalRuleResult).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExceptionExemptionKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ExceptionExemptionType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExceptionExemptionLabel).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ResultBeforeException).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ResultAfterException).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FinalComplianceResult).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Explanation).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.SuggestedNextAction).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.EvaluationId);
            entity.HasIndex(x => new { x.EvaluationId, x.VisiblePriority });
            entity.HasOne(x => x.Evaluation)
                .WithMany()
                .HasForeignKey(x => x.EvaluationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureStagedRow<TEntity>(ModelBuilder modelBuilder, string tableName)
        where TEntity : ImportStagedRowBase
    {
        modelBuilder.Entity<TEntity>(entity =>
        {
            entity.ToTable(tableName);
            entity.HasKey(x => x.StagedRowId);
            entity.Property(x => x.SourceFile).HasMaxLength(256).IsRequired();
            entity.Property(x => x.RawRowJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.NormalizedRowJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.RowHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ValidationStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ValidationErrorsJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.CanonicalKeyCandidate).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.ImportSessionId);
            entity.HasIndex(x => new { x.ImportSessionId, x.SourceFile, x.RowNumber });
            entity.HasIndex(x => new { x.ImportSessionId, x.CanonicalKeyCandidate });
            entity.HasOne(x => x.ImportSession)
                .WithMany()
                .HasForeignKey(x => x.ImportSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateImmutableSnapshots();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ValidateImmutableSnapshots();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ValidateImmutableSnapshots()
    {
        var changedSnapshots = ChangeTracker.Entries()
            .Where(entry =>
                entry.Entity is RuleEvaluationRun or ComplianceCoreAuditEvent &&
                entry.State is EntityState.Modified or EntityState.Deleted)
            .Select(entry => entry.Entity.GetType().Name)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (changedSnapshots.Length > 0)
        {
            throw new InvalidOperationException(
                $"Immutable compliance snapshots cannot be modified or deleted: {string.Join(", ", changedSnapshots)}.");
        }
    }

    private static void ConfigureReference<TEntity>(ModelBuilder modelBuilder, string tableName)
        where TEntity : ProductObjectReferenceBase
    {
        modelBuilder.Entity<TEntity>(entity =>
        {
            entity.ToTable(tableName);
            entity.HasKey(x => x.ReferenceId);
            entity.Property(x => x.SourceProduct).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ObjectKind).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ExternalRecordId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.StableKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.MetadataJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct, x.ObjectKind });
            entity.HasIndex(x => new { x.TenantId, x.StableKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct, x.ExternalRecordId });
        });
    }
}
