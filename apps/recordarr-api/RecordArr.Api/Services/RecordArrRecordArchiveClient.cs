using RecordArr.Api.Data;
using STLCompliance.Shared.Print;

namespace RecordArr.Api.Services;

public sealed class RecordArrRecordArchiveClient(
    RecordArrStore store,
    RecordArrDocumentStorageService storageService) : IRecordArchiveClient
{
    public async Task<StlRecordArchiveReceipt> ArchiveAsync(
        StlRecordArchiveRequest request,
        CancellationToken cancellationToken)
    {
        await using var content = new MemoryStream(request.Content, writable: false);
        var storageKey = await storageService.SaveAsync(
            request.TenantId,
            Guid.NewGuid(),
            request.ContentHash,
            request.FileName,
            content,
            cancellationToken);

        var record = store.CreateGeneratedPdfRecord(
            request.TenantId.ToString("D"),
            request.SourceProductKey,
            request.SourceEntityType,
            request.SourceEntityId,
            request.SourceDisplayRef,
            request.Title,
            $"Archived official print output for {request.SourceDisplayRef} using template {request.TemplateKey} v{request.TemplateVersion}.",
            request.DocumentClass,
            request.DocumentType,
            request.DocumentSubtype,
            "restricted",
            request.IssuedByPersonId.ToString("D"),
            request.IssuedByPersonId.ToString("D"),
            request.FileName,
            "recordarr",
            storageKey,
            request.Content.LongLength,
            request.ContentHash);

        return new StlRecordArchiveReceipt(
            record.RecordId,
            request.FileName,
            request.ContentHash);
    }
}
