using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class ReorderEvaluationService(
    SupplyArrDbContext db,
    PurchaseRequestService purchaseRequests,
    ComplianceCoreFactPublicationClient complianceCoreClient,
    IOptions<ComplianceCoreClientOptions> complianceCoreOptions,
    ISupplyArrAuditService audit)
{
    public const string ProcessEvaluationActionScope = "supplyarr.reorder.evaluate";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f8");

    public async Task<ReorderEvaluationResponse> EvaluateAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var evaluatedAt = DateTimeOffset.UtcNow;
        var suggestions = await BuildSuggestionsAsync(tenantId, evaluatedAt, null, cancellationToken);
        return new ReorderEvaluationResponse(evaluatedAt, suggestions);
    }

    public async Task<PartReorderPolicyResponse> GetPolicyAsync(
        Guid tenantId,
        Guid partId,
        CancellationToken cancellationToken = default)
    {
        var part = await LoadPartAsync(tenantId, partId, cancellationToken);
        return MapPolicy(part);
    }

    public async Task<PartReorderPolicyResponse> UpsertPolicyAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partId,
        UpsertPartReorderPolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        var part = await db.Parts.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == partId,
            cancellationToken);
        if (part is null)
        {
            throw new StlApiException("parts.not_found", "Part was not found.", 404);
        }

        var reorderPoint = NormalizeOptionalQuantity(request.ReorderPoint, "Reorder point");
        var reorderQuantity = NormalizeOptionalQuantity(request.ReorderQuantity, "Reorder quantity");
        if (reorderPoint is null && reorderQuantity is not null)
        {
            throw new StlApiException(
                "reorder.validation",
                "Reorder quantity requires a reorder point.",
                400);
        }

        part.ReorderPoint = reorderPoint;
        part.ReorderQuantity = reorderQuantity;
        part.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "part.reorder_policy.upsert",
            tenantId,
            actorUserId,
            "part",
            part.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapPolicy(part);
    }

    public async Task<PurchaseRequestResponse> CreatePurchaseRequestFromSuggestionsAsync(
        Guid tenantId,
        Guid actorUserId,
        CreatePurchaseRequestFromReorderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.PartIds is not { Count: > 0 })
        {
            throw new StlApiException(
                "reorder.validation",
                "At least one part id is required.",
                400);
        }

        var evaluation = await EvaluateAsync(tenantId, cancellationToken);
        var selected = evaluation.Suggestions
            .Where(x => request.PartIds.Contains(x.PartId))
            .ToList();

        if (selected.Count != request.PartIds.Count)
        {
            throw new StlApiException(
                "reorder.validation",
                "One or more parts are not eligible for reorder suggestions.",
                400);
        }

        if (selected.Any(x => x.HasOpenPurchaseRequest))
        {
            throw new StlApiException(
                "reorder.open_purchase_request",
                "One or more parts already have an open purchase request.",
                409);
        }

        var vendorPartyId = ResolveVendorPartyId(selected);
        var lines = selected
            .Select(x => new CreatePurchaseRequestLineRequest(
                x.PartId,
                x.SuggestedOrderQuantity,
                $"Auto-suggested reorder (available {x.QuantityAvailable}, reorder point {x.ReorderPoint})"))
            .ToList();

        var created = await purchaseRequests.CreateAsync(
            tenantId,
            actorUserId,
            new CreatePurchaseRequestRequest(
                request.RequestKey,
                request.Title,
                request.Notes,
                vendorPartyId,
                lines),
            cancellationToken);

        await audit.WriteAsync(
            "reorder.create_purchase_request",
            tenantId,
            actorUserId,
            "purchase_request",
            created.PurchaseRequestId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return created;
    }

    public async Task<PendingReorderEvaluationResponse> ListPendingAsync(
        Guid? tenantId,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var evaluatedAt = DateTimeOffset.UtcNow;
        var normalizedBatchSize = NormalizeBatchSize(batchSize);
        if (tenantId is null)
        {
            return new PendingReorderEvaluationResponse(evaluatedAt, normalizedBatchSize, []);
        }

        var suggestions = await BuildSuggestionsAsync(
            tenantId.Value,
            evaluatedAt,
            normalizedBatchSize,
            cancellationToken);

        var items = suggestions
            .Select(x => new PendingReorderEvaluationItem(
                x.PartId,
                x.PartKey,
                x.ReorderPoint,
                x.QuantityAvailable,
                x.SuggestedOrderQuantity,
                x.HasOpenPurchaseRequest))
            .ToList();

        return new PendingReorderEvaluationResponse(evaluatedAt, normalizedBatchSize, items);
    }

    public async Task<ProcessReorderEvaluationResponse> ProcessBatchAsync(
        ProcessReorderEvaluationRequest request,
        CancellationToken cancellationToken = default)
    {
        var evaluatedAt = DateTimeOffset.UtcNow;
        var batchSize = NormalizeBatchSize(request.BatchSize ?? 100);
        if (request.TenantId is null)
        {
            return new ProcessReorderEvaluationResponse(
                evaluatedAt,
                batchSize,
                0,
                0,
                0,
                0,
                [],
                []);
        }

        var suggestions = await BuildSuggestionsAsync(
            request.TenantId.Value,
            evaluatedAt,
            batchSize,
            cancellationToken);

        var actionable = suggestions
            .Where(x => !x.HasOpenPurchaseRequest)
            .ToList();

        await PublishLowInventoryFactsAsync(
            request.TenantId.Value,
            evaluatedAt,
            suggestions,
            cancellationToken);

        var skippedOpen = suggestions.Count - actionable.Count;
        var createdPurchaseRequestIds = new List<Guid>();

        if (request.CreateDraftPurchaseRequests && actionable.Count > 0)
        {
            foreach (var vendorGroup in actionable.GroupBy(x => x.PreferredVendorPartyId))
            {
                var groupItems = vendorGroup.ToList();
                var requestKey = $"reorder-{evaluatedAt:yyyyMMddHHmmss}-{createdPurchaseRequestIds.Count + 1}";
                var created = await purchaseRequests.CreateAsync(
                    request.TenantId.Value,
                    WorkerActorUserId,
                    new CreatePurchaseRequestRequest(
                        requestKey,
                        "Auto reorder evaluation",
                        "Draft purchase request created by reorder evaluation worker.",
                        vendorGroup.Key,
                        groupItems
                            .Select(x => new CreatePurchaseRequestLineRequest(
                                x.PartId,
                                x.SuggestedOrderQuantity,
                                $"Worker reorder suggestion (available {x.QuantityAvailable})"))
                            .ToList()),
                    cancellationToken);

                createdPurchaseRequestIds.Add(created.PurchaseRequestId);
            }

            await audit.WriteAsync(
                "reorder.process_evaluation",
                request.TenantId.Value,
                WorkerActorUserId,
                "reorder_evaluation",
                evaluatedAt.ToString("O"),
                "Succeeded",
                cancellationToken: cancellationToken);
        }

        return new ProcessReorderEvaluationResponse(
            evaluatedAt,
            batchSize,
            suggestions.Count,
            actionable.Count,
            skippedOpen,
            createdPurchaseRequestIds.Count,
            createdPurchaseRequestIds,
            suggestions);
    }

    private async Task PublishLowInventoryFactsAsync(
        Guid tenantId,
        DateTimeOffset evaluatedAt,
        IReadOnlyList<ReorderSuggestionResponse> suggestions,
        CancellationToken cancellationToken)
    {
        if (suggestions.Count == 0
            || string.IsNullOrWhiteSpace(complianceCoreOptions.Value.ServiceToken))
        {
            return;
        }

        var publicationId = Guid.NewGuid();
        var facts = new List<ComplianceCoreFactPublicationItem>(suggestions.Count * 2);
        foreach (var suggestion in suggestions)
        {
            var scopeKey = ScopeForPart(suggestion.PartId);
            facts.Add(new ComplianceCoreFactPublicationItem(
                SupplyArrComplianceCoreFactKeys.CriticalInventoryBelowMinimum,
                "boolean",
                scopeKey,
                null,
                true,
                null,
                null,
                "part",
                suggestion.PartId,
                "reorder_evaluation.processed",
                IdempotencyKey(publicationId, suggestion.PartId, SupplyArrComplianceCoreFactKeys.CriticalInventoryBelowMinimum)));
            facts.Add(new ComplianceCoreFactPublicationItem(
                SupplyArrComplianceCoreFactKeys.InventoryQuantityAvailable,
                "number",
                scopeKey,
                null,
                null,
                suggestion.QuantityAvailable,
                null,
                "part",
                suggestion.PartId,
                "reorder_evaluation.processed",
                IdempotencyKey(publicationId, suggestion.PartId, SupplyArrComplianceCoreFactKeys.InventoryQuantityAvailable)));
        }

        await complianceCoreClient.IngestAsync(
            new ComplianceCoreIngestProductFactsPayload(
                tenantId,
                publicationId,
                "supplyarr",
                evaluatedAt,
                facts),
            cancellationToken);
    }

    private async Task<IReadOnlyList<ReorderSuggestionResponse>> BuildSuggestionsAsync(
        Guid tenantId,
        DateTimeOffset evaluatedAt,
        int? batchSize,
        CancellationToken cancellationToken)
    {
        var stockTotals = await LoadStockTotalsAsync(tenantId, cancellationToken);
        var openPartIds = await LoadOpenPurchaseRequestPartIdsAsync(tenantId, cancellationToken);

        var query = db.Parts
            .AsNoTracking()
            .Include(x => x.VendorLinks)
                .ThenInclude(x => x.ExternalParty)
            .Where(x => x.TenantId == tenantId
                && x.Status == "active"
                && x.ReorderPoint != null);

        if (batchSize is > 0)
        {
            query = query.OrderBy(x => x.PartKey).Take(batchSize.Value);
        }

        var parts = await query.ToListAsync(cancellationToken);
        var suggestions = new List<ReorderSuggestionResponse>();

        foreach (var part in parts)
        {
            if (!ReorderEvaluationRules.HasReorderPolicy(part.ReorderPoint))
            {
                continue;
            }

            stockTotals.TryGetValue(part.Id, out var stock);
            var quantityOnHand = stock.OnHand;
            var quantityReserved = stock.Reserved;
            var quantityAvailable = quantityOnHand - quantityReserved;
            var reorderPoint = part.ReorderPoint!.Value;

            if (!ReorderEvaluationRules.NeedsReorder(quantityAvailable, reorderPoint))
            {
                continue;
            }

            var preferredVendor = part.VendorLinks
                .OrderByDescending(x => x.IsPreferred)
                .ThenBy(x => x.ExternalParty.DisplayName)
                .FirstOrDefault();

            var hasOpenPurchaseRequest = openPartIds.Contains(part.Id);
            suggestions.Add(new ReorderSuggestionResponse(
                part.Id,
                part.PartKey,
                part.DisplayName,
                part.UnitOfMeasure,
                reorderPoint,
                part.ReorderQuantity,
                quantityOnHand,
                quantityReserved,
                quantityAvailable,
                ReorderEvaluationRules.ResolveSuggestedQuantity(
                    quantityAvailable,
                    reorderPoint,
                    part.ReorderQuantity),
                preferredVendor?.ExternalPartyId,
                preferredVendor?.ExternalParty.PartyKey,
                preferredVendor?.ExternalParty.DisplayName,
                hasOpenPurchaseRequest,
                hasOpenPurchaseRequest ? "open_purchase_request" : null));
        }

        return suggestions
            .OrderBy(x => x.PartKey)
            .ToList();
    }

    private async Task<Dictionary<Guid, (decimal OnHand, decimal Reserved)>> LoadStockTotalsAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var rows = await db.PartStockLevels
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .GroupBy(x => x.PartId)
            .Select(g => new
            {
                PartId = g.Key,
                OnHand = g.Sum(x => x.QuantityOnHand),
                Reserved = g.Sum(x => x.QuantityReserved)
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            x => x.PartId,
            x => (x.OnHand, x.Reserved));
    }

    private async Task<HashSet<Guid>> LoadOpenPurchaseRequestPartIdsAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var openStatuses = new[]
        {
            PurchaseRequestStatuses.Draft,
            PurchaseRequestStatuses.Submitted
        };

        var partIds = await db.PurchaseRequestLines
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && openStatuses.Contains(x.PurchaseRequest.Status))
            .Select(x => x.PartId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return partIds.ToHashSet();
    }

    private async Task<Part> LoadPartAsync(
        Guid tenantId,
        Guid partId,
        CancellationToken cancellationToken)
    {
        var part = await db.Parts.AsNoTracking().FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == partId,
            cancellationToken);
        if (part is null)
        {
            throw new StlApiException("parts.not_found", "Part was not found.", 404);
        }

        return part;
    }

    private static PartReorderPolicyResponse MapPolicy(Part part) =>
        new(
            part.Id,
            part.PartKey,
            part.DisplayName,
            part.ReorderPoint,
            part.ReorderQuantity,
            part.UpdatedAt);

    private static Guid? ResolveVendorPartyId(IReadOnlyList<ReorderSuggestionResponse> selected)
    {
        var vendorIds = selected
            .Select(x => x.PreferredVendorPartyId)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        return vendorIds.Count == 1 ? vendorIds[0] : null;
    }

    private static int NormalizeBatchSize(int batchSize) =>
        Math.Clamp(batchSize, 1, 500);

    private static string ScopeForPart(Guid partId) => $"part:{partId:D}".ToLowerInvariant();

    private static string IdempotencyKey(Guid publicationId, Guid partId, string factKey) =>
        $"supplyarr:{publicationId:D}:{partId:D}:{factKey}".ToLowerInvariant();

    private static decimal? NormalizeOptionalQuantity(decimal? value, string label)
    {
        if (value is null)
        {
            return null;
        }

        if (value < 0)
        {
            throw new StlApiException(
                "reorder.validation",
                $"{label} cannot be negative.",
                400);
        }

        return value;
    }
}
