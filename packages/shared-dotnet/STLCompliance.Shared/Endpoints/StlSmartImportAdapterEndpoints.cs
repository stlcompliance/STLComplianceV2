using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.SmartImport;

namespace STLCompliance.Shared.Endpoints;

public static class StlSmartImportAdapterEndpoints
{
    private static readonly HashSet<string> CommitBlockedProducts = new(StringComparer.OrdinalIgnoreCase)
    {
        "fieldcompanion",
        "ordarr"
    };

    public static void MapStlSmartImportAdapterEndpoints(this WebApplication app)
    {
        var product = app.Services.GetRequiredService<ProductDescriptor>();
        var group = app.MapGroup("/api/v1/integrations/smart-import")
            .WithTags("Smart Import Integrations");

        group.MapPost("/{entityType}/validate", (
            string entityType,
            SmartImportDestinationValidateRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator) =>
        {
            ValidateServiceToken(context, tokenValidator, product.ProductKey, "platform.smart_import.validate", request.TenantId);

            var hasProductHandler = HasProductHandler(context.RequestServices, product.ProductKey);
            var reviewReasons = BuildRequiredReviewReasons(product.ProductKey, entityType, request.Operation, request.RequiredReviewReasons());
            var response = new SmartImportDestinationValidateResponse(
                Valid: !CommitBlockedProducts.Contains(product.ProductKey),
                RequiredPermissions: [$"{product.ProductKey}.smart_import.validate"],
                MissingPermissions: [],
                FieldResults: [],
                MatchCandidates: [],
                RequiredReviewReasons: reviewReasons,
                DeterministicPayload: request.ProposedPayload,
                Warnings: reviewReasons.Contains(SmartImportReviewReasons.UnsupportedProductApi, StringComparer.OrdinalIgnoreCase)
                    ? ["This destination does not expose a product-specific Smart Import commit adapter yet."]
                    : hasProductHandler
                        ? ["Destination product-specific Smart Import commit handler is registered."]
                        : ["Destination product can persist the approved Smart Import payload as a product-local committed import record."]);

            return Results.Ok(response);
        }).WithName($"Validate{product.ProductKey}SmartImport");

        group.MapPost("/{entityType}/commit", async (
            string entityType,
            SmartImportDestinationCommitRequest request,
            HttpContext context) =>
        {
            var tokenValidator = context.RequestServices.GetRequiredService<StlServiceTokenValidator>();
            ValidateServiceToken(context, tokenValidator, product.ProductKey, "platform.smart_import.commit", request.TenantId);

            if (CommitBlockedProducts.Contains(product.ProductKey))
            {
                return Results.Ok(new SmartImportDestinationCommitResponse(
                    Status: "review_required",
                    ResultEntityId: null,
                    DisplayName: null,
                    Links: [],
                    Retryable: false,
                    ErrorCode: "destination_commit_adapter_not_implemented",
                    ErrorMessage: "This product has not implemented a domain-specific Smart Import commit handler yet. The proposed record remains in Smart Import review and was not written."));
            }

            var handler = ResolveProductHandler(context.RequestServices, product.ProductKey);
            if (handler is not null)
            {
                return Results.Ok(await handler.CommitAsync(entityType, request, context.RequestAborted));
            }

            var db = context.RequestServices.GetRequiredService<PlatformDbContext>();
            return await CommitAsync(product.ProductKey, entityType, request, db, context.RequestAborted);
        }).WithName($"Commit{product.ProductKey}SmartImport");
    }

    private static bool HasProductHandler(IServiceProvider services, string productKey) =>
        ResolveProductHandler(services, productKey) is not null;

    private static ISmartImportDestinationCommitHandler? ResolveProductHandler(IServiceProvider services, string productKey) =>
        services
            .GetServices<ISmartImportDestinationCommitHandler>()
            .FirstOrDefault(handler => string.Equals(handler.ProductKey, productKey, StringComparison.OrdinalIgnoreCase));

    private static async Task<IResult> CommitAsync(
        string productKey,
        string entityType,
        SmartImportDestinationCommitRequest request,
        PlatformDbContext db,
        CancellationToken cancellationToken)
    {
        await EnsureDestinationRecordTableAsync(db, cancellationToken);

        var records = db.Set<SmartImportDestinationRecord>();
        var existing = await records.FirstOrDefaultAsync(
            x => x.TenantId == request.TenantId
                && x.DestinationProduct == productKey
                && x.IdempotencyKey == request.IdempotencyKey,
            cancellationToken);
        if (existing is not null)
        {
            return Results.Ok(ToCommitResponse(existing));
        }

        var now = DateTimeOffset.UtcNow;
        var displayName = ResolveDisplayName(request.DeterministicPayload, entityType, request.CommitStepId);
        var record = new SmartImportDestinationRecord
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            ActorPersonId = request.ActorPersonId,
            ApprovedByPersonId = request.ApprovedByPersonId,
            ImportBatchId = request.ImportBatchId,
            CommitPlanId = request.CommitPlanId,
            CommitStepId = request.CommitStepId,
            DestinationProduct = productKey,
            EntityType = entityType,
            Operation = request.Operation,
            IdempotencyKey = request.IdempotencyKey,
            PayloadJson = request.DeterministicPayload.GetRawText(),
            RecordArrSourceRecordId = request.RecordArrSourceRecordId,
            DisplayName = displayName,
            Status = "committed",
            CreatedAt = now,
            UpdatedAt = now
        };

