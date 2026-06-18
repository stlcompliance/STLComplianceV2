using CustomArr.Api.Data;
using CustomArr.Api.Services;

namespace CustomArr.Api.Endpoints;

public static class TenantSettingsEndpoints
{
    public static void MapCustomArrTenantSettingsEndpoints(this WebApplication app)
    {
        var settings = app.MapGroup("/api/v1/customarr/tenant-settings")
            .WithTags("CustomArr tenant settings")
            .RequireAuthorization();

        settings.MapGet("/", (
            HttpContext context,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.GetSettingsAsync(context.User, cancellationToken))
            .WithName("GetCustomArrTenantSettings");

        settings.MapPut("/", (
            HttpContext context,
            CustomArrTenantSettingsUpdateRequest request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.UpdateSettingsAsync(context.User, request, cancellationToken))
            .WithName("UpdateCustomArrTenantSettings");

        settings.MapGet("/catalogs", (
            HttpContext context,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ListCatalogsAsync(context.User, cancellationToken))
            .WithName("ListCustomArrTenantSettingCatalogs");

        settings.MapPut("/catalogs/{catalogType}", (
            string catalogType,
            HttpContext context,
            IReadOnlyList<CustomerClassificationCatalogItem> request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ReplaceCatalogAsync(context.User, catalogType, request, cancellationToken))
            .WithName("UpdateCustomArrTenantSettingCatalog");

        settings.MapGet("/lifecycle-stages", async (
            HttpContext context,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            (await service.GetSettingsAsync(context.User, cancellationToken)).LifecycleStages)
            .WithName("ListCustomArrTenantLifecycleStages");

        settings.MapPut("/lifecycle-stages", (
            HttpContext context,
            IReadOnlyList<CustomerLifecycleStageItem> request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ReplaceSectionAsync(context.User, current => current.ToUpdateRequest() with { LifecycleStages = request }, cancellationToken))
            .WithName("UpdateCustomArrTenantLifecycleStages");

        settings.MapGet("/required-fields", async (
            HttpContext context,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            (await service.GetSettingsAsync(context.User, cancellationToken)).RequiredFieldRules)
            .WithName("ListCustomArrTenantRequiredFieldRules");

        settings.MapPut("/required-fields", (
            HttpContext context,
            IReadOnlyList<CustomerRequiredFieldRuleItem> request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ReplaceSectionAsync(context.User, current => current.ToUpdateRequest() with { RequiredFieldRules = request }, cancellationToken))
            .WithName("UpdateCustomArrTenantRequiredFieldRules");

        settings.MapGet("/contact-roles", async (
            HttpContext context,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            (await service.GetSettingsAsync(context.User, cancellationToken)).ContactRoles)
            .WithName("ListCustomArrTenantContactRoles");

        settings.MapPut("/contact-roles", (
            HttpContext context,
            IReadOnlyList<CustomerContactRoleItem> request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ReplaceSectionAsync(context.User, current => current.ToUpdateRequest() with { ContactRoles = request }, cancellationToken))
            .WithName("UpdateCustomArrTenantContactRoles");

        settings.MapGet("/address-types", async (
            HttpContext context,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            (await service.GetSettingsAsync(context.User, cancellationToken)).AddressTypes)
            .WithName("ListCustomArrTenantAddressTypes");

        settings.MapPut("/address-types", (
            HttpContext context,
            IReadOnlyList<CustomerAddressTypeItem> request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ReplaceSectionAsync(context.User, current => current.ToUpdateRequest() with { AddressTypes = request }, cancellationToken))
            .WithName("UpdateCustomArrTenantAddressTypes");

        settings.MapGet("/onboarding-templates", async (
            HttpContext context,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            (await service.GetSettingsAsync(context.User, cancellationToken)).OnboardingTemplates)
            .WithName("ListCustomArrTenantOnboardingTemplates");

        settings.MapPost("/onboarding-templates", (
            HttpContext context,
            CustomerOnboardingTemplateItem request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ReplaceSectionAsync(context.User, current => current.ToUpdateRequest() with { OnboardingTemplates = current.OnboardingTemplates.Concat([request]).ToArray() }, cancellationToken))
            .WithName("CreateCustomArrTenantOnboardingTemplate");

        settings.MapPut("/onboarding-templates/{templateKey}", (
            string templateKey,
            HttpContext context,
            CustomerOnboardingTemplateItem request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ReplaceSectionAsync(
                context.User,
                current => current.ToUpdateRequest() with
                {
                    OnboardingTemplates = current.OnboardingTemplates
                        .Where(x => !x.Key.Equals(templateKey, StringComparison.OrdinalIgnoreCase))
                        .Concat([request with { Key = templateKey }])
                        .ToArray()
                },
                cancellationToken))
            .WithName("UpdateCustomArrTenantOnboardingTemplate");

        settings.MapGet("/documents", async (
            HttpContext context,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            (await service.GetSettingsAsync(context.User, cancellationToken)).DocumentRequirements)
            .WithName("ListCustomArrTenantDocumentRequirements");

        settings.MapPut("/document-requirements", (
            HttpContext context,
            IReadOnlyList<CustomerDocumentRequirementItem> request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ReplaceSectionAsync(context.User, current => current.ToUpdateRequest() with { DocumentRequirements = request }, cancellationToken))
            .WithName("UpdateCustomArrTenantDocumentRequirements");

        settings.MapGet("/portal", async (
            HttpContext context,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            (await service.GetSettingsAsync(context.User, cancellationToken)).PortalSettings)
            .WithName("GetCustomArrTenantPortalSettings");

        settings.MapPut("/portal", (
            HttpContext context,
            CustomerPortalTenantSettingsItem request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ReplaceSectionAsync(context.User, current => current.ToUpdateRequest() with { PortalSettings = request }, cancellationToken))
            .WithName("UpdateCustomArrTenantPortalSettings");

        settings.MapGet("/duplicates", async (
            HttpContext context,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            (await service.GetSettingsAsync(context.User, cancellationToken)).DuplicateDetectionRules)
            .WithName("ListCustomArrTenantDuplicateDetectionRules");

        settings.MapPut("/duplicates", (
            HttpContext context,
            IReadOnlyList<CustomerDuplicateDetectionRuleItem> request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ReplaceSectionAsync(context.User, current => current.ToUpdateRequest() with { DuplicateDetectionRules = request }, cancellationToken))
            .WithName("UpdateCustomArrTenantDuplicateDetectionRules");

        settings.MapGet("/integrations", async (
            HttpContext context,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            (await service.GetSettingsAsync(context.User, cancellationToken)).IntegrationSettings)
            .WithName("GetCustomArrTenantIntegrationSettings");

        settings.MapPut("/integrations", (
            HttpContext context,
            CustomerIntegrationSettingsItem request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ReplaceSectionAsync(context.User, current => current.ToUpdateRequest() with { IntegrationSettings = request }, cancellationToken))
            .WithName("UpdateCustomArrTenantIntegrationSettings");

        settings.MapGet("/notifications", async (
            HttpContext context,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            (await service.GetSettingsAsync(context.User, cancellationToken)).NotificationRules)
            .WithName("ListCustomArrTenantNotificationRules");

        settings.MapPut("/notifications", (
            HttpContext context,
            IReadOnlyList<CustomerNotificationRuleItem> request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ReplaceSectionAsync(context.User, current => current.ToUpdateRequest() with { NotificationRules = request }, cancellationToken))
            .WithName("UpdateCustomArrTenantNotificationRules");

        settings.MapGet("/custom-fields", async (
            HttpContext context,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            (await service.GetSettingsAsync(context.User, cancellationToken)).CustomFieldDefinitions)
            .WithName("ListCustomArrTenantCustomFields");

        settings.MapPost("/custom-fields", (
            HttpContext context,
            CustomerCustomFieldDefinitionItem request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ReplaceSectionAsync(context.User, current => current.ToUpdateRequest() with { CustomFieldDefinitions = current.CustomFieldDefinitions.Concat([request]).ToArray() }, cancellationToken))
            .WithName("CreateCustomArrTenantCustomField");

        settings.MapPut("/custom-fields/{fieldKey}", (
            string fieldKey,
            HttpContext context,
            CustomerCustomFieldDefinitionItem request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ReplaceSectionAsync(
                context.User,
                current => current.ToUpdateRequest() with
                {
                    CustomFieldDefinitions = current.CustomFieldDefinitions
                        .Where(x => !x.Key.Equals(fieldKey, StringComparison.OrdinalIgnoreCase))
                        .Concat([request with { Key = fieldKey }])
                        .ToArray()
                },
                cancellationToken))
            .WithName("UpdateCustomArrTenantCustomField");

        var customers = app.MapGroup("/api/v1/customarr/customers")
            .WithTags("CustomArr customer settings helpers")
            .RequireAuthorization();

        customers.MapPost("/", (
            HttpContext context,
            CustomArrCreateCustomerRequest request,
        CustomArrStore store) =>
        {
            var idempotencyKey = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
            var customer = store.CreateCustomer(context.User, request, idempotencyKey);
            return Results.Created($"/api/v1/customarr/customers/{customer.CustomerId}", customer);
        })
            .WithName("CreateCustomArrCustomer");

        customers.MapGet("/create-metadata", (
            HttpContext context,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.GetCreateMetadataAsync(context.User, cancellationToken))
            .WithName("GetCustomArrCustomerCreateMetadata");

        customers.MapGet("/{customerId}/edit-metadata", (
            HttpContext context,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.GetCreateMetadataAsync(context.User, cancellationToken))
            .WithName("GetCustomArrCustomerEditMetadata");

        customers.MapPost("/validate", (
            HttpContext context,
            CustomArrCustomerValidationRequest request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.ValidateCustomerAsync(context.User, request, cancellationToken))
            .WithName("ValidateCustomArrCustomer");

        customers.MapPost("/check-duplicates", (
            HttpContext context,
            CustomArrDuplicateCheckRequest request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.CheckDuplicatesAsync(context.User, request, cancellationToken))
            .WithName("CheckCustomArrCustomerDuplicates");

        customers.MapPost("/{customerId}/stage-transition-preview", (
            string customerId,
            HttpContext context,
            CustomArrStageTransitionPreviewRequest request,
            CustomArrTenantSettingsService service,
            CancellationToken cancellationToken) =>
            service.PreviewStageTransitionAsync(context.User, customerId, request, cancellationToken))
            .WithName("PreviewCustomArrCustomerStageTransition");
    }
}
