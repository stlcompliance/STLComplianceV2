using RoutArr.Api.Contracts;

namespace RoutArr.Api.Services;

public sealed class SupplyArrVendorOrderEventIngestionService(
    TripVendorReadinessService readinessService)
{
    public Task<IngestSupplyArrVendorOrderEventResponse> IngestAsync(
        IngestSupplyArrVendorOrderEventRequest request,
        CancellationToken cancellationToken = default) =>
        readinessService.IngestVendorOrderEventAsync(request, cancellationToken);
}
