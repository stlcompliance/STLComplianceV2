using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class VendorEmailInboxService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<VendorEmailInboxListResponse> ListAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = NormalizeLimit(limit);
        var rows = await db.VendorEmailInboxMessages
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ReceivedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new VendorEmailInboxListResponse(rows.Select(Map).ToList());
    }

    public async Task<IngestVendorEmailInboxResponse> IngestAsync(
        Guid tenantId,
        Guid actorUserId,
        IngestVendorEmailInboxRequest request,
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

        var existing = await db.VendorEmailInboxMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.MessageKey == messageKey,
                cancellationToken);

        if (existing is not null)
        {
            return new IngestVendorEmailInboxResponse(true, Map(existing));
        }

        var vendorParty = await LoadVendorByEmailAsync(tenantId, senderEmail, cancellationToken);
        var matches = await LoadReferenceMatchesAsync(tenantId, cancellationToken);
        var resolution = await ResolveMessageAsync(
            messageKind,
            subject,
            body,
            referenceKey,
            vendorParty,
            matches,
            cancellationToken);

        var entity = new VendorEmailInboxMessage
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
            VendorPartyId = resolution.VendorPartyId,
            VendorPartyKey = resolution.VendorPartyKey,
            VendorDisplayName = resolution.VendorDisplayName,
            LinkedReferenceType = resolution.LinkedReferenceType,
            LinkedReferenceId = resolution.LinkedReferenceId,
            LinkedReferenceKey = resolution.LinkedReferenceKey,
            ReceivedAt = receivedAt,
            CreatedAt = receivedAt,
            UpdatedAt = receivedAt,
            ProcessedAt = receivedAt,
        };

        db.VendorEmailInboxMessages.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "vendor_email_inbox.ingest",
            tenantId,
            actorUserId,
            "vendor_email_inbox_message",
            entity.Id.ToString(),
            "Succeeded",
            reasonCode: entity.MatchStatus,
            cancellationToken: cancellationToken);

        return new IngestVendorEmailInboxResponse(false, Map(entity));
    }

    private async Task<List<Rfq>> LoadRfqsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await db.Rfqs
            .AsNoTracking()
            .Include(x => x.VendorInvitations)
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

    private async Task<List<VendorReferenceMatch>> LoadReferenceMatchesAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var rfqs = await LoadRfqsAsync(tenantId, cancellationToken);
        var orders = await LoadPurchaseOrdersAsync(tenantId, cancellationToken);
        return [
            .. rfqs.Select(rfq => new VendorReferenceMatch(
                "rfq",
                rfq.Id,
                rfq.RfqKey,
                rfq.Title,
                rfq.Status,
                rfq.CreatedAt,
                rfq.VendorInvitations.Select(x => x.VendorPartyId).ToHashSet())),
            .. orders.Select(order => new VendorReferenceMatch(
                "purchase_order",
                order.Id,
                order.OrderKey,
                order.Title,
                order.Status,
                order.CreatedAt,
                [order.VendorPartyId])),
        ];
    }

    private async Task<ExternalParty?> LoadVendorByEmailAsync(
        Guid tenantId,
        string senderEmail,
        CancellationToken cancellationToken)
    {
        return await db.ExternalParties
            .AsNoTracking()
            .Include(x => x.Contacts)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.PartyType == "supplier"
                    && x.Contacts.Any(c => c.Email.ToLower() == senderEmail.ToLower()),
                cancellationToken);
    }

    private async Task<VendorEmailInboxResolution> ResolveMessageAsync(
        string messageKind,
        string subject,
        string body,
        string? referenceKey,
        ExternalParty? vendorParty,
        IReadOnlyList<VendorReferenceMatch> matches,
        CancellationToken cancellationToken)
    {
        var text = $"{subject}\n{body}".ToLowerInvariant();

        VendorReferenceMatch? linked = null;
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

        if (linked is null && vendorParty is not null)
        {
            linked = FindByVendorParty(messageKind, vendorParty.Id, matches);
            if (linked is not null)
            {
                reason = "matched sender email to supplier reference";
            }
        }

        var vendorPartyId = vendorParty?.Id
            ?? (linked is not null && linked.VendorPartyIds.Count > 0 ? linked.VendorPartyIds.First() : null);
        var vendorPartyKey = vendorParty?.PartyKey;
        var vendorDisplayName = vendorParty?.DisplayName;

        if (vendorParty is null && linked is not null && linked.VendorPartyIds.Count == 1)
        {
            var vendorId = linked.VendorPartyIds.First();
            var matchedVendor = await db.ExternalParties.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == vendorId, cancellationToken);
            if (matchedVendor is not null)
            {
                vendorPartyId = matchedVendor.Id;
                vendorPartyKey = matchedVendor.PartyKey;
                vendorDisplayName = matchedVendor.DisplayName;
            }
        }

        if (linked is not null && string.IsNullOrWhiteSpace(reason))
        {
            reason = $"matched to {linked.LinkedReferenceType} {linked.ReferenceKey}";
        }

        return new VendorEmailInboxResolution(
            linked is null ? VendorEmailInboxMatchStatuses.Unmatched : VendorEmailInboxMatchStatuses.Matched,
            string.IsNullOrWhiteSpace(reason) ? "no matching RFQ or purchase order reference found" : reason,
            linked?.LinkedReferenceType,
            linked?.LinkedReferenceId,
            linked?.ReferenceKey,
            vendorPartyId,
            vendorPartyKey,
            vendorDisplayName,
            vendorPartyId,
            vendorPartyKey,
            vendorDisplayName);
    }

    private static VendorReferenceMatch? FindByContentMatches(
        string messageKind,
        string text,
        IReadOnlyList<VendorReferenceMatch> matches)
    {
        if (string.Equals(messageKind, VendorEmailInboxMessageKinds.QuoteReceived, StringComparison.OrdinalIgnoreCase))
        {
            return matches.FirstOrDefault(match =>
                string.Equals(match.LinkedReferenceType, "rfq", StringComparison.OrdinalIgnoreCase)
                && text.Contains(match.ReferenceKey, StringComparison.OrdinalIgnoreCase))
                ?? matches.FirstOrDefault(match =>
                    string.Equals(match.LinkedReferenceType, "purchase_order", StringComparison.OrdinalIgnoreCase)
                    && text.Contains(match.ReferenceKey, StringComparison.OrdinalIgnoreCase));
        }

        if (string.Equals(messageKind, VendorEmailInboxMessageKinds.OrderConfirmationReceived, StringComparison.OrdinalIgnoreCase))
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

    private static VendorReferenceMatch? FindByVendorParty(
        string messageKind,
        Guid vendorPartyId,
        IReadOnlyList<VendorReferenceMatch> matches)
    {
        if (string.Equals(messageKind, VendorEmailInboxMessageKinds.QuoteReceived, StringComparison.OrdinalIgnoreCase))
        {
            return matches.FirstOrDefault(match =>
                string.Equals(match.LinkedReferenceType, "rfq", StringComparison.OrdinalIgnoreCase)
                && match.VendorPartyIds.Contains(vendorPartyId));
        }

        if (string.Equals(messageKind, VendorEmailInboxMessageKinds.OrderConfirmationReceived, StringComparison.OrdinalIgnoreCase))
        {
            return matches.FirstOrDefault(match =>
                string.Equals(match.LinkedReferenceType, "purchase_order", StringComparison.OrdinalIgnoreCase)
                && match.VendorPartyIds.Contains(vendorPartyId));
        }

        return matches.FirstOrDefault(match => match.VendorPartyIds.Contains(vendorPartyId));
    }

    private static VendorEmailInboxMessageResponse Map(VendorEmailInboxMessage x) =>
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
            x.VendorPartyId,
            x.VendorPartyKey,
            x.VendorDisplayName,
            x.VendorPartyId,
            x.VendorPartyKey,
            x.VendorDisplayName,
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
            "quote" or "quote_received" or "vendor_quote" => VendorEmailInboxMessageKinds.QuoteReceived,
            "order_confirmation" or "order_confirmation_received" or "order_acknowledgement" or "order_ack" =>
                VendorEmailInboxMessageKinds.OrderConfirmationReceived,
            _ => throw new StlApiException(
                "vendor_email_inbox.kind_invalid",
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
                "vendor_email_inbox.sender_email_invalid",
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
                $"vendor_email_inbox.{label.Replace(' ', '_')}_required",
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

    private sealed record VendorReferenceMatch(
        string LinkedReferenceType,
        Guid LinkedReferenceId,
        string ReferenceKey,
        string DisplayName,
        string Status,
        DateTimeOffset CreatedAt,
        HashSet<Guid> VendorPartyIds);

    private sealed record VendorEmailInboxResolution(
        string MatchStatus,
        string MatchReason,
        string? LinkedReferenceType,
        Guid? LinkedReferenceId,
        string? LinkedReferenceKey,
        Guid? SupplierId,
        string? SupplierKey,
        string? SupplierDisplayName,
        Guid? VendorPartyId,
        string? VendorPartyKey,
        string? VendorDisplayName);
}
