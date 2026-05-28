using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Options;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Http;

namespace NexArr.Api.Services;

public sealed class CompanionProductClient(
    IHttpClientFactory httpClientFactory,
    IOptions<CompanionProductUrlsOptions> options)
{
    public async Task<FieldInboxProductSlice> FetchFieldInboxAsync(
        string productKey,
        bool entitled,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (!entitled)
        {
            return new FieldInboxProductSlice(
                productKey,
                Entitled: false,
                Fetched: false,
                ErrorCode: "not_entitled",
                ErrorMessage: $"Tenant is not entitled to {productKey}.",
                Items: []);
        }

        var baseUrl = ResolveBaseUrl(productKey);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return new FieldInboxProductSlice(
                productKey,
                Entitled: true,
                Fetched: false,
                ErrorCode: "product_url_missing",
                ErrorMessage: $"{productKey} API URL is not configured.",
                Items: []);
        }

        var client = httpClientFactory.CreateClient(nameof(CompanionProductClient));
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl.TrimEnd('/')}/api/field-inbox");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return new FieldInboxProductSlice(
                    productKey,
                    Entitled: true,
                    Fetched: false,
                    ErrorCode: $"upstream_{(int)response.StatusCode}",
                    ErrorMessage: string.IsNullOrWhiteSpace(body)
                        ? $"{productKey} field inbox request failed."
                        : body,
                    Items: []);
            }

            var inbox = await response.Content.ReadFromJsonAsync<FieldInboxResponse>(cancellationToken);
            return new FieldInboxProductSlice(
                productKey,
                Entitled: true,
                Fetched: true,
                ErrorCode: null,
                ErrorMessage: null,
                Items: inbox?.Items ?? []);
        }
        catch (HttpRequestException ex)
        {
            return new FieldInboxProductSlice(
                productKey,
                Entitled: true,
                Fetched: false,
                ErrorCode: "upstream_unreachable",
                ErrorMessage: ex.Message,
                Items: []);
        }
    }

    public async Task<CompanionFieldEvidenceResponse> SubmitTrainArrAssignmentEvidenceAsync(
        string accessToken,
        Guid assignmentId,
        string evidenceTypeKey,
        string fileName,
        string contentType,
        string contentBase64,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ResolveBaseUrl("trainarr");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "companion.field_evidence.product_url_missing",
                "TrainArr API URL is not configured for companion evidence capture.",
                503);
        }

        var client = httpClientFactory.CreateClient(nameof(CompanionProductClient));
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl.TrimEnd('/')}/api/training-assignments/{assignmentId:D}/evidence");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new TrainArrCreateEvidenceUpstreamRequest(
            evidenceTypeKey,
            fileName,
            contentType,
            contentBase64,
            notes));

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "companion.field_evidence.upstream_failed",
                string.IsNullOrWhiteSpace(body) ? "TrainArr evidence upload failed." : body,
                (int)response.StatusCode);
        }

        var created = await response.Content.ReadFromJsonAsync<TrainArrEvidenceUpstreamResponse>(cancellationToken)
            ?? throw new StlApiException(
                "companion.field_evidence.upstream_invalid",
                "TrainArr returned an empty evidence response.",
                502);

        return new CompanionFieldEvidenceResponse(
            $"trainarr:assignment:{assignmentId:D}",
            "trainarr",
            created.EvidenceId,
            created.EvidenceTypeKey,
            created.FileName,
            created.ContentType,
            created.SizeBytes,
            created.Notes,
            created.CreatedAt);
    }

    private string ResolveBaseUrl(string productKey)
    {
        var urls = options.Value;
        return productKey switch
        {
            "staffarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.StaffArrBaseUrl),
            "trainarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.TrainArrBaseUrl),
            "maintainarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.MaintainArrBaseUrl),
            "routarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.RoutArrBaseUrl),
            "supplyarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.SupplyArrBaseUrl),
            _ => string.Empty,
        };
    }

    private sealed record TrainArrCreateEvidenceUpstreamRequest(
        string EvidenceTypeKey,
        string FileName,
        string ContentType,
        string ContentBase64,
        string? Notes);

    private sealed record TrainArrEvidenceUpstreamResponse(
        Guid EvidenceId,
        Guid TrainingAssignmentId,
        string EvidenceTypeKey,
        string FileName,
        string ContentType,
        long SizeBytes,
        string? Notes,
        Guid UploadedByUserId,
        DateTimeOffset CreatedAt);
}
