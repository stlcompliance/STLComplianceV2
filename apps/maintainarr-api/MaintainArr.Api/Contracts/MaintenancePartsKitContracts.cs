namespace MaintainArr.Api.Contracts;

public sealed record MaintenancePartsKitLineResponse(
    Guid PartsKitLineId,
    Guid PartsKitId,
    string ItemRef,
    string ItemDescriptionSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    bool Required,
    bool SubstituteAllowed,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record MaintenancePartsKitResponse(
    Guid PartsKitId,
    string KitNumber,
    string Title,
    string Description,
    IReadOnlyList<string> AssetTypeApplicability,
    IReadOnlyList<string> WorkOrderTypeApplicability,
    string? PmPlanRef,
    string Status,
    IReadOnlyList<Guid> LineRefs,
    IReadOnlyList<MaintenancePartsKitLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record MaintenancePartsKitListResponse(
    IReadOnlyList<MaintenancePartsKitResponse> Items);

public sealed record CreateMaintenancePartsKitRequest(
    string KitNumber,
    string Title,
    string? Description,
    IReadOnlyList<string>? AssetTypeApplicability,
    IReadOnlyList<string>? WorkOrderTypeApplicability,
    string? PmPlanRef);

public sealed record UpdateMaintenancePartsKitRequest(
    string Title,
    string? Description,
    IReadOnlyList<string>? AssetTypeApplicability,
    IReadOnlyList<string>? WorkOrderTypeApplicability,
    string? PmPlanRef);

public sealed record UpdateMaintenancePartsKitStatusRequest(string Status);

public sealed record CreateMaintenancePartsKitLineRequest(
    string ItemRef,
    string ItemDescriptionSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    bool Required,
    bool SubstituteAllowed);

public sealed record UpdateMaintenancePartsKitLineRequest(
    string ItemDescriptionSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    bool Required,
    bool SubstituteAllowed);
