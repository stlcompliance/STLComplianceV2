using System.Security.Claims;
using NexArr.Api.Contracts;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class FieldCompanionFieldDvirService(
    FieldCompanionProductClient productClient,
    FieldCompanionFieldSubmissionService submissions,
    FieldCompanionFieldTaskValidationService validation)
{
    public async Task<FieldCompanionFieldDvirResponse> SubmitAsync(
        ClaimsPrincipal principal,
        string accessToken,
        SubmitFieldCompanionFieldDvirRequest request,
        CancellationToken cancellationToken = default)
    {
        FieldCompanionFieldInboxService.RequireFieldCompanionAccess(principal);
        await validation.EnsureAllowedAsync(
            principal,
            accessToken,
            request.TaskKey,
            FieldCompanionFieldSubmissionKinds.Dvir,
            null,
            cancellationToken);

        if (!FieldCompanionFieldTaskKeyParser.TryParse(request.TaskKey, out var task))
        {
            throw new StlApiException(
                FieldCompanionFieldValidationReasonCodes.InvalidTaskKey,
                FieldCompanionDeniedReasonCatalog.ToPlainMessage(FieldCompanionFieldValidationReasonCodes.InvalidTaskKey),
                400);
        }

        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        FieldCompanionFieldDvirResponse response;
        try
        {
            response = task.ProductKey switch
            {
                "routarr" when string.Equals(task.ResourceType, "trip", StringComparison.Ordinal) =>
                    await productClient.SubmitRoutArrTripDvirAsync(
                        accessToken,
                        task.ResourceId,
                        request.Phase,
                        request.Result,
                        request.OdometerReading,
                        request.DefectNotes,
                        request.VehicleRefKey,
                        cancellationToken),
                _ => throw new StlApiException(
                    FieldCompanionFieldValidationReasonCodes.DvirUnsupported,
                    FieldCompanionDeniedReasonCatalog.ToPlainMessage(FieldCompanionFieldValidationReasonCodes.DvirUnsupported),
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
                FieldCompanionFieldSubmissionKinds.Dvir,
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
                FieldCompanionFieldSubmissionKinds.Dvir,
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
            FieldCompanionFieldSubmissionKinds.Dvir,
            FieldCompanionFieldSubmissionStatuses.Synced,
            $"Submitted {response.Phase.Replace('_', '-')} DVIR ({response.Result}).",
            DateTimeOffset.UtcNow,
            cancellationToken);

        return response;
    }
}
