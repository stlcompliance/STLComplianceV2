using System.Security.Claims;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class RoutArrAuthorizationService
{
    public void RequireAuthenticated(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }
    }

    public void RequireRoutArrEntitlement(ClaimsPrincipal principal)
    {
        RequireAuthenticated(principal);
        if (!principal.HasProductEntitlement("routarr"))
        {
            throw new StlApiException("auth.not_entitled", "RoutArr entitlement is required.", 403);
        }
    }

    public void RequireTripsRead(ClaimsPrincipal principal)
    {
        RequireRoutArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "routarr_admin",
                "routarr_manager",
                "routarr_dispatcher",
                "routarr_driver",
                "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Trip read access requires RoutArr entitlement.",
            403);
    }

    public void RequireTripsCreate(ClaimsPrincipal principal)
    {
        RequireRoutArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "routarr_admin",
                "routarr_manager",
                "routarr_dispatcher"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Trip creation requires routarr.routes.create scope.",
            403);
    }

    public void RequireTripsAssign(ClaimsPrincipal principal)
    {
        RequireRoutArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "routarr_admin",
                "routarr_manager",
                "routarr_dispatcher"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Driver assignment requires routarr.dispatch.assign scope.",
            403);
    }

    public void RequireTripsPerform(ClaimsPrincipal principal)
    {
        RequireRoutArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "routarr_admin",
                "routarr_manager",
                "routarr_dispatcher",
                "routarr_driver"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Trip execution requires routarr.trips.perform scope.",
            403);
    }

    public void RequireTripsManage(ClaimsPrincipal principal)
    {
        RequireRoutArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "routarr_admin",
                "routarr_manager"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Trip management requires routarr.dispatch.manage scope.",
            403);
    }

    public bool CanViewAllTrips(ClaimsPrincipal principal)
    {
        RequireRoutArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        return MatchesRole(
            principal.GetTenantRoleKey(),
            "tenant_admin",
            "routarr_admin",
            "routarr_manager",
            "routarr_dispatcher");
    }

    public void RequireTripAccess(
        ClaimsPrincipal principal,
        Guid createdByUserId,
        string? assignedDriverPersonId)
    {
        RequireTripsRead(principal);
        if (CanViewAllTrips(principal))
        {
            return;
        }

        var actorUserId = principal.GetUserId();
        var actorPersonId = principal.GetPersonId().ToString();
        if (createdByUserId == actorUserId)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(assignedDriverPersonId)
            && string.Equals(assignedDriverPersonId, actorPersonId, StringComparison.Ordinal))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "You can only access trips you created or are assigned to drive.",
            403);
    }

    public void RequireDispatchBoardRead(ClaimsPrincipal principal) => RequireTripsRead(principal);

    public void RequireRouteCalendarRead(ClaimsPrincipal principal) => RequireTripsRead(principal);

    public void RequireDriverAvailabilityRead(ClaimsPrincipal principal) => RequireTripsRead(principal);

    public void RequireDriverAvailabilityWrite(ClaimsPrincipal principal, string targetPersonId)
    {
        RequireRoutArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (CanViewAllTrips(principal))
        {
            return;
        }

        var actorPersonId = principal.GetPersonId().ToString();
        if (MatchesRole(principal.GetTenantRoleKey(), "routarr_driver")
            && !string.IsNullOrWhiteSpace(targetPersonId)
            && string.Equals(targetPersonId.Trim(), actorPersonId, StringComparison.Ordinal))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Driver availability changes require dispatch assign scope or your own person record.",
            403);
    }

    public void RequireEquipmentAvailabilityRead(ClaimsPrincipal principal) => RequireTripsRead(principal);

    public void RequireEquipmentAvailabilityWrite(ClaimsPrincipal principal)
    {
        RequireRoutArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (CanViewAllTrips(principal))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Equipment availability changes require dispatch assign scope.",
            403);
    }

    public void RequireRoutesRead(ClaimsPrincipal principal) => RequireTripsRead(principal);

    public void RequireRoutesCreate(ClaimsPrincipal principal) => RequireTripsCreate(principal);

    public void RequireStopsPerform(ClaimsPrincipal principal) => RequireTripsPerform(principal);

    public void RequireRouteAccess(
        ClaimsPrincipal principal,
        Guid routeCreatedByUserId,
        Guid? tripCreatedByUserId,
        string? tripAssignedDriverPersonId)
    {
        RequireRoutesRead(principal);
        if (CanViewAllTrips(principal))
        {
            return;
        }

        var actorUserId = principal.GetUserId();
        var actorPersonId = principal.GetPersonId().ToString();
        if (routeCreatedByUserId == actorUserId)
        {
            return;
        }

        if (tripCreatedByUserId.HasValue && tripCreatedByUserId.Value == actorUserId)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(tripAssignedDriverPersonId)
            && string.Equals(tripAssignedDriverPersonId, actorPersonId, StringComparison.Ordinal))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "You can only access routes you created or are assigned to drive.",
            403);
    }

    public void RequireNotificationSettingsManage(ClaimsPrincipal principal)
    {
        RequireRoutArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "routarr_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Dispatch notification settings require routarr admin access.",
            403);
    }

    public void RequireTripCompletionRollupSettingsManage(ClaimsPrincipal principal) =>
        RequireNotificationSettingsManage(principal);

    private static bool MatchesRole(string roleKey, params string[] candidates) =>
        candidates.Any(candidate => string.Equals(roleKey, candidate, StringComparison.OrdinalIgnoreCase));
}
