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

        return new MaintenanceVendorWorkResponse(
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
            entity.CreatedAt,
            entity.UpdatedAt,
            duplicate);
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
            entity.CreatedAt,
            entity.UpdatedAt,
            false);

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

    private static string Require(string? value, string code, int maxLength)
    {
        var normalized = Normalize(value, maxLength);
        if (normalized is null)
        {
            throw new StlApiException(code, "A value is required.", 400);
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