        records.Add(record);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            db.Entry(record).State = EntityState.Detached;
            existing = await records.FirstOrDefaultAsync(
                x => x.TenantId == request.TenantId
                    && x.DestinationProduct == productKey
                    && x.IdempotencyKey == request.IdempotencyKey,
                cancellationToken);
            if (existing is null)
            {
                throw;
            }

            return Results.Ok(ToCommitResponse(existing));
        }

        return Results.Ok(ToCommitResponse(record));
    }

    private static SmartImportDestinationCommitResponse ToCommitResponse(SmartImportDestinationRecord record) =>
        new(
            Status: record.Status,
            ResultEntityId: record.Id.ToString("D"),
            DisplayName: record.DisplayName,
            Links: [],
            Retryable: false,
            ErrorCode: null,
            ErrorMessage: null);

    private static async Task EnsureDestinationRecordTableAsync(
        PlatformDbContext db,
        CancellationToken cancellationToken)
    {
        if (!db.Database.IsRelational())
        {
            return;
        }

        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS smart_import_destination_records (
                "Id" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "ActorPersonId" uuid NOT NULL,
                "ApprovedByPersonId" uuid NOT NULL,
                "ImportBatchId" uuid NOT NULL,
                "CommitPlanId" uuid NOT NULL,
                "CommitStepId" uuid NOT NULL,
                "DestinationProduct" character varying(64) NOT NULL,
                "EntityType" character varying(128) NOT NULL,
                "Operation" character varying(32) NOT NULL,
                "IdempotencyKey" character varying(256) NOT NULL,
                "PayloadJson" jsonb NOT NULL,
                "RecordArrSourceRecordId" character varying(128),
                "DisplayName" character varying(256) NOT NULL,
                "Status" character varying(32) NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_smart_import_destination_records" PRIMARY KEY ("Id")
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_smart_import_destination_records_idempotency"
                ON smart_import_destination_records ("TenantId", "DestinationProduct", "IdempotencyKey");

            CREATE INDEX IF NOT EXISTS "IX_smart_import_destination_records_product_entity_created"
                ON smart_import_destination_records ("TenantId", "DestinationProduct", "EntityType", "CreatedAt");
            """,
            cancellationToken);
    }

    private static string ResolveDisplayName(JsonElement payload, string entityType, Guid commitStepId)
    {
        foreach (var key in new[] { "displayName", "name", "title" })
        {
            if (TryReadString(payload, key, out var direct))
            {
                return direct;
            }
        }

        if (payload.TryGetProperty("proposedFields", out var fields)
            && fields.ValueKind == JsonValueKind.Object)
        {
            foreach (var key in new[] { "displayName", "name", "title", "assetTag", "assetId", "unitNumber", "vin", "primaryEmail" })
            {
                if (TryReadString(fields, key, out var fieldValue))
                {
                    return fieldValue;
                }
            }
        }

        if (payload.TryGetProperty("source", out var source)
            && TryReadString(source, "fileName", out var fileName))
        {
            return Path.GetFileNameWithoutExtension(fileName);
        }

        return $"{entityType.Replace('_', ' ')} import {commitStepId.ToString("N")[..8]}";
    }

    private static bool TryReadString(JsonElement root, string propertyName, out string value)
    {
        value = string.Empty;
        if (!root.TryGetProperty(propertyName, out var element)
            || element.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var candidate = element.GetString();
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        value = candidate.Trim();
        return true;
    }

    private static void ValidateServiceToken(
        HttpContext context,
        StlServiceTokenValidator tokenValidator,
        string targetProduct,
        string actionScope,
        Guid tenantId)
    {
        var token = ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization);
        tokenValidator.ValidateOrThrow(token, new ServiceTokenRequirements
        {
            ExpectedSourceProduct = "nexarr",
            RequiredTargetProduct = targetProduct,
            TenantId = tenantId,
            RequiredActionScope = actionScope
        });
    }

    private static IReadOnlyList<string> BuildRequiredReviewReasons(
        string productKey,
        string entityType,
        string operation,
        IReadOnlyList<string> incoming)
    {
        var reasons = new HashSet<string>(incoming, StringComparer.OrdinalIgnoreCase);
        if (CommitBlockedProducts.Contains(productKey))
        {
            reasons.Add(SmartImportReviewReasons.UnsupportedProductApi);
        }

        if (string.Equals(productKey, "compliancecore", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(SmartImportReviewReasons.ComplianceCoreImport);
        }

        if (string.Equals(productKey, "staffarr", StringComparison.OrdinalIgnoreCase)
            && entityType.Contains("person", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(SmartImportReviewReasons.PersonCreateOrLink);
        }

        if (string.Equals(productKey, "trainarr", StringComparison.OrdinalIgnoreCase)
            && (entityType.Contains("training", StringComparison.OrdinalIgnoreCase)
                || entityType.Contains("cert", StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add(SmartImportReviewReasons.TrainingOrCertificationRecord);
        }

        if (string.Equals(operation, "update", StringComparison.OrdinalIgnoreCase)
            || string.Equals(operation, "overwrite", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(SmartImportReviewReasons.Overwrite);
        }

        return reasons.Order(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IReadOnlyList<string> RequiredReviewReasons(this SmartImportDestinationValidateRequest request) =>
        request.ReviewStatus.Contains("review", StringComparison.OrdinalIgnoreCase)
            ? [SmartImportReviewReasons.HumanConfirmationRequired]
            : [];
}
