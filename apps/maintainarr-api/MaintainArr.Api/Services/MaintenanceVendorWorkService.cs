using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class MaintenanceVendorWorkService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit)
{
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

        if (request.WorkOrderId == Guid.Empty)
        {
            throw new StlApiException("supplier_work_status.validation", "Work order id is required.", 400);
        }

        var workOrderExists = await db.WorkOrders.AnyAsync(
            x => x.TenantId == tenantId && x.Id == request.WorkOrderId,
            cancellationToken);
        if (!workOrderExists)
        {
            throw new StlApiException("work_orders.not_found", "Work order was not found.", 404);
        }

        var supplierRef = Require(request.SupplierRef, "supplier_work_status.supplier_ref_required", 128);
        var status = Require(request.Status, "supplier_work_status.status_required", 32).ToLowerInvariant();
        if (!MaintenanceVendorWorkStatuses.All.Contains(status))
        {
            throw new StlApiException(
                "supplier_work_status.invalid_status",
                "Supplier work status is not recognized.",
                400);
        }

        var workDescription = Normalize(request.WorkDescription, 1024);
        var quoteRecordRef = Normalize(request.QuoteRecordRef, 256);
        var approvalRef = Normalize(request.ApprovalRef, 256);
        var costEstimateSnapshot = Normalize(request.CostEstimateSnapshot, 256);
        var invoiceRecordRef = Normalize(request.InvoiceRecordRef, 256);
        var vendorContactSnapshot = Normalize(request.VendorContactSnapshot, 512);
        var notes = Normalize(request.Notes, 1024);

        var now = DateTimeOffset.UtcNow;
        var entity = await db.MaintenanceVendorWorks
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.WorkOrderId == request.WorkOrderId
                    && x.SupplierRef == supplierRef,
                cancellationToken);

        var duplicate = entity is not null;
        if (entity is null)
        {
            entity = new MaintenanceVendorWork
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WorkOrderId = request.WorkOrderId,
                SupplierRef = supplierRef,
                CreatedAt = now,
                UpdatedAt = now,
            };
            db.MaintenanceVendorWorks.Add(entity);
        }

        entity.VendorContactSnapshot = vendorContactSnapshot;
        entity.Status = status;
        entity.WorkDescription = workDescription;
        entity.QuoteRecordRef = quoteRecordRef;
        entity.ApprovalRef = approvalRef;
        entity.ScheduledAt = request.ScheduledAt;
        entity.CompletedAt = request.CompletedAt;
        entity.CostEstimateSnapshot = costEstimateSnapshot;
        entity.InvoiceRecordRef = invoiceRecordRef;
        entity.WarrantyFlag = request.WarrantyFlag;
        entity.Notes = notes;
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
