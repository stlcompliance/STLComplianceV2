using System.Security.Claims;
using NexArr.Api.Contracts;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class CompanionFieldEvidenceService(
    CompanionProductClient productClient,
    CompanionFieldSubmissionService submissions,
    CompanionFieldTaskValidationService validation)
{
    private const long MaxEvidenceBytes = 10 * 1024 * 1024;

    public async Task<CompanionFieldEvidenceResponse> SubmitAsync(
        ClaimsPrincipal principal,
        string accessToken,
        SubmitCompanionFieldEvidenceRequest request,
        CancellationToken cancellationToken = default)
    {
        CompanionFieldInboxService.RequireCompanionAccess(principal);
        await validation.EnsureAllowedAsync(
            principal,
            accessToken,
            request.TaskKey,
            CompanionFieldSubmissionKinds.Evidence,
            null,
            cancellationToken);

        if (!CompanionFieldTaskKeyParser.TryParse(request.TaskKey, out var task))
        {
            throw new StlApiException(
                CompanionFieldValidationReasonCodes.InvalidTaskKey,
                CompanionDeniedReasonCatalog.ToPlainMessage(CompanionFieldValidationReasonCodes.InvalidTaskKey),
                400);
        }

        var captureKind = NormalizeCaptureKind(request.CaptureKind);
        ValidatePayload(request.FileName, request.ContentType, request.ContentBase64);

        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        CompanionFieldEvidenceResponse response;
        try
        {
            response = task.ProductKey switch
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
        catch (StlApiException ex)
        {
            await submissions.RecordAsync(
                tenantId,
                userId,
                request.TaskKey.Trim(),
                task.ProductKey,
                CompanionFieldSubmissionKinds.Evidence,
                CompanionFieldSubmissionStatuses.Failed,
                ex.Message,
                DateTimeOffset.UtcNow,
                cancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            await submissions.RecordAsync(
                tenantId,
                userId,
                request.TaskKey.Trim(),
                task.ProductKey,
                CompanionFieldSubmissionKinds.Evidence,
                CompanionFieldSubmissionStatuses.Failed,
                ex.Message,
                DateTimeOffset.UtcNow,
                cancellationToken);
            throw;
        }

        await submissions.RecordAsync(
            tenantId,
            userId,
            request.TaskKey.Trim(),
            task.ProductKey,
            CompanionFieldSubmissionKinds.Evidence,
            CompanionFieldSubmissionStatuses.Synced,
            $"Uploaded {response.EvidenceTypeKey} evidence ({response.SizeBytes} bytes).",
            DateTimeOffset.UtcNow,
            cancellationToken);

        return response;
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
