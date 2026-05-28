using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class TripProofDvirEndpoints
{
    public static void MapRoutArrTripProofDvirEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/trips/{tripId:guid}")
            .WithTags("TripProofDvir")
            .RequireAuthorization();

        group.MapGet("/proofs", async (
            Guid tripId,
            HttpContext context,
            TripProofDvirService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ListProofsAsync(context.User, tripId, cancellationToken)))
        .WithName("ListTripProofs");

        group.MapPost("/proofs", async (
            Guid tripId,
            CreateTripProofRequest request,
            HttpContext context,
            TripProofDvirService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateProofAsync(context.User, tripId, request, cancellationToken)))
        .WithName("CreateTripProof");

        group.MapGet("/dvir", async (
            Guid tripId,
            HttpContext context,
            TripProofDvirService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ListDvirAsync(context.User, tripId, cancellationToken)))
        .WithName("ListTripDvirInspections");

        group.MapPost("/dvir", async (
            Guid tripId,
            SubmitTripDvirRequest request,
            HttpContext context,
            TripProofDvirService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.SubmitDvirAsync(context.User, tripId, request, cancellationToken)))
        .WithName("SubmitTripDvir");

        group.MapGet("/execution", async (
            Guid tripId,
            HttpContext context,
            TripProofDvirService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetExecutionSummaryAsync(context.User, tripId, cancellationToken)))
        .WithName("GetTripExecutionSummary");
    }
}
