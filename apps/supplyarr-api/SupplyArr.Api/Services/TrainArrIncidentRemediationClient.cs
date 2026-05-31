using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SupplyArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed record SupplyArrTrainArrIncidentPayload(
    Guid TenantId,
    string Summary,
    Guid SupplierIncidentId,
    string IncidentKey,
    string IncidentType,
    string Severity,
    string Status,
    Guid ExternalPartyId,
    string PartyDisplayName,
    Guid? PurchaseRequestId = null,
    Guid? PurchaseOrderId = null,
    Guid? ReceivingReceiptId = null,
    Guid? ReceivingExceptionId = null);

public sealed record TrainArrSupplyArrIncidentRemediationRequest(
    Guid TenantId,
    Guid SourceEventId,
    string EventKind,
    Guid SupplierIncidentId,
    Guid CorrelationId,
    Guid StaffarrPersonId,
    SupplyArrTrainArrIncidentPayload Payload,
    DateTimeOffset? OccurredAt = null);

public sealed record TrainArrSupplyArrIncidentRemediationResponse(
    Guid RemediationId,
    Guid TenantId,
    Guid SourceEventId,
    Guid StaffarrPersonId,
    string Status,
    bool IdempotentReplay);

public sealed class TrainArrIncidentRemediationClient(
    HttpClient httpClient,
    IOptions<TrainArrClientOptions> options)
{
    public async Task<TrainArrSupplyArrIncidentRemediationResponse> IngestAsync(
        TrainArrSupplyArrIncidentRemediationRequest payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "trainarr.incident_remediation_not_configured",
                "SupplyArr TrainArr incident remediation is not configured.",
                503);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/integrations/supplyarr-incident-remediations");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "trainarr.incident_remediation_ingest_failed",
                $"TrainArr incident remediation ingestion failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<TrainArrSupplyArrIncidentRemediationResponse>(cancellationToken)
            ?? throw new StlApiException(
                "trainarr.incident_remediation_empty_response",
                "TrainArr incident remediation ingestion returned an empty response.",
                502);
    }
}
