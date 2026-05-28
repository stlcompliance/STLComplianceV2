using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public static class AvailabilitySnapshotCaptureRules
{
    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 500);

    public static int NormalizeStalenessHours(int? stalenessHours) =>
        Math.Clamp(stalenessHours ?? AvailabilitySnapshotWorkerDefaults.StalenessHours, 1, 168);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 10, 1, 100);

    public static bool IsStale(DateTimeOffset? lastCapturedAt, DateTimeOffset asOfUtc, int stalenessHours)
    {
        if (lastCapturedAt is null)
        {
            return true;
        }

        var threshold = asOfUtc.AddHours(-stalenessHours);
        return lastCapturedAt < threshold;
    }

    public static bool NeedsCapture(
        decimal? catalogQuantityAvailable,
        string? catalogAvailabilityStatus,
        decimal? currentQuantityAvailable,
        string? currentAvailabilityStatus)
    {
        if (!HasCatalogAvailabilityData(catalogQuantityAvailable, catalogAvailabilityStatus))
        {
            return false;
        }

        if (currentQuantityAvailable is null && string.IsNullOrWhiteSpace(currentAvailabilityStatus))
        {
            return true;
        }

        if (catalogQuantityAvailable is not null)
        {
            if (currentQuantityAvailable is null
                || catalogQuantityAvailable.Value != currentQuantityAvailable.Value)
            {
                return true;
            }
        }

        var normalizedCatalogStatus = NormalizeOptionalStatus(catalogAvailabilityStatus);
        if (normalizedCatalogStatus is not null)
        {
            var normalizedCurrentStatus = NormalizeOptionalStatus(currentAvailabilityStatus);
            if (normalizedCurrentStatus is null
                || !string.Equals(normalizedCatalogStatus, normalizedCurrentStatus, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasCatalogAvailabilityData(
        decimal? catalogQuantityAvailable,
        string? catalogAvailabilityStatus) =>
        catalogQuantityAvailable is not null
        || !string.IsNullOrWhiteSpace(catalogAvailabilityStatus);

    public static string BuildWorkerSnapshotKey(Guid partVendorLinkId, DateTimeOffset effectiveFrom) =>
        $"worker-av-{partVendorLinkId:N}-{effectiveFrom:yyyyMMddHHmmss}";

    public static decimal? NormalizeOptionalQuantity(decimal? quantityAvailable)
    {
        if (quantityAvailable is null)
        {
            return null;
        }

        if (quantityAvailable.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityAvailable), "Quantity available cannot be negative.");
        }

        return decimal.Round(quantityAvailable.Value, 4, MidpointRounding.AwayFromZero);
    }

    public static string? NormalizeOptionalStatus(string? availabilityStatus)
    {
        if (string.IsNullOrWhiteSpace(availabilityStatus))
        {
            return null;
        }

        var normalized = availabilityStatus.Trim().ToLowerInvariant();
        return normalized switch
        {
            AvailabilityStatuses.InStock => AvailabilityStatuses.InStock,
            AvailabilityStatuses.Limited => AvailabilityStatuses.Limited,
            AvailabilityStatuses.Backorder => AvailabilityStatuses.Backorder,
            AvailabilityStatuses.OutOfStock => AvailabilityStatuses.OutOfStock,
            AvailabilityStatuses.Discontinued => AvailabilityStatuses.Discontinued,
            _ => throw new ArgumentOutOfRangeException(
                nameof(availabilityStatus),
                "Availability status must be in_stock, limited, backorder, out_of_stock, or discontinued."),
        };
    }

    public static string ResolveCaptureStatus(string? catalogAvailabilityStatus, string? currentAvailabilityStatus) =>
        NormalizeOptionalStatus(catalogAvailabilityStatus)
        ?? NormalizeOptionalStatus(currentAvailabilityStatus)
        ?? AvailabilityStatuses.InStock;
}
