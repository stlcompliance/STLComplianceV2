using RecordArr.Api.Data;

namespace RecordArr.Api.Endpoints;

public static class WorkspaceEndpoints
{
    public static void MapRecordArrWorkspaceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/workspace").WithTags("Workspace").RequireAuthorization();

        group.MapGet("/summary", (RecordArrStore store) => Results.Ok(store.GetDashboard()))
            .WithName("GetRecordArrWorkspaceSummary");

        group.MapGet("/records", (string? search, RecordArrStore store) => Results.Ok(store.GetRecords(search)))
            .WithName("ListRecordArrRecords");

        group.MapGet("/records/{recordId}", (string recordId, RecordArrStore store) =>
        {
            var record = store.GetRecord(recordId);
            return record is null ? Results.NotFound() : Results.Ok(record);
        }).WithName("GetRecordArrRecord");

        group.MapPost("/records", (CreateRecordRequest request, RecordArrStore store) =>
        {
            if (string.IsNullOrWhiteSpace(request.SourceProduct))
            {
                return Results.BadRequest(new { code = "missing_source_product", message = "Record creation requires a source product." });
            }

            var record = store.CreateRecord(
                request.Title,
                request.Description,
                request.RecordType,
                request.DocumentType,
                request.SourceProduct,
                request.SourceObjectType,
                request.SourceObjectId,
                request.SourceObjectDisplayName,
                request.OwnerPersonId,
                request.UploadedByPersonId,
                request.CurrentFileName,
                request.CurrentMimeType);
            return Results.Created($"/api/v1/workspace/records/{record.RecordId}", record);
        }).WithName("CreateRecordArrRecord");

        group.MapPatch("/records/{recordId}", (string recordId, UpdateRecordRequest request, RecordArrStore store) =>
        {
            var updated = store.UpdateRecordStatus(recordId, request.Status, request.Classification, request.EffectiveAt, request.ExpiresAt);
            return Results.Ok(updated);
        }).WithName("UpdateRecordArrRecord");

        group.MapPost("/records/{recordId}/archive", (string recordId, DisposeRecordRequest request, RecordArrStore store) =>
        {
            var updated = store.ArchiveRecord(recordId, request.ActorPersonId);
            return Results.Ok(updated);
        }).WithName("ArchiveRecordArrRecord");

        group.MapPost("/records/{recordId}/purge", (string recordId, DisposeRecordRequest request, RecordArrStore store) =>
        {
            var updated = store.PurgeRecord(recordId, request.ActorPersonId);
            return Results.Ok(updated);
        }).WithName("PurgeRecordArrRecord");

        group.MapPost("/upload-sessions", (CreateUploadSessionRequest request, RecordArrStore store) =>
        {
            var session = store.CreateUploadSession(
                request.SourceProduct,
                request.SourceObjectType,
                request.SourceObjectId,
                request.UploadPurpose,
                request.RequiresDocumentScan,
                request.RequiresOcr,
                request.RequiresManualReview);
            return Results.Created($"/api/v1/workspace/upload-sessions/{session.UploadSessionId}", session);
        }).WithName("CreateRecordArrUploadSession");

        group.MapGet("/upload-sessions", (RecordArrStore store) => Results.Ok(store.GetUploadSessions()))
            .WithName("ListRecordArrUploadSessions");

        group.MapGet("/upload-sessions/{uploadSessionId}", (string uploadSessionId, RecordArrStore store) =>
        {
            var session = store.GetUploadSession(uploadSessionId);
            return session is null ? Results.NotFound() : Results.Ok(session);
        }).WithName("GetRecordArrUploadSession");

        group.MapPost("/upload-sessions/{uploadSessionId}/complete", (string uploadSessionId, CompleteUploadSessionRequest request, RecordArrStore store) =>
        {
            var session = store.CompleteUploadSession(uploadSessionId, request.RecordId);
            return Results.Ok(session);
        }).WithName("CompleteRecordArrUploadSession");

