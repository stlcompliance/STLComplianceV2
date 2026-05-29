using System.Security.Claims;
using NexArr.Api.Contracts;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class CompanionFieldWorkOrderService(
    CompanionProductClient productClient,
    CompanionFieldSubmissionService submissions,
    CompanionFieldTaskValidationService validation)
{
    public async Task<CompanionFieldWorkOrderDetailResponse> GetDetailAsync(
        ClaimsPrincipal principal,
        string accessToken,
        string taskKey,
        CancellationToken cancellationToken = default)
    {
        var task = await EnsureWorkOrderTaskAsync(
            principal,
            accessToken,
            taskKey,
            cancellationToken);

        var detail = await productClient.GetMaintainArrWorkOrderAsync(
            accessToken,
            task.ResourceId,
            cancellationToken);
        var tasks = await productClient.ListMaintainArrWorkOrderTasksAsync(
            accessToken,
            task.ResourceId,
            cancellationToken);
        var labor = await productClient.ListMaintainArrWorkOrderLaborAsync(
            accessToken,
            task.ResourceId,
            cancellationToken);

        return MapDetail(taskKey, detail, tasks, labor);
    }

    public async Task<CompanionFieldWorkOrderStatusResponse> UpdateStatusAsync(
        ClaimsPrincipal principal,
        string accessToken,
        UpdateCompanionFieldWorkOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await EnsureWorkOrderTaskAsync(
            principal,
            accessToken,
            request.TaskKey,
            cancellationToken);

        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        CompanionFieldWorkOrderStatusResponse response;
        try
        {
            var updated = await productClient.UpdateMaintainArrWorkOrderStatusAsync(
                accessToken,
                task.ResourceId,
                request.Status,
                cancellationToken);

            response = new CompanionFieldWorkOrderStatusResponse(
                request.TaskKey.Trim(),
                task.ProductKey,
                updated.WorkOrderId,
                updated.Status,
                updated.UpdatedAt);
        }
        catch (StlApiException ex)
        {
            await submissions.RecordAsync(
                tenantId,
                userId,
                request.TaskKey.Trim(),
                task.ProductKey,
                CompanionFieldSubmissionKinds.WorkOrder,
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
                CompanionFieldSubmissionKinds.WorkOrder,
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
            CompanionFieldSubmissionKinds.WorkOrder,
            CompanionFieldSubmissionStatuses.Synced,
            $"Work order status updated to {response.Status}.",
            DateTimeOffset.UtcNow,
            cancellationToken);

        return response;
    }

    public async Task<CompanionFieldWorkOrderLaborResponse> LogLaborAsync(
        ClaimsPrincipal principal,
        string accessToken,
        LogCompanionFieldWorkOrderLaborRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await EnsureWorkOrderTaskAsync(
            principal,
            accessToken,
            request.TaskKey,
            cancellationToken);

        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();
        var personId = principal.GetPersonId().ToString();

        CompanionFieldWorkOrderLaborResponse response;
        try
        {
            var created = await productClient.LogMaintainArrWorkOrderLaborAsync(
                accessToken,
                task.ResourceId,
                personId,
                request.HoursWorked,
                request.LaborTypeKey,
                request.WorkOrderTaskLineId,
                request.Notes,
                cancellationToken);

            var detail = await productClient.GetMaintainArrWorkOrderAsync(
                accessToken,
                task.ResourceId,
                cancellationToken);

            response = new CompanionFieldWorkOrderLaborResponse(
                request.TaskKey.Trim(),
                task.ProductKey,
                task.ResourceId,
                created.LaborEntryId,
                created.HoursWorked,
                created.LaborTypeKey,
                detail.Status,
                created.LoggedAt);
        }
        catch (StlApiException ex)
        {
            await submissions.RecordAsync(
                tenantId,
                userId,
                request.TaskKey.Trim(),
                task.ProductKey,
                CompanionFieldSubmissionKinds.WorkOrder,
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
                CompanionFieldSubmissionKinds.WorkOrder,
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
            CompanionFieldSubmissionKinds.WorkOrder,
            CompanionFieldSubmissionStatuses.Synced,
            $"Logged {response.HoursWorked:0.##} hour(s) of {response.LaborTypeKey} labor.",
            DateTimeOffset.UtcNow,
            cancellationToken);

        return response;
    }

    private async Task<CompanionFieldTaskReference> EnsureWorkOrderTaskAsync(
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
            CompanionFieldSubmissionKinds.WorkOrder,
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
            || !string.Equals(task.ResourceType, "work-order", StringComparison.Ordinal))
        {
            throw new StlApiException(
                CompanionFieldValidationReasonCodes.WorkOrderUnsupported,
                CompanionDeniedReasonCatalog.ToPlainMessage(CompanionFieldValidationReasonCodes.WorkOrderUnsupported),
                409);
        }

        return task;
    }

    private static CompanionFieldWorkOrderDetailResponse MapDetail(
        string taskKey,
        MaintainArrWorkOrderDetailUpstreamResponse detail,
        IReadOnlyList<MaintainArrWorkOrderTaskLineUpstreamResponse> tasks,
        IReadOnlyList<MaintainArrWorkOrderLaborEntryUpstreamResponse> labor) =>
        new(
            taskKey,
            "maintainarr",
            detail.WorkOrderId,
            detail.WorkOrderNumber,
            detail.AssetTag,
            detail.AssetName,
            detail.Title,
            detail.Description,
            detail.Priority,
            detail.Status,
            tasks
                .Select(taskLine => new CompanionFieldWorkOrderTaskLine(
                    taskLine.TaskLineId,
                    taskLine.Title,
                    taskLine.Description,
                    taskLine.SortOrder,
                    taskLine.Status,
                    taskLine.CompletedAt))
                .ToList(),
            labor
                .Select(entry => new CompanionFieldWorkOrderLaborEntry(
                    entry.LaborEntryId,
                    entry.PersonId,
                    entry.HoursWorked,
                    entry.LaborTypeKey,
                    entry.Notes,
                    entry.LoggedAt))
                .ToList());
}
