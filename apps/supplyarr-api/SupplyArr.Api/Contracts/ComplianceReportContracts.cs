namespace SupplyArr.Api.Contracts;

public sealed record ComplianceReportTotalsResponse(
    int PartyCount,
    int DocumentCount,
    int ExpiredCount,
    int ExpiringSoonCount,
    int ReviewPendingCount,
    int ApprovedCount,
    int RejectedCount);

public sealed record SupplierComplianceReportTotalsResponse(
    int SupplierCount,
    int DocumentCount,
    int ExpiredCount,
    int ExpiringSoonCount,
    int ReviewPendingCount,
    int ApprovedCount,
    int RejectedCount);

public sealed record CompliancePartySummaryItemResponse(
    Guid ExternalPartyId,
    string PartyKey,
    string DisplayName,
    string PartyType,
    string ApprovalStatus,
    string CompliancePosture,
    int DocumentCount,
    int ExpiredCount,
    int ExpiringSoonCount,
    int ReviewPendingCount);

public sealed record SupplierComplianceSummaryItemResponse(
    Guid SupplierId,
    string SupplierKey,
    string DisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    string ApprovalStatus,
    string CompliancePosture,
    int DocumentCount,
    int ExpiredCount,
    int ExpiringSoonCount,
    int ReviewPendingCount);

public sealed record ComplianceDocumentSummaryItemResponse(
    Guid DocumentId,
    Guid ExternalPartyId,
    string PartyKey,
    string PartyDisplayName,
    string PartyType,
    string DocumentKey,
    string DocumentTypeKey,
    string Title,
    int Version,
    string ReviewStatus,
    string EffectiveStatus,
    bool IsExpired,
    bool IsExpiringSoon,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset UpdatedAt);

public sealed record SupplierComplianceDocumentSummaryItemResponse(
    Guid DocumentId,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    string DocumentKey,
    string DocumentTypeKey,
    string Title,
    int Version,
    string ReviewStatus,
    string EffectiveStatus,
    bool IsExpired,
    bool IsExpiringSoon,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset UpdatedAt);

public sealed record ComplianceReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    ComplianceReportTotalsResponse Totals,
    IReadOnlyList<CompliancePartySummaryItemResponse> Parties,
    IReadOnlyList<ComplianceDocumentSummaryItemResponse> Documents);

public sealed record SupplierComplianceReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    SupplierComplianceReportTotalsResponse Totals,
    IReadOnlyList<SupplierComplianceSummaryItemResponse> Suppliers,
    IReadOnlyList<SupplierComplianceDocumentSummaryItemResponse> Documents);

public sealed record ComplianceDocumentDetailItemResponse(
    Guid DocumentId,
    string DocumentKey,
    string DocumentTypeKey,
    string Title,
    int Version,
    string ReviewStatus,
    string EffectiveStatus,
    bool IsExpired,
    bool IsExpiringSoon,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? EffectiveAt,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Notes,
    DateTimeOffset? ReviewedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CompliancePartyDetailResponse(
    CompliancePartySummaryItemResponse Summary,
    IReadOnlyList<ComplianceDocumentDetailItemResponse> Documents);

public sealed record SupplierComplianceDetailResponse(
    SupplierComplianceSummaryItemResponse Summary,
    IReadOnlyList<ComplianceDocumentDetailItemResponse> Documents);

public sealed record ComplianceReportAlertResponse(
    string AlertType,
    string Severity,
    Guid? ExternalPartyId,
    string? PartyKey,
    Guid? PurchaseRequestId,
    string? PurchaseRequestKey,
    Guid? ProcurementExceptionId,
    string Message,
    DateTimeOffset DetectedAt);
