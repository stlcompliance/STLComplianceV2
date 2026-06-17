using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace CustomArr.Api.Data;

public sealed class CustomArrDbContext(DbContextOptions<CustomArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<CustomArrCustomer> Customers => Set<CustomArrCustomer>();
    public DbSet<CustomArrCustomerContact> CustomerContacts => Set<CustomArrCustomerContact>();
    public DbSet<CustomArrCustomerAddress> CustomerAddresses => Set<CustomArrCustomerAddress>();
    public DbSet<CustomArrCustomerIdentifier> CustomerIdentifiers => Set<CustomArrCustomerIdentifier>();
    public DbSet<CustomArrCustomerBillingProfile> CustomerBillingProfiles => Set<CustomArrCustomerBillingProfile>();
    public DbSet<CustomArrCustomerRequirement> CustomerRequirements => Set<CustomArrCustomerRequirement>();
    public DbSet<CustomArrCustomerExternalRef> CustomerExternalRefs => Set<CustomArrCustomerExternalRef>();
    public DbSet<CustomArrCustomerRelationship> CustomerRelationships => Set<CustomArrCustomerRelationship>();
    public DbSet<CustomArrCustomerCustomFieldValue> CustomerCustomFieldValues => Set<CustomArrCustomerCustomFieldValue>();
    public DbSet<CustomArrCustomerActivity> CustomerActivity => Set<CustomArrCustomerActivity>();
    public DbSet<CustomArrPortalSubmission> PortalSubmissions => Set<CustomArrPortalSubmission>();
    public DbSet<CustomArrIdempotencyRecord> IdempotencyRecords => Set<CustomArrIdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CustomArrCustomer>(entity =>
        {
            entity.ToTable("customarr_customers");
            entity.HasKey(x => x.CustomerId);
            entity.Property(x => x.CustomerId).HasMaxLength(64);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.CustomerNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CustomerCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LegalName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DbaName).HasMaxLength(256);
            entity.Property(x => x.CustomerTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ParentCustomerId).HasMaxLength(64);
            entity.Property(x => x.PrimaryContactId).HasMaxLength(64);
            entity.Property(x => x.PrimaryBillingAddressId).HasMaxLength(64);
            entity.Property(x => x.PrimaryShippingAddressId).HasMaxLength(64);
            entity.Property(x => x.PrimaryServiceAddressId).HasMaxLength(64);
            entity.Property(x => x.AccountOwnerPersonId).HasMaxLength(128);
            entity.Property(x => x.AssignedTeamId).HasMaxLength(128);
            entity.Property(x => x.SourceKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.Property(x => x.Tags).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.HoldStatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RiskRatingKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PortalDisplayName).HasMaxLength(256);
            entity.Property(x => x.DefaultPortalContactId).HasMaxLength(64);
            entity.Property(x => x.PortalInviteStatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PortalTermsAcceptedByPersonId).HasMaxLength(128);
            entity.Property(x => x.PortalNotes).HasMaxLength(4000);
            entity.Property(x => x.DefaultOrderTypeKey).HasMaxLength(64);
            entity.Property(x => x.DefaultServiceLevelKey).HasMaxLength(64);
            entity.Property(x => x.DefaultPickupAddressId).HasMaxLength(64);
            entity.Property(x => x.DefaultDeliveryAddressId).HasMaxLength(64);
            entity.Property(x => x.DefaultContactId).HasMaxLength(64);
            entity.Property(x => x.CustomerReferenceLabel).HasMaxLength(128);
            entity.Property(x => x.DefaultInstructions).HasMaxLength(4000);
            entity.Property(x => x.RestrictedServiceNotes).HasMaxLength(4000);
            entity.Property(x => x.NotificationPreferenceKey).HasMaxLength(64);
            entity.Property(x => x.CreatedByPersonId).HasMaxLength(128);
            entity.Property(x => x.UpdatedByPersonId).HasMaxLength(128);
            entity.Property(x => x.ArchivedByPersonId).HasMaxLength(128);
            entity.Property(x => x.RowVersion).IsConcurrencyToken();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CustomerNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.CustomerCode }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.StatusKey });
            entity.HasIndex(x => new { x.TenantId, x.ParentCustomerId });

            entity.HasMany(x => x.Contacts)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Addresses)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Identifiers)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.BillingProfiles)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Requirements)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.ExternalRefs)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Relationships)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.CustomFieldValues)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Activity)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomArrCustomerContact>(entity =>
        {
            entity.ToTable("customarr_customer_contacts");
            entity.HasKey(x => x.ContactId);
            entity.Property(x => x.ContactId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PersonId).HasMaxLength(128);
            entity.Property(x => x.FirstName).HasMaxLength(128);
            entity.Property(x => x.LastName).HasMaxLength(128);
            entity.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(128);
            entity.Property(x => x.Department).HasMaxLength(128);
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.Phone).HasMaxLength(64);
            entity.Property(x => x.MobilePhone).HasMaxLength(64);
            entity.Property(x => x.PhoneExtension).HasMaxLength(32);
            entity.Property(x => x.PreferredContactMethodKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PreferredLanguageKey).HasMaxLength(64);
            entity.Property(x => x.Timezone).HasMaxLength(128);
            entity.Property(x => x.PortalRoleKey).HasMaxLength(64);
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.Email });
        });

        modelBuilder.Entity<CustomArrCustomerAddress>(entity =>
        {
            entity.ToTable("customarr_customer_addresses");
            entity.HasKey(x => x.AddressId);
            entity.Property(x => x.AddressId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AddressTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LocationName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.AttentionTo).HasMaxLength(256);
            entity.Property(x => x.Line1).HasMaxLength(256);
            entity.Property(x => x.Line2).HasMaxLength(256);
            entity.Property(x => x.City).HasMaxLength(128);
            entity.Property(x => x.StateProvince).HasMaxLength(128);
            entity.Property(x => x.PostalCode).HasMaxLength(32);
            entity.Property(x => x.CountryCode).HasMaxLength(2).IsRequired();
            entity.Property(x => x.Latitude).HasPrecision(9, 6);
            entity.Property(x => x.Longitude).HasPrecision(9, 6);
            entity.Property(x => x.Timezone).HasMaxLength(128);
            entity.Property(x => x.DeliveryInstructions).HasMaxLength(4000);
            entity.Property(x => x.ReceivingHours).HasMaxLength(512);
            entity.Property(x => x.DockDoorNotes).HasMaxLength(1000);
            entity.Property(x => x.AccessRestrictions).HasMaxLength(1000);
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.AddressTypeKey });
        });

        modelBuilder.Entity<CustomArrCustomerIdentifier>(entity =>
        {
            entity.ToTable("customarr_customer_identifiers");
            entity.HasKey(x => x.IdentifierId);
            entity.Property(x => x.IdentifierId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IdentifierTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IdentifierValue).HasMaxLength(256).IsRequired();
            entity.Property(x => x.JurisdictionKey).HasMaxLength(128);
            entity.Property(x => x.IssuingAuthority).HasMaxLength(256);
            entity.Property(x => x.VerificationStatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordArrDocumentId).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.IdentifierTypeKey, x.IdentifierValue });
        });

        modelBuilder.Entity<CustomArrCustomerBillingProfile>(entity =>
        {
            entity.ToTable("customarr_customer_billing_profiles");
            entity.HasKey(x => x.BillingProfileId);
            entity.Property(x => x.BillingProfileId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.BillingContactId).HasMaxLength(64);
            entity.Property(x => x.BillingAddressId).HasMaxLength(64);
            entity.Property(x => x.PaymentTermsKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.InvoiceDeliveryMethodKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.BillingEmail).HasMaxLength(256);
            entity.Property(x => x.TaxExemptionRecordId).HasMaxLength(128);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.CreditStatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CreditLimit).HasPrecision(18, 2);
            entity.Property(x => x.ExternalAccountingCustomerRef).HasMaxLength(256);
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
        });

        modelBuilder.Entity<CustomArrCustomerRequirement>(entity =>
        {
            entity.ToTable("customarr_customer_requirements");
            entity.HasKey(x => x.RequirementId);
            entity.Property(x => x.RequirementId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RequirementTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RequirementName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.RequiredBeforeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordArrDocumentId).HasMaxLength(128);
            entity.Property(x => x.ComplianceCoreRuleRef).HasMaxLength(256);
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ReviewedByPersonId).HasMaxLength(128);
            entity.Property(x => x.WaiverReason).HasMaxLength(4000);
            entity.Property(x => x.WaivedByPersonId).HasMaxLength(128);
            entity.Property(x => x.OwnerTeam).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.StatusKey });
        });

        modelBuilder.Entity<CustomArrCustomerExternalRef>(entity =>
        {
            entity.ToTable("customarr_customer_external_refs");
            entity.HasKey(x => x.ExternalRefId);
            entity.Property(x => x.ExternalRefId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SystemKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExternalId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ExternalCode).HasMaxLength(256);
            entity.Property(x => x.SyncStatusKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.SystemKey, x.ExternalId }).IsUnique();
        });

        modelBuilder.Entity<CustomArrCustomerRelationship>(entity =>
        {
            entity.ToTable("customarr_customer_relationships");
            entity.HasKey(x => x.RelationshipId);
            entity.Property(x => x.RelationshipId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RelatedCustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RelationshipTypeKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.RelatedCustomerId });
        });

        modelBuilder.Entity<CustomArrCustomerCustomFieldValue>(entity =>
        {
            entity.ToTable("customarr_customer_custom_field_values");
            entity.HasKey(x => x.FieldValueId);
            entity.Property(x => x.FieldValueId).HasMaxLength(64);
            entity.Property(x => x.FieldDefinitionId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ValueText).HasMaxLength(4000);
            entity.Property(x => x.ValueNumber).HasPrecision(18, 4);
            entity.Property(x => x.ValueOptionKey).HasMaxLength(128);
            entity.Property(x => x.SourceKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.UpdatedByPersonId).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.FieldDefinitionId });
        });

        modelBuilder.Entity<CustomArrCustomerActivity>(entity =>
        {
            entity.ToTable("customarr_customer_activity");
            entity.HasKey(x => x.ActivityId);
            entity.Property(x => x.ActivityId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Kind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.SourceProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ActorPersonId).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.CustomerId, x.OccurredAt });
        });

        modelBuilder.Entity<CustomArrPortalSubmission>(entity =>
        {
            entity.ToTable("customarr_portal_submissions");
            entity.HasKey(x => x.SubmissionId);
            entity.Property(x => x.SubmissionId).HasMaxLength(64);
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CreatedEventType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CustomerNumberSnapshot).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CustomerNameSnapshot).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CustomerAddressId).HasMaxLength(64);
            entity.Property(x => x.CustomerAddressSnapshot).HasMaxLength(512);
            entity.Property(x => x.RequestType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OwnerPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.FulfillmentProductKeys).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.OrdArrOrderId).HasMaxLength(64);
            entity.Property(x => x.OrdArrOrderNumber).HasMaxLength(64);
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.SubmittedAt });
        });

        modelBuilder.Entity<CustomArrIdempotencyRecord>(entity =>
        {
            entity.ToTable("customarr_idempotency_records");
            entity.HasKey(x => x.IdempotencyRecordId);
            entity.Property(x => x.IdempotencyRecordId).HasMaxLength(64);
            entity.Property(x => x.OperationKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ResourceId).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.OperationKey, x.IdempotencyKey }).IsUnique();
        });
    }
}
