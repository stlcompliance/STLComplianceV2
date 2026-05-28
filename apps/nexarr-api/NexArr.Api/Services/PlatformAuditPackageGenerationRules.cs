using NexArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public static class PlatformAuditPackageGenerationRules
{
    public const int MaxArtifactZipBytes = 100 * 1024 * 1024;

    public const int MaxArtifactJsonChars = 50 * 1024 * 1024;

    public const int MaxErrorMessageLength = 2000;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 5, 1, 25);

    public static string NormalizeFormat(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new StlApiException(
                "platform_audit_package_generation.format_required",
                "Export format is required (zip or json).",
                400);
        }

        var normalized = raw.Trim().ToLowerInvariant();
        if (normalized is not (PlatformAuditPackageGenerationFormats.Zip or PlatformAuditPackageGenerationFormats.Json))
        {
            throw new StlApiException(
                "platform_audit_package_generation.format_invalid",
                "Export format must be zip or json.",
                400);
        }

        return normalized;
    }

    public static string TruncateErrorMessage(string? message) =>
        string.IsNullOrWhiteSpace(message)
            ? "Unknown error"
            : message.Length <= MaxErrorMessageLength
                ? message
                : message[..MaxErrorMessageLength];

    public static bool IsDownloadReady(PlatformAuditPackageGenerationJob job) =>
        job.Status == PlatformAuditPackageGenerationJobStatuses.Completed
        && (job.Format == PlatformAuditPackageGenerationFormats.Zip
            ? job.ArtifactZip is { Length: > 0 }
            : !string.IsNullOrWhiteSpace(job.ArtifactJson));
}
