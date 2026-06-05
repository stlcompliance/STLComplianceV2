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

        var detail = await productClient.GetSupplyArrReceivingReceiptAsync(
            accessToken,
            task.ResourceId,
            cancellationToken);

        return MapDetail(taskKey, detail);
    }

    public async Task<FieldCompanionFieldReceivingLineResponse> UpdateLineAsync(
        ClaimsPrincipal principal,
        string accessToken,
        UpdateFieldCompanionFieldReceivingLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await EnsureReceivingTaskAsync(
            principal,
            accessToken,
            request.TaskKey,
            cancellationToken);

        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        FieldCompanionFieldReceivingLineResponse response;
        try
        {
            var updated = await productClient.UpdateSupplyArrReceivingLineAsync(
                accessToken,
                task.ResourceId,
                request.LineId,
                request.QuantityReceived,
                cancellationToken);

            var line = updated.Lines.FirstOrDefault(x => x.LineId == request.LineId)
                ?? throw new StlApiException(
                    "fieldcompanion.field_receiving.line_missing",
                    "Receiving line was not found after update.",
                    502);

            response = new FieldCompanionFieldReceivingLineResponse(
                request.TaskKey.Trim(),
                task.ProductKey,
                task.ResourceId,
                request.LineId,
                line.QuantityReceived,
                updated.Status,
                updated.UpdatedAt);

            await submissions.RecordAsync(
                tenantId,
                userId,
                request.TaskKey.Trim(),
                task.ProductKey,
                FieldCompanionFieldSubmissionKinds.Receiving,
                FieldCompanionFieldSubmissionStatuses.Synced,
                $"Updated line {line.LineNumber} quantity to {line.QuantityReceived:0.##}.",
                DateTimeOffset.UtcNow,
                cancellationToken);

            return response;
        }
        catch (StlApiException ex)
        {
            await submissions.RecordAsync(
                tenantId,
                userId,
                request.TaskKey.Trim(),
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
                request.TaskKey.Trim(),
                task.ProductKey,
                FieldCompanionFieldSubmissionKinds.Receiving,
                FieldCompanionFieldSubmissionStatuses.Failed,
                ex.Message,
                DateTimeOffset.UtcNow,
                cancellationToken);
            throw;
        }

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
            var posted = await productClient.PostSupplyArrReceivingReceiptAsync(
                accessToken,
                task.ResourceId,
                cancellationToken);

            response = new FieldCompanionFieldReceivingPostResponse(
                request.TaskKey.Trim(),
                task.ProductKey,
                task.ResourceId,
                posted.Status,
                posted.PostedAt ?? posted.UpdatedAt);
        }
        catch (StlApiException ex)
        {
            await submissions.RecordAsync(
                tenantId,
                userId,
                request.TaskKey.Trim(),
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
                request.TaskKey.Trim(),
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
            request.TaskKey.Trim(),
            task.ProductKey,
            FieldCompanionFieldSubmissionKinds.Receiving,
            FieldCompanionFieldSubmissionStatuses.Synced,
            "Receiving receipt posted.",
            DateTimeOffset.UtcNow,
            cancellationToken);

        return response;
    }

    private async Task<FieldCompanionFieldTaskReference> EnsureReceivingTaskAsync(
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
            FieldCompanionFieldSubmissionKinds.Receiving,
            null,
            cancellationToken);

        if (!FieldCompanionFieldTaskKeyParser.TryParse(taskKey, out var task))
        {
            throw new StlApiException(
                FieldCompanionFieldValidationReasonCodes.InvalidTaskKey,
                FieldCompanionDeniedReasonCatalog.ToPlainMessage(FieldCompanionFieldValidationReasonCodes.InvalidTaskKey),
                400);
        }

        if (!string.Equals(task.ProductKey, "supplyarr", StringComparison.Ordinal)
            || !string.Equals(task.ResourceType, "receiving", StringComparison.Ordinal))
        {
            throw new StlApiException(
                FieldCompanionFieldValidationReasonCodes.ReceivingUnsupported,
                FieldCompanionDeniedReasonCatalog.ToPlainMessage(FieldCompanionFieldValidationReasonCodes.ReceivingUnsupported),
                409);
        }

        return task;
    }

    private static FieldCompanionFieldReceivingDetailResponse MapDetail(
        string taskKey,
        SupplyArrReceivingReceiptUpstreamResponse detail) =>
        new(
            taskKey,
            "supplyarr",
            detail.ReceivingReceiptId,
            detail.ReceiptKey,
            detail.Status,
            detail.PurchaseOrderKey,
            detail.BinKey,
            detail.BinName,
            detail.LocationName,
            detail.Notes,
            detail.Lines
                .Select(line => new FieldCompanionFieldReceivingLine(
                    line.LineId,
                    line.LineNumber,
                    line.PartKey,
                    line.PartDisplayName,
                    line.QuantityExpected,
                    line.QuantityReceived,
                    line.QuantityOrdered,
                    line.QuantityRemainingOnOrder,
                    line.Exceptions.Count(x =>
                        string.Equals(x.Status, "open", StringComparison.OrdinalIgnoreCase))))
                .ToList());
}
