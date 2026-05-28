using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using StaffArr.Api.Contracts;
using StaffArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed record TrainArrPersonTrainingHistoryPayload(
    Guid StaffarrPersonId,
    int TotalCount,
    IReadOnlyList<TrainArrPersonTrainingHistoryEntryPayload> Items);

public sealed record TrainArrPersonTrainingHistoryEntryPayload(
    Guid EntryId,
    string EventKind,
    string Summary,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    DateTimeOffset OccurredAt);

public sealed class TrainArrPersonTrainingHistoryClient(
    HttpClient httpClient,
    IOptions<TrainArrClientOptions> options)
{
    public async Task<TrainArrPersonTrainingHistoryPayload> GetForPersonAsync(
        Guid tenantId,
        Guid staffarrPersonId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "trainarr.service_token_missing",
                "StaffArr TrainArr service token is not configured.",
                500);
        }

        var query = new List<string>
        {
            $"tenantId={tenantId:D}",
            $"staffarrPersonId={staffarrPersonId:D}",
        };
        if (limit.HasValue)
        {
            query.Add($"limit={limit.Value}");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/integrations/person-training-history?{string.Join('&', query)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "trainarr.training_history_read_failed",
                $"TrainArr person training history read failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        var raw = await response.Content.ReadFromJsonAsync<TrainArrIntegrationHistoryResponse>(cancellationToken);
        if (raw is null)
        {
            throw new StlApiException(
                "trainarr.training_history_read_failed",
                "TrainArr person training history read returned an empty response.",
                502);
        }

        return new TrainArrPersonTrainingHistoryPayload(
            raw.StaffarrPersonId,
            raw.TotalCount,
            raw.Items.Select(x => new TrainArrPersonTrainingHistoryEntryPayload(
                x.EntryId,
                x.EventKind,
                x.Summary,
                x.RelatedEntityType,
                x.RelatedEntityId,
                x.OccurredAt)).ToList());
    }

    private sealed record TrainArrIntegrationHistoryEntry(
        Guid EntryId,
        string EventKind,
        string Summary,
        string? RelatedEntityType,
        Guid? RelatedEntityId,
        DateTimeOffset OccurredAt);

    private sealed record TrainArrIntegrationHistoryResponse(
        Guid StaffarrPersonId,
        int TotalCount,
        IReadOnlyList<TrainArrIntegrationHistoryEntry> Items);
}
