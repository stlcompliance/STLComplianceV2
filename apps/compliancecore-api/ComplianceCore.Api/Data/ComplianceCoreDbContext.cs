using Microsoft.EntityFrameworkCore;
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

    public DbSet<RegulatoryMapping> RegulatoryMappings => Set<RegulatoryMapping>();

    public DbSet<RuleEvaluationRun> RuleEvaluationRuns => Set<RuleEvaluationRun>();

    public DbSet<ScheduledRuleEvaluationRun> ScheduledRuleEvaluationRuns => Set<ScheduledRuleEvaluationRun>();

    public DbSet<FactSource> FactSources => Set<FactSource>();

    public DbSet<ProductFactMirror> ProductFactMirrors => Set<ProductFactMirror>();

    public DbSet<ComplianceFinding> ComplianceFindings => Set<ComplianceFinding>();

    public DbSet<WorkflowGateDefinition> WorkflowGateDefinitions => Set<WorkflowGateDefinition>();

    public DbSet<WorkflowGateCheckResult> WorkflowGateCheckResults => Set<WorkflowGateCheckResult>();

    public DbSet<SdsReference> SdsReferences => Set<SdsReference>();

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
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.FactDefinitionId);
            entity.HasIndex(x => x.RulePackId);
            entity.HasIndex(x => x.CitationId);
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
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.RulePackId);
            entity.HasIndex(x => x.CreatedAt);
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
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.WorkflowGateDefinitionId);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasOne(x => x.WorkflowGateDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowGateDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RuleEvaluationRun)
                .WithMany()
                .HasForeignKey(x => x.RuleEvaluationRunId)
                .OnDelete(DeleteBehavior.SetNull);
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

        modelBuilder.Entity<TenantM12AnalyticsWorkerSettings>(entity =>
        {
            entity.ToTable("compliancecore_tenant_m12_analytics_worker_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DefaultScopeKey).HasMaxLength(M12AnalyticsBatchRules.MaxScopeKeyLength).IsRequired();
            entity.HasIndex(x => x.TenantId).IsUnique();
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
    }
}
