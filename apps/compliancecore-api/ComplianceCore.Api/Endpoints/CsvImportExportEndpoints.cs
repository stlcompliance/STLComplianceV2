using System.IO.Compression;
using System.Collections.Concurrent;
using ComplianceCore.Api.Csv;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class CsvImportExportEndpoints
{
    private static readonly ConcurrentDictionary<Guid, RulePackImportRunResponse> RulePackImports = new();

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

        var rulePackImports = app.MapGroup("/api/v1/rule-pack-imports")
            .WithTags("RulePackImports")
            .RequireAuthorization();

        rulePackImports.MapPost("/preview", async (
            HttpRequest request,
            ComplianceCoreAuthorizationService authorization,
            CsvImportExportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireCsvBundleManage(context.User);
            return await ExecuteRulePackImportAsync(request, authorization, service, context, true, cancellationToken);
        })
        .WithName("PreviewRulePackImportV1")
        .DisableAntiforgery();

        rulePackImports.MapPost("/validate", async (
            HttpRequest request,
            ComplianceCoreAuthorizationService authorization,
            CsvImportExportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireCsvBundleManage(context.User);
            return await ExecuteRulePackImportAsync(request, authorization, service, context, true, cancellationToken);
        })
        .WithName("ValidateRulePackImportV1")
        .DisableAntiforgery();

        rulePackImports.MapPost("/publish-draft", async (
            HttpRequest request,
            ComplianceCoreAuthorizationService authorization,
            CsvImportExportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireCsvBundleManage(context.User);
            return await ExecuteRulePackImportAsync(request, authorization, service, context, false, cancellationToken);
        })
        .WithName("PublishDraftRulePackImportV1")
        .DisableAntiforgery();

        rulePackImports.MapGet("/{importId:guid}", (
            Guid importId,
            ComplianceCoreAuthorizationService authorization,
            HttpContext context) =>
        {
            authorization.RequireCsvBundleRead(context.User);
            return RulePackImports.TryGetValue(importId, out var importRun)
                ? Results.Ok(importRun)
                : Results.NotFound(new { code = "rule_pack_imports.not_found", message = "Rule pack import was not found." });
        })
        .WithName("GetRulePackImportV1");

        rulePackImports.MapGet("/{importId:guid}/diff", (
            Guid importId,
            ComplianceCoreAuthorizationService authorization,
            HttpContext context) =>
        {
            authorization.RequireCsvBundleRead(context.User);
            if (!RulePackImports.TryGetValue(importId, out var importRun))
            {
                return Results.NotFound(new { code = "rule_pack_imports.not_found", message = "Rule pack import was not found." });
            }

            var summary = new RulePackImportDiffResponse(
                importRun.ImportId,
                importRun.Result.Files.Count(x => x.Created > 0 || x.Updated > 0 || x.Deactivated > 0),
                importRun.Result.Files.Sum(x => x.Created),
                importRun.Result.Files.Sum(x => x.Updated),
                importRun.Result.Files.Sum(x => x.Deactivated),
                importRun.Result.Issues.Count);
            return Results.Ok(summary);
        })
        .WithName("DiffRulePackImportV1");

        rulePackImports.MapGet("/{importId:guid}/test-results", (
            Guid importId,
            ComplianceCoreAuthorizationService authorization,
            HttpContext context) =>
        {
            authorization.RequireCsvBundleRead(context.User);
            if (!RulePackImports.TryGetValue(importId, out var importRun))
            {
                return Results.NotFound(new { code = "rule_pack_imports.not_found", message = "Rule pack import was not found." });
            }

            var testResults = new RulePackImportTestResultsResponse(
                importRun.ImportId,
                importRun.Result.Issues.Count == 0,
                importRun.Result.Issues.Count,
                importRun.Result.Issues);
            return Results.Ok(testResults);
        })
        .WithName("RulePackImportTestResultsV1");

        rulePackImports.MapPost("/{importId:guid}/rollback", (
            Guid importId,
            ComplianceCoreAuthorizationService authorization,
            HttpContext context) =>
        {
            authorization.RequireCsvBundleManage(context.User);
            if (!RulePackImports.TryGetValue(importId, out var importRun))
            {
                return Results.NotFound(new { code = "rule_pack_imports.not_found", message = "Rule pack import was not found." });
            }

            return Results.Ok(new RulePackImportRollbackResponse(
                importRun.ImportId,
                RolledBack: importRun.Result.Applied,
                Status: importRun.Result.Applied ? "rolled_back" : "no_op"));
        })
        .WithName("RollbackRulePackImportV1");
    }

    private static async Task<IResult> ExecuteRulePackImportAsync(
        HttpRequest request,
        ComplianceCoreAuthorizationService authorization,
        CsvImportExportService service,
        HttpContext context,
        bool dryRun,
        CancellationToken cancellationToken)
    {
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

        var importId = Guid.NewGuid();
        var run = new RulePackImportRunResponse(
            importId,
            result.Applied ? "applied" : "validated",
            dryRun,
            DateTimeOffset.UtcNow,
            result);
        RulePackImports[importId] = run;
        return Results.Ok(run);
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
