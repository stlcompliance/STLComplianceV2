using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ReportArr.Api.Options;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Print;

namespace ReportArr.Api.Services;

public sealed class ReportArrRecordArchiveClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    IOptions<RecordArrClientOptions> options) : IRecordArchiveClient
{
    public async Task<StlRecordArchiveReceipt> ArchiveAsync(
        StlRecordArchiveRequest request,
        CancellationToken cancellationToken)
    {
        var bearerTokens = BuildBearerCandidates(httpContextAccessor.HttpContext, options.Value.ServiceToken);
        if (bearerTokens.Count == 0)
        {
            throw new StlApiException(
                "recordarr.archive_auth_missing",
                "ReportArr could not authenticate the RecordArr archive request.",
                500);
        }

        var createRecordRequest = new RecordArrCreateRecordRequest(
            Title: request.Title,
            Description: BuildDescription(request),
            RecordType: "generated_pdf",
            DocumentClass: request.DocumentClass,
            DocumentType: request.DocumentType,
            DocumentSubtype: request.DocumentSubtype,
            Classification: "internal",
            SourceProduct: request.SourceProductKey,
            SourceObjectType: request.SourceEntityType,
            SourceObjectId: request.SourceEntityId,
            SourceObjectDisplayName: request.SourceDisplayRef,
            OwnerPersonId: request.IssuedByPersonId.ToString("D"),
            UploadedByPersonId: request.IssuedByPersonId.ToString("D"),
            CurrentFileName: request.FileName,
            CurrentMimeType: "application/pdf",
            FileContentBase64: Convert.ToBase64String(request.Content));

        Exception? lastError = null;
        foreach (var bearerToken in bearerTokens)
        {
            try
            {
                var createdRecord = await CreateRecordAsync(createRecordRequest, bearerToken, cancellationToken);
                await WriteMetadataAsync(createdRecord.RecordId, request, bearerToken, cancellationToken);
                return new StlRecordArchiveReceipt(createdRecord.RecordId, request.FileName, request.ContentHash);
            }
            catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                lastError = ex;
            }
            catch (StlApiException ex) when (ex.StatusCode is 401 or 403)
            {
                lastError = ex;
            }
        }

        throw lastError as Exception ?? new StlApiException(
            "recordarr.archive_failed",
            "ReportArr could not archive the official PDF into RecordArr.",
            502);
    }

    private async Task<RecordArrCreateRecordResponse> CreateRecordAsync(
        RecordArrCreateRecordRequest request,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/v1/workspace/records");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        message.Content = JsonContent.Create(request);

        using var response = await httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "recordarr.archive_record_create_failed",
                $"RecordArr record create failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        var created = await response.Content.ReadFromJsonAsync<RecordArrCreateRecordResponse>(cancellationToken);
        if (created is null || string.IsNullOrWhiteSpace(created.RecordId))
        {
            throw new StlApiException(
                "recordarr.archive_record_create_failed",
                "RecordArr record creation returned an empty response.",
                502);
        }

        return created;
    }

    private async Task WriteMetadataAsync(
        string recordId,
        StlRecordArchiveRequest request,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        var metadataEntries = new[]
        {
            new RecordArrCreateMetadataRequest("content_hash", request.ContentHash, "string", "reportarr_print_archive", 1.0m, request.IssuedByPersonId.ToString("D")),
            new RecordArrCreateMetadataRequest("template_key", request.TemplateKey, "string", "reportarr_print_archive", 1.0m, request.IssuedByPersonId.ToString("D")),
            new RecordArrCreateMetadataRequest("template_version", request.TemplateVersion, "string", "reportarr_print_archive", 1.0m, request.IssuedByPersonId.ToString("D")),
            new RecordArrCreateMetadataRequest("issued_at_utc", request.IssuedAtUtc.UtcDateTime.ToString("O"), "datetime", "reportarr_print_archive", 1.0m, request.IssuedByPersonId.ToString("D")),
            new RecordArrCreateMetadataRequest("source_display_ref", request.SourceDisplayRef, "string", "reportarr_print_archive", 1.0m, request.IssuedByPersonId.ToString("D")),
            new RecordArrCreateMetadataRequest("retention_class", request.RetentionClass ?? "official_output", "string", "reportarr_print_archive", 1.0m, request.IssuedByPersonId.ToString("D")),
        };

        foreach (var entry in metadataEntries)
        {
            using var message = new HttpRequestMessage(
                HttpMethod.Post,
                $"api/v1/workspace/records/{Uri.EscapeDataString(recordId)}/metadata");
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            message.Content = JsonContent.Create(entry);

            using var response = await httpClient.SendAsync(message, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new StlApiException(
                    "recordarr.archive_metadata_failed",
                    $"RecordArr metadata write failed ({(int)response.StatusCode}): {body}",
                    (int)response.StatusCode);
            }
        }
    }

    private static IReadOnlyList<string> BuildBearerCandidates(HttpContext? httpContext, string configuredServiceToken)
    {
        var candidates = new List<string>();

        var forwardedBearer = ExtractBearer(httpContext?.Request.Headers.Authorization.ToString());
        if (!string.IsNullOrWhiteSpace(forwardedBearer))
        {
            candidates.Add(forwardedBearer);
        }

        if (!string.IsNullOrWhiteSpace(configuredServiceToken)
            && !candidates.Any(candidate => string.Equals(candidate, configuredServiceToken, StringComparison.Ordinal)))
        {
            candidates.Add(configuredServiceToken.Trim());
        }

        return candidates;
    }

    private static string? ExtractBearer(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return null;
        }

        const string prefix = "Bearer ";
        return authorizationHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? authorizationHeader[prefix.Length..].Trim()
            : authorizationHeader.Trim();
    }

    private static string BuildDescription(StlRecordArchiveRequest request) =>
        $"Official ReportArr archive generated from template {request.TemplateKey} v{request.TemplateVersion} for {request.SourceDisplayRef} on {request.IssuedAtUtc:O}.";

    private sealed record RecordArrCreateRecordRequest(
        string Title,
        string Description,
        string RecordType,
        string DocumentClass,
        string DocumentType,
        string DocumentSubtype,
        string Classification,
        string SourceProduct,
        string SourceObjectType,
        string SourceObjectId,
        string SourceObjectDisplayName,
        string OwnerPersonId,
        string UploadedByPersonId,
        string CurrentFileName,
        string CurrentMimeType,
        string FileContentBase64);

    private sealed record RecordArrCreateRecordResponse(
        string RecordId,
        string RecordNumber);

    private sealed record RecordArrCreateMetadataRequest(
        string Key,
        string Value,
        string ValueType,
        string Source,
        decimal ConfidenceScore,
        string CreatedByPersonId);
}
