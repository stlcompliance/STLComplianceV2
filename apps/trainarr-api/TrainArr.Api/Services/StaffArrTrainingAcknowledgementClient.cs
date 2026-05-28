using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TrainArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed record StaffArrIngestTrainingAcknowledgementPayload(
    Guid TenantId,
    Guid PersonId,
    Guid TrainarrAcknowledgementRequestId,
    Guid TrainarrAssignmentId,
    string TrainingTitle,
    string AssignmentReason,
    string Summary,
    DateTimeOffset? DueAt);

public sealed record StaffArrSupersedeTrainingAcknowledgementPayload(
    Guid TenantId,
    Guid PersonId,
    Guid TrainarrAcknowledgementRequestId);

public sealed record StaffArrTrainingAcknowledgementStatusPayload(
    Guid TrainarrAcknowledgementRequestId,
    Guid TrainarrAssignmentId,
    Guid PersonId,
    string Status,
    DateTimeOffset? AcknowledgedAt,
    Guid? AcknowledgedByUserId);

public sealed class StaffArrTrainingAcknowledgementClient(
    HttpClient httpClient,
    IOptions<StaffArrClientOptions> options)
{
    public async Task IngestAsync(
        StaffArrIngestTrainingAcknowledgementPayload payload,
        CancellationToken cancellationToken = default)
    {
        await PostAsync("api/integrations/training-acknowledgements", payload, cancellationToken);
    }

    public async Task SupersedeAsync(
        StaffArrSupersedeTrainingAcknowledgementPayload payload,
        CancellationToken cancellationToken = default)
    {
        await PostAsync("api/integrations/training-acknowledgements/supersede", payload, cancellationToken);
    }

    public async Task<StaffArrTrainingAcknowledgementStatusPayload?> GetStatusAsync(
        Guid tenantId,
        Guid trainarrAcknowledgementRequestId,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = RequireServiceToken();
        var url =
            $"api/integrations/training-acknowledgements/status?tenantId={tenantId:D}&trainarrAcknowledgementRequestId={trainarrAcknowledgementRequestId:D}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "staffarr.acknowledgement_status_failed",
                $"StaffArr acknowledgement status read failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<StaffArrTrainingAcknowledgementStatusPayload>(cancellationToken);
    }

    private async Task PostAsync<T>(string path, T payload, CancellationToken cancellationToken)
    {
        var serviceToken = RequireServiceToken();
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "staffarr.acknowledgement_publication_failed",
                $"StaffArr training acknowledgement call failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }
    }

    private string RequireServiceToken()
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "staffarr.service_token_missing",
                "TrainArr StaffArr service token is not configured.",
                500);
        }

        return serviceToken;
    }
}
