using System.Security.Claims;
using NexArr.Api.Contracts;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class FieldCompanionFieldWorkOrderService(
    FieldCompanionProductClient productClient,
    FieldCompanionFieldSubmissionService submissions,
    FieldCompanionFieldTaskValidationService validation)
{
    public async Task<FieldCompanionFieldWorkOrderDetailResponse> GetDetailAsync(
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

    public async Task<FieldCompanionFieldWorkOrderStatusResponse> UpdateStatusAsync(
        ClaimsPrincipal principal,
        string accessToken,
        UpdateFieldCompanionFieldWorkOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await EnsureWorkOrderTaskAsync(
            principal,
            accessToken,
            request.TaskKey,
            cancellationToken);

        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        FieldCompanionFieldWorkOrderStatusResponse response;
        try
        {
            var updated = await productClient.UpdateMaintainArrWorkOrderStatusAsync(
                accessToken,
                task.ResourceId,
                request.Status,
                cancellationToken);

            response = new FieldCompanionFieldWorkOrderStatusResponse(
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
                FieldCompanionFieldSubmissionKinds.WorkOrder,
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
                FieldCompanionFieldSubmissionKinds.WorkOrder,
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
            FieldCompanionFieldSubmissionKinds.WorkOrder,
            FieldCompanionFieldSubmissionStatuses.Synced,
            $"Work order status updated to {response.Status}.",
            DateTimeOffset.UtcNow,
            cancellationToken);

        return response;
    }

    public async Task<FieldCompanionFieldWorkOrderLaborResponse> LogLaborAsync(
        ClaimsPrincipal principal,
        string accessToken,
        LogFieldCompanionFieldWorkOrderLaborRequest request,
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

        FieldCompanionFieldWorkOrderLaborResponse response;
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

            response = new FieldCompanionFieldWorkOrderLaborResponse(
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
                FieldCompanionFieldSubmissionKinds.WorkOrder,
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
                FieldCompanionFieldSubmissionKinds.WorkOrder,
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
            FieldCompanionFieldSubmissionKinds.WorkOrder,
            FieldCompanionFieldSubmissionStatuses.Synced,
            $"Logged {response.HoursWorked:0.##} hour(s) of {response.LaborTypeKey} labor.",
            DateTimeOffset.UtcNow,
            cancellationToken);

        return response;
    }

    private async Task<FieldCompanionFieldTaskReference> EnsureWorkOrderTaskAsync(
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
            FieldCompanionFieldSubmissionKinds.WorkOrder,
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
            || !string.Equals(task.ResourceType, "work-order", StringComparison.Ordinal))
        {
            throw new StlApiException(
                FieldCompanionFieldValidationReasonCodes.WorkOrderUnsupported,
                FieldCompanionDeniedReasonCatalog.ToPlainMessage(FieldCompanionFieldValidationReasonCodes.WorkOrderUnsupported),
                409);
        }

        return task;
    }

    private static FieldCompanionFieldWorkOrderDetailResponse MapDetail(
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
                .Select(taskLine => new FieldCompanionFieldWorkOrderTaskLine(
                    taskLine.TaskLineId,
                    taskLine.Title,
                    taskLine.Description,
                    taskLine.SortOrder,
                    taskLine.Status,
                    taskLine.CompletedAt))
                .ToList(),
            labor
                .Select(entry => new FieldCompanionFieldWorkOrderLaborEntry(
                    entry.LaborEntryId,
                    entry.PersonId,
                    entry.HoursWorked,
                    entry.LaborTypeKey,
                    entry.Notes,
                    entry.LoggedAt))
                .ToList());
}
