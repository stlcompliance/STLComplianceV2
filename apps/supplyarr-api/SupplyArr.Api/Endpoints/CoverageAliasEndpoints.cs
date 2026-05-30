using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class CoverageAliasEndpoints
{
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
            Guid? partyId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSupplierOnboardingRead(context.User);
            var tenantId = context.User.GetTenantId();
            var query = db.PartyComplianceDocuments
                .AsNoTracking()
                .Include(x => x.ExternalParty)
                .Where(x => x.TenantId == tenantId);
            if (partyId.HasValue)
            {
                query = query.Where(x => x.ExternalPartyId == partyId.Value);
            }

            var items = await query
                .OrderByDescending(x => x.UpdatedAt)
                .Select(x => new SupplyDocumentItemResponse(
                    x.Id,
                    x.ExternalPartyId,
                    x.ExternalParty.PartyKey,
                    x.ExternalParty.DisplayName,
                    x.ExternalParty.PartyType,
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
            PartyComplianceDocumentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSupplierOnboardingManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.RegisterAsync(
                tenantId,
                actorUserId,
                request.PartyId,
                new RegisterPartyComplianceDocumentRequest(
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

            var pricing = await db.PartVendorPricingSnapshots
                .AsNoTracking()
                .Include(x => x.PartVendorLink)
                .ThenInclude(x => x.Part)
                .Include(x => x.PartVendorLink)
                .ThenInclude(x => x.ExternalParty)
                .Where(x => x.TenantId == tenantId && x.Source == SnapshotSources.Contract)
                .Select(x => new ContractSnapshotItemResponse(
                    "pricing",
                    x.Id,
                    x.PartVendorLink.PartId,
                    x.PartVendorLink.Part.PartKey,
                    x.PartVendorLink.Part.DisplayName,
                    x.PartVendorLink.ExternalPartyId,
                    x.PartVendorLink.ExternalParty.PartyKey,
                    x.PartVendorLink.ExternalParty.DisplayName,
                    x.SnapshotKey,
                    x.EffectiveFrom,
                    x.EffectiveTo,
                    x.UpdatedAt))
                .ToListAsync(cancellationToken);

            var leadTimes = await db.PartVendorLeadTimeSnapshots
                .AsNoTracking()
                .Include(x => x.PartVendorLink)
                .ThenInclude(x => x.Part)
                .Include(x => x.PartVendorLink)
                .ThenInclude(x => x.ExternalParty)
                .Where(x => x.TenantId == tenantId && x.Source == SnapshotSources.Contract)
                .Select(x => new ContractSnapshotItemResponse(
                    "lead_time",
                    x.Id,
                    x.PartVendorLink.PartId,
                    x.PartVendorLink.Part.PartKey,
                    x.PartVendorLink.Part.DisplayName,
                    x.PartVendorLink.ExternalPartyId,
                    x.PartVendorLink.ExternalParty.PartyKey,
                    x.PartVendorLink.ExternalParty.DisplayName,
                    x.SnapshotKey,
                    x.EffectiveFrom,
                    x.EffectiveTo,
                    x.UpdatedAt))
                .ToListAsync(cancellationToken);

            var availability = await db.PartVendorAvailabilitySnapshots
                .AsNoTracking()
                .Include(x => x.PartVendorLink)
                .ThenInclude(x => x.Part)
                .Include(x => x.PartVendorLink)
                .ThenInclude(x => x.ExternalParty)
                .Where(x => x.TenantId == tenantId && x.Source == SnapshotSources.Contract)
                .Select(x => new ContractSnapshotItemResponse(
                    "availability",
                    x.Id,
                    x.PartVendorLink.PartId,
                    x.PartVendorLink.Part.PartKey,
                    x.PartVendorLink.Part.DisplayName,
                    x.PartVendorLink.ExternalPartyId,
                    x.PartVendorLink.ExternalParty.PartyKey,
                    x.PartVendorLink.ExternalParty.DisplayName,
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
            var items = new[]
            {
                new ImportOptionResponse("part_catalog_csv", "Import part catalogs and parts from CSV."),
                new ImportOptionResponse("vendor_documents_csv", "Import vendor compliance document metadata.")
            };
            return Results.Ok(items);
        })
        .WithTags("Imports")
        .RequireAuthorization()
        .WithName("ListImportsV1");

        app.MapGet("/api/v1/exports", (
            HttpContext context,
            SupplyArrAuthorizationService authorization) =>
        {
            authorization.RequirePartsRead(context.User);
            var items = new[]
            {
                new ExportOptionResponse("vendors_summary_csv", "Vendor report summary export", "/api/v1/reports/vendors/summary/export"),
                new ExportOptionResponse("parts_inventory_summary_csv", "Parts inventory summary export", "/api/v1/reports/parts-inventory/summary/export"),
                new ExportOptionResponse("purchasing_summary_csv", "Purchasing summary export", "/api/v1/reports/purchasing/summary/export"),
                new ExportOptionResponse("compliance_summary_csv", "Compliance summary export", "/api/v1/reports/compliance/summary/export")
            };
            return Results.Ok(items);
        })
        .WithTags("Exports")
        .RequireAuthorization()
        .WithName("ListExportsV1");
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
                context.User.GetEntitlements(),
                ["settings", "integration-event-settings", "notification-settings", "audit-history"]);
            return Results.Ok(response);
        })
        .WithTags("Admin")
        .RequireAuthorization()
        .WithName("GetAdminOverviewV1");
    }
}

