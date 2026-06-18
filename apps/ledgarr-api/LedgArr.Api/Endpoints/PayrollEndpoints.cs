using LedgArr.Api.Services;

namespace LedgArr.Api.Endpoints;

public static class PayrollEndpoints
{
    public static void MapLedgArrPayrollEndpoints(this WebApplication app)
    {
        var payroll = app.MapGroup("/api/v1/payroll").WithTags("Payroll").RequireAuthorization();

        payroll.MapGet("/calendars", async (HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListCalendarsAsync(context.User, cancellationToken)));
        payroll.MapPost("/calendars", async (CreatePayrollCalendarRequest request, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpsertCalendarAsync(context.User, null, request, cancellationToken)));
        payroll.MapPatch("/calendars/{id:guid}", async (Guid id, CreatePayrollCalendarRequest request, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpsertCalendarAsync(context.User, id, request, cancellationToken)));

        payroll.MapGet("/code-mappings", async (HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListCodeMappingsAsync(context.User, cancellationToken)));
        payroll.MapPost("/code-mappings", async (CreatePayrollCodeMappingRequest request, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpsertCodeMappingAsync(context.User, null, request, cancellationToken)));
        payroll.MapPatch("/code-mappings/{id:guid}", async (Guid id, CreatePayrollCodeMappingRequest request, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpsertCodeMappingAsync(context.User, id, request, cancellationToken)));

        payroll.MapGet("/batches", async (HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListBatchesAsync(context.User, cancellationToken)));
        payroll.MapPost("/batches", async (CreatePayrollBatchRequest request, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateBatchAsync(context.User, request, cancellationToken)));
        payroll.MapGet("/batches/{id:guid}", async (Guid id, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
        {
            var batch = await service.GetBatchAsync(context.User, id, cancellationToken);
            return batch is null ? Results.NotFound() : Results.Ok(batch);
        });
        payroll.MapPost("/batches/{id:guid}/collect-time", async (Guid id, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CollectTimeAsync(context.User, id, cancellationToken)));
        payroll.MapPost("/batches/{id:guid}/validate", async (Guid id, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ValidateBatchAsync(context.User, id, cancellationToken)));
        payroll.MapPost("/batches/{id:guid}/approve-for-export", async (Guid id, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ApproveForExportAsync(context.User, id, cancellationToken)));
        payroll.MapPost("/batches/{id:guid}/export", async (Guid id, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ExportBatchAsync(context.User, id, cancellationToken)));
        payroll.MapPost("/batches/{id:guid}/mark-provider-accepted", async (Guid id, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.MarkProviderStatusAsync(context.User, id, "provider_accepted", cancellationToken)));
        payroll.MapPost("/batches/{id:guid}/mark-provider-rejected", async (Guid id, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.MarkProviderStatusAsync(context.User, id, "provider_rejected", cancellationToken)));
        payroll.MapPost("/batches/{id:guid}/post-journal", async (Guid id, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.PostJournalAsync(context.User, id, cancellationToken)));
        payroll.MapPost("/batches/{id:guid}/close", async (Guid id, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CloseBatchAsync(context.User, id, cancellationToken)));
        payroll.MapPost("/batches/{id:guid}/reopen", async (Guid id, ReopenPayrollBatchRequest request, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ReopenBatchAsync(context.User, id, request.CorrectionReason, cancellationToken)));

        payroll.MapGet("/batches/{id:guid}/lines", async (Guid id, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListBatchLinesAsync(context.User, id, cancellationToken)));
        payroll.MapGet("/batches/{id:guid}/export-packets", async (Guid id, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListExportPacketsAsync(context.User, id, cancellationToken)));
        payroll.MapGet("/batches/{id:guid}/journal-snapshot", async (Guid id, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListJournalSnapshotsAsync(context.User, id, cancellationToken)));
        payroll.MapGet("/batches/{id:guid}/export", async (Guid id, HttpContext context, PayrollService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.BuildGenericCsvExportAsync(context.User, id, cancellationToken)));
    }
}
