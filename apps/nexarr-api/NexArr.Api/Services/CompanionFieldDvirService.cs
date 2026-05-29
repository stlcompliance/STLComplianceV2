using System.Security.Claims;
using NexArr.Api.Contracts;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class CompanionFieldDvirService(
    CompanionProductClient productClient,
    CompanionFieldSubmissionService submissions,
    CompanionFieldTaskValidationService validation)
{
    public async Task<CompanionFieldDvirResponse> SubmitAsync(
        ClaimsPrincipal principal,
        string accessToken,
        SubmitCompanionFieldDvirRequest request,
        CancellationToken cancellationToken = default)
    {
        CompanionFieldInboxService.RequireCompanionAccess(principal);
        await validation.EnsureAllowedAsync(
            principal,
            accessToken,
            request.TaskKey,
            CompanionFieldSubmissionKinds.Dvir,
            null,
            cancellationToken);

        if (!CompanionFieldTaskKeyParser.TryParse(request.TaskKey, out var task))
        {
            throw new StlApiException(
                CompanionFieldValidationReasonCodes.InvalidTaskKey,
                CompanionDeniedReasonCatalog.ToPlainMessage(CompanionFieldValidationReasonCodes.InvalidTaskKey),
                400);
        }

        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        CompanionFieldDvirResponse response;
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
                    CompanionFieldValidationReasonCodes.DvirUnsupported,
                    CompanionDeniedReasonCatalog.ToPlainMessage(CompanionFieldValidationReasonCodes.DvirUnsupported),
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
                CompanionFieldSubmissionKinds.Dvir,
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
                CompanionFieldSubmissionKinds.Dvir,
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
            CompanionFieldSubmissionKinds.Dvir,
            CompanionFieldSubmissionStatuses.Synced,
            $"Submitted {response.Phase.Replace('_', '-')} DVIR ({response.Result}).",
            DateTimeOffset.UtcNow,
            cancellationToken);

        return response;
    }
}
