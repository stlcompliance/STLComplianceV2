using System.Security.Claims;
using NexArr.Api.Contracts;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class CompanionFieldInspectionService(
    CompanionProductClient productClient,
    CompanionFieldSubmissionService submissions,
    CompanionFieldTaskValidationService validation)
{
    public async Task<CompanionFieldInspectionDetailResponse> GetDetailAsync(
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

    public async Task<CompanionFieldInspectionAnswersResponse> SubmitAnswersAsync(
        ClaimsPrincipal principal,
        string accessToken,
        SubmitCompanionFieldInspectionAnswersRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await EnsureInspectionTaskAsync(
            principal,
            accessToken,
            request.TaskKey,
            cancellationToken);

        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        CompanionFieldInspectionAnswersResponse response;
        try
        {
            var updated = await productClient.SubmitMaintainArrInspectionAnswersAsync(
                accessToken,
                task.ResourceId,
                request.Answers,
                cancellationToken);

            response = new CompanionFieldInspectionAnswersResponse(
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
                CompanionFieldSubmissionKinds.Inspection,
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
                CompanionFieldSubmissionKinds.Inspection,
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
            CompanionFieldSubmissionKinds.Inspection,
            CompanionFieldSubmissionStatuses.Synced,
            $"Saved {response.AnswerCount} inspection answer(s).",
            DateTimeOffset.UtcNow,
            cancellationToken);

        return response;
    }

    public async Task<CompanionFieldInspectionCompleteResponse> CompleteAsync(
        ClaimsPrincipal principal,
        string accessToken,
        CompleteCompanionFieldInspectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await EnsureInspectionTaskAsync(
            principal,
            accessToken,
            request.TaskKey,
            cancellationToken);

        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        CompanionFieldInspectionCompleteResponse response;
        try
        {
            var completed = await productClient.CompleteMaintainArrInspectionRunAsync(
                accessToken,
                task.ResourceId,
                cancellationToken);

            response = new CompanionFieldInspectionCompleteResponse(
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
                CompanionFieldSubmissionKinds.Inspection,
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
                CompanionFieldSubmissionKinds.Inspection,
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
            CompanionFieldSubmissionKinds.Inspection,
            CompanionFieldSubmissionStatuses.Synced,
            $"Inspection completed ({response.Result}).",
            DateTimeOffset.UtcNow,
            cancellationToken);

        return response;
    }

    private async Task<CompanionFieldTaskReference> EnsureInspectionTaskAsync(
        ClaimsPrincipal principal,
        string accessToken,
        string taskKey,
        CancellationToken cancellationToken)
    {
        CompanionFieldInboxService.RequireCompanionAccess(principal);
        await validation.EnsureAllowedAsync(
            principal,
            accessToken,
            taskKey,
            CompanionFieldSubmissionKinds.Inspection,
            null,
            cancellationToken);

        if (!CompanionFieldTaskKeyParser.TryParse(taskKey, out var task))
        {
            throw new StlApiException(
                CompanionFieldValidationReasonCodes.InvalidTaskKey,
                CompanionDeniedReasonCatalog.ToPlainMessage(CompanionFieldValidationReasonCodes.InvalidTaskKey),
                400);
        }

        if (!string.Equals(task.ProductKey, "maintainarr", StringComparison.Ordinal)
            || !string.Equals(task.ResourceType, "inspection", StringComparison.Ordinal))
        {
            throw new StlApiException(
                CompanionFieldValidationReasonCodes.InspectionUnsupported,
                CompanionDeniedReasonCatalog.ToPlainMessage(CompanionFieldValidationReasonCodes.InspectionUnsupported),
                409);
        }

        return task;
    }

    private static CompanionFieldInspectionDetailResponse MapDetail(
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
                .Select(item => new CompanionFieldInspectionChecklistItem(
                    item.ChecklistItemId,
                    item.ItemKey,
                    item.Prompt,
                    item.ItemType,
                    item.IsRequired,
                    item.SortOrder))
                .ToList(),
            detail.Answers.Select(MapAnswer).ToList());

    private static CompanionFieldInspectionAnswer MapAnswer(MaintainArrInspectionAnswerUpstreamResponse answer) =>
        new(
            answer.ChecklistItemId,
            answer.ItemKey,
            answer.PassFailValue,
            answer.NumericValue,
            answer.TextValue,
            answer.AnsweredAt);
}
