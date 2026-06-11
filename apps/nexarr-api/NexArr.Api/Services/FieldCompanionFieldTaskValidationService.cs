using System.Security.Claims;
using NexArr.Api.Contracts;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class FieldCompanionFieldTaskValidationService(FieldCompanionFieldInboxService fieldInboxService)
{
    public async Task<ValidateFieldCompanionFieldTaskResponse> ValidateAsync(
        ClaimsPrincipal principal,
        string accessToken,
        ValidateFieldCompanionFieldTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        FieldCompanionFieldInboxService.RequireFieldCompanionAccess(principal);

        var submissionKind = NormalizeSubmissionKind(request.SubmissionKind);
        var taskKey = request.TaskKey.Trim();

        if (!FieldCompanionFieldTaskKeyParser.TryParse(taskKey, out var taskRef))
        {
            return Denied(
                FieldCompanionFieldValidationReasonCodes.InvalidTaskKey,
                taskKey,
                request.ProductKey?.Trim().ToLowerInvariant() ?? string.Empty);
        }

        if (!string.IsNullOrWhiteSpace(request.ProductKey)
            && !string.Equals(request.ProductKey.Trim(), taskRef.ProductKey, StringComparison.OrdinalIgnoreCase))
        {
            return Denied(
                FieldCompanionFieldValidationReasonCodes.ProductMismatch,
                taskKey,
                taskRef.ProductKey);
        }

        if (!IsEntitledToProduct(principal, taskRef.ProductKey))
        {
            return Denied(
                FieldCompanionFieldValidationReasonCodes.NotEntitled,
                taskKey,
                taskRef.ProductKey);
        }

        if (!SupportsSubmissionKind(taskRef, submissionKind))
        {
            return Denied(
                FieldCompanionFieldValidationReasonCodes.EvidenceUnsupported,
                taskKey,
                taskRef.ProductKey);
        }

        var inbox = await fieldInboxService.GetAsync(principal, accessToken, cancellationToken);
        var source = inbox.Sources.FirstOrDefault(slice =>
            string.Equals(slice.ProductKey, taskRef.ProductKey, StringComparison.OrdinalIgnoreCase));

        if (source is { Entitled: true, Fetched: false })
        {
            return Denied(
                FieldCompanionFieldValidationReasonCodes.InboxUnavailable,
                taskKey,
                taskRef.ProductKey,
                source.ErrorMessage);
        }

        var match = inbox.Items.FirstOrDefault(item =>
            string.Equals(item.TaskKey, taskKey, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            return Denied(
                FieldCompanionFieldValidationReasonCodes.NotInInbox,
                taskKey,
                taskRef.ProductKey);
        }

        if (submissionKind == FieldCompanionFieldSubmissionKinds.Evidence
            && !string.IsNullOrWhiteSpace(match.BlockedReason)
            && !BlockedReasonAllowsEvidence(match.BlockedReason))
        {
            return Denied(
                FieldCompanionFieldValidationReasonCodes.NotInInbox,
                taskKey,
                taskRef.ProductKey,
                FieldCompanionDeniedReasonCatalog.ForBlockedTask(match.BlockedReason));
        }

        return new ValidateFieldCompanionFieldTaskResponse(
            Allowed: true,
            ReasonCode: null,
            ReasonMessage: null,
            match.TaskKey,
            match.ProductKey,
            match.Title,
            match.BlockedReason);
    }

    public async Task EnsureAllowedAsync(
        ClaimsPrincipal principal,
        string accessToken,
        string taskKey,
        string submissionKind,
        string? productKey,
        CancellationToken cancellationToken = default)
    {
        var result = await ValidateAsync(
            principal,
            accessToken,
            new ValidateFieldCompanionFieldTaskRequest(taskKey, submissionKind, productKey),
            cancellationToken);

        if (result.Allowed)
        {
            return;
        }

        var statusCode = result.ReasonCode switch
        {
            FieldCompanionFieldValidationReasonCodes.NotEntitled => 403,
            FieldCompanionFieldValidationReasonCodes.EvidenceUnsupported => 409,
            FieldCompanionFieldValidationReasonCodes.DvirUnsupported => 409,
            FieldCompanionFieldValidationReasonCodes.InspectionUnsupported => 409,
            FieldCompanionFieldValidationReasonCodes.WorkOrderUnsupported => 409,
            FieldCompanionFieldValidationReasonCodes.ReceivingUnsupported => 409,
            FieldCompanionFieldValidationReasonCodes.NotInInbox => 404,
            FieldCompanionFieldValidationReasonCodes.InboxUnavailable => 503,
            _ => 400,
        };

        throw new StlApiException(
            result.ReasonCode ?? FieldCompanionFieldValidationReasonCodes.InvalidTaskKey,
            result.ReasonMessage ?? FieldCompanionDeniedReasonCatalog.ToPlainMessage(
                FieldCompanionFieldValidationReasonCodes.InvalidTaskKey),
            statusCode);
    }

    private static string NormalizeSubmissionKind(string submissionKind)
    {
        var normalized = submissionKind.Trim().ToLowerInvariant();
        if (!string.Equals(normalized, FieldCompanionFieldSubmissionKinds.Acknowledge, StringComparison.Ordinal)
            && !string.Equals(normalized, FieldCompanionFieldSubmissionKinds.Evidence, StringComparison.Ordinal)
            && !string.Equals(normalized, FieldCompanionFieldSubmissionKinds.Dvir, StringComparison.Ordinal)
            && !string.Equals(normalized, FieldCompanionFieldSubmissionKinds.Inspection, StringComparison.Ordinal)
            && !string.Equals(normalized, FieldCompanionFieldSubmissionKinds.WorkOrder, StringComparison.Ordinal)
            && !string.Equals(normalized, FieldCompanionFieldSubmissionKinds.Receiving, StringComparison.Ordinal))
        {
            throw new StlApiException(
                FieldCompanionFieldValidationReasonCodes.UnsupportedSubmissionKind,
                FieldCompanionDeniedReasonCatalog.ToPlainMessage(
                    FieldCompanionFieldValidationReasonCodes.UnsupportedSubmissionKind),
                400);
        }

        return normalized;
    }

    private static bool SupportsSubmissionKind(FieldCompanionFieldTaskReference taskRef, string submissionKind)
    {
        if (string.Equals(submissionKind, FieldCompanionFieldSubmissionKinds.Acknowledge, StringComparison.Ordinal))
        {
            return true;
        }

        if (string.Equals(submissionKind, FieldCompanionFieldSubmissionKinds.Evidence, StringComparison.Ordinal))
        {
            return string.Equals(taskRef.ProductKey, "trainarr", StringComparison.Ordinal)
                && string.Equals(taskRef.ResourceType, "assignment", StringComparison.Ordinal);
        }

        if (string.Equals(submissionKind, FieldCompanionFieldSubmissionKinds.Dvir, StringComparison.Ordinal))
        {
            return string.Equals(taskRef.ProductKey, "routarr", StringComparison.Ordinal)
                && string.Equals(taskRef.ResourceType, "trip", StringComparison.Ordinal);
        }

        if (string.Equals(submissionKind, FieldCompanionFieldSubmissionKinds.Inspection, StringComparison.Ordinal))
        {
            return string.Equals(taskRef.ProductKey, "maintainarr", StringComparison.Ordinal)
                && string.Equals(taskRef.ResourceType, "inspection", StringComparison.Ordinal);
        }

        if (string.Equals(submissionKind, FieldCompanionFieldSubmissionKinds.Receiving, StringComparison.Ordinal))
        {
            return string.Equals(taskRef.ProductKey, "loadarr", StringComparison.Ordinal)
                && string.Equals(taskRef.ResourceType, "receiving", StringComparison.Ordinal);
        }

        return string.Equals(submissionKind, FieldCompanionFieldSubmissionKinds.WorkOrder, StringComparison.Ordinal)
            && string.Equals(taskRef.ProductKey, "maintainarr", StringComparison.Ordinal)
            && string.Equals(taskRef.ResourceType, "work-order", StringComparison.Ordinal);
    }

    private static bool BlockedReasonAllowsEvidence(string blockedReason)
    {
        var normalized = blockedReason.Trim();
        return normalized.Contains("evidence", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("upload", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEntitledToProduct(ClaimsPrincipal principal, string productKey)
    {
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        return principal.HasProductEntitlement(productKey);
    }

    private static ValidateFieldCompanionFieldTaskResponse Denied(
        string reasonCode,
        string taskKey,
        string productKey,
        string? overrideMessage = null) =>
        new(
            Allowed: false,
            reasonCode,
            overrideMessage ?? FieldCompanionDeniedReasonCatalog.ToPlainMessage(reasonCode),
            taskKey,
            productKey,
            null,
            null);
}
