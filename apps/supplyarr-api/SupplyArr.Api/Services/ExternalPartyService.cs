using SupplyArr.Api.Data;

namespace SupplyArr.Api.Services;

public sealed class ExternalPartyService(
    SupplyArrDbContext db,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
    : SupplierDirectoryService(db, integrationOutbox, audit);
