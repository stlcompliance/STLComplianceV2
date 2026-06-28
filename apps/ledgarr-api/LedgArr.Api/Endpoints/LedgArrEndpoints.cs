using LedgArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace LedgArr.Api.Endpoints;

public static class LedgArrEndpoints
{
    public static void MapLedgArrEndpoints(this WebApplication app)
    {
        var ledgarr = app.MapGroup("/api/v1/ledgarr").WithTags("LedgArr").RequireAuthorization();
        var integrations = app.MapGroup("/api/v1/integrations/ledgarr").WithTags("LedgArr Integrations").RequireAuthorization();
        var workspace = app.MapGroup("/api/v1/workspace").WithTags("Workspace").RequireAuthorization();

        ledgarr.MapGet("/dashboard", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.GetDashboardAsync(context.User, cancellationToken)))
            .WithName("GetLedgArrDashboard");
        workspace.MapGet("/summary", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.GetDashboardAsync(context.User, cancellationToken)))
            .WithName("GetLedgArrWorkspaceSummary");

        ledgarr.MapGet("/settings", async (HttpContext context, LedgArrTenantSettingsService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetTenantSettingsAsync(context.User.GetTenantId(), context.User, cancellationToken)))
            .WithName("GetLedgArrTenantSettings");
        ledgarr.MapGet("/settings/options", async (HttpContext context, LedgArrTenantSettingsService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetOptionsAsync(context.User.GetTenantId(), context.User, cancellationToken)))
            .WithName("GetLedgArrTenantSettingsOptions");
        ledgarr.MapGet("/settings/posting-source-options", async (HttpContext context, LedgArrTenantSettingsService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetOptionsAsync(context.User.GetTenantId(), context.User, cancellationToken)))
            .WithName("GetLedgArrTenantPostingSourceOptions");
        ledgarr.MapGet("/settings/{sectionKey}", async (HttpContext context, string sectionKey, LedgArrTenantSettingsService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetTenantSettingsSectionAsync(context.User.GetTenantId(), sectionKey, context.User, cancellationToken)))
            .WithName("GetLedgArrTenantSettingsSection");
        ledgarr.MapPut("/settings/{sectionKey}", async (HttpContext context, string sectionKey, LedgArrTenantSettingsUpdateRequest request, LedgArrTenantSettingsService service, CancellationToken cancellationToken) =>
        {
            var section = await service.UpdateTenantSettingsSectionAsync(context.User.GetTenantId(), sectionKey, request, context.User, cancellationToken);
            return Results.Ok(section);
        }).WithName("UpdateLedgArrTenantSettingsSection");
        ledgarr.MapPost("/settings/{sectionKey}/validate", async (HttpContext context, string sectionKey, LedgArrTenantSettingsUpdateRequest request, LedgArrTenantSettingsService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ValidateTenantSettingsSectionAsync(context.User.GetTenantId(), sectionKey, request, context.User, cancellationToken)))
            .WithName("ValidateLedgArrTenantSettingsSection");
        ledgarr.MapPost("/settings/{sectionKey}/reset", async (HttpContext context, string sectionKey, ResetTenantSettingsSectionRequest request, LedgArrTenantSettingsService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ResetTenantSettingsSectionToDefaultAsync(context.User.GetTenantId(), sectionKey, context.User, request.Reason, cancellationToken)))
            .WithName("ResetLedgArrTenantSettingsSection");
        ledgarr.MapGet("/settings/{sectionKey}/audit", async (HttpContext context, string sectionKey, int? skip, int? take, LedgArrTenantSettingsService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetTenantSettingsAuditAsync(context.User.GetTenantId(), sectionKey, context.User, skip ?? 0, take ?? 20, cancellationToken)))
            .WithName("GetLedgArrTenantSettingsAudit");

        ledgarr.MapGet("/financial-legal-entities", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListFinancialLegalEntitiesAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrFinancialLegalEntities");
        ledgarr.MapGet("/financial-legal-entity-registrations", async (HttpContext context, Guid? financialLegalEntityId, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListFinancialLegalEntityRegistrationsAsync(context.User, financialLegalEntityId, cancellationToken)))
            .WithName("ListLedgArrFinancialLegalEntityRegistrations");
        ledgarr.MapGet("/financial-legal-entity-addresses", async (HttpContext context, Guid? financialLegalEntityId, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListFinancialLegalEntityAddressSnapshotsAsync(context.User, financialLegalEntityId, cancellationToken)))
            .WithName("ListLedgArrFinancialLegalEntityAddressSnapshots");
        ledgarr.MapPost("/financial-legal-entities", async (HttpContext context, CreateFinancialLegalEntityRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/financial-legal-entities", await store.CreateFinancialLegalEntityAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrFinancialLegalEntity");
        ledgarr.MapGet("/financial-legal-entities/{id:guid}", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var entity = await store.GetFinancialLegalEntityAsync(context.User, id, cancellationToken);
            return entity is null ? Results.NotFound() : Results.Ok(entity);
        }).WithName("GetLedgArrFinancialLegalEntity");
        ledgarr.MapPut("/financial-legal-entities/{id:guid}", async (HttpContext context, Guid id, CreateFinancialLegalEntityRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var entity = await store.UpdateFinancialLegalEntityAsync(context.User, id, request, cancellationToken);
            return entity is null ? Results.NotFound() : Results.Ok(entity);
        }).WithName("UpdateLedgArrFinancialLegalEntity");
        ledgarr.MapPost("/financial-legal-entities/{id:guid}/deactivate", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var entity = await store.DeactivateFinancialLegalEntityAsync(context.User, id, cancellationToken);
            return entity is null ? Results.NotFound() : Results.Ok(entity);
        }).WithName("DeactivateLedgArrFinancialLegalEntity");

        ledgarr.MapGet("/fiscal-calendars", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListFiscalCalendarsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrFiscalCalendars");
        ledgarr.MapPost("/fiscal-calendars", async (HttpContext context, CreateFiscalCalendarRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/fiscal-calendars", await store.CreateFiscalCalendarAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrFiscalCalendar");
        ledgarr.MapGet("/fiscal-periods", async (HttpContext context, string? status, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListFiscalPeriodsAsync(context.User, status, cancellationToken)))
            .WithName("ListLedgArrFiscalPeriods");
        ledgarr.MapPost("/fiscal-periods", async (HttpContext context, CreateFiscalPeriodRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/fiscal-periods", await store.CreateFiscalPeriodAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrFiscalPeriod");
        ledgarr.MapPost("/fiscal-periods/{id:guid}/close", async (HttpContext context, Guid id, PeriodActionRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var period = await store.CloseFiscalPeriodAsync(context.User, id, request.Reason, cancellationToken);
            return period is null ? Results.NotFound() : Results.Ok(period);
        }).WithName("CloseLedgArrFiscalPeriod");
        ledgarr.MapPost("/fiscal-periods/{id:guid}/reopen", async (HttpContext context, Guid id, PeriodActionRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var period = await store.ReopenFiscalPeriodAsync(context.User, id, request.Reason, cancellationToken);
            return period is null ? Results.NotFound() : Results.Ok(period);
        }).WithName("ReopenLedgArrFiscalPeriod");
        ledgarr.MapPost("/fiscal-periods/{id:guid}/lock", async (HttpContext context, Guid id, PeriodActionRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var period = await store.LockFiscalPeriodAsync(context.User, id, request.Reason, cancellationToken);
            return period is null ? Results.NotFound() : Results.Ok(period);
        }).WithName("LockLedgArrFiscalPeriod");

        ledgarr.MapGet("/chart-of-accounts", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListChartsOfAccountsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrChartsOfAccounts");
        ledgarr.MapPost("/chart-of-accounts", async (HttpContext context, CreateChartOfAccountsRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/chart-of-accounts", await store.CreateChartOfAccountsAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrChartOfAccounts");
        ledgarr.MapGet("/gl-accounts", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListGLAccountsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrGLAccounts");
        ledgarr.MapPost("/gl-accounts", async (HttpContext context, CreateGLAccountRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/gl-accounts", await store.CreateGLAccountAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrGLAccount");
        ledgarr.MapPut("/gl-accounts/{id:guid}", async (HttpContext context, Guid id, CreateGLAccountRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var account = await store.UpdateGLAccountAsync(context.User, id, request, cancellationToken);
            return account is null ? Results.NotFound() : Results.Ok(account);
        }).WithName("UpdateLedgArrGLAccount");
        ledgarr.MapPost("/gl-accounts/{id:guid}/deactivate", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var account = await store.DeactivateGLAccountAsync(context.User, id, cancellationToken);
            return account is null ? Results.NotFound() : Results.Ok(account);
        }).WithName("DeactivateLedgArrGLAccount");

        ledgarr.MapGet("/dimensions", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListDimensionTypesAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrDimensions");
        ledgarr.MapGet("/control/approval-policies", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListApprovalPoliciesAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrApprovalPolicies");
        ledgarr.MapGet("/control/sod-rules", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListSegregationOfDutiesRulesAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrSegregationOfDutiesRules");
        ledgarr.MapGet("/audit/events", async (HttpContext context, string? targetType, int? take, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListFinancialAuditEventsAsync(context.User, targetType, take ?? 25, cancellationToken)))
            .WithName("ListLedgArrAuditEvents");
        ledgarr.MapPost("/dimensions/types", async (HttpContext context, CreateDimensionTypeRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/dimensions/types", await store.CreateDimensionTypeAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrDimensionType");
        ledgarr.MapPost("/dimensions/values", async (HttpContext context, CreateDimensionValueRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/dimensions/values", await store.CreateDimensionValueAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrDimensionValue");
        ledgarr.MapGet("/dimension-mappings", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListDimensionMappingsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrDimensionMappings");
        ledgarr.MapPost("/dimension-mappings", async (HttpContext context, CreateDimensionMappingRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/dimension-mappings", await store.CreateDimensionMappingAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrDimensionMapping");
        ledgarr.MapPost("/dimensions/resolve", async (HttpContext context, DimensionResolveRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ResolveDimensionsAsync(context.User, request, cancellationToken)))
            .WithName("ResolveLedgArrDimensions");

        ledgarr.MapGet("/posting-rules", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListPostingRulesAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrPostingRules");
        ledgarr.MapPost("/posting-rules", async (HttpContext context, CreatePostingRuleRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/posting-rules", await store.CreatePostingRuleAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrPostingRule");
        ledgarr.MapPut("/posting-rules/{id:guid}", async (HttpContext context, Guid id, CreatePostingRuleRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            await store.UpdatePostingRuleAsync(context.User, id, request, cancellationToken) is { } rule ? Results.Ok(rule) : Results.NotFound())
            .WithName("UpdateLedgArrPostingRule");
        ledgarr.MapPost("/posting-rules/{id:guid}/activate", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
            await store.ActivatePostingRuleAsync(context.User, id, cancellationToken) is { } rule ? Results.Ok(rule) : Results.NotFound())
            .WithName("ActivateLedgArrPostingRule");
        ledgarr.MapPost("/posting-rules/{id:guid}/deactivate", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
            await store.DeactivatePostingRuleAsync(context.User, id, cancellationToken) is { } rule ? Results.Ok(rule) : Results.NotFound())
            .WithName("DeactivateLedgArrPostingRule");
        ledgarr.MapPost("/posting-preview", async (HttpContext context, PostingPreviewRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.CreateAdHocPostingPreviewAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrPostingPreview");
        ledgarr.MapPost("/posting-preview/{id:guid}/approve", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var preview = await store.ApprovePostingPreviewAsync(context.User, id, cancellationToken);
            return preview is null ? Results.NotFound() : Results.Ok(preview);
        }).WithName("ApproveLedgArrPostingPreview");
        ledgarr.MapPost("/posting-preview/{id:guid}/post", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var journal = await store.PostPostingPreviewAsync(context.User, id, cancellationToken);
            return journal is null ? Results.NotFound() : Results.Ok(journal);
        }).WithName("PostLedgArrPostingPreview");

        ledgarr.MapGet("/financial-packets", async (HttpContext context, string? status, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListFinancialPacketsAsync(context.User, status, cancellationToken)))
            .WithName("ListLedgArrFinancialPackets");
        ledgarr.MapGet("/financial-packets/{id:guid}", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var packet = await store.GetFinancialPacketAsync(context.User, id, cancellationToken);
            return packet is null ? Results.NotFound() : Results.Ok(packet);
        }).WithName("GetLedgArrFinancialPacket");
        ledgarr.MapPost("/financial-packets/{id:guid}/map", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var packet = await store.MapFinancialPacketAsync(context.User, id, cancellationToken);
            return packet is null ? Results.NotFound() : Results.Ok(packet);
        }).WithName("MapLedgArrFinancialPacket");
        ledgarr.MapPost("/financial-packets/{id:guid}/preview", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var preview = await store.CreatePostingPreviewForPacketAsync(context.User, id, cancellationToken);
            return preview is null ? Results.NotFound() : Results.Ok(preview);
        }).WithName("PreviewLedgArrFinancialPacket");
        ledgarr.MapPost("/financial-packets/{id:guid}/approve", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var preview = await store.CreatePostingPreviewForPacketAsync(context.User, id, cancellationToken);
            if (preview is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(await store.ApprovePostingPreviewAsync(context.User, preview.Preview.Id, cancellationToken));
        }).WithName("ApproveLedgArrFinancialPacket");
        ledgarr.MapPost("/financial-packets/{id:guid}/post", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var preview = await store.CreatePostingPreviewForPacketAsync(context.User, id, cancellationToken);
            if (preview is null)
            {
                return Results.NotFound();
            }
            await store.ApprovePostingPreviewAsync(context.User, preview.Preview.Id, cancellationToken);
            return Results.Ok(await store.PostPostingPreviewAsync(context.User, preview.Preview.Id, cancellationToken));
        }).WithName("PostLedgArrFinancialPacket");
        ledgarr.MapPost("/financial-packets/{id:guid}/reject", async (HttpContext context, Guid id, RejectFinancialPacketRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            await store.RejectFinancialPacketAsync(context.User, id, request, cancellationToken) is { } packet ? Results.Ok(packet) : Results.NotFound())
            .WithName("RejectLedgArrFinancialPacket");

        integrations.MapPost("/financial-packets", async (HttpContext context, FinancialPacketIngestRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/integrations/ledgarr/financial-packets", await store.IngestFinancialPacketAsync(context.User, request, cancellationToken)))
            .WithName("IngestLedgArrFinancialPacket");
        integrations.MapGet("/financial-packets/{id:guid}", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var packet = await store.GetFinancialPacketAsync(context.User, id, cancellationToken);
            return packet is null ? Results.NotFound() : Results.Ok(packet);
        }).WithName("GetLedgArrIntegrationFinancialPacket");
        integrations.MapPost("/posting-preview", async (HttpContext context, PostingPreviewRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.CreateAdHocPostingPreviewAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrIntegrationPostingPreview");
        integrations.MapPost("/resolve-dimensions", async (HttpContext context, DimensionResolveRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ResolveDimensionsAsync(context.User, request, cancellationToken)))
            .WithName("ResolveLedgArrIntegrationDimensions");
        integrations.MapPost("/resolve-account-mapping", async (HttpContext context, ExternalAccountMappingResolveRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ResolveExternalAccountMappingAsync(context.User, request, cancellationToken)))
            .WithName("ResolveLedgArrIntegrationAccountMapping");
        integrations.MapPost("/resolve-financial-legal-entity", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListFinancialLegalEntitiesAsync(context.User, cancellationToken)))
            .WithName("ResolveLedgArrIntegrationFinancialLegalEntity");

        ledgarr.MapGet("/journals", async (HttpContext context, string? status, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListJournalsAsync(context.User, status, cancellationToken)))
            .WithName("ListLedgArrJournals");
        ledgarr.MapGet("/journals/attachments", async (HttpContext context, Guid? journalEntryId, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListJournalAttachmentRefsAsync(context.User, journalEntryId, cancellationToken)))
            .WithName("ListLedgArrJournalAttachments");
        ledgarr.MapGet("/journals/audit-trail", async (HttpContext context, Guid? journalEntryId, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListJournalAuditTrailsAsync(context.User, journalEntryId, cancellationToken)))
            .WithName("ListLedgArrJournalAuditTrail");
        ledgarr.MapGet("/journals/{id:guid}", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var journal = await store.GetJournalAsync(context.User, id, cancellationToken);
            return journal is null ? Results.NotFound() : Results.Ok(journal);
        }).WithName("GetLedgArrJournal");
        ledgarr.MapPost("/journals", async (HttpContext context, CreateJournalRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/journals", await store.CreateJournalAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrJournal");
        ledgarr.MapPost("/journals/{id:guid}/submit", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var journal = await store.SubmitJournalAsync(context.User, id, cancellationToken);
            return journal is null ? Results.NotFound() : Results.Ok(journal);
        }).WithName("SubmitLedgArrJournal");
        ledgarr.MapPost("/journals/{id:guid}/approve", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var journal = await store.ApproveJournalAsync(context.User, id, cancellationToken);
            return journal is null ? Results.NotFound() : Results.Ok(journal);
        }).WithName("ApproveLedgArrJournal");
        ledgarr.MapPost("/journals/{id:guid}/attachments", async (HttpContext context, Guid id, CreateJournalAttachmentRefRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var attachment = await store.CreateJournalAttachmentRefAsync(context.User, id, request, cancellationToken);
            return attachment is null ? Results.NotFound() : Results.Ok(attachment);
        }).WithName("CreateLedgArrJournalAttachment");
        ledgarr.MapPost("/journals/{id:guid}/post", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var journal = await store.PostJournalAsync(context.User, id, cancellationToken);
            return journal is null ? Results.NotFound() : Results.Ok(journal);
        }).WithName("PostLedgArrJournal");
        ledgarr.MapPost("/journals/{id:guid}/reverse", async (HttpContext context, Guid id, ReverseJournalRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var journal = await store.ReverseJournalAsync(context.User, id, request, cancellationToken);
            return journal is null ? Results.NotFound() : Results.Ok(journal);
        }).WithName("ReverseLedgArrJournal");

        ledgarr.MapGet("/ap/vendor-bills", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListVendorBillsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrVendorBills");
        ledgarr.MapPost("/ap/vendor-bills", async (HttpContext context, CreateVendorBillRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/ap/vendor-bills", await store.CreateVendorBillAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrVendorBill");
        ledgarr.MapGet("/ap/vendor-bills/{id:guid}", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok((await store.ListVendorBillsAsync(context.User, cancellationToken)).FirstOrDefault(b => b.Id == id)))
            .WithName("GetLedgArrVendorBill");
        ledgarr.MapPost("/ap/vendor-bills/{id:guid}/match", async (HttpContext context, Guid id, MatchVendorBillRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var bill = await store.MatchVendorBillAsync(context.User, id, request, cancellationToken);
            return bill is null ? Results.NotFound() : Results.Ok(bill);
        }).WithName("MatchLedgArrVendorBill");
        ledgarr.MapPost("/ap/vendor-bills/{id:guid}/approve", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var bill = await store.ApproveVendorBillAsync(context.User, id, cancellationToken);
            return bill is null ? Results.NotFound() : Results.Ok(bill);
        }).WithName("ApproveLedgArrVendorBill");
        ledgarr.MapPost("/ap/vendor-bills/{id:guid}/post", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var bill = await store.PostVendorBillAsync(context.User, id, cancellationToken);
            return bill is null ? Results.NotFound() : Results.Ok(bill);
        }).WithName("PostLedgArrVendorBill");
        ledgarr.MapPost("/ap/vendor-bills/{id:guid}/dispute", async (HttpContext context, Guid id, DisputeVendorBillRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            await store.DisputeVendorBillAsync(context.User, id, request, cancellationToken) is { } dispute ? Results.Ok(dispute) : Results.NotFound())
            .WithName("DisputeLedgArrVendorBill");
        ledgarr.MapGet("/ap/aging", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.GetAPAgingAsync(context.User, cancellationToken)))
            .WithName("GetLedgArrAPAging");
        ledgarr.MapGet("/ap/payment-runs", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListPaymentRunsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrPaymentRuns");
        ledgarr.MapPost("/ap/payment-runs", async (HttpContext context, PaymentRunRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/ap/payment-runs", await store.CreatePaymentRunAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrPaymentRun");
        ledgarr.MapPost("/ap/payment-runs/{id:guid}/approve", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var run = await store.ApprovePaymentRunAsync(context.User, id, cancellationToken);
            return run is null ? Results.NotFound() : Results.Ok(run);
        }).WithName("ApproveLedgArrPaymentRun");
        ledgarr.MapPost("/ap/payment-runs/{id:guid}/export", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var export = await store.ExportPaymentRunAsync(context.User, id, cancellationToken);
            return export is null ? Results.NotFound() : Results.Ok(export);
        }).WithName("ExportLedgArrPaymentRun");

        ledgarr.MapGet("/ar/customer-invoices", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListCustomerInvoicesAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrCustomerInvoices");
        ledgarr.MapPost("/ar/customer-invoices", async (HttpContext context, CreateCustomerInvoiceRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/ar/customer-invoices", await store.CreateCustomerInvoiceAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrCustomerInvoice");
        ledgarr.MapGet("/ar/customer-invoices/{id:guid}", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok((await store.ListCustomerInvoicesAsync(context.User, cancellationToken)).FirstOrDefault(i => i.Id == id)))
            .WithName("GetLedgArrCustomerInvoice");
        ledgarr.MapPost("/ar/customer-invoices/{id:guid}/approve", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var invoice = await store.ApproveCustomerInvoiceAsync(context.User, id, cancellationToken);
            return invoice is null ? Results.NotFound() : Results.Ok(invoice);
        }).WithName("ApproveLedgArrCustomerInvoice");
        ledgarr.MapPost("/ar/customer-invoices/{id:guid}/issue", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var invoice = await store.IssueCustomerInvoiceAsync(context.User, id, cancellationToken);
            return invoice is null ? Results.NotFound() : Results.Ok(invoice);
        }).WithName("IssueLedgArrCustomerInvoice");
        ledgarr.MapPost("/ar/customer-invoices/{id:guid}/post", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var invoice = await store.PostCustomerInvoiceAsync(context.User, id, cancellationToken);
            return invoice is null ? Results.NotFound() : Results.Ok(invoice);
        }).WithName("PostLedgArrCustomerInvoice");
        ledgarr.MapPost("/ar/credit-memos", async (HttpContext context, CreateCreditMemoRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/ar/credit-memos", await store.CreateCreditMemoAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrCreditMemo");
        ledgarr.MapPost("/ar/customer-payments", async (HttpContext context, CreateCustomerPaymentRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/ar/customer-payments", await store.CreateCustomerPaymentAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrCustomerPayment");
        ledgarr.MapGet("/ar/customer-payments", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListCustomerPaymentsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrCustomerPayments");
        ledgarr.MapGet("/ar/aging", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.GetARAgingAsync(context.User, cancellationToken)))
            .WithName("GetLedgArrARAging");
        ledgarr.MapGet("/ar/statements", async (HttpContext context, string? customerRefId, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListCustomerStatementsAsync(context.User, customerRefId, cancellationToken)))
            .WithName("ListLedgArrStatements");

        ledgarr.MapGet("/billing/events", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListBillableEventsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrBillableEvents");
        ledgarr.MapPost("/billing/events/from-packet/{id:guid}", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created($"/api/v1/ledgarr/billing/events/from-packet/{id}", await store.CreateBillableEventFromPacketAsync(context.User, id, cancellationToken)))
            .WithName("CreateLedgArrBillableEventFromPacket");
        ledgarr.MapPost("/billing/events/{id:guid}/approve", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
            await store.ApproveBillableEventAsync(context.User, id, cancellationToken) is { } billableEvent
                ? Results.Ok(billableEvent)
                : Results.NotFound())
            .WithName("ApproveLedgArrBillableEvent");
        ledgarr.MapPost("/billing/events/{id:guid}/hold", async (HttpContext context, Guid id, BillableEventActionRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            await store.HoldBillableEventAsync(context.User, id, request.Reason, cancellationToken) is { } billableEvent
                ? Results.Ok(billableEvent)
                : Results.NotFound())
            .WithName("HoldLedgArrBillableEvent");
        ledgarr.MapPost("/billing/events/{id:guid}/generate-invoice-draft", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
            await store.GenerateInvoiceDraftForBillableEventAsync(context.User, id, cancellationToken) is { } billableEvent
                ? Results.Ok(billableEvent)
                : Results.NotFound())
            .WithName("GenerateLedgArrBillableEventInvoiceDraft");

        ledgarr.MapGet("/banking/accounts", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListBankAccountsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrBankAccounts");
        ledgarr.MapPost("/banking/accounts", async (HttpContext context, CreateBankAccountRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/banking/accounts", await store.CreateBankAccountAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrBankAccount");
        ledgarr.MapGet("/banking/transactions", async (HttpContext context, Guid? bankAccountId, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListBankTransactionsAsync(context.User, bankAccountId, cancellationToken)))
            .WithName("ListLedgArrBankTransactions");
        ledgarr.MapPost("/banking/transactions", async (HttpContext context, CreateBankTransactionRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/banking/transactions", await store.CreateBankTransactionAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrBankTransaction");
        ledgarr.MapPost("/banking/transactions/{id:guid}/match", async (HttpContext context, Guid id, MatchBankTransactionRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var transaction = await store.MatchBankTransactionAsync(context.User, id, request, cancellationToken);
            return transaction is null ? Results.NotFound() : Results.Ok(transaction);
        }).WithName("MatchLedgArrBankTransaction");
        ledgarr.MapGet("/banking/reconciliations", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListBankReconciliationsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrBankReconciliations");
        ledgarr.MapPost("/banking/reconciliations", async (HttpContext context, CreateBankReconciliationRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/banking/reconciliations", await store.CreateBankReconciliationAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrBankReconciliation");
        ledgarr.MapPost("/banking/reconciliations/{id:guid}/approve", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var reconciliation = await store.ApproveBankReconciliationAsync(context.User, id, cancellationToken);
            return reconciliation is null ? Results.NotFound() : Results.Ok(reconciliation);
        }).WithName("ApproveLedgArrBankReconciliation");
        ledgarr.MapPost("/banking/reconciliations/{id:guid}/lock", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var reconciliation = await store.LockBankReconciliationAsync(context.User, id, cancellationToken);
            return reconciliation is null ? Results.NotFound() : Results.Ok(reconciliation);
        }).WithName("LockLedgArrBankReconciliation");

        ledgarr.MapGet("/inventory-valuation/items", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListInventoryValuationItemsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrInventoryValuationItems");
        ledgarr.MapGet("/inventory-valuation/layers", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListInventoryCostLayersAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrInventoryCostLayers");
        ledgarr.MapGet("/inventory-valuation/movements", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListInventoryValuationMovementsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrInventoryValuationMovements");
        ledgarr.MapPost("/inventory-valuation/revalue", async (HttpContext context, InventoryRevalueRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/inventory-valuation/movements", await store.RevalueInventoryAsync(context.User, request, cancellationToken)))
            .WithName("RevalueLedgArrInventory");

        ledgarr.MapGet("/fixed-assets", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListFixedAssetsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrFixedAssets");
        ledgarr.MapGet("/fixed-assets/{id:guid}/depreciation-schedule", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListFixedAssetDepreciationSchedulesAsync(context.User, id, cancellationToken)))
            .WithName("ListLedgArrFixedAssetDepreciationSchedules");
        ledgarr.MapPost("/fixed-assets/capitalize", async (HttpContext context, CapitalizeAssetRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/fixed-assets", await store.CapitalizeFixedAssetAsync(context.User, request, cancellationToken)))
            .WithName("CapitalizeLedgArrFixedAsset");
        ledgarr.MapGet("/budgets", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListBudgetsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrBudgets");
        ledgarr.MapPost("/budgets", async (HttpContext context, CreateBudgetRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/budgets", await store.CreateBudgetAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrBudget");
        ledgarr.MapPost("/budget-check", async (HttpContext context, BudgetCheckRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.CheckBudgetAsync(context.User, request, cancellationToken)))
            .WithName("CheckLedgArrBudget");
        ledgarr.MapGet("/projects", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListFinancialProjectsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrProjects");

        ledgarr.MapGet("/tax/codes", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListTaxCodesAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrTaxCodes");
        ledgarr.MapPost("/tax/codes", async (HttpContext context, CreateTaxCodeRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/tax/codes", await store.CreateTaxCodeAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrTaxCode");
        ledgarr.MapGet("/tax/adjustments", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListTaxAdjustmentsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrTaxAdjustments");
        ledgarr.MapPost("/tax/adjustments", async (HttpContext context, CreateTaxAdjustmentRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/tax/adjustments", await store.CreateTaxAdjustmentAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrTaxAdjustment");
        ledgarr.MapGet("/tax/liability-summary", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListTaxLiabilitySummariesAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrTaxLiabilitySummaries");

        ledgarr.MapGet("/intercompany/relationships", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListFinancialLegalEntityRelationshipsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrFinancialLegalEntityRelationships");
        ledgarr.MapPost("/intercompany/relationships", async (HttpContext context, CreateFinancialLegalEntityRelationshipRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/intercompany/relationships", await store.CreateFinancialLegalEntityRelationshipAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrFinancialLegalEntityRelationship");
        ledgarr.MapGet("/intercompany/transactions", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListIntercompanyTransactionsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrIntercompanyTransactions");
        ledgarr.MapPost("/intercompany/transactions", async (HttpContext context, CreateIntercompanyTransactionRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/intercompany/transactions", await store.CreateIntercompanyTransactionAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrIntercompanyTransaction");
        ledgarr.MapPost("/intercompany/transactions/{id:guid}/settle", async (HttpContext context, Guid id, IntercompanySettlementRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            await store.SettleIntercompanyTransactionAsync(context.User, id, request.Reason, cancellationToken) is { } transaction
                ? Results.Ok(transaction)
                : Results.NotFound())
            .WithName("SettleLedgArrIntercompanyTransaction");
        ledgarr.MapGet("/intercompany/balances", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListIntercompanyBalanceSummariesAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrIntercompanyBalances");

        ledgarr.MapGet("/external/systems", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListExternalFinanceSystemsAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrExternalFinanceSystems");
        ledgarr.MapPost("/external/systems", async (HttpContext context, CreateExternalFinanceSystemRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/external/systems", await store.CreateExternalFinanceSystemAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrExternalFinanceSystem");
        ledgarr.MapGet("/external/posting-batches", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListExternalPostingBatchesAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrExternalPostingBatches");
        ledgarr.MapPost("/external/posting-batches", async (HttpContext context, CreateExternalPostingBatchRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/external/posting-batches", await store.CreateExternalPostingBatchAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrExternalPostingBatch");
        ledgarr.MapPost("/external/posting-batches/{id:guid}/export", async (HttpContext context, Guid id, LedgArrStore store, CancellationToken cancellationToken) =>
        {
            var batch = await store.ExportExternalPostingBatchAsync(context.User, id, cancellationToken);
            return batch is null ? Results.NotFound() : Results.Ok(batch);
        }).WithName("ExportLedgArrExternalPostingBatch");

        ledgarr.MapGet("/reports/trial-balance", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.GetTrialBalanceAsync(context.User, cancellationToken)))
            .WithName("GetLedgArrTrialBalance");
        ledgarr.MapGet("/reports/{reportKey}", async (HttpContext context, string reportKey, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.GetReportSummaryAsync(context.User, reportKey, cancellationToken)))
            .WithName("GetLedgArrReportSummary");
    }
}

public sealed record ResetTenantSettingsSectionRequest(string? Reason);
