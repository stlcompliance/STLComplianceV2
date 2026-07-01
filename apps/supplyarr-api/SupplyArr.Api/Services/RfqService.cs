using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SupplyArr.Api.Services;

public sealed class RfqService(
    SupplyArrDbContext db,
    SupplierProcurementGuardService supplierProcurementGuard,
    PurchaseRequestService purchaseRequests,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    public static readonly Guid SupplierPortalActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f0");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<RfqResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.Rfqs
            .AsNoTracking()
            .Include(x => x.AwardedSupplier).ThenInclude(x => x!.ParentSupplier)
            .Include(x => x.Lines).ThenInclude(x => x.Part)
            .Include(x => x.SupplierInvitations).ThenInclude(x => x.Supplier).ThenInclude(x => x!.ParentSupplier)
            .Include(x => x.SupplierQuotes).ThenInclude(x => x.Supplier).ThenInclude(x => x!.ParentSupplier)
            .Include(x => x.SupplierQuotes).ThenInclude(x => x.Lines).ThenInclude(x => x.RfqLine).ThenInclude(x => x.Part)
            .Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim().ToLowerInvariant());
        }

        var rows = await query.OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken);
        return rows.Select(Map).ToList();
    }

    public async Task<RfqResponse> GetAsync(
        Guid tenantId,
        Guid rfqId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, rfqId, cancellationToken);
        return Map(entity);
    }

    public async Task<RfqResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateRfqRequest request,
        CancellationToken cancellationToken = default)
    {
        var rfqKey = NormalizeRfqKey(request.RfqKey);
        if (await db.Rfqs.AnyAsync(x => x.TenantId == tenantId && x.RfqKey == rfqKey, cancellationToken))
        {
            throw new StlApiException("rfq.duplicate", "An RFQ with this key already exists.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new Rfq
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RfqKey = rfqKey,
            Title = NormalizeTitle(request.Title),
            Notes = NormalizeNotes(request.Notes),
            Status = RfqStatuses.Draft,
            RequestedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Rfqs.Add(entity);

        if (request.Lines is { Count: > 0 })
        {
            foreach (var line in request.Lines)
            {
                await AddLineInternalAsync(entity, line, now, cancellationToken);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "rfq.create",
            tenantId,
            actorUserId,
            "rfq",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<RfqResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid rfqId,
        UpdateRfqRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, rfqId, cancellationToken);
        EnsureEditable(entity);

        entity.Title = NormalizeTitle(request.Title);
        entity.Notes = NormalizeNotes(request.Notes);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "rfq.update",
            tenantId,
            actorUserId,
            "rfq",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<RfqResponse> AddLineAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid rfqId,
        AddRfqLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, rfqId, cancellationToken);
        EnsureEditable(entity);

        var now = DateTimeOffset.UtcNow;
        await AddLineInternalAsync(
            entity,
            new CreateRfqLineRequest(request.PartId, request.QuantityRequested, request.Notes),
            now,
            cancellationToken);
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "rfq.line.add",
            tenantId,
            actorUserId,
            "rfq_line",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<RfqResponse> UpdateLineAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid rfqId,
        Guid lineId,
        UpdateRfqLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, rfqId, cancellationToken);
        EnsureEditable(entity);

        var line = entity.Lines.FirstOrDefault(x => x.Id == lineId)
            ?? throw new StlApiException("rfq.line.not_found", "RFQ line was not found.", 404);

        line.QuantityRequested = NormalizeQuantity(request.QuantityRequested);
        line.Notes = NormalizeNotes(request.Notes);
        line.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = line.UpdatedAt;

        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<RfqResponse> SubmitAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid rfqId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, rfqId, cancellationToken);
        if (!RfqStatuses.Editable.Contains(entity.Status))
        {
            throw new StlApiException("rfq.invalid_transition", "Only draft RFQs can be submitted.", 409);
        }

        if (entity.Lines.Count == 0)
        {
            throw new StlApiException("rfq.lines.required", "At least one line is required before submission.", 400);
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = RfqStatuses.Submitted;
        entity.SubmittedAt = now;
        entity.SubmittedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "rfq.submit",
            tenantId,
            actorUserId,
            "rfq",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.RfqSubmitted,
            "rfq",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"RFQ submitted: {entity.RfqKey}"),
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<RfqResponse> InviteSuppliersAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid rfqId,
        InviteRfqSuppliersRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, rfqId, cancellationToken);
        if (!RfqStatuses.OpenForQuotes.Contains(entity.Status))
        {
            throw new StlApiException("rfq.not_open", "Suppliers can only be invited on submitted RFQs.", 409);
        }

        var selectedSupplierIds = request.SupplierIds;
        if (selectedSupplierIds is not { Count: > 0 })
        {
            throw new StlApiException("rfq.invitations.required", "At least one supplier is required.", 400);
        }

        var now = DateTimeOffset.UtcNow;
        var existingSupplierIds = await db.RfqSupplierInvitations
            .Where(x => x.TenantId == tenantId && x.RfqId == entity.Id)
            .Select(x => x.SupplierId)
            .ToListAsync(cancellationToken);

        foreach (var supplierId in selectedSupplierIds.Distinct())
        {
            await EnsureSupplierAllowedAsync(tenantId, supplierId, cancellationToken);
            if (existingSupplierIds.Contains(supplierId))
            {
                continue;
            }

            var portalAccessCode = GenerateSupplierPortalAccessCode();
            var portalAccessIssuedAt = now;
            var portalAccessExpiresAt = now.AddDays(14);

            db.RfqSupplierInvitations.Add(new RfqSupplierInvitation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RfqId = entity.Id,
                SupplierId = supplierId,
                Status = RfqInvitationStatuses.Invited,
                InvitedAt = now,
                InvitedByUserId = actorUserId,
                PortalAccessCode = portalAccessCode,
                PortalAccessCodeIssuedAt = portalAccessIssuedAt,
                PortalAccessCodeExpiresAt = portalAccessExpiresAt,
            });
            existingSupplierIds.Add(supplierId);
        }

        entity.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "rfq.suppliers.invite",
            tenantId,
            actorUserId,
            "rfq",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.RfqSuppliersInvited,
            "rfq",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"RFQ suppliers invited: {entity.RfqKey}"),
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<SupplierPortalRfqResponse> GetSupplierPortalAsync(
        Guid rfqId,
        string accessCode,
        CancellationToken cancellationToken = default)
    {
        var invitation = await ResolveSupplierPortalInvitationAsync(rfqId, accessCode, cancellationToken);
        var rfq = await LoadAsync(invitation.TenantId, invitation.RfqId, cancellationToken);
        return MapSupplierPortal(
            rfq,
            invitation,
            ResolveSupplierQuoteForInvitation(rfq, invitation));
    }

    public async Task<SupplierQuoteResponse> CreateSupplierPortalQuoteAsync(
        Guid rfqId,
        string accessCode,
        SupplierPortalCreateQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var invitation = await ResolveSupplierPortalInvitationAsync(rfqId, accessCode, cancellationToken);
        return await CreateSupplierQuoteAsync(
            invitation.TenantId,
            SupplierPortalActorUserId,
            rfqId,
            new CreateSupplierQuoteRequest(
                invitation.SupplierId,
                request.QuoteKey,
                request.CurrencyCode,
                request.Notes),
            cancellationToken);
    }

    public async Task<SupplierQuoteResponse> UpsertSupplierPortalQuoteLineAsync(
        Guid rfqId,
        Guid supplierQuoteId,
        string accessCode,
        UpsertSupplierQuoteLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var invitation = await ResolveSupplierPortalInvitationAsync(rfqId, accessCode, cancellationToken);
        var quote = await LoadQuoteTrackedAsync(invitation.TenantId, rfqId, supplierQuoteId, cancellationToken);
        EnsureSupplierPortalQuoteOwnership(invitation, quote);
        return await UpsertQuoteLineAsync(
            invitation.TenantId,
            SupplierPortalActorUserId,
            rfqId,
            supplierQuoteId,
            request,
            cancellationToken);
    }

    public async Task<SupplierQuoteResponse> SubmitSupplierPortalQuoteAsync(
        Guid rfqId,
        Guid supplierQuoteId,
        string accessCode,
        CancellationToken cancellationToken = default)
    {
        var invitation = await ResolveSupplierPortalInvitationAsync(rfqId, accessCode, cancellationToken);
        var quote = await LoadQuoteTrackedAsync(invitation.TenantId, rfqId, supplierQuoteId, cancellationToken);
        EnsureSupplierPortalQuoteOwnership(invitation, quote);
        return await SubmitSupplierQuoteAsync(
            invitation.TenantId,
            SupplierPortalActorUserId,
            rfqId,
            supplierQuoteId,
            cancellationToken);
    }

    public async Task<SupplierQuoteResponse> CreateSupplierQuoteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid rfqId,
        CreateSupplierQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var rfq = await LoadTrackedAsync(tenantId, rfqId, cancellationToken);
        if (!RfqStatuses.OpenForQuotes.Contains(rfq.Status))
        {
            throw new StlApiException("rfq.not_open", "Quotes can only be recorded on submitted RFQs.", 409);
        }

        var selectedSupplierId = request.SupplierId
            ?? throw new StlApiException("rfq.quote.supplier_required", "Supplier is required.", 400);
        await EnsureSupplierAllowedAsync(tenantId, selectedSupplierId, cancellationToken);
        var invited = await db.RfqSupplierInvitations.AnyAsync(
            x => x.TenantId == tenantId && x.RfqId == rfqId && x.SupplierId == selectedSupplierId,
            cancellationToken);
        if (!invited)
        {
            throw new StlApiException(
                "rfq.supplier.not_invited",
                "Supplier must be invited before recording a quote.",
                409);
        }

        var quoteKey = NormalizeQuoteKey(request.QuoteKey);
        if (await db.SupplierQuotes.AnyAsync(
                x => x.TenantId == tenantId && x.RfqId == rfqId && x.QuoteKey == quoteKey,
                cancellationToken))
        {
            throw new StlApiException("rfq.quote.duplicate", "A quote with this key already exists on the RFQ.", 409);
        }

        if (await db.SupplierQuotes.AnyAsync(
                x => x.TenantId == tenantId
                    && x.RfqId == rfqId
                    && x.SupplierId == selectedSupplierId
                    && x.Status != SupplierQuoteStatuses.Withdrawn,
                cancellationToken))
        {
            throw new StlApiException(
                "rfq.quote.supplier_exists",
                "An active quote already exists for this supplier.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var quote = new SupplierQuote
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RfqId = rfq.Id,
            SupplierId = selectedSupplierId,
            QuoteKey = quoteKey,
            Status = SupplierQuoteStatuses.Draft,
            CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode),
            Notes = NormalizeNotes(request.Notes),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.SupplierQuotes.Add(quote);
        rfq.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "rfq.quote.create",
            tenantId,
            actorUserId,
            "supplier_quote",
            quote.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapQuote(await LoadQuoteAsync(tenantId, quote.Id, cancellationToken));
    }

    public async Task<SupplierQuoteResponse> UpsertQuoteLineAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid rfqId,
        Guid supplierQuoteId,
        UpsertSupplierQuoteLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var quote = await LoadQuoteTrackedAsync(tenantId, rfqId, supplierQuoteId, cancellationToken);
        EnsureQuoteEditable(quote);

        var rfqLineExists = await db.RfqLines.AnyAsync(
            x => x.TenantId == tenantId && x.RfqId == rfqId && x.Id == request.RfqLineId,
            cancellationToken);
        if (!rfqLineExists)
        {
            throw new StlApiException("rfq.line.not_found", "RFQ line was not found.", 404);
        }

        var now = DateTimeOffset.UtcNow;
        var existing = await db.SupplierQuoteLines.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.SupplierQuoteId == quote.Id && x.RfqLineId == request.RfqLineId,
            cancellationToken);
        if (existing is null)
        {
            db.SupplierQuoteLines.Add(new SupplierQuoteLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SupplierQuoteId = quote.Id,
                RfqLineId = request.RfqLineId,
                UnitPrice = NormalizeUnitPrice(request.UnitPrice),
                QuantityQuoted = NormalizeQuantity(request.QuantityQuoted),
                LeadTimeDays = NormalizeLeadTimeDays(request.LeadTimeDays),
                Notes = NormalizeNotes(request.Notes),
                CreatedAt = now,
                UpdatedAt = now,
            });
        }
        else
        {
            existing.UnitPrice = NormalizeUnitPrice(request.UnitPrice);
            existing.QuantityQuoted = NormalizeQuantity(request.QuantityQuoted);
            existing.LeadTimeDays = NormalizeLeadTimeDays(request.LeadTimeDays);
            existing.Notes = NormalizeNotes(request.Notes);
            existing.UpdatedAt = now;
        }

        quote = await LoadQuoteTrackedAsync(tenantId, rfqId, supplierQuoteId, cancellationToken);
        RecalculateQuoteTotals(quote);
        quote.UpdatedAt = now;
        quote.Rfq.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        return MapQuote(await LoadQuoteAsync(tenantId, quote.Id, cancellationToken));
    }

    public async Task<SupplierQuoteResponse> SubmitSupplierQuoteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid rfqId,
        Guid supplierQuoteId,
        CancellationToken cancellationToken = default)
    {
        var quote = await LoadQuoteTrackedAsync(tenantId, rfqId, supplierQuoteId, cancellationToken);
        if (!SupplierQuoteStatuses.Editable.Contains(quote.Status))
        {
            throw new StlApiException("rfq.quote.not_editable", "Only draft quotes can be submitted.", 409);
        }

        if (quote.Lines.Count == 0)
        {
            throw new StlApiException("rfq.quote.lines.required", "At least one quote line is required.", 400);
        }

        var now = DateTimeOffset.UtcNow;
        quote.Status = SupplierQuoteStatuses.Submitted;
        quote.SubmittedAt = now;
        quote.UpdatedAt = now;
        RecalculateQuoteTotals(quote);

        var invitation = await db.RfqSupplierInvitations.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.RfqId == rfqId && x.SupplierId == quote.SupplierId,
            cancellationToken);
        if (invitation is not null)
        {
            invitation.Status = RfqInvitationStatuses.Responded;
        }

        quote.Rfq.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "rfq.quote.submit",
            tenantId,
            actorUserId,
            "supplier_quote",
            quote.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.RfqQuoteSubmitted,
            "supplier_quote",
            quote.Id,
            new IntegrationOutboxPayload(
                tenantId,
                $"RFQ quote submitted: {quote.QuoteKey}",
                quote.SupplierId),
            cancellationToken: cancellationToken);

        return MapQuote(await LoadQuoteAsync(tenantId, quote.Id, cancellationToken));
    }

    public async Task<RfqQuoteComparisonResponse> CompareQuotesAsync(
        Guid tenantId,
        Guid rfqId,
        CancellationToken cancellationToken = default)
    {
        var rfq = await LoadAsync(tenantId, rfqId, cancellationToken);
        var submittedQuotes = rfq.SupplierQuotes
            .Where(x => x.Status == SupplierQuoteStatuses.Submitted || x.Status == SupplierQuoteStatuses.Selected)
            .ToList();

        var lineRows = new List<RfqLineComparisonRow>();
        foreach (var rfqLine in rfq.Lines.OrderBy(x => x.LineNumber))
        {
            var metrics = new List<RfqQuoteLineMetric>();
            var priced = submittedQuotes
                .Select(q => new
                {
                    Quote = q,
                    Line = q.Lines.FirstOrDefault(l => l.RfqLineId == rfqLine.Id),
                })
                .Where(x => x.Line is not null)
                .ToList();

            var lowestPrice = priced.MinBy(x => x.Line!.UnitPrice * x.Line.QuantityQuoted)?.Line;
            var fastestLead = priced
                .Where(x => x.Line!.LeadTimeDays.HasValue)
                .MinBy(x => x.Line!.LeadTimeDays)?.Line;

            foreach (var item in priced)
            {
                var line = item.Line!;
                var lineTotal = line.UnitPrice * line.QuantityQuoted;
                metrics.Add(new RfqQuoteLineMetric(
                    item.Quote.Id,
                    item.Quote.SupplierId,
                    item.Quote.Supplier.SupplierKey,
                    item.Quote.Supplier.DisplayName,
                    item.Quote.Supplier.ParentSupplierId,
                    item.Quote.Supplier.ParentSupplier?.DisplayName,
                    item.Quote.Supplier.UnitKind,
                    ParseServiceTypes(item.Quote.Supplier.ServiceTypesJson),
                    item.Quote.Status,
                    line.UnitPrice,
                    lineTotal,
                    line.LeadTimeDays,
                    lowestPrice is not null && line.Id == lowestPrice.Id,
                    fastestLead is not null && line.Id == fastestLead.Id));
            }

            lineRows.Add(new RfqLineComparisonRow(
                rfqLine.Id,
                rfqLine.LineNumber,
                rfqLine.PartId,
                rfqLine.Part.PartKey,
                rfqLine.Part.DisplayName,
                rfqLine.QuantityRequested,
                metrics));
        }

        var summaries = submittedQuotes
            .Select(q => new RfqQuoteSummary(
                q.Id,
                q.SupplierId,
                q.Supplier.SupplierKey,
                q.Supplier.DisplayName,
                q.Supplier.ParentSupplierId,
                q.Supplier.ParentSupplier?.DisplayName,
                q.Supplier.UnitKind,
                ParseServiceTypes(q.Supplier.ServiceTypesJson),
                q.Status,
                q.TotalAmount,
                q.LeadTimeDays,
                q.Lines.Count,
                q.Id == rfq.SelectedSupplierQuoteId))
            .ToList();

        return new RfqQuoteComparisonResponse(
            rfq.Id,
            rfq.RfqKey,
            rfq.Status,
            lineRows,
            summaries);
    }

    public async Task<RfqResponse> SelectSupplierQuoteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid rfqId,
        SelectSupplierQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var rfq = await LoadTrackedAsync(tenantId, rfqId, cancellationToken);
        if (!RfqStatuses.OpenForQuotes.Contains(rfq.Status))
        {
            throw new StlApiException("rfq.not_open", "Quotes can only be selected on submitted RFQs.", 409);
        }

        var quote = rfq.SupplierQuotes.FirstOrDefault(x => x.Id == request.SupplierQuoteId)
            ?? throw new StlApiException("rfq.quote.not_found", "Supplier quote was not found.", 404);

        if (quote.Status != SupplierQuoteStatuses.Submitted)
        {
            throw new StlApiException("rfq.quote.not_submitted", "Only submitted quotes can be selected.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var other in rfq.SupplierQuotes)
        {
            if (other.Id == quote.Id)
            {
                other.Status = SupplierQuoteStatuses.Selected;
            }
            else if (other.Status == SupplierQuoteStatuses.Submitted)
            {
                other.Status = SupplierQuoteStatuses.Rejected;
            }

            other.UpdatedAt = now;
        }

        rfq.Status = RfqStatuses.Awarded;
        rfq.SelectedSupplierQuoteId = quote.Id;
        rfq.AwardedSupplierId = quote.SupplierId;
        rfq.AwardedAt = now;
        rfq.AwardedByUserId = actorUserId;
        rfq.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "rfq.award",
            tenantId,
            actorUserId,
            "rfq",
            rfq.Id.ToString(),
            quote.Id.ToString(),
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.RfqAwarded,
            "rfq",
            rfq.Id,
            new IntegrationOutboxPayload(
                tenantId,
                $"RFQ awarded: {rfq.RfqKey}",
                quote.SupplierId),
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, rfq.Id, cancellationToken);
    }

    public async Task<CreatePurchaseRequestFromRfqResponse> CreatePurchaseRequestFromRfqAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid rfqId,
        CreatePurchaseRequestFromRfqRequest request,
        CancellationToken cancellationToken = default)
    {
        var rfq = await LoadTrackedAsync(tenantId, rfqId, cancellationToken);
        if (rfq.Status != RfqStatuses.Awarded)
        {
            throw new StlApiException("rfq.not_awarded", "Purchase requests can only be created from awarded RFQs.", 409);
        }

        if (rfq.PurchaseRequestId.HasValue)
        {
            throw new StlApiException(
                "rfq.purchase_request.exists",
                "A purchase request has already been created for this RFQ.",
                409);
        }

        var quote = rfq.SupplierQuotes.FirstOrDefault(x => x.Id == rfq.SelectedSupplierQuoteId)
            ?? throw new StlApiException("rfq.quote.not_found", "Selected supplier quote was not found.", 404);

        var prLines = quote.Lines
            .OrderBy(x => x.RfqLine.LineNumber)
            .Select(x => new CreatePurchaseRequestLineRequest(
                x.RfqLine.PartId,
                x.QuantityQuoted,
                x.Notes))
            .ToList();

        var pr = await purchaseRequests.CreateAsync(
            tenantId,
            actorUserId,
            new CreatePurchaseRequestRequest(
                RequestKey: NormalizeRequestKey(request.RequestKey),
                Title: string.IsNullOrWhiteSpace(request.Title) ? rfq.Title : NormalizeTitle(request.Title!),
                Notes: string.IsNullOrWhiteSpace(request.Notes) ? rfq.Notes : NormalizeNotes(request.Notes!),
                SupplierId: quote.SupplierId,
                Lines: prLines),
            cancellationToken);

        rfq.PurchaseRequestId = pr.PurchaseRequestId;
        rfq.Status = RfqStatuses.Closed;
        rfq.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "rfq.purchase_request.create",
            tenantId,
            actorUserId,
            "rfq",
            rfq.Id.ToString(),
            pr.PurchaseRequestId.ToString(),
            cancellationToken: cancellationToken);

        return new CreatePurchaseRequestFromRfqResponse(rfq.Id, pr.PurchaseRequestId, pr);
    }

    private async Task AddLineInternalAsync(
        Rfq entity,
        CreateRfqLineRequest request,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var part = await db.Parts.AsNoTracking().FirstOrDefaultAsync(
                x => x.TenantId == entity.TenantId && x.Id == request.PartId,
                cancellationToken)
            ?? throw new StlApiException("rfq.part.not_found", "Part was not found.", 404);

        var lineNumber = entity.Lines.Count == 0 ? 1 : entity.Lines.Max(x => x.LineNumber) + 1;
        entity.Lines.Add(new RfqLine
        {
            Id = Guid.NewGuid(),
            TenantId = entity.TenantId,
            RfqId = entity.Id,
            LineNumber = lineNumber,
            PartId = part.Id,
            QuantityRequested = NormalizeQuantity(request.QuantityRequested),
            UnitOfMeasure = part.UnitOfMeasure,
            Notes = NormalizeNotes(request.Notes),
            CreatedAt = now,
            UpdatedAt = now,
        });
    }

    private static void RecalculateQuoteTotals(SupplierQuote quote)
    {
        if (quote.Lines.Count == 0)
        {
            quote.TotalAmount = null;
            quote.LeadTimeDays = null;
            return;
        }

        quote.TotalAmount = quote.Lines.Sum(x => x.UnitPrice * x.QuantityQuoted);
        quote.LeadTimeDays = quote.Lines.Max(x => x.LeadTimeDays);
    }

    private static void EnsureEditable(Rfq entity)
    {
        if (!RfqStatuses.Editable.Contains(entity.Status))
        {
            throw new StlApiException("rfq.not_editable", "RFQ can only be edited while in draft status.", 409);
        }
    }

    private static void EnsureQuoteEditable(SupplierQuote quote)
    {
        if (!SupplierQuoteStatuses.Editable.Contains(quote.Status))
        {
            throw new StlApiException("rfq.quote.not_editable", "Quote can only be edited while in draft status.", 409);
        }
    }

    private Task EnsureSupplierAllowedAsync(Guid tenantId, Guid supplierId, CancellationToken cancellationToken) =>
        supplierProcurementGuard.EnsureSupplierAllowedForScopeAsync(
            tenantId,
            supplierId,
            SupplierRestrictionScopes.RfqInvitations,
            cancellationToken);

    private async Task<Rfq> LoadAsync(Guid tenantId, Guid rfqId, CancellationToken cancellationToken) =>
        await db.Rfqs
            .AsNoTracking()
            .Include(x => x.AwardedSupplier).ThenInclude(x => x!.ParentSupplier)
            .Include(x => x.Lines).ThenInclude(x => x.Part)
            .Include(x => x.SupplierInvitations).ThenInclude(x => x.Supplier).ThenInclude(x => x!.ParentSupplier)
            .Include(x => x.SupplierQuotes).ThenInclude(x => x.Supplier).ThenInclude(x => x!.ParentSupplier)
            .Include(x => x.SupplierQuotes).ThenInclude(x => x.Lines).ThenInclude(x => x.RfqLine).ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == rfqId, cancellationToken)
        ?? throw new StlApiException("rfq.not_found", "RFQ was not found.", 404);

    private async Task<Rfq> LoadTrackedAsync(Guid tenantId, Guid rfqId, CancellationToken cancellationToken) =>
        await db.Rfqs
            .Include(x => x.Lines).ThenInclude(x => x.Part)
            .Include(x => x.SupplierInvitations).ThenInclude(x => x.Supplier).ThenInclude(x => x!.ParentSupplier)
            .Include(x => x.SupplierQuotes).ThenInclude(x => x.Supplier).ThenInclude(x => x!.ParentSupplier)
            .Include(x => x.SupplierQuotes).ThenInclude(x => x.Lines).ThenInclude(x => x.RfqLine)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == rfqId, cancellationToken)
        ?? throw new StlApiException("rfq.not_found", "RFQ was not found.", 404);

    private async Task<SupplierQuote> LoadQuoteAsync(Guid tenantId, Guid quoteId, CancellationToken cancellationToken) =>
        await db.SupplierQuotes
            .AsNoTracking()
            .Include(x => x.Supplier).ThenInclude(x => x!.ParentSupplier)
            .Include(x => x.Lines).ThenInclude(x => x.RfqLine).ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == quoteId, cancellationToken)
        ?? throw new StlApiException("rfq.quote.not_found", "Supplier quote was not found.", 404);

    private async Task<SupplierQuote> LoadQuoteTrackedAsync(
        Guid tenantId,
        Guid rfqId,
        Guid supplierQuoteId,
        CancellationToken cancellationToken) =>
        await db.SupplierQuotes
            .Include(x => x.Rfq).ThenInclude(x => x.Lines).ThenInclude(x => x.Part)
        .Include(x => x.Rfq).ThenInclude(x => x.SupplierInvitations)
        .Include(x => x.Supplier).ThenInclude(x => x!.ParentSupplier)
        .Include(x => x.Lines).ThenInclude(x => x.RfqLine)
        .FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.RfqId == rfqId && x.Id == supplierQuoteId,
            cancellationToken)
        ?? throw new StlApiException("rfq.quote.not_found", "Supplier quote was not found.", 404);

    private async Task<RfqSupplierInvitation> ResolveSupplierPortalInvitationAsync(
        Guid rfqId,
        string accessCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accessCode))
        {
            throw new StlApiException("rfq.supplier_portal.access_code_required", "Supplier portal access code is required.", 401);
        }

        var now = DateTimeOffset.UtcNow;

        var invitation = await db.RfqSupplierInvitations
            .Include(x => x.Rfq)
                .ThenInclude(x => x.Lines).ThenInclude(x => x.Part)
            .Include(x => x.Rfq)
                .ThenInclude(x => x.SupplierQuotes).ThenInclude(x => x.Supplier).ThenInclude(x => x!.ParentSupplier)
            .Include(x => x.Rfq)
                .ThenInclude(x => x.SupplierQuotes).ThenInclude(x => x.Lines).ThenInclude(x => x.RfqLine).ThenInclude(x => x.Part)
            .Include(x => x.Supplier).ThenInclude(x => x!.ParentSupplier)
            .FirstOrDefaultAsync(
                x => x.RfqId == rfqId && x.PortalAccessCode == accessCode.Trim(),
                cancellationToken);

        if (invitation is null)
        {
            throw new StlApiException("rfq.supplier_portal.invalid_access_code", "Supplier portal access code was not recognized.", 401);
        }

        if (invitation.PortalAccessCodeExpiresAt < now)
        {
            throw new StlApiException("rfq.supplier_portal.expired_access_code", "Supplier portal access code has expired.", 401);
        }

        if (invitation.Rfq.Status is not (RfqStatuses.Submitted or RfqStatuses.Awarded or RfqStatuses.Closed))
        {
            throw new StlApiException("rfq.supplier_portal.not_open", "Supplier portal access is only available for submitted RFQs.", 409);
        }

        return invitation;
    }

    private static SupplierQuote? ResolveSupplierQuoteForInvitation(
        Rfq rfq,
        RfqSupplierInvitation invitation) =>
        rfq.SupplierQuotes.FirstOrDefault(x =>
            x.SupplierId == invitation.SupplierId
            && x.Status != SupplierQuoteStatuses.Withdrawn);

    private static void EnsureSupplierPortalQuoteOwnership(
        RfqSupplierInvitation invitation,
        SupplierQuote quote)
    {
        if (quote.SupplierId != invitation.SupplierId || quote.RfqId != invitation.RfqId)
        {
            throw new StlApiException("rfq.supplier_portal.quote_forbidden", "The quote does not belong to this supplier portal invitation.", 403);
        }
    }

    private static RfqResponse Map(Rfq entity) =>
        new(
            entity.Id,
            entity.RfqKey,
            entity.Title,
            entity.Notes,
            entity.Status,
            entity.RequestedByUserId,
            entity.SubmittedAt,
            entity.AwardedSupplierId,
            entity.AwardedSupplier?.SupplierKey,
            entity.AwardedSupplier?.DisplayName,
            entity.AwardedSupplier?.ParentSupplierId,
            entity.AwardedSupplier?.ParentSupplier?.DisplayName,
            entity.AwardedSupplier?.UnitKind,
            ParseServiceTypes(entity.AwardedSupplier?.ServiceTypesJson),
            entity.SelectedSupplierQuoteId,
            entity.PurchaseRequestId,
            entity.AwardedAt,
            entity.Lines.OrderBy(x => x.LineNumber).Select(MapLine).ToList(),
            entity.SupplierInvitations
                .OrderBy(x => x.InvitedAt)
                .Select(MapInvitation)
                .ToList(),
            entity.SupplierQuotes.OrderBy(x => x.CreatedAt).Select(MapQuote).ToList(),
            entity.CreatedAt,
            entity.UpdatedAt);

    private static RfqLineResponse MapLine(RfqLine line) =>
        new(
            line.Id,
            line.LineNumber,
            line.PartId,
            line.Part.PartKey,
            line.Part.DisplayName,
            line.QuantityRequested,
            line.UnitOfMeasure,
            line.Notes,
            line.CreatedAt,
            line.UpdatedAt);

    private static RfqSupplierInvitationResponse MapInvitation(RfqSupplierInvitation invitation) =>
        new(
            invitation.Id,
            invitation.SupplierId,
            invitation.Supplier.SupplierKey,
            invitation.Supplier.DisplayName,
            invitation.Supplier.ParentSupplierId,
            invitation.Supplier.ParentSupplier?.DisplayName,
            invitation.Supplier.UnitKind,
            ParseServiceTypes(invitation.Supplier.ServiceTypesJson),
            invitation.Status,
            invitation.InvitedAt,
            invitation.PortalAccessCodeIssuedAt,
            invitation.PortalAccessCodeExpiresAt,
            invitation.PortalAccessCode,
            GenerateSupplierPortalLink(invitation.RfqId, invitation.PortalAccessCode));

    private static SupplierPortalRfqResponse MapSupplierPortal(
        Rfq rfq,
        RfqSupplierInvitation invitation,
        SupplierQuote? quote)
    {
        var quoteLinesByRfqLineId = quote?.Lines.ToDictionary(x => x.RfqLineId) ?? new Dictionary<Guid, SupplierQuoteLine>();
        return new SupplierPortalRfqResponse(
            rfq.Id,
            rfq.RfqKey,
            rfq.Title,
            rfq.Notes,
            rfq.Status,
            invitation.SupplierId,
            invitation.Supplier.SupplierKey,
            invitation.Supplier.DisplayName,
            invitation.Supplier.ParentSupplierId,
            invitation.Supplier.ParentSupplier?.DisplayName,
            invitation.Supplier.UnitKind,
            ParseServiceTypes(invitation.Supplier.ServiceTypesJson),
            invitation.Id,
            invitation.Status,
            invitation.InvitedAt,
            invitation.PortalAccessCodeExpiresAt,
            quote?.Id,
            quote?.QuoteKey,
            quote?.Status,
            quote?.CurrencyCode,
            quote?.TotalAmount,
            quote?.LeadTimeDays,
            quote?.Notes,
            quote?.SubmittedAt,
            rfq.Lines.OrderBy(x => x.LineNumber).Select(line =>
            {
                quoteLinesByRfqLineId.TryGetValue(line.Id, out var quoteLine);
                return new SupplierPortalRfqLineResponse(
                    line.Id,
                    line.LineNumber,
                    line.PartId,
                    line.Part.PartKey,
                    line.Part.DisplayName,
                    line.QuantityRequested,
                    line.UnitOfMeasure,
                    line.Notes,
                    quoteLine?.Id,
                    quoteLine?.UnitPrice,
                    quoteLine?.QuantityQuoted,
                    quoteLine?.LeadTimeDays,
                    quoteLine?.Notes ?? string.Empty);
            }).ToList(),
            rfq.CreatedAt,
            rfq.UpdatedAt);
    }

    private static string GenerateSupplierPortalAccessCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string GenerateSupplierPortalLink(Guid rfqId, string portalAccessCode) =>
        string.IsNullOrWhiteSpace(portalAccessCode)
            ? string.Empty
            : $"/supplier-quote-portal?rfqId={rfqId:D}&accessCode={Uri.EscapeDataString(portalAccessCode)}";

    private static SupplierQuoteResponse MapQuote(SupplierQuote quote) =>
        new(
            quote.Id,
            quote.RfqId,
            quote.SupplierId,
            quote.Supplier.SupplierKey,
            quote.Supplier.DisplayName,
            quote.Supplier.ParentSupplierId,
            quote.Supplier.ParentSupplier?.DisplayName,
            quote.Supplier.UnitKind,
            ParseServiceTypes(quote.Supplier.ServiceTypesJson),
            quote.QuoteKey,
            quote.Status,
            quote.CurrencyCode,
            quote.TotalAmount,
            quote.LeadTimeDays,
            quote.Notes,
            quote.SubmittedAt,
            quote.Lines
                .OrderBy(x => x.RfqLine.LineNumber)
                .Select(MapQuoteLine)
                .ToList(),
            quote.CreatedAt,
            quote.UpdatedAt);

    private static IReadOnlyList<string> ParseServiceTypes(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(value, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static SupplierQuoteLineResponse MapQuoteLine(SupplierQuoteLine line) =>
        new(
            line.Id,
            line.RfqLineId,
            line.RfqLine.LineNumber,
            line.RfqLine.PartId,
            line.RfqLine.Part.PartKey,
            line.UnitPrice,
            line.QuantityQuoted,
            line.UnitPrice * line.QuantityQuoted,
            line.LeadTimeDays,
            line.Notes);

    private static string NormalizeRfqKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("rfq.key.required", "RFQ key is required.", 400);
        }

        return value.Trim().ToUpperInvariant();
    }

    private static string NormalizeRequestKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("rfq.pr_key.required", "Purchase request key is required.", 400);
        }

        return value.Trim().ToUpperInvariant();
    }

    private static string NormalizeQuoteKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("rfq.quote_key.required", "Quote key is required.", 400);
        }

        return value.Trim().ToUpperInvariant();
    }

    private static string NormalizeTitle(string value) =>
        string.IsNullOrWhiteSpace(value) ? "RFQ" : value.Trim();

    private static string NormalizeNotes(string? value) => value?.Trim() ?? string.Empty;

    private static string NormalizeCurrencyCode(string value) =>
        string.IsNullOrWhiteSpace(value) ? "USD" : value.Trim().ToUpperInvariant();

    private static decimal NormalizeQuantity(decimal value)
    {
        if (value <= 0)
        {
            throw new StlApiException("rfq.quantity.invalid", "Quantity must be greater than zero.", 400);
        }

        return value;
    }

    private static decimal NormalizeUnitPrice(decimal value)
    {
        if (value < 0)
        {
            throw new StlApiException("rfq.unit_price.invalid", "Unit price cannot be negative.", 400);
        }

        return value;
    }

    private static int? NormalizeLeadTimeDays(int? value) =>
        value is < 0 ? throw new StlApiException("rfq.lead_time.invalid", "Lead time cannot be negative.", 400) : value;
}

