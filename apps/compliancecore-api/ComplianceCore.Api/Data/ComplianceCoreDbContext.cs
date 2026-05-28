using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Entities;
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
