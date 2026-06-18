using System.Security.Claims;
using CustomArr.Api.Data;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace CustomArr.Api.Services;

public sealed class CustomArrTenantSettingsService(CustomArrDbContext db)
{
    private static readonly HashSet<string> AllowedCatalogTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "customer_type",
        "industry",
        "priority_tier",
        "service_model",
        "territory",
        "tag",
        "lost_reason",
        "inactive_reason",
        "credit_status",
        "payment_terms",
        "contract_status"
    };

    private static readonly HashSet<string> AllowedRequirementLevels = new(StringComparer.OrdinalIgnoreCase)
    {
        "hidden",
        "optional",
        "recommended",
        "required"
    };

    private static readonly HashSet<string> AllowedCustomFieldTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text",
        "number",
        "date",
        "boolean",
        "enum",
        "multi_enum",
        "money",
        "url",
        "email",
        "phone"
    };

    private static readonly HashSet<string> AllowedMatchTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "exact",
        "normalized",
        "fuzzy",
        "domain",
        "phone",
        "tax_id",
        "external_id"
    };

    public async Task<CustomArrTenantSettingsResponse> GetSettingsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureDefaultsAsync(tenantId, principal.GetPersonId().ToString("D"), cancellationToken);
        return await BuildResponseAsync(tenantId, cancellationToken);
    }

    public async Task<CustomArrTenantSettingsResponse> UpdateSettingsAsync(
        ClaimsPrincipal principal,
        CustomArrTenantSettingsUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureManage(principal);
        await EnsureDefaultsAsync(tenantId, principal.GetPersonId().ToString("D"), cancellationToken);
        ValidateAggregate(request);

        var actorPersonId = principal.GetPersonId().ToString("D");
        var now = DateTimeOffset.UtcNow;
        var settings = await db.TenantSettings.SingleAsync(x => x.TenantId == tenantId && x.IsActive, cancellationToken);
        settings.SettingsVersion += 1;
        settings.UpdatedAt = now;
        settings.UpdatedByPersonId = actorPersonId;

        ReplaceSingleton(tenantId, db.CustomerNumberingSettings, ToEntity(tenantId, request.Numbering));
        ReplaceCollection(tenantId, db.CustomerLifecycleStages, request.LifecycleStages.Select(x => ToEntity(tenantId, x)));
        ReplaceCollection(tenantId, db.CustomerLifecycleTransitionRules, request.TransitionRules.Select(x => ToEntity(tenantId, x)));
        ReplaceCollection(tenantId, db.CustomerClassificationCatalogs, request.ClassificationCatalogs.Select(x => ToEntity(tenantId, x)));
        ReplaceCollection(tenantId, db.CustomerRequiredFieldRules, request.RequiredFieldRules.Select(x => ToEntity(tenantId, x)));
        ReplaceCollection(tenantId, db.CustomerContactRoles, request.ContactRoles.Select(x => ToEntity(tenantId, x)));
        ReplaceCollection(tenantId, db.CustomerAddressTypes, request.AddressTypes.Select(x => ToEntity(tenantId, x)));
        ReplaceCollection(tenantId, db.CustomerOwnerRules, request.OwnerRules.Select(x => ToEntity(tenantId, x)));
        ReplaceCollection(tenantId, db.CustomerOnboardingTemplates, request.OnboardingTemplates.Select(x => ToEntity(tenantId, x)));
        ReplaceCollection(tenantId, db.CustomerOnboardingChecklistItemTemplates, request.OnboardingChecklistItems.Select(x => ToEntity(tenantId, x)));
        ReplaceSingleton(tenantId, db.CustomerPortalTenantSettings, ToEntity(tenantId, request.PortalSettings));
        ReplaceCollection(tenantId, db.CustomerDocumentRequirements, request.DocumentRequirements.Select(x => ToEntity(tenantId, x)));
        ReplaceCollection(tenantId, db.CustomerDuplicateDetectionRules, request.DuplicateDetectionRules.Select(x => ToEntity(tenantId, x)));
        ReplaceSingleton(tenantId, db.CustomerIntegrationSettings, ToEntity(tenantId, request.IntegrationSettings));
        ReplaceCollection(tenantId, db.CustomerExternalIdSources, request.ExternalIdSources.Select(x => ToEntity(tenantId, x)));
        ReplaceCollection(tenantId, db.CustomerNotificationRules, request.NotificationRules.Select(x => ToEntity(tenantId, x)));
        ReplaceCollection(tenantId, db.CustomerCustomFieldDefinitions, request.CustomFieldDefinitions.Select(x => ToEntity(tenantId, x)));
        ReplaceCollection(tenantId, db.CustomerCustomFieldOptions, request.CustomFieldOptions.Select(x => ToEntity(tenantId, x)));
        db.TenantSettingsAuditEvents.Add(new CustomArrTenantSettingsAuditEvent
        {
            Id = NewId("settingsaudit"),
            TenantId = tenantId,
            SettingsVersion = settings.SettingsVersion,
            Scope = "tenant",
            SectionKey = "all",
            ChangeSummary = BuildAuditSummary(request),
            ActorPersonId = actorPersonId,
            OccurredAt = now,
            SourceProductKey = StlProductKeys.CustomArr
        });

        await db.SaveChangesAsync(cancellationToken);
        return await BuildResponseAsync(tenantId, cancellationToken);
    }

    public async Task<CustomArrCustomerCreateMetadataResponse> GetCreateMetadataAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(principal, cancellationToken);
        var initialStage = settings.LifecycleStages.FirstOrDefault(x => x.IsInitial) ?? settings.LifecycleStages.OrderBy(x => x.SortOrder).First();
        return new CustomArrCustomerCreateMetadataResponse(
            settings.Numbering.Preview,
            initialStage.Key,
            settings.LifecycleStages,
            settings.ClassificationCatalogs.Where(x => x.IsActive).OrderBy(x => x.CatalogType).ThenBy(x => x.SortOrder).ToArray(),
            settings.ContactRoles.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToArray(),
            settings.AddressTypes.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToArray(),
            settings.RequiredFieldRules.Where(x => x.AppliesToInternalCreate).ToArray(),
            settings.OnboardingTemplates.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToArray(),
            settings.DocumentRequirements.Where(x => x.Required).ToArray(),
            settings.CustomFieldDefinitions.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToArray(),
            settings.OwnerRules.Where(x => x.IsActive).OrderBy(x => x.Priority).ToArray());
    }

    public async Task<CustomArrCustomerValidationResponse> ValidateCustomerAsync(
        ClaimsPrincipal principal,
        CustomArrCustomerValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureDefaultsAsync(tenantId, principal.GetPersonId().ToString("D"), cancellationToken);
        var errors = await ValidateCustomerPayloadAsync(tenantId, request, cancellationToken);
        return new CustomArrCustomerValidationResponse(errors.Count == 0, errors);
    }

    public async Task<CustomArrDuplicateCheckResponse> CheckDuplicatesAsync(
        ClaimsPrincipal principal,
        CustomArrDuplicateCheckRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureDefaultsAsync(tenantId, principal.GetPersonId().ToString("D"), cancellationToken);

        var rules = await db.CustomerDuplicateDetectionRules
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .OrderByDescending(x => x.Priority)
            .ToListAsync(cancellationToken);

        var candidates = await db.Customers
            .AsNoTracking()
            .Include(x => x.Contacts)
            .Include(x => x.Addresses)
            .Include(x => x.Identifiers)
            .Include(x => x.ExternalRefs)
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var matches = new List<CustomArrDuplicateCandidateResponse>();
        foreach (var customer in candidates)
        {
            var score = 0;
            var reasons = new List<string>();
            var matchedRules = new List<CustomArrCustomerDuplicateDetectionRule>();
            foreach (var rule in rules)
            {
                if (!RuleMatches(rule, request, customer))
                {
                    continue;
                }

                score += Math.Max(0, rule.Weight);
                matchedRules.Add(rule);
                reasons.Add(rule.Label);
            }

            if (score == 0)
            {
                continue;
            }

            var reviewThreshold = matchedRules.Count == 0 ? 60 : matchedRules.Min(x => x.ReviewThreshold);
            var blockThreshold = matchedRules.Count == 0 ? 90 : matchedRules.Min(x => x.AutoBlockThreshold);
            var recommendation = score >= blockThreshold ? "block" : score >= reviewThreshold ? "review" : "possible";
            matches.Add(new CustomArrDuplicateCandidateResponse(
                customer.CustomerId,
                customer.CustomerNumber,
                customer.DisplayName,
                score,
                recommendation,
                reasons));
        }

        var overall = matches.Any(x => x.Recommendation == "block")
            ? "block"
            : matches.Any(x => x.Recommendation == "review")
                ? "review"
                : "clear";

        return new CustomArrDuplicateCheckResponse(overall, matches.OrderByDescending(x => x.Score).ToArray());
    }

    public async Task<CustomArrStageTransitionPreviewResponse> PreviewStageTransitionAsync(
        ClaimsPrincipal principal,
        string customerId,
        CustomArrStageTransitionPreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureDefaultsAsync(tenantId, principal.GetPersonId().ToString("D"), cancellationToken);
        var customer = await db.Customers.AsNoTracking()
            .Include(x => x.Contacts)
            .Include(x => x.Addresses)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.CustomerId == customerId, cancellationToken)
            ?? throw new StlApiException("customarr.customer_not_found", "Customer was not found in CustomArr.", 404);

        var fromStage = NormalizeKey(request.FromStageKey ?? customer.StatusKey, "prospect");
        var toStage = NormalizeKey(request.ToStageKey, string.Empty);
        var stages = await db.CustomerLifecycleStages.AsNoTracking().Where(x => x.TenantId == tenantId).ToListAsync(cancellationToken);
        var current = stages.FirstOrDefault(x => x.Key == fromStage);
        var target = stages.FirstOrDefault(x => x.Key == toStage)
            ?? throw new StlApiException("customarr.lifecycle_stage_unknown", "Target lifecycle stage is not configured for this tenant.", 400);

        var blockers = new List<string>();
        var warnings = new List<string>();
        if (current is not null && current.AllowedNextStageKeys.Length > 0 && !current.AllowedNextStageKeys.Contains(target.Key, StringComparer.OrdinalIgnoreCase))
        {
            blockers.Add($"{target.Label} is not an allowed next stage from {current.Label}.");
        }

        var rule = await db.CustomerLifecycleTransitionRules.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.FromStageKey == fromStage && x.ToStageKey == toStage, cancellationToken);
        if (rule?.RequiresApproval == true)
        {
            warnings.Add(rule.RequiredPermission is null
                ? "This transition requires approval."
                : $"This transition requires {rule.RequiredPermission}.");
        }

        if (rule?.RequiredReason == true && string.IsNullOrWhiteSpace(request.Reason))
        {
            blockers.Add("A reason is required for this lifecycle transition.");
        }

        if (rule?.BlockIfMissingRequiredFields == true || target.IsActiveCustomerStage)
        {
            var validation = await ValidateCustomerPayloadAsync(
                tenantId,
                new CustomArrCustomerValidationRequest(
                    customer.CustomerId,
                    customer.LegalName,
                    customer.DbaName,
                    customer.DisplayName,
                    target.Key,
                    customer.CustomerTypeKey,
                    customer.IndustryKey,
                    null,
                    null,
                    null,
                    null,
                    customer.AccountOwnerPersonId,
                    null,
                    null,
                    null,
                    null,
                    null,
                    customer.Contacts.Any(x => x.IsPrimary),
                    customer.Addresses.Any(x => x.IsDefaultBilling),
                    customer.Addresses.Any(x => x.StatusKey == "active")),
                cancellationToken);
            blockers.AddRange(validation.Select(x => x.Message));
        }

        return new CustomArrStageTransitionPreviewResponse(
            blockers.Count == 0,
            fromStage,
            target.Key,
            blockers,
            warnings,
            target.BlocksOrders,
            target.BlocksPortalAccess);
    }

    public async Task<IReadOnlyList<CustomerClassificationCatalogItem>> ListCatalogsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(principal, cancellationToken);
        return settings.ClassificationCatalogs;
    }

    public async Task<IReadOnlyList<CustomerClassificationCatalogItem>> ReplaceCatalogAsync(
        ClaimsPrincipal principal,
        string catalogType,
        IReadOnlyList<CustomerClassificationCatalogItem> items,
        CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(principal, cancellationToken);
        var normalizedType = NormalizeCatalogType(catalogType);
        var update = settings.ToUpdateRequest() with
        {
            ClassificationCatalogs = settings.ClassificationCatalogs
                .Where(x => !x.CatalogType.Equals(normalizedType, StringComparison.OrdinalIgnoreCase))
                .Concat(items.Select(x => x with { CatalogType = normalizedType }))
                .ToArray()
        };
        return (await UpdateSettingsAsync(principal, update, cancellationToken)).ClassificationCatalogs
            .Where(x => x.CatalogType == normalizedType)
            .ToArray();
    }

    public async Task<CustomArrTenantSettingsResponse> ReplaceSectionAsync(
        ClaimsPrincipal principal,
        Func<CustomArrTenantSettingsResponse, CustomArrTenantSettingsUpdateRequest> buildRequest,
        CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(principal, cancellationToken);
        return await UpdateSettingsAsync(principal, buildRequest(settings), cancellationToken);
    }

    public async Task EnsureDefaultsAsync(Guid tenantId, string? actorPersonId, CancellationToken cancellationToken = default)
    {
        if (await db.TenantSettings.AnyAsync(x => x.TenantId == tenantId && x.IsActive, cancellationToken))
        {
            return;
        }

        CustomArrTenantSettingsDefaults.AddDefaults(db, tenantId, actorPersonId, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<CustomArrTenantSettingsResponse> BuildResponseAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var settings = await db.TenantSettings.AsNoTracking().SingleAsync(x => x.TenantId == tenantId && x.IsActive, cancellationToken);
        var numbering = await db.CustomerNumberingSettings.AsNoTracking().SingleAsync(x => x.TenantId == tenantId, cancellationToken);
        var portal = await db.CustomerPortalTenantSettings.AsNoTracking().SingleAsync(x => x.TenantId == tenantId, cancellationToken);
        var integration = await db.CustomerIntegrationSettings.AsNoTracking().SingleAsync(x => x.TenantId == tenantId, cancellationToken);

        return new CustomArrTenantSettingsResponse(
            "tenant",
            settings.SettingsVersion,
            settings.IsActive,
            settings.EffectiveFrom,
            settings.EffectiveTo,
            settings.UpdatedAt,
            ToResponse(numbering),
            (await db.CustomerLifecycleStages.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken)).Select(ToResponse).ToArray(),
            (await db.CustomerLifecycleTransitionRules.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.FromStageKey).ThenBy(x => x.ToStageKey).ToListAsync(cancellationToken)).Select(ToResponse).ToArray(),
            (await db.CustomerClassificationCatalogs.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.CatalogType).ThenBy(x => x.SortOrder).ToListAsync(cancellationToken)).Select(ToResponse).ToArray(),
            (await db.CustomerRequiredFieldRules.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.LifecycleStageKey).ThenBy(x => x.FieldKey).ToListAsync(cancellationToken)).Select(ToResponse).ToArray(),
            (await db.CustomerContactRoles.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken)).Select(ToResponse).ToArray(),
            (await db.CustomerAddressTypes.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken)).Select(ToResponse).ToArray(),
            (await db.CustomerOwnerRules.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.Priority).ToListAsync(cancellationToken)).Select(ToResponse).ToArray(),
            (await db.CustomerOnboardingTemplates.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken)).Select(ToResponse).ToArray(),
            (await db.CustomerOnboardingChecklistItemTemplates.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.TemplateKey).ThenBy(x => x.SortOrder).ToListAsync(cancellationToken)).Select(ToResponse).ToArray(),
            ToResponse(portal),
            (await db.CustomerDocumentRequirements.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.Key).ToListAsync(cancellationToken)).Select(ToResponse).ToArray(),
            (await db.CustomerDuplicateDetectionRules.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.Priority).ToListAsync(cancellationToken)).Select(ToResponse).ToArray(),
            ToResponse(integration),
            (await db.CustomerExternalIdSources.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.Key).ToListAsync(cancellationToken)).Select(ToResponse).ToArray(),
            (await db.CustomerNotificationRules.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.EventType).ThenBy(x => x.DelayMinutes).ToListAsync(cancellationToken)).Select(ToResponse).ToArray(),
            (await db.CustomerCustomFieldDefinitions.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken)).Select(ToResponse).ToArray(),
            (await db.CustomerCustomFieldOptions.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.FieldKey).ThenBy(x => x.SortOrder).ToListAsync(cancellationToken)).Select(ToResponse).ToArray(),
            BuildWarnings());
    }

    private async Task<IReadOnlyList<CustomArrValidationError>> ValidateCustomerPayloadAsync(
        Guid tenantId,
        CustomArrCustomerValidationRequest request,
        CancellationToken cancellationToken)
    {
        var stageKey = NormalizeKey(request.LifecycleStageKey, "prospect");
        var customerTypeKey = NormalizeKey(request.CustomerTypeKey, "standard");
        var rules = await db.CustomerRequiredFieldRules.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RequirementLevel == "required" && x.AppliesToInternalCreate)
            .Where(x => x.LifecycleStageKey == null || x.LifecycleStageKey == stageKey)
            .Where(x => x.CustomerTypeKey == null || x.CustomerTypeKey == customerTypeKey)
            .ToListAsync(cancellationToken);

        var errors = new List<CustomArrValidationError>();
        foreach (var rule in rules)
        {
            var missing = rule.FieldKey switch
            {
                "legalName" => string.IsNullOrWhiteSpace(request.LegalName),
                "customerType" => string.IsNullOrWhiteSpace(request.CustomerTypeKey),
                "lifecycleStage" => string.IsNullOrWhiteSpace(request.LifecycleStageKey),
                "primaryContact" => !request.HasPrimaryContact,
                "billingAddress" => !request.HasBillingAddress,
                "accountOwner" => string.IsNullOrWhiteSpace(request.AccountOwnerRefId),
                "validAddress" => !request.HasValidAddress,
                "primaryEmail" => string.IsNullOrWhiteSpace(request.PrimaryEmail),
                "primaryPhone" => string.IsNullOrWhiteSpace(request.PrimaryPhone),
                "paymentTerms" => string.IsNullOrWhiteSpace(request.PaymentTermsKey),
                "creditStatus" => string.IsNullOrWhiteSpace(request.CreditStatusKey),
                "onboardingTemplate" => string.IsNullOrWhiteSpace(request.OnboardingTemplateKey),
                "externalId" => string.IsNullOrWhiteSpace(request.ExternalId),
                _ => false
            };

            if (missing)
            {
                errors.Add(new CustomArrValidationError(rule.FieldKey, rule.ValidationMessage));
            }
        }

        var configuredStage = await db.CustomerLifecycleStages.AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.Key == stageKey, cancellationToken);
        if (!configuredStage)
        {
            errors.Add(new CustomArrValidationError("lifecycleStageKey", "Lifecycle stage is not configured for this tenant."));
        }

        var configuredType = await db.CustomerClassificationCatalogs.AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.CatalogType == "customer_type" && x.Key == customerTypeKey && x.IsActive, cancellationToken);
        if (!configuredType)
        {
            errors.Add(new CustomArrValidationError("customerTypeKey", "Customer type is not active for this tenant."));
        }

        return errors;
    }

    private static bool RuleMatches(CustomArrCustomerDuplicateDetectionRule rule, CustomArrDuplicateCheckRequest request, CustomArrCustomer customer)
    {
        static string Normalize(string? value) => new((value ?? string.Empty).Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
        static string Domain(string? value)
        {
            var at = value?.IndexOf('@') ?? -1;
            return at >= 0 ? value![(at + 1)..].Trim().ToLowerInvariant() : string.Empty;
        }

        return rule.MatchField switch
        {
            "legalName" => Normalize(request.LegalName) == Normalize(customer.LegalName),
            "dbaName" => !string.IsNullOrWhiteSpace(request.DbaName) && Normalize(request.DbaName) == Normalize(customer.DbaName),
            "primaryEmail" => !string.IsNullOrWhiteSpace(request.PrimaryEmail) && Domain(request.PrimaryEmail).Length > 0 && request.PrimaryEmail.Equals(customer.Contacts.FirstOrDefault(x => x.IsPrimary)?.Email, StringComparison.OrdinalIgnoreCase),
            "emailDomain" => Domain(request.PrimaryEmail).Length > 0 && customer.Contacts.Any(x => Domain(x.Email) == Domain(request.PrimaryEmail)),
            "primaryPhone" => Normalize(request.PrimaryPhone) == Normalize(customer.Contacts.FirstOrDefault(x => x.IsPrimary)?.Phone),
            "taxId" => !string.IsNullOrWhiteSpace(request.TaxId) && customer.Identifiers.Any(x => Normalize(x.IdentifierValue) == Normalize(request.TaxId)),
            "externalId" => !string.IsNullOrWhiteSpace(request.ExternalId) && customer.ExternalRefs.Any(x => x.ExternalId.Equals(request.ExternalId, StringComparison.OrdinalIgnoreCase)),
            "address" => !string.IsNullOrWhiteSpace(request.AddressLine1) && customer.Addresses.Any(x => Normalize(x.Line1) == Normalize(request.AddressLine1) && Normalize(x.PostalCode) == Normalize(request.PostalCode)),
            "website" => !string.IsNullOrWhiteSpace(request.WebsiteUrl) && Normalize(customer.WebsiteUrl) == Normalize(request.WebsiteUrl),
            _ => false
        };
    }

    private static void ValidateAggregate(CustomArrTenantSettingsUpdateRequest request)
    {
        if (request.Numbering.PaddingLength is < 3 or > 12)
        {
            throw new StlApiException("customarr.settings.numbering_padding", "Customer number padding must be between 3 and 12.", 400);
        }

        ValidateUnique(request.LifecycleStages.Select(x => x.Key), "lifecycle stage");
        ValidateUnique(request.ContactRoles.Select(x => x.Key), "contact role");
        ValidateUnique(request.AddressTypes.Select(x => x.Key), "address type");
        ValidateUnique(request.OnboardingTemplates.Select(x => x.Key), "onboarding template");
        ValidateUnique(request.CustomFieldDefinitions.Select(x => x.Key), "custom field");

        foreach (var catalog in request.ClassificationCatalogs)
        {
            _ = NormalizeCatalogType(catalog.CatalogType);
        }

        foreach (var rule in request.RequiredFieldRules)
        {
            if (!AllowedRequirementLevels.Contains(rule.RequirementLevel))
            {
                throw new StlApiException("customarr.settings.requirement_level", $"Requirement level '{rule.RequirementLevel}' is not supported.", 400);
            }
        }

        foreach (var rule in request.DuplicateDetectionRules)
        {
            if (!AllowedMatchTypes.Contains(rule.MatchType))
            {
                throw new StlApiException("customarr.settings.duplicate_match_type", $"Duplicate match type '{rule.MatchType}' is not supported.", 400);
            }
        }

        foreach (var field in request.CustomFieldDefinitions)
        {
            if (!AllowedCustomFieldTypes.Contains(field.FieldType))
            {
                throw new StlApiException("customarr.settings.custom_field_type", $"Custom field type '{field.FieldType}' is not supported.", 400);
            }

            if (field.Key is "legalName" or "customerType" or "accountOwner" or "primaryContact")
            {
                throw new StlApiException("customarr.settings.custom_field_duplicate", "Custom fields cannot duplicate canonical customer fields.", 400);
            }
        }
    }

    private static void ValidateUnique(IEnumerable<string> keys, string label)
    {
        var duplicates = keys.Select(x => NormalizeKey(x, string.Empty))
            .Where(x => x.Length > 0)
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToArray();
        if (duplicates.Length > 0)
        {
            throw new StlApiException("customarr.settings.duplicate_key", $"Duplicate {label} keys are not allowed: {string.Join(", ", duplicates)}.", 400);
        }
    }

    private static string NormalizeCatalogType(string catalogType)
    {
        var normalized = NormalizeKey(catalogType, string.Empty);
        if (!AllowedCatalogTypes.Contains(normalized))
        {
            throw new StlApiException("customarr.settings.catalog_type", $"Catalog type '{catalogType}' is not supported by CustomArr customer settings.", 400);
        }

        return normalized;
    }

    private static IReadOnlyList<CustomArrSettingsWarning> BuildWarnings() =>
    [
        new("active_customer_requirements", "Changes to active-customer required fields can block activation and stage transitions until existing records are remapped or completed."),
        new("integration_writeback", "External create, update, and bidirectional sync settings affect customer integrations and should be audited before enabling.")
    ];

    private static Guid EnsureEntitled(ClaimsPrincipal principal)
    {
        if (!principal.HasProductEntitlement(StlProductKeys.CustomArr))
        {
            throw new StlApiException("customarr.not_entitled", "Active CustomArr entitlement is required.", 403);
        }

        return principal.GetTenantId();
    }

    private static Guid EnsureManage(ClaimsPrincipal principal)
    {
        var tenantId = EnsureEntitled(principal);
        var role = principal.GetTenantRoleKey();
        if (!role.Equals("tenant_admin", StringComparison.OrdinalIgnoreCase)
            && !role.Equals("product_admin", StringComparison.OrdinalIgnoreCase)
            && !role.Equals("customarr_admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("customarr.tenant_settings.manage_required", "Managing CustomArr tenant settings requires CustomArr settings administration access.", 403);
        }

        return tenantId;
    }

    private static void ReplaceCollection<TEntity>(
        Guid tenantId,
        DbSet<TEntity> set,
        IEnumerable<TEntity> values)
        where TEntity : class
    {
        set.RemoveRange(set.Where(x => EF.Property<Guid>(x, nameof(CustomArrTenantSettings.TenantId)) == tenantId));
        set.AddRange(values);
    }

    private static void ReplaceSingleton<TEntity>(
        Guid tenantId,
        DbSet<TEntity> set,
        TEntity value)
        where TEntity : class
    {
        set.RemoveRange(set.Where(x => EF.Property<Guid>(x, nameof(CustomArrTenantSettings.TenantId)) == tenantId));
        set.Add(value);
    }

    private static string NewId(string prefix) => $"{prefix}-{Guid.NewGuid():N}"[..Math.Min(prefix.Length + 33, 64)];

    internal static string NormalizeKey(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var normalized = value.Trim().Replace("-", "_", StringComparison.Ordinal).Replace(" ", "_", StringComparison.Ordinal).ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? fallback : normalized;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string[] NormalizeKeys(IReadOnlyList<string>? values) =>
        values is null
            ? []
            : values.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => NormalizeKey(x, string.Empty)).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private static CustomerNumberingSettings ToResponse(CustomArrCustomerNumberingSettings x) =>
        new(x.Prefix, x.SequenceName, x.PaddingLength, x.NextNumber, x.AllowManualOverride, x.ManualOverrideRequiresPermission, x.DisplayFormat, x.UniquenessScope, CustomArrTenantSettingsDefaults.FormatCustomerNumber(x));

    private static CustomerLifecycleStageItem ToResponse(CustomArrCustomerLifecycleStage x) =>
        new(x.Key, x.Label, x.Description, x.SortOrder, x.IsInitial, x.IsActiveCustomerStage, x.IsTerminal, x.BlocksOrders, x.BlocksPortalAccess, x.RequiresApprovalToEnter, x.RequiresReasonToExit, x.AllowedNextStageKeys, x.ColorToken, x.IsSystemRequired);

    private static CustomerLifecycleTransitionRuleItem ToResponse(CustomArrCustomerLifecycleTransitionRule x) =>
        new(x.FromStageKey, x.ToStageKey, x.RequiresApproval, x.RequiredPermission, x.RequiredChecklistTemplateKey, x.RequiredReason, x.BlockIfOpenIssues, x.BlockIfExpiredRequiredDocuments, x.BlockIfMissingRequiredFields);

    private static CustomerClassificationCatalogItem ToResponse(CustomArrCustomerClassificationCatalog x) =>
        new(x.CatalogType, x.Key, x.Label, x.Description, x.SortOrder, x.IsActive, x.IsDefault, x.MetadataKey, x.MetadataValue);

    private static CustomerRequiredFieldRuleItem ToResponse(CustomArrCustomerRequiredFieldRule x) =>
        new(x.CustomerTypeKey, x.LifecycleStageKey, x.FieldKey, x.RequirementLevel, x.ValidationMessage, x.AppliesToPortal, x.AppliesToInternalCreate, x.AppliesToInternalEdit);

    private static CustomerContactRoleItem ToResponse(CustomArrCustomerContactRole x) =>
        new(x.Key, x.Label, x.Description, x.IsRequiredForActiveCustomer, x.RequiresUniquePrimary, x.AllowsPortalAccess, x.CanReceiveOrderNotifications, x.CanReceiveBillingNotifications, x.CanReceiveComplianceNotifications, x.SortOrder, x.IsActive);

    private static CustomerAddressTypeItem ToResponse(CustomArrCustomerAddressType x) =>
        new(x.Key, x.Label, x.Description, x.IsRequiredForActiveCustomer, x.RequiresValidation, x.RequiresGeocode, x.UsableForBilling, x.UsableForPickup, x.UsableForDelivery, x.UsableForService, x.SortOrder, x.IsActive);

    private static CustomerOwnerRuleItem ToResponse(CustomArrCustomerOwnerRule x) =>
        new(x.RuleName, x.Priority, x.IsActive, x.CustomerTypeKey, x.TerritoryKey, x.IndustryKey, x.SourceKey, x.DefaultOwnerType, x.DefaultOwnerRefId, x.DefaultOwnerNameSnapshot, x.RequiresOwnerForActiveCustomer, x.RequiresApprovalForReassignment, x.ApprovalPermission);

    private static CustomerOnboardingTemplateItem ToResponse(CustomArrCustomerOnboardingTemplate x) =>
        new(x.Key, x.Label, x.Description, x.CustomerTypeKey, x.IndustryKey, x.PriorityTierKey, x.IsDefault, x.IsActive, x.BlocksActivationUntilComplete, x.SortOrder);

    private static CustomerOnboardingChecklistItemTemplateItem ToResponse(CustomArrCustomerOnboardingChecklistItemTemplate x) =>
        new(x.TemplateKey, x.Key, x.Label, x.Description, x.ItemType, x.Required, x.SortOrder, x.OwnerType, x.OwnerRefId, x.OwnerNameSnapshot, x.DocumentTypeKey, x.ComplianceQuestionnaireKey, x.BlocksActivation, x.BlocksOrders, x.BlocksPortalAccess);

    private static CustomerPortalTenantSettingsItem ToResponse(CustomArrCustomerPortalTenantSettings x) =>
        new(x.PortalEnabled, x.InviteOnly, x.SelfRegistrationAllowed, x.RequireEmailVerification, x.RequireInternalApprovalForPortalUsers, x.AllowedEmailDomains, x.SupportContactName, x.SupportContactEmail, x.SupportContactPhone, x.PortalDisplayName, x.LogoRecordArrDocumentId, new CustomerPortalActionFlags(x.CanViewProfile, x.CanRequestQuote, x.CanPlaceOrderRequest, x.CanUploadDocuments, x.CanSubmitIssue, x.CanViewOrderStatus, x.CanViewInvoicesSnapshot), x.DefaultPortalContactRoleKey, x.PortalAdminContactRoleKey);

    private static CustomerDocumentRequirementItem ToResponse(CustomArrCustomerDocumentRequirement x) =>
        new(x.Key, x.Label, x.Description, x.CustomerTypeKey, x.LifecycleStageKey, x.Required, x.Expires, x.ExpirationWarningDays, x.RecordArrDocumentTypeKey, x.CustomerCanUpload, x.VisibleInPortal, x.BlocksActivation, x.BlocksOrders, x.BlocksPortalAccess);

    private static CustomerDuplicateDetectionRuleItem ToResponse(CustomArrCustomerDuplicateDetectionRule x) =>
        new(x.Key, x.Label, x.IsActive, x.Priority, x.MatchField, x.MatchType, x.Weight, x.AutoBlockThreshold, x.ReviewThreshold);

    private static CustomerIntegrationSettingsItem ToResponse(CustomArrCustomerIntegrationSettings x) =>
        new(x.ErpSyncMode, x.DefaultConflictResolution, x.EmitEventsForDraftCustomers, x.EmitEventsForProspects, x.EmitEventsOnlyAfterActivation, x.AllowExternalCreate, x.AllowExternalUpdate, x.RequireReviewForExternalUpdate);

    private static CustomerExternalIdSourceItem ToResponse(CustomArrCustomerExternalIdSource x) =>
        new(x.Key, x.Label, x.SourceType, x.Required, x.UniqueWithinTenant, x.VisibleInUi, x.EditableInUi, x.IsActive);

    private static CustomerNotificationRuleItem ToResponse(CustomArrCustomerNotificationRule x) =>
        new(x.Key, x.Label, x.EventType, x.IsActive, x.RecipientType, x.RecipientRefId, x.RecipientNameSnapshot, x.CustomerContactRoleKey, x.DelayMinutes, x.EscalationAfterMinutes, x.TemplateKey);

    private static CustomerCustomFieldDefinitionItem ToResponse(CustomArrCustomerCustomFieldDefinition x) =>
        new(x.Key, x.Label, x.Description, x.FieldType, x.AppliesToCustomerTypeKey, x.AppliesToLifecycleStageKey, x.Required, x.VisibleInPortal, x.EditableInPortal, x.InternalOnly, x.SortOrder, x.IsActive);

    private static CustomerCustomFieldOptionItem ToResponse(CustomArrCustomerCustomFieldOption x) =>
        new(x.FieldKey, x.Key, x.Label, x.SortOrder, x.IsActive);

    private static CustomArrCustomerNumberingSettings ToEntity(Guid tenantId, CustomerNumberingSettings x) =>
        new()
        {
            Id = $"num-{tenantId:N}",
            TenantId = tenantId,
            Prefix = NormalizeKey(x.Prefix, "CUS").ToUpperInvariant(),
            SequenceName = NormalizeKey(x.SequenceName, "customarr_customer"),
            PaddingLength = x.PaddingLength,
            NextNumber = Math.Max(1, x.NextNumber),
            AllowManualOverride = x.AllowManualOverride,
            ManualOverrideRequiresPermission = x.ManualOverrideRequiresPermission,
            DisplayFormat = string.IsNullOrWhiteSpace(x.DisplayFormat) ? "{prefix}-{number}" : x.DisplayFormat.Trim(),
            UniquenessScope = NormalizeKey(x.UniquenessScope, "tenant")
        };

    private static CustomArrCustomerLifecycleStage ToEntity(Guid tenantId, CustomerLifecycleStageItem x) =>
        new() { Id = NewId("stage"), TenantId = tenantId, Key = NormalizeKey(x.Key, string.Empty), Label = x.Label.Trim(), Description = x.Description.Trim(), SortOrder = x.SortOrder, IsInitial = x.IsInitial, IsActiveCustomerStage = x.IsActiveCustomerStage, IsTerminal = x.IsTerminal, BlocksOrders = x.BlocksOrders, BlocksPortalAccess = x.BlocksPortalAccess, RequiresApprovalToEnter = x.RequiresApprovalToEnter, RequiresReasonToExit = x.RequiresReasonToExit, AllowedNextStageKeys = NormalizeKeys(x.AllowedNextStageKeys), ColorToken = NormalizeOptional(x.ColorToken), IsSystemRequired = x.IsSystemRequired };

    private static CustomArrCustomerLifecycleTransitionRule ToEntity(Guid tenantId, CustomerLifecycleTransitionRuleItem x) =>
        new() { Id = NewId("transition"), TenantId = tenantId, FromStageKey = NormalizeKey(x.FromStageKey, string.Empty), ToStageKey = NormalizeKey(x.ToStageKey, string.Empty), RequiresApproval = x.RequiresApproval, RequiredPermission = NormalizeOptional(x.RequiredPermission), RequiredChecklistTemplateKey = NormalizeOptional(x.RequiredChecklistTemplateKey), RequiredReason = x.RequiredReason, BlockIfOpenIssues = x.BlockIfOpenIssues, BlockIfExpiredRequiredDocuments = x.BlockIfExpiredRequiredDocuments, BlockIfMissingRequiredFields = x.BlockIfMissingRequiredFields };

    private static CustomArrCustomerClassificationCatalog ToEntity(Guid tenantId, CustomerClassificationCatalogItem x) =>
        new() { Id = NewId("catalog"), TenantId = tenantId, CatalogType = NormalizeCatalogType(x.CatalogType), Key = NormalizeKey(x.Key, string.Empty), Label = x.Label.Trim(), Description = x.Description.Trim(), SortOrder = x.SortOrder, IsActive = x.IsActive, IsDefault = x.IsDefault, MetadataKey = NormalizeOptional(x.MetadataKey), MetadataValue = NormalizeOptional(x.MetadataValue) };

    private static CustomArrCustomerRequiredFieldRule ToEntity(Guid tenantId, CustomerRequiredFieldRuleItem x) =>
        new() { Id = NewId("fieldrule"), TenantId = tenantId, CustomerTypeKey = NormalizeOptional(x.CustomerTypeKey), LifecycleStageKey = NormalizeOptional(x.LifecycleStageKey), FieldKey = x.FieldKey.Trim(), RequirementLevel = NormalizeKey(x.RequirementLevel, "optional"), ValidationMessage = string.IsNullOrWhiteSpace(x.ValidationMessage) ? $"{x.FieldKey} is required." : x.ValidationMessage.Trim(), AppliesToPortal = x.AppliesToPortal, AppliesToInternalCreate = x.AppliesToInternalCreate, AppliesToInternalEdit = x.AppliesToInternalEdit };

    private static CustomArrCustomerContactRole ToEntity(Guid tenantId, CustomerContactRoleItem x) =>
        new() { Id = NewId("role"), TenantId = tenantId, Key = NormalizeKey(x.Key, string.Empty), Label = x.Label.Trim(), Description = x.Description.Trim(), IsRequiredForActiveCustomer = x.IsRequiredForActiveCustomer, RequiresUniquePrimary = x.RequiresUniquePrimary, AllowsPortalAccess = x.AllowsPortalAccess, CanReceiveOrderNotifications = x.CanReceiveOrderNotifications, CanReceiveBillingNotifications = x.CanReceiveBillingNotifications, CanReceiveComplianceNotifications = x.CanReceiveComplianceNotifications, SortOrder = x.SortOrder, IsActive = x.IsActive };

    private static CustomArrCustomerAddressType ToEntity(Guid tenantId, CustomerAddressTypeItem x) =>
        new() { Id = NewId("addrtype"), TenantId = tenantId, Key = NormalizeKey(x.Key, string.Empty), Label = x.Label.Trim(), Description = x.Description.Trim(), IsRequiredForActiveCustomer = x.IsRequiredForActiveCustomer, RequiresValidation = x.RequiresValidation, RequiresGeocode = x.RequiresGeocode, UsableForBilling = x.UsableForBilling, UsableForPickup = x.UsableForPickup, UsableForDelivery = x.UsableForDelivery, UsableForService = x.UsableForService, SortOrder = x.SortOrder, IsActive = x.IsActive };

    private static CustomArrCustomerOwnerRule ToEntity(Guid tenantId, CustomerOwnerRuleItem x) =>
        new() { Id = NewId("ownerrule"), TenantId = tenantId, RuleName = x.RuleName.Trim(), Priority = x.Priority, IsActive = x.IsActive, CustomerTypeKey = NormalizeOptional(x.CustomerTypeKey), TerritoryKey = NormalizeOptional(x.TerritoryKey), IndustryKey = NormalizeOptional(x.IndustryKey), SourceKey = NormalizeOptional(x.SourceKey), DefaultOwnerType = NormalizeKey(x.DefaultOwnerType, "staffarr_person"), DefaultOwnerRefId = x.DefaultOwnerRefId.Trim(), DefaultOwnerNameSnapshot = x.DefaultOwnerNameSnapshot.Trim(), RequiresOwnerForActiveCustomer = x.RequiresOwnerForActiveCustomer, RequiresApprovalForReassignment = x.RequiresApprovalForReassignment, ApprovalPermission = NormalizeOptional(x.ApprovalPermission) };

    private static CustomArrCustomerOnboardingTemplate ToEntity(Guid tenantId, CustomerOnboardingTemplateItem x) =>
        new() { Id = NewId("onbtemplate"), TenantId = tenantId, Key = NormalizeKey(x.Key, string.Empty), Label = x.Label.Trim(), Description = x.Description.Trim(), CustomerTypeKey = NormalizeOptional(x.CustomerTypeKey), IndustryKey = NormalizeOptional(x.IndustryKey), PriorityTierKey = NormalizeOptional(x.PriorityTierKey), IsDefault = x.IsDefault, IsActive = x.IsActive, BlocksActivationUntilComplete = x.BlocksActivationUntilComplete, SortOrder = x.SortOrder };

    private static CustomArrCustomerOnboardingChecklistItemTemplate ToEntity(Guid tenantId, CustomerOnboardingChecklistItemTemplateItem x) =>
        new() { Id = NewId("onbitem"), TenantId = tenantId, TemplateKey = NormalizeKey(x.TemplateKey, string.Empty), Key = NormalizeKey(x.Key, string.Empty), Label = x.Label.Trim(), Description = x.Description.Trim(), ItemType = NormalizeKey(x.ItemType, "task"), Required = x.Required, SortOrder = x.SortOrder, OwnerType = NormalizeOptional(x.OwnerType), OwnerRefId = NormalizeOptional(x.OwnerRefId), OwnerNameSnapshot = NormalizeOptional(x.OwnerNameSnapshot), DocumentTypeKey = NormalizeOptional(x.DocumentTypeKey), ComplianceQuestionnaireKey = NormalizeOptional(x.ComplianceQuestionnaireKey), BlocksActivation = x.BlocksActivation, BlocksOrders = x.BlocksOrders, BlocksPortalAccess = x.BlocksPortalAccess };

    private static CustomArrCustomerPortalTenantSettings ToEntity(Guid tenantId, CustomerPortalTenantSettingsItem x) =>
        new() { Id = $"portal-{tenantId:N}", TenantId = tenantId, PortalEnabled = x.PortalEnabled, InviteOnly = x.InviteOnly, SelfRegistrationAllowed = x.SelfRegistrationAllowed, RequireEmailVerification = x.RequireEmailVerification, RequireInternalApprovalForPortalUsers = x.RequireInternalApprovalForPortalUsers, AllowedEmailDomains = NormalizeKeys(x.AllowedEmailDomains), SupportContactName = x.SupportContactName.Trim(), SupportContactEmail = x.SupportContactEmail.Trim(), SupportContactPhone = x.SupportContactPhone.Trim(), PortalDisplayName = x.PortalDisplayName.Trim(), LogoRecordArrDocumentId = NormalizeOptional(x.LogoRecordArrDocumentId), CanViewProfile = x.AllowedActions.CanViewProfile, CanRequestQuote = x.AllowedActions.CanRequestQuote, CanPlaceOrderRequest = x.AllowedActions.CanPlaceOrderRequest, CanUploadDocuments = x.AllowedActions.CanUploadDocuments, CanSubmitIssue = x.AllowedActions.CanSubmitIssue, CanViewOrderStatus = x.AllowedActions.CanViewOrderStatus, CanViewInvoicesSnapshot = x.AllowedActions.CanViewInvoicesSnapshot, DefaultPortalContactRoleKey = NormalizeKey(x.DefaultPortalContactRoleKey, "primary"), PortalAdminContactRoleKey = NormalizeKey(x.PortalAdminContactRoleKey, "portal_admin") };

    private static CustomArrCustomerDocumentRequirement ToEntity(Guid tenantId, CustomerDocumentRequirementItem x) =>
        new() { Id = NewId("docreq"), TenantId = tenantId, Key = NormalizeKey(x.Key, string.Empty), Label = x.Label.Trim(), Description = x.Description.Trim(), CustomerTypeKey = NormalizeOptional(x.CustomerTypeKey), LifecycleStageKey = NormalizeOptional(x.LifecycleStageKey), Required = x.Required, Expires = x.Expires, ExpirationWarningDays = x.ExpirationWarningDays, RecordArrDocumentTypeKey = NormalizeKey(x.RecordArrDocumentTypeKey, "customer_document"), CustomerCanUpload = x.CustomerCanUpload, VisibleInPortal = x.VisibleInPortal, BlocksActivation = x.BlocksActivation, BlocksOrders = x.BlocksOrders, BlocksPortalAccess = x.BlocksPortalAccess };

    private static CustomArrCustomerDuplicateDetectionRule ToEntity(Guid tenantId, CustomerDuplicateDetectionRuleItem x) =>
        new() { Id = NewId("duperule"), TenantId = tenantId, Key = NormalizeKey(x.Key, string.Empty), Label = x.Label.Trim(), IsActive = x.IsActive, Priority = x.Priority, MatchField = x.MatchField.Trim(), MatchType = NormalizeKey(x.MatchType, "exact"), Weight = x.Weight, AutoBlockThreshold = x.AutoBlockThreshold, ReviewThreshold = x.ReviewThreshold };

    private static CustomArrCustomerIntegrationSettings ToEntity(Guid tenantId, CustomerIntegrationSettingsItem x) =>
        new() { Id = $"integration-{tenantId:N}", TenantId = tenantId, ErpSyncMode = NormalizeKey(x.ErpSyncMode, "none"), DefaultConflictResolution = NormalizeKey(x.DefaultConflictResolution, "manual_review"), EmitEventsForDraftCustomers = x.EmitEventsForDraftCustomers, EmitEventsForProspects = x.EmitEventsForProspects, EmitEventsOnlyAfterActivation = x.EmitEventsOnlyAfterActivation, AllowExternalCreate = x.AllowExternalCreate, AllowExternalUpdate = x.AllowExternalUpdate, RequireReviewForExternalUpdate = x.RequireReviewForExternalUpdate };

    private static CustomArrCustomerExternalIdSource ToEntity(Guid tenantId, CustomerExternalIdSourceItem x) =>
        new() { Id = NewId("externalid"), TenantId = tenantId, Key = NormalizeKey(x.Key, string.Empty), Label = x.Label.Trim(), SourceType = NormalizeKey(x.SourceType, "erp"), Required = x.Required, UniqueWithinTenant = x.UniqueWithinTenant, VisibleInUi = x.VisibleInUi, EditableInUi = x.EditableInUi, IsActive = x.IsActive };

    private static CustomArrCustomerNotificationRule ToEntity(Guid tenantId, CustomerNotificationRuleItem x) =>
        new() { Id = NewId("notify"), TenantId = tenantId, Key = NormalizeKey(x.Key, string.Empty), Label = x.Label.Trim(), EventType = NormalizeKey(x.EventType, string.Empty), IsActive = x.IsActive, RecipientType = NormalizeKey(x.RecipientType, "account_owner"), RecipientRefId = NormalizeOptional(x.RecipientRefId), RecipientNameSnapshot = NormalizeOptional(x.RecipientNameSnapshot), CustomerContactRoleKey = NormalizeOptional(x.CustomerContactRoleKey), DelayMinutes = x.DelayMinutes, EscalationAfterMinutes = x.EscalationAfterMinutes, TemplateKey = NormalizeOptional(x.TemplateKey) };

    private static CustomArrCustomerCustomFieldDefinition ToEntity(Guid tenantId, CustomerCustomFieldDefinitionItem x) =>
        new() { Id = NewId("customfield"), TenantId = tenantId, Key = NormalizeKey(x.Key, string.Empty), Label = x.Label.Trim(), Description = x.Description.Trim(), FieldType = NormalizeKey(x.FieldType, "text"), AppliesToCustomerTypeKey = NormalizeOptional(x.AppliesToCustomerTypeKey), AppliesToLifecycleStageKey = NormalizeOptional(x.AppliesToLifecycleStageKey), Required = x.Required, VisibleInPortal = x.VisibleInPortal, EditableInPortal = x.EditableInPortal, InternalOnly = x.InternalOnly, SortOrder = x.SortOrder, IsActive = x.IsActive };

    private static CustomArrCustomerCustomFieldOption ToEntity(Guid tenantId, CustomerCustomFieldOptionItem x) =>
        new() { Id = NewId("customoption"), TenantId = tenantId, FieldKey = NormalizeKey(x.FieldKey, string.Empty), Key = NormalizeKey(x.Key, string.Empty), Label = x.Label.Trim(), SortOrder = x.SortOrder, IsActive = x.IsActive };

    private static string BuildAuditSummary(CustomArrTenantSettingsUpdateRequest request) =>
        string.Join("; ", new[]
        {
            $"numbering={request.Numbering.Prefix}-{request.Numbering.NextNumber}",
            $"lifecycleStages={request.LifecycleStages.Count}",
            $"catalogValues={request.ClassificationCatalogs.Count}",
            $"requiredFieldRules={request.RequiredFieldRules.Count}",
            $"contactRoles={request.ContactRoles.Count}",
            $"addressTypes={request.AddressTypes.Count}",
            $"onboardingTemplates={request.OnboardingTemplates.Count}",
            $"documentRequirements={request.DocumentRequirements.Count}",
            $"duplicateRules={request.DuplicateDetectionRules.Count}",
            $"externalIdSources={request.ExternalIdSources.Count}",
            $"notificationRules={request.NotificationRules.Count}",
            $"customFields={request.CustomFieldDefinitions.Count}"
        });
}

public static class CustomArrTenantSettingsDefaults
{
    public static void EnsureSeeded(CustomArrDbContext db, Guid tenantId, string? actorPersonId = null)
    {
        if (db.TenantSettings.Any(x => x.TenantId == tenantId && x.IsActive))
        {
            return;
        }

        AddDefaults(db, tenantId, actorPersonId, DateTimeOffset.UtcNow);
        db.SaveChanges();
    }

    public static void AddDefaults(CustomArrDbContext db, Guid tenantId, string? actorPersonId, DateTimeOffset now)
    {
        var bundle = Build(tenantId, actorPersonId, now);
        db.TenantSettings.Add(bundle.Settings);
        db.CustomerNumberingSettings.Add(bundle.Numbering);
        db.CustomerLifecycleStages.AddRange(bundle.LifecycleStages);
        db.CustomerLifecycleTransitionRules.AddRange(bundle.TransitionRules);
        db.CustomerClassificationCatalogs.AddRange(bundle.Catalogs);
        db.CustomerRequiredFieldRules.AddRange(bundle.RequiredFields);
        db.CustomerContactRoles.AddRange(bundle.ContactRoles);
        db.CustomerAddressTypes.AddRange(bundle.AddressTypes);
        db.CustomerOwnerRules.AddRange(bundle.OwnerRules);
        db.CustomerOnboardingTemplates.AddRange(bundle.OnboardingTemplates);
        db.CustomerOnboardingChecklistItemTemplates.AddRange(bundle.OnboardingItems);
        db.CustomerPortalTenantSettings.Add(bundle.Portal);
        db.CustomerDocumentRequirements.AddRange(bundle.DocumentRequirements);
        db.CustomerDuplicateDetectionRules.AddRange(bundle.DuplicateRules);
        db.CustomerIntegrationSettings.Add(bundle.Integration);
        db.CustomerExternalIdSources.AddRange(bundle.ExternalIdSources);
        db.CustomerNotificationRules.AddRange(bundle.NotificationRules);
        db.CustomerCustomFieldDefinitions.AddRange(bundle.CustomFields);
        db.CustomerCustomFieldOptions.AddRange(bundle.CustomFieldOptions);
        db.TenantSettingsAuditEvents.Add(new CustomArrTenantSettingsAuditEvent
        {
            Id = $"settingsaudit-{Guid.NewGuid():N}"[..46],
            TenantId = tenantId,
            SettingsVersion = bundle.Settings.SettingsVersion,
            Scope = "tenant",
            SectionKey = "bootstrap",
            ChangeSummary = "Seeded CustomArr tenant customer settings defaults.",
            ActorPersonId = actorPersonId,
            OccurredAt = now,
            SourceProductKey = StlProductKeys.CustomArr
        });
    }

    public static string FormatCustomerNumber(CustomArrCustomerNumberingSettings numbering)
    {
        var padded = numbering.NextNumber.ToString(new string('0', Math.Max(1, numbering.PaddingLength)));
        return numbering.DisplayFormat
            .Replace("{prefix}", numbering.Prefix, StringComparison.OrdinalIgnoreCase)
            .Replace("{number}", padded, StringComparison.OrdinalIgnoreCase);
    }

    private static DefaultSettingsBundle Build(Guid tenantId, string? actorPersonId, DateTimeOffset now)
    {
        static string Id(string prefix) => $"{prefix}-{Guid.NewGuid():N}"[..Math.Min(prefix.Length + 33, 64)];
        static CustomArrCustomerClassificationCatalog Catalog(Guid tenantId, string type, string key, string label, int order, bool isDefault = false, string description = "") =>
            new() { Id = Id("catalog"), TenantId = tenantId, CatalogType = type, Key = key, Label = label, Description = description, SortOrder = order, IsActive = true, IsDefault = isDefault };

        var stages = new[]
        {
            Stage(tenantId, "lead", "Lead", "Unqualified customer interest.", 10, true, false, false, true, true, ["prospect", "lost"], "cyan", true),
            Stage(tenantId, "prospect", "Prospect", "Potential customer being evaluated.", 20, false, false, false, false, false, ["qualified", "lost"], "sky", true),
            Stage(tenantId, "qualified", "Qualified", "Fit and need are confirmed.", 30, false, false, false, false, false, ["onboarding", "lost"], "blue", true),
            Stage(tenantId, "onboarding", "Onboarding", "Customer setup and requirements are in progress.", 40, false, false, false, true, false, ["active", "suspended", "inactive"], "amber", true),
            Stage(tenantId, "active", "Active", "Customer can be used by downstream execution workflows.", 50, false, true, false, false, false, ["suspended", "inactive"], "emerald", true),
            Stage(tenantId, "suspended", "Suspended", "Customer is temporarily blocked from orders or portal access.", 60, false, false, false, true, true, ["active", "inactive"], "rose", true),
            Stage(tenantId, "inactive", "Inactive", "Customer is no longer active but remains retained.", 70, false, false, true, true, true, ["active"], "slate", true),
            Stage(tenantId, "lost", "Lost", "Prospect did not convert.", 80, false, false, true, true, true, [], "slate", true)
        };

        var catalogs = new List<CustomArrCustomerClassificationCatalog>
        {
            Catalog(tenantId, "customer_type", "standard", "Standard", 10, true),
            Catalog(tenantId, "customer_type", "shipper", "Shipper", 20),
            Catalog(tenantId, "customer_type", "consignee", "Consignee", 30),
            Catalog(tenantId, "customer_type", "broker_customer", "Broker Customer", 40),
            Catalog(tenantId, "customer_type", "distributor", "Distributor", 50),
            Catalog(tenantId, "customer_type", "manufacturer", "Manufacturer", 60),
            Catalog(tenantId, "customer_type", "retailer", "Retailer", 70),
            Catalog(tenantId, "industry", "general_business", "General Business", 10, true),
            Catalog(tenantId, "industry", "logistics", "Logistics", 20),
            Catalog(tenantId, "industry", "manufacturing", "Manufacturing", 30),
            Catalog(tenantId, "priority_tier", "standard", "Standard", 10, true),
            Catalog(tenantId, "priority_tier", "core", "Core", 20),
            Catalog(tenantId, "priority_tier", "strategic", "Strategic", 30),
            Catalog(tenantId, "service_model", "standard", "Standard", 10, true),
            Catalog(tenantId, "service_model", "scheduled", "Scheduled", 20),
            Catalog(tenantId, "service_model", "dedicated", "Dedicated", 30),
            Catalog(tenantId, "territory", "default", "Default Territory", 10, true),
            Catalog(tenantId, "tag", "key_account", "Key Account", 10),
            Catalog(tenantId, "lost_reason", "duplicate", "Duplicate", 10),
            Catalog(tenantId, "lost_reason", "price", "Price", 20),
            Catalog(tenantId, "lost_reason", "service_area", "Service Area", 30),
            Catalog(tenantId, "lost_reason", "credit_risk", "Credit Risk", 40),
            Catalog(tenantId, "lost_reason", "other", "Other", 90, true),
            Catalog(tenantId, "inactive_reason", "duplicate", "Duplicate", 10),
            Catalog(tenantId, "inactive_reason", "contract_ended", "Contract Ended", 20),
            Catalog(tenantId, "inactive_reason", "customer_closed", "Customer Closed", 30),
            Catalog(tenantId, "inactive_reason", "no_activity", "No Activity", 40),
            Catalog(tenantId, "inactive_reason", "other", "Other", 90, true),
            Catalog(tenantId, "credit_status", "normal", "Normal", 10, true),
            Catalog(tenantId, "credit_status", "review", "Review", 20),
            Catalog(tenantId, "credit_status", "credit_hold", "Credit Hold", 30),
            Catalog(tenantId, "credit_status", "prepaid_only", "Prepaid Only", 40),
            Catalog(tenantId, "payment_terms", "prepaid", "Prepaid", 10),
            Catalog(tenantId, "payment_terms", "due_on_receipt", "Due On Receipt", 20),
            Catalog(tenantId, "payment_terms", "net_15", "Net 15", 30),
            Catalog(tenantId, "payment_terms", "net_30", "Net 30", 40, true),
            Catalog(tenantId, "payment_terms", "net_45", "Net 45", 50),
            Catalog(tenantId, "contract_status", "none", "None", 10, true),
            Catalog(tenantId, "contract_status", "pending", "Pending", 20),
            Catalog(tenantId, "contract_status", "active", "Active", 30),
            Catalog(tenantId, "contract_status", "expired", "Expired", 40),
            Catalog(tenantId, "contract_status", "terminated", "Terminated", 50)
        };

        return new DefaultSettingsBundle(
            new CustomArrTenantSettings
            {
                Id = $"settings-{tenantId:N}",
                TenantId = tenantId,
                SettingsVersion = 1,
                IsActive = true,
                EffectiveFrom = now,
                CreatedAt = now,
                CreatedByPersonId = actorPersonId,
                UpdatedAt = now,
                UpdatedByPersonId = actorPersonId
            },
            new CustomArrCustomerNumberingSettings
            {
                Id = $"num-{tenantId:N}",
                TenantId = tenantId,
                Prefix = "CUS",
                SequenceName = "customarr_customer",
                PaddingLength = 4,
                NextNumber = 1001,
                AllowManualOverride = false,
                ManualOverrideRequiresPermission = true,
                DisplayFormat = "{prefix}-{number}",
                UniquenessScope = "tenant"
            },
            stages,
            [
                Transition(tenantId, "qualified", "onboarding", true, "customarr.customer.transition", "default_customer_onboarding", true, false, false, true),
                Transition(tenantId, "onboarding", "active", true, "customarr.customer.activate", "default_customer_onboarding", false, true, true, true),
                Transition(tenantId, "active", "suspended", true, "customarr.customer.suspend", null, true, false, false, false)
            ],
            catalogs,
            [
                Required(tenantId, "active", "legalName", "Legal name is required before activation."),
                Required(tenantId, "active", "customerType", "Customer type is required before activation."),
                Required(tenantId, "active", "primaryContact", "At least one primary customer contact is required before activation."),
                Required(tenantId, "active", "billingAddress", "A billing address is required before activation."),
                Required(tenantId, "active", "accountOwner", "A StaffArr account owner reference is required before activation."),
                Required(tenantId, "active", "lifecycleStage", "Lifecycle stage is required before activation."),
                Required(tenantId, "active", "validAddress", "At least one active customer address is required before activation.")
            ],
            [
                ContactRole(tenantId, "primary", "Primary", 10, true, true, true, true, true, true),
                ContactRole(tenantId, "billing", "Billing", 20, true, true, false, false, true, false),
                ContactRole(tenantId, "accounts_payable", "Accounts Payable", 30, false, false, false, false, true, false),
                ContactRole(tenantId, "operations", "Operations", 40, false, false, true, true, false, false),
                ContactRole(tenantId, "ordering", "Ordering", 50, false, false, true, true, false, false),
                ContactRole(tenantId, "after_hours", "After Hours", 60, false, false, false, true, false, false),
                ContactRole(tenantId, "safety", "Safety", 70, false, false, false, false, false, true),
                ContactRole(tenantId, "compliance", "Compliance", 80, false, false, false, false, false, true),
                ContactRole(tenantId, "executive", "Executive", 90, false, false, false, true, true, true),
                ContactRole(tenantId, "portal_admin", "Portal Admin", 100, false, true, true, true, true, true)
            ],
            [
                AddressType(tenantId, "headquarters", "Headquarters", 10, false, true, false, false, false, false, true),
                AddressType(tenantId, "billing", "Billing", 20, true, true, false, true, false, false, false),
                AddressType(tenantId, "shipping", "Shipping", 30, false, true, true, false, true, true, true),
                AddressType(tenantId, "pickup", "Pickup", 40, false, true, true, false, true, false, true),
                AddressType(tenantId, "delivery", "Delivery", 50, false, true, true, false, false, true, true),
                AddressType(tenantId, "service", "Service", 60, false, true, true, false, true, true, true),
                AddressType(tenantId, "remittance", "Remittance", 70, false, true, false, true, false, false, false)
            ],
            [
                new CustomArrCustomerOwnerRule { Id = Id("ownerrule"), TenantId = tenantId, RuleName = "Require account owner for active customers", Priority = 10, IsActive = true, DefaultOwnerType = "staffarr_person", DefaultOwnerRefId = string.Empty, DefaultOwnerNameSnapshot = "Select a StaffArr account owner", RequiresOwnerForActiveCustomer = true, RequiresApprovalForReassignment = true, ApprovalPermission = "customarr.customer.owner.reassign" }
            ],
            [
                new CustomArrCustomerOnboardingTemplate { Id = Id("onbtemplate"), TenantId = tenantId, Key = "default_customer_onboarding", Label = "Default Customer Onboarding", Description = "Default checklist for activating a customer.", IsDefault = true, IsActive = true, BlocksActivationUntilComplete = true, SortOrder = 10 }
            ],
            [
                ChecklistItem(tenantId, "default_customer_onboarding", "confirm_profile", "Confirm customer profile", "task", 10, true, true, false, false),
                ChecklistItem(tenantId, "default_customer_onboarding", "verify_billing", "Verify billing setup", "approval", 20, true, true, true, false),
                ChecklistItem(tenantId, "default_customer_onboarding", "collect_required_documents", "Collect required documents", "document", 30, true, true, true, true)
            ],
            new CustomArrCustomerPortalTenantSettings
            {
                Id = $"portal-{tenantId:N}",
                TenantId = tenantId,
                PortalEnabled = true,
                InviteOnly = true,
                SelfRegistrationAllowed = false,
                RequireEmailVerification = true,
                RequireInternalApprovalForPortalUsers = true,
                PortalDisplayName = "Customer Portal",
                SupportContactName = "Customer Success",
                SupportContactEmail = "support@example.com",
                CanViewProfile = true,
                CanRequestQuote = true,
                CanPlaceOrderRequest = false,
                CanUploadDocuments = true,
                CanSubmitIssue = true,
                CanViewOrderStatus = true,
                CanViewInvoicesSnapshot = false,
                DefaultPortalContactRoleKey = "primary",
                PortalAdminContactRoleKey = "portal_admin"
            },
            [
                DocumentRequirement(tenantId, "certificate_of_insurance", "Certificate Of Insurance", "customer_document.coi", true, true, 30, true, true, true, true),
                DocumentRequirement(tenantId, "tax_registration", "Tax Registration / W-9", "customer_document.tax", true, false, null, true, true, true, false)
            ],
            [
                DuplicateRule(tenantId, "external_id_exact", "Exact External ID", 10, "externalId", "external_id", 100, 100, 70),
                DuplicateRule(tenantId, "tax_id_exact", "Exact Tax ID", 20, "taxId", "tax_id", 100, 100, 70),
                DuplicateRule(tenantId, "legal_name_review", "Normalized Legal Name Review", 30, "legalName", "normalized", 35, 999, 60),
                DuplicateRule(tenantId, "address_review", "Address Review", 35, "address", "normalized", 35, 999, 60),
                DuplicateRule(tenantId, "email_domain_review", "Email Domain Review", 40, "emailDomain", "domain", 30, 999, 60),
                DuplicateRule(tenantId, "phone_review", "Phone Review", 50, "primaryPhone", "phone", 30, 999, 60)
            ],
            new CustomArrCustomerIntegrationSettings
            {
                Id = $"integration-{tenantId:N}",
                TenantId = tenantId,
                ErpSyncMode = "review_queue",
                DefaultConflictResolution = "manual_review",
                EmitEventsForDraftCustomers = false,
                EmitEventsForProspects = true,
                EmitEventsOnlyAfterActivation = false,
                AllowExternalCreate = false,
                AllowExternalUpdate = false,
                RequireReviewForExternalUpdate = true
            },
            [
                new CustomArrCustomerExternalIdSource { Id = Id("externalid"), TenantId = tenantId, Key = "erp_customer", Label = "ERP Customer", SourceType = "erp", Required = false, UniqueWithinTenant = true, VisibleInUi = true, EditableInUi = true, IsActive = true },
                new CustomArrCustomerExternalIdSource { Id = Id("externalid"), TenantId = tenantId, Key = "legacy_customer", Label = "Legacy Customer", SourceType = "legacy", Required = false, UniqueWithinTenant = true, VisibleInUi = true, EditableInUi = false, IsActive = true }
            ],
            [
                NotificationRule(tenantId, "customer_created_owner", "Customer Created", "customer_created", "account_owner", 0, null),
                NotificationRule(tenantId, "document_expiring_owner", "Document Expiring", "customer_document_expiring", "account_owner", 0, 1440),
                NotificationRule(tenantId, "duplicate_detected_team", "Duplicate Detected", "customer_duplicate_detected", "owner_team", 0, null)
            ],
            [
                new CustomArrCustomerCustomFieldDefinition { Id = Id("customfield"), TenantId = tenantId, Key = "customer_success_segment_note", Label = "Success Segment Note", Description = "Tenant-defined note used by customer success during onboarding reviews.", FieldType = "text", Required = false, VisibleInPortal = false, EditableInPortal = false, InternalOnly = true, SortOrder = 10, IsActive = true }
            ],
            []);
    }

    private static CustomArrCustomerLifecycleStage Stage(Guid tenantId, string key, string label, string description, int order, bool initial, bool active, bool terminal, bool blocksOrders, bool blocksPortal, string[] next, string color, bool required) =>
        new() { Id = $"stage-{Guid.NewGuid():N}"[..38], TenantId = tenantId, Key = key, Label = label, Description = description, SortOrder = order, IsInitial = initial, IsActiveCustomerStage = active, IsTerminal = terminal, BlocksOrders = blocksOrders, BlocksPortalAccess = blocksPortal, AllowedNextStageKeys = next, ColorToken = color, IsSystemRequired = required };

    private static CustomArrCustomerLifecycleTransitionRule Transition(Guid tenantId, string from, string to, bool approval, string? permission, string? template, bool reason, bool openIssues, bool expiredDocs, bool missingFields) =>
        new() { Id = $"transition-{Guid.NewGuid():N}"[..43], TenantId = tenantId, FromStageKey = from, ToStageKey = to, RequiresApproval = approval, RequiredPermission = permission, RequiredChecklistTemplateKey = template, RequiredReason = reason, BlockIfOpenIssues = openIssues, BlockIfExpiredRequiredDocuments = expiredDocs, BlockIfMissingRequiredFields = missingFields };

    private static CustomArrCustomerRequiredFieldRule Required(Guid tenantId, string stage, string field, string message) =>
        new() { Id = $"fieldrule-{Guid.NewGuid():N}"[..42], TenantId = tenantId, LifecycleStageKey = stage, FieldKey = field, RequirementLevel = "required", ValidationMessage = message, AppliesToInternalCreate = true, AppliesToInternalEdit = true };

    private static CustomArrCustomerContactRole ContactRole(Guid tenantId, string key, string label, int order, bool activeRequired, bool uniquePrimary, bool portal, bool orders, bool billing, bool compliance) =>
        new() { Id = $"role-{Guid.NewGuid():N}"[..37], TenantId = tenantId, Key = key, Label = label, Description = $"{label} customer contact role.", IsRequiredForActiveCustomer = activeRequired, RequiresUniquePrimary = uniquePrimary, AllowsPortalAccess = portal, CanReceiveOrderNotifications = orders, CanReceiveBillingNotifications = billing, CanReceiveComplianceNotifications = compliance, SortOrder = order, IsActive = true };

    private static CustomArrCustomerAddressType AddressType(Guid tenantId, string key, string label, int order, bool activeRequired, bool validation, bool geocode, bool billing, bool pickup, bool delivery, bool service) =>
        new() { Id = $"addrtype-{Guid.NewGuid():N}"[..41], TenantId = tenantId, Key = key, Label = label, Description = $"{label} customer address type.", IsRequiredForActiveCustomer = activeRequired, RequiresValidation = validation, RequiresGeocode = geocode, UsableForBilling = billing, UsableForPickup = pickup, UsableForDelivery = delivery, UsableForService = service, SortOrder = order, IsActive = true };

    private static CustomArrCustomerOnboardingChecklistItemTemplate ChecklistItem(Guid tenantId, string templateKey, string key, string label, string itemType, int order, bool required, bool activation, bool orders, bool portal) =>
        new() { Id = $"onbitem-{Guid.NewGuid():N}"[..40], TenantId = tenantId, TemplateKey = templateKey, Key = key, Label = label, Description = $"{label} before activation.", ItemType = itemType, Required = required, SortOrder = order, BlocksActivation = activation, BlocksOrders = orders, BlocksPortalAccess = portal };

    private static CustomArrCustomerDocumentRequirement DocumentRequirement(Guid tenantId, string key, string label, string recordArrType, bool required, bool expires, int? warningDays, bool customerUpload, bool visiblePortal, bool activation, bool orders) =>
        new() { Id = $"docreq-{Guid.NewGuid():N}"[..39], TenantId = tenantId, Key = key, Label = label, Description = $"{label} document requirement.", Required = required, Expires = expires, ExpirationWarningDays = warningDays, RecordArrDocumentTypeKey = recordArrType, CustomerCanUpload = customerUpload, VisibleInPortal = visiblePortal, BlocksActivation = activation, BlocksOrders = orders, BlocksPortalAccess = false };

    private static CustomArrCustomerDuplicateDetectionRule DuplicateRule(Guid tenantId, string key, string label, int priority, string field, string type, int weight, int block, int review) =>
        new() { Id = $"duperule-{Guid.NewGuid():N}"[..41], TenantId = tenantId, Key = key, Label = label, Priority = priority, MatchField = field, MatchType = type, Weight = weight, AutoBlockThreshold = block, ReviewThreshold = review, IsActive = true };

    private static CustomArrCustomerNotificationRule NotificationRule(Guid tenantId, string key, string label, string eventType, string recipientType, int delay, int? escalation) =>
        new() { Id = $"notify-{Guid.NewGuid():N}"[..39], TenantId = tenantId, Key = key, Label = label, EventType = eventType, RecipientType = recipientType, DelayMinutes = delay, EscalationAfterMinutes = escalation, IsActive = true };

    private sealed record DefaultSettingsBundle(
        CustomArrTenantSettings Settings,
        CustomArrCustomerNumberingSettings Numbering,
        IReadOnlyList<CustomArrCustomerLifecycleStage> LifecycleStages,
        IReadOnlyList<CustomArrCustomerLifecycleTransitionRule> TransitionRules,
        IReadOnlyList<CustomArrCustomerClassificationCatalog> Catalogs,
        IReadOnlyList<CustomArrCustomerRequiredFieldRule> RequiredFields,
        IReadOnlyList<CustomArrCustomerContactRole> ContactRoles,
        IReadOnlyList<CustomArrCustomerAddressType> AddressTypes,
        IReadOnlyList<CustomArrCustomerOwnerRule> OwnerRules,
        IReadOnlyList<CustomArrCustomerOnboardingTemplate> OnboardingTemplates,
        IReadOnlyList<CustomArrCustomerOnboardingChecklistItemTemplate> OnboardingItems,
        CustomArrCustomerPortalTenantSettings Portal,
        IReadOnlyList<CustomArrCustomerDocumentRequirement> DocumentRequirements,
        IReadOnlyList<CustomArrCustomerDuplicateDetectionRule> DuplicateRules,
        CustomArrCustomerIntegrationSettings Integration,
        IReadOnlyList<CustomArrCustomerExternalIdSource> ExternalIdSources,
        IReadOnlyList<CustomArrCustomerNotificationRule> NotificationRules,
        IReadOnlyList<CustomArrCustomerCustomFieldDefinition> CustomFields,
        IReadOnlyList<CustomArrCustomerCustomFieldOption> CustomFieldOptions);
}
