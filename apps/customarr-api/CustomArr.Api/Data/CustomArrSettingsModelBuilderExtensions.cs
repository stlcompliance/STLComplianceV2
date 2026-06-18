using Microsoft.EntityFrameworkCore;

namespace CustomArr.Api.Data;

public static class CustomArrSettingsModelBuilderExtensions
{
    public static void ConfigureCustomArrTenantSettings(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomArrTenantSettings>(entity =>
        {
            entity.ToTable("customarr_tenant_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.CreatedByPersonId).HasMaxLength(128);
            entity.Property(x => x.UpdatedByPersonId).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.IsActive });
            entity.HasIndex(x => new { x.TenantId, x.SettingsVersion }).IsUnique();
        });

        modelBuilder.Entity<CustomArrTenantSettingsAuditEvent>(entity =>
        {
            entity.ToTable("customarr_tenant_settings_audit_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Scope).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SectionKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ChangeSummary).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.ActorPersonId).HasMaxLength(128);
            entity.Property(x => x.SourceProductKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.SettingsVersion });
            entity.HasIndex(x => new { x.TenantId, x.OccurredAt });
        });

        modelBuilder.Entity<CustomArrCustomerNumberingSettings>(entity =>
        {
            entity.ToTable("customarr_customer_numbering_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Prefix).HasMaxLength(16).IsRequired();
            entity.Property(x => x.SequenceName).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DisplayFormat).HasMaxLength(64).IsRequired();
            entity.Property(x => x.UniquenessScope).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<CustomArrCustomerLifecycleStage>(entity =>
        {
            entity.ToTable("customarr_customer_lifecycle_stages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Key).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.AllowedNextStageKeys).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.ColorToken).HasMaxLength(64);
            entity.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SortOrder });
            entity.HasIndex(x => new { x.TenantId, x.IsInitial });
        });

        modelBuilder.Entity<CustomArrCustomerLifecycleTransitionRule>(entity =>
        {
            entity.ToTable("customarr_customer_lifecycle_transition_rules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.FromStageKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ToStageKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RequiredPermission).HasMaxLength(128);
            entity.Property(x => x.RequiredChecklistTemplateKey).HasMaxLength(64);
            entity.HasIndex(x => new { x.TenantId, x.FromStageKey, x.ToStageKey }).IsUnique();
        });

        modelBuilder.Entity<CustomArrCustomerClassificationCatalog>(entity =>
        {
            entity.ToTable("customarr_customer_classification_catalogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.CatalogType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Key).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.MetadataKey).HasMaxLength(64);
            entity.Property(x => x.MetadataValue).HasMaxLength(256);
            entity.HasIndex(x => new { x.TenantId, x.CatalogType, x.Key }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.CatalogType, x.SortOrder });
        });

        modelBuilder.Entity<CustomArrCustomerRequiredFieldRule>(entity =>
        {
            entity.ToTable("customarr_customer_required_field_rules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.CustomerTypeKey).HasMaxLength(64);
            entity.Property(x => x.LifecycleStageKey).HasMaxLength(64);
            entity.Property(x => x.FieldKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RequirementLevel).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ValidationMessage).HasMaxLength(512);
            entity.HasIndex(x => new { x.TenantId, x.FieldKey, x.CustomerTypeKey, x.LifecycleStageKey }).IsUnique();
        });

        modelBuilder.Entity<CustomArrCustomerContactRole>(entity =>
        {
            entity.ToTable("customarr_customer_contact_roles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Key).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SortOrder });
        });

        modelBuilder.Entity<CustomArrCustomerAddressType>(entity =>
        {
            entity.ToTable("customarr_customer_address_types");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Key).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SortOrder });
        });

