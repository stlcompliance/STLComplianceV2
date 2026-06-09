using Microsoft.EntityFrameworkCore;
using NexArr.Api.Entities;
using STLCompliance.Shared.Data;

namespace NexArr.Api.Data;

public sealed class NexArrDbContext(DbContextOptions<NexArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<PlatformUser> Users => Set<PlatformUser>();
    public DbSet<UserCredential> UserCredentials => Set<UserCredential>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();
    public DbSet<PlatformRoleAssignment> PlatformRoleAssignments => Set<PlatformRoleAssignment>();
    public DbSet<ExternalIdentityProviderMapping> ExternalIdentityProviderMappings => Set<ExternalIdentityProviderMapping>();
    public DbSet<ProductCatalogItem> ProductCatalog => Set<ProductCatalogItem>();
    public DbSet<TenantProductEntitlement> Entitlements => Set<TenantProductEntitlement>();
    public DbSet<PlatformAuditEvent> AuditEvents => Set<PlatformAuditEvent>();
    public DbSet<ServiceClient> ServiceClients => Set<ServiceClient>();
    public DbSet<ServiceTokenRecord> ServiceTokens => Set<ServiceTokenRecord>();
    public DbSet<ProductLaunchProfile> LaunchProfiles => Set<ProductLaunchProfile>();
    public DbSet<HandoffCodeRecord> HandoffCodes => Set<HandoffCodeRecord>();
    public DbSet<ReferenceDataset> ReferenceDatasets => Set<ReferenceDataset>();
    public DbSet<ReferenceSource> ReferenceSources => Set<ReferenceSource>();
    public DbSet<IngestionJob> IngestionJobs => Set<IngestionJob>();
    public DbSet<StagingRecord> StagingRecords => Set<StagingRecord>();
    public DbSet<ReferenceEntity> ReferenceEntities => Set<ReferenceEntity>();
    public DbSet<ReferenceEntityVersion> ReferenceEntityVersions => Set<ReferenceEntityVersion>();
    public DbSet<ReferenceCrosswalk> ReferenceCrosswalks => Set<ReferenceCrosswalk>();
    public DbSet<TenantReferenceOverlay> TenantReferenceOverlays => Set<TenantReferenceOverlay>();
    public DbSet<ProductMapping> ProductMappings => Set<ProductMapping>();
    public DbSet<ReferencePublishEvent> ReferencePublishEvents => Set<ReferencePublishEvent>();
    public DbSet<ReferenceAuditEvent> ReferenceAuditEvents => Set<ReferenceAuditEvent>();

    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<ProductCallbackAllowlistEntry> CallbackAllowlist => Set<ProductCallbackAllowlistEntry>();

    public DbSet<TenantFieldCompanionNotificationSettings> TenantFieldCompanionNotificationSettings =>
        Set<TenantFieldCompanionNotificationSettings>();

    public DbSet<FieldCompanionNotificationDispatch> FieldCompanionNotificationDispatches =>
        Set<FieldCompanionNotificationDispatch>();

    public DbSet<FieldCompanionPushSubscription> FieldCompanionPushSubscriptions => Set<FieldCompanionPushSubscription>();

    public DbSet<PlatformAuditPackageGenerationJob> PlatformAuditPackageGenerationJobs =>
        Set<PlatformAuditPackageGenerationJob>();

    public DbSet<PlatformServiceTokenCleanupSettings> PlatformServiceTokenCleanupSettings =>
        Set<PlatformServiceTokenCleanupSettings>();

    public DbSet<PlatformSessionSettings> PlatformSessionSettings => Set<PlatformSessionSettings>();

    public DbSet<ServiceTokenCleanupRun> ServiceTokenCleanupRuns => Set<ServiceTokenCleanupRun>();

    public DbSet<TenantProductLicense> TenantProductLicenses => Set<TenantProductLicense>();

    public DbSet<PlatformEntitlementReconciliationSettings> PlatformEntitlementReconciliationSettings =>
        Set<PlatformEntitlementReconciliationSettings>();

    public DbSet<EntitlementReconciliationRun> EntitlementReconciliationRuns =>
        Set<EntitlementReconciliationRun>();

    public DbSet<PlatformTenantLifecycleSettings> PlatformTenantLifecycleSettings =>
        Set<PlatformTenantLifecycleSettings>();

    public DbSet<TenantLifecycleRun> TenantLifecycleRuns => Set<TenantLifecycleRun>();

    public DbSet<FieldCompanionOfflineAction> FieldCompanionOfflineActions => Set<FieldCompanionOfflineAction>();
    public DbSet<FieldCompanionFieldSubmission> FieldCompanionFieldSubmissions => Set<FieldCompanionFieldSubmission>();

    public DbSet<TenantProductDataPlaneProfile> DataPlaneProfiles => Set<TenantProductDataPlaneProfile>();

    public DbSet<PlatformOutboxEvent> PlatformOutboxEvents => Set<PlatformOutboxEvent>();

    public DbSet<PlatformOutboxPublisherSettings> PlatformOutboxPublisherSettings =>
        Set<PlatformOutboxPublisherSettings>();

    public DbSet<PlatformOutboxPublisherRun> PlatformOutboxPublisherRuns => Set<PlatformOutboxPublisherRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PlatformUser>(entity =>
        {
            entity.ToTable("platform_users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<UserCredential>(entity =>
        {
            entity.ToTable("user_credentials");
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
            entity.Property(x => x.IsEmailVerified).HasDefaultValue(true);
            entity.Property(x => x.IsMfaEnabled).HasDefaultValue(false);
            entity.Property(x => x.MfaSecret).HasMaxLength(128);
            entity.Property(x => x.MfaRecoveryCodeHashesJson).HasMaxLength(4096);
            entity.Property(x => x.FailedLoginCount).HasDefaultValue(0);
            entity.HasOne(x => x.User).WithOne(x => x.Credential).HasForeignKey<UserCredential>(x => x.UserId);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.ToTable("user_sessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RefreshTokenHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.IsRemembered).HasDefaultValue(false);
            entity.HasIndex(x => x.RefreshTokenHash);
            entity.HasIndex(x => x.UserId);
            entity.HasOne(x => x.User).WithMany(x => x.Sessions).HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Slug).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SubscriptionTier).HasMaxLength(32).IsRequired();
            entity.Property(x => x.BillingCustomerId).HasMaxLength(128);
            entity.Property(x => x.BillingSubscriptionId).HasMaxLength(128);
            entity.Property(x => x.BillingGraceDays);
            entity.Property(x => x.IsTrial).HasDefaultValue(false);
            entity.Property(x => x.IsInternalTenant).HasDefaultValue(false);
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<TenantMembership>(entity =>
        {
            entity.ToTable("tenant_memberships");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RoleKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();
            entity.HasOne(x => x.Tenant).WithMany(x => x.Memberships).HasForeignKey(x => x.TenantId);
            entity.HasOne(x => x.User).WithMany(x => x.Memberships).HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<PlatformRoleAssignment>(entity =>
        {
            entity.ToTable("platform_role_assignments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RoleKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.UserId, x.RoleKey, x.TenantId }).IsUnique();
            entity.HasOne(x => x.User).WithMany(x => x.RoleAssignments).HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<ExternalIdentityProviderMapping>(entity =>
        {
            entity.ToTable("external_identity_provider_mappings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProviderKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExternalSubject).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ExternalEmail).HasMaxLength(320);
            entity.HasIndex(x => new { x.ProviderKey, x.ExternalSubject }).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.ProviderKey }).IsUnique();
            entity.HasOne(x => x.User).WithMany(x => x.ExternalIdentityProviderMappings).HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<ProductCatalogItem>(entity =>
        {
            entity.ToTable("product_catalog");
            entity.HasKey(x => x.ProductKey);
            entity.Property(x => x.ProductKey).HasMaxLength(64);
            entity.Property(x => x.DisplayName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ProductCategory).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ProductOwner).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ProductStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CanonicalCallbackPath).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ApiBaseUrl).HasMaxLength(512).IsRequired();
            entity.Property(x => x.HealthUrl).HasMaxLength(512).IsRequired();
            entity.Property(x => x.ServiceAudience).HasMaxLength(128).IsRequired();
            entity.Property(x => x.MarketingUrl).HasMaxLength(512).IsRequired();
            entity.Property(x => x.DocumentationUrl).HasMaxLength(512).IsRequired();
            entity.Property(x => x.SupportUrl).HasMaxLength(512).IsRequired();
            entity.Property(x => x.EnvironmentKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EntitlementDependencyRules).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.ProductDependencyMetadata).HasMaxLength(2048).IsRequired();
        });

        modelBuilder.Entity<TenantProductEntitlement>(entity =>
        {
            entity.ToTable("tenant_product_entitlements");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.ProductKey }).IsUnique();
            entity.HasOne(x => x.Tenant).WithMany(x => x.Entitlements).HasForeignKey(x => x.TenantId);
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductKey);
        });

        modelBuilder.Entity<PlatformAuditEvent>(entity =>
        {
            entity.ToTable("platform_audit_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasMaxLength(128).IsRequired();
            entity.Property(x => x.TargetType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TargetId).HasMaxLength(128);
            entity.Property(x => x.Result).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(64);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.OccurredAt);
        });

        modelBuilder.Entity<ServiceClient>(entity =>
        {
            entity.ToTable("service_clients");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ClientKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SourceProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AllowedProductKeys).HasMaxLength(512).IsRequired();
            entity.Property(x => x.AllowedTenantIds).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.FailedAuthenticationAttempts).HasDefaultValue(0);
            entity.HasIndex(x => x.ClientKey).IsUnique();
            entity.HasOne(x => x.SourceProduct).WithMany().HasForeignKey(x => x.SourceProductKey);
        });

        modelBuilder.Entity<ServiceTokenRecord>(entity =>
        {
            entity.ToTable("service_tokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Jti).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AllowedProductKeys).HasMaxLength(512).IsRequired();
            entity.Property(x => x.ActionScope).HasMaxLength(512);
            entity.HasIndex(x => x.Jti).IsUnique();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.ExpiresAt);
            entity.HasIndex(x => x.RevokedAt);
            entity.HasOne(x => x.ServiceClient).WithMany(x => x.Tokens).HasForeignKey(x => x.ServiceClientId);
        });

        modelBuilder.Entity<PlatformServiceTokenCleanupSettings>(entity =>
        {
            entity.ToTable("nexarr_platform_service_token_cleanup_settings");
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<PlatformSessionSettings>(entity =>
        {
            entity.ToTable("nexarr_platform_session_settings");
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<ServiceTokenCleanupRun>(entity =>
        {
            entity.ToTable("nexarr_service_token_cleanup_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Outcome).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SkipReason).HasMaxLength(512);
            entity.HasIndex(x => x.ProcessedAt);
        });

        modelBuilder.Entity<TenantProductLicense>(entity =>
        {
            entity.ToTable("nexarr_tenant_product_licenses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ExternalReference).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.ProductKey }).IsUnique();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.ValidTo);
            entity.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductKey);
        });

        modelBuilder.Entity<PlatformEntitlementReconciliationSettings>(entity =>
        {
            entity.ToTable("nexarr_platform_entitlement_reconciliation_settings");
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<EntitlementReconciliationRun>(entity =>
        {
            entity.ToTable("nexarr_entitlement_reconciliation_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Outcome).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SkipReason).HasMaxLength(512);
            entity.HasIndex(x => x.ProcessedAt);
        });

        modelBuilder.Entity<ProductLaunchProfile>(entity =>
        {
            entity.ToTable("product_launch_profiles");
            entity.HasKey(x => x.ProductKey);
            entity.Property(x => x.ProductKey).HasMaxLength(64);
            entity.Property(x => x.BaseUrl).HasMaxLength(512).IsRequired();
            entity.Property(x => x.LaunchPath).HasMaxLength(256).IsRequired();
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductKey);
        });

        modelBuilder.Entity<HandoffCodeRecord>(entity =>
        {
            entity.ToTable("handoff_codes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CodeHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.TargetProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CallbackUrl).HasMaxLength(2048);
            entity.HasIndex(x => x.CodeHash).IsUnique();
            entity.HasIndex(x => x.ExpiresAt);
            entity.HasIndex(x => new { x.TenantId, x.TargetProductKey });
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            entity.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("password_reset_tokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => x.ExpiresAt);
            entity.HasIndex(x => new { x.UserId, x.UsedAt });
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<ProductCallbackAllowlistEntry>(entity =>
        {
            entity.ToTable("product_callback_allowlist");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.UrlPattern).HasMaxLength(512).IsRequired();
            entity.Property(x => x.PatternType).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.ProductKey, x.TenantId });
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductKey);
            entity.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
        });

        modelBuilder.Entity<TenantFieldCompanionNotificationSettings>(entity =>
        {
            entity.ToTable("nexarr_fieldcompanion_notification_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NotificationWebhookUrl).HasMaxLength(2048);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<FieldCompanionNotificationDispatch>(entity =>
        {
            entity.ToTable("nexarr_fieldcompanion_notification_dispatches");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RelatedEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DispatchStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.WebhookHost).HasMaxLength(256);
            entity.Property(x => x.ErrorMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DispatchStatus, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.EventKind, x.RelatedEntityType, x.RelatedEntityId });
        });

        modelBuilder.Entity<FieldCompanionPushSubscription>(entity =>
        {
            entity.ToTable("nexarr_fieldcompanion_push_subscriptions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Endpoint).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.P256dhKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.AuthKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.UserAgent).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.UserId });
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.Endpoint }).IsUnique();
        });

        modelBuilder.Entity<FieldCompanionOfflineAction>(entity =>
        {
            entity.ToTable("nexarr_fieldcompanion_offline_actions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ActionKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TaskKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ProductKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.SyncedAt });
        });

        modelBuilder.Entity<FieldCompanionFieldSubmission>(entity =>
        {
            entity.ToTable("nexarr_fieldcompanion_field_submissions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TaskKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SubmissionKind).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.DetailMessage).HasMaxLength(512);
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.TaskKey, x.RecordedAt });
        });

        modelBuilder.Entity<PlatformTenantLifecycleSettings>(entity =>
        {
            entity.ToTable("nexarr_platform_tenant_lifecycle_settings");
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<TenantLifecycleRun>(entity =>
        {
            entity.ToTable("nexarr_tenant_lifecycle_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Outcome).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SkipReason).HasMaxLength(512);
            entity.HasIndex(x => x.ProcessedAt);
        });

        modelBuilder.Entity<PlatformAuditPackageGenerationJob>(entity =>
        {
            entity.ToTable("nexarr_platform_audit_package_generation_jobs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Format).HasMaxLength(16).IsRequired();
            entity.Property(x => x.FilterJson).HasMaxLength(4096);
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => new { x.ScopeTenantId, x.Status, x.CreatedAt });
        });

        modelBuilder.Entity<TenantProductDataPlaneProfile>(entity =>
        {
            entity.ToTable("nexarr_tenant_product_data_plane_profiles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DeploymentMode).HasMaxLength(32).IsRequired();
            entity.Property(x => x.DataEndpointUrl).HasMaxLength(512);
            entity.Property(x => x.TrustStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(512);
            entity.HasIndex(x => new { x.TenantId, x.ProductKey }).IsUnique();
            entity.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductKey);
        });

        modelBuilder.Entity<PlatformOutboxEvent>(entity =>
        {
            entity.ToTable("platform_outbox_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ProcessingStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProductCode).HasMaxLength(64);
            entity.Property(x => x.ErrorMessage).HasMaxLength(512);
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasIndex(x => new { x.ProcessingStatus, x.OccurredAt });
            entity.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<PlatformOutboxPublisherSettings>(entity =>
        {
            entity.ToTable("nexarr_platform_outbox_publisher_settings");
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<PlatformOutboxPublisherRun>(entity =>
        {
            entity.ToTable("nexarr_platform_outbox_publisher_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Outcome).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SkipReason).HasMaxLength(512);
            entity.HasIndex(x => x.ProcessedAt);
        });

        modelBuilder.Entity<ReferenceDataset>(entity =>
        {
            entity.ToTable("reference_datasets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OwnerService).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CurrentPublishedVersion).HasMaxLength(32);
            entity.HasIndex(x => x.Key).IsUnique();
        });

        modelBuilder.Entity<ReferenceSource>(entity =>
        {
            entity.ToTable("reference_sources");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ConnectorType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RefreshCadence).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TermsNotes).HasMaxLength(512);
            entity.HasIndex(x => x.Key).IsUnique();
        });

        modelBuilder.Entity<IngestionJob>(entity =>
        {
            entity.ToTable("ingestion_jobs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RawObjectKey).HasMaxLength(256);
            entity.Property(x => x.FileName).HasMaxLength(256);
            entity.Property(x => x.ErrorSummary).HasMaxLength(1024);
            entity.HasIndex(x => new { x.DatasetId, x.SourceId, x.CreatedAt });
            entity.HasOne(x => x.Dataset).WithMany().HasForeignKey(x => x.DatasetId);
            entity.HasOne(x => x.Source).WithMany().HasForeignKey(x => x.SourceId);
            entity.HasOne<PlatformUser>().WithMany().HasForeignKey(x => x.RequestedByPersonId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StagingRecord>(entity =>
        {
            entity.ToTable("staging_records");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TargetDatasetId);
            entity.Property(x => x.RawPayloadJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.NormalizedPayloadJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.ProposedEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ProposedCanonicalKey).HasMaxLength(128);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ReviewReason).HasMaxLength(1024);
            entity.HasIndex(x => x.JobId);
            entity.HasIndex(x => new { x.Status, x.Confidence });
            entity.HasOne(x => x.Job).WithMany().HasForeignKey(x => x.JobId);
            entity.HasOne(x => x.TargetDataset).WithMany().HasForeignKey(x => x.TargetDatasetId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<PlatformUser>().WithMany().HasForeignKey(x => x.ReviewerPersonId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<ReferenceEntity>().WithMany().HasForeignKey(x => x.ReferenceEntityId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReferenceEntity>(entity =>
        {
            entity.ToTable("reference_entities");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CanonicalKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.NormalizedFieldsJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => new { x.DatasetId, x.EntityType, x.CanonicalKey }).IsUnique();
            entity.HasIndex(x => new { x.DatasetId, x.Status });
            entity.HasOne(x => x.Dataset).WithMany().HasForeignKey(x => x.DatasetId);
            entity.HasOne<ReferenceSource>().WithMany().HasForeignKey(x => x.FirstSeenSourceId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<ReferenceEntityVersion>().WithMany().HasForeignKey(x => x.CurrentVersionId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReferenceEntityVersion>(entity =>
        {
            entity.ToTable("reference_entity_versions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FieldsJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.SourceEvidenceJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => new { x.ReferenceEntityId, x.Version }).IsUnique();
            entity.HasOne<ReferenceEntity>().WithMany().HasForeignKey(x => x.ReferenceEntityId);
            entity.HasOne<ReferenceEntityVersion>().WithMany().HasForeignKey(x => x.SupersededByVersionId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReferenceCrosswalk>(entity =>
        {
            entity.ToTable("reference_crosswalks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExternalSystem).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ExternalKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.ExternalSystem, x.ExternalKey }).IsUnique();
            entity.HasIndex(x => x.ReferenceEntityId);
            entity.HasOne<ReferenceEntity>().WithMany().HasForeignKey(x => x.ReferenceEntityId);
            entity.HasOne<ReferenceSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TenantReferenceOverlay>(entity =>
        {
            entity.ToTable("tenant_reference_overlays");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LocalName).HasMaxLength(200);
            entity.Property(x => x.LocalStatus).HasMaxLength(64);
            entity.Property(x => x.Notes).HasMaxLength(1024);
            entity.HasIndex(x => new { x.TenantId, x.ReferenceEntityId }).IsUnique();
            entity.HasOne<ReferenceEntity>().WithMany().HasForeignKey(x => x.ReferenceEntityId);
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId);
        });

        modelBuilder.Entity<ProductMapping>(entity =>
        {
            entity.ToTable("product_mappings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LocalEntityType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.LocalEntityId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.MappingStatus).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.ProductCode, x.LocalEntityType, x.LocalEntityId }).IsUnique();
            entity.HasOne<ReferenceEntity>().WithMany().HasForeignKey(x => x.ReferenceEntityId);
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId);
        });

        modelBuilder.Entity<ReferencePublishEvent>(entity =>
        {
            entity.ToTable("reference_publish_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PublishedVersion).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => new { x.DatasetId, x.CreatedAt });
            entity.HasOne<ReferenceDataset>().WithMany().HasForeignKey(x => x.DatasetId);
            entity.HasOne<PlatformUser>().WithMany().HasForeignKey(x => x.PublishedByPersonId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReferenceAuditEvent>(entity =>
        {
            entity.ToTable("reference_audit_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.BeforeSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.AfterSnapshotJson).HasColumnType("jsonb");
            entity.HasIndex(x => new { x.EntityType, x.EntityId, x.CreatedAt });
            entity.HasOne<PlatformUser>().WithMany().HasForeignKey(x => x.ActorPersonId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
