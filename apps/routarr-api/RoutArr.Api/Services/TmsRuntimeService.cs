using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Data;

namespace RoutArr.Api.Services;

public sealed class TmsRuntimeService(
    RoutArrDbContext db,
    IRoutArrAuditService audit,
    IntegrationOutboxEnqueueService outbox)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<TransportationDemandResponse>> ListDemandsAsync(
        Guid tenantId,
        string? status,
        string? sourceProduct,
        Guid? tripId,
        CancellationToken cancellationToken = default)
    {
        var query = db.TransportationDemands
            .AsNoTracking()
            .Include(x => x.Lines)
            .Include(x => x.Requirements)
            .Include(x => x.SourceRefs)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(sourceProduct))
        {
            query = query.Where(x => x.SourceProduct == sourceProduct.Trim());
        }

        if (tripId.HasValue)
        {
            query = query.Where(x => x.TripId == tripId.Value);
        }

        return await query
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => MapDemand(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<TransportationDemandResponse> GetDemandAsync(
        Guid tenantId,
        Guid demandId,
        CancellationToken cancellationToken = default) =>
        MapDemand(await LoadDemandAsync(tenantId, demandId, true, cancellationToken));

    public async Task<TransportationDemandResponse> CreateDemandAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateTransportationDemandRequest request,
        CancellationToken cancellationToken = default)
    {
        var title = Required(request.Title, "transportation_demand.title_required", "Transportation demand title is required.");
        var origin = Required(request.OriginLocationRef, "transportation_demand.origin_required", "Origin location reference is required.");
        var destination = Required(request.DestinationLocationRef, "transportation_demand.destination_required", "Destination location reference is required.");
        var status = NormalizeStatus(request.Status, TransportationDemandStatuses.All, TransportationDemandStatuses.Draft);
        var now = DateTimeOffset.UtcNow;
        var entity = new TransportationDemand
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DemandNumber = await GenerateNumberAsync(tenantId, "TD", db.TransportationDemands, x => x.DemandNumber, cancellationToken),
            Title = title,
            Description = request.Description?.Trim() ?? string.Empty,
            Status = status,
            SourceProduct = NormalizeText(request.SourceProduct, "manual"),
            SourceObjectType = NormalizeText(request.SourceObjectType, "manual"),
            SourceObjectId = NormalizeOptional(request.SourceObjectId),
            SourceObjectNumber = NormalizeOptional(request.SourceObjectNumber),
            OriginLocationRef = origin,
            DestinationLocationRef = destination,
            RequestedPickupStartAt = request.RequestedPickupStartAt,
            RequestedPickupEndAt = request.RequestedPickupEndAt,
            RequestedDeliveryStartAt = request.RequestedDeliveryStartAt,
            RequestedDeliveryEndAt = request.RequestedDeliveryEndAt,
            PromisedPickupStartAt = request.PromisedPickupStartAt,
            PromisedPickupEndAt = request.PromisedPickupEndAt,
            PromisedDeliveryStartAt = request.PromisedDeliveryStartAt,
            PromisedDeliveryEndAt = request.PromisedDeliveryEndAt,
            ScheduledPickupStartAt = request.ScheduledPickupStartAt,
            ScheduledPickupEndAt = request.ScheduledPickupEndAt,
            ScheduledDeliveryStartAt = request.ScheduledDeliveryStartAt,
            ScheduledDeliveryEndAt = request.ScheduledDeliveryEndAt,
            TransportMode = NormalizeText(request.TransportMode, TransportationModes.Truckload),
            ServiceLevel = NormalizeText(request.ServiceLevel, "standard"),
            EquipmentRequirement = request.EquipmentRequirement?.Trim() ?? string.Empty,
            HandlingRequirementsJson = SerializeList(request.HandlingRequirements),
            CustomerRefsJson = SerializeList(request.CustomerRefs),
            OrderRefsJson = SerializeList(request.OrderRefs),
            VendorRefsJson = SerializeList(request.VendorRefs),
            RequirementRefsJson = SerializeList(request.RequirementRefs),
            PlanningStatus = status == TransportationDemandStatuses.Planning || status == TransportationDemandStatuses.Planned
                ? status
                : "not_started",
            TenderStatus = status == TransportationDemandStatuses.TenderRequired ? "required" : "not_required",
            RatingStatus = "not_rated",
            VisibilityStatus = "not_tracking",
            FreshnessState = "live",
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var lineNumber = 1;
        foreach (var line in request.Lines ?? [])
        {
            entity.Lines.Add(new TransportationDemandLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TransportationDemandId = entity.Id,
                LineNumber = lineNumber++,
                SourceProduct = NormalizeText(line.SourceProduct, entity.SourceProduct),
                SourceObjectRef = NormalizeOptional(line.SourceObjectRef),
                DescriptionSnapshot = Required(line.DescriptionSnapshot, "transportation_demand.line_description_required", "Line description is required."),
                QuantitySnapshot = line.QuantitySnapshot,
                UnitOfMeasure = NormalizeText(line.UnitOfMeasure, "each"),
                WeightSnapshot = line.WeightSnapshot,
                VolumeSnapshot = line.VolumeSnapshot,
                PalletCountSnapshot = line.PalletCountSnapshot,
                HandlingRequirementSnapshot = line.HandlingRequirementSnapshot?.Trim() ?? string.Empty,
            });
        }

        foreach (var requirement in request.Requirements ?? [])
        {
            entity.Requirements.Add(new TransportationDemandRequirement
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TransportationDemandId = entity.Id,
                RequirementType = NormalizeText(requirement.RequirementType, "other"),
                SourceProduct = NormalizeText(requirement.SourceProduct, "routarr"),
                SourceRequirementRef = NormalizeOptional(requirement.SourceRequirementRef),
                Required = requirement.Required,
                Status = NormalizeText(requirement.Status, "pending"),
                EvidenceRefsJson = SerializeList(requirement.EvidenceRefs),
            });
        }

        foreach (var sourceRef in request.SourceRefs ?? [])
        {
            entity.SourceRefs.Add(new TransportationDemandSourceRef
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TransportationDemandId = entity.Id,
                SourceProduct = Required(sourceRef.SourceProduct, "transportation_demand.source_product_required", "Source product is required."),
                SourceObjectType = Required(sourceRef.SourceObjectType, "transportation_demand.source_object_type_required", "Source object type is required."),
                SourceObjectId = Required(sourceRef.SourceObjectId, "transportation_demand.source_object_id_required", "Source object id is required."),
                SourceObjectNumber = NormalizeOptional(sourceRef.SourceObjectNumber),
                DisplayNameSnapshot = Required(sourceRef.DisplayNameSnapshot, "transportation_demand.source_display_required", "Source display label is required."),
                StatusSnapshot = NormalizeText(sourceRef.StatusSnapshot, "unknown"),
                SnapshotAt = now,
                FreshnessState = NormalizeText(sourceRef.FreshnessState, "live"),
            });
        }

        db.TransportationDemands.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAndEventAsync(
            tenantId,
            actorUserId,
            "transportation_demand.create",
            "transportation_demand",
            entity.Id,
            $"Created {entity.DemandNumber}",
            RoutArrIntegrationOutboxEventKinds.TransportationDemandCreated,
            BuildTmsPayload(entity, "Transportation demand created"),
            cancellationToken);

        return MapDemand(entity);
    }

    public async Task<TransportationDemandResponse> UpdateDemandStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid demandId,
        UpdateTransportationDemandStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadDemandAsync(tenantId, demandId, true, cancellationToken);
        var status = NormalizeStatus(request.Status, TransportationDemandStatuses.All, entity.Status);
        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (status == TransportationDemandStatuses.Canceled)
        {
            entity.CanceledAt ??= entity.UpdatedAt;
            entity.CancelReason = NormalizeOptional(request.Reason);
        }

        if (status == TransportationDemandStatuses.Planning || status == TransportationDemandStatuses.Planned)
        {
            entity.PlanningStatus = status;
        }

        if (status == TransportationDemandStatuses.Tendered || status == TransportationDemandStatuses.TenderRequired)
        {
            entity.TenderStatus = status;
        }

        await db.SaveChangesAsync(cancellationToken);
        var eventKind = status == TransportationDemandStatuses.Planned
            ? RoutArrIntegrationOutboxEventKinds.TransportationDemandPlanned
            : RoutArrIntegrationOutboxEventKinds.TransportationDemandStatusChanged;
        await WriteAuditAndEventAsync(
            tenantId,
            actorUserId,
            "transportation_demand.status",
            "transportation_demand",
            entity.Id,
            status,
            eventKind,
            BuildTmsPayload(entity, $"Transportation demand status changed to {status}"),
            cancellationToken);
        return MapDemand(entity);
    }

    public async Task<TransportationDemandResponse> LinkDemandAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid demandId,
        LinkTransportationDemandTripRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadDemandAsync(tenantId, demandId, true, cancellationToken);
        entity.TripId = request.TripId;
        entity.RouteId = request.RouteId;
        entity.DispatchPlanId = request.DispatchPlanId;
        entity.Status = request.TripId.HasValue || request.RouteId.HasValue
            ? TransportationDemandStatuses.Assigned
            : entity.Status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "transportation_demand.link",
            tenantId,
            actorUserId,
            "transportation_demand",
            entity.Id.ToString(),
            "linked",
            cancellationToken: cancellationToken);
        return MapDemand(entity);
    }

    public async Task<IReadOnlyList<CarrierTenderResponse>> ListTendersAsync(
        Guid tenantId,
        Guid? demandId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.CarrierTenders.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (demandId.HasValue)
        {
            query = query.Where(x => x.TransportationDemandId == demandId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim());
        }

        return await query.OrderByDescending(x => x.CreatedAt).Select(x => MapTender(x)).ToListAsync(cancellationToken);
    }

    public async Task<CarrierTenderResponse> CreateTenderAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateCarrierTenderRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureDemandExistsAsync(tenantId, request.TransportationDemandId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var entity = new CarrierTender
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TransportationDemandId = request.TransportationDemandId,
            TenderNumber = await GenerateNumberAsync(tenantId, "TDR", db.CarrierTenders, x => x.TenderNumber, cancellationToken),
            Status = CarrierTenderStatuses.Created,
            RoutingGuideSequence = request.RoutingGuideSequence,
            CarrierSupplierRef = Required(request.CarrierSupplierRef, "tender.carrier_required", "Carrier supplier reference is required."),
            CarrierSnapshotJson = request.CarrierSnapshotJson?.Trim() ?? "{}",
            TenderMethod = NormalizeText(request.TenderMethod, "manual"),
            ExpiresAt = request.ExpiresAt,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.CarrierTenders.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAndEventAsync(
            tenantId,
            actorUserId,
            "tender.create",
            "carrier_tender",
            entity.Id,
            entity.Status,
            RoutArrIntegrationOutboxEventKinds.TenderCreated,
            BuildTenderPayload(entity, "Carrier tender created"),
            cancellationToken);
        return MapTender(entity);
    }

    public async Task<CarrierTenderResponse> UpdateTenderStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid tenderId,
        UpdateTenderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var tender = await db.CarrierTenders.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tenderId, cancellationToken)
            ?? throw new StlApiException("tender.not_found", "Carrier tender was not found.", 404);
        tender.Status = NormalizeStatus(request.Status, CarrierTenderStatuses.All, tender.Status);
        tender.DeclineReason = NormalizeOptional(request.DeclineReason);
        tender.CounterSummary = NormalizeOptional(request.CounterSummary);
        tender.ProposedAlternative = NormalizeOptional(request.ProposedAlternative);
        tender.UpdatedAt = DateTimeOffset.UtcNow;
        if (tender.Status == CarrierTenderStatuses.Sent)
        {
            tender.SentAt ??= tender.UpdatedAt;
        }

        if (tender.Status is CarrierTenderStatuses.Accepted or CarrierTenderStatuses.Rejected or CarrierTenderStatuses.Countered)
        {
            tender.RespondedAt ??= tender.UpdatedAt;
        }

        await db.SaveChangesAsync(cancellationToken);
        var eventKind = tender.Status switch
        {
            CarrierTenderStatuses.Accepted => RoutArrIntegrationOutboxEventKinds.TenderAccepted,
            CarrierTenderStatuses.Rejected => RoutArrIntegrationOutboxEventKinds.TenderRejected,
            _ => RoutArrIntegrationOutboxEventKinds.TenderCreated,
        };
        await WriteAuditAndEventAsync(
            tenantId,
            actorUserId,
            "tender.status",
            "carrier_tender",
            tender.Id,
            tender.Status,
            eventKind,
            BuildTenderPayload(tender, $"Carrier tender status changed to {tender.Status}"),
            cancellationToken);
        return MapTender(tender);
    }

    public async Task<IReadOnlyList<FreightRatingResponse>> ListFreightRatingsAsync(
        Guid tenantId,
        Guid? demandId,
        Guid? tripId,
        CancellationToken cancellationToken = default)
    {
        var query = db.FreightRatings.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (demandId.HasValue)
        {
            query = query.Where(x => x.TransportationDemandId == demandId.Value);
        }

        if (tripId.HasValue)
        {
            query = query.Where(x => x.TripId == tripId.Value);
        }

        return await query.OrderByDescending(x => x.UpdatedAt).Select(x => MapFreightRating(x)).ToListAsync(cancellationToken);
    }

    public async Task<FreightRatingResponse> CreateFreightRatingAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateFreightRatingRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureDemandExistsAsync(tenantId, request.TransportationDemandId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var status = request.ActualFreightCost.HasValue ? FreightRatingStatuses.Actualized : FreightRatingStatuses.Estimated;
        decimal? variance = request.ActualFreightCost.HasValue && request.PlannedFreightCost.HasValue
            ? request.ActualFreightCost.Value - request.PlannedFreightCost.Value
            : null;
        if (variance is not null && variance != 0)
        {
            status = FreightRatingStatuses.VarianceDetected;
        }

        var entity = new FreightRating
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TransportationDemandId = request.TransportationDemandId,
            TripId = request.TripId,
            RatingNumber = await GenerateNumberAsync(tenantId, "FRT", db.FreightRatings, x => x.RatingNumber, cancellationToken),
            Status = status,
            BuyRateEstimate = request.BuyRateEstimate,
            SellRateEstimate = request.SellRateEstimate,
            PlannedFreightCost = request.PlannedFreightCost,
            ActualFreightCost = request.ActualFreightCost,
            CurrencyCode = NormalizeText(request.CurrencyCode, "USD").ToUpperInvariant(),
            RateSourceSnapshot = request.RateSourceSnapshot?.Trim() ?? string.Empty,
            FuelSurcharge = request.FuelSurcharge,
            AccessorialTotal = 0,
            VarianceAmount = variance,
            VarianceReason = variance is null or 0 ? null : "planned_actual_variance",
            AllocationSnapshotJson = request.AllocationSnapshotJson?.Trim() ?? "[]",
            AuditStatus = variance is null or 0 ? "not_reviewed" : "review_required",
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.FreightRatings.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        var eventKind = status == FreightRatingStatuses.VarianceDetected
            ? RoutArrIntegrationOutboxEventKinds.FreightCostVarianceDetected
            : status == FreightRatingStatuses.Actualized
                ? RoutArrIntegrationOutboxEventKinds.FreightRateActualized
                : RoutArrIntegrationOutboxEventKinds.FreightRateEstimated;
        await WriteAuditAndEventAsync(
            tenantId,
            actorUserId,
            "freight_rating.create",
            "freight_rating",
            entity.Id,
            entity.Status,
            eventKind,
            BuildRatingPayload(entity, "Freight rating created"),
            cancellationToken);
        return MapFreightRating(entity);
    }

    public async Task<FreightAccessorialResponse> CreateAccessorialAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid freightRatingId,
        CreateFreightAccessorialRequest request,
        CancellationToken cancellationToken = default)
    {
        var rating = await db.FreightRatings.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == freightRatingId, cancellationToken)
            ?? throw new StlApiException("freight_rating.not_found", "Freight rating was not found.", 404);
        var entity = new FreightAccessorial
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FreightRatingId = rating.Id,
            TransportationDemandId = rating.TransportationDemandId,
            TripId = rating.TripId,
            AccessorialType = NormalizeText(request.AccessorialType, "other"),
            Amount = request.Amount,
            CurrencyCode = NormalizeText(request.CurrencyCode, rating.CurrencyCode).ToUpperInvariant(),
            Status = NormalizeText(request.Status, "pending_review"),
            SourceEventRef = NormalizeOptional(request.SourceEventRef),
            EvidenceRefsJson = SerializeList(request.EvidenceRefs),
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.FreightAccessorials.Add(entity);
        rating.AccessorialTotal = (rating.AccessorialTotal ?? 0) + entity.Amount;
        rating.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAndEventAsync(
            tenantId,
            actorUserId,
            "freight_accessorial.create",
            "freight_accessorial",
            entity.Id,
            entity.AccessorialType,
            RoutArrIntegrationOutboxEventKinds.AccessorialCreated,
            BuildRatingPayload(rating, "Freight accessorial created"),
            cancellationToken);
        return MapAccessorial(entity);
    }

    public async Task<IReadOnlyList<VisibilityEventResponse>> ListVisibilityEventsAsync(
        Guid tenantId,
        Guid? demandId,
        Guid? tripId,
        string? reviewStatus,
        CancellationToken cancellationToken = default)
    {
        var query = db.TransportationVisibilityEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (demandId.HasValue)
        {
            query = query.Where(x => x.TransportationDemandId == demandId.Value);
        }

        if (tripId.HasValue)
        {
            query = query.Where(x => x.TripId == tripId.Value);
        }

        if (!string.IsNullOrWhiteSpace(reviewStatus))
        {
            query = query.Where(x => x.ReviewStatus == reviewStatus.Trim());
        }

        return await query.OrderByDescending(x => x.ReceivedAt).Select(x => MapVisibilityEvent(x)).ToListAsync(cancellationToken);
    }

    public async Task<VisibilityEventResponse> CreateVisibilityEventAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateVisibilityEventRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.RawExternalRef))
        {
            var existing = await db.TransportationVisibilityEvents
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.RawExternalRef == request.RawExternalRef.Trim(), cancellationToken);
            if (existing is not null)
            {
                return MapVisibilityEvent(existing);
            }
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new TransportationVisibilityEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TransportationDemandId = request.TransportationDemandId,
            TripId = request.TripId,
            StopId = request.StopId,
            EventType = NormalizeText(request.EventType, "status_update"),
            Source = NormalizeText(request.Source, "manual_check_call"),
            SourceOccurredAt = request.SourceOccurredAt ?? now,
            ReceivedAt = now,
            NormalizedStatus = request.NormalizedStatus?.Trim() ?? string.Empty,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Eta = request.Eta,
            EtaConfidence = NormalizeText(request.EtaConfidence, "unknown"),
            FreshnessState = NormalizeText(request.FreshnessState, "live"),
            ReviewStatus = NormalizeText(request.ReviewStatus, "accepted"),
            RawExternalRef = NormalizeOptional(request.RawExternalRef),
            Summary = request.Summary?.Trim() ?? string.Empty,
            UpdatedTrackingState = true,
        };
        db.TransportationVisibilityEvents.Add(entity);
        await UpsertTrackingSnapshotAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAndEventAsync(
            tenantId,
            actorUserId,
            "visibility_event.create",
            "visibility_event",
            entity.Id,
            entity.EventType,
            RoutArrIntegrationOutboxEventKinds.VisibilityEventReceived,
            new RoutArrIntegrationOutboxPayload(
                tenantId,
                "Transportation visibility event received",
                entity.TripId,
                TransportationDemandId: entity.TransportationDemandId,
                VisibilityEventId: entity.Id),
            cancellationToken);
        return MapVisibilityEvent(entity);
    }

    public async Task<IReadOnlyList<PlanningScenarioResponse>> ListPlanningScenariosAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var scenarios = await db.TransportationPlanningScenarios
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        var suggestions = await db.TransportationPlanningSuggestions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);
        return scenarios.Where(x => x.TenantId == tenantId).Select(x => MapScenario(x, suggestions)).ToList();
    }

    public async Task<PlanningScenarioResponse> CreatePlanningScenarioAsync(
        Guid tenantId,
        Guid actorUserId,
        CreatePlanningScenarioRequest request,
        CancellationToken cancellationToken = default)
    {
        var demandRefs = request.DemandRefs.Where(x => x != Guid.Empty).Distinct().ToList();
        if (demandRefs.Count == 0)
        {
            throw new StlApiException("planning.demand_required", "At least one transportation demand is required.", 400);
        }

        var demands = await db.TransportationDemands
            .Where(x => x.TenantId == tenantId && demandRefs.Contains(x.Id))
            .ToListAsync(cancellationToken);
        if (demands.Count != demandRefs.Count)
        {
            throw new StlApiException("planning.demand_not_found", "One or more transportation demands were not found.", 404);
        }

        var now = DateTimeOffset.UtcNow;
        var blockers = demands
            .Where(x => string.IsNullOrWhiteSpace(x.OriginLocationRef) || string.IsNullOrWhiteSpace(x.DestinationLocationRef))
            .Select(x => $"{x.DemandNumber}: missing origin or destination")
            .ToList();
        var warnings = demands.Count > 1
            ? new List<string> { "Multiple demands may be consolidated if windows and equipment are compatible." }
            : new List<string>();
        var scenario = new TransportationPlanningScenario
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ScenarioNumber = await GenerateNumberAsync(tenantId, "TPL", db.TransportationPlanningScenarios, x => x.ScenarioNumber, cancellationToken),
            Status = blockers.Count > 0 ? "evaluating" : "suggestions_ready",
            Objective = NormalizeText(request.Objective, "balance_cost_service"),
            DemandRefsJson = SerializeList(demandRefs.Select(x => x.ToString("D"))),
            RouteRefsJson = SerializeList((request.RouteRefs ?? []).Select(x => x.ToString("D"))),
            TripRefsJson = SerializeList((request.TripRefs ?? []).Select(x => x.ToString("D"))),
            HardBlockersJson = SerializeList(blockers),
            WarningsJson = SerializeList(warnings),
            ServiceRiskEstimate = blockers.Count > 0 ? 1 : 0.25m,
            CostEstimate = demands.Count * 250,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            EvaluatedAt = now,
        };

        db.TransportationPlanningScenarios.Add(scenario);
        var suggestion = new TransportationPlanningSuggestion
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlanningScenarioId = scenario.Id,
            SuggestionType = demands.Count > 1 ? "consolidate_demands" : "assign_private_fleet",
            Status = "proposed",
            Summary = demands.Count > 1
                ? "Review consolidation by compatible pickup and delivery windows."
                : "Review private fleet assignment or tender requirement.",
            HardBlockersJson = scenario.HardBlockersJson,
            SoftWarningsJson = scenario.WarningsJson,
            EstimatedCost = scenario.CostEstimate,
            EstimatedMiles = null,
            EstimatedServiceRisk = scenario.ServiceRiskEstimate,
            AffectedDemandRefsJson = scenario.DemandRefsJson,
            CreatedAt = now,
        };
        db.TransportationPlanningSuggestions.Add(suggestion);
        foreach (var demand in demands)
        {
            demand.PlanningStatus = "scenario_created";
            demand.Status = demand.Status == TransportationDemandStatuses.Draft
                ? TransportationDemandStatuses.Planning
                : demand.Status;
            demand.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("planning_scenario.create", tenantId, actorUserId, "planning_scenario", scenario.Id.ToString(), scenario.Status, cancellationToken: cancellationToken);
        return MapScenario(scenario, [suggestion]);
    }

    public async Task<IReadOnlyList<DriverCapacitySnapshotResponse>> ListCapacitySnapshotsAsync(
        Guid tenantId,
        string? personId,
        CancellationToken cancellationToken = default)
    {
        var query = db.DriverCapacitySnapshots.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(personId))
        {
            query = query.Where(x => x.PersonId == personId.Trim());
        }

        return await query.OrderByDescending(x => x.SnapshotAt).Select(x => MapCapacity(x)).ToListAsync(cancellationToken);
    }

    public async Task<DriverCapacitySnapshotResponse> CreateCapacitySnapshotAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateDriverCapacitySnapshotRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = new DriverCapacitySnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = Required(request.PersonId, "capacity.person_required", "Driver person id is required."),
            Source = NormalizeText(request.Source, "dispatcher"),
            ShiftWindowStart = request.ShiftWindowStart,
            ShiftWindowEnd = request.ShiftWindowEnd,
            HosRemainingMinutes = request.HosRemainingMinutes,
            DriveTimeRemainingMinutes = request.DriveTimeRemainingMinutes,
            OnDutyRemainingMinutes = request.OnDutyRemainingMinutes,
            BreakRequiredBy = request.BreakRequiredBy,
            DomicileLocationRef = request.DomicileLocationRef?.Trim() ?? string.Empty,
            FeasibilityStatus = NormalizeText(request.FeasibilityStatus, "unknown"),
            BlockerSummary = request.BlockerSummary?.Trim() ?? string.Empty,
            SnapshotAt = DateTimeOffset.UtcNow,
            FreshnessState = NormalizeText(request.FreshnessState, "live"),
        };
        db.DriverCapacitySnapshots.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("capacity_snapshot.create", tenantId, actorUserId, "driver_capacity_snapshot", entity.Id.ToString(), entity.FeasibilityStatus, cancellationToken: cancellationToken);
        return MapCapacity(entity);
    }

    public async Task<IReadOnlyList<YardEventResponse>> ListYardEventsAsync(
        Guid tenantId,
        Guid? demandId,
        Guid? tripId,
        CancellationToken cancellationToken = default)
    {
        var query = db.TransportationYardEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (demandId.HasValue)
        {
            query = query.Where(x => x.TransportationDemandId == demandId.Value);
        }

        if (tripId.HasValue)
        {
            query = query.Where(x => x.TripId == tripId.Value);
        }

        return await query.OrderByDescending(x => x.OccurredAt).Select(x => MapYardEvent(x)).ToListAsync(cancellationToken);
    }

    public async Task<YardEventResponse> CreateYardEventAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateYardEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = new TransportationYardEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TransportationDemandId = request.TransportationDemandId,
            TripId = request.TripId,
            EventType = NormalizeText(request.EventType, "gate_in"),
            TrailerAssetRef = request.TrailerAssetRef?.Trim() ?? string.Empty,
            TractorAssetRef = request.TractorAssetRef?.Trim() ?? string.Empty,
            StaffarrYardLocationRef = request.StaffarrYardLocationRef?.Trim() ?? string.Empty,
            StaffarrDockLocationRef = request.StaffarrDockLocationRef?.Trim() ?? string.Empty,
            LoadedEmptyStatus = NormalizeText(request.LoadedEmptyStatus, "unknown"),
            SealNumber = NormalizeOptional(request.SealNumber),
            Source = NormalizeText(request.Source, "dispatcher"),
            OccurredAt = request.OccurredAt ?? DateTimeOffset.UtcNow,
            EvidenceRefsJson = SerializeList(request.EvidenceRefs),
            DispatchImpact = request.DispatchImpact?.Trim() ?? string.Empty,
        };
        db.TransportationYardEvents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAndEventAsync(
            tenantId,
            actorUserId,
            "yard_event.create",
            "yard_event",
            entity.Id,
            entity.EventType,
            MapYardEventKind(entity.EventType),
            new RoutArrIntegrationOutboxPayload(
                tenantId,
                $"Yard event {entity.EventType}",
                entity.TripId,
                TransportationDemandId: entity.TransportationDemandId,
                YardEventId: entity.Id),
            cancellationToken);
        return MapYardEvent(entity);
    }

    public async Task<IReadOnlyList<CollaborationSubmissionResponse>> ListCollaborationSubmissionsAsync(
        Guid tenantId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.PortalCollaborationSubmissions.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim());
        }

        return await query.OrderByDescending(x => x.SubmittedAt).Select(x => MapCollaboration(x)).ToListAsync(cancellationToken);
    }

    public async Task<CollaborationSubmissionResponse> CreateCollaborationSubmissionAsync(
        Guid tenantId,
        CreateCollaborationSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = new PortalCollaborationSubmission
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TransportationDemandId = request.TransportationDemandId,
            TenderId = request.TenderId,
            ExternalActorType = NormalizeText(request.ExternalActorType, "carrier_contact"),
            ExternalActorRef = Required(request.ExternalActorRef, "collaboration.actor_required", "External actor reference is required."),
            ActionType = NormalizeText(request.ActionType, "submit_status_update"),
            Status = "review_required",
            SubmittedDataSummary = request.SubmittedDataSummary?.Trim() ?? string.Empty,
            UploadedRecordRefsJson = SerializeList(request.UploadedRecordRefs),
            SubmittedAt = DateTimeOffset.UtcNow,
        };
        db.PortalCollaborationSubmissions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return MapCollaboration(entity);
    }

    public async Task<CollaborationSubmissionResponse> ReviewCollaborationSubmissionAsync(
        Guid tenantId,
        string actorPersonId,
        Guid submissionId,
        ReviewCollaborationSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PortalCollaborationSubmissions.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == submissionId, cancellationToken)
            ?? throw new StlApiException("collaboration.not_found", "Collaboration submission was not found.", 404);
        entity.Status = NormalizeText(request.Status, "accepted");
        entity.ReviewedByPersonId = actorPersonId;
        entity.ReviewedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return MapCollaboration(entity);
    }

    public async Task<IReadOnlyList<FreightClaimResponse>> ListFreightClaimsAsync(
        Guid tenantId,
        Guid? demandId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.FreightClaims.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (demandId.HasValue)
        {
            query = query.Where(x => x.TransportationDemandId == demandId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim());
        }

        return await query.OrderByDescending(x => x.UpdatedAt).Select(x => MapFreightClaim(x)).ToListAsync(cancellationToken);
    }

    public async Task<FreightClaimResponse> CreateFreightClaimAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateFreightClaimRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new FreightClaim
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ClaimNumber = await GenerateNumberAsync(tenantId, "FCL", db.FreightClaims, x => x.ClaimNumber, cancellationToken),
            TransportationDemandId = request.TransportationDemandId,
            TripId = request.TripId,
            ClaimAgainstPartyType = NormalizeText(request.ClaimAgainstPartyType, "carrier"),
            ClaimReason = NormalizeText(request.ClaimReason, "damage"),
            ClaimAmount = request.ClaimAmount,
            CurrencyCode = NormalizeText(request.CurrencyCode, "USD").ToUpperInvariant(),
            Status = "requested",
            EvidenceRefsJson = SerializeList(request.EvidenceRefs),
            AssurarrNonconformanceRef = NormalizeOptional(request.AssurarrNonconformanceRef),
            SupplyarrPerformanceImpactRef = NormalizeOptional(request.SupplyarrPerformanceImpactRef),
            OrdarrCloseoutImpactRef = NormalizeOptional(request.OrdarrCloseoutImpactRef),
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.FreightClaims.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAndEventAsync(
            tenantId,
            actorUserId,
            "freight_claim.create",
            "freight_claim",
            entity.Id,
            entity.Status,
            RoutArrIntegrationOutboxEventKinds.FreightClaimRequested,
            new RoutArrIntegrationOutboxPayload(
                tenantId,
                "Freight claim requested",
                entity.TripId,
                TransportationDemandId: entity.TransportationDemandId,
                FreightClaimId: entity.Id),
            cancellationToken);
        return MapFreightClaim(entity);
    }

    public async Task<IReadOnlyList<DocumentPacketResponse>> ListDocumentPacketsAsync(
        Guid tenantId,
        Guid? demandId,
        CancellationToken cancellationToken = default)
    {
        var query = db.TransportationDocumentPacketRequests.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (demandId.HasValue)
        {
            query = query.Where(x => x.TransportationDemandId == demandId.Value);
        }

        return await query.OrderByDescending(x => x.UpdatedAt).Select(x => MapDocumentPacket(x)).ToListAsync(cancellationToken);
    }

    public async Task<DocumentPacketResponse> CreateDocumentPacketAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateDocumentPacketRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new TransportationDocumentPacketRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TransportationDemandId = request.TransportationDemandId,
            TripId = request.TripId,
            PacketType = NormalizeText(request.PacketType, "trip_packet"),
            Status = "requested",
            RequiredDocumentTypesJson = SerializeList(request.RequiredDocumentTypes),
            SourceFactsJson = request.SourceFactsJson?.Trim() ?? "{}",
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.TransportationDocumentPacketRequests.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("document_packet.create", tenantId, actorUserId, "document_packet", entity.Id.ToString(), entity.PacketType, cancellationToken: cancellationToken);
        return MapDocumentPacket(entity);
    }

    public async Task<IReadOnlyList<FinancePacketContributionResponse>> ListFinanceContributionsAsync(
        Guid tenantId,
        string? targetProduct,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.TransportationFinancePacketContributions.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(targetProduct))
        {
            query = query.Where(x => x.TargetProduct == targetProduct.Trim());
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim());
        }

        return await query.OrderByDescending(x => x.UpdatedAt).Select(x => MapFinanceContribution(x)).ToListAsync(cancellationToken);
    }

    public async Task<FinancePacketContributionResponse> CreateFinanceContributionAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateFinancePacketContributionRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new TransportationFinancePacketContribution
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ContributionNumber = await GenerateNumberAsync(tenantId, "FPC", db.TransportationFinancePacketContributions, x => x.ContributionNumber, cancellationToken),
            TransportationDemandId = request.TransportationDemandId,
            TripId = request.TripId,
            FreightRatingId = request.FreightRatingId,
            ContributionType = NormalizeText(request.ContributionType, "invoice_ready_context"),
            TargetProduct = NormalizeText(request.TargetProduct, "ordarr"),
            Status = "ready",
            OperationalSummary = request.OperationalSummary?.Trim() ?? string.Empty,
            CostSnapshotJson = request.CostSnapshotJson?.Trim() ?? "{}",
            AccessorialRefsJson = SerializeList(request.AccessorialRefs),
            ProofRefsJson = SerializeList(request.ProofRefs),
            DocumentPacketRefsJson = SerializeList(request.DocumentPacketRefs),
            ClaimRefsJson = SerializeList(request.ClaimRefs),
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.TransportationFinancePacketContributions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAndEventAsync(
            tenantId,
            actorUserId,
            "finance_packet_contribution.create",
            "finance_packet_contribution",
            entity.Id,
            entity.Status,
            RoutArrIntegrationOutboxEventKinds.FinancePacketContributionReady,
            new RoutArrIntegrationOutboxPayload(
                tenantId,
                "Transportation finance packet contribution ready",
                entity.TripId,
                TransportationDemandId: entity.TransportationDemandId,
                FreightRatingId: entity.FreightRatingId,
                FinancePacketContributionId: entity.Id),
            cancellationToken);
        return MapFinanceContribution(entity);
    }

    private async Task<TransportationDemand> LoadDemandAsync(
        Guid tenantId,
        Guid demandId,
        bool includeChildren,
        CancellationToken cancellationToken)
    {
        IQueryable<TransportationDemand> query = db.TransportationDemands;
        if (includeChildren)
        {
            query = query
                .Include(x => x.Lines)
                .Include(x => x.Requirements)
                .Include(x => x.SourceRefs);
        }

        return await query.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == demandId, cancellationToken)
            ?? throw new StlApiException("transportation_demand.not_found", "Transportation demand was not found.", 404);
    }

    private async Task EnsureDemandExistsAsync(Guid tenantId, Guid demandId, CancellationToken cancellationToken)
    {
        if (!await db.TransportationDemands.AnyAsync(x => x.TenantId == tenantId && x.Id == demandId, cancellationToken))
        {
            throw new StlApiException("transportation_demand.not_found", "Transportation demand was not found.", 404);
        }
    }

    private async Task UpsertTrackingSnapshotAsync(TransportationVisibilityEvent visibilityEvent, CancellationToken cancellationToken)
    {
        if (!visibilityEvent.TransportationDemandId.HasValue && !visibilityEvent.TripId.HasValue)
        {
            return;
        }

        var snapshot = await db.TransportationTrackingSnapshots.FirstOrDefaultAsync(
            x => x.TenantId == visibilityEvent.TenantId
                && x.TransportationDemandId == visibilityEvent.TransportationDemandId
                && x.TripId == visibilityEvent.TripId,
            cancellationToken);
        if (snapshot is null)
        {
            snapshot = new TransportationTrackingSnapshot
            {
                Id = Guid.NewGuid(),
                TenantId = visibilityEvent.TenantId,
                TransportationDemandId = visibilityEvent.TransportationDemandId,
                TripId = visibilityEvent.TripId,
            };
            db.TransportationTrackingSnapshots.Add(snapshot);
        }

        snapshot.CurrentStatus = string.IsNullOrWhiteSpace(visibilityEvent.NormalizedStatus)
            ? visibilityEvent.EventType
            : visibilityEvent.NormalizedStatus;
        snapshot.CurrentLatitude = visibilityEvent.Latitude;
        snapshot.CurrentLongitude = visibilityEvent.Longitude;
        snapshot.CurrentEta = visibilityEvent.Eta;
        snapshot.EtaConfidence = visibilityEvent.EtaConfidence;
        snapshot.LastVisibilityEventId = visibilityEvent.Id;
        snapshot.TrackingSource = visibilityEvent.Source;
        snapshot.FreshnessState = visibilityEvent.FreshnessState;
        snapshot.UpdatedAt = visibilityEvent.ReceivedAt;
    }

    private async Task WriteAuditAndEventAsync(
        Guid tenantId,
        Guid actorUserId,
        string auditAction,
        string relatedType,
        Guid relatedId,
        string auditResult,
        string eventKind,
        RoutArrIntegrationOutboxPayload payload,
        CancellationToken cancellationToken)
    {
        await audit.WriteAsync(auditAction, tenantId, actorUserId, relatedType, relatedId.ToString(), auditResult, cancellationToken: cancellationToken);
        await outbox.TryEnqueueAsync(
            tenantId,
            eventKind,
            relatedType,
            relatedId,
            payload,
            idempotencySuffix: Guid.NewGuid().ToString("N"),
            cancellationToken: cancellationToken);
    }

    private static RoutArrIntegrationOutboxPayload BuildTmsPayload(TransportationDemand demand, string summary) =>
        new(
            demand.TenantId,
            summary,
            demand.TripId,
            TransportationDemandId: demand.Id,
            TransportationDemandNumber: demand.DemandNumber,
            TransportationDemandStatus: demand.Status);

    private static RoutArrIntegrationOutboxPayload BuildTenderPayload(CarrierTender tender, string summary) =>
        new(
            tender.TenantId,
            summary,
            null,
            TransportationDemandId: tender.TransportationDemandId,
            TenderId: tender.Id);

    private static RoutArrIntegrationOutboxPayload BuildRatingPayload(FreightRating rating, string summary) =>
        new(
            rating.TenantId,
            summary,
            rating.TripId,
            TransportationDemandId: rating.TransportationDemandId,
            FreightRatingId: rating.Id);

    private static string MapYardEventKind(string eventType) =>
        eventType.ToLowerInvariant() switch
        {
            "gate_in" => RoutArrIntegrationOutboxEventKinds.GateIn,
            "gate_out" => RoutArrIntegrationOutboxEventKinds.GateOut,
            "trailer_dropped" => RoutArrIntegrationOutboxEventKinds.TrailerDropped,
            "trailer_hooked" => RoutArrIntegrationOutboxEventKinds.TrailerHooked,
            _ => RoutArrIntegrationOutboxEventKinds.GateIn,
        };

    private static TransportationDemandResponse MapDemand(TransportationDemand demand) =>
        new(
            demand.Id,
            demand.DemandNumber,
            demand.Title,
            demand.Description,
            demand.Status,
            demand.SourceProduct,
            demand.SourceObjectType,
            demand.SourceObjectId,
            demand.SourceObjectNumber,
            demand.OriginLocationRef,
            demand.DestinationLocationRef,
            demand.RequestedPickupStartAt,
            demand.RequestedPickupEndAt,
            demand.RequestedDeliveryStartAt,
            demand.RequestedDeliveryEndAt,
            demand.PromisedPickupStartAt,
            demand.PromisedPickupEndAt,
            demand.PromisedDeliveryStartAt,
            demand.PromisedDeliveryEndAt,
            demand.ScheduledPickupStartAt,
            demand.ScheduledPickupEndAt,
            demand.ScheduledDeliveryStartAt,
            demand.ScheduledDeliveryEndAt,
            demand.TransportMode,
            demand.ServiceLevel,
            demand.EquipmentRequirement,
            DeserializeList(demand.HandlingRequirementsJson),
            DeserializeList(demand.CustomerRefsJson),
            DeserializeList(demand.OrderRefsJson),
            DeserializeList(demand.VendorRefsJson),
            DeserializeList(demand.RequirementRefsJson),
            demand.PlanningStatus,
            demand.TenderStatus,
            demand.RatingStatus,
            demand.VisibilityStatus,
            demand.FreshnessState,
            demand.TripId,
            demand.RouteId,
            demand.DispatchPlanId,
            demand.CreatedByUserId,
            demand.CreatedAt,
            demand.UpdatedAt,
            demand.CanceledAt,
            demand.CancelReason,
            demand.Lines.OrderBy(x => x.LineNumber).Select(MapDemandLine).ToList(),
            demand.Requirements.OrderBy(x => x.RequirementType).Select(MapDemandRequirement).ToList(),
            demand.SourceRefs.OrderBy(x => x.SourceProduct).Select(MapDemandSourceRef).ToList());

    private static TransportationDemandLineResponse MapDemandLine(TransportationDemandLine line) =>
        new(
            line.Id,
            line.LineNumber,
            line.SourceProduct,
            line.SourceObjectRef,
            line.DescriptionSnapshot,
            line.QuantitySnapshot,
            line.UnitOfMeasure,
            line.WeightSnapshot,
            line.VolumeSnapshot,
            line.PalletCountSnapshot,
            line.HandlingRequirementSnapshot);

    private static TransportationDemandRequirementResponse MapDemandRequirement(TransportationDemandRequirement requirement) =>
        new(
            requirement.Id,
            requirement.RequirementType,
            requirement.SourceProduct,
            requirement.SourceRequirementRef,
            requirement.Required,
            requirement.Status,
            DeserializeList(requirement.EvidenceRefsJson));

    private static TransportationDemandSourceRefResponse MapDemandSourceRef(TransportationDemandSourceRef sourceRef) =>
        new(
            sourceRef.Id,
            sourceRef.SourceProduct,
            sourceRef.SourceObjectType,
            sourceRef.SourceObjectId,
            sourceRef.SourceObjectNumber,
            sourceRef.DisplayNameSnapshot,
            sourceRef.StatusSnapshot,
            sourceRef.SnapshotAt,
            sourceRef.FreshnessState);

    private static CarrierTenderResponse MapTender(CarrierTender tender) =>
        new(
            tender.Id,
            tender.TransportationDemandId,
            tender.TenderNumber,
            tender.Status,
            tender.RoutingGuideSequence,
            tender.CarrierSupplierRef,
            tender.CarrierSnapshotJson,
            tender.TenderMethod,
            tender.ExpiresAt,
            tender.SentAt,
            tender.RespondedAt,
            tender.DeclineReason,
            tender.CounterSummary,
            tender.ProposedAlternative,
            tender.CreatedAt,
            tender.UpdatedAt);

    private static FreightRatingResponse MapFreightRating(FreightRating rating) =>
        new(
            rating.Id,
            rating.TransportationDemandId,
            rating.TripId,
            rating.RatingNumber,
            rating.Status,
            rating.BuyRateEstimate,
            rating.SellRateEstimate,
            rating.PlannedFreightCost,
            rating.ActualFreightCost,
            rating.CurrencyCode,
            rating.RateSourceSnapshot,
            rating.FuelSurcharge,
            rating.AccessorialTotal,
            rating.VarianceAmount,
            rating.VarianceReason,
            rating.AllocationSnapshotJson,
            rating.AuditStatus,
            rating.CreatedAt,
            rating.UpdatedAt);

    private static FreightAccessorialResponse MapAccessorial(FreightAccessorial accessorial) =>
        new(
            accessorial.Id,
            accessorial.FreightRatingId,
            accessorial.TransportationDemandId,
            accessorial.TripId,
            accessorial.AccessorialType,
            accessorial.Amount,
            accessorial.CurrencyCode,
            accessorial.Status,
            accessorial.SourceEventRef,
            DeserializeList(accessorial.EvidenceRefsJson),
            accessorial.CreatedAt);

    private static VisibilityEventResponse MapVisibilityEvent(TransportationVisibilityEvent visibilityEvent) =>
        new(
            visibilityEvent.Id,
            visibilityEvent.TransportationDemandId,
            visibilityEvent.TripId,
            visibilityEvent.StopId,
            visibilityEvent.EventType,
            visibilityEvent.Source,
            visibilityEvent.SourceOccurredAt,
            visibilityEvent.ReceivedAt,
            visibilityEvent.NormalizedStatus,
            visibilityEvent.Latitude,
            visibilityEvent.Longitude,
            visibilityEvent.Eta,
            visibilityEvent.EtaConfidence,
            visibilityEvent.FreshnessState,
            visibilityEvent.ReviewStatus,
            visibilityEvent.RawExternalRef,
            visibilityEvent.Summary,
            visibilityEvent.UpdatedTrackingState);

    private static PlanningScenarioResponse MapScenario(
        TransportationPlanningScenario scenario,
        IReadOnlyList<TransportationPlanningSuggestion> suggestions) =>
        new(
            scenario.Id,
            scenario.ScenarioNumber,
            scenario.Status,
            scenario.Objective,
            scenario.DemandRefsJson,
            scenario.RouteRefsJson,
            scenario.TripRefsJson,
            scenario.HardBlockersJson,
            scenario.WarningsJson,
            scenario.ServiceRiskEstimate,
            scenario.CostEstimate,
            scenario.CreatedAt,
            scenario.EvaluatedAt,
            suggestions
                .Where(x => x.PlanningScenarioId == scenario.Id)
                .OrderBy(x => x.CreatedAt)
                .Select(MapSuggestion)
                .ToList());

    private static PlanningSuggestionResponse MapSuggestion(TransportationPlanningSuggestion suggestion) =>
        new(
            suggestion.Id,
            suggestion.PlanningScenarioId,
            suggestion.SuggestionType,
            suggestion.Status,
            suggestion.Summary,
            suggestion.HardBlockersJson,
            suggestion.SoftWarningsJson,
            suggestion.EstimatedCost,
            suggestion.EstimatedMiles,
            suggestion.EstimatedServiceRisk,
            suggestion.AffectedDemandRefsJson,
            suggestion.CreatedAt);

    private static DriverCapacitySnapshotResponse MapCapacity(DriverCapacitySnapshot snapshot) =>
        new(
            snapshot.Id,
            snapshot.PersonId,
            snapshot.Source,
            snapshot.ShiftWindowStart,
            snapshot.ShiftWindowEnd,
            snapshot.HosRemainingMinutes,
            snapshot.DriveTimeRemainingMinutes,
            snapshot.OnDutyRemainingMinutes,
            snapshot.BreakRequiredBy,
            snapshot.DomicileLocationRef,
            snapshot.FeasibilityStatus,
            snapshot.BlockerSummary,
            snapshot.SnapshotAt,
            snapshot.FreshnessState);

    private static YardEventResponse MapYardEvent(TransportationYardEvent yardEvent) =>
        new(
            yardEvent.Id,
            yardEvent.TransportationDemandId,
            yardEvent.TripId,
            yardEvent.EventType,
            yardEvent.TrailerAssetRef,
            yardEvent.TractorAssetRef,
            yardEvent.StaffarrYardLocationRef,
            yardEvent.StaffarrDockLocationRef,
            yardEvent.LoadedEmptyStatus,
            yardEvent.SealNumber,
            yardEvent.Source,
            yardEvent.OccurredAt,
            DeserializeList(yardEvent.EvidenceRefsJson),
            yardEvent.DispatchImpact);

    private static CollaborationSubmissionResponse MapCollaboration(PortalCollaborationSubmission submission) =>
        new(
            submission.Id,
            submission.TransportationDemandId,
            submission.TenderId,
            submission.ExternalActorType,
            submission.ExternalActorRef,
            submission.ActionType,
            submission.Status,
            submission.SubmittedDataSummary,
            DeserializeList(submission.UploadedRecordRefsJson),
            submission.SubmittedAt,
            submission.ReviewedByPersonId,
            submission.ReviewedAt);

    private static FreightClaimResponse MapFreightClaim(FreightClaim claim) =>
        new(
            claim.Id,
            claim.ClaimNumber,
            claim.TransportationDemandId,
            claim.TripId,
            claim.ClaimAgainstPartyType,
            claim.ClaimReason,
            claim.ClaimAmount,
            claim.RecoveryAmount,
            claim.CurrencyCode,
            claim.Status,
            DeserializeList(claim.EvidenceRefsJson),
            claim.AssurarrNonconformanceRef,
            claim.SupplyarrPerformanceImpactRef,
            claim.OrdarrCloseoutImpactRef,
            claim.CreatedAt,
            claim.UpdatedAt);

    private static DocumentPacketResponse MapDocumentPacket(TransportationDocumentPacketRequest packet) =>
        new(
            packet.Id,
            packet.TransportationDemandId,
            packet.TripId,
            packet.PacketType,
            packet.Status,
            DeserializeList(packet.RequiredDocumentTypesJson),
            packet.SourceFactsJson,
            packet.RecordPackageRef,
            packet.CreatedAt,
            packet.UpdatedAt);

    private static FinancePacketContributionResponse MapFinanceContribution(TransportationFinancePacketContribution contribution) =>
        new(
            contribution.Id,
            contribution.ContributionNumber,
            contribution.TransportationDemandId,
            contribution.TripId,
            contribution.FreightRatingId,
            contribution.ContributionType,
            contribution.TargetProduct,
            contribution.Status,
            contribution.OperationalSummary,
            contribution.CostSnapshotJson,
            DeserializeList(contribution.AccessorialRefsJson),
            DeserializeList(contribution.ProofRefsJson),
            DeserializeList(contribution.DocumentPacketRefsJson),
            DeserializeList(contribution.ClaimRefsJson),
            contribution.CreatedAt,
            contribution.UpdatedAt,
            contribution.SentAt,
            contribution.AcceptedAt);

    private async Task<string> GenerateNumberAsync<TEntity>(
        Guid tenantId,
        string prefix,
        IQueryable<TEntity> set,
        System.Linq.Expressions.Expression<Func<TEntity, string>> numberSelector,
        CancellationToken cancellationToken)
        where TEntity : class, IHasTenant
    {
        var count = await set.CountAsync(x => x.TenantId == tenantId, cancellationToken);
        return $"{prefix}-{DateTimeOffset.UtcNow:yyyyMMdd}-{count + 1:0000}";
    }

    private static string Required(string? value, string code, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException(code, message, 400);
        }

        return value.Trim();
    }

    private static string NormalizeStatus(string? value, IReadOnlySet<string> allowed, string fallback)
    {
        var normalized = NormalizeText(value, fallback);
        if (!allowed.Contains(normalized))
        {
            throw new StlApiException("tms.status_invalid", $"Status '{normalized}' is not valid.", 400);
        }

        return normalized;
    }

    private static string NormalizeText(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string SerializeList(IEnumerable<string>? values) =>
        JsonSerializer.Serialize((values ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList(), JsonOptions);

    private static IReadOnlyList<string> DeserializeList(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<IReadOnlyList<string>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
