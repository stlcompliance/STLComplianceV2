using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.SmartImport;

namespace NexArr.Api.Endpoints;

public static class SmartImportEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void MapSmartImportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/imports")
            .WithTags("Smart Import")
            .RequireAuthorization();

        group.MapGet("/batches", async (
            HttpContext context,
            SmartImportService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ListBatchesAsync(context.User, cancellationToken)))
            .WithName("ListSmartImportBatches");

        group.MapPost("/batches", async (
            HttpContext context,
            SmartImportService service,
            CancellationToken cancellationToken) =>
        {
            if (!context.Request.HasFormContentType)
            {
                throw new StlApiException("smart_import.multipart_required", "Smart Import upload requires multipart form data.", 400);
            }

            var form = await context.Request.ReadFormAsync(cancellationToken);
            var file = form.Files.FirstOrDefault()
                ?? throw new StlApiException("smart_import.file_required", "Smart Import upload requires a file.", 400);
            var destinationProduct = form["destinationProduct"].FirstOrDefault()
                ?? form["destinationProductHint"].FirstOrDefault();

            var authorizationHeader = context.Request.Headers.Authorization.FirstOrDefault();
            return Results.Created(
                "/api/v1/imports/batches",
                await service.CreateBatchAsync(context.User, file, destinationProduct, authorizationHeader, cancellationToken));
        }).WithName("CreateSmartImportBatch")
        .Accepts<IFormFile>("multipart/form-data");

        group.MapGet("/batches/{batchId:guid}", async (
            Guid batchId,
            HttpContext context,
            SmartImportService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetBatchAsync(context.User, batchId, cancellationToken)))
            .WithName("GetSmartImportBatch");

        group.MapPost("/batches/{batchId:guid}/review-decisions", async (
            Guid batchId,
            SmartImportReviewDecisionRequest request,
            HttpContext context,
            SmartImportService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.DecideAsync(context.User, request, cancellationToken);
            return Results.Ok(result);
        }).WithName("CreateSmartImportReviewDecision");

        group.MapPost("/batches/{batchId:guid}/review-decisions/bulk", async (
            Guid batchId,
            SmartImportBulkReviewDecisionRequest request,
            HttpContext context,
            SmartImportService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.DecideBulkAsync(context.User, batchId, request, cancellationToken)))
            .WithName("CreateBulkSmartImportReviewDecision");

        group.MapPost("/batches/{batchId:guid}/mapping-overrides", async (
            Guid batchId,
            SmartImportManualMappingOverrideRequest request,
            HttpContext context,
            SmartImportService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ApplyManualMappingOverrideAsync(context.User, batchId, request, cancellationToken)))
            .WithName("ApplySmartImportManualMappingOverride");

        group.MapPost("/batches/{batchId:guid}/commit-plans", async (
            Guid batchId,
            SmartImportCreateCommitPlanRequest request,
            HttpContext context,
            SmartImportService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateCommitPlanAsync(context.User, batchId, request, cancellationToken)))
            .WithName("CreateSmartImportCommitPlan");

        group.MapPost("/commit-plans/{commitPlanId:guid}/approve", async (
            Guid commitPlanId,
            HttpContext context,
            SmartImportService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ApproveCommitPlanAsync(context.User, commitPlanId, cancellationToken)))
            .WithName("ApproveSmartImportCommitPlan");

        group.MapPost("/commit-plans/{commitPlanId:guid}/commit", async (
            Guid commitPlanId,
            HttpContext context,
            SmartImportService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.CommitAsync(context.User, commitPlanId, cancellationToken)))
            .WithName("CommitSmartImportCommitPlan");

        group.MapPost("/commit-plans/{commitPlanId:guid}/steps/{stepId:guid}/retry", async (
            Guid commitPlanId,
            Guid stepId,
            HttpContext context,
            SmartImportService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.RetryStepAsync(context.User, commitPlanId, stepId, cancellationToken)))
            .WithName("RetrySmartImportCommitStep");

        group.MapGet("/templates", async (
            HttpContext context,
            NexArrDbContext db,
            string? destinationProduct,
            string? entityType,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var query = db.ImportMappingTemplates.AsNoTracking().Where(x => x.TenantId == tenantId && x.Active);
            if (!string.IsNullOrWhiteSpace(destinationProduct))
            {
                query = query.Where(x => x.DestinationProduct == destinationProduct.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(entityType))
            {
                query = query.Where(x => x.EntityType == entityType);
            }

            return Results.Ok(await query.OrderBy(x => x.TemplateName).ToListAsync(cancellationToken));
        }).WithName("ListSmartImportMappingTemplates");

        group.MapPost("/templates", async (
            ImportMappingTemplate request,
            HttpContext context,
            NexArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var actorPersonId = context.User.GetPersonId();
            var template = new ImportMappingTemplate
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                DestinationProduct = request.DestinationProduct.Trim().ToLowerInvariant(),
                EntityType = request.EntityType.Trim(),
                TemplateName = request.TemplateName.Trim(),
                MappingJson = string.IsNullOrWhiteSpace(request.MappingJson) ? "{}" : request.MappingJson,
                Active = true,
                CreatedByPersonId = actorPersonId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            ValidateJson(template.MappingJson);
            db.ImportMappingTemplates.Add(template);
            await db.SaveChangesAsync(cancellationToken);
            return Results.Created($"/api/v1/imports/templates/{template.Id:D}", template);
        }).WithName("CreateSmartImportMappingTemplate");
    }

    public static void MapNexArrInternalSmartImportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/internal/smart-import")
            .WithTags("Internal Smart Import");

        group.MapPost("/process-batch", async (
            InternalProcessSmartImportBatchRequest request,
            HttpContext context,
            SmartImportService service,
            StlServiceTokenValidator tokenValidator,
            CancellationToken cancellationToken) =>
        {
            ValidateSharedWorkerToken(context, tokenValidator, request.TenantId ?? Guid.Empty);
            return Results.Ok(await service.ProcessBatchAsync(request.TenantId, request.BatchSize <= 0 ? 5 : request.BatchSize, cancellationToken));
        }).WithName("ProcessSmartImportBatchInternal");
    }

    private static void ValidateSharedWorkerToken(HttpContext context, StlServiceTokenValidator tokenValidator, Guid tenantId)
    {
        var token = ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization);
        var validated = tokenValidator.TryValidate(token)
            ?? throw new StlApiException("auth.service_token_invalid", "Service token is invalid.", 401);
        if (!string.Equals(validated.SourceProductKey, "shared-worker", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("auth.service_token_scope", "Service token source product is not authorized for Smart Import processing.", 403);
        }

        if (!validated.AllowedProductKeys.Contains("nexarr", StringComparer.OrdinalIgnoreCase))
        {
            throw new StlApiException("auth.service_token_scope", "Service token is not authorized for NexArr.", 403);
        }

        if (validated.TenantScope is Guid scopedTenant && tenantId != Guid.Empty && scopedTenant != tenantId)
        {
            throw new StlApiException("auth.service_token_scope", "Service token tenant scope does not match the request tenant.", 403);
        }

        if (!string.IsNullOrWhiteSpace(validated.ActionScope)
            && !validated.ActionScope.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Contains("nexarr.smart_import.process", StringComparer.Ordinal))
        {
            throw new StlApiException("auth.service_token_scope", "Service token action scope is not authorized for Smart Import processing.", 403);
        }
    }

    private static void ValidateJson(string json)
    {
        try
        {
            using var _ = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            throw new StlApiException("smart_import.invalid_mapping_json", "Mapping template JSON is invalid.", 400);
        }
    }

    private sealed record InternalProcessSmartImportBatchRequest(Guid? TenantId, int BatchSize);
}