        modelBuilder.Entity<CustomArrCustomerOwnerRule>(entity =>
        {
            entity.ToTable("customarr_customer_owner_rules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.RuleName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CustomerTypeKey).HasMaxLength(64);
            entity.Property(x => x.TerritoryKey).HasMaxLength(64);
            entity.Property(x => x.IndustryKey).HasMaxLength(64);
            entity.Property(x => x.SourceKey).HasMaxLength(64);
            entity.Property(x => x.DefaultOwnerType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DefaultOwnerRefId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DefaultOwnerNameSnapshot).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ApprovalPermission).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.Priority });
        });

        modelBuilder.Entity<CustomArrCustomerOnboardingTemplate>(entity =>
        {
            entity.ToTable("customarr_customer_onboarding_templates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Key).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.CustomerTypeKey).HasMaxLength(64);
            entity.Property(x => x.IndustryKey).HasMaxLength(64);
            entity.Property(x => x.PriorityTierKey).HasMaxLength(64);
            entity.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SortOrder });
        });

        modelBuilder.Entity<CustomArrCustomerOnboardingChecklistItemTemplate>(entity =>
        {
            entity.ToTable("customarr_customer_onboarding_checklist_item_templates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.TemplateKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Key).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.ItemType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OwnerType).HasMaxLength(64);
            entity.Property(x => x.OwnerRefId).HasMaxLength(128);
            entity.Property(x => x.OwnerNameSnapshot).HasMaxLength(256);
            entity.Property(x => x.DocumentTypeKey).HasMaxLength(64);
            entity.Property(x => x.ComplianceQuestionnaireKey).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.TemplateKey, x.Key }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.TemplateKey, x.SortOrder });
        });

        modelBuilder.Entity<CustomArrCustomerPortalTenantSettings>(entity =>
        {
            entity.ToTable("customarr_customer_portal_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.AllowedEmailDomains).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.SupportContactName).HasMaxLength(128);
            entity.Property(x => x.SupportContactEmail).HasMaxLength(256);
            entity.Property(x => x.SupportContactPhone).HasMaxLength(64);
            entity.Property(x => x.PortalDisplayName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.LogoRecordArrDocumentId).HasMaxLength(128);
            entity.Property(x => x.DefaultPortalContactRoleKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PortalAdminContactRoleKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<CustomArrCustomerDocumentRequirement>(entity =>
        {
            entity.ToTable("customarr_customer_document_requirements");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Key).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.CustomerTypeKey).HasMaxLength(64);
            entity.Property(x => x.LifecycleStageKey).HasMaxLength(64);
            entity.Property(x => x.RecordArrDocumentTypeKey).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        });

        modelBuilder.Entity<CustomArrCustomerDuplicateDetectionRule>(entity =>
        {
            entity.ToTable("customarr_customer_duplicate_detection_rules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Key).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.MatchField).HasMaxLength(64).IsRequired();
            entity.Property(x => x.MatchType).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Priority });
        });

        modelBuilder.Entity<CustomArrCustomerIntegrationSettings>(entity =>
        {
            entity.ToTable("customarr_customer_integration_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.ErpSyncMode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DefaultConflictResolution).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<CustomArrCustomerExternalIdSource>(entity =>
        {
            entity.ToTable("customarr_customer_external_id_sources");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Key).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        });

        modelBuilder.Entity<CustomArrCustomerNotificationRule>(entity =>
        {
            entity.ToTable("customarr_customer_notification_rules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Key).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EventType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.RecipientType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecipientRefId).HasMaxLength(128);
            entity.Property(x => x.RecipientNameSnapshot).HasMaxLength(256);
            entity.Property(x => x.CustomerContactRoleKey).HasMaxLength(64);
            entity.Property(x => x.TemplateKey).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        });

        modelBuilder.Entity<CustomArrCustomerCustomFieldDefinition>(entity =>
        {
            entity.ToTable("customarr_customer_custom_field_definitions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Key).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.FieldType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AppliesToCustomerTypeKey).HasMaxLength(64);
            entity.Property(x => x.AppliesToLifecycleStageKey).HasMaxLength(64);
            entity.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SortOrder });
        });

        modelBuilder.Entity<CustomArrCustomerCustomFieldOption>(entity =>
        {
            entity.ToTable("customarr_customer_custom_field_options");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.FieldKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Key).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.FieldKey, x.Key }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.FieldKey, x.SortOrder });
        });
    }
}
