using System.Security.Claims;
using NexArr.Api.Contracts;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class CompanionFieldTaskValidationService(CompanionFieldInboxService fieldInboxService)
{
    public async Task<ValidateCompanionFieldTaskResponse> ValidateAsync(
        ClaimsPrincipal principal,
        string accessToken,
        ValidateCompanionFieldTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        CompanionFieldInboxService.RequireCompanionAccess(principal);

        var submissionKind = NormalizeSubmissionKind(request.SubmissionKind);
        var taskKey = request.TaskKey.Trim();

        if (!CompanionFieldTaskKeyParser.TryParse(taskKey, out var taskRef))
        {
            return Denied(
                CompanionFieldValidationReasonCodes.InvalidTaskKey,
                taskKey,
                request.ProductKey?.Trim().ToLowerInvariant() ?? string.Empty);
        }

        if (!string.IsNullOrWhiteSpace(request.ProductKey)
            && !string.Equals(request.ProductKey.Trim(), taskRef.ProductKey, StringComparison.OrdinalIgnoreCase))
        {
            return Denied(
                CompanionFieldValidationReasonCodes.ProductMismatch,
                taskKey,
                taskRef.ProductKey);
        }

        if (!IsEntitledToProduct(principal, taskRef.ProductKey))
        {
            return Denied(
                CompanionFieldValidationReasonCodes.NotEntitled,
                taskKey,
                taskRef.ProductKey);
        }

        if (!SupportsSubmissionKind(taskRef, submissionKind))
        {
            return Denied(
                CompanionFieldValidationReasonCodes.EvidenceUnsupported,
                taskKey,
                taskRef.ProductKey);
        }

        var inbox = await fieldInboxService.GetAsync(principal, accessToken, cancellationToken);
        var source = inbox.Sources.FirstOrDefault(slice =>
            string.Equals(slice.ProductKey, taskRef.ProductKey, StringComparison.OrdinalIgnoreCase));

        if (source is { Entitled: true, Fetched: false })
        {
            return Denied(
                CompanionFieldValidationReasonCodes.InboxUnavailable,
                taskKey,
                taskRef.ProductKey,
                source.ErrorMessage);
        }

        var match = inbox.Items.FirstOrDefault(item =>
            string.Equals(item.TaskKey, taskKey, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            return Denied(
                CompanionFieldValidationReasonCodes.NotInInbox,
                taskKey,
                taskRef.ProductKey);
        }

        if (submissionKind == CompanionFieldSubmissionKinds.Evidence
            && !string.IsNullOrWhiteSpace(match.BlockedReason)
            && !BlockedReasonAllowsEvidence(match.BlockedReason))
        {
            return Denied(
                CompanionFieldValidationReasonCodes.NotInInbox,
                taskKey,
                taskRef.ProductKey,
                CompanionDeniedReasonCatalog.ForBlockedTask(match.BlockedReason));
        }

        return new ValidateCompanionFieldTaskResponse(
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
            new ValidateCompanionFieldTaskRequest(taskKey, submissionKind, productKey),
            cancellationToken);

        if (result.Allowed)
        {
            return;
        }

        var statusCode = result.ReasonCode switch
        {
            CompanionFieldValidationReasonCodes.NotEntitled => 403,
            CompanionFieldValidationReasonCodes.EvidenceUnsupported => 409,
            CompanionFieldValidationReasonCodes.NotInInbox => 404,
            CompanionFieldValidationReasonCodes.InboxUnavailable => 503,
            _ => 400,
        };

        throw new StlApiException(
            result.ReasonCode ?? CompanionFieldValidationReasonCodes.InvalidTaskKey,
            result.ReasonMessage ?? CompanionDeniedReasonCatalog.ToPlainMessage(
                CompanionFieldValidationReasonCodes.InvalidTaskKey),
            statusCode);
    }

    private static string NormalizeSubmissionKind(string submissionKind)
    {
        var normalized = submissionKind.Trim().ToLowerInvariant();
        if (!string.Equals(normalized, CompanionFieldSubmissionKinds.Acknowledge, StringComparison.Ordinal)
            && !string.Equals(normalized, CompanionFieldSubmissionKinds.Evidence, StringComparison.Ordinal))
        {
            throw new StlApiException(
                CompanionFieldValidationReasonCodes.UnsupportedSubmissionKind,
                CompanionDeniedReasonCatalog.ToPlainMessage(
                    CompanionFieldValidationReasonCodes.UnsupportedSubmissionKind),
                400);
        }

        return normalized;
    }

    private static bool SupportsSubmissionKind(CompanionFieldTaskReference taskRef, string submissionKind)
    {
        if (string.Equals(submissionKind, CompanionFieldSubmissionKinds.Acknowledge, StringComparison.Ordinal))
        {
            return true;
        }

        return string.Equals(taskRef.ProductKey, "trainarr", StringComparison.Ordinal)
            && string.Equals(taskRef.ResourceType, "assignment", StringComparison.Ordinal);
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

    private static ValidateCompanionFieldTaskResponse Denied(
        string reasonCode,
        string taskKey,
        string productKey,
        string? overrideMessage = null) =>
        new(
            Allowed: false,
            reasonCode,
            overrideMessage ?? CompanionDeniedReasonCatalog.ToPlainMessage(reasonCode),
            taskKey,
            productKey,
            null,
            null);
}
