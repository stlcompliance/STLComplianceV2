using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public static class RoutArrStatusQueryExtensions
{
    public static IQueryable<DispatchException> WhereDispatchExceptionOpenQueue(
        this IQueryable<DispatchException> query) =>
        query.Where(x =>
            x.Status == DispatchExceptionStatuses.Open
            || x.Status == DispatchExceptionStatuses.Assigned);

    public static IQueryable<Trip> WhereActiveDispatchStatus(this IQueryable<Trip> query) =>
        query.Where(x =>
            x.DispatchStatus == TripDispatchStatuses.Planned
            || x.DispatchStatus == TripDispatchStatuses.Assigned
            || x.DispatchStatus == TripDispatchStatuses.Dispatched
            || x.DispatchStatus == TripDispatchStatuses.InProgress);

    public static IQueryable<Trip> WhereDriverPortalScheduleStatus(this IQueryable<Trip> query) =>
        query.Where(x =>
            x.DispatchStatus == TripDispatchStatuses.Planned
            || x.DispatchStatus == TripDispatchStatuses.Assigned
            || x.DispatchStatus == TripDispatchStatuses.Dispatched
            || x.DispatchStatus == TripDispatchStatuses.InProgress
            || (x.DispatchStatus == TripDispatchStatuses.Completed && x.ClosedAt == null));
}
