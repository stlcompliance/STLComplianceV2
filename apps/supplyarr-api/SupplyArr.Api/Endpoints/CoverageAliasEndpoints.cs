using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class CoverageAliasEndpoints
{
    private static readonly IReadOnlyDictionary<string, string[]> ImportHeaders =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["part_catalog_csv"] =
            [
                "catalog_key",
                "catalog_name",
                "catalog_description",
                "part_key",
                "part_name",
                "part_description",
                "category_key",
                "unit_of_measure",
                "manufacturer_name",
                "manufacturer_part_number"
            ],
            ["supplier_catalog_csv"] =
            [
                "supplier_key",
                "part_key",
                "supplier_part_number",
                "is_preferred",
                "catalog_unit_price",
                "catalog_currency_code",
                "catalog_minimum_order_quantity",
                "catalog_lead_time_days",
                "catalog_quantity_available",
                "catalog_availability_status"
            ],
            ["supplier_documents_csv"] =
            [
                "supplier_key",
                "document_key",
                "document_type_key",
                "title",
                "effective_at",
                "expires_at",
                "file_name",
                "content_type",
                "size_bytes",
                "storage_uri"
            ],
            ["inventory_counts_csv"] =
            [
                "part_key",
                "location_key",
                "bin_key",
                "quantity_on_hand"
            ],
            ["price_list_csv"] =
            [
                "supplier_key",
                "part_key",
                "snapshot_key",
                "unit_price",
                "currency_code",
                "minimum_order_quantity",
                "effective_from",
                "source",
                "notes"
            ],
            ["lead_time_list_csv"] =
            [
                "supplier_key",
                "part_key",
                "snapshot_key",
                "lead_time_days",
                "effective_from",
                "source",
                "notes"
            ],
            ["availability_list_csv"] =
            [
                "supplier_key",
                "part_key",
                "snapshot_key",
                "quantity_available",
                "availability_status",
                "effective_from",
                "source",
                "notes"
            ],
            ["contracts_csv"] =
            [
                "supplier_key",
                "contract_key",
                "contract_type",
                "title",
                "effective_from",
                "effective_to",
                "renewal_notice_at",
                "payment_terms",
                "freight_terms",
                "warranty_terms",
                "minimum_spend",
                "service_level_notes",
                "approval_status",
                "status",
                "notes"
            ],
            ["suppliers_csv"] =
            [
                "supplier_key",
                "parent_supplier_key",
                "unit_kind",
                "display_name",
                "legal_name",
                "tax_identifier",
                "approval_status",
                "status",
                "notes",
                "service_types"
            ],
            ["contacts_csv"] =
            [
                "supplier_key",
                "contact_name",
                "email",
                "phone",
                "role_label",
                "is_primary"
            ],
            ["open_purchase_orders_csv"] =
            [
                "order_key",
                "request_key",
                "supplier_key",
                "part_key",
                "quantity_ordered",
                "title",
                "line_notes",
                "order_notes"
            ],
            ["purchase_history_csv"] =
            [
                "order_key",
                "request_key",
                "receipt_key",
                "supplier_key",
                "part_key",
                "quantity_ordered",
                "quantity_received",
                "inventory_bin_key",
                "title",
                "line_notes",
                "order_notes",
                "receipt_notes"
            ]
        };

    private static readonly IReadOnlyDictionary<string, string> ImportDescriptions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["part_catalog_csv"] = "Import part catalogs and parts from CSV.",
            ["supplier_catalog_csv"] = "Import supplier catalog links and catalog facts from CSV.",
            ["supplier_documents_csv"] = "Import supplier compliance document metadata.",
            ["inventory_counts_csv"] = "Import inventory count quantities by location, bin, and part.",
            ["price_list_csv"] = "Import supplier price list snapshots by supplier and part.",
            ["lead_time_list_csv"] = "Import supplier lead-time snapshots by supplier and part.",
            ["availability_list_csv"] = "Import supplier availability snapshots by supplier and part.",
            ["contracts_csv"] = "Import supplier identity and sub-unit contracts from CSV.",
            ["suppliers_csv"] = "Import supplier identities, supplier sub-units, and service coverage from CSV.",
            ["contacts_csv"] = "Import supplier contacts from CSV.",
            ["open_purchase_orders_csv"] = "Import open purchase orders from CSV.",
            ["purchase_history_csv"] = "Import fully received historical purchases from CSV."
        };

    public static void MapSupplyArrCoverageAliasEndpoints(this WebApplication app)
    {
        MapSubstitutions(app);
        MapDocuments(app);
        MapContracts(app);
        MapImportsExports(app);
        MapAdmin(app);
    }

    private static void MapSubstitutions(WebApplication app)
    {
        app.MapGet("/api/v1/substitutions", async (
            Guid? partId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartRegistryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var parts = await service.ListAsync(tenantId, null, cancellationToken);
            var substitutions = parts
                .Where(p => !partId.HasValue || p.PartId == partId.Value)
                .SelectMany(p => p.ManufacturerAliases.Select(a => new SubstitutionItemResponse(
                    p.PartId,
                    p.PartKey,
                    p.DisplayName,
                    a.AliasId,
                    a.AliasKey,
                    a.ManufacturerName,
                    a.ManufacturerPartNumber,
                    a.CreatedAt)))
                .OrderBy(x => x.PartKey)
                .ToList();
            return Results.Ok(substitutions);
        })
        .WithTags("PartCatalog")
        .RequireAuthorization()
        .WithName("ListSubstitutionsV1");
    }

    private static void MapDocuments(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/documents").WithTags("Documents").RequireAuthorization();

        group.MapGet("/", async (
            Guid? supplierId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSupplierOnboardingRead(context.User);
            var tenantId = context.User.GetTenantId();
            var query = db.SupplierComplianceDocuments
                .AsNoTracking()
                .Include(x => x.Supplier)
                .Where(x => x.TenantId == tenantId);
            if (supplierId.HasValue)
            {
                query = query.Where(x => x.SupplierId == supplierId.Value);
            }

            var items = await query
                .OrderByDescending(x => x.UpdatedAt)
                .Select(x => new SupplyDocumentItemResponse(
                    x.Id,
                    x.SupplierId,
                    x.Supplier.SupplierKey,
                    x.Supplier.DisplayName,
                    x.Supplier.UnitKind,
                    x.DocumentKey,
                    x.DocumentTypeKey,
                    x.Title,
                    x.ReviewStatus,
                    x.EffectiveAt,
                    x.ExpiresAt,
                    x.FileName,
                    x.ContentType,
                    x.SizeBytes,
                    x.UpdatedAt))
                .ToListAsync(cancellationToken);
            return Results.Ok(items);
        })
        .WithName("ListDocumentsV1");

        group.MapPost("/", async (
            CreateSupplyDocumentRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierComplianceDocumentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSupplierOnboardingManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.RegisterAsync(
                tenantId,
                actorUserId,
                request.SupplierId,
                new SupplierComplianceDocumentRegistrationRequest(
                    request.DocumentKey,
                    request.DocumentTypeKey,
                    request.Title,
                    request.EffectiveAt,
                    request.ExpiresAt,
                    request.FileName,
                    request.ContentType,
                    request.SizeBytes,
                    request.StorageUri),
                cancellationToken);
            return Results.Created($"/api/v1/documents/{created.DocumentId}", created);
        })
        .WithName("CreateDocumentV1");
    }

    private static void MapContracts(WebApplication app)
    {
        app.MapGet("/api/v1/contracts", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsRead(context.User);
            var tenantId = context.User.GetTenantId();

            var pricing = await db.PartSupplierPricingSnapshots
                .AsNoTracking()
                .Include(x => x.PartSupplierLink)
                .ThenInclude(x => x.Part)
                .Include(x => x.PartSupplierLink)
                .ThenInclude(x => x.Supplier)
                .Where(x => x.TenantId == tenantId && x.Source == SnapshotSources.Contract)
                .Select(x => new ContractSnapshotItemResponse(
                    "pricing",
                    x.Id,
                    x.PartSupplierLink.PartId,
                    x.PartSupplierLink.Part.PartKey,
                    x.PartSupplierLink.Part.DisplayName,
                    x.PartSupplierLink.SupplierId,
                    x.PartSupplierLink.Supplier.SupplierKey,
                    x.PartSupplierLink.Supplier.DisplayName,
                    x.SnapshotKey,
                    x.EffectiveFrom,
                    x.EffectiveTo,
                    x.UpdatedAt))
                .ToListAsync(cancellationToken);

            var leadTimes = await db.PartSupplierLeadTimeSnapshots
                .AsNoTracking()
                .Include(x => x.PartSupplierLink)
                .ThenInclude(x => x.Part)
                .Include(x => x.PartSupplierLink)
                .ThenInclude(x => x.Supplier)
                .Where(x => x.TenantId == tenantId && x.Source == SnapshotSources.Contract)
                .Select(x => new ContractSnapshotItemResponse(
                    "lead_time",
                    x.Id,
                    x.PartSupplierLink.PartId,
                    x.PartSupplierLink.Part.PartKey,
                    x.PartSupplierLink.Part.DisplayName,
                    x.PartSupplierLink.SupplierId,
                    x.PartSupplierLink.Supplier.SupplierKey,
                    x.PartSupplierLink.Supplier.DisplayName,
                    x.SnapshotKey,
                    x.EffectiveFrom,
                    x.EffectiveTo,
                    x.UpdatedAt))
                .ToListAsync(cancellationToken);

            var availability = await db.PartSupplierAvailabilitySnapshots
                .AsNoTracking()
                .Include(x => x.PartSupplierLink)
                .ThenInclude(x => x.Part)
                .Include(x => x.PartSupplierLink)
                .ThenInclude(x => x.Supplier)
                .Where(x => x.TenantId == tenantId && x.Source == SnapshotSources.Contract)
                .Select(x => new ContractSnapshotItemResponse(
                    "availability",
                    x.Id,
                    x.PartSupplierLink.PartId,
                    x.PartSupplierLink.Part.PartKey,
                    x.PartSupplierLink.Part.DisplayName,
                    x.PartSupplierLink.SupplierId,
                    x.PartSupplierLink.Supplier.SupplierKey,
                    x.PartSupplierLink.Supplier.DisplayName,
                    x.SnapshotKey,
                    x.EffectiveFrom,
                    x.EffectiveTo,
                    x.UpdatedAt))
                .ToListAsync(cancellationToken);

            return Results.Ok(pricing.Concat(leadTimes).Concat(availability).OrderByDescending(x => x.UpdatedAt));
        })
        .WithTags("Contracts")
        .RequireAuthorization()
        .WithName("ListContractsV1");
    }

    private static void MapImportsExports(WebApplication app)
    {
        app.MapGet("/api/v1/imports", (
            HttpContext context,
            SupplyArrAuthorizationService authorization) =>
        {
            authorization.RequirePartsManage(context.User);
            var items = ImportDescriptions
                .Select(entry => new ImportOptionResponse(entry.Key, entry.Value))
                .ToArray();
            return Results.Ok(items);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ListImportsV1");

        app.MapGet("/api/v1/imports/manifests", (
            HttpContext context,
            SupplyArrAuthorizationService authorization) =>
        {
            authorization.RequirePartsManage(context.User);
            var manifests = ImportDescriptions
                .Select(entry => BuildImportManifest(entry.Key, entry.Value))
                .ToArray();
            return Results.Ok(manifests);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ListImportManifestsV1");

        app.MapGet("/api/v1/imports/manifests/{importTypeKey}/template", (
            string importTypeKey,
            HttpContext context,
            SupplyArrAuthorizationService authorization) =>
        {
            authorization.RequirePartsManage(context.User);
            var normalizedImportType = NormalizeImportType(importTypeKey);
            if (!ImportHeaders.TryGetValue(normalizedImportType, out var headers))
            {
                return Results.NotFound();
            }

            var exampleRow = GetImportTemplateExampleRow(normalizedImportType);
            var builder = new StringBuilder();
            AppendCsvRow(builder, headers);
            AppendCsvRow(builder, exampleRow);
            return Results.File(
                Encoding.UTF8.GetBytes(builder.ToString()),
                "text/csv",
                $"supplyarr-{normalizedImportType.Replace('_', '-')}-template-v2026-06.csv");
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("DownloadImportTemplateV1");

        app.MapGet("/api/v1/imports/history", async (
            string? importType,
            int? limit,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var take = Math.Clamp(limit ?? 50, 1, 100);
            var query = db.AuditEvents
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.TargetType == "import");
            if (!string.IsNullOrWhiteSpace(importType))
            {
                var normalizedImportType = importType.Trim().ToLowerInvariant();
                query = query.Where(x => x.TargetId == normalizedImportType);
            }

            var events = await query
                .OrderByDescending(x => x.OccurredAt)
                .Take(take)
                .ToListAsync(cancellationToken);
            return Results.Ok(new ImportHistoryListResponse(events.Select(MapImportHistory).ToList()));
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ListImportHistoryV1");

        app.MapPost("/api/v1/imports/errors/export", async (
            ImportErrorExportRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ISupplyArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var importType = NormalizeImportType(request.ImportType);
            var builder = new StringBuilder();
            builder.AppendLine("importType,lineNumber,code,message");
            foreach (var issue in request.Issues.OrderBy(x => x.LineNumber).ThenBy(x => x.Code, StringComparer.OrdinalIgnoreCase))
            {
                builder.Append(CsvEscape(importType));
                builder.Append(',');
                builder.Append(issue.LineNumber);
                builder.Append(',');
                builder.Append(CsvEscape(issue.Code));
                builder.Append(',');
                builder.AppendLine(CsvEscape(issue.Message));
            }

            await audit.WriteAsync(
                "import.errors.export",
                tenantId,
                actorUserId,
                "import",
                importType,
                "success",
                reasonCode: $"issues:{request.Issues.Count}",
                cancellationToken: cancellationToken);
            var fileName = $"supplyarr-import-errors-{importType}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
            return Results.File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", fileName);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ExportImportErrorsV1");

        app.MapPost("/api/v1/imports/map-fields", (
            ImportFieldMappingRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization) =>
        {
            authorization.RequirePartsManage(context.User);
            var importType = NormalizeImportType(request.ImportType);
            if (!ImportHeaders.TryGetValue(importType, out var canonicalHeaders))
            {
                return Results.BadRequest(new ImportFieldMappingResponse(
                    importType,
                    false,
                    [],
                    [],
                    [],
                    [],
                    string.Empty));
            }

            var sourceRows = ParseCsvRows(request.Csv);
            if (sourceRows.Count == 0)
            {
                return Results.BadRequest(new ImportFieldMappingResponse(
                    importType,
                    false,
                    [],
                    canonicalHeaders,
                    canonicalHeaders,
                    [],
                    string.Empty));
            }

            var sourceHeaders = sourceRows[0].Select(x => x.Trim()).ToList();
            var mappedByCanonical = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var mappedSourceHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var mapping in request.FieldMappings)
            {
                var sourceHeader = mapping.Key.Trim();
                var canonicalHeader = mapping.Value.Trim().ToLowerInvariant();
                var sourceIndex = sourceHeaders.FindIndex(x => string.Equals(x, sourceHeader, StringComparison.OrdinalIgnoreCase));
                if (sourceIndex < 0 || !canonicalHeaders.Contains(canonicalHeader, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                mappedByCanonical[canonicalHeader] = sourceIndex;
                mappedSourceHeaders.Add(sourceHeaders[sourceIndex]);
            }

            for (var index = 0; index < sourceHeaders.Count; index++)
            {
                var sourceHeader = sourceHeaders[index].Trim().ToLowerInvariant();
                if (canonicalHeaders.Contains(sourceHeader, StringComparer.OrdinalIgnoreCase)
                    && !mappedByCanonical.ContainsKey(sourceHeader))
                {
                    mappedByCanonical[sourceHeader] = index;
                    mappedSourceHeaders.Add(sourceHeaders[index]);
                }
            }

            var missingRequiredHeaders = canonicalHeaders
                .Where(header => !mappedByCanonical.ContainsKey(header))
                .ToList();
            var unmappedSourceHeaders = sourceHeaders
                .Where(header => !mappedSourceHeaders.Contains(header))
                .ToList();
            var builder = new StringBuilder();
            builder.AppendLine(string.Join(',', canonicalHeaders.Select(CsvEscape)));
            foreach (var row in sourceRows.Skip(1))
            {
                for (var index = 0; index < canonicalHeaders.Length; index++)
                {
                    if (index > 0)
                    {
                        builder.Append(',');
                    }

                    if (mappedByCanonical.TryGetValue(canonicalHeaders[index], out var sourceIndex)
                        && sourceIndex < row.Count)
                    {
                        builder.Append(CsvEscape(row[sourceIndex]));
                    }
                }

                builder.AppendLine();
            }

            var response = new ImportFieldMappingResponse(
                importType,
                missingRequiredHeaders.Count == 0,
                sourceHeaders,
                canonicalHeaders,
                missingRequiredHeaders,
                unmappedSourceHeaders,
                builder.ToString());
            return response.Succeeded ? Results.Ok(response) : Results.BadRequest(response);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("MapImportFieldsV1");

        app.MapPost("/api/v1/imports/part-catalog-csv", async (
            PartCatalogCsvImportRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartCatalogCsvImportService service,
            ISupplyArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.ImportAsync(tenantId, actorUserId, request, cancellationToken);
            await WriteImportHistoryAsync(audit, tenantId, actorUserId, result.ImportType, result.DryRun, result.Succeeded, result.RowsRead, result.Issues.Count, cancellationToken);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ImportPartCatalogCsvV1");

        app.MapPost("/api/v1/imports/supplier-catalog-csv", async (
            SupplierCatalogCsvImportRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierCatalogCsvImportService service,
            ISupplyArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.ImportAsync(tenantId, actorUserId, request, cancellationToken);
            await WriteImportHistoryAsync(audit, tenantId, actorUserId, result.ImportType, result.DryRun, result.Succeeded, result.RowsRead, result.Issues.Count, cancellationToken);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ImportSupplierCatalogCsvV1");

        app.MapPost("/api/v1/imports/supplier-documents-csv", async (
            SupplierDocumentsCsvImportRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDocumentsCsvImportService service,
            ISupplyArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSupplierOnboardingManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.ImportAsync(tenantId, actorUserId, request, cancellationToken);
            await WriteImportHistoryAsync(audit, tenantId, actorUserId, result.ImportType, result.DryRun, result.Succeeded, result.RowsRead, result.Issues.Count, cancellationToken);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ImportSupplierDocumentsCsvV1");

        app.MapPost("/api/v1/imports/inventory-counts-csv", async (
            InventoryCountsCsvImportRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            InventoryCountsCsvImportService service,
            ISupplyArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.ImportAsync(tenantId, actorUserId, request, cancellationToken);
            await WriteImportHistoryAsync(audit, tenantId, actorUserId, result.ImportType, result.DryRun, result.Succeeded, result.RowsRead, result.Issues.Count, cancellationToken);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ImportInventoryCountsCsvV1");

        app.MapPost("/api/v1/imports/price-list-csv", async (
            PriceListCsvImportRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PriceListCsvImportService service,
            ISupplyArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.ImportAsync(tenantId, actorUserId, request, cancellationToken);
            await WriteImportHistoryAsync(audit, tenantId, actorUserId, result.ImportType, result.DryRun, result.Succeeded, result.RowsRead, result.Issues.Count, cancellationToken);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ImportPriceListCsvV1");

        app.MapPost("/api/v1/imports/lead-time-list-csv", async (
            LeadTimeListCsvImportRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            LeadTimeListCsvImportService service,
            ISupplyArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.ImportAsync(tenantId, actorUserId, request, cancellationToken);
            await WriteImportHistoryAsync(audit, tenantId, actorUserId, result.ImportType, result.DryRun, result.Succeeded, result.RowsRead, result.Issues.Count, cancellationToken);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ImportLeadTimeListCsvV1");

        app.MapPost("/api/v1/imports/availability-list-csv", async (
            AvailabilityListCsvImportRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            AvailabilityListCsvImportService service,
            ISupplyArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.ImportAsync(tenantId, actorUserId, request, cancellationToken);
            await WriteImportHistoryAsync(audit, tenantId, actorUserId, result.ImportType, result.DryRun, result.Succeeded, result.RowsRead, result.Issues.Count, cancellationToken);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ImportAvailabilityListCsvV1");

        app.MapPost("/api/v1/imports/contracts-csv", async (
            ContractsCsvImportRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ContractsCsvImportService service,
            ISupplyArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.ImportAsync(tenantId, actorUserId, request, cancellationToken);
            await WriteImportHistoryAsync(audit, tenantId, actorUserId, result.ImportType, result.DryRun, result.Succeeded, result.RowsRead, result.Issues.Count, cancellationToken);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ImportContractsCsvV1");

        app.MapPost("/api/v1/imports/suppliers-csv", async (
            SuppliersCsvImportRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SuppliersCsvImportService service,
            ISupplyArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.ImportAsync(tenantId, actorUserId, request, cancellationToken);
            await WriteImportHistoryAsync(audit, tenantId, actorUserId, result.ImportType, result.DryRun, result.Succeeded, result.RowsRead, result.Issues.Count, cancellationToken);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ImportSuppliersCsvV1");

        app.MapPost("/api/v1/imports/contacts-csv", async (
            ContactsCsvImportRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ContactsCsvImportService service,
            ISupplyArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.ImportAsync(tenantId, actorUserId, request, cancellationToken);
            await WriteImportHistoryAsync(audit, tenantId, actorUserId, result.ImportType, result.DryRun, result.Succeeded, result.RowsRead, result.Issues.Count, cancellationToken);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ImportContactsCsvV1");

        app.MapPost("/api/v1/imports/open-purchase-orders-csv", async (
            OpenPurchaseOrdersCsvImportRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            OpenPurchaseOrdersCsvImportService service,
            ISupplyArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseOrderCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId();
            var result = await service.ImportAsync(tenantId, actorUserId, actorPersonId, request, cancellationToken);
            await WriteImportHistoryAsync(audit, tenantId, actorUserId, result.ImportType, result.DryRun, result.Succeeded, result.RowsRead, result.Issues.Count, cancellationToken);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ImportOpenPurchaseOrdersCsvV1");

        app.MapPost("/api/v1/imports/purchase-history-csv", async (
            PurchaseHistoryCsvImportRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseHistoryCsvImportService service,
            ISupplyArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseOrderCreate(context.User);
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId();
            var result = await service.ImportAsync(tenantId, actorUserId, actorPersonId, request, cancellationToken);
            await WriteImportHistoryAsync(audit, tenantId, actorUserId, result.ImportType, result.DryRun, result.Succeeded, result.RowsRead, result.Issues.Count, cancellationToken);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ImportPurchaseHistoryCsvV1");

        app.MapGet("/api/v1/exports", (
            HttpContext context,
            SupplyArrAuthorizationService authorization) =>
        {
            authorization.RequirePartsRead(context.User);
            var items = new[]
            {
                new ExportOptionResponse("supplier_summary_csv", "Supplier report summary export", "/api/v1/reports/suppliers/summary/export"),
                new ExportOptionResponse("supplier_list_csv", "Supplier list export", "/api/v1/exports/suppliers.csv"),
                new ExportOptionResponse("approved_supplier_list_csv", "Approved supplier list export", "/api/v1/exports/approved-suppliers.csv"),
                new ExportOptionResponse("parts_catalog_csv", "Parts catalog export", "/api/v1/exports/parts-catalog.csv"),
                new ExportOptionResponse("inventory_valuation_csv", "Inventory valuation export", "/api/v1/exports/inventory-valuation.csv"),
                new ExportOptionResponse("purchase_orders_csv", "Purchase order line export", "/api/v1/exports/purchase-orders.csv"),
                new ExportOptionResponse("receipts_csv", "Receiving receipt line export", "/api/v1/exports/receipts.csv"),
                new ExportOptionResponse("invoice_support_csv", "Invoice-support export for accounting reconciliation", "/api/v1/exports/invoice-support.csv"),
                new ExportOptionResponse("supplier_document_report_csv", "Supplier document report export", "/api/v1/exports/supplier-documents.csv"),
                new ExportOptionResponse("compliance_evidence_packet_csv", "Procurement compliance evidence packet export", "/api/v1/exports/compliance-evidence-packet.csv"),
                new ExportOptionResponse("spend_report_csv", "Spend report export", "/api/v1/exports/spend.csv"),
                new ExportOptionResponse("parts_inventory_summary_csv", "Parts inventory summary export", "/api/v1/reports/parts-inventory/summary/export"),
                new ExportOptionResponse("purchasing_summary_csv", "Purchasing summary export", "/api/v1/reports/purchasing/summary/export"),
                new ExportOptionResponse("compliance_summary_csv", "Compliance summary export", "/api/v1/reports/compliance/summary/export")
            };
            return Results.Ok(items);
        })
        .WithTags("Exports")
        .RequireAuthorization()
        .WithName("ListExportsV1");

        static async Task<IResult> ExportSupplierListAsync(
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyArrDbContext db,
            CancellationToken cancellationToken,
            bool approvedOnly)
        {
            authorization.RequireSuppliersRead(context.User);
            var tenantId = context.User.GetTenantId();
            var suppliers = await db.Suppliers
                .AsNoTracking()
                .Include(x => x.ParentSupplier)
                .Where(x => x.TenantId == tenantId
                    && true
                    && (!approvedOnly || x.ApprovalStatus == "approved"))
                .OrderBy(x => x.SupplierKey)
                .ToListAsync(cancellationToken);
            var prefix = approvedOnly ? "supplyarr-approved-suppliers" : "supplyarr-suppliers";
            return Results.File(BuildSupplierListCsv(suppliers), "text/csv", $"{prefix}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv");
        }

        app.MapGet("/api/v1/exports/suppliers.csv", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyArrDbContext db,
            CancellationToken cancellationToken) =>
            await ExportSupplierListAsync(context, authorization, db, cancellationToken, approvedOnly: false))
        .WithTags("Exports")
        .RequireAuthorization()
        .WithName("ExportSupplierListV1");

        app.MapGet("/api/v1/exports/approved-suppliers.csv", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyArrDbContext db,
            CancellationToken cancellationToken) =>
            await ExportSupplierListAsync(context, authorization, db, cancellationToken, approvedOnly: true))
        .WithTags("Exports")
        .RequireAuthorization()
        .WithName("ExportApprovedSupplierListV1");

        app.MapGet("/api/v1/exports/parts-catalog.csv", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var parts = await db.Parts
                .AsNoTracking()
                .Include(x => x.PartCatalog)
                .OrderBy(x => x.PartKey)
                .Where(x => x.TenantId == tenantId)
                .ToListAsync(cancellationToken);
            return Results.File(BuildPartsCatalogCsv(parts), "text/csv", $"supplyarr-parts-catalog-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv");
        })
        .WithTags("Exports")
        .RequireAuthorization()
        .WithName("ExportPartsCatalogV1");

        app.MapGet("/api/v1/exports/inventory-valuation.csv", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var stockRows = await db.PartStockLevels
                .AsNoTracking()
                .Include(x => x.Part)
                .Include(x => x.InventoryBin)
                .ThenInclude(x => x!.InventoryLocation)
                .Where(x => x.TenantId == tenantId)
                .OrderBy(x => x.Part!.PartKey)
                .ThenBy(x => x.InventoryBin!.BinKey)
                .ToListAsync(cancellationToken);
            var unitPrices = await ResolveLatestPartPricesAsync(db, tenantId, stockRows.Select(x => x.PartId).Distinct().ToList(), cancellationToken);
            return Results.File(BuildInventoryValuationCsv(stockRows, unitPrices), "text/csv", $"supplyarr-inventory-valuation-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv");
        })
        .WithTags("Exports")
        .RequireAuthorization()
        .WithName("ExportInventoryValuationV1");

        app.MapGet("/api/v1/exports/purchase-orders.csv", async (
            string? status,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseOrderRead(context.User);
            var tenantId = context.User.GetTenantId();
            var query = db.PurchaseOrders
                .AsNoTracking()
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseRequest)
                .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
                .Where(x => x.TenantId == tenantId);
            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalizedStatus = status.Trim().ToLowerInvariant();
                query = query.Where(x => x.Status == normalizedStatus);
            }

            var orders = await query.OrderBy(x => x.OrderKey).ToListAsync(cancellationToken);
            return Results.File(BuildPurchaseOrdersCsv(orders), "text/csv", $"supplyarr-purchase-orders-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv");
        })
        .WithTags("Exports")
        .RequireAuthorization()
        .WithName("ExportPurchaseOrdersV1");

        app.MapGet("/api/v1/exports/receipts.csv", async (
            string? status,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingRead(context.User);
            var tenantId = context.User.GetTenantId();
            var query = db.ReceivingReceipts
                .AsNoTracking()
                .Include(x => x.PurchaseOrder)
                .ThenInclude(x => x.Supplier)
                .Include(x => x.InventoryBin)
                .ThenInclude(x => x.InventoryLocation)
                .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
                .Where(x => x.TenantId == tenantId);
            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalizedStatus = status.Trim().ToLowerInvariant();
                query = query.Where(x => x.Status == normalizedStatus);
            }

            var receipts = await query.OrderBy(x => x.ReceiptKey).ToListAsync(cancellationToken);
            return Results.File(BuildReceiptsCsv(receipts), "text/csv", $"supplyarr-receipts-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv");
        })
        .WithTags("Exports")
        .RequireAuthorization()
        .WithName("ExportReceiptsV1");

        app.MapGet("/api/v1/exports/invoice-support.csv", async (
            string? status,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingRead(context.User);
            var tenantId = context.User.GetTenantId();
            var query = db.ReceivingReceipts
                .AsNoTracking()
                .Include(x => x.PurchaseOrder)
                .ThenInclude(x => x.Supplier)
                .Include(x => x.PurchaseOrder)
                .ThenInclude(x => x.PurchaseRequest)
                .Include(x => x.InventoryBin)
                .ThenInclude(x => x.InventoryLocation)
                .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
                .Where(x => x.TenantId == tenantId);
            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalizedStatus = status.Trim().ToLowerInvariant();
                query = query.Where(x => x.Status == normalizedStatus);
            }

            var receipts = await query
                .OrderBy(x => x.ReceiptKey)
                .ToListAsync(cancellationToken);
            var partIds = receipts
                .SelectMany(x => x.Lines)
                .Select(x => x.PartId)
                .Distinct()
                .ToList();
            var unitPrices = await ResolveLatestPartPricesAsync(db, tenantId, partIds, cancellationToken);
            return Results.File(BuildInvoiceSupportCsv(receipts, unitPrices), "text/csv", $"supplyarr-invoice-support-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv");
        })
        .WithTags("Exports")
        .RequireAuthorization()
        .WithName("ExportInvoiceSupportV1");

        app.MapGet("/api/v1/exports/supplier-documents.csv", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSupplierOnboardingRead(context.User);
            var tenantId = context.User.GetTenantId();
            var documents = await db.SupplierComplianceDocuments
                .AsNoTracking()
                .Include(x => x.Supplier)
                .ThenInclude(x => x.ParentSupplier)
                .Where(x => x.TenantId == tenantId)
                .OrderBy(x => x.Supplier.SupplierKey)
                .ThenBy(x => x.DocumentKey)
                .ThenByDescending(x => x.Version)
                .ToListAsync(cancellationToken);
            return Results.File(BuildSupplierDocumentsCsv(documents), "text/csv", $"supplyarr-supplier-documents-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv");
        })
        .WithTags("Exports")
        .RequireAuthorization()
        .WithName("ExportSupplierDocumentsV1");

        app.MapGet("/api/v1/exports/compliance-evidence-packet.csv", async (
            Guid? supplierId,
            Guid? purchaseOrderId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireComplianceReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var ordersQuery = db.PurchaseOrders
                .AsNoTracking()
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseRequest)
                .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
                .Where(x => x.TenantId == tenantId);
            if (purchaseOrderId is not null)
            {
                ordersQuery = ordersQuery.Where(x => x.Id == purchaseOrderId.Value);
            }

            if (supplierId is not null)
            {
                ordersQuery = ordersQuery.Where(x => x.SupplierId == supplierId.Value);
            }

            var orders = await ordersQuery
                .OrderBy(x => x.OrderKey)
                .ToListAsync(cancellationToken);
            var scopedSupplierIds = orders.Select(x => x.SupplierId).Distinct().ToList();
            if (supplierId is not null && !scopedSupplierIds.Contains(supplierId.Value))
            {
                scopedSupplierIds.Add(supplierId.Value);
            }

            var orderIds = orders.Select(x => x.Id).ToList();
            var receipts = await db.ReceivingReceipts
                .AsNoTracking()
                .Include(x => x.PurchaseOrder)
                .ThenInclude(x => x.Supplier)
                .Include(x => x.InventoryBin)
                .ThenInclude(x => x.InventoryLocation)
                .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
                .Where(x => x.TenantId == tenantId && orderIds.Contains(x.PurchaseOrderId))
                .OrderBy(x => x.ReceiptKey)
                .ToListAsync(cancellationToken);
            IReadOnlyList<SupplierComplianceDocument> documents = scopedSupplierIds.Count == 0
                ? []
                : await db.SupplierComplianceDocuments
                    .AsNoTracking()
                    .Include(x => x.Supplier)
                    .Where(x => x.TenantId == tenantId && scopedSupplierIds.Contains(x.SupplierId))
                    .OrderBy(x => x.Supplier.SupplierKey)
                    .ThenBy(x => x.DocumentKey)
                    .ToListAsync(cancellationToken);
            var targetIds = orderIds.Select(x => x.ToString("D")).ToList();
            targetIds.AddRange(receipts.Select(x => x.Id.ToString("D")));
            targetIds.AddRange(documents.Select(x => x.Id.ToString("D")));
            IReadOnlyList<SupplyArrAuditEvent> auditEvents = targetIds.Count == 0
                ? []
                : await db.AuditEvents
                    .AsNoTracking()
                    .Where(x => x.TenantId == tenantId && x.TargetId != null && targetIds.Contains(x.TargetId))
                    .OrderByDescending(x => x.OccurredAt)
                    .Take(100)
                    .ToListAsync(cancellationToken);
            return Results.File(BuildComplianceEvidencePacketCsv(orders, receipts, documents, auditEvents), "text/csv", $"supplyarr-compliance-evidence-packet-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv");
        })
        .WithTags("Exports")
        .RequireAuthorization()
        .WithName("ExportComplianceEvidencePacketV1");

        app.MapGet("/api/v1/exports/spend.csv", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseOrderRead(context.User);
            var tenantId = context.User.GetTenantId();
            var orders = await db.PurchaseOrders
                .AsNoTracking()
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseRequest)
                .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
                .Where(x => x.TenantId == tenantId)
                .OrderBy(x => x.OrderKey)
                .ToListAsync(cancellationToken);
            var partIds = orders.SelectMany(x => x.Lines).Select(x => x.PartId).Distinct().ToList();
            var unitPrices = await ResolveLatestPartPricesAsync(db, tenantId, partIds, cancellationToken);
            return Results.File(BuildSpendCsv(orders, unitPrices), "text/csv", $"supplyarr-spend-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv");
        })
        .WithTags("Exports")
        .RequireAuthorization()
        .WithName("ExportSpendV1");
    }

    private static void MapAdmin(WebApplication app)
    {
        app.MapGet("/api/v1/admin", (
            HttpContext context,
            SupplyArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationEventSettingsManage(context.User);
            var response = new AdminOverviewResponse(
                "supplyarr",
                context.User.GetTenantRoleKey(),
                context.User.IsPlatformAdmin(),
                SupplyArrSuiteLaunchCatalog.OrdinaryProductKeys,
                ["settings", "integration-event-settings", "notification-settings", "audit-history"]);
            return Results.Ok(response);
        })
        .WithTags("Admin")
        .RequireAuthorization()
        .WithName("GetAdminOverviewV1");
    }

    private static Task WriteImportHistoryAsync(
        ISupplyArrAuditService audit,
        Guid tenantId,
        Guid actorUserId,
        string importType,
        bool dryRun,
        bool succeeded,
        int rowsRead,
        int issueCount,
        CancellationToken cancellationToken) =>
        audit.WriteAsync(
            dryRun ? "import.preview" : "import.commit",
            tenantId,
            actorUserId,
            "import",
            importType,
            succeeded ? "success" : "failed",
            reasonCode: $"rows:{rowsRead};issues:{issueCount}",
            cancellationToken: cancellationToken);

    private static string NormalizeImportType(string importType) =>
        string.IsNullOrWhiteSpace(importType) ? "unknown" : importType.Trim().ToLowerInvariant();

    private static ImportHistoryItemResponse MapImportHistory(SupplyArrAuditEvent auditEvent)
    {
        var (rowsRead, issueCount) = ParseImportHistoryReason(auditEvent.ReasonCode);
        return new ImportHistoryItemResponse(
            auditEvent.Id,
            auditEvent.TargetId ?? string.Empty,
            string.Equals(auditEvent.Action, "import.preview", StringComparison.OrdinalIgnoreCase),
            string.Equals(auditEvent.Result, "success", StringComparison.OrdinalIgnoreCase),
            rowsRead,
            issueCount,
            auditEvent.ActorUserId,
            auditEvent.OccurredAt);
    }

    private static ProductImportManifestResponse BuildImportManifest(string importTypeKey, string description)
    {
        var requiredColumns = GetRequiredColumns(importTypeKey);
        var headers = ImportHeaders[importTypeKey];
        var optionalColumns = headers
            .Except(requiredColumns, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var controlledVocabularyColumns = headers
            .Where(header =>
                header.Contains("status", StringComparison.OrdinalIgnoreCase)
                || header.Contains("type", StringComparison.OrdinalIgnoreCase)
                || header.Contains("terms", StringComparison.OrdinalIgnoreCase)
                || header.Contains("currency", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var referenceColumns = GetReferenceColumns(importTypeKey);

        return new ProductImportManifestResponse(
            "supplyarr",
            importTypeKey,
            ToDisplayName(importTypeKey),
            description,
            ["csv"],
            "2026-06",
            $"supplyarr.import.{importTypeKey.Replace("_csv", string.Empty, StringComparison.Ordinal)}",
            GetTargetEntity(importTypeKey),
            GetAllowedOperations(importTypeKey),
            requiredColumns,
            optionalColumns,
            controlledVocabularyColumns,
            referenceColumns,
            GetUniquenessRules(importTypeKey),
            GetDuplicateDetectionRules(importTypeKey),
            [
                "Required field missing",
                "Invalid type or format",
                "Invalid enum or controlled vocabulary value",
                "Unknown referenced record",
                "Duplicate in uploaded file",
                "Duplicate against existing SupplyArr records"
            ],
            headers.Take(6).ToArray(),
            GetCommitBehavior(importTypeKey),
            GetEmittedEvents(importTypeKey),
            false,
            $"supplyarr.{importTypeKey}.import");
    }

    private static (int RowsRead, int IssueCount) ParseImportHistoryReason(string? reasonCode)
    {
        var rowsRead = 0;
        var issueCount = 0;
        if (string.IsNullOrWhiteSpace(reasonCode))
        {
            return (rowsRead, issueCount);
        }

        foreach (var part in reasonCode.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = part.IndexOf(':', StringComparison.Ordinal);
            if (separator <= 0 || separator == part.Length - 1)
            {
                continue;
            }

            var key = part[..separator];
            var value = part[(separator + 1)..];
            if (string.Equals(key, "rows", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(value, out var parsedRows))
            {
                rowsRead = parsedRows;
            }
            else if (string.Equals(key, "issues", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(value, out var parsedIssues))
            {
                issueCount = parsedIssues;
            }
        }

        return (rowsRead, issueCount);
    }

    private static IReadOnlyList<string> GetRequiredColumns(string importTypeKey) =>
        NormalizeImportType(importTypeKey) switch
        {
            "part_catalog_csv" => ["catalog_key", "catalog_name", "part_key", "part_name"],
            "supplier_catalog_csv" => ["supplier_key", "part_key", "supplier_part_number"],
            "supplier_documents_csv" => ["supplier_key", "document_key", "document_type_key", "title"],
            "inventory_counts_csv" => ["part_key", "location_key", "quantity_on_hand"],
            "price_list_csv" => ["supplier_key", "part_key", "snapshot_key", "unit_price", "currency_code"],
            "lead_time_list_csv" => ["supplier_key", "part_key", "snapshot_key", "lead_time_days"],
            "availability_list_csv" => ["supplier_key", "part_key", "snapshot_key", "availability_status"],
            "contracts_csv" => ["supplier_key", "contract_key", "contract_type", "title", "effective_from", "status"],
            "suppliers_csv" => ["supplier_key", "display_name", "status"],
            "contacts_csv" => ["supplier_key", "contact_name", "email"],
            "open_purchase_orders_csv" => ["order_key", "supplier_key", "part_key", "quantity_ordered"],
            "purchase_history_csv" => ["order_key", "receipt_key", "supplier_key", "part_key", "quantity_received"],
            _ => ImportHeaders[NormalizeImportType(importTypeKey)].Take(4).ToArray()
        };

    private static IReadOnlyList<string> GetReferenceColumns(string importTypeKey) =>
        NormalizeImportType(importTypeKey) switch
        {
            "part_catalog_csv" => ["catalog_key"],
            "supplier_catalog_csv" => ["supplier_key", "part_key"],
            "supplier_documents_csv" => ["supplier_key"],
            "inventory_counts_csv" => ["part_key", "location_key", "bin_key"],
            "price_list_csv" => ["supplier_key", "part_key"],
            "lead_time_list_csv" => ["supplier_key", "part_key"],
            "availability_list_csv" => ["supplier_key", "part_key"],
            "contracts_csv" => ["supplier_key"],
            "suppliers_csv" => ["parent_supplier_key", "service_types"],
            "contacts_csv" => ["supplier_key"],
            "open_purchase_orders_csv" => ["request_key", "supplier_key", "part_key"],
            "purchase_history_csv" => ["request_key", "supplier_key", "part_key", "inventory_bin_key"],
            _ => []
        };

    private static IReadOnlyList<string> GetAllowedOperations(string importTypeKey) =>
        NormalizeImportType(importTypeKey) switch
        {
            "inventory_counts_csv" => ["adjust"],
            "price_list_csv" or "lead_time_list_csv" or "availability_list_csv" => ["create"],
            "purchase_history_csv" => ["reference-only", "create"],
            _ => ["create"]
        };

    private static IReadOnlyList<string> GetUniquenessRules(string importTypeKey) =>
        NormalizeImportType(importTypeKey) switch
        {
            "part_catalog_csv" => ["catalog_key + part_key"],
            "supplier_catalog_csv" => ["supplier_key + part_key + supplier_part_number"],
            "supplier_documents_csv" => ["supplier_key + document_key"],
            "inventory_counts_csv" => ["part_key + location_key + bin_key"],
            "price_list_csv" => ["supplier_key + part_key + snapshot_key"],
            "lead_time_list_csv" => ["supplier_key + part_key + snapshot_key"],
            "availability_list_csv" => ["supplier_key + part_key + snapshot_key"],
            "contracts_csv" => ["supplier_key + contract_key"],
            "suppliers_csv" => ["supplier_key"],
            "contacts_csv" => ["supplier_key + email"],
            "open_purchase_orders_csv" => ["order_key + part_key + supplier_key"],
            "purchase_history_csv" => ["order_key + receipt_key + part_key"],
            _ => []
        };

    private static IReadOnlyList<string> GetDuplicateDetectionRules(string importTypeKey) =>
        NormalizeImportType(importTypeKey) switch
        {
            "part_catalog_csv" => ["part_key", "manufacturer_name + manufacturer_part_number"],
            "supplier_catalog_csv" => ["supplier_key + part_key + supplier_part_number"],
            "supplier_documents_csv" => ["supplier_key + document_key"],
            "inventory_counts_csv" => ["part_key + location_key + bin_key"],
            "price_list_csv" => ["supplier_key + part_key + snapshot_key"],
            "lead_time_list_csv" => ["supplier_key + part_key + snapshot_key"],
            "availability_list_csv" => ["supplier_key + part_key + snapshot_key"],
            "contracts_csv" => ["supplier_key + contract_key", "title + supplier_key"],
            "suppliers_csv" => ["supplier_key", "display_name + parent_supplier_key"],
            "contacts_csv" => ["supplier_key + email"],
            "open_purchase_orders_csv" => ["order_key + part_key"],
            "purchase_history_csv" => ["order_key + receipt_key + part_key"],
            _ => []
        };

    private static string GetTargetEntity(string importTypeKey) =>
        NormalizeImportType(importTypeKey) switch
        {
            "part_catalog_csv" => "part_catalog",
            "supplier_catalog_csv" => "part_supplier_link",
            "supplier_documents_csv" => "supplier_compliance_document",
            "inventory_counts_csv" => "inventory_count",
            "price_list_csv" => "part_supplier_pricing_snapshot",
            "lead_time_list_csv" => "part_supplier_lead_time_snapshot",
            "availability_list_csv" => "part_supplier_availability_snapshot",
            "contracts_csv" => "supply_contract",
            "suppliers_csv" => "supplier",
            "contacts_csv" => "supplier_contact",
            "open_purchase_orders_csv" => "purchase_order",
            "purchase_history_csv" => "purchase_history",
            _ => "supply_import"
        };

    private static string GetCommitBehavior(string importTypeKey) =>
        NormalizeImportType(importTypeKey) switch
        {
            "inventory_counts_csv" => "Applies inventory quantity adjustments through SupplyArr inventory services.",
            "purchase_history_csv" => "Creates historical purchase and receipt records through normal procurement services.",
            "contracts_csv" => "Registers contract metadata and linked supplier references without bypassing RecordArr ownership for files.",
            _ => "Creates or stages SupplyArr records through normal product services after deterministic CSV validation."
        };

    private static IReadOnlyList<string> GetEmittedEvents(string importTypeKey) =>
        NormalizeImportType(importTypeKey) switch
        {
            "contracts_csv" => ["supplyarr.contract.imported"],
            "suppliers_csv" => ["supplyarr.supplier.created"],
            "contacts_csv" => ["supplyarr.supplier_contact.created"],
            "open_purchase_orders_csv" => ["supplyarr.purchase_order.imported"],
            "purchase_history_csv" => ["supplyarr.purchase_order.imported", "supplyarr.receipt.imported"],
            "inventory_counts_csv" => ["supplyarr.inventory_count.imported"],
            _ => ["supplyarr.import.completed"]
        };

    private static string ToDisplayName(string importTypeKey) =>
        NormalizeImportType(importTypeKey) switch
        {
            "part_catalog_csv" => "Part catalog",
            "supplier_catalog_csv" => "Supplier catalog",
            "supplier_documents_csv" => "Supplier documents",
            "inventory_counts_csv" => "Inventory counts",
            "price_list_csv" => "Price list",
            "lead_time_list_csv" => "Lead time list",
            "availability_list_csv" => "Availability list",
            "contracts_csv" => "Contracts",
            "suppliers_csv" => "Supplier directory",
            "contacts_csv" => "Supplier contacts",
            "open_purchase_orders_csv" => "Open purchase orders",
            "purchase_history_csv" => "Purchase history",
            _ => importTypeKey.Replace('_', ' ')
        };

    private static string[] GetImportTemplateExampleRow(string importTypeKey) =>
        NormalizeImportType(importTypeKey) switch
        {
            "part_catalog_csv" => ["CAT-100", "Core parts", "Primary stocked parts", "PART-100", "Brake pad", "Fleet brake pad", "brakes", "each", "Acme", "ACM-100"],
            "supplier_catalog_csv" => ["SUP-100", "PART-100", "SUP-100-PART-100", "true", "29.95", "USD", "10", "7", "120", "in_stock"],
            "supplier_documents_csv" => ["SUP-100", "DOC-100", "insurance_certificate", "General liability certificate", "2026-01-01T00:00:00Z", "2027-01-01T00:00:00Z", "insurance.pdf", "application/pdf", "24576", "recordarr://documents/doc-100"],
            "inventory_counts_csv" => ["PART-100", "WH1", "BIN-A1", "42"],
            "price_list_csv" => ["SUP-100", "PART-100", "PRICE-2026-01", "29.95", "USD", "10", "2026-01-01T00:00:00Z", "supplier_portal", "Annual pricing refresh"],
            "lead_time_list_csv" => ["SUP-100", "PART-100", "LEAD-2026-01", "7", "2026-01-01T00:00:00Z", "supplier_portal", "Standard lead time"],
            "availability_list_csv" => ["SUP-100", "PART-100", "AVAIL-2026-01", "120", "in_stock", "2026-01-01T00:00:00Z", "supplier_portal", "Warehouse availability"],
            "contracts_csv" => ["SUP-100", "CON-2026-01", "master_supply_agreement", "Supply agreement 2026", "2026-01-01T00:00:00Z", "2026-12-31T00:00:00Z", "2026-11-01T00:00:00Z", "Net 30", "FOB destination", "12 months", "25000", "95% on-time", "approved", "active", "Priority supplier"],
            "suppliers_csv" => ["SUP-100-KC", "SUP-100", "sub_unit", "Acme Supply Kansas City", "Acme Supply LLC", "12-3456789", "approved", "active", "Regional brake supplier", "parts|maintenance"],
            "contacts_csv" => ["SUP-100", "Morgan Lee", "morgan.lee@example.com", "555-0100", "Account manager", "true"],
            "open_purchase_orders_csv" => ["PO-100", "PR-100", "SUP-100", "PART-100", "25", "Brake pads replenishment", "Line note", "Order note"],
            "purchase_history_csv" => ["PO-100", "PR-100", "RCV-100", "SUP-100", "PART-100", "25", "25", "BIN-A1", "Brake pads replenishment", "Line note", "Order note", "Receipt note"],
            _ => ImportHeaders[NormalizeImportType(importTypeKey)].Select(_ => string.Empty).ToArray()
        };

    private static string CsvEscape(string value)
    {
        if (value.Contains('"', StringComparison.Ordinal)
            || value.Contains(',', StringComparison.Ordinal)
            || value.Contains('\n', StringComparison.Ordinal)
            || value.Contains('\r', StringComparison.Ordinal))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }

    private static byte[] BuildSupplierListCsv(IReadOnlyList<Supplier> suppliers)
    {
        var builder = new StringBuilder();
        builder.AppendLine("supplierKey,parentSupplierKey,displayName,legalName,taxIdentifier,approvalStatus,status,unitKind,serviceTypes,addressLine1,addressLine2,locality,regionCode,postalCode,countryCode,createdAt,updatedAt");
        foreach (var supplier in suppliers)
        {
            AppendCsvRow(
                builder,
                supplier.SupplierKey,
                supplier.ParentSupplier?.SupplierKey ?? string.Empty,
                supplier.DisplayName,
                supplier.LegalName,
                supplier.TaxIdentifier ?? string.Empty,
                supplier.ApprovalStatus,
                supplier.Status,
                supplier.UnitKind,
                string.Join('|', ParseServiceTypes(supplier.ServiceTypesJson)),
                supplier.AddressLine1,
                supplier.AddressLine2,
                supplier.Locality,
                supplier.RegionCode,
                supplier.PostalCode,
                supplier.CountryCode,
                supplier.CreatedAt.ToString("O"),
                supplier.UpdatedAt.ToString("O"));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static IReadOnlyList<string> ParseServiceTypes(string? serviceTypesJson)
    {
        if (string.IsNullOrWhiteSpace(serviceTypesJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(serviceTypesJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static byte[] BuildPartsCatalogCsv(IReadOnlyList<Part> parts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("partKey,catalogKey,displayName,description,categoryKey,unitOfMeasure,manufacturerName,manufacturerPartNumber,status,reorderPoint,reorderQuantity,createdAt,updatedAt");
        foreach (var part in parts)
        {
            AppendCsvRow(
                builder,
                part.PartKey,
                part.PartCatalog?.CatalogKey ?? string.Empty,
                part.DisplayName,
                part.Description,
                part.CategoryKey,
                part.UnitOfMeasure,
                part.ManufacturerName,
                part.ManufacturerPartNumber,
                part.Status,
                part.ReorderPoint?.ToString() ?? string.Empty,
                part.ReorderQuantity?.ToString() ?? string.Empty,
                part.CreatedAt.ToString("O"),
                part.UpdatedAt.ToString("O"));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static byte[] BuildInventoryValuationCsv(
        IReadOnlyList<PartStockLevel> stockRows,
        IReadOnlyDictionary<Guid, decimal> unitPrices)
    {
        var builder = new StringBuilder();
        builder.AppendLine("partKey,partDisplayName,locationKey,binKey,quantityOnHand,quantityReserved,quantityAvailable,unitPrice,extendedValue,updatedAt");
        foreach (var stock in stockRows)
        {
            var unitPrice = unitPrices.GetValueOrDefault(stock.PartId);
            var available = stock.QuantityOnHand - stock.QuantityReserved;
            AppendCsvRow(
                builder,
                stock.Part?.PartKey ?? string.Empty,
                stock.Part?.DisplayName ?? string.Empty,
                stock.InventoryBin?.InventoryLocation?.LocationKey ?? string.Empty,
                stock.InventoryBin?.BinKey ?? string.Empty,
                stock.QuantityOnHand.ToString(),
                stock.QuantityReserved.ToString(),
                available.ToString(),
                unitPrice.ToString(),
                (stock.QuantityOnHand * unitPrice).ToString(),
                stock.UpdatedAt.ToString("O"));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static byte[] BuildPurchaseOrdersCsv(IReadOnlyList<PurchaseOrder> orders)
    {
        var builder = new StringBuilder();
        builder.AppendLine("orderKey,status,requestKey,supplierKey,supplierDisplayName,title,lineNumber,partKey,partDisplayName,quantityOrdered,quantityReceived,quantityRemaining,unitOfMeasure,lineNotes,approvedAt,issuedAt,createdAt,updatedAt");
        foreach (var order in orders)
        {
            foreach (var line in order.Lines.OrderBy(x => x.LineNumber))
            {
                AppendCsvRow(
                    builder,
                    order.OrderKey,
                    order.Status,
                    order.PurchaseRequest.RequestKey,
                    order.Supplier.SupplierKey,
                    order.Supplier.DisplayName,
                    order.Title,
                    line.LineNumber.ToString(),
                    line.Part.PartKey,
                    line.Part.DisplayName,
                    line.QuantityOrdered.ToString(),
                    line.QuantityReceived.ToString(),
                    (line.QuantityOrdered - line.QuantityReceived).ToString(),
                    line.Part.UnitOfMeasure,
                    line.Notes,
                    order.ApprovedAt?.ToString("O") ?? string.Empty,
                    order.IssuedAt?.ToString("O") ?? string.Empty,
                    order.CreatedAt.ToString("O"),
                    order.UpdatedAt.ToString("O"));
            }
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static byte[] BuildReceiptsCsv(IReadOnlyList<ReceivingReceipt> receipts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("receiptKey,status,orderKey,supplierKey,locationKey,binKey,lineNumber,partKey,partDisplayName,quantityExpected,quantityReceived,postedAt,notes,createdAt,updatedAt");
        foreach (var receipt in receipts)
        {
            foreach (var line in receipt.Lines.OrderBy(x => x.LineNumber))
            {
                AppendCsvRow(
                    builder,
                    receipt.ReceiptKey,
                    receipt.Status,
                    receipt.PurchaseOrder.OrderKey,
                    receipt.PurchaseOrder.Supplier.SupplierKey,
                    receipt.InventoryBin.InventoryLocation?.LocationKey ?? string.Empty,
                    receipt.InventoryBin.BinKey,
                    line.LineNumber.ToString(),
                    line.Part.PartKey,
                    line.Part.DisplayName,
                    line.QuantityExpected.ToString(),
                    line.QuantityReceived.ToString(),
                    receipt.PostedAt?.ToString("O") ?? string.Empty,
                    receipt.Notes,
                    receipt.CreatedAt.ToString("O"),
                    receipt.UpdatedAt.ToString("O"));
            }
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static byte[] BuildInvoiceSupportCsv(
        IReadOnlyList<ReceivingReceipt> receipts,
        IReadOnlyDictionary<Guid, decimal> unitPrices)
    {
        var builder = new StringBuilder();
        builder.AppendLine("receiptKey,receiptStatus,postedAt,orderKey,orderStatus,requestKey,supplierKey,supplierDisplayName,locationKey,binKey,lineNumber,partKey,partDisplayName,quantityReceived,unitOfMeasure,unitPrice,receivedAmount,receiptNotes,orderIssuedAt");
        foreach (var receipt in receipts)
        {
            foreach (var line in receipt.Lines.OrderBy(x => x.LineNumber))
            {
                var unitPrice = unitPrices.GetValueOrDefault(line.PartId);
                AppendCsvRow(
                    builder,
                    receipt.ReceiptKey,
                    receipt.Status,
                    receipt.PostedAt?.ToString("O") ?? string.Empty,
                    receipt.PurchaseOrder.OrderKey,
                    receipt.PurchaseOrder.Status,
                    receipt.PurchaseOrder.PurchaseRequest.RequestKey,
                    receipt.PurchaseOrder.Supplier.SupplierKey,
                    receipt.PurchaseOrder.Supplier.DisplayName,
                    receipt.InventoryBin.InventoryLocation?.LocationKey ?? string.Empty,
                    receipt.InventoryBin.BinKey,
                    line.LineNumber.ToString(),
                    line.Part.PartKey,
                    line.Part.DisplayName,
                    line.QuantityReceived.ToString(),
                    line.Part.UnitOfMeasure,
                    unitPrice.ToString(),
                    (line.QuantityReceived * unitPrice).ToString(),
                    receipt.Notes,
                    receipt.PurchaseOrder.IssuedAt?.ToString("O") ?? string.Empty);
            }
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static byte[] BuildSupplierDocumentsCsv(IReadOnlyList<SupplierComplianceDocument> documents)
    {
        var builder = new StringBuilder();
        builder.AppendLine("supplierKey,parentSupplierKey,supplierDisplayName,supplierUnitKind,documentKey,documentTypeKey,title,version,reviewStatus,effectiveAt,expiresAt,fileName,contentType,sizeBytes,reviewedAt,createdAt,updatedAt");
        foreach (var document in documents)
        {
            AppendCsvRow(
                builder,
                document.Supplier.SupplierKey,
                document.Supplier.ParentSupplier?.SupplierKey ?? string.Empty,
                document.Supplier.DisplayName,
                document.Supplier.UnitKind,
                document.DocumentKey,
                document.DocumentTypeKey,
                document.Title,
                document.Version.ToString(),
                document.ReviewStatus,
                document.EffectiveAt?.ToString("O") ?? string.Empty,
                document.ExpiresAt?.ToString("O") ?? string.Empty,
                document.FileName,
                document.ContentType,
                document.SizeBytes.ToString(),
                document.ReviewedAt?.ToString("O") ?? string.Empty,
                document.CreatedAt.ToString("O"),
                document.UpdatedAt.ToString("O"));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static byte[] BuildSpendCsv(
        IReadOnlyList<PurchaseOrder> orders,
        IReadOnlyDictionary<Guid, decimal> unitPrices)
    {
        var builder = new StringBuilder();
        builder.AppendLine("orderKey,status,requestKey,supplierKey,supplierDisplayName,lineNumber,partKey,partDisplayName,quantityOrdered,quantityReceived,unitPrice,orderedAmount,receivedAmount,issuedAt,createdAt");
        foreach (var order in orders)
        {
            foreach (var line in order.Lines.OrderBy(x => x.LineNumber))
            {
                var unitPrice = unitPrices.GetValueOrDefault(line.PartId);
                AppendCsvRow(
                    builder,
                    order.OrderKey,
                    order.Status,
                    order.PurchaseRequest.RequestKey,
                    order.Supplier.SupplierKey,
                    order.Supplier.DisplayName,
                    line.LineNumber.ToString(),
                    line.Part.PartKey,
                    line.Part.DisplayName,
                    line.QuantityOrdered.ToString(),
                    line.QuantityReceived.ToString(),
                    unitPrice.ToString(),
                    (line.QuantityOrdered * unitPrice).ToString(),
                    (line.QuantityReceived * unitPrice).ToString(),
                    order.IssuedAt?.ToString("O") ?? string.Empty,
                    order.CreatedAt.ToString("O"));
            }
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static byte[] BuildComplianceEvidencePacketCsv(
        IReadOnlyList<PurchaseOrder> orders,
        IReadOnlyList<ReceivingReceipt> receipts,
        IReadOnlyList<SupplierComplianceDocument> documents,
        IReadOnlyList<SupplyArrAuditEvent> auditEvents)
    {
        var builder = new StringBuilder();
        builder.AppendLine("recordType,entityType,entityKey,status,relatedKey,description,evidenceAt,sourcePath");
        foreach (var order in orders)
        {
            AppendCsvRow(
                builder,
                "purchase_order",
                "purchase_order",
                order.OrderKey,
                order.Status,
                order.PurchaseRequest.RequestKey,
                $"{order.Supplier.SupplierKey} · {order.Title}",
                (order.IssuedAt ?? order.UpdatedAt).ToString("O"),
                $"/api/v1/purchase-orders/{order.Id}");
            foreach (var line in order.Lines.OrderBy(x => x.LineNumber))
            {
                AppendCsvRow(
                    builder,
                    "purchase_order_line",
                    "purchase_order",
                    order.OrderKey,
                    order.Status,
                    line.Part.PartKey,
                    $"{line.QuantityOrdered} {line.Part.UnitOfMeasure} ordered · {line.QuantityReceived} received",
                    line.UpdatedAt.ToString("O"),
                    $"/api/v1/purchase-orders/{order.Id}");
            }
        }

        foreach (var receipt in receipts)
        {
            AppendCsvRow(
                builder,
                "receipt",
                "receiving_receipt",
                receipt.ReceiptKey,
                receipt.Status,
                receipt.PurchaseOrder.OrderKey,
                $"{receipt.PurchaseOrder.Supplier.SupplierKey} · {receipt.InventoryBin.BinKey}",
                (receipt.PostedAt ?? receipt.UpdatedAt).ToString("O"),
                $"/api/v1/receipts/{receipt.Id}");
            foreach (var line in receipt.Lines.OrderBy(x => x.LineNumber))
            {
                AppendCsvRow(
                    builder,
                    "receipt_line",
                    "receiving_receipt",
                    receipt.ReceiptKey,
                    receipt.Status,
                    line.Part.PartKey,
                    $"{line.QuantityReceived} {line.Part.UnitOfMeasure} received",
                    line.UpdatedAt.ToString("O"),
                    $"/api/v1/receipts/{receipt.Id}");
            }
        }

        foreach (var document in documents)
        {
            AppendCsvRow(
                builder,
                "supplier_document",
                "supplier_compliance_document",
                document.DocumentKey,
                document.ReviewStatus,
                document.Supplier.SupplierKey,
                $"{document.DocumentTypeKey} · {document.Title}",
                (document.ReviewedAt ?? document.UpdatedAt).ToString("O"),
                $"/api/v1/documents?supplierId={document.SupplierId}");
        }

        foreach (var auditEvent in auditEvents)
        {
            AppendCsvRow(
                builder,
                "audit_event",
                auditEvent.TargetType,
                auditEvent.TargetId ?? string.Empty,
                auditEvent.Result,
                auditEvent.Action,
                auditEvent.ReasonCode ?? string.Empty,
                auditEvent.OccurredAt.ToString("O"),
                $"/api/v1/audit-history?targetType={auditEvent.TargetType}&targetId={auditEvent.TargetId}");
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static async Task<IReadOnlyDictionary<Guid, decimal>> ResolveLatestPartPricesAsync(
        SupplyArrDbContext db,
        Guid tenantId,
        IReadOnlyList<Guid> partIds,
        CancellationToken cancellationToken)
    {
        if (partIds.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        var snapshotPrices = await db.PartSupplierPricingSnapshots
            .AsNoTracking()
            .Include(x => x.PartSupplierLink)
            .Where(x => x.TenantId == tenantId && partIds.Contains(x.PartSupplierLink.PartId))
            .OrderByDescending(x => x.EffectiveFrom)
            .ThenByDescending(x => x.UpdatedAt)
            .Select(x => new { x.PartSupplierLink.PartId, x.UnitPrice })
            .ToListAsync(cancellationToken);
        var prices = snapshotPrices
            .GroupBy(x => x.PartId)
            .ToDictionary(x => x.Key, x => x.First().UnitPrice);

        var missingPartIds = partIds.Where(x => !prices.ContainsKey(x)).ToList();
        if (missingPartIds.Count == 0)
        {
            return prices;
        }

        var catalogPrices = await db.PartSupplierLinks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && missingPartIds.Contains(x.PartId)
                && x.CatalogUnitPrice != null)
            .OrderByDescending(x => x.IsPreferred)
            .ThenByDescending(x => x.UpdatedAt)
            .Select(x => new { x.PartId, UnitPrice = x.CatalogUnitPrice!.Value })
            .ToListAsync(cancellationToken);
        foreach (var price in catalogPrices.GroupBy(x => x.PartId).Select(x => x.First()))
        {
            prices[price.PartId] = price.UnitPrice;
        }

        return prices;
    }

    private static void AppendCsvRow(StringBuilder builder, params string[] values)
    {
        for (var index = 0; index < values.Length; index++)
        {
            if (index > 0)
            {
                builder.Append(',');
            }

            builder.Append(CsvEscape(values[index]));
        }

        builder.AppendLine();
    }

    private static IReadOnlyList<IReadOnlyList<string>> ParseCsvRows(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return [];
        }

        return csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(ParseCsvRow)
            .ToList();
    }

    private static IReadOnlyList<string> ParseCsvRow(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (inQuotes)
            {
                if (character == '"')
                {
                    if (index + 1 < line.Length && line[index + 1] == '"')
                    {
                        current.Append('"');
                        index++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(character);
                }

                continue;
            }

            if (character == '"')
            {
                inQuotes = true;
                continue;
            }

            if (character == ',')
            {
                fields.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        fields.Add(current.ToString().Trim());
        return fields;
    }
}



