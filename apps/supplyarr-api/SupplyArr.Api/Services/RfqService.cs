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
    VendorProcurementGuardService vendorProcurementGuard,
    PurchaseRequestService purchaseRequests,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    public static readonly Guid VendorPortalActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f0");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<RfqResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.Rfqs
            .AsNoTracking()
            .Include(x => x.AwardedVendorParty).ThenInclude(x => x!.ParentExternalParty)
            .Include(x => x.Lines).ThenInclude(x => x.Part)
            .Include(x => x.VendorInvitations).ThenInclude(x => x.VendorParty).ThenInclude(x => x!.ParentExternalParty)
            .Include(x => x.VendorQuotes).ThenInclude(x => x.VendorParty).ThenInclude(x => x!.ParentExternalParty)
            .Include(x => x.VendorQuotes).ThenInclude(x => x.Lines).ThenInclude(x => x.RfqLine).ThenInclude(x => x.Part)
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

    public Task<RfqResponse> InviteVendorsAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid rfqId,
        InviteRfqVendorsRequest request,
        CancellationToken cancellationToken = default) =>
        InviteSuppliersAsync(
            tenantId,
            actorUserId,
            rfqId,
            new InviteRfqSuppliersRequest(request.SupplierIds, request.VendorPartyIds),
            cancellationToken);

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
            throw new StlApiException("rfq.not_open", "Vendors can only be invited on submitted RFQs.", 409);
        }

        var selectedSupplierIds = request.SupplierIds ?? request.VendorPartyIds;
        if (selectedSupplierIds is not { Count: > 0 })
        {
            throw new StlApiException("rfq.invitations.required", "At least one supplier is required.", 400);
        }

        var now = DateTimeOffset.UtcNow;
        var existingVendorIds = await db.RfqVendorInvitations
            .Where(x => x.TenantId == tenantId && x.RfqId == entity.Id)
            .Select(x => x.VendorPartyId)
            .ToListAsync(cancellationToken);

        foreach (var supplierId in selectedSupplierIds.Distinct())
        {
            await EnsureSupplierAllowedAsync(tenantId, supplierId, cancellationToken);
            if (existingVendorIds.Contains(supplierId))
            {
                continue;
            }

            var portalAccessCode = GenerateVendorPortalAccessCode();
            var portalAccessIssuedAt = now;
            var portalAccessExpiresAt = now.AddDays(14);

            db.RfqVendorInvitations.Add(new RfqVendorInvitation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RfqId = entity.Id,
                VendorPartyId = supplierId,
                Status = RfqInvitationStatuses.Invited,
                InvitedAt = now,
                InvitedByUserId = actorUserId,
                PortalAccessCode = portalAccessCode,
                PortalAccessCodeIssuedAt = portalAccessIssuedAt,
                PortalAccessCodeExpiresAt = portalAccessExpiresAt,
            });
            existingVendorIds.Add(supplierId);
        }

        entity.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "rfq.vendors.invite",
            tenantId,
            actorUserId,
            "rfq",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.RfqVendorsInvited,
            "rfq",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"RFQ suppliers invited: {entity.RfqKey}"),
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<VendorPortalRfqResponse> GetVendorPortalAsync(
        Guid rfqId,
        string accessCode,
        CancellationToken cancellationToken = default)
    {
        var invitation = await ResolveVendorPortalInvitationAsync(rfqId, accessCode, cancellationToken);
        var rfq = await LoadAsync(invitation.TenantId, invitation.RfqId, cancellationToken);
        return MapVendorPortal(
            rfq,
            invitation,
            ResolveVendorQuoteForInvitation(rfq, invitation));
    }

    public async Task<VendorQuoteResponse> CreateVendorPortalQuoteAsync(
        Guid rfqId,
        string accessCode,
        VendorPortalCreateQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var invitation = await ResolveVendorPortalInvitationAsync(rfqId, accessCode, cancellationToken);
        return await CreateVendorQuoteAsync(
            invitation.TenantId,
            VendorPortalActorUserId,
            rfqId,
            new CreateVendorQuoteRequest(
                invitation.VendorPartyId,
                invitation.VendorPartyId,
                request.QuoteKey,
                request.CurrencyCode,
                request.Notes),
            cancellationToken);
    }

    public async Task<VendorQuoteResponse> UpsertVendorPortalQuoteLineAsync(
        Guid rfqId,
        Guid vendorQuoteId,
        string accessCode,
        UpsertVendorQuoteLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var invitation = await ResolveVendorPortalInvitationAsync(rfqId, accessCode, cancellationToken);
        var quote = await LoadQuoteTrackedAsync(invitation.TenantId, rfqId, vendorQuoteId, cancellationToken);
        EnsureVendorPortalQuoteOwnership(invitation, quote);
        return await UpsertQuoteLineAsync(
            invitation.TenantId,
            VendorPortalActorUserId,
            rfqId,
            vendorQuoteId,
            request,
            cancellationToken);
    }

    public async Task<VendorQuoteResponse> SubmitVendorPortalQuoteAsync(
        Guid rfqId,
        Guid vendorQuoteId,
        string accessCode,
        CancellationToken cancellationToken = default)
    {
        var invitation = await ResolveVendorPortalInvitationAsync(rfqId, accessCode, cancellationToken);
        var quote = await LoadQuoteTrackedAsync(invitation.TenantId, rfqId, vendorQuoteId, cancellationToken);
        EnsureVendorPortalQuoteOwnership(invitation, quote);
        return await SubmitVendorQuoteAsync(
            invitation.TenantId,
            VendorPortalActorUserId,
            rfqId,
            vendorQuoteId,
            cancellationToken);
    }

    public Task<VendorQuoteResponse> CreateVendorQuoteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid rfqId,
        CreateVendorQuoteRequest request,
        CancellationToken cancellationToken = default) =>
        CreateSupplierQuoteAsync(
            tenantId,
            actorUserId,
            rfqId,
            new CreateSupplierQuoteRequest(
                request.SupplierId,
                request.QuoteKey,
                request.CurrencyCode,
                request.Notes,
                request.VendorPartyId),
            cancellationToken);

    public async Task<VendorQuoteResponse> CreateSupplierQuoteAsync(
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

        var selectedSupplierId = request.SupplierId ?? request.VendorPartyId
            ?? throw new StlApiException("rfq.quote.supplier_required", "Supplier is required.", 400);
        await EnsureSupplierAllowedAsync(tenantId, selectedSupplierId, cancellationToken);
        var invited = await db.RfqVendorInvitations.AnyAsync(
            x => x.TenantId == tenantId && x.RfqId == rfqId && x.VendorPartyId == selectedSupplierId,
            cancellationToken);
        if (!invited)
        {
            throw new StlApiException(
                "rfq.vendor.not_invited",
                "Supplier must be invited before recording a quote.",
                409);
        }

        var quoteKey = NormalizeQuoteKey(request.QuoteKey);
        if (await db.VendorQuotes.AnyAsync(
                x => x.TenantId == tenantId && x.RfqId == rfqId && x.QuoteKey == quoteKey,
                cancellationToken))
        {
            throw new StlApiException("rfq.quote.duplicate", "A quote with this key already exists on the RFQ.", 409);
        }

        if (await db.VendorQuotes.AnyAsync(
                x => x.TenantId == tenantId
                    && x.RfqId == rfqId
                    && x.VendorPartyId == selectedSupplierId
                    && x.Status != VendorQuoteStatuses.Withdrawn,
                cancellationToken))
        {
            throw new StlApiException(
                "rfq.quote.vendor_exists",
                "An active quote already exists for this supplier.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var quote = new VendorQuote
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RfqId = rfq.Id,
            VendorPartyId = selectedSupplierId,
            QuoteKey = quoteKey,
            Status = VendorQuoteStatuses.Draft,
            CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode),
            Notes = NormalizeNotes(request.Notes),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.VendorQuotes.Add(quote);
        rfq.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "rfq.quote.create",
            tenantId,
            actorUserId,
            "vendor_quote",
            quote.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapQuote(await LoadQuoteAsync(tenantId, quote.Id, cancellationToken));
    }

    public async Task<VendorQuoteResponse> UpsertQuoteLineAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid rfqId,
        Guid vendorQuoteId,
        UpsertVendorQuoteLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var quote = await LoadQuoteTrackedAsync(tenantId, rfqId, vendorQuoteId, cancellationToken);
        EnsureQuoteEditable(quote);

        var rfqLineExists = await db.RfqLines.AnyAsync(
            x => x.TenantId == tenantId && x.RfqId == rfqId && x.Id == request.RfqLineId,
            cancellationToken);
        if (!rfqLineExists)
        {
            throw new StlApiException("rfq.line.not_found", "RFQ line was not found.", 404);
        }

        var now = DateTimeOffset.UtcNow;
        var existing = await db.VendorQuoteLines.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.VendorQuoteId == quote.Id && x.RfqLineId == request.RfqLineId,
            cancellationToken);
        if (existing is null)
        {
            db.VendorQuoteLines.Add(new VendorQuoteLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                VendorQuoteId = quote.Id,
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

        quote = await LoadQuoteTrackedAsync(tenantId, rfqId, vendorQuoteId, cancellationToken);
        RecalculateQuoteTotals(quote);
        quote.UpdatedAt = now;
        quote.Rfq.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        return MapQuote(await LoadQuoteAsync(tenantId, quote.Id, cancellationToken));
    }

    public async Task<VendorQuoteResponse> SubmitVendorQuoteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid rfqId,
        Guid vendorQuoteId,
        CancellationToken cancellationToken = default)
    {
        var quote = await LoadQuoteTrackedAsync(tenantId, rfqId, vendorQuoteId, cancellationToken);
        if (!VendorQuoteStatuses.Editable.Contains(quote.Status))
        {
            throw new StlApiException("rfq.quote.not_editable", "Only draft quotes can be submitted.", 409);
        }

        if (quote.Lines.Count == 0)
        {
            throw new StlApiException("rfq.quote.lines.required", "At least one quote line is required.", 400);
        }

        var now = DateTimeOffset.UtcNow;
        quote.Status = VendorQuoteStatuses.Submitted;
        quote.SubmittedAt = now;
        quote.UpdatedAt = now;
        RecalculateQuoteTotals(quote);

        var invitation = await db.RfqVendorInvitations.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.RfqId == rfqId && x.VendorPartyId == quote.VendorPartyId,
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
            "vendor_quote",
            quote.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.RfqQuoteSubmitted,
            "vendor_quote",
            quote.Id,
            new IntegrationOutboxPayload(
                tenantId,
                $"RFQ quote submitted: {quote.QuoteKey}",
                quote.VendorPartyId),
            cancellationToken: cancellationToken);

        return MapQuote(await LoadQuoteAsync(tenantId, quote.Id, cancellationToken));
    }

    public async Task<RfqQuoteComparisonResponse> CompareQuotesAsync(
        Guid tenantId,
        Guid rfqId,
        CancellationToken cancellationToken = default)
    {
        var rfq = await LoadAsync(tenantId, rfqId, cancellationToken);
        var submittedQuotes = rfq.VendorQuotes
            .Where(x => x.Status == VendorQuoteStatuses.Submitted || x.Status == VendorQuoteStatuses.Selected)
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
                    item.Quote.VendorPartyId,
                    item.Quote.VendorParty.PartyKey,
                    item.Quote.VendorParty.DisplayName,
                    item.Quote.VendorParty.ParentExternalPartyId,
                    item.Quote.VendorParty.ParentExternalParty?.DisplayName,
                    item.Quote.VendorParty.UnitKind,
                    ParseServiceTypes(item.Quote.VendorParty.ServiceTypesJson),
                    item.Quote.Status,
                    line.UnitPrice,
                    lineTotal,
                    line.LeadTimeDays,
                    lowestPrice is not null && line.Id == lowestPrice.Id,
                    fastestLead is not null && line.Id == fastestLead.Id,
                    item.Quote.VendorPartyId,
                    item.Quote.VendorParty.PartyKey,
                    item.Quote.VendorParty.DisplayName));
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
                q.VendorPartyId,
                q.VendorParty.PartyKey,
                q.VendorParty.DisplayName,
                q.VendorParty.ParentExternalPartyId,
                q.VendorParty.ParentExternalParty?.DisplayName,
                q.VendorParty.UnitKind,
                ParseServiceTypes(q.VendorParty.ServiceTypesJson),
                q.Status,
                q.TotalAmount,
                q.LeadTimeDays,
                q.Lines.Count,
                q.Id == rfq.SelectedVendorQuoteId,
                q.VendorPartyId,
                q.VendorParty.PartyKey,
                q.VendorParty.DisplayName))
            .ToList();

        return new RfqQuoteComparisonResponse(
            rfq.Id,
            rfq.RfqKey,
            rfq.Status,
            lineRows,
            summaries);
    }

    public Task<RfqResponse> SelectVendorQuoteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid rfqId,
        SelectVendorQuoteRequest request,
        CancellationToken cancellationToken = default) =>
        SelectSupplierQuoteAsync(
            tenantId,
            actorUserId,
            rfqId,
            new SelectSupplierQuoteRequest(request.VendorQuoteId),
            cancellationToken);

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

        var quote = rfq.VendorQuotes.FirstOrDefault(x => x.Id == request.SupplierQuoteId)
            ?? throw new StlApiException("rfq.quote.not_found", "Supplier quote was not found.", 404);

        if (quote.Status != VendorQuoteStatuses.Submitted)
        {
            throw new StlApiException("rfq.quote.not_submitted", "Only submitted quotes can be selected.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var other in rfq.VendorQuotes)
        {
            if (other.Id == quote.Id)
            {
                other.Status = VendorQuoteStatuses.Selected;
            }
            else if (other.Status == VendorQuoteStatuses.Submitted)
            {
                other.Status = VendorQuoteStatuses.Rejected;
            }

            other.UpdatedAt = now;
        }

        rfq.Status = RfqStatuses.Awarded;
        rfq.SelectedVendorQuoteId = quote.Id;
        rfq.AwardedVendorPartyId = quote.VendorPartyId;
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
                quote.VendorPartyId),
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

        var quote = rfq.VendorQuotes.FirstOrDefault(x => x.Id == rfq.SelectedVendorQuoteId)
            ?? throw new StlApiException("rfq.quote.not_found", "Selected vendor quote was not found.", 404);

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
                SupplierId: quote.VendorPartyId,
                VendorPartyId: quote.VendorPartyId,
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

    private static void RecalculateQuoteTotals(VendorQuote quote)
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

    private static void EnsureQuoteEditable(VendorQuote quote)
    {
        if (!VendorQuoteStatuses.Editable.Contains(quote.Status))
        {
            throw new StlApiException("rfq.quote.not_editable", "Quote can only be edited while in draft status.", 409);
        }
    }

    private Task EnsureSupplierAllowedAsync(Guid tenantId, Guid supplierId, CancellationToken cancellationToken) =>
        vendorProcurementGuard.EnsureVendorAllowedForScopeAsync(
            tenantId,
            supplierId,
            VendorRestrictionScopes.RfqInvitations,
            cancellationToken);

    private async Task<Rfq> LoadAsync(Guid tenantId, Guid rfqId, CancellationToken cancellationToken) =>
        await db.Rfqs
            .AsNoTracking()
            .Include(x => x.AwardedVendorParty).ThenInclude(x => x!.ParentExternalParty)
            .Include(x => x.Lines).ThenInclude(x => x.Part)
            .Include(x => x.VendorInvitations).ThenInclude(x => x.VendorParty).ThenInclude(x => x!.ParentExternalParty)
            .Include(x => x.VendorQuotes).ThenInclude(x => x.VendorParty).ThenInclude(x => x!.ParentExternalParty)
            .Include(x => x.VendorQuotes).ThenInclude(x => x.Lines).ThenInclude(x => x.RfqLine).ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == rfqId, cancellationToken)
        ?? throw new StlApiException("rfq.not_found", "RFQ was not found.", 404);

    private async Task<Rfq> LoadTrackedAsync(Guid tenantId, Guid rfqId, CancellationToken cancellationToken) =>
        await db.Rfqs
            .Include(x => x.Lines).ThenInclude(x => x.Part)
            .Include(x => x.VendorInvitations).ThenInclude(x => x.VendorParty).ThenInclude(x => x!.ParentExternalParty)
            .Include(x => x.VendorQuotes).ThenInclude(x => x.VendorParty).ThenInclude(x => x!.ParentExternalParty)
            .Include(x => x.VendorQuotes).ThenInclude(x => x.Lines).ThenInclude(x => x.RfqLine)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == rfqId, cancellationToken)
        ?? throw new StlApiException("rfq.not_found", "RFQ was not found.", 404);

    private async Task<VendorQuote> LoadQuoteAsync(Guid tenantId, Guid quoteId, CancellationToken cancellationToken) =>
        await db.VendorQuotes
            .AsNoTracking()
            .Include(x => x.VendorParty).ThenInclude(x => x!.ParentExternalParty)
            .Include(x => x.Lines).ThenInclude(x => x.RfqLine).ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == quoteId, cancellationToken)
        ?? throw new StlApiException("rfq.quote.not_found", "Vendor quote was not found.", 404);

    private async Task<VendorQuote> LoadQuoteTrackedAsync(
        Guid tenantId,
        Guid rfqId,
        Guid vendorQuoteId,
        CancellationToken cancellationToken) =>
        await db.VendorQuotes
            .Include(x => x.Rfq).ThenInclude(x => x.Lines).ThenInclude(x => x.Part)
        .Include(x => x.Rfq).ThenInclude(x => x.VendorInvitations)
        .Include(x => x.VendorParty).ThenInclude(x => x!.ParentExternalParty)
        .Include(x => x.Lines).ThenInclude(x => x.RfqLine)
        .FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.RfqId == rfqId && x.Id == vendorQuoteId,
            cancellationToken)
        ?? throw new StlApiException("rfq.quote.not_found", "Vendor quote was not found.", 404);

    private async Task<RfqVendorInvitation> ResolveVendorPortalInvitationAsync(
        Guid rfqId,
        string accessCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accessCode))
        {
            throw new StlApiException("rfq.vendor_portal.access_code_required", "Vendor portal access code is required.", 401);
        }

        var now = DateTimeOffset.UtcNow;

        var invitation = await db.RfqVendorInvitations
            .Include(x => x.Rfq)
                .ThenInclude(x => x.Lines).ThenInclude(x => x.Part)
            .Include(x => x.Rfq)
                .ThenInclude(x => x.VendorQuotes).ThenInclude(x => x.VendorParty).ThenInclude(x => x!.ParentExternalParty)
            .Include(x => x.Rfq)
                .ThenInclude(x => x.VendorQuotes).ThenInclude(x => x.Lines).ThenInclude(x => x.RfqLine).ThenInclude(x => x.Part)
            .Include(x => x.VendorParty).ThenInclude(x => x!.ParentExternalParty)
            .FirstOrDefaultAsync(
                x => x.RfqId == rfqId && x.PortalAccessCode == accessCode.Trim(),
                cancellationToken);

        if (invitation is null)
        {
            throw new StlApiException("rfq.vendor_portal.invalid_access_code", "Vendor portal access code was not recognized.", 401);
        }

        if (invitation.PortalAccessCodeExpiresAt < now)
        {
            throw new StlApiException("rfq.vendor_portal.expired_access_code", "Vendor portal access code has expired.", 401);
        }

        if (invitation.Rfq.Status is not (RfqStatuses.Submitted or RfqStatuses.Awarded or RfqStatuses.Closed))
        {
            throw new StlApiException("rfq.vendor_portal.not_open", "Vendor portal access is only available for submitted RFQs.", 409);
        }

        return invitation;
    }

    private static VendorQuote? ResolveVendorQuoteForInvitation(
        Rfq rfq,
        RfqVendorInvitation invitation) =>
        rfq.VendorQuotes.FirstOrDefault(x =>
            x.VendorPartyId == invitation.VendorPartyId
            && x.Status != VendorQuoteStatuses.Withdrawn);

    private static void EnsureVendorPortalQuoteOwnership(
        RfqVendorInvitation invitation,
        VendorQuote quote)
    {
        if (quote.VendorPartyId != invitation.VendorPartyId || quote.RfqId != invitation.RfqId)
        {
            throw new StlApiException("rfq.vendor_portal.quote_forbidden", "The quote does not belong to this vendor portal invitation.", 403);
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
            entity.AwardedVendorPartyId,
            entity.AwardedVendorParty?.PartyKey,
            entity.AwardedVendorParty?.DisplayName,
            entity.AwardedVendorParty?.ParentExternalPartyId,
            entity.AwardedVendorParty?.ParentExternalParty?.DisplayName,
            entity.AwardedVendorParty?.UnitKind,
            ParseServiceTypes(entity.AwardedVendorParty?.ServiceTypesJson),
            entity.SelectedVendorQuoteId,
            entity.PurchaseRequestId,
            entity.AwardedAt,
            entity.Lines.OrderBy(x => x.LineNumber).Select(MapLine).ToList(),
            entity.VendorInvitations
                .OrderBy(x => x.InvitedAt)
                .Select(MapInvitation)
                .ToList(),
            entity.VendorQuotes.OrderBy(x => x.CreatedAt).Select(MapQuote).ToList(),
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.AwardedVendorPartyId,
            entity.AwardedVendorParty?.PartyKey,
            entity.AwardedVendorParty?.DisplayName);

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

    private static RfqVendorInvitationResponse MapInvitation(RfqVendorInvitation invitation) =>
        new(
            invitation.Id,
            invitation.VendorPartyId,
            invitation.VendorParty.PartyKey,
            invitation.VendorParty.DisplayName,
            invitation.VendorParty.ParentExternalPartyId,
            invitation.VendorParty.ParentExternalParty?.DisplayName,
            invitation.VendorParty.UnitKind,
            ParseServiceTypes(invitation.VendorParty.ServiceTypesJson),
            invitation.Status,
            invitation.InvitedAt,
            invitation.PortalAccessCodeIssuedAt,
            invitation.PortalAccessCodeExpiresAt,
            invitation.PortalAccessCode,
            GenerateVendorPortalLink(invitation.RfqId, invitation.PortalAccessCode),
            invitation.VendorPartyId,
            invitation.VendorParty.PartyKey,
            invitation.VendorParty.DisplayName);

    private static VendorPortalRfqResponse MapVendorPortal(
        Rfq rfq,
        RfqVendorInvitation invitation,
        VendorQuote? quote)
    {
        var quoteLinesByRfqLineId = quote?.Lines.ToDictionary(x => x.RfqLineId) ?? new Dictionary<Guid, VendorQuoteLine>();
        return new VendorPortalRfqResponse(
            rfq.Id,
            rfq.RfqKey,
            rfq.Title,
            rfq.Notes,
            rfq.Status,
            invitation.VendorPartyId,
            invitation.VendorParty.PartyKey,
            invitation.VendorParty.DisplayName,
            invitation.VendorParty.ParentExternalPartyId,
            invitation.VendorParty.ParentExternalParty?.DisplayName,
            invitation.VendorParty.UnitKind,
            ParseServiceTypes(invitation.VendorParty.ServiceTypesJson),
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
                return new VendorPortalRfqLineResponse(
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
            rfq.UpdatedAt,
            invitation.VendorPartyId,
            invitation.VendorParty.PartyKey,
            invitation.VendorParty.DisplayName);
    }

    private static string GenerateVendorPortalAccessCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string GenerateVendorPortalLink(Guid rfqId, string portalAccessCode) =>
        string.IsNullOrWhiteSpace(portalAccessCode)
            ? string.Empty
            : $"/supplier-quote-portal?rfqId={rfqId:D}&accessCode={Uri.EscapeDataString(portalAccessCode)}";

    private static VendorQuoteResponse MapQuote(VendorQuote quote) =>
        new(
            quote.Id,
            quote.RfqId,
            quote.VendorPartyId,
            quote.VendorParty.PartyKey,
            quote.VendorParty.DisplayName,
            quote.VendorParty.ParentExternalPartyId,
            quote.VendorParty.ParentExternalParty?.DisplayName,
            quote.VendorParty.UnitKind,
            ParseServiceTypes(quote.VendorParty.ServiceTypesJson),
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
            quote.UpdatedAt,
            quote.VendorPartyId,
            quote.VendorParty.PartyKey,
            quote.VendorParty.DisplayName);

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

    private static VendorQuoteLineResponse MapQuoteLine(VendorQuoteLine line) =>
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
