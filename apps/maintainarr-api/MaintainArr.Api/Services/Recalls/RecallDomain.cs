namespace MaintainArr.Api.Services.Recalls;

public static class RecallCampaignStatuses
{
    public const string Active = "active";
    public const string Superseded = "superseded";
    public const string Closed = "closed";
    public const string Unknown = "unknown";
}

public static class RecallSourceTypes
{
    public const string Nhtsa = "nhtsa";
    public const string TransportCanada = "transport_canada";
    public const string Oem = "oem";
    public const string Dealer = "dealer";
    public const string ManufacturerBulletin = "manufacturer_bulletin";
    public const string TenantUploaded = "tenant_uploaded";
    public const string Manual = "manual";
    public const string PaidProvider = "paid_provider";
}

public static class RecallMatchBases
{
    public const string YearMakeModel = "year_make_model";
    public const string VinDecode = "vin_decode";
    public const string CampaignNumber = "campaign_number";
    public const string SerialRange = "serial_range";
    public const string Component = "component";
    public const string TireDot = "tire_dot";
    public const string EngineFamily = "engine_family";
    public const string Manual = "manual";
    public const string Provider = "provider";
}

public static class RecallMatchConfidenceLevels
{
    public const string High = "high";
    public const string Medium = "medium";
    public const string Low = "low";
}

public static class RecallCaseStatuses
{
    public const string PotentialMatch = "potential_match";
    public const string NeedsVinCheck = "needs_vin_check";
    public const string NeedsSerialCheck = "needs_serial_check";
    public const string NeedsManualReview = "needs_manual_review";
    public const string VinConfirmedOpen = "vin_confirmed_open";
    public const string SerialConfirmedOpen = "serial_confirmed_open";
    public const string ConfirmedNotApplicable = "confirmed_not_applicable";
    public const string CompletedClaimed = "completed_claimed";
    public const string CompletedVerified = "completed_verified";
    public const string Dismissed = "dismissed";
    public const string Superseded = "superseded";
    public const string Monitoring = "monitoring";
    public const string Unknown = "unknown";
}

public static class RecallReadinessImpacts
{
    public const string NoHold = "no_hold";
    public const string Advisory = "advisory";
    public const string InspectBeforeUse = "inspect_before_use";
    public const string RepairRequired = "repair_required";
    public const string OutOfService = "out_of_service";
    public const string DoNotDrive = "do_not_drive";
    public const string ParkOutside = "park_outside";
    public const string ParkIt = "park_it";
    public const string OverTheAirUpdateAvailable = "over_the_air_update_available";
}

public static class RecallVerificationSources
{
    public const string NhtsaVinLinkout = "nhtsa_vin_linkout";
    public const string Oem = "oem";
    public const string Dealer = "dealer";
    public const string PaidProvider = "paid_provider";
    public const string ManufacturerReport = "manufacturer_report";
    public const string TenantUpload = "tenant_upload";
    public const string Manual = "manual";
}

public static class RecallVerificationMethods
{
    public const string VinLookup = "vin_lookup";
    public const string SerialLookup = "serial_lookup";
    public const string DealerStatement = "dealer_statement";
    public const string RepairInvoice = "repair_invoice";
    public const string ManufacturerFile = "manufacturer_file";
    public const string UserAttestation = "user_attestation";
    public const string Api = "api";
}

public static class RecallActionTypes
{
    public const string WorkOrder = "work_order";
    public const string InspectionItem = "inspection_item";
    public const string Defect = "defect";
    public const string ReadinessHold = "readiness_hold";
    public const string NoteOnly = "note_only";
}

public static class RecallActionStatuses
{
    public const string Planned = "planned";
    public const string Open = "open";
    public const string InProgress = "in_progress";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";
}

public static class RecallHelpers
{
    public static bool IsResolvedCaseStatus(string? status) =>
        string.Equals(status, RecallCaseStatuses.ConfirmedNotApplicable, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, RecallCaseStatuses.CompletedVerified, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, RecallCaseStatuses.Dismissed, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, RecallCaseStatuses.Superseded, StringComparison.OrdinalIgnoreCase);

    public static bool IsOpenCaseStatus(string? status) =>
        string.Equals(status, RecallCaseStatuses.PotentialMatch, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, RecallCaseStatuses.NeedsVinCheck, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, RecallCaseStatuses.NeedsSerialCheck, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, RecallCaseStatuses.NeedsManualReview, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, RecallCaseStatuses.VinConfirmedOpen, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, RecallCaseStatuses.SerialConfirmedOpen, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, RecallCaseStatuses.Monitoring, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, RecallCaseStatuses.CompletedClaimed, StringComparison.OrdinalIgnoreCase);

    public static bool IsVerifiedOpenStatus(string? status) =>
        string.Equals(status, RecallCaseStatuses.VinConfirmedOpen, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, RecallCaseStatuses.SerialConfirmedOpen, StringComparison.OrdinalIgnoreCase);

    public static bool IsCompletedStatus(string? status) =>
        string.Equals(status, RecallCaseStatuses.CompletedClaimed, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, RecallCaseStatuses.CompletedVerified, StringComparison.OrdinalIgnoreCase);

    public static bool RequiresHold(string? status, string? readinessImpact) =>
        IsVerifiedOpenStatus(status)
        || string.Equals(readinessImpact, RecallReadinessImpacts.ParkIt, StringComparison.OrdinalIgnoreCase)
        || string.Equals(readinessImpact, RecallReadinessImpacts.ParkOutside, StringComparison.OrdinalIgnoreCase)
        || string.Equals(readinessImpact, RecallReadinessImpacts.DoNotDrive, StringComparison.OrdinalIgnoreCase)
        || string.Equals(readinessImpact, RecallReadinessImpacts.OutOfService, StringComparison.OrdinalIgnoreCase)
        || string.Equals(readinessImpact, RecallReadinessImpacts.RepairRequired, StringComparison.OrdinalIgnoreCase);
}
