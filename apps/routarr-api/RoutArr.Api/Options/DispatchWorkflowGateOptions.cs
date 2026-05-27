namespace RoutArr.Api.Options;

public sealed class DispatchWorkflowGateOptions
{
    public const string SectionName = "DispatchWorkflowGates";

    public bool CheckComplianceCoreWorkflowGates { get; set; } = true;

    public string[] DriverAssignmentGateKeys { get; set; } =
    [
        "dispatch_driver_qualification",
        "dispatch_hazmat",
        "dispatch_hours_of_service",
    ];

    public string[] VehicleAssignmentGateKeys { get; set; } =
    [
        "dispatch_hazmat",
    ];
}
