namespace SupplyArr.Api.Contracts;

public sealed record SubstitutionItemResponse(
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    Guid AliasId,
    string AliasKey,
    string ManufacturerName,
    string ManufacturerPartNumber,
    DateTimeOffset CreatedAt);

public sealed record SupplyDocumentItemResponse(
    Guid DocumentId,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    string SupplierUnitKind,
    string DocumentKey,
    string DocumentTypeKey,
    string Title,
    string ReviewStatus,
    DateTimeOffset? EffectiveAt,
    DateTimeOffset? ExpiresAt,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset UpdatedAt);

public sealed record CreateSupplyDocumentRequest(
    Guid SupplierId,
    string DocumentKey,
    string DocumentTypeKey,
    string Title,
    DateTimeOffset? EffectiveAt,
    DateTimeOffset? ExpiresAt,
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageUri);

public sealed record ContractSnapshotItemResponse(
    string ContractType,
    Guid SnapshotId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    string SnapshotKey,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    DateTimeOffset UpdatedAt);

public sealed record ImportOptionResponse(
    string ImportType,
    string Description);

public sealed record ImportHistoryItemResponse(
    Guid ImportHistoryId,
    string ImportType,
    bool DryRun,
    bool Succeeded,
    int RowsRead,
    int IssueCount,
    Guid? ActorUserId,
    DateTimeOffset OccurredAt);

public sealed record ImportHistoryListResponse(
    IReadOnlyList<ImportHistoryItemResponse> Items);

public sealed record ImportErrorExportIssueRequest(
    int LineNumber,
    string Code,
    string Message);

public sealed record ImportErrorExportRequest(
    string ImportType,
    IReadOnlyList<ImportErrorExportIssueRequest> Issues);

public sealed record ImportFieldMappingRequest(
    string ImportType,
    string Csv,
    IReadOnlyDictionary<string, string> FieldMappings);

public sealed record ImportFieldMappingResponse(
    string ImportType,
    bool Succeeded,
    IReadOnlyList<string> SourceHeaders,
    IReadOnlyList<string> CanonicalHeaders,
    IReadOnlyList<string> MissingRequiredHeaders,
    IReadOnlyList<string> UnmappedSourceHeaders,
    string Csv);

public sealed record PartCatalogCsvImportRequest(
    string Csv,
    bool DryRun = true,
    string? FileName = null);

public sealed record PartCatalogCsvImportIssue(
    int LineNumber,
    string Code,
    string Message);

public sealed record PartCatalogCsvImportResponse(
    string ImportType,
    bool DryRun,
    bool Succeeded,
    int RowsRead,
    int CatalogsAccepted,
    int PartsAccepted,
    int CatalogsCreated,
    int PartsCreated,
    IReadOnlyList<PartCatalogCsvImportIssue> Issues);

public sealed record SupplierCatalogCsvImportRequest(
    string Csv,
    bool DryRun = true,
    string? FileName = null);

public sealed record SupplierCatalogCsvImportIssue(
    int LineNumber,
    string Code,
    string Message);

public sealed record SupplierCatalogCsvImportResponse(
    string ImportType,
    bool DryRun,
    bool Succeeded,
    int RowsRead,
    int LinksAccepted,
    int LinksCreated,
    IReadOnlyList<SupplierCatalogCsvImportIssue> Issues);

public sealed record SupplierDocumentsCsvImportRequest(
    string Csv,
    bool DryRun = true,
    string? FileName = null);

public sealed record SupplierDocumentsCsvImportIssue(
    int LineNumber,
    string Code,
    string Message);

public sealed record SupplierDocumentsCsvImportResponse(
    string ImportType,
    bool DryRun,
    bool Succeeded,
    int RowsRead,
    int DocumentsAccepted,
    int DocumentsCreated,
    IReadOnlyList<SupplierDocumentsCsvImportIssue> Issues);

public sealed record InventoryCountsCsvImportRequest(
    string Csv,
    bool DryRun = true,
    string? FileName = null);

public sealed record InventoryCountsCsvImportIssue(
    int LineNumber,
    string Code,
    string Message);

public sealed record InventoryCountsCsvImportResponse(
    string ImportType,
    bool DryRun,
    bool Succeeded,
    int RowsRead,
    int CountsAccepted,
    int CountsApplied,
    IReadOnlyList<InventoryCountsCsvImportIssue> Issues);

public sealed record PriceListCsvImportRequest(
    string Csv,
    bool DryRun = true,
    string? FileName = null);

