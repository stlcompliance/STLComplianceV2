using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class SupplierEmailInboxService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<SupplierEmailInboxListResponse> ListAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = NormalizeLimit(limit);
        var rows = await db.SupplierEmailInboxMessages
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ReceivedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new SupplierEmailInboxListResponse(rows.Select(Map).ToList());
    }

    public async Task<IngestSupplierEmailInboxResponse> IngestAsync(
        Guid tenantId,
        Guid actorUserId,
        IngestSupplierEmailInboxRequest request,
        CancellationToken cancellationToken = default)
    {
        var messageKey = NormalizeRequired(request.MessageKey, "message key");
        var messageKind = NormalizeMessageKind(request.MessageKind);
        var senderEmail = NormalizeEmail(request.SenderEmail);
        var senderName = NormalizeRequired(request.SenderName, "sender name");
        var subject = NormalizeRequired(request.Subject, "subject");
        var body = NormalizeRequired(request.Body, "body");
        var referenceKey = NormalizeOptional(request.ReferenceKey);
        var receivedAt = DateTimeOffset.UtcNow;

        var existing = await db.SupplierEmailInboxMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.MessageKey == messageKey,
                cancellationToken);

        if (existing is not null)
        {
            return new IngestSupplierEmailInboxResponse(true, Map(existing));
        }

        var supplier = await LoadSupplierByEmailAsync(tenantId, senderEmail, cancellationToken);
        var matches = await LoadReferenceMatchesAsync(tenantId, cancellationToken);
        var resolution = await ResolveMessageAsync(
            messageKind,
            subject,
            body,
            referenceKey,
            supplier,
            matches,
            cancellationToken);

        var entity = new SupplierEmailInboxMessage
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MessageKey = messageKey,
            MessageKind = messageKind,
            SenderEmail = senderEmail,
            SenderName = senderName,
            Subject = subject,
            BodyPreview = Truncate(body, 4096),
            MatchStatus = resolution.MatchStatus,
            MatchReason = resolution.MatchReason,
            SupplierId = resolution.SupplierId,
            SupplierKey = resolution.SupplierKey,
            SupplierDisplayName = resolution.SupplierDisplayName,
            LinkedReferenceType = resolution.LinkedReferenceType,
            LinkedReferenceId = resolution.LinkedReferenceId,
            LinkedReferenceKey = resolution.LinkedReferenceKey,
            ReceivedAt = receivedAt,
            CreatedAt = receivedAt,
            UpdatedAt = receivedAt,
            ProcessedAt = receivedAt,
        };

        db.SupplierEmailInboxMessages.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplier_email_inbox.ingest",
            tenantId,
            actorUserId,
            "supplier_email_inbox_message",
            entity.Id.ToString(),
            "Succeeded",
            reasonCode: entity.MatchStatus,
            cancellationToken: cancellationToken);

        return new IngestSupplierEmailInboxResponse(false, Map(entity));
    }

    private async Task<List<Rfq>> LoadRfqsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await db.Rfqs
            .AsNoTracking()
            .Include(x => x.SupplierInvitations)
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<PurchaseOrder>> LoadPurchaseOrdersAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await db.PurchaseOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<SupplierReferenceMatch>> LoadReferenceMatchesAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var rfqs = await LoadRfqsAsync(tenantId, cancellationToken);
        var orders = await LoadPurchaseOrdersAsync(tenantId, cancellationToken);
        return [
            .. rfqs.Select(rfq => new SupplierReferenceMatch(
                "rfq",
                rfq.Id,
                rfq.RfqKey,
                rfq.Title,
                rfq.Status,
                rfq.CreatedAt,
                rfq.SupplierInvitations.Select(x => x.SupplierId).ToHashSet())),
            .. orders.Select(order => new SupplierReferenceMatch(
                "purchase_order",
                order.Id,
                order.OrderKey,
                order.Title,
                order.Status,
                order.CreatedAt,
                [order.SupplierId])),
        ];
    }

    private async Task<Supplier?> LoadSupplierByEmailAsync(
        Guid tenantId,
        string senderEmail,
        CancellationToken cancellationToken)
    {
        return await db.Suppliers
            .AsNoTracking()
            .Include(x => x.Contacts)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.Contacts.Any(c => c.Email.ToLower() == senderEmail.ToLower()),
                cancellationToken);
    }

    private async Task<SupplierEmailInboxResolution> ResolveMessageAsync(
        string messageKind,
        string subject,
        string body,
        string? referenceKey,
        Supplier? supplier,
        IReadOnlyList<SupplierReferenceMatch> matches,
        CancellationToken cancellationToken)
    {
        var text = $"{subject}\n{body}".ToLowerInvariant();

        SupplierReferenceMatch? linked = null;
        var reason = string.Empty;

        if (!string.IsNullOrWhiteSpace(referenceKey))
        {
            linked = matches.FirstOrDefault(match =>
                string.Equals(match.ReferenceKey, referenceKey, StringComparison.OrdinalIgnoreCase));
            if (linked is not null)
            {
                reason = "matched explicit reference key";
            }
        }

        if (linked is null)
        {
            linked = FindByContentMatches(messageKind, text, matches);
            if (linked is not null)
            {
                reason = "matched subject/body reference key";
            }
        }

        if (linked is null && supplier is not null)
        {
            linked = FindBySupplier(messageKind, supplier.Id, matches);
            if (linked is not null)
            {
                reason = "matched sender email to supplier reference";
            }
        }

        var supplierId = supplier?.Id
            ?? (linked is not null && linked.SupplierIds.Count > 0 ? linked.SupplierIds.First() : null);
        var supplierKey = supplier?.SupplierKey;
        var supplierDisplayName = supplier?.DisplayName;

        if (supplier is null && linked is not null && linked.SupplierIds.Count == 1)
        {
            var matchedSupplierId = linked.SupplierIds.First();
            var matchedSupplier = await db.Suppliers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == matchedSupplierId, cancellationToken);
            if (matchedSupplier is not null)
            {
                supplierId = matchedSupplier.Id;
                supplierKey = matchedSupplier.SupplierKey;
                supplierDisplayName = matchedSupplier.DisplayName;
            }
        }

        if (linked is not null && string.IsNullOrWhiteSpace(reason))
        {
            reason = $"matched to {linked.LinkedReferenceType} {linked.ReferenceKey}";
        }

        return new SupplierEmailInboxResolution(
            linked is null ? SupplierEmailInboxMatchStatuses.Unmatched : SupplierEmailInboxMatchStatuses.Matched,
            string.IsNullOrWhiteSpace(reason) ? "no matching RFQ or purchase order reference found" : reason,
            linked?.LinkedReferenceType,
            linked?.LinkedReferenceId,
            linked?.ReferenceKey,
            supplierId,
            supplierKey,
            supplierDisplayName);
    }

    private static SupplierReferenceMatch? FindByContentMatches(
        string messageKind,
        string text,
        IReadOnlyList<SupplierReferenceMatch> matches)
    {
        if (string.Equals(messageKind, SupplierEmailInboxMessageKinds.QuoteReceived, StringComparison.OrdinalIgnoreCase))
        {
            return matches.FirstOrDefault(match =>
                string.Equals(match.LinkedReferenceType, "rfq", StringComparison.OrdinalIgnoreCase)
                && text.Contains(match.ReferenceKey, StringComparison.OrdinalIgnoreCase))
                ?? matches.FirstOrDefault(match =>
                    string.Equals(match.LinkedReferenceType, "purchase_order", StringComparison.OrdinalIgnoreCase)
                    && text.Contains(match.ReferenceKey, StringComparison.OrdinalIgnoreCase));
        }

        if (string.Equals(messageKind, SupplierEmailInboxMessageKinds.OrderConfirmationReceived, StringComparison.OrdinalIgnoreCase))
        {
            return matches.FirstOrDefault(match =>
                string.Equals(match.LinkedReferenceType, "purchase_order", StringComparison.OrdinalIgnoreCase)
                && text.Contains(match.ReferenceKey, StringComparison.OrdinalIgnoreCase))
                ?? matches.FirstOrDefault(match =>
                    string.Equals(match.LinkedReferenceType, "rfq", StringComparison.OrdinalIgnoreCase)
                    && text.Contains(match.ReferenceKey, StringComparison.OrdinalIgnoreCase));
        }

        return matches.FirstOrDefault(match => text.Contains(match.ReferenceKey, StringComparison.OrdinalIgnoreCase));
    }

    private static SupplierReferenceMatch? FindBySupplier(
        string messageKind,
        Guid supplierId,
        IReadOnlyList<SupplierReferenceMatch> matches)
    {
        if (string.Equals(messageKind, SupplierEmailInboxMessageKinds.QuoteReceived, StringComparison.OrdinalIgnoreCase))
        {
            return matches.FirstOrDefault(match =>
                string.Equals(match.LinkedReferenceType, "rfq", StringComparison.OrdinalIgnoreCase)
                && match.SupplierIds.Contains(supplierId));
        }

        if (string.Equals(messageKind, SupplierEmailInboxMessageKinds.OrderConfirmationReceived, StringComparison.OrdinalIgnoreCase))
        {
            return matches.FirstOrDefault(match =>
                string.Equals(match.LinkedReferenceType, "purchase_order", StringComparison.OrdinalIgnoreCase)
                && match.SupplierIds.Contains(supplierId));
        }

        return matches.FirstOrDefault(match => match.SupplierIds.Contains(supplierId));
    }

    private static SupplierEmailInboxMessageResponse Map(SupplierEmailInboxMessage x) =>
        new(
            x.Id,
            x.MessageKey,
            x.MessageKind,
            x.SenderEmail,
            x.SenderName,
            x.Subject,
            x.BodyPreview,
            x.MatchStatus,
            x.MatchReason,
            x.SupplierId,
            x.SupplierKey,
            x.SupplierDisplayName,
            x.LinkedReferenceType,
            x.LinkedReferenceId,
            x.LinkedReferenceKey,
            x.ReceivedAt,
            x.CreatedAt,
            x.UpdatedAt,
            x.ProcessedAt);

    private static int NormalizeLimit(int? limit) =>
        Math.Clamp(limit ?? 25, 1, 50);

    private static string NormalizeMessageKind(string value)
    {
        var normalized = NormalizeRequired(value, "message kind").Replace('-', '_').ToLowerInvariant();
        return normalized switch
        {
            "quote" or "quote_received" or "supplier_quote" => SupplierEmailInboxMessageKinds.QuoteReceived,
            "order_confirmation" or "order_confirmation_received" or "order_acknowledgement" or "order_ack" =>
                SupplierEmailInboxMessageKinds.OrderConfirmationReceived,
            _ => throw new StlApiException(
                "supplier_email_inbox.kind_invalid",
                "Message kind must be quote_received or order_confirmation_received.",
                400),
        };
    }

    private static string NormalizeEmail(string value)
    {
        var normalized = NormalizeRequired(value, "sender email").ToLowerInvariant();
        if (!normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new StlApiException(
                "supplier_email_inbox.sender_email_invalid",
                "Sender email must be a valid email address.",
                400);
        }

        return normalized;
    }

    private static string NormalizeRequired(string value, string label)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException(
                $"supplier_email_inbox.{label.Replace(' ', '_')}_required",
                $"{char.ToUpperInvariant(label[0])}{label[1..]} is required.",
                400);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    private sealed record SupplierReferenceMatch(
        string LinkedReferenceType,
        Guid LinkedReferenceId,
        string ReferenceKey,
        string DisplayName,
        string Status,
        DateTimeOffset CreatedAt,
        HashSet<Guid> SupplierIds);

    private sealed record SupplierEmailInboxResolution(
        string MatchStatus,
        string MatchReason,
        string? LinkedReferenceType,
        Guid? LinkedReferenceId,
        string? LinkedReferenceKey,
        Guid? SupplierId,
        string? SupplierKey,
        string? SupplierDisplayName);
}

