using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Options;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Http;

namespace NexArr.Api.Services;

public sealed class RecordArrSmartImportClient(
    IHttpClientFactory httpClientFactory,
    IOptions<PlatformProductUrlsOptions> options,
    IConfiguration configuration)
{
    public const string HttpClientName = "RecordArrSmartImportClient";

    public async Task<RecordArrSmartImportRetainSourceResponse> RetainSourceAsync(
        RecordArrSmartImportRetainSourceRequest request,
        string? forwardedAuthorizationHeader,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = StlServiceUrl.NormalizeHttpBaseUrl(options.Value.RecordArrBaseUrl);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "smart_import.recordarr_url_missing",
                "RecordArr API URL is not configured for Smart Import source retention.",
                503);
        }

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl.TrimEnd('/')}/api/v1/integrations/smart-import/source-files");

        var serviceToken = configuration["RecordArr:ServiceToken"]
            ?? configuration["RecordArr__ServiceToken"]
            ?? configuration["SmartImport:RecordArrServiceToken"]
            ?? configuration["SmartImport__RecordArrServiceToken"];
        if (!string.IsNullOrWhiteSpace(serviceToken))
        {
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        }
        else if (!string.IsNullOrWhiteSpace(forwardedAuthorizationHeader)
            && AuthenticationHeaderValue.TryParse(forwardedAuthorizationHeader, out var forwarded))
        {
            httpRequest.Headers.Authorization = forwarded;
        }

        httpRequest.Content = JsonContent.Create(request);
        using var response = await client.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "smart_import.recordarr_retention_failed",
                string.IsNullOrWhiteSpace(body)
                    ? "RecordArr source retention failed."
                    : body,
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<RecordArrSmartImportRetainSourceResponse>(cancellationToken)
            ?? throw new StlApiException(
                "smart_import.recordarr_invalid_response",
                "RecordArr returned an invalid Smart Import source retention response.",
                502);
    }
}