public sealed record PriceListCsvImportIssue(
    int LineNumber,
    string Code,
    string Message);

public sealed record PriceListCsvImportResponse(
    string ImportType,
    bool DryRun,
    bool Succeeded,
    int RowsRead,
    int PricesAccepted,
    int PricesCreated,
    IReadOnlyList<PriceListCsvImportIssue> Issues);

public sealed record LeadTimeListCsvImportRequest(
    string Csv,
    bool DryRun = true,
    string? FileName = null);

public sealed record LeadTimeListCsvImportIssue(
    int LineNumber,
    string Code,
    string Message);

public sealed record LeadTimeListCsvImportResponse(
    string ImportType,
    bool DryRun,
    bool Succeeded,
    int RowsRead,
    int LeadTimesAccepted,
    int LeadTimesCreated,
    IReadOnlyList<LeadTimeListCsvImportIssue> Issues);

public sealed record AvailabilityListCsvImportRequest(
    string Csv,
    bool DryRun = true,
    string? FileName = null);

public sealed record AvailabilityListCsvImportIssue(
    int LineNumber,
    string Code,
    string Message);

public sealed record AvailabilityListCsvImportResponse(
    string ImportType,
    bool DryRun,
    bool Succeeded,
    int RowsRead,
    int AvailabilityAccepted,
    int AvailabilityCreated,
    IReadOnlyList<AvailabilityListCsvImportIssue> Issues);

public sealed record ContractsCsvImportRequest(
    string Csv,
    bool DryRun = true,
    string? FileName = null);

public sealed record ContractsCsvImportIssue(
    int LineNumber,
    string Code,
    string Message);

public sealed record ContractsCsvImportResponse(
    string ImportType,
    bool DryRun,
    bool Succeeded,
    int RowsRead,
    int ContractsAccepted,
    int ContractsCreated,
    IReadOnlyList<ContractsCsvImportIssue> Issues);

public sealed record SuppliersCsvImportRequest(
    string Csv,
    bool DryRun = true,
    string? FileName = null);

public sealed record SuppliersCsvImportIssue(
    int LineNumber,
    string Code,
    string Message);

public sealed record SuppliersCsvImportResponse(
    string ImportType,
    bool DryRun,
    bool Succeeded,
    int RowsRead,
    int SuppliersAccepted,
    int SuppliersCreated,
    IReadOnlyList<SuppliersCsvImportIssue> Issues);

public sealed record ContactsCsvImportRequest(
    string Csv,
    bool DryRun = true,
    string? FileName = null);

public sealed record ContactsCsvImportIssue(
    int LineNumber,
    string Code,
    string Message);

public sealed record ContactsCsvImportResponse(
    string ImportType,
    bool DryRun,
    bool Succeeded,
    int RowsRead,
    int ContactsAccepted,
    int ContactsCreated,
    IReadOnlyList<ContactsCsvImportIssue> Issues);

public sealed record OpenPurchaseOrdersCsvImportRequest(
    string Csv,
    bool DryRun = true,
    string? FileName = null);

public sealed record OpenPurchaseOrdersCsvImportIssue(
    int LineNumber,
    string Code,
    string Message);

public sealed record OpenPurchaseOrdersCsvImportResponse(
    string ImportType,
    bool DryRun,
    bool Succeeded,
    int RowsRead,
    int OrdersAccepted,
    int LinesAccepted,
    int OrdersCreated,
    IReadOnlyList<OpenPurchaseOrdersCsvImportIssue> Issues);

public sealed record PurchaseHistoryCsvImportRequest(
    string Csv,
    bool DryRun = true,
    string? FileName = null);

public sealed record PurchaseHistoryCsvImportIssue(
    int LineNumber,
    string Code,
    string Message);

public sealed record PurchaseHistoryCsvImportResponse(
    string ImportType,
    bool DryRun,
    bool Succeeded,
    int RowsRead,
    int OrdersAccepted,
    int LinesAccepted,
    int OrdersCreated,
    int ReceiptsPosted,
    IReadOnlyList<PurchaseHistoryCsvImportIssue> Issues);

public sealed record ExportOptionResponse(
    string ExportType,
    string Description,
    string Endpoint);

public sealed record AdminOverviewResponse(
    string ProductKey,
    string RoleKey,
    bool IsPlatformAdmin,
    IReadOnlyList<string> LaunchableProductKeys,
    IReadOnlyList<string> AvailableAdminAreas);
