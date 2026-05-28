namespace SupplyArr.Api.Services;

public static class SupplyArrDemandStatusEventTypes
{
    public const string PrDrafted = "pr_drafted";

    public const string PrSubmitted = "pr_submitted";

    public const string PrApproved = "pr_approved";

    public const string PrRejected = "pr_rejected";

    public const string PoCreated = "po_created";

    public const string PoIssued = "po_issued";

    public const string ReceivingPosted = "receiving_posted";

    public const string ReceivingComplete = "receiving_complete";
}
