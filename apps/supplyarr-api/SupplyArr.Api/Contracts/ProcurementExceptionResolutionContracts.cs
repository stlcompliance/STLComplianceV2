namespace SupplyArr.Api.Contracts;

public sealed record ProcurementExceptionResolutionTemplateResponse(
    string TemplateKey,
    string Label,
    string DefaultResolutionNotes);

public sealed record AssignProcurementExceptionRequest(
    Guid AssignedToUserId,
    DateTimeOffset? SlaDueAt);

public sealed record LinkProcurementExceptionActionsRequest(
    Guid? LinkedPurchaseRequestId,
    Guid? LinkedPurchaseOrderId);
