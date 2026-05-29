using System.Security.Claims;
using NexArr.Api.Contracts;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class CompanionFieldReceivingService(
    CompanionProductClient productClient,
    CompanionFieldSubmissionService submissions,
    CompanionFieldTaskValidationService validation)
{
    public async Task<CompanionFieldReceivingDetailResponse> GetDetailAsync(
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

    public async Task<CompanionFieldReceivingLineResponse> UpdateLineAsync(
        ClaimsPrincipal principal,
        string accessToken,
        UpdateCompanionFieldReceivingLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await EnsureReceivingTaskAsync(
            principal,
            accessToken,
            request.TaskKey,
            cancellationToken);

        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        CompanionFieldReceivingLineResponse response;
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
                    "companion.field_receiving.line_missing",
                    "Receiving line was not found after update.",
                    502);

            response = new CompanionFieldReceivingLineResponse(
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
                CompanionFieldSubmissionKinds.Receiving,
                CompanionFieldSubmissionStatuses.Synced,
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
                CompanionFieldSubmissionKinds.Receiving,
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
                CompanionFieldSubmissionKinds.Receiving,
                CompanionFieldSubmissionStatuses.Failed,
                ex.Message,
                DateTimeOffset.UtcNow,
                cancellationToken);
            throw;
        }

    }

    public async Task<CompanionFieldReceivingPostResponse> PostAsync(
        ClaimsPrincipal principal,
        string accessToken,
        PostCompanionFieldReceivingRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await EnsureReceivingTaskAsync(
            principal,
            accessToken,
            request.TaskKey,
            cancellationToken);

        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        CompanionFieldReceivingPostResponse response;
        try
        {
            var posted = await productClient.PostSupplyArrReceivingReceiptAsync(
                accessToken,
                task.ResourceId,
                cancellationToken);

            response = new CompanionFieldReceivingPostResponse(
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
                CompanionFieldSubmissionKinds.Receiving,
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
                CompanionFieldSubmissionKinds.Receiving,
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
            CompanionFieldSubmissionKinds.Receiving,
            CompanionFieldSubmissionStatuses.Synced,
            "Receiving receipt posted.",
            DateTimeOffset.UtcNow,
            cancellationToken);

        return response;
    }

    private async Task<CompanionFieldTaskReference> EnsureReceivingTaskAsync(
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
            CompanionFieldSubmissionKinds.Receiving,
            null,
            cancellationToken);

        if (!CompanionFieldTaskKeyParser.TryParse(taskKey, out var task))
        {
            throw new StlApiException(
                CompanionFieldValidationReasonCodes.InvalidTaskKey,
                CompanionDeniedReasonCatalog.ToPlainMessage(CompanionFieldValidationReasonCodes.InvalidTaskKey),
                400);
        }

        if (!string.Equals(task.ProductKey, "supplyarr", StringComparison.Ordinal)
            || !string.Equals(task.ResourceType, "receiving", StringComparison.Ordinal))
        {
            throw new StlApiException(
                CompanionFieldValidationReasonCodes.ReceivingUnsupported,
                CompanionDeniedReasonCatalog.ToPlainMessage(CompanionFieldValidationReasonCodes.ReceivingUnsupported),
                409);
        }

        return task;
    }

    private static CompanionFieldReceivingDetailResponse MapDetail(
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
                .Select(line => new CompanionFieldReceivingLine(
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
