using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class FieldInboxService(SupplyArrDbContext db)
{
    public async Task<FieldInboxResponse> GetAsync(
        Guid tenantId,
        Guid actorUserId,
        bool viewAll,
        CancellationToken cancellationToken = default)
    {
        var query = db.ReceivingReceipts
            .AsNoTracking()
            .Include(x => x.PurchaseOrder)
            .Where(x => x.TenantId == tenantId && x.Status == ReceivingReceiptStatuses.Draft);

        if (!viewAll)
        {
            query = query.Where(x => x.CreatedByUserId == actorUserId);
        }

        var receipts = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        var items = receipts.Select(receipt => new FieldInboxTaskItem(
            $"supplyarr:receiving:{receipt.Id:D}",
            "supplyarr",
            "receiving",
            receipt.ReceiptKey,
            receipt.PurchaseOrder?.OrderKey,
            receipt.Status,
            null,
            null,
            receipt.UpdatedAt,
            $"/receiving/{receipt.Id:D}")).ToList();

        return FieldInboxRules.BuildProductResponse(items);
    }
}
