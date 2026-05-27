namespace RoutArr.Api.Options;

public sealed class DriverEligibilityOptions
{
    public const string SectionName = "DriverEligibility";

    public string QualificationKey { get; set; } = "driver_qualification";

    public string? RulePackKey { get; set; }

    public bool CheckTrainArrQualification { get; set; } = true;

    public bool CheckStaffArrReadiness { get; set; } = true;
}
