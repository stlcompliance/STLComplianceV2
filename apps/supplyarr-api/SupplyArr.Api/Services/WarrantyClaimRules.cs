using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public static class WarrantyClaimRules
{
    public static string NormalizeClaimKey(string claimKey)
    {
        var normalized = (claimKey ?? string.Empty).Trim();
        if (normalized.Length is < 2 or > 64)
        {
            throw new StlApiException(
                "warranty_claims.invalid_key",
                "Claim key must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    public static string NormalizeClaimType(string claimType)
    {
        var normalized = (claimType ?? string.Empty).Trim().ToLowerInvariant();
        if (!WarrantyClaimTypes.All.Contains(normalized))
        {
            throw new StlApiException(
                "warranty_claims.invalid_type",
                "Claim type is not supported.",
                400);
        }

        return normalized;
    }

    public static string NormalizeProblemDescription(string description)
    {
        var normalized = (description ?? string.Empty).Trim();
        if (normalized.Length is < 3 or > 2048)
        {
            throw new StlApiException(
                "warranty_claims.invalid_description",
                "Problem description must be between 3 and 2048 characters.",
                400);
        }

        return normalized;
    }

    public static string NormalizeOptionalText(string? value, int maxLength, string fieldName)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException(
                "warranty_claims.invalid_field",
                $"{fieldName} must be at most {maxLength} characters.",
                400);
        }

        return normalized;
    }

    public static string NormalizeSupplierDisposition(string disposition)
    {
        var normalized = (disposition ?? string.Empty).Trim().ToLowerInvariant();
        if (!WarrantyClaimSupplierDispositions.All.Contains(normalized))
        {
            throw new StlApiException(
                "warranty_claims.invalid_disposition",
                "Supplier disposition is not supported.",
                400);
        }

        return normalized;
    }

    public static decimal NormalizeQuantity(decimal quantity)
    {
        if (quantity <= 0m)
        {
            throw new StlApiException(
                "warranty_claims.invalid_quantity",
                "Quantity claimed must be greater than zero.",
                400);
        }

        return decimal.Round(quantity, 4, MidpointRounding.AwayFromZero);
    }

    public static void EnsureStatus(WarrantyClaim entity, params string[] allowedStatuses)
    {
        if (!allowedStatuses.Contains(entity.Status, StringComparer.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "warranty_claims.invalid_status",
                $"Warranty claim must be in {string.Join(", ", allowedStatuses)} status.",
                409);
        }
    }

    public static void Transition(WarrantyClaim entity, string nextStatus)
    {
        entity.Status = nextStatus;
    }
}
