using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class InspectionTemplate : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string TemplateKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string TemplateCategoryKey { get; set; } = string.Empty;

    public string? OwningSiteRef { get; set; }

    public string? OwningTeamRef { get; set; }

    public string? OwnerPersonId { get; set; }

    public string? OwnerRoleKey { get; set; }

    public int? EstimatedDurationMinutes { get; set; }

    public string TagsJson { get; set; } = "[]";

    public string SettingsJson { get; set; } = "{}";

    public string? CreatedByPersonId { get; set; }

    public string? UpdatedByPersonId { get; set; }

    public string? PublishedByPersonId { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public string? RetiredByPersonId { get; set; }

    public DateTimeOffset? RetiredAt { get; set; }

    public string InspectionType { get; set; } = InspectionTemplateInspectionTypes.Custom;

    public int Version { get; set; } = 1;

    public string Status { get; set; } = InspectionTemplateStatuses.Draft;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<InspectionTemplateCategory> Categories { get; set; } = [];

    public ICollection<InspectionChecklistItem> ChecklistItems { get; set; } = [];

    public ICollection<InspectionTemplateAssetType> AssetTypeLinks { get; set; } = [];
}

public static class InspectionTemplateStatuses
{
    public const string Draft = "draft";
    public const string Active = "active";
    public const string Retired = "retired";
    public const string Archived = "archived";
    public const string Inactive = Retired;
}

public static class InspectionChecklistItemTypes
{
    public const string PassFail = "pass_fail";
    public const string PassFailNa = "pass_fail_na";
    public const string YesNo = "yes_no";
    public const string YesNoNa = "yes_no_na";
    public const string Numeric = "numeric";
    public const string Text = "text";
    public const string DateTime = "date_time";
    public const string Select = "select";
    public const string MultiSelect = "multi_select";
    public const string Photo = "photo";
    public const string Signature = "signature";
    public const string MeterReading = "meter_reading";
    public const string OdometerMileage = "odometer_mileage";
    public const string EngineHours = "engine_hours";
    public const string ChecklistAcknowledgment = "checklist_acknowledgment";
    public const string BarcodeQrScan = "barcode_qr_scan";
    public const string VinSerialVerification = "vin_serial_verification";
}

public static class InspectionTemplateInspectionTypes
{
    public const string Periodic = "periodic";
    public const string AnnualDot = "annual_dot";
    public const string Dvir = "dvir";
    public const string PreTrip = "pre_trip";
    public const string PostTrip = "post_trip";
    public const string PmInspection = "pm_inspection";
    public const string ShopInspection = "shop_inspection";
    public const string SafetyInspection = "safety_inspection";
    public const string AssetIntake = "asset_intake";
    public const string ReturnToService = "return_to_service";
    public const string DamageInspection = "damage_inspection";
    public const string RoadCallInspection = "road_call_inspection";
    public const string DriverOperatorInspection = "driver_operator_inspection";
    public const string OperatorWalkaround = "operator_walkaround";
    public const string CalibrationCheck = "calibration_check";
    public const string DefectFollowUpInspection = "defect_follow_up_inspection";
    public const string AssetOnboardingInspection = "asset_onboarding_inspection";
    public const string AssetRetirementDisposalInspection = "asset_retirement_disposal_inspection";
    public const string GeneralInspection = "general_inspection";
    public const string QualityCheck = "quality_check";
    public const string RoadsideBreakdownInspection = "roadside_breakdown_inspection";
    public const string Custom = "custom";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Periodic,
        AnnualDot,
        Dvir,
        PreTrip,
        PostTrip,
        PmInspection,
        ShopInspection,
        SafetyInspection,
        AssetIntake,
        ReturnToService,
        DamageInspection,
        RoadCallInspection,
        DriverOperatorInspection,
        OperatorWalkaround,
        CalibrationCheck,
        DefectFollowUpInspection,
        AssetOnboardingInspection,
        AssetRetirementDisposalInspection,
        GeneralInspection,
        QualityCheck,
        RoadsideBreakdownInspection,
        Custom,
    };
}
