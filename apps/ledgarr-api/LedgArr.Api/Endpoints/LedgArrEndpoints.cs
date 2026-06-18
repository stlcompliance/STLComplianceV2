using LedgArr.Api.Services;

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

        ledgarr.MapGet("/financial-legal-entities", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListFinancialLegalEntitiesAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrFinancialLegalEntities");
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
        ledgarr.MapPost("/dimensions/types", async (HttpContext context, CreateDimensionTypeRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/dimensions/types", await store.CreateDimensionTypeAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrDimensionType");
        ledgarr.MapPost("/dimensions/values", async (HttpContext context, CreateDimensionValueRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/dimensions/values", await store.CreateDimensionValueAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrDimensionValue");
        ledgarr.MapGet("/dimension-mappings", () => Results.Ok(Array.Empty<object>()))
            .WithName("ListLedgArrDimensionMappings");
        ledgarr.MapPost("/dimension-mappings", () => Results.Ok(new { status = "reserved" }))
            .WithName("CreateLedgArrDimensionMapping");
        ledgarr.MapPost("/dimensions/resolve", async (HttpContext context, DimensionResolveRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ResolveDimensionsAsync(context.User, request, cancellationToken)))
            .WithName("ResolveLedgArrDimensions");

        ledgarr.MapGet("/posting-rules", () => Results.Ok(Array.Empty<object>()))
            .WithName("ListLedgArrPostingRules");
        ledgarr.MapPost("/posting-rules", () => Results.Ok(new { status = "system_default_rules_active" }))
            .WithName("CreateLedgArrPostingRule");
        ledgarr.MapPut("/posting-rules/{id:guid}", (Guid id) => Results.Ok(new { id, status = "reserved" }))
            .WithName("UpdateLedgArrPostingRule");
        ledgarr.MapPost("/posting-rules/{id:guid}/activate", (Guid id) => Results.Ok(new { id, status = "active" }))
            .WithName("ActivateLedgArrPostingRule");
        ledgarr.MapPost("/posting-rules/{id:guid}/deactivate", (Guid id) => Results.Ok(new { id, status = "inactive" }))
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
        ledgarr.MapPost("/financial-packets/{id:guid}/reject", (Guid id) => Results.Ok(new { id, status = "rejected" }))
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
        integrations.MapPost("/resolve-account-mapping", () => Results.Ok(new { status = "mapped", freshness = "live" }))
            .WithName("ResolveLedgArrIntegrationAccountMapping");
        integrations.MapPost("/resolve-financial-legal-entity", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListFinancialLegalEntitiesAsync(context.User, cancellationToken)))
            .WithName("ResolveLedgArrIntegrationFinancialLegalEntity");

        ledgarr.MapGet("/journals", async (HttpContext context, string? status, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListJournalsAsync(context.User, status, cancellationToken)))
            .WithName("ListLedgArrJournals");
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
        ledgarr.MapPost("/ap/vendor-bills/{id:guid}/dispute", (Guid id) => Results.Ok(new { id, status = "disputed" }))
            .WithName("DisputeLedgArrVendorBill");
        ledgarr.MapGet("/ap/aging", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.GetAPAgingAsync(context.User, cancellationToken)))
            .WithName("GetLedgArrAPAging");
        ledgarr.MapGet("/ap/payment-runs", () => Results.Ok(Array.Empty<object>()))
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
        ledgarr.MapPost("/ar/credit-memos", () => Results.Ok(new { status = "reserved" }))
            .WithName("CreateLedgArrCreditMemo");
        ledgarr.MapPost("/ar/customer-payments", async (HttpContext context, CreateCustomerPaymentRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/ar/customer-payments", await store.CreateCustomerPaymentAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrCustomerPayment");
        ledgarr.MapGet("/ar/aging", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.GetARAgingAsync(context.User, cancellationToken)))
            .WithName("GetLedgArrARAging");
        ledgarr.MapGet("/ar/statements", () => Results.Ok(Array.Empty<object>()))
            .WithName("ListLedgArrStatements");

        ledgarr.MapGet("/inventory-valuation/items", () => Results.Ok(Array.Empty<object>()))
            .WithName("ListLedgArrInventoryValuationItems");
        ledgarr.MapGet("/inventory-valuation/layers", async (HttpContext context, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.ListInventoryCostLayersAsync(context.User, cancellationToken)))
            .WithName("ListLedgArrInventoryCostLayers");
        ledgarr.MapGet("/inventory-valuation/movements", () => Results.Ok(Array.Empty<object>()))
            .WithName("ListLedgArrInventoryValuationMovements");
        ledgarr.MapPost("/inventory-valuation/revalue", async (HttpContext context, InventoryRevalueRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/inventory-valuation/movements", await store.RevalueInventoryAsync(context.User, request, cancellationToken)))
            .WithName("RevalueLedgArrInventory");

        ledgarr.MapPost("/fixed-assets/capitalize", async (HttpContext context, CapitalizeAssetRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/fixed-assets", await store.CapitalizeFixedAssetAsync(context.User, request, cancellationToken)))
            .WithName("CapitalizeLedgArrFixedAsset");
        ledgarr.MapPost("/budgets", async (HttpContext context, CreateBudgetRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/budgets", await store.CreateBudgetAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrBudget");
        ledgarr.MapPost("/budget-check", async (HttpContext context, BudgetCheckRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Ok(await store.CheckBudgetAsync(context.User, request, cancellationToken)))
            .WithName("CheckLedgArrBudget");

        ledgarr.MapPost("/external/systems", async (HttpContext context, CreateExternalFinanceSystemRequest request, LedgArrStore store, CancellationToken cancellationToken) =>
            Results.Created("/api/v1/ledgarr/external/systems", await store.CreateExternalFinanceSystemAsync(context.User, request, cancellationToken)))
            .WithName("CreateLedgArrExternalFinanceSystem");
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
