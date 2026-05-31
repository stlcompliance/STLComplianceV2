using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class DispatchMessageService(
    RoutArrDbContext db,
    RoutArrAuthorizationService authorization,
    IRoutArrAuditService audit)
{
    public const string MessageCreatedAction = "dispatch_message.created";

    public const string MessageAcknowledgedAction = "dispatch_message.acknowledged";

    public async Task<DispatchThreadResponse> GetThreadAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        var trip = await GetAuthorizedTripAsync(principal, tripId, cancellationToken);
        var messages = await db.DispatchMessages
            .AsNoTracking()
            .Where(x => x.TenantId == trip.TenantId && x.TripId == trip.Id)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Select(x => MapMessage(x))
            .ToListAsync(cancellationToken);

        return new DispatchThreadResponse(trip.Id, messages);
    }

    public Task<DispatchMessageResponse> CreateDispatcherMessageAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        CreateDispatchMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireTripsAssign(principal);
        return CreateMessageAsync(
            principal,
            tripId,
            request,
            DispatchMessageSenderRoles.Dispatch,
            requireAssignedDriver: false,
            cancellationToken);
    }

    public Task<DispatchMessageResponse> CreateDriverMessageAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        CreateDispatchMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDriverPortalExecute(principal);
        return CreateMessageAsync(
            principal,
            tripId,
            request,
            DispatchMessageSenderRoles.Driver,
            requireAssignedDriver: true,
            cancellationToken);
    }

    private async Task<DispatchMessageResponse> CreateMessageAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        CreateDispatchMessageRequest request,
        string senderRole,
        bool requireAssignedDriver,
        CancellationToken cancellationToken)
    {
        var trip = await GetAuthorizedTripAsync(principal, tripId, cancellationToken);
        var actorPersonId = principal.GetPersonId().ToString();
        if (requireAssignedDriver
            && !string.Equals(trip.AssignedDriverPersonId?.Trim(), actorPersonId, StringComparison.Ordinal))
        {
            throw new StlApiException(
                "driver_portal.not_assigned",
                "You can only message dispatch for trips assigned to you.",
                403);
        }

        var body = NormalizeBody(request.Body);
        if (request.RequiresAcknowledgement
            && !string.Equals(senderRole, DispatchMessageSenderRoles.Dispatch, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "dispatch_message.acknowledgement_dispatch_only",
                "Only dispatch messages can require driver acknowledgement.",
                400);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new DispatchMessage
        {
            Id = Guid.NewGuid(),
            TenantId = trip.TenantId,
            TripId = trip.Id,
            SenderUserId = principal.GetUserId(),
            SenderPersonId = actorPersonId,
            SenderRole = senderRole,
            Body = body,
            RequiresAcknowledgement = request.RequiresAcknowledgement,
            CreatedAt = now
        };

        db.DispatchMessages.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            MessageCreatedAction,
            trip.TenantId,
            entity.SenderUserId,
            "trip",
            trip.Id.ToString(),
            senderRole,
            cancellationToken: cancellationToken);

        return MapMessage(entity);
    }

    public async Task<DispatchMessageResponse> AcknowledgeDriverMessageAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDriverPortalExecute(principal);
        var trip = await GetAuthorizedTripAsync(principal, tripId, cancellationToken);
        var actorPersonId = principal.GetPersonId().ToString();
        if (!string.Equals(trip.AssignedDriverPersonId?.Trim(), actorPersonId, StringComparison.Ordinal))
        {
            throw new StlApiException(
                "driver_portal.not_assigned",
                "You can only acknowledge messages for trips assigned to you.",
                403);
        }

        var message = await db.DispatchMessages
            .FirstOrDefaultAsync(
                x => x.TenantId == trip.TenantId
                    && x.TripId == trip.Id
                    && x.Id == messageId,
                cancellationToken)
            ?? throw new StlApiException("dispatch_message.not_found", "Message was not found.", 404);

        if (!message.RequiresAcknowledgement)
        {
            throw new StlApiException(
                "dispatch_message.acknowledgement_not_required",
                "This message does not require acknowledgement.",
                400);
        }

        if (!string.Equals(message.SenderRole, DispatchMessageSenderRoles.Dispatch, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "dispatch_message.acknowledgement_dispatch_only",
                "Only dispatch messages can be acknowledged by the driver.",
                400);
        }

        if (message.AcknowledgedAt.HasValue)
        {
            return MapMessage(message);
        }

        message.AcknowledgedByUserId = principal.GetUserId();
        message.AcknowledgedByPersonId = actorPersonId;
        message.AcknowledgedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            MessageAcknowledgedAction,
            trip.TenantId,
            message.AcknowledgedByUserId.Value,
            "dispatch_message",
            message.Id.ToString(),
            trip.Id.ToString(),
            cancellationToken: cancellationToken);

        return MapMessage(message);
    }

    private async Task<Trip> GetAuthorizedTripAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        CancellationToken cancellationToken)
    {
        authorization.RequireTripsRead(principal);
        var tenantId = principal.GetTenantId();
        var trip = await db.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken)
            ?? throw new StlApiException("trip.not_found", "Trip was not found.", 404);

        authorization.RequireTripAccess(principal, trip.CreatedByUserId, trip.AssignedDriverPersonId);
        return trip;
    }

    private static string NormalizeBody(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new StlApiException(
                "dispatch_message.body_required",
                "Message body is required.",
                400);
        }

        var trimmed = body.Trim();
        if (trimmed.Length > 2000)
        {
            throw new StlApiException(
                "dispatch_message.body_too_long",
                "Message body must be 2,000 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static DispatchMessageResponse MapMessage(DispatchMessage entity) =>
        new(
            entity.Id,
            entity.TripId,
            entity.SenderUserId,
            entity.SenderPersonId,
            entity.SenderRole,
            entity.Body,
            entity.RequiresAcknowledgement,
            entity.AcknowledgedByUserId,
            entity.AcknowledgedByPersonId,
            entity.AcknowledgedAt,
            entity.CreatedAt);
}
