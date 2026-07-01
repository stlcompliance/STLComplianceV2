using RoutArr.Api.Contracts;

namespace RoutArr.Api.Services;

public sealed class SupplyArrSupplierOrderEventIngestionService(
    TripSupplierReadinessService readinessService)
{
    public Task<IngestSupplyArrSupplierOrderEventResponse> IngestAsync(
        IngestSupplyArrSupplierOrderEventRequest request,
        CancellationToken cancellationToken = default) =>
        readinessService.IngestSupplierOrderEventAsync(request, cancellationToken);
}
