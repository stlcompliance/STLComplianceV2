using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public static class ProductFactMirrorRules
{
    public static string NormalizeScopeKey(string scopeKey)
    {
        var normalized = string.IsNullOrWhiteSpace(scopeKey) ? "tenant" : scopeKey.Trim().ToLowerInvariant();
        if (normalized.Length is < 2 or > 256)
        {
            throw new StlApiException(
                "product_facts.invalid_scope_key",
                "Scope key must be between 2 and 256 characters.",
                400);
        }

        return normalized;
    }

    public static string NormalizeFactKey(string factKey)
    {
        var normalized = factKey.Trim().ToLowerInvariant();
        if (normalized.Length is < 3 or > 128)
        {
            throw new StlApiException(
                "product_facts.invalid_fact_key",
                "Fact key must be between 3 and 128 characters.",
                400);
        }

        return normalized;
    }

    public static string NormalizeIdempotencyKey(string idempotencyKey)
    {
        var normalized = idempotencyKey.Trim();
        if (normalized.Length is < 8 or > 256)
        {
            throw new StlApiException(
                "product_facts.invalid_idempotency_key",
                "Idempotency key must be between 8 and 256 characters.",
                400);
        }

        return normalized;
    }

    public static string ResolveScopeKeyFromContext(IReadOnlyDictionary<string, string>? context)
    {
        if (context is null)
        {
            return "tenant";
        }

        if (context.TryGetValue("scope_key", out var explicitScope) && !string.IsNullOrWhiteSpace(explicitScope))
        {
            return NormalizeScopeKey(explicitScope);
        }

        if (context.TryGetValue("purchase_request_id", out var purchaseRequestId)
            && Guid.TryParse(purchaseRequestId, out _))
        {
            return $"purchase_request:{purchaseRequestId.Trim().ToLowerInvariant()}";
        }

        if (context.TryGetValue("purchase_order_id", out var purchaseOrderId)
            && Guid.TryParse(purchaseOrderId, out _))
        {
            return $"purchase_order:{purchaseOrderId.Trim().ToLowerInvariant()}";
        }

        if (context.TryGetValue("vendor_party_id", out var vendorPartyId)
            && Guid.TryParse(vendorPartyId, out _))
        {
            return $"vendor:{vendorPartyId.Trim().ToLowerInvariant()}";
        }

        if (context.TryGetValue("procurement_exception_id", out var exceptionId)
            && Guid.TryParse(exceptionId, out _))
        {
            return $"procurement_exception:{exceptionId.Trim().ToLowerInvariant()}";
        }

        return "tenant";
    }

    public static bool TryToJsonElement(ProductFactMirror mirror, out System.Text.Json.JsonElement value)
    {
        value = default;
        return mirror.ValueType.ToLowerInvariant() switch
        {
            FactValueTypes.Boolean => mirror.BooleanValue is bool boolean
                && SetRaw(System.Text.Json.JsonSerializer.SerializeToElement(boolean), out value),
            FactValueTypes.Number => mirror.NumberValue is decimal number
                && SetRaw(System.Text.Json.JsonSerializer.SerializeToElement(number), out value),
            FactValueTypes.Date => mirror.DateValue is DateOnly date
                && SetRaw(System.Text.Json.JsonSerializer.SerializeToElement(date.ToString("O")), out value),
            _ => !string.IsNullOrWhiteSpace(mirror.StringValue)
                && SetRaw(System.Text.Json.JsonSerializer.SerializeToElement(mirror.StringValue), out value),
        };
    }

    private static bool SetRaw(System.Text.Json.JsonElement element, out System.Text.Json.JsonElement value)
    {
        value = element;
        return true;
    }
}
