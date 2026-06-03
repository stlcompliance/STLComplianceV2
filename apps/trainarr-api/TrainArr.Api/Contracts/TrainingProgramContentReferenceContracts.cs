namespace TrainArr.Api.Contracts;

public static class TrainingProgramContentReferenceTypes
{
    public const string UploadedPdf = "uploaded_pdf";
    public const string UploadedVideo = "uploaded_video";
    public const string ExternalUrl = "external_url";
    public const string InternalDocumentReference = "internal_document_reference";
    public const string PolicyDocument = "policy_document";
    public const string ComplianceCoreCitation = "compliance_core_citation";
    public const string MaintainArrAssetProcedure = "maintainarr_asset_procedure";
    public const string StaffArrPolicy = "staffarr_policy";
    public const string SupplyArrVendorDocument = "supplyarr_vendor_document";
    public const string EmbeddedTextLesson = "embedded_text_lesson";
    public const string QuizBank = "quiz_bank";
}

public sealed record CreateTrainingProgramContentReferenceRequest(
    string ContentType,
    string Title,
    string ReferenceValue,
    string? Notes = null,
    string? LocaleTag = null);

public sealed record TrainingProgramContentReferenceResponse(
    Guid ContentReferenceId,
    Guid TrainingProgramId,
    string ContentType,
    string Title,
    string ReferenceValue,
    string? Notes,
    string? LocaleTag,
    Guid? CreatedByUserId,
    DateTimeOffset CreatedAt);
