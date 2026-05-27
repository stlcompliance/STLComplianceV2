using System.IO.Compression;
using ComplianceCore.Api.Csv;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class CsvImportExportEndpoints
{
    public static void MapComplianceCoreCsvImportExportEndpoints(this WebApplication app)
    {
        var bundle = app.MapGroup("/api/csv-bundle")
            .WithTags("CsvBundle")
            .RequireAuthorization();

        bundle.MapGet("/manifest", (
            ComplianceCoreAuthorizationService authorization,
            CsvImportExportService service,
            HttpContext context) =>
        {
            authorization.RequireCsvBundleRead(context.User);
            return Results.Ok(service.GetManifest());
        })
        .WithName("GetCsvBundleManifest");

        bundle.MapGet("/export", async (
            ComplianceCoreAuthorizationService authorization,
            CsvImportExportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireCsvBundleRead(context.User);
            var tenantId = context.User.GetTenantId();
            var zipBytes = await service.ExportZipAsync(tenantId, cancellationToken);
            return Results.File(
                zipBytes,
                "application/zip",
                $"compliancecore-csv-bundle-{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
        })
        .WithName("ExportCsvBundleZip");

        bundle.MapGet("/files/{fileName}", async (
            string fileName,
            ComplianceCoreAuthorizationService authorization,
            CsvImportExportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireCsvBundleRead(context.User);
            if (!CsvBundleFiles.IsKnownFile(fileName))
            {
                return Results.NotFound();
            }

            var tenantId = context.User.GetTenantId();
            var csv = await service.ExportFileAsync(tenantId, fileName, cancellationToken);
            return Results.Text(csv, "text/csv");
        })
        .WithName("ExportCsvBundleFile");

        bundle.MapPost("/import", async (
            bool dryRun,
            HttpRequest request,
            ComplianceCoreAuthorizationService authorization,
            CsvImportExportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireCsvBundleManage(context.User);
            if (!request.HasFormContentType)
            {
                return Results.BadRequest(new { code = "csv_bundle.invalid_request", message = "Multipart form upload is required." });
            }

            var fileContents = await ReadUploadedCsvFilesAsync(request, cancellationToken);
            if (fileContents.Count == 0)
            {
                return Results.BadRequest(new { code = "csv_bundle.no_files", message = "Upload at least one CSV file or a zip bundle." });
            }

            var tenantId = context.User.GetTenantId();
            var result = await service.ImportAsync(
                tenantId,
                context.User.GetUserId(),
                fileContents,
                dryRun,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ImportCsvBundle")
        .DisableAntiforgery();
    }

    private static async Task<Dictionary<string, string>> ReadUploadedCsvFilesAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in request.Form.Files)
        {
            if (file.Length == 0)
            {
                continue;
            }

            if (string.Equals(Path.GetExtension(file.FileName), ".zip", StringComparison.OrdinalIgnoreCase))
            {
                await using var stream = file.OpenReadStream();
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
                foreach (var entry in archive.Entries)
                {
                    if (entry.Length == 0 || !CsvBundleFiles.IsKnownFile(entry.Name))
                    {
                        continue;
                    }

                    await using var entryStream = entry.Open();
                    using var reader = new StreamReader(entryStream);
                    files[entry.Name] = await reader.ReadToEndAsync(cancellationToken);
                }

                continue;
            }

            if (!CsvBundleFiles.IsKnownFile(file.FileName))
            {
                continue;
            }

            using var csvReader = new StreamReader(file.OpenReadStream());
            files[file.FileName] = await csvReader.ReadToEndAsync(cancellationToken);
        }

        return files;
    }
}
