using System.IO.Compression;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class StagedImportEndpoints
{
    public static void MapComplianceCoreStagedImportEndpoints(this WebApplication app)
    {
        var imports = app.MapGroup("/api/v1/import-sessions")
            .WithTags("ImportSessions")
            .RequireAuthorization();

        imports.MapPost("/", async (
            CreateImportSessionRequest request,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportCreate(context.User);
            var session = await service.CreateSessionAsync(
                context.User.GetTenantId(),
                context.User.GetPersonId(),
                request,
                cancellationToken);
            return Results.Ok(session);
        })
        .WithName("CreateImportSessionV1");

        imports.MapPost("/{id:guid}/upload", async (
            Guid id,
            HttpRequest request,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportCreate(context.User);
            if (!request.HasFormContentType)
            {
                return Results.BadRequest(new { code = "import_sessions.invalid_request", message = "Multipart form upload is required." });
            }

            var files = await ReadUploadedFilesAsync(request, cancellationToken);
            var response = await service.UploadAsync(
                context.User.GetTenantId(),
                id,
                context.User.GetPersonId(),
                files,
                cancellationToken);
            return Results.Ok(response);
        })
        .WithName("UploadImportSessionBundleV1")
        .DisableAntiforgery();

        imports.MapPost("/{id:guid}/parse", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportValidate(context.User);
            return Results.Ok(await service.ParseAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("ParseImportSessionV1");

        imports.MapPost("/{id:guid}/validate", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportValidate(context.User);
            return Results.Ok(await service.ValidateAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("ValidateImportSessionV1");

        imports.MapPost("/{id:guid}/generate-mapping-candidates", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.GenerateMappingCandidatesAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("GenerateImportMappingCandidatesV1");

        imports.MapGet("/{id:guid}", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportRead(context.User);
            return Results.Ok(await service.GetSessionAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("GetImportSessionV1");

        imports.MapGet("/{id:guid}/validation-results", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportRead(context.User);
            return Results.Ok(await service.GetValidationResultsAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("GetImportSessionValidationResultsV1");

        imports.MapGet("/{id:guid}/mapping-candidates", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportRead(context.User);
            return Results.Ok(await service.GetMappingCandidatesAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("GetImportSessionMappingCandidatesV1");

        imports.MapGet("/{id:guid}/commit-preview", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportRead(context.User);
            return Results.Ok(await service.GetCommitPreviewAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("GetImportSessionCommitPreviewV1");

        imports.MapPost("/{id:guid}/commit", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportCommit(context.User);
            return Results.Ok(await service.CommitAsync(
                context.User.GetTenantId(),
                id,
                context.User.GetPersonId(),
                cancellationToken));
        })
        .WithName("CommitImportSessionV1");

        imports.MapPost("/{id:guid}/reject", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportReject(context.User);
            return Results.Ok(await service.RejectSessionAsync(
                context.User.GetTenantId(),
                id,
                context.User.GetPersonId(),
                cancellationToken));
        })
        .WithName("RejectImportSessionV1");

        imports.MapGet("/{id:guid}/audit-log", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportRead(context.User);
            return Results.Ok(await service.GetAuditLogAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("GetImportSessionAuditLogV1");

        imports.MapGet("/{id:guid}/wizard/summary", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportRead(context.User);
            return Results.Ok(await service.GetWizardSummaryAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("GetImportWizardSummaryV1");

        imports.MapGet("/{id:guid}/wizard/next", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            var item = await service.GetNextWizardItemAsync(context.User.GetTenantId(), id, cancellationToken);
            return item is null ? Results.NoContent() : Results.Ok(item);
        })
        .WithName("GetNextImportWizardItemV1");

        imports.MapGet("/{id:guid}/wizard/items/{itemId:guid}", async (
            Guid id,
            Guid itemId,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportRead(context.User);
            return Results.Ok(await service.BuildWizardItemAsync(context.User.GetTenantId(), id, itemId, cancellationToken));
        })
        .WithName("GetImportWizardItemV1");

        imports.MapGet("/{id:guid}/wizard/items/{itemId:guid}/evidence-options", async (
            Guid id,
            Guid itemId,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportRead(context.User);
            return Results.Ok(await service.GetEvidenceOptionsAsync(context.User.GetTenantId(), id, itemId, cancellationToken));
        })
        .WithName("GetImportWizardItemEvidenceOptionsV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/confirm", async (
            Guid id,
            Guid itemId,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.ConfirmAsync(context.User.GetTenantId(), id, itemId, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("ConfirmImportWizardItemV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/select-evidence-option", async (
            Guid id,
            Guid itemId,
            SelectEvidenceOptionRequest request,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.SelectEvidenceOptionAsync(context.User.GetTenantId(), id, itemId, request, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("SelectImportWizardEvidenceOptionV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/select-target", async (
            Guid id,
            Guid itemId,
            SelectTargetRequest request,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.SelectTargetAsync(context.User.GetTenantId(), id, itemId, request, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("SelectImportWizardTargetV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/create-target", async (
            Guid id,
            Guid itemId,
            CreateTargetRequest request,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.CreateTargetAsync(context.User.GetTenantId(), id, itemId, request, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("CreateImportWizardTargetV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/mark-no-document-required", async (
            Guid id,
            Guid itemId,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.MarkNoDocumentRequiredAsync(context.User.GetTenantId(), id, itemId, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("MarkImportWizardNoDocumentRequiredV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/add-supporting-evidence", async (
            Guid id,
            Guid itemId,
            SupportingEvidenceRequest request,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.AddSupportingEvidenceAsync(context.User.GetTenantId(), id, itemId, request, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("AddImportWizardSupportingEvidenceV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/map-as-normal-evidence", async (
            Guid id,
            Guid itemId,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.MapAsNormalEvidenceAsync(context.User.GetTenantId(), id, itemId, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("MapImportWizardItemAsNormalEvidenceV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/map-as-exception-proof", async (
            Guid id,
            Guid itemId,
            ExceptionProofMappingRequest request,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.MapAsExceptionProofAsync(context.User.GetTenantId(), id, itemId, request, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("MapImportWizardItemAsExceptionProofV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/map-as-exemption-proof", async (
            Guid id,
            Guid itemId,
            ExceptionProofMappingRequest request,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.MapAsExemptionProofAsync(context.User.GetTenantId(), id, itemId, request, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("MapImportWizardItemAsExemptionProofV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/map-as-special-permit-approval-proof", async (
            Guid id,
            Guid itemId,
            ExceptionProofMappingRequest request,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.MapAsSpecialPermitApprovalProofAsync(context.User.GetTenantId(), id, itemId, request, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("MapImportWizardItemAsSpecialPermitApprovalProofV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/create-exception-exemption", async (
            Guid id,
            Guid itemId,
            ExceptionProofMappingRequest request,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.CreateExceptionExemptionAsync(context.User.GetTenantId(), id, itemId, request, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("CreateImportWizardExceptionExemptionV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/select-exception-exemption", async (
            Guid id,
            Guid itemId,
            ExceptionProofMappingRequest request,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.SelectExceptionExemptionAsync(context.User.GetTenantId(), id, itemId, request, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("SelectImportWizardExceptionExemptionV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/mark-exception-not-applicable", async (
            Guid id,
            Guid itemId,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.MarkExceptionNotApplicableAsync(context.User.GetTenantId(), id, itemId, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("MarkImportWizardExceptionNotApplicableV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/not-applicable", async (
            Guid id,
            Guid itemId,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.NotApplicableAsync(context.User.GetTenantId(), id, itemId, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("MarkImportWizardNotApplicableV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/reference-only", async (
            Guid id,
            Guid itemId,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.ReferenceOnlyAsync(context.User.GetTenantId(), id, itemId, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("MarkImportWizardReferenceOnlyV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/skip", async (
            Guid id,
            Guid itemId,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.SkipAsync(context.User.GetTenantId(), id, itemId, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("SkipImportWizardItemV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/reject", async (
            Guid id,
            Guid itemId,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.RejectAsync(context.User.GetTenantId(), id, itemId, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("RejectImportWizardItemV1");

        imports.MapPost("/{id:guid}/wizard/items/{itemId:guid}/force-map", async (
            Guid id,
            Guid itemId,
            ForceMapRequest request,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportOverride(context.User);
            return Results.Ok(await service.ForceMapAsync(context.User.GetTenantId(), id, itemId, request, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("ForceMapImportWizardItemV1");

        imports.MapPost("/{id:guid}/wizard/bulk-confirm", async (
            Guid id,
            BulkConfirmMappingsRequest request,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportMap(context.User);
            return Results.Ok(await service.BulkConfirmAsync(context.User.GetTenantId(), id, request, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("BulkConfirmImportWizardMappingsV1");

        imports.MapGet("/{id:guid}/wizard/commit-preview", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportRead(context.User);
            return Results.Ok(await service.GetCommitPreviewAsync(context.User.GetTenantId(), id, cancellationToken));
        })
        .WithName("GetImportWizardCommitPreviewV1");

        imports.MapPost("/{id:guid}/wizard/commit", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            StagedImportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireImportCommit(context.User);
            return Results.Ok(await service.CommitAsync(context.User.GetTenantId(), id, context.User.GetPersonId(), cancellationToken));
        })
        .WithName("CommitImportWizardV1");
    }

    private static async Task<IReadOnlyList<UploadedImportFile>> ReadUploadedFilesAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var files = new List<UploadedImportFile>();
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
                    var sourceFile = Path.GetFileName(entry.Name);
                    if (entry.Length == 0 || !StagedImportService.IsSupportedSourceFile(sourceFile))
                    {
                        continue;
                    }

                    await using var entryStream = entry.Open();
                    using var reader = new StreamReader(entryStream);
                    var content = await reader.ReadToEndAsync(cancellationToken);
                    files.Add(new UploadedImportFile(sourceFile, entry.FullName, content, entry.Length));
                }

                continue;
            }

            var fileName = Path.GetFileName(file.FileName);
            if (!StagedImportService.IsSupportedSourceFile(fileName))
            {
                continue;
            }

            using var csvReader = new StreamReader(file.OpenReadStream());
            files.Add(new UploadedImportFile(
                fileName,
                file.FileName,
                await csvReader.ReadToEndAsync(cancellationToken),
                file.Length));
        }

        return files;
    }
}
