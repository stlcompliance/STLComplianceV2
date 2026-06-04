using RecordArr.Api.Data;

namespace RecordArr.Api.Endpoints;

public static class RecordArrIntegrationEndpoints
{
    public static void MapRecordArrIntegrationEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/integrations"), "/api/integrations");
        MapRoutes(app.MapGroup("/api/v1/integrations"), "/api/v1/integrations");
    }

    private static void MapRoutes(RouteGroupBuilder group, string routePrefix)
    {
        group.WithTags("Integrations").RequireAuthorization();

        group.MapGet("/records", (string? search, RecordArrStore store) => Results.Ok(store.GetRecords(search)))
            .WithName($"ListRecordArrIntegrationRecords{routePrefix}");

        group.MapGet("/records/{recordId}", (string recordId, RecordArrStore store) =>
        {
            var record = store.GetRecord(recordId);
            return record is null ? Results.NotFound() : Results.Ok(record);
        }).WithName($"GetRecordArrIntegrationRecord{routePrefix}");

        group.MapPost("/records", (WorkspaceEndpoints.CreateRecordRequest request, RecordArrStore store) =>
        {
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
            return Results.Created($"{routePrefix}/records/{record.RecordId}", record);
        }).WithName($"CreateRecordArrIntegrationRecord{routePrefix}");

        group.MapPatch("/records/{recordId}", (string recordId, WorkspaceEndpoints.UpdateRecordRequest request, RecordArrStore store) =>
        {
            var updated = store.UpdateRecordStatus(recordId, request.Status, request.Classification, request.EffectiveAt, request.ExpiresAt);
            return Results.Ok(updated);
        }).WithName($"UpdateRecordArrIntegrationRecord{routePrefix}");

        group.MapPost("/records/{recordId}/archive", (string recordId, WorkspaceEndpoints.DisposeRecordRequest request, RecordArrStore store) =>
        {
            var updated = store.ArchiveRecord(recordId, request.ActorPersonId);
            return Results.Ok(updated);
        }).WithName($"ArchiveRecordArrIntegrationRecord{routePrefix}");

        group.MapPost("/records/{recordId}/purge", (string recordId, WorkspaceEndpoints.DisposeRecordRequest request, RecordArrStore store) =>
        {
            var updated = store.PurgeRecord(recordId, request.ActorPersonId);
            return Results.Ok(updated);
        }).WithName($"PurgeRecordArrIntegrationRecord{routePrefix}");

        group.MapGet("/upload-sessions/{uploadSessionId}", (string uploadSessionId, RecordArrStore store) =>
        {
            var session = store.GetUploadSession(uploadSessionId);
            return session is null ? Results.NotFound() : Results.Ok(session);
        }).WithName($"GetRecordArrIntegrationUploadSession{routePrefix}");

        group.MapGet("/upload-sessions", (RecordArrStore store) => Results.Ok(store.GetUploadSessions()))
            .WithName($"ListRecordArrIntegrationUploadSessions{routePrefix}");

        group.MapPost("/upload-sessions/{uploadSessionId}/complete", (string uploadSessionId, WorkspaceEndpoints.CompleteUploadSessionRequest request, RecordArrStore store) =>
        {
            var session = store.CompleteUploadSession(uploadSessionId, request.RecordId);
            return Results.Ok(session);
        }).WithName($"CompleteRecordArrIntegrationUploadSession{routePrefix}");

        group.MapPost("/upload-sessions/{uploadSessionId}/revoke", (string uploadSessionId, WorkspaceEndpoints.RevokeUploadSessionRequest request, RecordArrStore store) =>
        {
            var session = store.RevokeUploadSession(uploadSessionId, request.Reason);
            return Results.Ok(session);
        }).WithName($"RevokeRecordArrIntegrationUploadSession{routePrefix}");

        group.MapPost("/document-scans", (WorkspaceEndpoints.CreateDocumentScanRequest request, RecordArrStore store) =>
        {
            var scan = store.CreateScanProcessing(request.RecordId, request.OriginalFileName, request.ScanPurpose);
            return Results.Created($"{routePrefix}/document-scans/{scan.ScanProcessingId}", scan);
        }).WithName($"CreateRecordArrIntegrationDocumentScan{routePrefix}");

        group.MapGet("/document-scans", (RecordArrStore store) => Results.Ok(store.GetScanProcessing()))
            .WithName($"ListRecordArrIntegrationDocumentScans{routePrefix}");

        group.MapGet("/document-scans/{scanProcessingId}", (string scanProcessingId, RecordArrStore store) =>
        {
            var scan = store.GetScanProcessing(scanProcessingId);
            return scan is null ? Results.NotFound() : Results.Ok(scan);
        }).WithName($"GetRecordArrIntegrationDocumentScan{routePrefix}");

        group.MapPost("/document-scans/{scanProcessingId}/manual-correction", (string scanProcessingId, WorkspaceEndpoints.ManualCorrectionRequest request, RecordArrStore store) =>
        {
            var scan = store.ApplyManualCorrection(scanProcessingId, request.EdgeCoordinates);
            return Results.Ok(scan);
        }).WithName($"ApplyRecordArrIntegrationManualCorrection{routePrefix}");

        group.MapGet("/ocr-results/{ocrResultId}", (string ocrResultId, RecordArrStore store) =>
        {
            var result = store.GetOcrResult(ocrResultId);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).WithName($"GetRecordArrIntegrationOcrResult{routePrefix}");

        group.MapGet("/extraction-results/{extractionResultId}", (string extractionResultId, RecordArrStore store) =>
        {
            var result = store.GetExtractionResult(extractionResultId);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).WithName($"GetRecordArrIntegrationExtractionResult{routePrefix}");

        group.MapPost("/extraction-results/{extractionResultId}/review", (string extractionResultId, WorkspaceEndpoints.ReviewExtractionResultRequest request, RecordArrStore store) =>
        {
            var result = store.ReviewExtractionResult(extractionResultId, request.ReviewedByPersonId, request.Status, request.FailureReason);
            return Results.Ok(result);
        }).WithName($"ReviewRecordArrIntegrationExtractionResult{routePrefix}");

        group.MapGet("/evidence-mappings", (RecordArrStore store) => Results.Ok(store.GetEvidenceMappings()))
            .WithName($"ListRecordArrIntegrationEvidenceMappings{routePrefix}");

        group.MapPost("/evidence-mappings", (WorkspaceEndpoints.CreateEvidenceMappingRequest request, RecordArrStore store) =>
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
            return Results.Created($"{routePrefix}/evidence-mappings/{mapping.EvidenceMappingId}", mapping);
        }).WithName($"CreateRecordArrIntegrationEvidenceMapping{routePrefix}");

        group.MapPost("/evidence-mappings/{mappingId}/confirm", (string mappingId, WorkspaceEndpoints.ConfirmEvidenceMappingRequest request, RecordArrStore store) =>
        {
            var mapping = store.UpdateEvidenceMapping(mappingId, "confirmed", request.ConfirmedByPersonId, request.Notes, null);
            return Results.Ok(mapping);
        }).WithName($"ConfirmRecordArrIntegrationEvidenceMapping{routePrefix}");

        group.MapPost("/evidence-mappings/{mappingId}/reject", (string mappingId, WorkspaceEndpoints.RejectEvidenceMappingRequest request, RecordArrStore store) =>
        {
            var mapping = store.UpdateEvidenceMapping(mappingId, "rejected", request.RejectedByPersonId, request.Notes, request.RejectionReason);
            return Results.Ok(mapping);
        }).WithName($"RejectRecordArrIntegrationEvidenceMapping{routePrefix}");

        group.MapGet("/record-packages", (RecordArrStore store) => Results.Ok(store.GetPackages()))
            .WithName($"ListRecordArrIntegrationPackages{routePrefix}");

        group.MapGet("/record-packages/{packageId}", (string packageId, RecordArrStore store) =>
        {
            var package = store.GetPackage(packageId);
            return package is null ? Results.NotFound() : Results.Ok(package);
        }).WithName($"GetRecordArrIntegrationPackage{routePrefix}");

        group.MapPost("/record-packages", (WorkspaceEndpoints.CreatePackageRequest request, RecordArrStore store) =>
        {
            var package = store.CreatePackage(request.Title, request.PackageType, request.SourceProduct, request.SourceObjectRef, request.RecordRef);
            return Results.Created($"{routePrefix}/record-packages/{package.PackageId}", package);
        }).WithName($"CreateRecordArrIntegrationPackage{routePrefix}");

        group.MapPost("/record-packages/{packageId}/lock", (string packageId, RecordArrStore store) =>
        {
            var package = store.LockPackage(packageId);
            return Results.Ok(package);
        }).WithName($"LockRecordArrIntegrationPackage{routePrefix}");

        group.MapGet("/record-packages/{packageId}/download", (string packageId, RecordArrStore store) =>
        {
            var package = store.GetPackage(packageId);
            return package is null
                ? Results.NotFound()
                : Results.Ok(new { package.PackageId, package.PackageNumber, package.Title, package.Status, package.RecordRefs, package.SourceObjectRefs });
        }).WithName($"DownloadRecordArrIntegrationPackage{routePrefix}");

        group.MapGet("/retention-policies", (RecordArrStore store) => Results.Ok(store.GetRetentionPolicies()))
            .WithName($"ListRecordArrIntegrationRetentionPolicies{routePrefix}");

        group.MapGet("/records/{recordId}/retention-status", (string recordId, RecordArrStore store) =>
        {
            var status = store.GetRetentionStatus(recordId);
            return status is null ? Results.NotFound() : Results.Ok(status);
        }).WithName($"GetRecordArrIntegrationRetentionStatus{routePrefix}");

        group.MapPost("/legal-holds", (WorkspaceEndpoints.CreateLegalHoldRequest request, RecordArrStore store) =>
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
            return Results.Created($"{routePrefix}/legal-holds/{hold.LegalHoldId}", hold);
        }).WithName($"CreateRecordArrIntegrationLegalHold{routePrefix}");

        group.MapGet("/legal-holds", (RecordArrStore store) => Results.Ok(store.GetLegalHolds()))
            .WithName($"ListRecordArrIntegrationLegalHolds{routePrefix}");

        group.MapPost("/legal-holds/{legalHoldId}/release", (string legalHoldId, WorkspaceEndpoints.ReleaseLegalHoldRequest request, RecordArrStore store) =>
        {
            var hold = store.ReleaseLegalHold(legalHoldId, request.ReleasedByPersonId, request.ReleaseReason);
            return Results.Ok(hold);
        }).WithName($"ReleaseRecordArrIntegrationLegalHold{routePrefix}");

        group.MapGet("/controlled-documents", (RecordArrStore store) => Results.Ok(store.GetControlledDocuments()))
            .WithName($"ListRecordArrIntegrationControlledDocuments{routePrefix}");

        group.MapGet("/controlled-documents/{controlledDocumentId}", (string controlledDocumentId, RecordArrStore store) =>
        {
            var document = store.GetControlledDocument(controlledDocumentId);
            return document is null ? Results.NotFound() : Results.Ok(document);
        }).WithName($"GetRecordArrIntegrationControlledDocument{routePrefix}");

        group.MapPost("/controlled-documents", (CreateControlledDocumentRequest request, RecordArrStore store) =>
        {
            var document = store.CreateControlledDocument(request.Title, request.Description, request.ControlledDocumentType, request.OwnerPersonId, request.DepartmentOrgUnitId, request.StaffarrSiteId, request.AcknowledgementRequired);
            return Results.Created($"{routePrefix}/controlled-documents/{document.ControlledDocumentId}", document);
        }).WithName($"CreateRecordArrIntegrationControlledDocument{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/versions", (string controlledDocumentId, CreateControlledDocumentVersionRequest request, RecordArrStore store) =>
        {
            var version = store.CreateDocumentVersion(controlledDocumentId, request.FileName, request.CreatedByPersonId, request.ChangeSummary);
            return Results.Created($"{routePrefix}/controlled-documents/{controlledDocumentId}/versions/{version.VersionId}", version);
        }).WithName($"CreateRecordArrIntegrationControlledDocumentVersion{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/reviews", (string controlledDocumentId, CreateDocumentReviewRequest request, RecordArrStore store) =>
        {
            var review = store.RequestDocumentReview(controlledDocumentId, request.VersionId, request.ReviewType, request.RequestedByPersonId, request.ReviewerPersonId, request.DueAt);
            return Results.Created($"{routePrefix}/controlled-documents/{controlledDocumentId}/reviews/{review.DocumentReviewId}", review);
        }).WithName($"CreateRecordArrIntegrationDocumentReview{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/reviews/{reviewId}/complete", (string controlledDocumentId, string reviewId, CompleteDocumentReviewRequest request, RecordArrStore store) =>
        {
            var review = store.CompleteDocumentReview(reviewId, request.Status, request.DecisionReason, request.Comments);
            return Results.Ok(review);
        }).WithName($"CompleteRecordArrIntegrationDocumentReview{routePrefix}");

        group.MapGet("/controlled-documents/{controlledDocumentId}/versions", (string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentVersions(controlledDocumentId)))
            .WithName($"ListRecordArrIntegrationControlledDocumentVersions{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/versions/{versionId}/promote", (string controlledDocumentId, string versionId, WorkspaceEndpoints.PromoteControlledDocumentVersionRequest request, RecordArrStore store) =>
        {
            var version = store.PromoteDocumentVersion(controlledDocumentId, versionId, request.ApprovedByPersonId, request.EffectiveAt);
            return Results.Ok(version);
        }).WithName($"PromoteRecordArrIntegrationControlledDocumentVersion{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/archive", (string controlledDocumentId, WorkspaceEndpoints.UpdateControlledDocumentStatusRequest request, RecordArrStore store) =>
        {
            var document = store.UpdateControlledDocumentStatus(controlledDocumentId, "archived", request.UpdatedByPersonId);
            return Results.Ok(document);
        }).WithName($"ArchiveRecordArrIntegrationControlledDocument{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/obsolete", (string controlledDocumentId, WorkspaceEndpoints.UpdateControlledDocumentStatusRequest request, RecordArrStore store) =>
        {
            var document = store.UpdateControlledDocumentStatus(controlledDocumentId, "obsolete", request.UpdatedByPersonId);
            return Results.Ok(document);
        }).WithName($"ObsoleteRecordArrIntegrationControlledDocument{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/supersede", (string controlledDocumentId, WorkspaceEndpoints.SupersedeControlledDocumentRequest request, RecordArrStore store) =>
        {
            var document = store.SupersedeControlledDocument(controlledDocumentId, request.SupersededByDocumentRef, request.SupersededByPersonId);
            return Results.Ok(document);
        }).WithName($"SupersedeRecordArrIntegrationControlledDocument{routePrefix}");

        group.MapGet("/controlled-documents/{controlledDocumentId}/reviews", (string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentReviews(controlledDocumentId)))
            .WithName($"ListRecordArrIntegrationControlledDocumentReviews{routePrefix}");

        group.MapGet("/controlled-documents/{controlledDocumentId}/distributions", (string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentDistributions(controlledDocumentId)))
            .WithName($"ListRecordArrIntegrationControlledDocumentDistributions{routePrefix}");

        group.MapGet("/controlled-documents/{controlledDocumentId}/acknowledgements", (string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentAcknowledgements(controlledDocumentId)))
            .WithName($"ListRecordArrIntegrationControlledDocumentAcknowledgements{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/distributions", (string controlledDocumentId, WorkspaceEndpoints.CreateDocumentDistributionRequest request, RecordArrStore store) =>
        {
            var distribution = store.CreateDocumentDistribution(controlledDocumentId, request.VersionId, request.DistributionType, request.TargetRef);
            return Results.Created($"{routePrefix}/controlled-documents/{controlledDocumentId}/distributions/{distribution.DistributionId}", distribution);
        }).WithName($"CreateRecordArrIntegrationControlledDocumentDistribution{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/distributions/{distributionId}/revoke", (string controlledDocumentId, string distributionId, WorkspaceEndpoints.RevokeDocumentDistributionRequest request, RecordArrStore store) =>
        {
            var distribution = store.RevokeDocumentDistribution(distributionId, request.RevokedByPersonId, request.RevokeReason);
            return Results.Ok(distribution);
        }).WithName($"RevokeRecordArrIntegrationControlledDocumentDistribution{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/distributions/{distributionId}/expire", (string controlledDocumentId, string distributionId, WorkspaceEndpoints.ExpireDocumentDistributionRequest request, RecordArrStore store) =>
        {
            var distribution = store.ExpireDocumentDistribution(distributionId, request.ExpiredByPersonId, request.ExpireReason);
            return Results.Ok(distribution);
        }).WithName($"ExpireRecordArrIntegrationControlledDocumentDistribution{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/acknowledgements", (string controlledDocumentId, WorkspaceEndpoints.CreateDocumentAcknowledgementRequest request, RecordArrStore store) =>
        {
            var acknowledgement = store.CreateDocumentAcknowledgement(controlledDocumentId, request.VersionId, request.PersonId, request.AttestationText, request.DueAt);
            return Results.Created($"{routePrefix}/controlled-documents/{controlledDocumentId}/acknowledgements/{acknowledgement.AcknowledgementId}", acknowledgement);
        }).WithName($"CreateRecordArrIntegrationControlledDocumentAcknowledgement{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/acknowledgements/{acknowledgementId}/complete", (string controlledDocumentId, string acknowledgementId, WorkspaceEndpoints.CompleteDocumentAcknowledgementRequest request, RecordArrStore store) =>
        {
            var acknowledgement = store.CompleteDocumentAcknowledgement(acknowledgementId, request.SignatureRecordRef);
            return Results.Ok(acknowledgement);
        }).WithName($"CompleteRecordArrIntegrationControlledDocumentAcknowledgement{routePrefix}");

        group.MapPost("/external-shares", (CreateExternalShareRequest request, RecordArrStore store) =>
        {
            var share = store.CreateExternalShare(request.RecordId, request.RecipientName, request.RecipientEmail, request.SharePurpose, request.AllowedActions, request.CreatedByPersonId);
            return Results.Created($"{routePrefix}/external-shares/{share.ExternalShareId}", share);
        }).WithName($"CreateRecordArrIntegrationExternalShare{routePrefix}");

        group.MapPost("/external-shares/{externalShareId}/revoke", (string externalShareId, RevokeExternalShareRequest request, RecordArrStore store) =>
        {
            var share = store.RevokeExternalShare(externalShareId, request.RevokedByPersonId);
            return Results.Ok(share);
        }).WithName($"RevokeRecordArrIntegrationExternalShare{routePrefix}");

        group.MapPost("/external-shares/{externalShareId}/access", (string externalShareId, WorkspaceEndpoints.RecordExternalShareAccessRequest request, RecordArrStore store) =>
        {
            var share = store.RecordExternalShareAccess(externalShareId, request.AccessedByPersonId, request.AccessAction, request.SourceIp, request.UserAgent);
            return Results.Ok(share);
        }).WithName($"AccessRecordArrIntegrationExternalShare{routePrefix}");

        group.MapPost("/external-shares/{externalShareId}/expire", (string externalShareId, WorkspaceEndpoints.ExpireExternalShareRequest request, RecordArrStore store) =>
        {
            var share = store.ExpireExternalShare(externalShareId, request.ExpiredByPersonId);
            return Results.Ok(share);
        }).WithName($"ExpireRecordArrIntegrationExternalShare{routePrefix}");

        group.MapPost("/redactions", (CreateRedactionRequest request, RecordArrStore store) =>
        {
            var redaction = store.CreateRedaction(request.SourceRecordId, request.RedactedRecordId, request.RedactionReason, request.RedactedByPersonId, request.RedactionRules);
            return Results.Created($"{routePrefix}/redactions/{redaction.RedactionId}", redaction);
        }).WithName($"CreateRecordArrIntegrationRedaction{routePrefix}");

        group.MapGet("/access-policies", (RecordArrStore store) => Results.Ok(store.GetAccessPolicies()))
            .WithName($"ListRecordArrIntegrationAccessPolicies{routePrefix}");

        group.MapGet("/access-grants", (RecordArrStore store) => Results.Ok(store.GetAccessGrants()))
            .WithName($"ListRecordArrIntegrationAccessGrants{routePrefix}");

        group.MapPost("/access-grants", (WorkspaceEndpoints.CreateAccessGrantRequest request, RecordArrStore store) =>
        {
            var grant = store.CreateAccessGrant(request.RecordId, request.GranteeType, request.GranteeRef, request.Permission, request.GrantedByPersonId, request.ExpiresAt);
            return Results.Created($"{routePrefix}/access-grants/{grant.AccessGrantId}", grant);
        }).WithName($"CreateRecordArrIntegrationAccessGrant{routePrefix}");

        group.MapPost("/access-grants/{accessGrantId}/revoke", (string accessGrantId, WorkspaceEndpoints.RevokeAccessGrantRequest request, RecordArrStore store) =>
        {
            var grant = store.RevokeAccessGrant(accessGrantId, request.RevokedByPersonId, request.RevokeReason);
            return Results.Ok(grant);
        }).WithName($"RevokeRecordArrIntegrationAccessGrant{routePrefix}");

        group.MapGet("/external-shares", (RecordArrStore store) => Results.Ok(store.GetExternalShares()))
            .WithName($"ListRecordArrIntegrationExternalShares{routePrefix}");

        group.MapGet("/redactions", (RecordArrStore store) => Results.Ok(store.GetRedactions()))
            .WithName($"ListRecordArrIntegrationRedactions{routePrefix}");

        group.MapGet("/disposal-reviews", (RecordArrStore store) => Results.Ok(store.GetDisposalReviews()))
            .WithName($"ListRecordArrIntegrationDisposalReviews{routePrefix}");

        group.MapPost("/disposal-reviews", (WorkspaceEndpoints.CreateDisposalReviewRequest request, RecordArrStore store) =>
        {
            var review = store.CreateDisposalReview(request.RecordId, request.RetentionStatusRef, request.ProposedAction, request.RequestedByPersonId);
            return Results.Created($"{routePrefix}/disposal-reviews/{review.DisposalReviewId}", review);
        }).WithName($"CreateRecordArrIntegrationDisposalReview{routePrefix}");

        group.MapPost("/disposal-reviews/{disposalReviewId}/complete", (string disposalReviewId, WorkspaceEndpoints.CompleteDisposalReviewRequest request, RecordArrStore store) =>
        {
            var review = store.CompleteDisposalReview(disposalReviewId, request.Status, request.ReviewedByPersonId, request.DecisionReason);
            return Results.Ok(review);
        }).WithName($"CompleteRecordArrIntegrationDisposalReview{routePrefix}");

        group.MapGet("/access-logs", (RecordArrStore store) => Results.Ok(store.GetAccessLogs()))
            .WithName($"ListRecordArrIntegrationAccessLogs{routePrefix}");
    }

    public sealed record CreateControlledDocumentRequest(string Title, string Description, string ControlledDocumentType, string OwnerPersonId, string DepartmentOrgUnitId, string StaffarrSiteId, bool AcknowledgementRequired);
    public sealed record CreateControlledDocumentVersionRequest(string FileName, string CreatedByPersonId, string? ChangeSummary);
    public sealed record CreateDocumentReviewRequest(string VersionId, string ReviewType, string RequestedByPersonId, string ReviewerPersonId, DateTimeOffset? DueAt);
    public sealed record CompleteDocumentReviewRequest(string Status, string? DecisionReason, string? Comments);
    public sealed record CreateExternalShareRequest(string RecordId, string RecipientName, string RecipientEmail, string SharePurpose, IReadOnlyList<string> AllowedActions, string CreatedByPersonId);
    public sealed record RevokeExternalShareRequest(string RevokedByPersonId);
    public sealed record CreateRedactionRequest(string SourceRecordId, string RedactedRecordId, string RedactionReason, string RedactedByPersonId, IReadOnlyList<string> RedactionRules);
}