        group.MapPost("/upload-sessions/{uploadSessionId}/revoke", (string uploadSessionId, RevokeUploadSessionRequest request, RecordArrStore store) =>
        {
            var session = store.RevokeUploadSession(uploadSessionId, request.Reason);
            return Results.Ok(session);
        }).WithName("RevokeRecordArrUploadSession");

        group.MapPost("/document-scans", (CreateDocumentScanRequest request, RecordArrStore store) =>
        {
            var scan = store.CreateScanProcessing(request.RecordId, request.OriginalFileName, request.ScanPurpose);
            return Results.Created($"/api/v1/workspace/document-scans/{scan.ScanProcessingId}", scan);
        }).WithName("CreateRecordArrDocumentScan");

        group.MapGet("/document-scans", (RecordArrStore store) => Results.Ok(store.GetScanProcessing()))
            .WithName("ListRecordArrDocumentScans");

        group.MapGet("/document-scans/{scanProcessingId}", (string scanProcessingId, RecordArrStore store) =>
        {
            var scan = store.GetScanProcessing(scanProcessingId);
            return scan is null ? Results.NotFound() : Results.Ok(scan);
        }).WithName("GetRecordArrDocumentScan");

        group.MapPost("/document-scans/{scanProcessingId}/manual-correction", (string scanProcessingId, ManualCorrectionRequest request, RecordArrStore store) =>
        {
            var scan = store.ApplyManualCorrection(scanProcessingId, request.EdgeCoordinates);
            return Results.Ok(scan);
        }).WithName("ApplyRecordArrManualCorrection");

