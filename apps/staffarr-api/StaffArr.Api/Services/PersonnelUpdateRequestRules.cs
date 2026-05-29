using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public static class PersonnelUpdateRequestRules
{
    private static readonly HashSet<string> AllowedTypes =
    [
        PersonnelUpdateRequestTypes.PhoneUpdate,
        PersonnelUpdateRequestTypes.ContactInfoUpdate,
        PersonnelUpdateRequestTypes.ProfileCorrection,
        PersonnelUpdateRequestTypes.Other,
    ];

    public static string NormalizeRequestType(string requestType)
    {
        if (string.IsNullOrWhiteSpace(requestType))
        {
            throw new StlApiException(
                "personnel_update.validation",
                "Request type is required.",
                400);
        }

        var normalized = requestType.Trim().ToLowerInvariant();
        if (!AllowedTypes.Contains(normalized))
        {
            throw new StlApiException(
                "personnel_update.validation",
                $"Request type '{requestType}' is not supported.",
                400);
        }

        return normalized;
    }

    public static string NormalizeFieldKey(string fieldKey)
    {
        if (string.IsNullOrWhiteSpace(fieldKey))
        {
            throw new StlApiException(
                "personnel_update.validation",
                "Field key is required.",
                400);
        }

        var normalized = fieldKey.Trim().ToLowerInvariant();
        if (normalized.Length > 64)
        {
            throw new StlApiException(
                "personnel_update.validation",
                "Field key must be 64 characters or fewer.",
                400);
        }

        return normalized;
    }

    public static string NormalizeRequestedValue(string requestedValue)
    {
        if (string.IsNullOrWhiteSpace(requestedValue))
        {
            throw new StlApiException(
                "personnel_update.validation",
                "Requested value is required.",
                400);
        }

        var trimmed = requestedValue.Trim();
        if (trimmed.Length > 512)
        {
            throw new StlApiException(
                "personnel_update.validation",
                "Requested value must be 512 characters or fewer.",
                400);
        }

        return trimmed;
    }

    public static string? NormalizeOptionalText(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new StlApiException(
                "personnel_update.validation",
                $"{fieldName} must be {maxLength} characters or fewer.",
                400);
        }

        return trimmed;
    }

    public static string NormalizeReviewDecision(string decision)
    {
        if (string.IsNullOrWhiteSpace(decision))
        {
            throw new StlApiException(
                "personnel_update.validation",
                "Review decision is required.",
                400);
        }

        var normalized = decision.Trim().ToLowerInvariant();
        if (normalized is not ("approve" or "deny"))
        {
            throw new StlApiException(
                "personnel_update.validation",
                "Review decision must be approve or deny.",
                400);
        }

        return normalized;
    }

    public static string? NormalizeReviewNotes(string? reviewNotes) =>
        NormalizeOptionalText(reviewNotes, 1024, "Review notes");

    public static bool SupportsProfileApply(string fieldKey) =>
        fieldKey is "work_phone"
            or "primary_email"
            or "job_title"
            or "given_name"
            or "family_name";
}
