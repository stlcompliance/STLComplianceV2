using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public static class PriceSnapshotCaptureRules
{
    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 500);

    public static int NormalizeStalenessHours(int? stalenessHours) =>
        Math.Clamp(stalenessHours ?? PriceSnapshotWorkerDefaults.StalenessHours, 1, 168);

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
        decimal? catalogUnitPrice,
        string? catalogCurrencyCode,
        decimal? catalogMinimumOrderQuantity,
        decimal? currentUnitPrice,
        string? currentCurrencyCode,
        decimal? currentMinimumOrderQuantity)
    {
        if (catalogUnitPrice is null || catalogUnitPrice <= 0)
        {
            return false;
        }

        if (currentUnitPrice is null)
        {
            return true;
        }

        var catalogCurrency = NormalizeCurrencyCode(catalogCurrencyCode);
        var currentCurrency = NormalizeCurrencyCode(currentCurrencyCode);

        return RoundPrice(currentUnitPrice.Value) != RoundPrice(catalogUnitPrice.Value)
            || !string.Equals(catalogCurrency, currentCurrency, StringComparison.OrdinalIgnoreCase)
            || RoundQuantity(catalogMinimumOrderQuantity) != RoundQuantity(currentMinimumOrderQuantity);
    }

    public static string BuildWorkerSnapshotKey(Guid partVendorLinkId, DateTimeOffset effectiveFrom) =>
        $"worker-{partVendorLinkId:N}-{effectiveFrom:yyyyMMddHHmmss}";

    public static decimal RoundPrice(decimal value) =>
        decimal.Round(value, 4, MidpointRounding.AwayFromZero);

    public static decimal? RoundQuantity(decimal? value) =>
        value is null ? null : decimal.Round(value.Value, 4, MidpointRounding.AwayFromZero);

    public static string NormalizeCurrencyCode(string? currencyCode) =>
        string.IsNullOrWhiteSpace(currencyCode) ? "USD" : currencyCode.Trim().ToUpperInvariant();
}
