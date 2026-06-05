using System.Security.Claims;
using NexArr.Api.Contracts;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class FieldCompanionFieldInspectionService(
    FieldCompanionProductClient productClient,
    FieldCompanionFieldSubmissionService submissions,
    FieldCompanionFieldTaskValidationService validation)
{
    public async Task<FieldCompanionFieldInspectionDetailResponse> GetDetailAsync(
        ClaimsPrincipal principal,
        string accessToken,
        string taskKey,
        CancellationToken cancellationToken = default)
    {
        var task = await EnsureInspectionTaskAsync(
            principal,
            accessToken,
            taskKey,
            cancellationToken);

        var detail = await productClient.GetMaintainArrInspectionRunAsync(
            accessToken,
            task.ResourceId,
            cancellationToken);

        return MapDetail(taskKey, detail);
    }

    public async Task<FieldCompanionFieldInspectionAnswersResponse> SubmitAnswersAsync(
        ClaimsPrincipal principal,
        string accessToken,
        SubmitFieldCompanionFieldInspectionAnswersRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await EnsureInspectionTaskAsync(
            principal,
            accessToken,
            request.TaskKey,
            cancellationToken);

        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        FieldCompanionFieldInspectionAnswersResponse response;
        try
        {
            var updated = await productClient.SubmitMaintainArrInspectionAnswersAsync(
                accessToken,
                task.ResourceId,
                request.Answers,
                cancellationToken);

            response = new FieldCompanionFieldInspectionAnswersResponse(
                request.TaskKey.Trim(),
                task.ProductKey,
                updated.InspectionRunId,
                updated.Status,
                updated.Answers.Count,
                updated.ChecklistItems.Count(x => x.IsRequired),
                updated.Answers.Select(MapAnswer).ToList());
        }
        catch (StlApiException ex)
        {
            await submissions.RecordAsync(
                tenantId,
                userId,
                request.TaskKey.Trim(),
                task.ProductKey,
                FieldCompanionFieldSubmissionKinds.Inspection,
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
                request.TaskKey.Trim(),
                task.ProductKey,
                FieldCompanionFieldSubmissionKinds.Inspection,
                FieldCompanionFieldSubmissionStatuses.Failed,
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
            FieldCompanionFieldSubmissionKinds.Inspection,
            FieldCompanionFieldSubmissionStatuses.Synced,
            $"Saved {response.AnswerCount} inspection answer(s).",
            DateTimeOffset.UtcNow,
            cancellationToken);

        return response;
    }

    public async Task<FieldCompanionFieldInspectionCompleteResponse> CompleteAsync(
        ClaimsPrincipal principal,
        string accessToken,
        CompleteFieldCompanionFieldInspectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await EnsureInspectionTaskAsync(
            principal,
            accessToken,
            request.TaskKey,
            cancellationToken);

        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        FieldCompanionFieldInspectionCompleteResponse response;
        try
        {
            var completed = await productClient.CompleteMaintainArrInspectionRunAsync(
                accessToken,
                task.ResourceId,
                cancellationToken);

            response = new FieldCompanionFieldInspectionCompleteResponse(
                request.TaskKey.Trim(),
                task.ProductKey,
                completed.InspectionRunId,
                completed.Status,
                completed.Result ?? "unknown",
                completed.CompletedAt ?? DateTimeOffset.UtcNow);
        }
        catch (StlApiException ex)
        {
            await submissions.RecordAsync(
                tenantId,
                userId,
                request.TaskKey.Trim(),
                task.ProductKey,
                FieldCompanionFieldSubmissionKinds.Inspection,
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
                request.TaskKey.Trim(),
                task.ProductKey,
                FieldCompanionFieldSubmissionKinds.Inspection,
                FieldCompanionFieldSubmissionStatuses.Failed,
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
            FieldCompanionFieldSubmissionKinds.Inspection,
            FieldCompanionFieldSubmissionStatuses.Synced,
            $"Inspection completed ({response.Result}).",
            DateTimeOffset.UtcNow,
            cancellationToken);

        return response;
    }

    private async Task<FieldCompanionFieldTaskReference> EnsureInspectionTaskAsync(
        ClaimsPrincipal principal,
        string accessToken,
        string taskKey,
        CancellationToken cancellationToken)
    {
        FieldCompanionFieldInboxService.RequireFieldCompanionAccess(principal);
        await validation.EnsureAllowedAsync(
            principal,
            accessToken,
            taskKey,
            FieldCompanionFieldSubmissionKinds.Inspection,
            null,
            cancellationToken);

        if (!FieldCompanionFieldTaskKeyParser.TryParse(taskKey, out var task))
        {
            throw new StlApiException(
                FieldCompanionFieldValidationReasonCodes.InvalidTaskKey,
                FieldCompanionDeniedReasonCatalog.ToPlainMessage(FieldCompanionFieldValidationReasonCodes.InvalidTaskKey),
                400);
        }

        if (!string.Equals(task.ProductKey, "maintainarr", StringComparison.Ordinal)
            || !string.Equals(task.ResourceType, "inspection", StringComparison.Ordinal))
        {
            throw new StlApiException(
                FieldCompanionFieldValidationReasonCodes.InspectionUnsupported,
                FieldCompanionDeniedReasonCatalog.ToPlainMessage(FieldCompanionFieldValidationReasonCodes.InspectionUnsupported),
                409);
        }

        return task;
    }

    private static FieldCompanionFieldInspectionDetailResponse MapDetail(
        string taskKey,
        MaintainArrInspectionRunUpstreamResponse detail) =>
        new(
            taskKey,
            "maintainarr",
            detail.InspectionRunId,
            detail.AssetTag,
            detail.AssetName,
            detail.TemplateName,
            detail.Status,
            detail.Result,
            detail.ChecklistItems
                .Select(item => new FieldCompanionFieldInspectionChecklistItem(
                    item.ChecklistItemId,
                    item.ItemKey,
                    item.Prompt,
                    item.ItemType,
                    item.IsRequired,
                    item.SortOrder))
                .ToList(),
            detail.Answers.Select(MapAnswer).ToList());

    private static FieldCompanionFieldInspectionAnswer MapAnswer(MaintainArrInspectionAnswerUpstreamResponse answer) =>
        new(
            answer.ChecklistItemId,
            answer.ItemKey,
            answer.PassFailValue,
            answer.NumericValue,
            answer.TextValue,
            answer.AnsweredAt);
}
