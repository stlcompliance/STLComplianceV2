using LoadArr.Api.Endpoints;
using LoadArr.Api.Options;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace LoadArr.Api.Services;

public interface ILoadArrSupplyArrItemReferenceService
{
    Task<IReadOnlyCollection<LoadArrSupplyArrItemReferenceResponse>> ListItemReferencesAsync(
        Guid tenantId,
        string? query,
        CancellationToken cancellationToken);

    Task<LoadArrSupplyArrItemReferenceResponse?> GetItemReferenceAsync(
        Guid tenantId,
        string supplyarrItemId,
        CancellationToken cancellationToken);
}

public sealed class LoadArrSupplyArrItemReferenceService(
    SupplyArrItemReferenceLookupClient itemReferenceClient,
    IOptions<SupplyArrClientOptions> options) : ILoadArrSupplyArrItemReferenceService
{
    public async Task<IReadOnlyCollection<LoadArrSupplyArrItemReferenceResponse>> ListItemReferencesAsync(
        Guid tenantId,
        string? query,
        CancellationToken cancellationToken)
    {
        try
        {
            var items = await itemReferenceClient.ListAsync(
                tenantId,
                options.Value.ServiceToken,
                Normalize(query),
                cancellationToken);

            return items
                .Select(MapItemReference)
                .ToArray();
        }
        catch (StlApiException ex)
        {
            throw new StlApiException(
                "loadarr.item_references.unavailable",
                "LoadArr item references are unavailable because SupplyArr item synchronization is not available right now.",
                ex.StatusCode);
        }
    }

    public async Task<LoadArrSupplyArrItemReferenceResponse?> GetItemReferenceAsync(
        Guid tenantId,
        string supplyarrItemId,
        CancellationToken cancellationToken)
    {
        var normalizedItemId = Normalize(supplyarrItemId);
        if (normalizedItemId is null)
        {
            return null;
        }

        var items = await ListItemReferencesAsync(tenantId, normalizedItemId, cancellationToken);
        return items.FirstOrDefault(item =>
            string.Equals(item.SupplyarrItemId, normalizedItemId, StringComparison.OrdinalIgnoreCase));
    }

    private static LoadArrSupplyArrItemReferenceResponse MapItemReference(SupplyArrItemReferenceLookupResponse item)
    {
        // SupplyArr currently owns whether serial/lot traceability is required, but not the
        // finer-grained lot-vs-serial distinction or hazard/SDS flags that LoadArr will need
        // before reopening warehouse write paths.
        return new LoadArrSupplyArrItemReferenceResponse(
            item.PartKey,
            item.PartKey,
            item.DisplayName,
            item.UnitOfMeasure,
            string.IsNullOrWhiteSpace(item.CategoryKey) ? "inventory_part" : item.CategoryKey,
            false,
            false,
            false,
            false,
            item.UpdatedAt.ToString("O"),
            item.RequiresSerialLotTracking);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
