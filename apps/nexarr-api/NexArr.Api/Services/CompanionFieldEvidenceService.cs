using System.Security.Claims;
using NexArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class CompanionFieldEvidenceService(CompanionProductClient productClient)
{
    private const long MaxEvidenceBytes = 10 * 1024 * 1024;

    public async Task<CompanionFieldEvidenceResponse> SubmitAsync(
        ClaimsPrincipal principal,
        string accessToken,
        SubmitCompanionFieldEvidenceRequest request,
        CancellationToken cancellationToken = default)
    {
        CompanionFieldInboxService.RequireCompanionAccess(principal);

        if (!CompanionFieldTaskKeyParser.TryParse(request.TaskKey, out var task))
        {
            throw new StlApiException(
                "companion.field_task.invalid_key",
                "Task key is not a recognized companion field task reference.",
                400);
        }

        var captureKind = NormalizeCaptureKind(request.CaptureKind);
        ValidatePayload(request.FileName, request.ContentType, request.ContentBase64);

        return task.ProductKey switch
        {
            "trainarr" when string.Equals(task.ResourceType, "assignment", StringComparison.Ordinal) =>
                await productClient.SubmitTrainArrAssignmentEvidenceAsync(
                    accessToken,
                    task.ResourceId,
                    captureKind,
                    request.FileName,
                    request.ContentType,
                    request.ContentBase64,
                    request.Notes,
                    cancellationToken),
            _ => throw new StlApiException(
                "companion.field_evidence.unsupported_task",
                $"Evidence capture is not yet supported for {task.ProductKey} field tasks.",
                409),
        };
    }

    private static string NormalizeCaptureKind(string captureKind)
    {
        var normalized = captureKind.Trim().ToLowerInvariant();
        if (!CompanionFieldEvidenceCaptureKinds.All.Contains(normalized))
        {
            throw new StlApiException(
                "companion.field_evidence.invalid_capture_kind",
                "Capture kind must be photo, document, or signature.",
                400);
        }

        return normalized;
    }

    private static void ValidatePayload(string fileName, string contentType, string contentBase64)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new StlApiException("companion.field_evidence.file_required", "File name is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new StlApiException("companion.field_evidence.content_type_required", "Content type is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(contentBase64))
        {
            throw new StlApiException("companion.field_evidence.content_required", "Evidence content is required.", 400);
        }

        try
        {
            var bytes = Convert.FromBase64String(contentBase64);
            if (bytes.Length == 0)
            {
                throw new StlApiException("companion.field_evidence.content_required", "Evidence content is required.", 400);
            }

            if (bytes.Length > MaxEvidenceBytes)
            {
                throw new StlApiException(
                    "companion.field_evidence.too_large",
                    "Evidence must be 10 MB or smaller.",
                    400);
            }
        }
        catch (FormatException)
        {
            throw new StlApiException(
                "companion.field_evidence.invalid_base64",
                "Evidence content must be valid base64.",
                400);
        }
    }
}
