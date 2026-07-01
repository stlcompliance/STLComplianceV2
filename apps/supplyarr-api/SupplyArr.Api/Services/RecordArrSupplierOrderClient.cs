using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SupplyArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed record RecordArrSupplierOrderRecordCreateRequest(
    string Title,
    string Description,
    string RecordType,
    string DocumentType,
    string Classification,
    string SourceProduct,
    string SourceObjectType,
    string SourceObjectId,
    string SourceObjectDisplayName,
    string OwnerPersonId,
    string UploadedByPersonId,
    string CurrentFileName,
    string CurrentMimeType);

public sealed record RecordArrSupplierOrderFileCreateRequest(
    string RecordId,
    string OriginalFilename,
    string MimeType,
    string UploadedByPersonId,
    string? StorageProvider,
    string? StorageKey,
    long? SizeBytes,
    int? PageCount,
    int? ImageWidth,
    int? ImageHeight,
    int? DurationSeconds);

public sealed record RecordArrSupplierOrderRecordResponse(
    string RecordId,
    string RecordNumber);

public sealed record RecordArrSupplierOrderFileResponse(
    string FileId);

public sealed record RegisteredSupplierOrderDocumentRecord(
    string RecordId,
    string RecordNumber,
    string FileId);

public sealed class RecordArrSupplierOrderClient(
    HttpClient httpClient,
    IOptions<RecordArrClientOptions> options)
{
    public async Task<RegisteredSupplierOrderDocumentRecord> RegisterDocumentAsync(
        RecordArrSupplierOrderRecordCreateRequest recordRequest,
        RecordArrSupplierOrderFileCreateRequest fileRequest,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "recordarr.service_token_missing",
                "SupplyArr RecordArr service token is not configured.",
                500);
        }

        using var createRecordMessage = new HttpRequestMessage(HttpMethod.Post, "api/v1/integrations/records");
        createRecordMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        createRecordMessage.Content = JsonContent.Create(recordRequest);

        using var createRecordResponse = await httpClient.SendAsync(createRecordMessage, cancellationToken);
        if (!createRecordResponse.IsSuccessStatusCode)
        {
            var body = await createRecordResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "recordarr.record_create_failed",
                $"RecordArr record create failed ({(int)createRecordResponse.StatusCode}): {body}",
                (int)createRecordResponse.StatusCode);
        }

        var record = (await createRecordResponse.Content
            .ReadFromJsonAsync<RecordArrSupplierOrderRecordResponse>(cancellationToken))!;

        using var createFileMessage = new HttpRequestMessage(HttpMethod.Post, "api/v1/integrations/files");
        createFileMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        createFileMessage.Content = JsonContent.Create(fileRequest with { RecordId = record.RecordId });

        using var createFileResponse = await httpClient.SendAsync(createFileMessage, cancellationToken);
        if (!createFileResponse.IsSuccessStatusCode)
        {
            var body = await createFileResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "recordarr.file_create_failed",
                $"RecordArr file create failed ({(int)createFileResponse.StatusCode}): {body}",
                (int)createFileResponse.StatusCode);
        }

        var file = (await createFileResponse.Content
            .ReadFromJsonAsync<RecordArrSupplierOrderFileResponse>(cancellationToken))!;

        return new RegisteredSupplierOrderDocumentRecord(record.RecordId, record.RecordNumber, file.FileId);
    }
}