        group.MapGet("/ocr-results/{ocrResultId}", (string ocrResultId, RecordArrStore store) =>
        {
            var result = store.GetOcrResult(ocrResultId);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).WithName("GetRecordArrOcrResult");

        group.MapGet("/extraction-results/{extractionResultId}", (string extractionResultId, RecordArrStore store) =>
        {
            var result = store.GetExtractionResult(extractionResultId);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).WithName("GetRecordArrExtractionResult");

        group.MapPost("/extraction-results/{extractionResultId}/review", (string extractionResultId, ReviewExtractionResultRequest request, RecordArrStore store) =>
        {
            var result = store.ReviewExtractionResult(extractionResultId, request.ReviewedByPersonId, request.Status, request.FailureReason);
            return Results.Ok(result);
        }).WithName("ReviewRecordArrExtractionResult");

        group.MapGet("/evidence-mappings", (RecordArrStore store) => Results.Ok(store.GetEvidenceMappings()))
            .WithName("ListRecordArrEvidenceMappings");

        group.MapPost("/evidence-mappings", (CreateEvidenceMappingRequest request, RecordArrStore store) =>
        {
            var mapping = store.CreateEvidenceMapping(
                request.RecordId,
                request.SourceProduct,
                request.SourceObjectType,
                request.SourceObjectId,
                request.ComplianceRequirementRef,
                request.EvidenceTypeKey,
                request.MappingSource,
                request.ConfidenceScore);
            return Results.Created($"/api/v1/workspace/evidence-mappings/{mapping.EvidenceMappingId}", mapping);
        }).WithName("CreateRecordArrEvidenceMapping");

        group.MapPost("/evidence-mappings/{mappingId}/confirm", (string mappingId, ConfirmEvidenceMappingRequest request, RecordArrStore store) =>
        {
            var mapping = store.UpdateEvidenceMapping(mappingId, "confirmed", request.ConfirmedByPersonId, request.Notes, null);
            return Results.Ok(mapping);
        }).WithName("ConfirmRecordArrEvidenceMapping");

        group.MapPost("/evidence-mappings/{mappingId}/reject", (string mappingId, RejectEvidenceMappingRequest request, RecordArrStore store) =>
        {
            var mapping = store.UpdateEvidenceMapping(mappingId, "rejected", request.RejectedByPersonId, request.Notes, request.RejectionReason);
            return Results.Ok(mapping);
        }).WithName("RejectRecordArrEvidenceMapping");

        group.MapPost("/record-packages", (CreatePackageRequest request, RecordArrStore store) =>
        {
            var package = store.CreatePackage(request.Title, request.PackageType, request.SourceProduct, request.SourceObjectRef, request.RecordRef);
            return Results.Created($"/api/v1/workspace/record-packages/{package.PackageId}", package);
        }).WithName("CreateRecordArrPackage");

        group.MapGet("/record-packages/{packageId}", (string packageId, RecordArrStore store) =>
        {
            var package = store.GetPackage(packageId);
            return package is null ? Results.NotFound() : Results.Ok(package);
        }).WithName("GetRecordArrPackage");

        group.MapPost("/record-packages/{packageId}/lock", (string packageId, RecordArrStore store) =>
        {
            var package = store.LockPackage(packageId);
            return Results.Ok(package);
        }).WithName("LockRecordArrPackage");

        group.MapPost("/record-packages/{packageId}/archive", (string packageId, RecordArrStore store) =>
        {
            var package = store.ArchivePackage(packageId);
            return Results.Ok(package);
        }).WithName("ArchiveRecordArrPackage");

        group.MapGet("/record-packages/{packageId}/manifest", (string packageId, RecordArrStore store) =>
        {
            var manifest = store.GetManifest(packageId);
            return manifest is null ? Results.NotFound() : Results.Ok(manifest);
        }).WithName("GetRecordArrPackageManifest");

        group.MapGet("/record-packages/{packageId}/download", (string packageId, RecordArrStore store) =>
        {
            var package = store.GetPackage(packageId);
            if (package is null)
            {
                return Results.NotFound();
            }

            var manifest = store.GetManifest(packageId);
            var lines = new List<string>
            {
                $"Package: {package.PackageNumber}",
                $"Title: {package.Title}",
                $"Type: {package.PackageType}",
                $"Status: {package.Status}",
                $"Source product: {package.SourceProduct}",
                $"Source objects: {string.Join(", ", package.SourceObjectRefs)}",
                $"Record refs: {string.Join(", ", package.RecordRefs)}",
                $"Created at: {package.CreatedAt:O}",
                $"Completed at: {package.CompletedAt:O}",
                $"Locked at: {package.LockedAt:O}",
                $"Archived at: {package.ArchivedAt:O}",
                $"Expires at: {package.ExpiresAt:O}",
            };

            if (manifest is not null)
            {
                lines.Add(string.Empty);
                lines.Add($"Manifest: {manifest.ManifestId}");
                lines.Add($"Manifest version: {manifest.ManifestVersion}");
                lines.Add($"Checksum: {manifest.Checksum}");
                lines.Add($"Generated at: {manifest.GeneratedAt:O}");
                lines.Add($"Generated by: {manifest.GeneratedByPersonId}");
                lines.Add(string.Empty);
                lines.Add("Records:");
                lines.AddRange(manifest.RecordEntries.Select(entry => $"- {entry.DisplayName} [{entry.EntryType}] :: {entry.StatusSnapshot ?? "n/a"}"));
                lines.Add("Source objects:");
                lines.AddRange(manifest.SourceObjectEntries.Select(entry => $"- {entry.DisplayName} [{entry.EntryType}] :: {entry.StatusSnapshot ?? "n/a"}"));
                lines.Add("Requirements:");
                lines.AddRange(manifest.RequirementEntries.Select(entry => $"- {entry.DisplayName} [{entry.EntryType}] :: {entry.StatusSnapshot ?? "n/a"}"));
            }

            return Results.Text(string.Join(Environment.NewLine, lines), "text/plain");
        }).WithName("DownloadRecordArrPackage");

        group.MapGet("/controlled-documents/{controlledDocumentId}/versions", (string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentVersions(controlledDocumentId)))
            .WithName("ListRecordArrControlledDocumentVersions");

        group.MapPost("/controlled-documents/refresh-workflows", (RecordArrStore store) =>
            Results.Ok(store.RefreshControlledDocumentWorkflows()))
            .WithName("RefreshRecordArrControlledDocumentWorkflows");

        group.MapPost("/controlled-documents/{controlledDocumentId}/versions/{versionId}/promote", (string controlledDocumentId, string versionId, PromoteControlledDocumentVersionRequest request, RecordArrStore store) =>
        {
            var version = store.PromoteDocumentVersion(controlledDocumentId, versionId, request.ApprovedByPersonId, request.EffectiveAt);
            return Results.Ok(version);
        }).WithName("PromoteRecordArrControlledDocumentVersion");

        group.MapPost("/controlled-documents/{controlledDocumentId}/archive", (string controlledDocumentId, UpdateControlledDocumentStatusRequest request, RecordArrStore store) =>
        {
            var document = store.UpdateControlledDocumentStatus(controlledDocumentId, "archived", request.UpdatedByPersonId);
            return Results.Ok(document);
        }).WithName("ArchiveRecordArrControlledDocument");

        group.MapPost("/controlled-documents/{controlledDocumentId}/obsolete", (string controlledDocumentId, UpdateControlledDocumentStatusRequest request, RecordArrStore store) =>
        {
            var document = store.UpdateControlledDocumentStatus(controlledDocumentId, "obsolete", request.UpdatedByPersonId);
            return Results.Ok(document);
        }).WithName("ObsoleteRecordArrControlledDocument");

        group.MapPost("/controlled-documents/{controlledDocumentId}/supersede", (string controlledDocumentId, SupersedeControlledDocumentRequest request, RecordArrStore store) =>
        {
            var document = store.SupersedeControlledDocument(controlledDocumentId, request.SupersededByDocumentRef, request.SupersededByPersonId);
            return Results.Ok(document);
        }).WithName("SupersedeRecordArrControlledDocument");

        group.MapGet("/controlled-documents/{controlledDocumentId}/reviews", (string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentReviews(controlledDocumentId)))
            .WithName("ListRecordArrControlledDocumentReviews");

        group.MapGet("/controlled-documents/{controlledDocumentId}/distributions", (string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentDistributions(controlledDocumentId)))
            .WithName("ListRecordArrControlledDocumentDistributions");

        group.MapGet("/controlled-documents/{controlledDocumentId}/acknowledgements", (string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentAcknowledgements(controlledDocumentId)))
            .WithName("ListRecordArrControlledDocumentAcknowledgements");

        group.MapPost("/controlled-documents/{controlledDocumentId}/distributions", (string controlledDocumentId, CreateDocumentDistributionRequest request, RecordArrStore store) =>
        {
            var distribution = store.CreateDocumentDistribution(controlledDocumentId, request.VersionId, request.DistributionType, request.TargetRef);
            return Results.Created($"/api/v1/workspace/controlled-documents/{controlledDocumentId}/distributions/{distribution.DistributionId}", distribution);
        }).WithName("CreateRecordArrControlledDocumentDistribution");

        group.MapPost("/controlled-documents/{controlledDocumentId}/distributions/{distributionId}/revoke", (string controlledDocumentId, string distributionId, RevokeDocumentDistributionRequest request, RecordArrStore store) =>
        {
            var distribution = store.RevokeDocumentDistribution(distributionId, request.RevokedByPersonId, request.RevokeReason);
            return Results.Ok(distribution);
        }).WithName("RevokeRecordArrControlledDocumentDistribution");

        group.MapPost("/controlled-documents/{controlledDocumentId}/distributions/{distributionId}/expire", (string controlledDocumentId, string distributionId, ExpireDocumentDistributionRequest request, RecordArrStore store) =>
        {
            var distribution = store.ExpireDocumentDistribution(distributionId, request.ExpiredByPersonId, request.ExpireReason);
            return Results.Ok(distribution);
        }).WithName("ExpireRecordArrControlledDocumentDistribution");

        group.MapPost("/controlled-documents/{controlledDocumentId}/acknowledgements", (string controlledDocumentId, CreateDocumentAcknowledgementRequest request, RecordArrStore store) =>
        {
            var acknowledgement = store.CreateDocumentAcknowledgement(controlledDocumentId, request.VersionId, request.PersonId, request.AttestationText, request.DueAt);
            return Results.Created($"/api/v1/workspace/controlled-documents/{controlledDocumentId}/acknowledgements/{acknowledgement.AcknowledgementId}", acknowledgement);
        }).WithName("CreateRecordArrControlledDocumentAcknowledgement");

        group.MapPost("/controlled-documents/{controlledDocumentId}/acknowledgements/{acknowledgementId}/complete", (string controlledDocumentId, string acknowledgementId, CompleteDocumentAcknowledgementRequest request, RecordArrStore store) =>
        {
            var acknowledgement = store.CompleteDocumentAcknowledgement(acknowledgementId, request.SignatureRecordRef);
            return Results.Ok(acknowledgement);
        }).WithName("CompleteRecordArrControlledDocumentAcknowledgement");

        group.MapGet("/access-policies", (RecordArrStore store) => Results.Ok(store.GetAccessPolicies()))
            .WithName("ListRecordArrAccessPolicies");

        group.MapPost("/access-policies", (CreateAccessPolicyRequest request, RecordArrStore store) =>
        {
            var policy = store.CreateAccessPolicy(
                request.RecordId,
                request.PolicyType,
                request.Status,
                request.ReadRules,
                request.WriteRules,
                request.DownloadRules,
                request.ShareRules,
                request.ExportRules,
                request.PurgeRules,
                request.CreatedByPersonId);
            return Results.Created($"/api/v1/workspace/access-policies/{policy.AccessPolicyId}", policy);
        }).WithName("CreateRecordArrAccessPolicy");

        group.MapPost("/access-policies/{accessPolicyId}/update", (string accessPolicyId, UpdateAccessPolicyRequest request, RecordArrStore store) =>
        {
            var policy = store.UpdateAccessPolicy(
                accessPolicyId,
                request.RecordId,
                request.PolicyType,
                request.Status,
                request.ReadRules,
                request.WriteRules,
                request.DownloadRules,
                request.ShareRules,
                request.ExportRules,
                request.PurgeRules,
                request.UpdatedByPersonId);
            return Results.Ok(policy);
        }).WithName("UpdateRecordArrAccessPolicy");

        group.MapGet("/access-grants", (RecordArrStore store) => Results.Ok(store.GetAccessGrants()))
            .WithName("ListRecordArrAccessGrants");

        group.MapPost("/access-grants", (CreateAccessGrantRequest request, RecordArrStore store) =>
        {
            var grant = store.CreateAccessGrant(request.RecordId, request.GranteeType, request.GranteeRef, request.Permission, request.GrantedByPersonId, request.ExpiresAt);
            return Results.Created($"/api/v1/workspace/access-grants/{grant.AccessGrantId}", grant);
        }).WithName("CreateRecordArrAccessGrant");

        group.MapPost("/access-grants/{accessGrantId}/revoke", (string accessGrantId, RevokeAccessGrantRequest request, RecordArrStore store) =>
        {
            var grant = store.RevokeAccessGrant(accessGrantId, request.RevokedByPersonId, request.RevokeReason);
            return Results.Ok(grant);
        }).WithName("RevokeRecordArrAccessGrant");

        group.MapGet("/external-shares", (RecordArrStore store) => Results.Ok(store.GetExternalShares()))
            .WithName("ListRecordArrExternalShares");

        group.MapPost("/external-shares/{externalShareId}/access", (string externalShareId, RecordExternalShareAccessRequest request, RecordArrStore store) =>
        {
            var share = store.RecordExternalShareAccess(externalShareId, request.AccessedByPersonId, request.AccessAction, request.SourceIp, request.UserAgent);
            return Results.Ok(share);
        }).WithName("AccessRecordArrExternalShare");

        group.MapPost("/external-shares/{externalShareId}/expire", (string externalShareId, ExpireExternalShareRequest request, RecordArrStore store) =>
        {
            var share = store.ExpireExternalShare(externalShareId, request.ExpiredByPersonId);
            return Results.Ok(share);
        }).WithName("ExpireRecordArrExternalShare");

        group.MapGet("/redactions", (RecordArrStore store) => Results.Ok(store.GetRedactions()))
            .WithName("ListRecordArrRedactions");

        group.MapGet("/disposal-reviews", (RecordArrStore store) => Results.Ok(store.GetDisposalReviews()))
            .WithName("ListRecordArrDisposalReviews");

        group.MapPost("/disposal-reviews", (CreateDisposalReviewRequest request, RecordArrStore store) =>
        {
            var review = store.CreateDisposalReview(request.RecordId, request.RetentionStatusRef, request.ProposedAction, request.RequestedByPersonId);
            return Results.Created($"/api/v1/workspace/disposal-reviews/{review.DisposalReviewId}", review);
        }).WithName("CreateRecordArrDisposalReview");

        group.MapPost("/disposal-reviews/{disposalReviewId}/complete", (string disposalReviewId, CompleteDisposalReviewRequest request, RecordArrStore store) =>
        {
            var review = store.CompleteDisposalReview(disposalReviewId, request.Status, request.ReviewedByPersonId, request.DecisionReason);
            return Results.Ok(review);
        }).WithName("CompleteRecordArrDisposalReview");

        group.MapGet("/retention-policies", (RecordArrStore store) => Results.Ok(store.GetRetentionPolicies()))
            .WithName("ListRecordArrRetentionPolicies");

        group.MapGet("/records/{recordId}/retention-status", (string recordId, RecordArrStore store) =>
        {
            var status = store.GetRetentionStatus(recordId);
            return status is null ? Results.NotFound() : Results.Ok(status);
        }).WithName("GetRecordArrRetentionStatus");

        group.MapPost("/retention-statuses/recalculate", (RecordArrStore store) =>
        {
            return Results.Ok(store.RecalculateRetentionStatuses());
        }).WithName("RecalculateRecordArrRetentionStatuses");

        group.MapPost("/legal-holds", (CreateLegalHoldRequest request, RecordArrStore store) =>
        {
            var hold = store.CreateLegalHold(
                request.Title,
                request.Description,
                request.HoldType,
                request.SourceProduct,
                request.SourceObjectType,
                request.SourceObjectId,
                request.CreatedByPersonId,
                request.ScopeRules,
                request.RecordRefs);
            return Results.Created($"/api/v1/workspace/legal-holds/{hold.LegalHoldId}", hold);
        }).WithName("CreateRecordArrLegalHold");

        group.MapGet("/legal-holds", (RecordArrStore store) => Results.Ok(store.GetLegalHolds()))
            .WithName("ListRecordArrLegalHolds");

        group.MapPost("/legal-holds/{legalHoldId}/activate", (string legalHoldId, RecordArrStore store) =>
        {
            var hold = store.ActivateLegalHold(legalHoldId);
            return Results.Ok(hold);
        }).WithName("ActivateRecordArrLegalHold");

        group.MapPost("/legal-holds/{legalHoldId}/release", (string legalHoldId, ReleaseLegalHoldRequest request, RecordArrStore store) =>
        {
            var hold = store.ReleaseLegalHold(legalHoldId, request.ReleasedByPersonId, request.ReleaseReason);
            return Results.Ok(hold);
        }).WithName("ReleaseRecordArrLegalHold");
    }

    public sealed record CreateRecordRequest(
        string Title,
        string Description,
        string RecordType,
        string DocumentType,
        string SourceProduct,
        string SourceObjectType,
        string SourceObjectId,
        string SourceObjectDisplayName,
        string OwnerPersonId,
        string UploadedByPersonId,
        string CurrentFileName,
        string CurrentMimeType);

    public sealed record UpdateRecordRequest(
        string Status,
        string? Classification,
        DateTimeOffset? EffectiveAt,
        DateTimeOffset? ExpiresAt);

    public sealed record DisposeRecordRequest(string ActorPersonId);

    public sealed record CreateUploadSessionRequest(
        string SourceProduct,
        string SourceObjectType,
        string SourceObjectId,
        string UploadPurpose,
        bool RequiresDocumentScan,
        bool RequiresOcr,
        bool RequiresManualReview);

    public sealed record CompleteUploadSessionRequest(string RecordId);
    public sealed record RevokeUploadSessionRequest(string Reason);
    public sealed record CreateDocumentScanRequest(string RecordId, string OriginalFileName, string ScanPurpose);
    public sealed record ManualCorrectionRequest(string EdgeCoordinates);
    public sealed record ReviewExtractionResultRequest(string ReviewedByPersonId, string Status, string? FailureReason);
    public sealed record CreateDocumentDistributionRequest(string VersionId, string DistributionType, string TargetRef);
    public sealed record RevokeDocumentDistributionRequest(string RevokedByPersonId, string? RevokeReason);
    public sealed record ExpireDocumentDistributionRequest(string ExpiredByPersonId, string? ExpireReason);
    public sealed record CreateDocumentAcknowledgementRequest(string VersionId, string PersonId, string? AttestationText, DateTimeOffset? DueAt);
    public sealed record CompleteDocumentAcknowledgementRequest(string? SignatureRecordRef);
    public sealed record PromoteControlledDocumentVersionRequest(string ApprovedByPersonId, DateTimeOffset? EffectiveAt);
    public sealed record UpdateControlledDocumentStatusRequest(string UpdatedByPersonId);
    public sealed record SupersedeControlledDocumentRequest(string SupersededByDocumentRef, string SupersededByPersonId);
    public sealed record CreateAccessPolicyRequest(
        string RecordId,
        string PolicyType,
        string Status,
        IReadOnlyList<string> ReadRules,
        IReadOnlyList<string> WriteRules,
        IReadOnlyList<string> DownloadRules,
        IReadOnlyList<string> ShareRules,
        IReadOnlyList<string> ExportRules,
        IReadOnlyList<string> PurgeRules,
        string CreatedByPersonId);
    public sealed record UpdateAccessPolicyRequest(
        string RecordId,
        string PolicyType,
        string Status,
        IReadOnlyList<string> ReadRules,
        IReadOnlyList<string> WriteRules,
        IReadOnlyList<string> DownloadRules,
        IReadOnlyList<string> ShareRules,
        IReadOnlyList<string> ExportRules,
        IReadOnlyList<string> PurgeRules,
        string UpdatedByPersonId);
    public sealed record CreateAccessGrantRequest(string RecordId, string GranteeType, string GranteeRef, string Permission, string GrantedByPersonId, DateTimeOffset? ExpiresAt);
    public sealed record RevokeAccessGrantRequest(string RevokedByPersonId, string? RevokeReason);
    public sealed record RecordExternalShareAccessRequest(string AccessedByPersonId, string AccessAction, string? SourceIp, string? UserAgent);
    public sealed record ExpireExternalShareRequest(string ExpiredByPersonId);
    public sealed record CreateDisposalReviewRequest(string RecordId, string RetentionStatusRef, string ProposedAction, string RequestedByPersonId);
    public sealed record CompleteDisposalReviewRequest(string Status, string? ReviewedByPersonId, string? DecisionReason);

    public sealed record CreateEvidenceMappingRequest(
        string RecordId,
        string SourceProduct,
        string SourceObjectType,
        string SourceObjectId,
        string ComplianceRequirementRef,
        string EvidenceTypeKey,
        string MappingSource,
        decimal ConfidenceScore);

    public sealed record ConfirmEvidenceMappingRequest(string ConfirmedByPersonId, string? Notes);
    public sealed record RejectEvidenceMappingRequest(string RejectedByPersonId, string RejectionReason, string? Notes);

    public sealed record CreatePackageRequest(
        string Title,
        string PackageType,
        string SourceProduct,
        string SourceObjectRef,
        string RecordRef);

    public sealed record CreateLegalHoldRequest(
        string Title,
        string Description,
        string HoldType,
        string SourceProduct,
        string SourceObjectType,
        string SourceObjectId,
        string CreatedByPersonId,
        IReadOnlyList<string> ScopeRules,
        IReadOnlyList<string> RecordRefs);

    public sealed record ReleaseLegalHoldRequest(string ReleasedByPersonId, string ReleaseReason);
}
