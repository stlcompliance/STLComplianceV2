using RecordArr.Api.Data;
using STLCompliance.Shared.Integration;

namespace RecordArr.Api.Endpoints;

public static class ReferenceIntegrationEndpoints
{
    private const string ProductKey = "recordarr";
    private const string RecordReferenceType = "record";

    public static void MapRecordArrReferenceIntegrationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/integrations")
            .WithTags("Integrations")
            .RequireAuthorization();

        group.MapGet("/reference-types", () =>
            Results.Ok(new[]
            {
                new ReferenceTypeDescriptor(
                    ProductKey,
                    RecordReferenceType,
                    "Record",
                    Description: "RecordArr-owned record reference."),
            }))
            .WithName("ListRecordArrReferenceTypes");

        group.MapPost("/references/search", (
            ReferenceSearchRequest request,
            HttpContext context,
            RecordArrStore store) =>
        {
            var referenceType = RequireReferenceType(request.ReferenceType);
            var limit = Math.Clamp(request.Limit <= 0 ? 25 : request.Limit, 1, 50);
            var records = store.GetRecords(context.User, request.Query)
                .Take(limit)
                .Select(ToSummary)
                .ToArray();

            return Results.Ok(new ReferenceSearchResponse(records));
        })
        .WithName("SearchRecordArrReferences");

        group.MapGet("/references/{referenceType}/{id}/summary", (
            string referenceType,
            string id,
            HttpContext context,
            RecordArrStore store) =>
        {
            RequireReferenceType(referenceType);
            var record = store.GetRecord(context.User, id);
            return record is null ? Results.NotFound() : Results.Ok(ToSummary(record));
        })
        .WithName("GetRecordArrReferenceSummary");

        group.MapGet("/references/{referenceType}/quick-create-schema", (string referenceType) =>
        {
            var normalizedReferenceType = RequireReferenceType(referenceType);
            return Results.Ok(new QuickCreateSchemaResponse(
                ProductKey,
                normalizedReferenceType,
                false,
                "RecordArr",
                DisabledReason: "Record references must be selected from existing RecordArr records."));
        })
        .WithName("GetRecordArrQuickCreateSchema");
    }

    private static string RequireReferenceType(string referenceType)
    {
        var normalized = referenceType.Trim();
        if (!string.Equals(normalized, RecordReferenceType, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unsupported RecordArr reference type '{referenceType}'.");
        }

        return RecordReferenceType;
    }

    private static ReferenceSummaryResponse ToSummary(RecordArr.Api.Models.RecordArrRecordResponse record) =>
        new(
            ProductKey,
            RecordReferenceType,
            record.RecordId,
            record.Title,
            $"{record.RecordNumber} · {record.RecordType} · {record.DocumentType}",
            record.Status,
            record.VersionNumber.ToString(System.Globalization.CultureInfo.InvariantCulture),
            $"/records/{record.RecordId}",
            new Dictionary<string, string>
            {
                ["recordNumber"] = record.RecordNumber,
                ["recordType"] = record.RecordType,
                ["documentType"] = record.DocumentType,
                ["classification"] = record.Classification,
                ["sourceProduct"] = record.SourceProduct,
                ["sourceObjectRef"] = $"{record.SourceProduct}:{record.SourceObjectType}:{record.SourceObjectId}",
            });
}
