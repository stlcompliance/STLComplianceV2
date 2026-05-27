using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class ReceivingExceptionService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<IReadOnlyList<ReceivingExceptionResponse>> ListForReceiptAsync(
        Guid tenantId,
        Guid receivingReceiptId,
        CancellationToken cancellationToken = default)
    {
        var receiptExists = await db.ReceivingReceipts.AnyAsync(
            x => x.TenantId == tenantId && x.Id == receivingReceiptId,
            cancellationToken);
        if (!receiptExists)
        {
            throw new StlApiException(
                "receiving.not_found",
                "Receiving receipt was not found.",
                404);
        }

        var exceptions = await db.ReceivingExceptions
            .AsNoTracking()
            .Include(x => x.ReceivingReceiptLine)
            .ThenInclude(x => x!.Part)
            .Where(x => x.TenantId == tenantId && x.ReceivingReceiptId == receivingReceiptId)
            .OrderBy(x => x.ReceivingReceiptLine.LineNumber)
            .ThenBy(x => x.ExceptionType)
            .ToListAsync(cancellationToken);

        return exceptions.Select(x => Map(x)).ToList();
    }

    public async Task<ReceivingExceptionResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid receivingReceiptId,
        Guid lineId,
        CreateReceivingExceptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var receipt = await db.ReceivingReceipts
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == receivingReceiptId,
                cancellationToken)
            ?? throw new StlApiException(
                "receiving.not_found",
                "Receiving receipt was not found.",
                404);

        if (!ReceivingReceiptStatuses.Editable.Contains(receipt.Status))
        {
            throw new StlApiException(
                "receiving.not_editable",
                "Receiving exceptions can only be recorded on draft receipts.",
                409);
        }

        var line = receipt.Lines.FirstOrDefault(x => x.Id == lineId)
            ?? throw new StlApiException(
                "receiving.line.not_found",
                "Receiving receipt line was not found.",
                404);

        var exceptionType = NormalizeExceptionType(request.ExceptionType);
        var quantity = NormalizeQuantity(request.Quantity);
        var notes = NormalizeNotes(request.Notes ?? string.Empty);

        var now = DateTimeOffset.UtcNow;
        var entity = new ReceivingException
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReceivingReceiptId = receipt.Id,
            ReceivingReceiptLineId = line.Id,
            ExceptionType = exceptionType,
            Quantity = quantity,
            Notes = notes,
            Status = ReceivingExceptionStatuses.Open,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
            ReceivingReceiptLine = line
        };

        db.ReceivingExceptions.Add(entity);
        receipt.UpdatedAt = now;
        line.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "receiving_exception.create",
            tenantId,
            actorUserId,
            "receiving_exception",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity, line);
    }

    public async Task<ReceivingExceptionResponse> ResolveAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid receivingExceptionId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.ReceivingExceptions
            .Include(x => x.ReceivingReceipt)
            .Include(x => x.ReceivingReceiptLine)
                .ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == receivingExceptionId,
                cancellationToken)
            ?? throw new StlApiException(
                "receiving_exception.not_found",
                "Receiving exception was not found.",
                404);

        if (!string.Equals(
                entity.Status,
                ReceivingExceptionStatuses.Open,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "receiving_exception.not_open",
                "Only open receiving exceptions can be resolved.",
                409);
        }

        if (!ReceivingReceiptStatuses.Editable.Contains(entity.ReceivingReceipt.Status))
        {
            throw new StlApiException(
                "receiving.not_editable",
                "Receiving exceptions can only be resolved on draft receipts.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = ReceivingExceptionStatuses.Resolved;
        entity.ResolvedByUserId = actorUserId;
        entity.ResolvedAt = now;
        entity.UpdatedAt = now;
        entity.ReceivingReceipt.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "receiving_exception.resolve",
            tenantId,
            actorUserId,
            "receiving_exception",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity, entity.ReceivingReceiptLine);
    }

    internal static void ValidateLineCoverageForPost(
        ReceivingReceiptLine line,
        IReadOnlyList<ReceivingException> exceptions)
    {
        var expected = line.QuantityExpected;
        var received = line.QuantityReceived;
        var remainingOnOrder = line.PurchaseOrderLine.QuantityOrdered - line.PurchaseOrderLine.QuantityReceived;

        var shortQty = SumByType(exceptions, ReceivingExceptionTypes.Short);
        var overQty = SumByType(exceptions, ReceivingExceptionTypes.Over);
        var damageQty = SumByType(exceptions, ReceivingExceptionTypes.Damage);

        if (received > remainingOnOrder)
        {
            var overVariance = received - remainingOnOrder;
            if (overQty + 0.0001m < overVariance)
            {
                throw new StlApiException(
                    "receiving.exception.over_required",
                    "An over-receive exception is required when quantity received exceeds the remaining purchase order quantity.",
                    400);
            }
        }

        var accounted = received + damageQty + shortQty;
        if (accounted + 0.0001m < expected)
        {
            throw new StlApiException(
                "receiving.exception.short_required",
                "A short-shipment exception is required when received and damaged quantities are below the expected receipt quantity.",
                400);
        }

        if (accounted - 0.0001m > expected + overQty)
        {
            throw new StlApiException(
                "receiving.exception.over_expected",
                "Recorded exceptions exceed the expected receipt quantity for this line.",
                400);
        }

    }

    internal static ReceivingExceptionResponse Map(
        ReceivingException entity,
        ReceivingReceiptLine? line = null)
    {
        line ??= entity.ReceivingReceiptLine;
        return new(
            entity.Id,
            entity.ReceivingReceiptId,
            entity.ReceivingReceiptLineId,
            line?.LineNumber ?? 0,
            line?.Part?.PartKey ?? entity.ReceivingReceiptLine?.Part?.PartKey ?? string.Empty,
            entity.ExceptionType,
            entity.Quantity,
            entity.Notes,
            entity.Status,
            entity.CreatedByUserId,
            entity.ResolvedByUserId,
            entity.ResolvedAt,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private static decimal SumByType(IEnumerable<ReceivingException> exceptions, string exceptionType) =>
        exceptions
            .Where(x => string.Equals(x.ExceptionType, exceptionType, StringComparison.OrdinalIgnoreCase))
            .Sum(x => x.Quantity);

    private static string NormalizeExceptionType(string value)
    {
        var type = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (!ReceivingExceptionTypes.All.Contains(type))
        {
            throw new StlApiException(
                "receiving_exception.type.invalid",
                "Exception type must be short, over, or damage.",
                400);
        }

        return type;
    }

    private static decimal NormalizeQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new StlApiException(
                "receiving_exception.quantity.invalid",
                "Exception quantity must be greater than zero.",
                400);
        }

        return decimal.Round(quantity, 4, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeNotes(string value)
    {
        var notes = (value ?? string.Empty).Trim();
        return notes.Length > 1024 ? notes[..1024] : notes;
    }
}
