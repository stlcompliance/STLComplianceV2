namespace SupplyArr.Api.Entities;

public static class ReceivingExceptionTypes
{
    public const string Short = "short";

    public const string Over = "over";

    public const string Damage = "damage";

    public const string QuantityMismatch = "quantity_mismatch";

    public const string DamagedGoods = "damaged_goods";

    public const string WrongItem = "wrong_item";

    public const string MissingItem = "missing_item";

    public const string DuplicateShipment = "duplicate_shipment";

    public const string LateDelivery = "late_delivery";

    public const string NoPo = "no_po";

    public const string MissingPackingSlip = "missing_packing_slip";

    public const string PriceMismatch = "price_mismatch";

    public const string QualityIssue = "quality_issue";

    public const string ExpiredItem = "expired_item";

    public const string HazmatDocumentMissing = "hazmat_document_missing";

    public const string RequiresInspection = "requires_inspection";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Short,
        Over,
        Damage,
        QuantityMismatch,
        DamagedGoods,
        WrongItem,
        MissingItem,
        DuplicateShipment,
        LateDelivery,
        NoPo,
        MissingPackingSlip,
        PriceMismatch,
        QualityIssue,
        ExpiredItem,
        HazmatDocumentMissing,
        RequiresInspection
    };
}
