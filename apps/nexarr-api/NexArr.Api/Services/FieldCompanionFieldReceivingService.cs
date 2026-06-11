using System.Security.Claims;
using NexArr.Api.Contracts;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class FieldCompanionFieldReceivingService(
    FieldCompanionProductClient productClient,
    FieldCompanionFieldSubmissionService submissions,
    FieldCompanionFieldTaskValidationService validation)
{
    public async Task<FieldCompanionFieldReceivingDetailResponse> GetDetailAsync(
        ClaimsPrincipal principal,
        string accessToken,
        string taskKey,
        CancellationToken cancellationToken = default)
    {
        var task = await EnsureReceivingTaskAsync(
            principal,
            accessToken,
            taskKey,
            cancellationToken);

        return MapDetail(
            task.TaskKey,
            await productClient.GetLoadArrReceivingSessionAsync(
                accessToken,
                task.OwnerResourceId,
                cancellationToken));
    }

    public async Task<FieldCompanionFieldReceivingLineResponse> UpdateLineAsync(
        ClaimsPrincipal principal,
        string accessToken,
        UpdateFieldCompanionFieldReceivingLineRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureReceivingTaskAsync(
            principal,
            accessToken,
            request.TaskKey,
            cancellationToken);

        throw new StlApiException(
            FieldCompanionFieldValidationReasonCodes.ReceivingUnsupported,
            "Line quantity edits are not available for this task yet. Open the receiving session in LoadArr to adjust counts.",
            409);
    }

    public async Task<FieldCompanionFieldReceivingPostResponse> PostAsync(
        ClaimsPrincipal principal,
        string accessToken,
        PostFieldCompanionFieldReceivingRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await EnsureReceivingTaskAsync(
            principal,
            accessToken,
            request.TaskKey,
            cancellationToken);

        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        FieldCompanionFieldReceivingPostResponse response;
        try
        {
            var session = await productClient.GetLoadArrReceivingSessionAsync(
                accessToken,
                task.OwnerResourceId,
                cancellationToken);
            var completed = await productClient.CompleteLoadArrReceivingSessionAsync(
                accessToken,
                task.OwnerResourceId,
                BuildLoadArrCompleteRequest(principal, session),
                cancellationToken);

            response = new FieldCompanionFieldReceivingPostResponse(
                task.TaskKey,
                task.ProductKey,
                task.OwnerResourceId,
                completed.Session.Status,
                ParseLoadArrTimestamp(completed.Session.CompletedAtUtc)
                    ?? DateTimeOffset.UtcNow);
        }
        catch (StlApiException ex)
        {
            await submissions.RecordAsync(
                tenantId,
                userId,
                task.TaskKey,
                task.ProductKey,
                FieldCompanionFieldSubmissionKinds.Receiving,
                FieldCompanionFieldSubmissionStatuses.Failed,
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
                task.TaskKey,
                task.ProductKey,
                FieldCompanionFieldSubmissionKinds.Receiving,
                FieldCompanionFieldSubmissionStatuses.Failed,
                ex.Message,
                DateTimeOffset.UtcNow,
                cancellationToken);
            throw;
        }

        await submissions.RecordAsync(
            tenantId,
            userId,
            task.TaskKey,
            task.ProductKey,
            FieldCompanionFieldSubmissionKinds.Receiving,
            FieldCompanionFieldSubmissionStatuses.Synced,
            "Receiving session completed in LoadArr.",
            DateTimeOffset.UtcNow,
            cancellationToken);

        return response;
    }

    private async Task<ResolvedFieldCompanionReceivingTask> EnsureReceivingTaskAsync(
        ClaimsPrincipal principal,
        string accessToken,
        string taskKey,
        CancellationToken cancellationToken)
    {
        FieldCompanionFieldInboxService.RequireFieldCompanionAccess(principal);
        var normalizedTaskKey = taskKey.Trim();
        await validation.EnsureAllowedAsync(
            principal,
            accessToken,
            normalizedTaskKey,
            FieldCompanionFieldSubmissionKinds.Receiving,
            null,
            cancellationToken);

        if (!FieldCompanionFieldTaskKeyParser.TryParse(normalizedTaskKey, out var task))
        {
            throw new StlApiException(
                FieldCompanionFieldValidationReasonCodes.InvalidTaskKey,
                FieldCompanionDeniedReasonCatalog.ToPlainMessage(FieldCompanionFieldValidationReasonCodes.InvalidTaskKey),
                400);
        }

        if (!string.Equals(task.ResourceType, "receiving", StringComparison.Ordinal)
            || !string.Equals(task.ProductKey, "loadarr", StringComparison.Ordinal))
        {
            throw new StlApiException(
                FieldCompanionFieldValidationReasonCodes.ReceivingUnsupported,
                FieldCompanionDeniedReasonCatalog.ToPlainMessage(FieldCompanionFieldValidationReasonCodes.ReceivingUnsupported),
                409);
        }

        var sessionId = await productClient.ResolveLoadArrReceivingSessionIdAsync(
            accessToken,
            normalizedTaskKey,
            cancellationToken);
        return new ResolvedFieldCompanionReceivingTask(
            normalizedTaskKey,
            task.ProductKey,
            sessionId,
            task.ResourceId);
    }

    private static FieldCompanionFieldReceivingDetailResponse MapDetail(
        string taskKey,
        LoadArrReceivingSessionUpstreamResponse detail)
    {
        var primaryLine = detail.Lines.FirstOrDefault();
        var notes = primaryLine?.EvidenceSummary;
        if (string.IsNullOrWhiteSpace(notes))
        {
            notes = detail.SupplierNameSnapshot;
        }

        return new FieldCompanionFieldReceivingDetailResponse(
            taskKey,
            "loadarr",
            detail.Id,
            detail.ReceivingNumber,
            detail.Status,
            detail.SourceObjectId,
            primaryLine?.WarehouseLocationId ?? string.Empty,
            primaryLine?.LocationNameSnapshot ?? detail.StaffarrSiteNameSnapshot,
            detail.StaffarrSiteNameSnapshot,
            notes ?? string.Empty,
            detail.Lines
                .Select((line, index) => new FieldCompanionFieldReceivingLine(
                    line.Id,
                    index + 1,
                    line.SupplyarrItemId,
                    line.ItemNameSnapshot,
                    line.ExpectedQuantity,
                    line.ReceivedQuantity,
                    line.ExpectedQuantity,
                    Math.Max(line.ExpectedQuantity - line.ReceivedQuantity, 0m),
                    LoadArrLineHasOpenException(line) ? 1 : 0))
                .ToList());
    }

    private static bool LoadArrLineHasOpenException(LoadArrReceivingLineUpstreamResponse line) =>
        line.Status.Contains("blocked", StringComparison.OrdinalIgnoreCase)
        || !string.IsNullOrWhiteSpace(line.DiscrepancyReasonCode);

    private static CompleteLoadArrReceivingSessionUpstreamRequest BuildLoadArrCompleteRequest(
        ClaimsPrincipal principal,
        LoadArrReceivingSessionUpstreamResponse session)
    {
        if (!string.Equals(session.Status, "open", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                FieldCompanionFieldValidationReasonCodes.ReceivingUnsupported,
                "This receiving session cannot be completed from Field Companion until LoadArr clears the current status.",
                409);
        }

        var line = session.Lines.FirstOrDefault()
            ?? throw new StlApiException(
                "fieldcompanion.field_receiving.line_missing",
                "Receiving session does not contain any lines to complete.",
                409);

        return new CompleteLoadArrReceivingSessionUpstreamRequest(
            session.ReceivingType,
            session.SourceProductKey,
            session.SourceObjectType,
            session.SourceObjectId,
            session.SupplierNameSnapshot,
            principal.GetPersonId().ToString("D"),
            line.SupplyarrItemId,
            line.ExpectedQuantity,
            line.ReceivedQuantity,
            line.WarehouseLocationId,
            line.LotCode,
            line.SerialCode,
            line.Condition,
            line.DiscrepancyReasonCode,
            null,
            line.EvidenceSummary);
    }

    private static DateTimeOffset? ParseLoadArrTimestamp(string? value) =>
        DateTimeOffset.TryParse(value, out var parsed) ? parsed : null;

    private sealed record ResolvedFieldCompanionReceivingTask(
        string TaskKey,
        string ProductKey,
        string OwnerResourceId,
        Guid TaskResourceId);
}
