using System.Security.Cryptography;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class MaintenanceVendorWorkService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit,
    MaintenancePlatformOutboxEnqueueService platformOutboxEnqueueService)
{
    public async Task<MaintenanceVendorWorkListResponse> ListAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        var exists = await db.WorkOrders.AnyAsync(
            x => x.TenantId == tenantId && x.Id == workOrderId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException("work_orders.not_found", "Work order was not found.", 404);
        }

        var entities = await db.MaintenanceVendorWorks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId)
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var items = entities.Select(Map).ToArray();
        return new MaintenanceVendorWorkListResponse(items);
    }

    public async Task<MaintenanceVendorWorkResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        IngestSupplierWorkStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.TenantId != tenantId)
        {
            throw new StlApiException(
                "supplier_work_status.tenant_mismatch",
                "Request tenant does not match the integration tenant.",
                400);
        }

        return await UpsertCoreAsync(
            tenantId,
            actorUserId,
            request.WorkOrderId,
            request.SupplierRef,
            request.VendorContactSnapshot,
            request.Status,
            request.WorkDescription,
            request.QuoteRecordRef,
            request.ApprovalRef,
            request.ScheduledAt,
            request.CompletedAt,
            request.CostEstimateSnapshot,
            request.InvoiceRecordRef,
            request.WarrantyFlag,
            request.Notes,
            request.OccurredAt,
            cancellationToken);
    }

    public async Task<MaintenanceVendorWorkResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        UpsertMaintenanceVendorWorkRequest request,
        CancellationToken cancellationToken = default)
    {
        return await UpsertCoreAsync(
            tenantId,
            actorUserId,
            workOrderId,
            request.SupplierRef,
            request.VendorContactSnapshot,
            request.Status,
            request.WorkDescription,
            request.QuoteRecordRef,
            request.ApprovalRef,
            request.ScheduledAt,
            request.CompletedAt,
            request.CostEstimateSnapshot,
            request.InvoiceRecordRef,
            request.WarrantyFlag,
            request.Notes,
            DateTimeOffset.UtcNow,
            cancellationToken);
    }

    public async Task<MaintenanceVendorWorkResponse> IssuePortalAccessAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        Guid vendorWorkId,
        CancellationToken cancellationToken = default)
    {
        var vendorWork = await LoadVendorWorkAsync(tenantId, workOrderId, vendorWorkId, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        vendorWork.PortalAccessCode = GeneratePortalAccessCode();
        vendorWork.PortalAccessCodeIssuedAt = now;
        vendorWork.PortalAccessExpiresAt = now.AddDays(14);
        vendorWork.PortalAccessStatus = MaintenanceVendorWorkPortalAccessStatuses.Sent;
        vendorWork.PortalAccessOpenedAt = null;
        vendorWork.PortalAccessRevokedAt = null;
        vendorWork.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintenance_vendor_work.portal_access_issued",
            tenantId,
            actorUserId,
            "maintenance_vendor_work",
            vendorWork.Id.ToString(),
            vendorWork.PortalAccessStatus,
            cancellationToken: cancellationToken);

        await EnqueuePortalAccessEventAsync(
            tenantId,
            actorUserId,
            vendorWork,
            workOrderId,
            MaintenancePlatformOutboxEventKinds.MaintenanceVendorWorkPortalAccessIssued,
            $"Vendor portal invitation issued for supplier {vendorWork.SupplierRef} on work order {workOrderId:D}.",
            vendorWork.PortalAccessStatus,
            "issued",
            now,
            cancellationToken);

        return Map(vendorWork);
    }

    public async Task<MaintenanceVendorWorkResponse> RevokePortalAccessAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        Guid vendorWorkId,
        CancellationToken cancellationToken = default)
    {
        var vendorWork = await LoadVendorWorkAsync(tenantId, workOrderId, vendorWorkId, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        vendorWork.PortalAccessStatus = MaintenanceVendorWorkPortalAccessStatuses.Revoked;
        vendorWork.PortalAccessRevokedAt = now;
        vendorWork.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintenance_vendor_work.portal_access_revoked",
            tenantId,
            actorUserId,
            "maintenance_vendor_work",
            vendorWork.Id.ToString(),
            vendorWork.PortalAccessStatus,
            cancellationToken: cancellationToken);

        await EnqueuePortalAccessEventAsync(
            tenantId,
            actorUserId,
            vendorWork,
            workOrderId,
            MaintenancePlatformOutboxEventKinds.MaintenanceVendorWorkPortalAccessRevoked,
            $"Vendor portal invitation revoked for supplier {vendorWork.SupplierRef} on work order {workOrderId:D}.",
            vendorWork.PortalAccessStatus,
            "revoked",
            now,
            cancellationToken);

        return Map(vendorWork);
    }

    public async Task<MaintenanceVendorWorkPortalResponse> GetPortalAsync(
        Guid workOrderId,
        string accessCode,
        CancellationToken cancellationToken = default)
    {
        var vendorWork = await ResolvePortalAccessAsync(workOrderId, accessCode, markOpened: true, cancellationToken);
        var workOrder = await LoadWorkOrderAsync(vendorWork.TenantId, workOrderId, cancellationToken);
        var asset = await LoadAssetAsync(vendorWork.TenantId, workOrder.AssetId, cancellationToken);
        return MapPortal(vendorWork, workOrder, asset);
    }

    public async Task<MaintenanceVendorWorkPortalResponse> UpdatePortalAsync(
        Guid workOrderId,
        string accessCode,
        UpdateMaintenanceVendorWorkPortalRequest request,
        CancellationToken cancellationToken = default)
    {
        var vendorWork = await ResolvePortalAccessAsync(workOrderId, accessCode, markOpened: true, cancellationToken);
        var workOrder = await LoadWorkOrderAsync(vendorWork.TenantId, workOrderId, cancellationToken);
        var asset = await LoadAssetAsync(vendorWork.TenantId, workOrder.AssetId, cancellationToken);
        var previousStatus = vendorWork.Status;
        var now = DateTimeOffset.UtcNow;

        var normalizedStatus = RequirePortalStatus(request.Status);
        vendorWork.Status = normalizedStatus;
        vendorWork.ScheduledAt = request.ScheduledAt;
        vendorWork.CompletedAt = request.CompletedAt ?? (string.Equals(normalizedStatus, "completed", StringComparison.OrdinalIgnoreCase) ? now : vendorWork.CompletedAt);
        vendorWork.Notes = Normalize(request.Notes, 1024);
        vendorWork.PortalAccessStatus = MaintenanceVendorWorkPortalAccessStatuses.Used;
        vendorWork.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintenance_vendor_work.portal_status_updated",
            vendorWork.TenantId,
            null,
            "maintenance_vendor_work",
            vendorWork.Id.ToString(),
            vendorWork.Status,
            cancellationToken: cancellationToken);

        await EnqueueVendorWorkEventsAsync(
            vendorWork.TenantId,
            Guid.Empty,
            vendorWork,
            workOrderId,
            previousStatus,
            duplicate: true,
            now,
            cancellationToken);

        return MapPortal(vendorWork, workOrder, asset);
    }

    private async Task<MaintenanceVendorWorkResponse> UpsertCoreAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        string supplierRef,
        string? vendorContactSnapshot,
        string status,
        string? workDescription,
        string? quoteRecordRef,
        string? approvalRef,
        DateTimeOffset? scheduledAt,
        DateTimeOffset? completedAt,
        string? costEstimateSnapshot,
        string? invoiceRecordRef,
        bool warrantyFlag,
        string? notes,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        if (workOrderId == Guid.Empty)
        {
            throw new StlApiException("supplier_work_status.validation", "Work order id is required.", 400);
        }

        var workOrderExists = await db.WorkOrders.AnyAsync(
            x => x.TenantId == tenantId && x.Id == workOrderId,
            cancellationToken);
        if (!workOrderExists)
        {
            throw new StlApiException("work_orders.not_found", "Work order was not found.", 404);
        }

        var normalizedSupplierRef = Require(supplierRef, "supplier_work_status.supplier_ref_required", 128);
        var normalizedStatus = Require(status, "supplier_work_status.status_required", 32).ToLowerInvariant();
        if (!MaintenanceVendorWorkStatuses.All.Contains(normalizedStatus))
        {
            throw new StlApiException(
                "supplier_work_status.invalid_status",
                "Supplier work status is not recognized.",
                400);
        }

        var normalizedWorkDescription = Normalize(workDescription, 1024);
        var normalizedQuoteRecordRef = Normalize(quoteRecordRef, 256);
        var normalizedApprovalRef = Normalize(approvalRef, 256);
        var normalizedCostEstimateSnapshot = Normalize(costEstimateSnapshot, 256);
        var normalizedInvoiceRecordRef = Normalize(invoiceRecordRef, 256);
        var normalizedVendorContactSnapshot = Normalize(vendorContactSnapshot, 512);
        var normalizedNotes = Normalize(notes, 1024);

        var entity = await db.MaintenanceVendorWorks
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.WorkOrderId == workOrderId
                    && x.SupplierRef == normalizedSupplierRef,
                cancellationToken);

        var duplicate = entity is not null;
        var previousStatus = entity?.Status;
        var now = occurredAt;
        if (entity is null)
        {
            entity = new MaintenanceVendorWork
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WorkOrderId = workOrderId,
                SupplierRef = normalizedSupplierRef,
                CreatedAt = now,
                UpdatedAt = now,
            };
            db.MaintenanceVendorWorks.Add(entity);
        }

        entity.VendorContactSnapshot = normalizedVendorContactSnapshot;
        entity.Status = normalizedStatus;
        entity.WorkDescription = normalizedWorkDescription;
        entity.QuoteRecordRef = normalizedQuoteRecordRef;
        entity.ApprovalRef = normalizedApprovalRef;
        entity.ScheduledAt = scheduledAt;
        entity.CompletedAt = completedAt;
        entity.CostEstimateSnapshot = normalizedCostEstimateSnapshot;
        entity.InvoiceRecordRef = normalizedInvoiceRecordRef;
        entity.WarrantyFlag = warrantyFlag;
        entity.Notes = normalizedNotes;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintenance_vendor_work.upsert",
            tenantId,
            actorUserId,
            "maintenance_vendor_work",
            entity.Id.ToString(),
            entity.Status,
            cancellationToken: cancellationToken);

        await EnqueueVendorWorkEventsAsync(
            tenantId,
            actorUserId,
            entity,
            workOrderId,
            previousStatus,
            duplicate,
            now,
            cancellationToken);

        return Map(entity);
    }

    private static MaintenanceVendorWorkResponse Map(MaintenanceVendorWork entity) =>
        new(
            entity.Id,
            entity.WorkOrderId,
            entity.SupplierRef,
            entity.VendorContactSnapshot,
            entity.Status,
            entity.WorkDescription,
            entity.QuoteRecordRef,
            entity.ApprovalRef,
            entity.ScheduledAt,
            entity.CompletedAt,
            entity.CostEstimateSnapshot,
            entity.InvoiceRecordRef,
            entity.WarrantyFlag,
            entity.Notes,
            entity.PortalAccessCode,
            entity.PortalAccessCodeIssuedAt,
            entity.PortalAccessExpiresAt,
            entity.PortalAccessOpenedAt,
            entity.PortalAccessRevokedAt,
            ResolvePortalAccessStatus(entity, DateTimeOffset.UtcNow),
            BuildPortalUrl(entity.WorkOrderId, entity.PortalAccessCode),
            entity.CreatedAt,
            entity.UpdatedAt,
            false);

    private static MaintenanceVendorWorkPortalResponse MapPortal(
        MaintenanceVendorWork vendorWork,
        WorkOrder workOrder,
        Asset asset)
        => new(
            vendorWork.Id,
            vendorWork.WorkOrderId,
            workOrder.WorkOrderNumber,
            workOrder.Title,
            workOrder.Priority,
            workOrder.Status,
            asset.Id,
            asset.AssetTag,
            asset.Name,
            vendorWork.SupplierRef,
            vendorWork.VendorContactSnapshot,
            vendorWork.Status,
            vendorWork.WorkDescription,
            vendorWork.QuoteRecordRef,
            vendorWork.ApprovalRef,
            vendorWork.ScheduledAt,
            vendorWork.CompletedAt,
            vendorWork.CostEstimateSnapshot,
            vendorWork.InvoiceRecordRef,
            vendorWork.WarrantyFlag,
            vendorWork.Notes,
            vendorWork.PortalAccessExpiresAt,
            ResolvePortalAccessStatus(vendorWork, DateTimeOffset.UtcNow),
            AllowedPortalActions(vendorWork),
            vendorWork.CreatedAt,
            vendorWork.UpdatedAt);

    private async Task<MaintenanceVendorWork> LoadVendorWorkAsync(
        Guid tenantId,
        Guid workOrderId,
        Guid vendorWorkId,
        CancellationToken cancellationToken)
    {
        var vendorWork = await db.MaintenanceVendorWorks
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.WorkOrderId == workOrderId
                    && x.Id == vendorWorkId,
                cancellationToken);

        return vendorWork ?? throw new StlApiException("maintenance_vendor_work.not_found", "Vendor work was not found.", 404);
    }

    private async Task<WorkOrder> LoadWorkOrderAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken)
    {
        var workOrder = await db.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrderId, cancellationToken);

        return workOrder ?? throw new StlApiException("work_orders.not_found", "Work order was not found.", 404);
    }

    private async Task<Asset> LoadAssetAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken)
    {
        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken);

        return asset ?? throw new StlApiException("assets.not_found", "Asset was not found.", 404);
    }

    private async Task<MaintenanceVendorWork> ResolvePortalAccessAsync(
        Guid workOrderId,
        string accessCode,
        bool markOpened,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accessCode))
        {
            throw new StlApiException(
                "maintenance_vendor_work.portal_access_code_required",
                "Vendor portal access code is required.",
                401);
        }

        var normalizedAccessCode = accessCode.Trim();
        var vendorWork = await db.MaintenanceVendorWorks
            .FirstOrDefaultAsync(
                x => x.WorkOrderId == workOrderId && x.PortalAccessCode == normalizedAccessCode,
                cancellationToken);

        if (vendorWork is null)
        {
            throw new StlApiException(
                "maintenance_vendor_work.portal_access_invalid",
                "Vendor portal access code was not recognized.",
                401);
        }

        var now = DateTimeOffset.UtcNow;
        var effectiveStatus = ResolvePortalAccessStatus(vendorWork, now);
        if (string.Equals(effectiveStatus, MaintenanceVendorWorkPortalAccessStatuses.Expired, StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(vendorWork.PortalAccessStatus, MaintenanceVendorWorkPortalAccessStatuses.Expired, StringComparison.OrdinalIgnoreCase))
            {
                vendorWork.PortalAccessStatus = MaintenanceVendorWorkPortalAccessStatuses.Expired;
                vendorWork.UpdatedAt = now;
                await db.SaveChangesAsync(cancellationToken);
                await audit.WriteAsync(
                    "maintenance_vendor_work.portal_access_expired",
                    vendorWork.TenantId,
                    null,
                    "maintenance_vendor_work",
                    vendorWork.Id.ToString(),
                    vendorWork.PortalAccessStatus,
                    cancellationToken: cancellationToken);
            }

            throw new StlApiException(
                "maintenance_vendor_work.portal_access_expired",
                "Vendor portal access has expired.",
                401);
        }

        if (string.Equals(effectiveStatus, MaintenanceVendorWorkPortalAccessStatuses.Revoked, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "maintenance_vendor_work.portal_access_revoked",
                "Vendor portal access has been revoked.",
                401);
        }

        if (markOpened
            && !string.Equals(vendorWork.PortalAccessStatus, MaintenanceVendorWorkPortalAccessStatuses.Opened, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(vendorWork.PortalAccessStatus, MaintenanceVendorWorkPortalAccessStatuses.Used, StringComparison.OrdinalIgnoreCase))
        {
            vendorWork.PortalAccessStatus = MaintenanceVendorWorkPortalAccessStatuses.Opened;
            vendorWork.PortalAccessOpenedAt ??= now;
            vendorWork.UpdatedAt = now;
            await db.SaveChangesAsync(cancellationToken);

            await audit.WriteAsync(
                "maintenance_vendor_work.portal_access_opened",
                vendorWork.TenantId,
                null,
                "maintenance_vendor_work",
                vendorWork.Id.ToString(),
                vendorWork.PortalAccessStatus,
                cancellationToken: cancellationToken);

            await EnqueuePortalAccessEventAsync(
                vendorWork.TenantId,
                null,
                vendorWork,
                workOrderId,
                MaintenancePlatformOutboxEventKinds.MaintenanceVendorWorkPortalAccessOpened,
                $"Vendor portal invitation opened for supplier {vendorWork.SupplierRef} on work order {workOrderId:D}.",
                vendorWork.PortalAccessStatus,
                "opened",
                now,
                cancellationToken);
        }

        return vendorWork;
    }

    private async Task EnqueueVendorWorkEventsAsync(
        Guid tenantId,
        Guid actorUserId,
        MaintenanceVendorWork vendorWork,
        Guid workOrderId,
        string? previousStatus,
        bool duplicate,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var workOrder = await db.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == workOrderId,
                cancellationToken);

        if (workOrder is null)
        {
            return;
        }

        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == workOrder.AssetId,
                cancellationToken);

        if (asset is null)
        {
            return;
        }

        if (!duplicate)
        {
            await platformOutboxEnqueueService.TryEnqueueVendorWorkEventAsync(
                tenantId,
                MaintenancePlatformOutboxEventKinds.MaintenanceVendorWorkCreated,
                workOrder,
                asset,
                vendorWork,
                actorUserId,
                occurredAt,
                $"Vendor work created for supplier {vendorWork.SupplierRef} on work order {workOrder.WorkOrderNumber}.",
                eventResult: vendorWork.Status,
                idempotencyDiscriminator: "created",
                cancellationToken: cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(previousStatus)
            && !string.Equals(previousStatus, vendorWork.Status, StringComparison.OrdinalIgnoreCase))
        {
            await platformOutboxEnqueueService.TryEnqueueVendorWorkEventAsync(
                tenantId,
                MaintenancePlatformOutboxEventKinds.MaintenanceVendorWorkStatusChanged,
                workOrder,
                asset,
                vendorWork,
                actorUserId,
                occurredAt,
                $"Vendor work status changed for supplier {vendorWork.SupplierRef} on work order {workOrder.WorkOrderNumber}.",
                eventResult: vendorWork.Status,
                idempotencyDiscriminator: $"{previousStatus ?? "none"}->{vendorWork.Status}",
                cancellationToken: cancellationToken);
        }

        if (string.Equals(vendorWork.Status, "completed", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(previousStatus, "completed", StringComparison.OrdinalIgnoreCase))
        {
            await platformOutboxEnqueueService.TryEnqueueVendorWorkEventAsync(
                tenantId,
                MaintenancePlatformOutboxEventKinds.MaintenanceVendorWorkCompleted,
                workOrder,
                asset,
                vendorWork,
                actorUserId,
                occurredAt,
                $"Vendor work completed for supplier {vendorWork.SupplierRef} on work order {workOrder.WorkOrderNumber}.",
                eventResult: vendorWork.Status,
                idempotencyDiscriminator: $"{previousStatus ?? "none"}->completed",
                cancellationToken: cancellationToken);
        }
    }

    private async Task EnqueuePortalAccessEventAsync(
        Guid tenantId,
        Guid? actorUserId,
        MaintenanceVendorWork vendorWork,
        Guid workOrderId,
        string eventKind,
        string summary,
        string? eventResult,
        string idempotencyDiscriminator,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var workOrder = await db.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == workOrderId,
                cancellationToken);

        if (workOrder is null)
        {
            return;
        }

        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == workOrder.AssetId,
                cancellationToken);

        if (asset is null)
        {
            return;
        }

        await platformOutboxEnqueueService.TryEnqueueVendorWorkEventAsync(
            tenantId,
            eventKind,
            workOrder,
            asset,
            vendorWork,
            actorUserId ?? Guid.Empty,
            occurredAt,
            summary,
            eventResult: eventResult,
            idempotencyDiscriminator: idempotencyDiscriminator,
            cancellationToken: cancellationToken);
    }

    private static string Require(string? value, string code, int maxLength)
    {
        var normalized = Normalize(value, maxLength);
        if (normalized is null)
        {
            throw new StlApiException(code, "A value is required.", 400);
        }

        return normalized;
    }

    private static string RequirePortalStatus(string? value)
    {
        var normalized = Normalize(value, 32);
        if (normalized is null)
        {
            throw new StlApiException("maintenance_vendor_work.portal_status_required", "A portal status is required.", 400);
        }

        normalized = normalized.ToLowerInvariant();
        if (!MaintenanceVendorWorkPortalUpdateStatuses.All.Contains(normalized))
        {
            throw new StlApiException(
                "maintenance_vendor_work.portal_status_invalid",
                "Portal status is not recognized.",
                400);
        }

        return normalized;
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException("supplier_work_status.validation", $"Value must be {maxLength} characters or fewer.", 400);
        }

        return normalized;
    }

    private static string GeneratePortalAccessCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string? BuildPortalUrl(Guid workOrderId, string? accessCode)
        => string.IsNullOrWhiteSpace(accessCode)
            ? null
            : $"/vendor-portal/work-orders/{workOrderId:D}?accessCode={Uri.EscapeDataString(accessCode)}";

    private static string ResolvePortalAccessStatus(MaintenanceVendorWork entity, DateTimeOffset now)
    {
        if (string.Equals(entity.PortalAccessStatus, MaintenanceVendorWorkPortalAccessStatuses.Revoked, StringComparison.OrdinalIgnoreCase))
        {
            return MaintenanceVendorWorkPortalAccessStatuses.Revoked;
        }

        if (string.Equals(entity.PortalAccessStatus, MaintenanceVendorWorkPortalAccessStatuses.Expired, StringComparison.OrdinalIgnoreCase))
        {
            return MaintenanceVendorWorkPortalAccessStatuses.Expired;
        }

        if (entity.PortalAccessExpiresAt is not null && entity.PortalAccessExpiresAt <= now)
        {
            return MaintenanceVendorWorkPortalAccessStatuses.Expired;
        }

        return entity.PortalAccessStatus;
    }

    private static IReadOnlyList<string> AllowedPortalActions(MaintenanceVendorWork vendorWork)
    {
        if (string.Equals(vendorWork.PortalAccessStatus, MaintenanceVendorWorkPortalAccessStatuses.Revoked, StringComparison.OrdinalIgnoreCase)
            || string.Equals(vendorWork.PortalAccessStatus, MaintenanceVendorWorkPortalAccessStatuses.Expired, StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        if (string.Equals(vendorWork.Status, "completed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(vendorWork.Status, "rejected", StringComparison.OrdinalIgnoreCase)
            || string.Equals(vendorWork.Status, "canceled", StringComparison.OrdinalIgnoreCase))
        {
            return ["view_limited_status"];
        }

        return ["view_limited_status", "submit_status_update", "confirm_completion"];
    }
}

public static class MaintenanceVendorWorkStatuses
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "requested",
        "quoted",
        "approved",
        "scheduled",
        "in_progress",
        "completed",
        "rejected",
        "canceled",
    };
}

public static class MaintenanceVendorWorkPortalUpdateStatuses
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "scheduled",
        "in_progress",
        "completed",
        "rejected",
        "canceled",
    };
}
