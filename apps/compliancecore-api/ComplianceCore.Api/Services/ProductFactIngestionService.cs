using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public sealed class ProductFactIngestionService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public const string IngestFactsActionScope = "compliancecore.facts.ingest";

    public async Task<IngestProductFactsResponse> IngestAsync(
        IngestProductFactsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Facts.Count == 0)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "product_facts.validation",
                "At least one fact publication item is required.",
                400);
        }

        if (request.Facts.Count > 100)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "product_facts.validation",
                "A maximum of 100 fact items can be published per request.",
                400);
        }

        var sourceProduct = request.SourceProduct.Trim().ToLowerInvariant();
        var accepted = 0;
        var skipped = 0;
        var now = DateTimeOffset.UtcNow;

        foreach (var item in request.Facts)
        {
            var idempotencyKey = ProductFactMirrorRules.NormalizeIdempotencyKey(item.IdempotencyKey);
            var duplicate = await db.ProductFactMirrors.AnyAsync(
                x => x.TenantId == request.TenantId && x.IdempotencyKey == idempotencyKey,
                cancellationToken);
            if (duplicate)
            {
                skipped++;
                continue;
            }

            var factKey = ProductFactMirrorRules.NormalizeFactKey(item.FactKey);
            var scopeKey = ProductFactMirrorRules.NormalizeScopeKey(item.ScopeKey);
            var valueType = item.ValueType.Trim().ToLowerInvariant();
            ValidateValue(valueType, item);

            var mirror = await db.ProductFactMirrors
                .FirstOrDefaultAsync(
                    x => x.TenantId == request.TenantId
                        && x.SourceProduct == sourceProduct
                        && x.FactKey == factKey
                        && x.ScopeKey == scopeKey,
                    cancellationToken);

            if (mirror is null)
            {
                mirror = new ProductFactMirror
                {
                    Id = Guid.NewGuid(),
                    TenantId = request.TenantId,
                    SourceProduct = sourceProduct,
                    FactKey = factKey,
                    ScopeKey = scopeKey,
                    CreatedAt = now,
                };
                db.ProductFactMirrors.Add(mirror);
            }
            else if (request.PublishedAt < mirror.PublishedAt)
            {
                skipped++;
                continue;
            }

            mirror.ValueType = valueType;
            mirror.StringValue = item.StringValue;
            mirror.BooleanValue = item.BooleanValue;
            mirror.NumberValue = item.NumberValue;
            mirror.DateValue = ParseDate(item.DateValue);
            mirror.SourceEntityType = item.SourceEntityType.Trim().ToLowerInvariant();
            mirror.SourceEntityId = item.SourceEntityId;
            mirror.SourceEventKind = item.SourceEventKind.Trim().ToLowerInvariant();
            mirror.SourcePublicationId = request.PublicationId;
            mirror.IdempotencyKey = idempotencyKey;
            mirror.PublishedAt = request.PublishedAt;
            mirror.UpdatedAt = now;

            accepted++;
        }

        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "product_facts.ingest",
            request.TenantId,
            actorUserId: null,
            "product_fact_publication",
            request.PublicationId.ToString(),
            "Succeeded",
            reasonCode: $"{sourceProduct}:{accepted}:{skipped}",
            cancellationToken: cancellationToken);

        return new IngestProductFactsResponse(
            request.TenantId,
            request.PublicationId,
            accepted,
            skipped);
    }

    private static void ValidateValue(string valueType, ProductFactPublicationItemRequest item)
    {
        if (!FactValueTypes.All.Contains(valueType))
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "product_facts.invalid_value_type",
                "Value type is not supported.",
                400);
        }

        var hasValue = valueType switch
        {
            FactValueTypes.Boolean => item.BooleanValue.HasValue,
            FactValueTypes.Number => item.NumberValue.HasValue,
            FactValueTypes.Date => !string.IsNullOrWhiteSpace(item.DateValue),
            _ => !string.IsNullOrWhiteSpace(item.StringValue),
        };

        if (!hasValue)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "product_facts.missing_value",
                $"A value is required for fact type {valueType}.",
                400);
        }
    }

    private static DateOnly? ParseDate(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? null
            : DateOnly.TryParse(raw, out var date)
                ? date
                : throw new STLCompliance.Shared.Contracts.StlApiException(
                    "product_facts.invalid_date",
                    "Date value must be ISO-8601 date.",
                    400);
}
