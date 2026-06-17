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
    public DbSet<CustomArrLead> Leads => Set<CustomArrLead>();
    public DbSet<CustomArrOpportunity> Opportunities => Set<CustomArrOpportunity>();
    public DbSet<CustomArrProposal> Proposals => Set<CustomArrProposal>();
    public DbSet<CustomArrAgreement> Agreements => Set<CustomArrAgreement>();
    public DbSet<CustomArrCustomerCase> CustomerCases => Set<CustomArrCustomerCase>();
    public DbSet<CustomArrTask> CustomerTasks => Set<CustomArrTask>();
    public DbSet<CustomArrPortalAccessRecord> PortalAccessRecords => Set<CustomArrPortalAccessRecord>();
    public DbSet<CustomArrCustomerServiceProfile> CustomerServiceProfiles => Set<CustomArrCustomerServiceProfile>();
    public DbSet<CustomArrEligibilityCheck> EligibilityChecks => Set<CustomArrEligibilityCheck>();
    public DbSet<CustomArrCustomerOnboarding> CustomerOnboarding => Set<CustomArrCustomerOnboarding>();
    public DbSet<CustomArrCustomerOnboardingChecklistItem> CustomerOnboardingChecklistItems => Set<CustomArrCustomerOnboardingChecklistItem>();
    public DbSet<CustomArrCustomerHealthProfile> CustomerHealthProfiles => Set<CustomArrCustomerHealthProfile>();
    public DbSet<CustomArrImportBatch> ImportBatches => Set<CustomArrImportBatch>();
    public DbSet<CustomArrDedupeCandidate> DedupeCandidates => Set<CustomArrDedupeCandidate>();
    public DbSet<CustomArrMergeRecord> MergeRecords => Set<CustomArrMergeRecord>();
    public DbSet<CustomArrIntegrationReference> IntegrationReferences => Set<CustomArrIntegrationReference>();
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
            entity.Property(x => x.RelationshipRoleKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AccountClassKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OnboardingStatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ServiceEligibilityStatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ComplianceStatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IndustryKey).HasMaxLength(128);
            entity.Property(x => x.NaicsCode).HasMaxLength(32);
            entity.Property(x => x.SicCode).HasMaxLength(32);
            entity.Property(x => x.WebsiteUrl).HasMaxLength(512);
            entity.Property(x => x.RegionKey).HasMaxLength(64);
            entity.Property(x => x.MarketKey).HasMaxLength(64);
            entity.Property(x => x.VerticalKey).HasMaxLength(64);
            entity.Property(x => x.RevenueBandKey).HasMaxLength(64);
            entity.Property(x => x.ParentCustomerId).HasMaxLength(64);
            entity.Property(x => x.PrimaryContactId).HasMaxLength(64);
            entity.Property(x => x.PrimaryBillingAddressId).HasMaxLength(64);
            entity.Property(x => x.PrimaryShippingAddressId).HasMaxLength(64);
            entity.Property(x => x.PrimaryServiceAddressId).HasMaxLength(64);
            entity.Property(x => x.AccountOwnerPersonId).HasMaxLength(128);
            entity.Property(x => x.SalesOwnerPersonId).HasMaxLength(128);
            entity.Property(x => x.SupportOwnerPersonId).HasMaxLength(128);
            entity.Property(x => x.CustomerSuccessOwnerPersonId).HasMaxLength(128);
            entity.Property(x => x.AssignedTeamId).HasMaxLength(128);
            entity.Property(x => x.SourceKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.Property(x => x.Tags).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.HoldStatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RiskRatingKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.HealthScoreKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.HealthScoreReason).HasMaxLength(1000);
            entity.Property(x => x.RelationshipSummary).HasMaxLength(4000);
            entity.Property(x => x.InternalNotes).HasMaxLength(4000);
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
            entity.HasIndex(x => new { x.TenantId, x.ServiceEligibilityStatusKey });
            entity.HasIndex(x => new { x.TenantId, x.OnboardingStatusKey });
            entity.HasIndex(x => new { x.TenantId, x.HealthScoreKey });
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
            entity.HasMany(x => x.Leads)
                .WithOne()
                .HasForeignKey(x => x.ConvertedCustomerId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasMany(x => x.Opportunities)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasMany(x => x.Proposals)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Agreements)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Cases)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Tasks)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.PortalAccessRecords)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.ServiceProfiles)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.OnboardingRecords)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.HealthProfiles)
                .WithOne()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.IntegrationReferences)
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
            entity.Property(x => x.AuthorizationScopes).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.ConsentLegalBasisKey).HasMaxLength(128);
            entity.Property(x => x.LocationId).HasMaxLength(64);
            entity.Property(x => x.PortalRoleKey).HasMaxLength(64);
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.Email });
            entity.HasIndex(x => new { x.TenantId, x.LocationId });
        });

        modelBuilder.Entity<CustomArrCustomerAddress>(entity =>
        {
            entity.ToTable("customarr_customer_addresses");
            entity.HasKey(x => x.AddressId);
            entity.Property(x => x.AddressId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AddressTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LocationCode).HasMaxLength(64).IsRequired();
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
            entity.Property(x => x.MailingLine1).HasMaxLength(256);
            entity.Property(x => x.MailingLine2).HasMaxLength(256);
            entity.Property(x => x.MailingCity).HasMaxLength(128);
            entity.Property(x => x.MailingStateProvince).HasMaxLength(128);
            entity.Property(x => x.MailingPostalCode).HasMaxLength(32);
            entity.Property(x => x.MailingCountryCode).HasMaxLength(2);
            entity.Property(x => x.ShippingHours).HasMaxLength(512);
            entity.Property(x => x.AppointmentInstructions).HasMaxLength(1000);
            entity.Property(x => x.DriverCheckInRules).HasMaxLength(1000);
            entity.Property(x => x.DeliveryInstructions).HasMaxLength(4000);
            entity.Property(x => x.ReceivingHours).HasMaxLength(512);
            entity.Property(x => x.DockDoorNotes).HasMaxLength(1000);
            entity.Property(x => x.GateInstructions).HasMaxLength(1000);
            entity.Property(x => x.ParkingInstructions).HasMaxLength(1000);
            entity.Property(x => x.AfterHoursRules).HasMaxLength(1000);
            entity.Property(x => x.TruckSizeRestrictions).HasMaxLength(1000);
            entity.Property(x => x.HazmatRestrictions).HasMaxLength(1000);
            entity.Property(x => x.PpeRequirements).HasMaxLength(1000);
            entity.Property(x => x.TemperatureRules).HasMaxLength(1000);
            entity.Property(x => x.ServiceAreaKeys).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.EligibleProductKeys).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.ComplianceNotes).HasMaxLength(2000);
            entity.Property(x => x.AccessRestrictions).HasMaxLength(1000);
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.LocationCode });
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
            entity.Property(x => x.ExternalEntityType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SyncDirectionKey).HasMaxLength(64).IsRequired();
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
            entity.Property(x => x.ActivityTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(256);
            entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Body).HasMaxLength(4000);
            entity.Property(x => x.SourceProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256);
            entity.Property(x => x.ContactId).HasMaxLength(64);
            entity.Property(x => x.CustomerLocationId).HasMaxLength(64);
            entity.Property(x => x.DirectionKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.VisibilityKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RelatedObjectRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.RecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.ActorPersonId).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.CustomerId, x.OccurredAt });
            entity.HasIndex(x => new { x.TenantId, x.ActivityTypeKey });
        });

        modelBuilder.Entity<CustomArrLead>(entity =>
        {
            entity.ToTable("customarr_leads");
            entity.HasKey(x => x.LeadId);
            entity.Property(x => x.LeadId).HasMaxLength(64);
            entity.Property(x => x.LeadNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CompanyName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PersonName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.Phone).HasMaxLength(64);
            entity.Property(x => x.SourceKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.NeedSummary).HasMaxLength(1000);
            entity.Property(x => x.BudgetSummary).HasMaxLength(1000);
            entity.Property(x => x.TimingSummary).HasMaxLength(1000);
            entity.Property(x => x.AuthoritySummary).HasMaxLength(1000);
            entity.Property(x => x.ServiceInterest).HasMaxLength(1000);
            entity.Property(x => x.OwnerPersonId).HasMaxLength(128);
            entity.Property(x => x.AssignedTeamId).HasMaxLength(128);
            entity.Property(x => x.ConvertedCustomerId).HasMaxLength(64);
            entity.Property(x => x.ConvertedContactId).HasMaxLength(64);
            entity.Property(x => x.ConvertedOpportunityId).HasMaxLength(64);
            entity.Property(x => x.DisqualificationReason).HasMaxLength(1000);
            entity.HasIndex(x => new { x.TenantId, x.LeadNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.StatusKey });
            entity.HasIndex(x => new { x.TenantId, x.Email });
        });

        modelBuilder.Entity<CustomArrOpportunity>(entity =>
        {
            entity.ToTable("customarr_opportunities");
            entity.HasKey(x => x.OpportunityId);
            entity.Property(x => x.OpportunityId).HasMaxLength(64);
            entity.Property(x => x.OpportunityNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LeadId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64);
            entity.Property(x => x.OpportunityName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.StageKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ForecastCategoryKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EstimatedRevenue).HasPrecision(18, 2);
            entity.Property(x => x.EstimatedMargin).HasPrecision(18, 2);
            entity.Property(x => x.RecurringRevenue).HasPrecision(18, 2);
            entity.Property(x => x.OneTimeRevenue).HasPrecision(18, 2);
            entity.Property(x => x.ServiceInterestKeys).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.ScopeSummary).HasMaxLength(4000);
            entity.Property(x => x.PrimaryContactId).HasMaxLength(64);
            entity.Property(x => x.StakeholderContactIds).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.Competitor).HasMaxLength(256);
            entity.Property(x => x.IncumbentProvider).HasMaxLength(256);
            entity.Property(x => x.WinLossReason).HasMaxLength(1000);
            entity.Property(x => x.NextStep).HasMaxLength(1000);
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OutcomeKey).HasMaxLength(64);
            entity.Property(x => x.HandoffProductKey).HasMaxLength(64);
            entity.Property(x => x.HandoffObjectRef).HasMaxLength(256);
            entity.HasIndex(x => new { x.TenantId, x.OpportunityNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.StageKey });
            entity.HasIndex(x => new { x.TenantId, x.StatusKey });
        });

        modelBuilder.Entity<CustomArrProposal>(entity =>
        {
            entity.ToTable("customarr_proposals");
            entity.HasKey(x => x.ProposalId);
            entity.Property(x => x.ProposalId).HasMaxLength(64);
            entity.Property(x => x.ProposalNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OpportunityId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ScopeSummary).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.PricingSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.TermsSnapshot).HasMaxLength(4000);
            entity.Property(x => x.SlaSnapshot).HasMaxLength(1000);
            entity.Property(x => x.ApprovalStatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ApprovedByPersonId).HasMaxLength(128);
            entity.Property(x => x.CustomerResponseKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExternalAccountingQuoteRef).HasMaxLength(256);
            entity.Property(x => x.CreatedOrdArrOrderRef).HasMaxLength(256);
            entity.HasIndex(x => new { x.TenantId, x.ProposalNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.StatusKey });
        });

        modelBuilder.Entity<CustomArrAgreement>(entity =>
        {
            entity.ToTable("customarr_agreements");
            entity.HasKey(x => x.AgreementId);
            entity.Property(x => x.AgreementId).HasMaxLength(64);
            entity.Property(x => x.AgreementNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AgreementTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ScopeSummary).HasMaxLength(4000);
            entity.Property(x => x.TermsSummary).HasMaxLength(4000);
            entity.Property(x => x.CoveredProductKeys).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.CoveredLocationIds).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.RequirementRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.RecordRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.OwnerPersonId).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.AgreementNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.StatusKey });
        });

        modelBuilder.Entity<CustomArrCustomerCase>(entity =>
        {
            entity.ToTable("customarr_customer_cases");
            entity.HasKey(x => x.CaseId);
            entity.Property(x => x.CaseId).HasMaxLength(64);
            entity.Property(x => x.CaseNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ContactId).HasMaxLength(64);
            entity.Property(x => x.CustomerLocationId).HasMaxLength(64);
            entity.Property(x => x.Subject).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.SourceKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PriorityKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SeverityKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SupportOwnerPersonId).HasMaxLength(128);
            entity.Property(x => x.EscalationOwnerPersonId).HasMaxLength(128);
            entity.Property(x => x.OwningProductKey).HasMaxLength(64);
            entity.Property(x => x.OwningProductIssueRef).HasMaxLength(256);
            entity.Property(x => x.RootCauseCategoryKey).HasMaxLength(128);
            entity.Property(x => x.ResolutionSummary).HasMaxLength(4000);
            entity.Property(x => x.CustomerSatisfactionKey).HasMaxLength(64);
            entity.HasIndex(x => new { x.TenantId, x.CaseNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.StatusKey });
        });

        modelBuilder.Entity<CustomArrTask>(entity =>
        {
            entity.ToTable("customarr_tasks");
            entity.HasKey(x => x.TaskId);
            entity.Property(x => x.TaskId).HasMaxLength(64);
            entity.Property(x => x.TaskNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RelatedObjectType).HasMaxLength(128);
            entity.Property(x => x.RelatedObjectId).HasMaxLength(64);
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.OwnerPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PriorityKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.TaskNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.OwnerPersonId });
            entity.HasIndex(x => new { x.TenantId, x.StatusKey });
        });

        modelBuilder.Entity<CustomArrPortalAccessRecord>(entity =>
        {
            entity.ToTable("customarr_portal_access_records");
            entity.HasKey(x => x.PortalAccessId);
            entity.Property(x => x.PortalAccessId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ContactId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.NexArrExternalIdentityRef).HasMaxLength(256);
            entity.Property(x => x.PortalRoleKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AllowedLocationIds).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.AuthorizationRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.InvitedByPersonId).HasMaxLength(128);
            entity.Property(x => x.RevokeReason).HasMaxLength(1000);
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.ContactId });
            entity.HasIndex(x => new { x.TenantId, x.StatusKey });
        });

        modelBuilder.Entity<CustomArrCustomerServiceProfile>(entity =>
        {
            entity.ToTable("customarr_customer_service_profiles");
            entity.HasKey(x => x.ServiceProfileId);
            entity.Property(x => x.ServiceProfileId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CustomerLocationId).HasMaxLength(64);
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ServiceEligibilityStatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AllowedProductKeys).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.BlockedProductKeys).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.AllowedWorkflowKeys).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.BlockedWorkflowKeys).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.RequiredApprovalTypes).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.RequiredRequirementRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.ActiveHoldRefs).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.Restrictions).HasMaxLength(4000);
            entity.Property(x => x.ServiceLevelKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExternalCreditStatusSnapshotKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LastEligibilityReason).HasMaxLength(1000);
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.ServiceEligibilityStatusKey });
        });

        modelBuilder.Entity<CustomArrEligibilityCheck>(entity =>
        {
            entity.ToTable("customarr_eligibility_checks");
            entity.HasKey(x => x.EligibilityCheckId);
            entity.Property(x => x.EligibilityCheckId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CustomerLocationId).HasMaxLength(64);
            entity.Property(x => x.CustomerContactId).HasMaxLength(64);
            entity.Property(x => x.WorkflowKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SourceProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256);
            entity.Property(x => x.ResultKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Explanation).HasMaxLength(2000);
            entity.Property(x => x.Blockers).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.Warnings).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.ActorPersonId).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.CustomerId, x.CheckedAt });
            entity.HasIndex(x => new { x.TenantId, x.ResultKey });
        });

        modelBuilder.Entity<CustomArrCustomerOnboarding>(entity =>
        {
            entity.ToTable("customarr_customer_onboarding");
            entity.HasKey(x => x.OnboardingId);
            entity.Property(x => x.OnboardingId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OnboardingNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OnboardingTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OwnerPersonId).HasMaxLength(128);
            entity.Property(x => x.Blockers).HasColumnType("text[]").IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.OnboardingNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.StatusKey });
            entity.HasMany(x => x.ChecklistItems)
                .WithOne()
                .HasForeignKey(x => x.OnboardingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomArrCustomerOnboardingChecklistItem>(entity =>
        {
            entity.ToTable("customarr_customer_onboarding_checklist_items");
            entity.HasKey(x => x.ChecklistItemId);
            entity.Property(x => x.ChecklistItemId).HasMaxLength(64);
            entity.Property(x => x.OnboardingId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ItemTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OwnerProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EvidenceRecordRefs).HasColumnType("text[]").IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.OnboardingId });
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
        });

        modelBuilder.Entity<CustomArrCustomerHealthProfile>(entity =>
        {
            entity.ToTable("customarr_customer_health_profiles");
            entity.HasKey(x => x.HealthProfileId);
            entity.Property(x => x.HealthProfileId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.HealthStatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ScoreReason).HasMaxLength(1000);
            entity.Property(x => x.RevenueSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.ChurnRiskKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PaymentRiskKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExpansionSummary).HasMaxLength(1000);
            entity.HasIndex(x => new { x.TenantId, x.CustomerId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.HealthStatusKey });
        });

        modelBuilder.Entity<CustomArrImportBatch>(entity =>
        {
            entity.ToTable("customarr_import_batches");
            entity.HasKey(x => x.ImportBatchId);
            entity.Property(x => x.ImportBatchId).HasMaxLength(64);
            entity.Property(x => x.SourceKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceFileName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ImporterPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.MappingSummaryJson).HasColumnType("jsonb");
            entity.Property(x => x.ValidationErrors).HasColumnType("text[]").IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.StatusKey });
        });

        modelBuilder.Entity<CustomArrDedupeCandidate>(entity =>
        {
            entity.ToTable("customarr_dedupe_candidates");
            entity.HasKey(x => x.DedupeCandidateId);
            entity.Property(x => x.DedupeCandidateId).HasMaxLength(64);
            entity.Property(x => x.ImportBatchId).HasMaxLength(64);
            entity.Property(x => x.CandidateTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceRecordRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.MatchedCustomerId).HasMaxLength(64);
            entity.Property(x => x.MatchReason).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.ConfidenceScore).HasPrecision(5, 4);
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.ImportBatchId });
            entity.HasIndex(x => new { x.TenantId, x.StatusKey });
        });

        modelBuilder.Entity<CustomArrMergeRecord>(entity =>
        {
            entity.ToTable("customarr_merge_records");
            entity.HasKey(x => x.MergeRecordId);
            entity.Property(x => x.MergeRecordId).HasMaxLength(64);
            entity.Property(x => x.SurvivorCustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.MergedCustomerIds).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.MergeReason).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.MergeStrategyKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FieldResolutionSummary).HasMaxLength(4000);
            entity.Property(x => x.ProposedByPersonId).HasMaxLength(128);
            entity.Property(x => x.ApprovedByPersonId).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.SurvivorCustomerId });
            entity.HasIndex(x => new { x.TenantId, x.StatusKey });
        });

        modelBuilder.Entity<CustomArrIntegrationReference>(entity =>
        {
            entity.ToTable("customarr_integration_references");
            entity.HasKey(x => x.IntegrationReferenceId);
            entity.Property(x => x.IntegrationReferenceId).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CustomerLocationId).HasMaxLength(64);
            entity.Property(x => x.CustomerContactId).HasMaxLength(64);
            entity.Property(x => x.RelatedEntityType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.RelatedEntityId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExternalSystemKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExternalEntityType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ExternalId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ExternalDisplayName).HasMaxLength(256);
            entity.Property(x => x.SyncDirectionKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.CustomerId });
            entity.HasIndex(x => new { x.TenantId, x.ExternalSystemKey, x.ExternalEntityType, x.ExternalId }).IsUnique();
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
