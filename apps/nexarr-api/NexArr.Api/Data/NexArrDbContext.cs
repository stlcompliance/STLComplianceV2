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
    public DbSet<ProductCatalogItem> ProductCatalog => Set<ProductCatalogItem>();
    public DbSet<TenantProductEntitlement> Entitlements => Set<TenantProductEntitlement>();
    public DbSet<PlatformAuditEvent> AuditEvents => Set<PlatformAuditEvent>();
    public DbSet<ServiceClient> ServiceClients => Set<ServiceClient>();
    public DbSet<ServiceTokenRecord> ServiceTokens => Set<ServiceTokenRecord>();
    public DbSet<ProductLaunchProfile> LaunchProfiles => Set<ProductLaunchProfile>();
    public DbSet<HandoffCodeRecord> HandoffCodes => Set<HandoffCodeRecord>();
    public DbSet<ProductCallbackAllowlistEntry> CallbackAllowlist => Set<ProductCallbackAllowlistEntry>();

    public DbSet<TenantCompanionNotificationSettings> TenantCompanionNotificationSettings =>
        Set<TenantCompanionNotificationSettings>();

    public DbSet<CompanionNotificationDispatch> CompanionNotificationDispatches =>
        Set<CompanionNotificationDispatch>();

    public DbSet<CompanionPushSubscription> CompanionPushSubscriptions => Set<CompanionPushSubscription>();

    public DbSet<PlatformAuditPackageGenerationJob> PlatformAuditPackageGenerationJobs =>
        Set<PlatformAuditPackageGenerationJob>();

    public DbSet<PlatformServiceTokenCleanupSettings> PlatformServiceTokenCleanupSettings =>
        Set<PlatformServiceTokenCleanupSettings>();

    public DbSet<ServiceTokenCleanupRun> ServiceTokenCleanupRuns => Set<ServiceTokenCleanupRun>();

    public DbSet<CompanionOfflineAction> CompanionOfflineActions => Set<CompanionOfflineAction>();
    public DbSet<CompanionFieldSubmission> CompanionFieldSubmissions => Set<CompanionFieldSubmission>();

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
            entity.HasOne(x => x.User).WithOne(x => x.Credential).HasForeignKey<UserCredential>(x => x.UserId);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.ToTable("user_sessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RefreshTokenHash).HasMaxLength(128).IsRequired();
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

        modelBuilder.Entity<ProductCatalogItem>(entity =>
        {
            entity.ToTable("product_catalog");
            entity.HasKey(x => x.ProductKey);
            entity.Property(x => x.ProductKey).HasMaxLength(64);
            entity.Property(x => x.DisplayName).HasMaxLength(128).IsRequired();
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
            entity.Property(x => x.ActionScope).HasMaxLength(128);
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

        modelBuilder.Entity<ServiceTokenCleanupRun>(entity =>
        {
            entity.ToTable("nexarr_service_token_cleanup_runs");
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

        modelBuilder.Entity<TenantCompanionNotificationSettings>(entity =>
        {
            entity.ToTable("nexarr_companion_notification_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NotificationWebhookUrl).HasMaxLength(2048);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<CompanionNotificationDispatch>(entity =>
        {
            entity.ToTable("nexarr_companion_notification_dispatches");
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

        modelBuilder.Entity<CompanionPushSubscription>(entity =>
        {
            entity.ToTable("nexarr_companion_push_subscriptions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Endpoint).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.P256dhKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.AuthKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.UserAgent).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.UserId });
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.Endpoint }).IsUnique();
        });

        modelBuilder.Entity<CompanionOfflineAction>(entity =>
        {
            entity.ToTable("nexarr_companion_offline_actions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ActionKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TaskKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ProductKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.SyncedAt });
        });

        modelBuilder.Entity<CompanionFieldSubmission>(entity =>
        {
            entity.ToTable("nexarr_companion_field_submissions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TaskKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SubmissionKind).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.DetailMessage).HasMaxLength(512);
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.TaskKey, x.RecordedAt });
        });

        modelBuilder.Entity<PlatformAuditPackageGenerationJob>(entity =>
        {
            entity.ToTable("nexarr_platform_audit_package_generation_jobs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Format).HasMaxLength(16).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => new { x.ScopeTenantId, x.Status, x.CreatedAt });
        });
    }
}
