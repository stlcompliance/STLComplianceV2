using RoutArr.Api.Contracts;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

internal sealed record RoutArrTenantSettingDefinition(
    string GroupKey,
    string GroupLabel,
    string GroupDescription,
    string SettingKey,
    string Label,
    RoutArrTenantSettingValueKind ValueKind,
    object? PlatformDefaultValue,
    string HelpText,
    int DisplayOrder,
    IReadOnlyList<RoutArrSettingOptionResponse> Options,
    decimal? MinValue = null,
    decimal? MaxValue = null);

internal static class RoutArrTenantSettingsDefinitions
{
    public const string ProductKey = "routarr";

    public static readonly IReadOnlyList<string> Permissions =
    [
        "routarr.settings.read",
        "routarr.settings.write",
        "routarr.settings.audit.read",
        "routarr.settings.overrides.read",
        "routarr.settings.overrides.write",
        "routarr.settings.integration.read",
        "routarr.settings.integration.write",
        "routarr.settings.reset",
        "routarr.settings.preview",
    ];

    public static readonly IReadOnlyList<RoutArrSettingOptionResponse> ScopeTypes =
    [
        Opt("tenant", "Tenant"),
        Opt("site", "Site"),
        Opt("terminal", "Terminal"),
        Opt("customer", "Customer"),
        Opt("carrier", "Carrier"),
        Opt("lane", "Lane"),
        Opt("routeType", "Route type"),
        Opt("serviceType", "Service type"),
        Opt("demand", "Transportation demand"),
        Opt("trip", "Trip"),
    ];

    public static IReadOnlyList<RoutArrTenantSettingDefinition> All { get; } = Build();

    public static IReadOnlyDictionary<string, RoutArrTenantSettingDefinition> ByPath { get; } =
        All.ToDictionary(x => Path(x.GroupKey, x.SettingKey), StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<RoutArrSettingGroupDefinitionResponse> ToOptionsResponse() =>
        All.GroupBy(x => x.GroupKey)
            .Select(group =>
            {
                var first = group.First();
                return new RoutArrSettingGroupDefinitionResponse(
                    first.GroupKey,
                    first.GroupLabel,
                    first.GroupDescription,
                    group.OrderBy(x => x.DisplayOrder)
                        .Select(x => new RoutArrSettingDefinitionResponse(
                            x.GroupKey,
                            x.SettingKey,
                            x.Label,
                            ToKindKey(x.ValueKind),
                            x.PlatformDefaultValue,
                            x.HelpText,
                            x.Options))
                        .ToList());
            })
            .ToList();

    public static string Path(string groupKey, string settingKey) => $"{groupKey}.{settingKey}";

    public static string ToKindKey(RoutArrTenantSettingValueKind valueKind) =>
        valueKind switch
        {
            RoutArrTenantSettingValueKind.Boolean => "boolean",
            RoutArrTenantSettingValueKind.Integer => "integer",
            RoutArrTenantSettingValueKind.Decimal => "decimal",
            RoutArrTenantSettingValueKind.Text => "text",
            RoutArrTenantSettingValueKind.Enum => "enum",
            RoutArrTenantSettingValueKind.Time => "time",
            RoutArrTenantSettingValueKind.DurationMinutes => "durationMinutes",
            RoutArrTenantSettingValueKind.MultiSelect => "multiSelect",
            _ => "text",
        };

    private static IReadOnlyList<RoutArrTenantSettingDefinition> Build()
    {
        var fields = new List<RoutArrTenantSettingDefinition>();

        AddGroup(fields, "general", "General", "Operating profile for display, ETA, planning windows, exports, and currency.",
        [
            Enum("defaultOperatingTimezone", "Default operating timezone", "America/Chicago", TimeZones(), "Controls how RoutArr displays board times and planning windows."),
            Enum("distanceUnit", "Distance unit", "miles", [Opt("miles", "Miles"), Opt("kilometers", "Kilometers")], "Used for distance display, exports, and planning thresholds."),
            Enum("weightUnit", "Weight unit", "pounds", [Opt("pounds", "Pounds"), Opt("kilograms", "Kilograms")], "Used for transportation weight display."),
            Enum("volumeUnit", "Volume unit", "cubic_feet", [Opt("cubic_feet", "Cubic feet"), Opt("cubic_meters", "Cubic meters")], "Used for transportation volume display."),
            Enum("temperatureUnit", "Temperature unit", "fahrenheit", [Opt("fahrenheit", "Fahrenheit"), Opt("celsius", "Celsius")], "Used for temperature-sensitive transportation display."),
            Enum("defaultCurrency", "Default currency", "USD", [Opt("USD", "USD"), Opt("CAD", "CAD"), Opt("MXN", "MXN"), Opt("EUR", "EUR")], "Used for rating display and exports; this is not a finance ledger setting."),
            Enum("businessWeekStartDay", "Business week start day", "monday", WeekDays(), "Controls RoutArr planning and board week grouping."),
            Time("dispatchDayCutoffTime", "Dispatch day cutoff time", "17:00", "Defines when RoutArr rolls dispatch-day views forward."),
            Enum("defaultServiceArea", "Default service area", "all", [Opt("all", "All service areas"), Opt("local", "Local"), Opt("regional", "Regional"), Opt("linehaul", "Linehaul"), Opt("dedicated", "Dedicated")], "Tenant-wide default service-area classification."),
            Enum("defaultTransportationMode", "Default transportation mode", "truckload", TransportModes(), "Default mode applied to new transportation demand when no source value is supplied."),
        ]);

        AddGroup(fields, "dispatchBoard", "Dispatch Board", "Tenant defaults for the RoutArr dispatch board experience.",
        [
            Enum("defaultBoardView", "Default board view", "driver", [Opt("driver", "Driver"), Opt("asset", "Asset"), Opt("route", "Route"), Opt("customer", "Customer"), Opt("lane", "Lane"), Opt("terminal", "Terminal"), Opt("status", "Status")], "Initial board view when no user preference exists."),
            Duration("defaultPlanningHorizon", "Default planning horizon", 1440, "Default board planning horizon."),
            Bool("showUnassignedDemandByDefault", "Show unassigned demand by default", true, "Shows demand that is not assigned to a trip."),
            Duration("autoRefreshInterval", "Auto-refresh interval", 5, "Dispatch board refresh cadence in minutes.", 1, 240),
            Enum("defaultGrouping", "Default grouping", "status", [Opt("none", "None"), Opt("status", "Status"), Opt("driver", "Driver"), Opt("asset", "Asset"), Opt("lane", "Lane"), Opt("terminal", "Terminal"), Opt("customer", "Customer")], "Default grouping for board rows."),
            Enum("defaultSortOrder", "Default sort order", "priority_then_eta", [Opt("priority_then_eta", "Priority then ETA"), Opt("eta", "ETA"), Opt("created_at", "Created time"), Opt("status", "Status"), Opt("customer", "Customer")], "Default sort order for board rows."),
            Bool("displayExternalCarrierWork", "Display external carrier work", true, "Includes tendered and carrier-executed movements on the board."),
            Bool("showCancelledCompletedTrips", "Show cancelled/completed trips", false, "Shows terminal trips on board views by default."),
            Enum("boardDensity", "Board density", "compact", [Opt("comfortable", "Comfortable"), Opt("compact", "Compact"), Opt("operations_wall", "Operations wall")], "Default board density when no user preference exists."),
        ]);

        AddGroup(fields, "demand", "Demand", "Defaults and lifecycle gates for RoutArr-owned transportation demand.",
        [
            Enum("demandCreationMode", "Demand creation mode", "mixed", [Opt("manual", "Manual"), Opt("api_only", "API-only"), Opt("order_driven", "Order-driven"), Opt("portal_driven", "Portal-driven"), Opt("mixed", "Mixed")], "Controls how new transportation demand may enter RoutArr."),
            Bool("requireSourceReference", "Require source reference", false, "Requires a typed source reference before ready/planning status."),
            Enum("defaultDemandPriority", "Default demand priority", "normal", PriorityOptions(), "Default priority for manually created demand."),
            Enum("defaultDemandStatus", "Default demand status", "draft", [Opt("draft", "Draft"), Opt("ready_for_planning", "Ready for planning")], "Initial status for new transportation demand."),
            Bool("allowDemandWithoutCustomer", "Allow demand without customer", true, "Allows draft demand without a CustomArr customer reference."),
            Bool("allowDemandWithoutKnownDestination", "Allow demand without known destination", true, "Allows draft demand before destination is known."),
            Multi("requiredDemandFields", "Required demand fields", ["origin", "destination"], DemandFieldOptions(), "Fields required before ready/planning status."),
            Duration("demandConsolidationWindow", "Demand consolidation window", 120, "Window used for consolidation suggestions."),
            Bool("splitDemandAllowed", "Split demand allowed", true, "Allows planners to split demand into multiple movements."),
            Bool("mergeDemandAllowed", "Merge demand allowed", true, "Allows planners to merge compatible demand."),
        ]);

        AddGroup(fields, "planning", "Planning", "Planning and optimization defaults for RoutArr dispatch scenarios.",
        [
            Enum("planningMode", "Planning mode", "assisted", [Opt("manual", "Manual"), Opt("assisted", "Assisted"), Opt("optimization_first", "Optimization-first")], "Controls the default planning workflow."),
            Duration("defaultPlanningHorizon", "Default planning horizon", 1440, "Planning horizon used for route suggestions."),
            Bool("autoSuggestRoutes", "Auto-suggest routes", true, "Creates suggestions for planners to accept, reject, or adjust."),
            Bool("autoCreateTripsFromApprovedPlan", "Auto-create trips from approved plan", false, "Creates trips only after a plan is approved."),
            Enum("optimizationObjective", "Optimization objective", "maximize_on_time", [Opt("minimize_miles", "Minimize miles"), Opt("minimize_cost", "Minimize cost"), Opt("maximize_on_time", "Maximize on-time"), Opt("balance_workload", "Balance workload")], "Default optimization goal."),
            Int("maxStopsPerRoute", "Max stops per route", 8, "Maximum stops suggested on one route.", 1, 200),
            Duration("maxPlannedRouteDuration", "Max planned route duration", 600, "Maximum planned route duration in minutes."),
            Decimal("maxEmptyMilesDeadhead", "Max empty miles/deadhead", 50, "Maximum empty miles threshold."),
            Bool("backhaulMatchingEnabled", "Backhaul matching enabled", true, "Allows backhaul matching suggestions."),
            Bool("consolidationEnabled", "Consolidation enabled", true, "Allows consolidation suggestions."),
            Bool("multiLegPlanningEnabled", "Multi-leg planning enabled", false, "Allows multi-leg route planning suggestions."),
            Decimal("planningConfidenceThreshold", "Planning confidence threshold", 0.75m, "Minimum confidence for highlighted suggestions.", 0, 1),
            Bool("requireDispatcherApproval", "Require dispatcher approval", true, "Requires dispatcher approval before plan activation."),
        ]);

        AddGroup(fields, "tendering", "Tendering", "Routing guide and tender execution defaults.",
        [
            Bool("routingGuideEnabled", "Routing guide enabled", false, "Enables routing-guide execution for tendering."),
            Enum("defaultTenderMethod", "Default tender method", "manual", [Opt("manual", "Manual"), Opt("sequential", "Sequential"), Opt("broadcast", "Broadcast"), Opt("auto_tender", "Auto-tender")], "Default tender workflow."),
            Duration("tenderExpirationTime", "Tender expiration time", 120, "Default tender expiration window."),
            Int("maxTenderAttempts", "Max tender attempts", 3, "Maximum tender attempts before exception.", 1, 20),
            Bool("autoFallbackToNextCarrier", "Auto-fallback to next carrier", false, "Allows fallback to next routing-guide carrier."),
            Bool("requireRateConfirmationBeforeTender", "Require rate confirmation before tender", true, "Requires rate confirmation before tender execution."),
            Bool("allowSpotQuoteFallback", "Allow spot quote fallback", true, "Allows spot quote fallback when routing guide fails."),
            Multi("tenderAcceptanceRequiredFields", "Tender acceptance required fields", ["acceptedBy", "acceptedAt"], TenderFieldOptions(), "Fields required for tender acceptance."),
            Bool("carrierRejectionReasonRequired", "Carrier rejection reason required", true, "Requires a reason when a carrier rejects tender."),
            Bool("tenderCancellationReasonRequired", "Tender cancellation reason required", true, "Requires a reason when tender is cancelled."),
        ]);

        AddGroup(fields, "numbering", "Numbering", "Tenant-scoped number format and duplicate-reference policy.",
        [
            Text("tripNumberFormat", "Trip number format", "TRIP-{yyyy}-{seq:00000}", "Generated trip number format."),
            Text("loadNumberFormat", "Load number format", "LOAD-{yyyy}-{seq:00000}", "Generated load number format."),
            Text("routeCodeFormat", "Route code format", "ROUTE-{yyyy}-{seq:00000}", "Generated route code format."),
            Enum("stopNumberBehavior", "Stop number behavior", "route_sequence", [Opt("route_sequence", "Route sequence"), Opt("global_sequence", "Global sequence"), Opt("manual", "Manual")], "How stop numbers are assigned."),
            Bool("allowManualOverrideGeneratedNumbers", "Allow manual override of generated numbers", false, "Manual overrides require permission and audit."),
            Enum("externalReferenceVisibility", "External reference visibility", "internal_only", [Opt("hidden", "Hidden"), Opt("internal_only", "Internal only"), Opt("portal_visible", "Portal visible")], "Controls where external references are shown."),
            Enum("duplicateExternalReferencePolicy", "Duplicate external reference policy", "warn", [Opt("allow", "Allow"), Opt("warn", "Warn"), Opt("block", "Block")], "Policy for duplicate external references."),
        ]);

        AddGroup(fields, "assignment", "Assignment", "Driver, asset, carrier, and override behavior for dispatch assignment.",
        [
            Enum("assignmentMode", "Assignment mode", "driver_first", [Opt("driver_first", "Driver-first"), Opt("asset_first", "Asset-first"), Opt("route_first", "Route-first"), Opt("carrier_first", "Carrier-first")], "Primary assignment workflow."),
            Enum("allowUnqualifiedDriverAssignment", "Allow unqualified driver assignment", "block", AssignmentGuardOptions(), "Controls assignment when TrainArr/StaffArr/Compliance Core signals indicate driver is not qualified."),
            Enum("allowUnavailableAssetAssignment", "Allow unavailable asset assignment", "block", AssignmentGuardOptions(), "Controls assignment when MaintainArr readiness indicates asset is unavailable."),
            Bool("requireTractorAssignment", "Require tractor assignment", false, "Requires tractor before dispatch."),
            Bool("requireTrailerAssignment", "Require trailer assignment", false, "Requires trailer before dispatch."),
            Bool("requireDriverBeforeDispatch", "Require driver before dispatch", true, "Requires driver assignment before dispatch."),
            Bool("allowTeamDrivers", "Allow team drivers", true, "Allows team driver assignments."),
            Bool("allowDriverSwapMidTrip", "Allow driver swap mid-trip", true, "Allows audited driver swaps during an active trip."),
            Bool("allowEquipmentSwapMidTrip", "Allow equipment swap mid-trip", true, "Allows audited equipment swaps during an active trip."),
            Enum("assignmentConflictBehavior", "Assignment conflict behavior", "warn", [Opt("allow", "Allow"), Opt("warn", "Warn"), Opt("block", "Block"), Opt("require_override", "Require override")], "Behavior when assignment conflicts are detected."),
            Bool("overrideReasonRequired", "Override reason required", true, "Requires a reason for assignment overrides."),
            Bool("managerApprovalRequiredForOverrides", "Manager approval required for overrides", false, "Requires manager approval for assignment overrides."),
        ]);

        AddGroup(fields, "hosAvailability", "HOS & Availability", "HOS and availability display/guard behavior without encoding legal conclusions.",
        [
            Bool("hosVisibilityEnabled", "HOS visibility enabled", true, "Shows HOS/availability snapshots in RoutArr."),
            Enum("hosSourcePreference", "HOS source preference", "mixed", [Opt("eld_integration", "ELD integration"), Opt("manual_entry", "Manual entry"), Opt("staffarr_schedule", "StaffArr schedule"), Opt("mixed", "Mixed")], "Preferred HOS data source."),
            Bool("shortHaulWorkflowSupportEnabled", "Short-haul workflow support enabled", false, "Enables short-haul workflow support without declaring compliance status."),
            Bool("manualDutyStatusEntryAllowed", "Manual duty-status entry allowed", false, "Allows manual duty-status entry where permitted."),
            Enum("requireHosCheckBeforeDispatch", "Require HOS check before dispatch", "warn", ReviewGuardOptions(), "Controls HOS check gate before dispatch."),
            Enum("requireRestAvailabilityCheck", "Require rest-availability check", "warn", ReviewGuardOptions(), "Controls rest availability gate before dispatch."),
            Duration("hosWarningThreshold", "HOS warning threshold", 120, "Warning threshold in minutes."),
            Enum("driverAvailabilitySource", "Driver availability source", "staffarr_schedule", [Opt("staffarr_schedule", "StaffArr schedule"), Opt("routarr_availability", "RoutArr availability"), Opt("eld", "ELD"), Opt("mixed", "Mixed")], "Preferred driver availability source."),
            Enum("outOfServiceDriverBehavior", "Out-of-service driver behavior", "show_unavailable", [Opt("hide", "Hide"), Opt("show_unavailable", "Show unavailable"), Opt("allow_override", "Allow override")], "How out-of-service drivers are shown."),
        ]);

        AddGroup(fields, "stopsAppointments", "Stops & Appointments", "Stop lifecycle, appointment tolerance, and confirmation defaults.",
        [
            Bool("appointmentRequiredByDefault", "Appointment required by default", false, "Requires appointment window on new stops by default."),
            Duration("appointmentWindowTolerance", "Appointment window tolerance", 30, "Tolerance around appointment windows."),
            Enum("earlyArrivalBehavior", "Early arrival behavior", "warn", [Opt("allow", "Allow"), Opt("warn", "Warn"), Opt("block", "Block"), Opt("require_reason", "Require reason")], "Behavior for early arrivals."),
            Duration("lateArrivalThreshold", "Late arrival threshold", 30, "Threshold before late arrival is flagged."),
            Multi("stopConfirmationRequiredFields", "Stop confirmation required fields", ["timestamp"], StopConfirmationFieldOptions(), "Fields required for stop confirmation."),
            Multi("pickupConfirmationRequirements", "Pickup confirmation requirements", ["proof"], StopConfirmationFieldOptions(), "Pickup-specific confirmation requirements."),
            Multi("deliveryConfirmationRequirements", "Delivery confirmation requirements", ["proof"], StopConfirmationFieldOptions(), "Delivery-specific confirmation requirements."),
            Bool("allowStopResequencing", "Allow stop resequencing", true, "Allows audited stop resequencing."),
            Bool("allowDriverAddedStops", "Allow driver-added stops", false, "Allows driver-added stops with review."),
            Bool("requireReasonForStopChange", "Require reason for stop change", true, "Requires reason for stop changes."),
            Bool("autoCompleteStopFromGeofence", "Auto-complete stop from geofence", false, "Allows geofence-based stop completion with correction audit."),
            Bool("manualArrivalDepartureAllowed", "Manual arrival/departure allowed", true, "Allows manual arrival/departure corrections with audit reason."),
        ]);

        AddGroup(fields, "dockYardHandoffs", "Dock/Yard Handoffs", "LoadArr handoff, yard event, drop-hook, and dwell clock behavior.",
        [
            Bool("notifyLoadArrForInboundAppointments", "Notify LoadArr for inbound appointments", true, "Sends inbound appointment visibility to LoadArr."),
            Bool("notifyLoadArrForEtaChanges", "Notify LoadArr for ETA changes", true, "Sends ETA changes to LoadArr."),
            Bool("notifyLoadArrOnArrival", "Notify LoadArr on arrival", true, "Sends arrival visibility to LoadArr."),
            Bool("notifyLoadArrOnDeparture", "Notify LoadArr on departure", true, "Sends departure visibility to LoadArr."),
            Bool("requireDockAppointmentBeforeDispatch", "Require dock appointment before dispatch", false, "Requires dock appointment before dispatch where applicable."),
            Bool("allowRoutArrCreatedDockRequest", "Allow RoutArr-created dock request", true, "Allows RoutArr to request a dock appointment without owning LoadArr dock execution."),
            Bool("yardEventCaptureEnabled", "Yard event capture enabled", true, "Enables RoutArr yard/gate/drop-hook event capture."),
            Bool("dropHookEnabled", "Drop-hook enabled", true, "Enables drop-hook workflows."),
            Enum("yardLocationSource", "Yard location source", "staffarr", [Opt("staffarr", "StaffArr locations"), Opt("integration_snapshot", "Integration snapshot"), Opt("manual_snapshot", "Manual snapshot")], "Source of yard location references."),
            Bool("autoCreateYardEventFromGeofence", "Auto-create yard event from geofence", false, "Creates yard event candidates from geofence signals."),
            Bool("trailerDwellClockEnabled", "Trailer dwell clock enabled", true, "Tracks trailer dwell clocks."),
            Enum("detentionClockSource", "Detention clock source", "appointment_time", [Opt("appointment_time", "Appointment time"), Opt("actual_arrival", "Actual arrival"), Opt("gate_in", "Gate-in"), Opt("dock_in", "Dock-in")], "Basis for detention clocks."),
        ]);

        AddGroup(fields, "trackingVisibility", "Tracking & Visibility", "Tracking, ETA, and customer/carrier visibility policy.",
        [
            Bool("visibilityEnabled", "Visibility enabled", true, "Enables normalized RoutArr visibility events."),
            Enum("defaultTrackingMethod", "Default tracking method", "driver_mobile", [Opt("driver_mobile", "Driver mobile"), Opt("eld_gps", "ELD/GPS"), Opt("carrier_portal", "Carrier portal"), Opt("manual", "Manual"), Opt("api", "API")], "Default tracking source."),
            Duration("trackingFrequency", "Tracking frequency", 15, "Tracking refresh frequency in minutes."),
            Int("geofenceRadius", "Geofence radius", 500, "Default geofence radius in meters.", 25, 10000),
            Enum("etaCalculationMethod", "ETA calculation method", "simple_distance_time", [Opt("manual", "Manual"), Opt("external_provider", "External provider"), Opt("simple_distance_time", "Simple distance/time"), Opt("integration", "Integration")], "Default ETA calculation method."),
            Duration("etaRefreshFrequency", "ETA refresh frequency", 15, "ETA refresh frequency in minutes."),
            Bool("autoStatusUpdates", "Auto-status updates", true, "Allows audited auto-status updates from visibility events."),
            Bool("customerVisibilityEnabled", "Customer visibility enabled", false, "Enables customer-visible shipment tracking."),
            Bool("carrierVisibilityEnabled", "Carrier visibility enabled", true, "Enables carrier-visible shipment tracking."),
            Enum("locationPrecisionSharing", "Location precision sharing", "milestone_only", [Opt("exact", "Exact"), Opt("approximate", "Approximate"), Opt("milestone_only", "Milestone-only")], "Controls location precision shared externally."),
            Duration("trackingExceptionThreshold", "Tracking exception threshold", 60, "Tracking gap threshold in minutes."),
            Bool("allowManualTrackingCorrection", "Allow manual tracking correction", true, "Allows audited manual tracking corrections."),
        ]);

        AddGroup(fields, "exceptions", "Exceptions", "Exception taxonomy, auto-create, ownership, escalation, and notification defaults.",
        [
            Bool("exceptionTaxonomyEnabled", "Exception taxonomy enabled", true, "Uses first-class RoutArr exception taxonomy."),
            Multi("requiredExceptionFields", "Required exception fields", ["owner", "severity", "affectedRecord"], ExceptionFieldOptions(), "Fields required for exception creation/resolution."),
            Enum("defaultSeverityMapping", "Default severity mapping", "operational_risk", [Opt("operational_risk", "Operational risk"), Opt("customer_impact", "Customer impact"), Opt("safety_first", "Safety first"), Opt("sla_first", "SLA first")], "Default severity mapping profile."),
            Bool("autoCreateExceptionFromLateEta", "Auto-create exception from late ETA", true, "Creates an exception when ETA threshold is crossed."),
            Bool("autoCreateExceptionFromTrackingGap", "Auto-create exception from tracking gap", true, "Creates an exception from tracking gaps."),
            Bool("autoCreateExceptionFromAssetBreakdown", "Auto-create exception from asset breakdown", true, "Creates or updates exception from MaintainArr breakdown events."),
            Bool("autoCreateExceptionFromReceivingIssue", "Auto-create exception from receiving issue", true, "Creates or updates exception from LoadArr/AssurArr receiving issues."),
            Enum("exceptionOwnerAssignment", "Exception owner assignment", "dispatcher_queue", [Opt("dispatcher_queue", "Dispatcher queue"), Opt("trip_owner", "Trip owner"), Opt("terminal_queue", "Terminal queue"), Opt("manual", "Manual")], "Default owner assignment rule."),
            Duration("escalationThresholds", "Escalation thresholds", 60, "Default escalation threshold in minutes."),
            Bool("requireCorrectiveAction", "Require corrective action", false, "Requires corrective action on exception resolution."),
            Bool("requireResolutionNote", "Require resolution note", true, "Requires resolution notes."),
            Multi("customerNotificationRules", "Customer notification rules", ["late_delivery"], NotificationRuleOptions(), "Customer-visible notification rules."),
            Multi("internalNotificationRules", "Internal notification rules", ["late_eta", "tracking_gap"], NotificationRuleOptions(), "Internal notification rules."),
        ]);

        AddGroup(fields, "detentionAccessorials", "Detention & Accessorials", "Detention, layover, accessorial evidence, and finance-packet contribution behavior.",
        [
            Bool("detentionTrackingEnabled", "Detention tracking enabled", true, "Tracks detention candidates."),
            Duration("defaultFreeTime", "Default free time", 120, "Default free time in minutes."),
            Enum("detentionStartBasis", "Detention start basis", "appointment_time", [Opt("appointment_time", "Appointment time"), Opt("actual_arrival", "Actual arrival"), Opt("gate_in", "Gate-in"), Opt("dock_in", "Dock-in")], "Basis for detention start."),
            Enum("detentionEndBasis", "Detention end basis", "departure", [Opt("departure", "Departure"), Opt("gate_out", "Gate-out"), Opt("dock_out", "Dock-out"), Opt("manual_close", "Manual close")], "Basis for detention end."),
            Bool("layoverTrackingEnabled", "Layover tracking enabled", true, "Tracks layover candidates."),
            Bool("tonuTrackingEnabled", "TONU tracking enabled", true, "Tracks truck ordered not used candidates."),
            Bool("lumperTrackingEnabled", "Lumper tracking enabled", true, "Tracks lumper accessorial candidates."),
            Bool("accessorialApprovalRequired", "Accessorial approval required", true, "Requires approval before accessorial contribution."),
            Multi("requiredEvidenceForAccessorial", "Required evidence for accessorial", ["event", "document"], EvidenceOptions(), "Evidence required for accessorial approval."),
            Bool("financePacketContributionEnabled", "Finance packet contribution enabled", true, "Packages approved operational facts for downstream finance flows."),
            Decimal("rateAuditWarningThreshold", "Rate audit warning threshold", 0.1m, "Variance ratio that triggers rate-audit warning.", 0, 1),
        ]);

        AddGroup(fields, "rating", "Rating", "Freight rating and operational cost snapshot behavior.",
        [
            Bool("ratingEnabled", "Rating enabled", true, "Enables operational rating and freight cost snapshots."),
            Enum("defaultRatingMode", "Default rating mode", "manual", [Opt("manual", "Manual"), Opt("tariff_table", "Tariff/table"), Opt("contract", "Contract"), Opt("spot", "Spot"), Opt("integration", "Integration")], "Default rating mode."),
            Enum("requireRateBeforeDispatch", "Require rate before dispatch", "warn", [Opt("off", "Off"), Opt("warn", "Warn"), Opt("block", "Block")], "Rate gate before dispatch."),
            Enum("requireRateBeforeTender", "Require rate before tender", "warn", [Opt("off", "Off"), Opt("warn", "Warn"), Opt("block", "Block")], "Rate gate before tender."),
            Bool("fuelSurchargeEnabled", "Fuel surcharge enabled", true, "Enables fuel surcharge capture."),
            Enum("accessorialCatalogSource", "Accessorial catalog source", "routarr", [Opt("routarr", "RoutArr operational catalog"), Opt("supplyarr_snapshot", "SupplyArr snapshot"), Opt("external_integration", "External integration")], "Source for accessorial catalog entries."),
            Decimal("rateVarianceThreshold", "Rate variance threshold", 0.1m, "Variance ratio for freight rate warnings.", 0, 1),
            Decimal("spotQuoteApprovalThreshold", "Spot quote approval threshold", 500m, "Spot quote amount requiring approval."),
            Enum("currencyConversionBehavior", "Currency conversion behavior", "display_only", [Opt("display_only", "Display only"), Opt("snapshot_conversion", "Snapshot conversion"), Opt("block_mismatch", "Block mismatch")], "Currency conversion behavior for operational display."),
            Bool("storeRateSnapshots", "Store rate snapshots", true, "Stores rate-at-time snapshots where operationally needed."),
        ]);

        AddGroup(fields, "documents", "Documents", "Transportation document packet expectations and RecordArr handoff behavior.",
        [
            Bool("documentPacketEnabled", "Document packet enabled", true, "Enables transportation document packet requests."),
            Multi("requiredPickupDocuments", "Required pickup documents", ["bill_of_lading"], DocumentTypeOptions(), "Default pickup documents."),
            Multi("requiredDeliveryDocuments", "Required delivery documents", ["proof_of_delivery"], DocumentTypeOptions(), "Default delivery documents."),
            Bool("requireDocumentBeforeStopCompletion", "Require document before stop completion", false, "Blocks stop completion until configured documents are present."),
            Bool("requireDocumentBeforeTripCloseout", "Require document before trip closeout", true, "Blocks trip closeout until configured documents are present."),
            Bool("recordArrHandoffEnabled", "RecordArr handoff enabled", true, "Sends uploaded/retained documents through RecordArr when enabled."),
            Bool("allowMobileUpload", "Allow mobile upload", true, "Allows mobile document upload."),
            Bool("allowCarrierPortalUpload", "Allow carrier portal upload", true, "Allows carrier portal document upload."),
            Bool("documentReviewRequired", "Document review required", false, "Requires review before document packet is considered complete."),
            Duration("missingDocumentEscalation", "Missing document escalation", 240, "Escalation delay for missing documents."),
        ]);

        AddGroup(fields, "portal", "Portal", "External collaboration and portal sharing behavior.",
        [
            Bool("carrierPortalEnabled", "Carrier portal enabled", true, "Enables carrier collaboration when authorized."),
            Bool("customerPortalVisibilityEnabled", "Customer portal visibility enabled", false, "Enables customer-facing visibility."),
            Bool("shipperConsigneePortalEnabled", "Shipper/consignee portal enabled", false, "Enables shipper/consignee collaboration."),
            Duration("portalInviteExpiration", "Portal invite expiration", 4320, "Portal invite expiration in minutes."),
            Bool("requireMfaForPortalUsersWhereSupported", "Require MFA for portal users where supported", true, "Requires MFA for portal users where supported."),
            Enum("externalStatusUpdateApproval", "External status update approval", "review_task", [Opt("auto_apply", "Auto-apply"), Opt("review_task", "Create review task"), Opt("block", "Block")], "How external status updates are handled."),
            Enum("externalDocumentUploadApproval", "External document upload approval", "review_task", [Opt("auto_apply", "Auto-apply"), Opt("review_task", "Create review task"), Opt("block", "Block")], "How external document uploads are handled."),
            Bool("externalExceptionReportingEnabled", "External exception reporting enabled", true, "Allows external exception reporting."),
            Bool("hideInternalNotesFromPortal", "Hide internal notes from portal", true, "Keeps internal notes out of external portals."),
            Enum("sharedFieldsPolicy", "Shared fields policy", "milestone_summary", [Opt("minimal", "Minimal"), Opt("milestone_summary", "Milestone summary"), Opt("operational_summary", "Operational summary"), Opt("custom_policy", "Custom policy")], "Controls fields visible to external collaborators."),
        ]);

        AddGroup(fields, "notifications", "Notifications", "Operational notification channels, alert rules, escalation, quiet hours, and digest behavior.",
        [
            Multi("notificationChannels", "Notification channels", ["in_app"], [Opt("in_app", "In-app"), Opt("email", "Email"), Opt("sms", "SMS"), Opt("webhook", "Webhook")], "Default notification delivery channels."),
            Multi("dispatcherAlertRules", "Dispatcher alert rules", ["late_eta", "assignment_block"], NotificationRuleOptions(), "Dispatcher alert rules."),
            Multi("driverAlertRules", "Driver alert rules", ["assignment_changed"], NotificationRuleOptions(), "Driver alert rules."),
            Multi("carrierAlertRules", "Carrier alert rules", ["tender_expiring"], NotificationRuleOptions(), "Carrier alert rules."),
            Multi("customerAlertRules", "Customer alert rules", ["late_delivery"], NotificationRuleOptions(), "Customer alert rules."),
            Multi("loadArrAlertRules", "LoadArr alert rules", ["eta_change", "arrival"], NotificationRuleOptions(), "LoadArr alert rules."),
            Multi("maintainArrAlertRules", "MaintainArr alert rules", ["asset_breakdown"], NotificationRuleOptions(), "MaintainArr alert rules."),
            Duration("escalationDelay", "Escalation delay", 60, "Default escalation delay in minutes."),
            Text("quietHours", "Quiet hours", "22:00-06:00", "Quiet-hours window for noncritical notifications."),
            Bool("digestEnabled", "Digest enabled", true, "Enables noncritical notification digests."),
        ]);

        AddGroup(fields, "statusModel", "Status Model", "Lifecycle variants and status transition guardrails.",
        [
            Enum("demandLifecycleVariant", "Demand lifecycle variant", "standard", [Opt("standard", "Standard"), Opt("planning_heavy", "Planning-heavy"), Opt("tender_heavy", "Tender-heavy")], "Demand lifecycle variant."),
            Enum("tripLifecycleVariant", "Trip lifecycle variant", "standard", [Opt("standard", "Standard"), Opt("appointment_driven", "Appointment-driven"), Opt("proof_driven", "Proof-driven")], "Trip lifecycle variant."),
            Bool("requireApprovalBeforeDispatch", "Require approval before dispatch", false, "Requires approval before dispatch."),
            Bool("requireCloseoutReview", "Require closeout review", true, "Requires closeout review."),
            Bool("allowReopenCompletedTrip", "Allow reopen completed trip", false, "Allows completed trip reopen with permission and reason."),
            Bool("cancellationReasonRequired", "Cancellation reason required", true, "Requires cancellation reason."),
            Bool("rejectionReasonRequired", "Rejection reason required", true, "Requires rejection reason."),
            Multi("statusTransitionGuardrails", "Status transition guardrails", ["reason_for_terminal", "block_invalid_reference"], [Opt("reason_for_terminal", "Reason for terminal statuses"), Opt("block_invalid_reference", "Block invalid references"), Opt("require_approval", "Require approval"), Opt("require_documents", "Require documents")], "Configured status transition guardrails."),
            Text("customDisplayLabels", "Custom display labels", "", "Optional display-label overrides; machine statuses remain canonical."),
        ]);

        AddGroup(fields, "integrations", "Integrations", "Behavior toggles for product and external integrations. Credentials are not stored here.",
        [
            Bool("eldGpsIntegrationEnabled", "ELD/GPS integration enabled", false, "Enables ELD/GPS behavior toggles."),
            Bool("tmsImportIntegrationEnabled", "TMS import integration enabled", false, "Enables TMS import behavior toggles."),
            Bool("wmsLoadArrIntegrationEnabled", "WMS/LoadArr integration enabled", true, "Enables LoadArr handoff behavior."),
            Bool("maintainArrIntegrationEnabled", "MaintainArr integration enabled", true, "Enables MaintainArr readiness consumption."),
            Bool("staffArrIntegrationEnabled", "StaffArr integration enabled", true, "Enables StaffArr people/location reference behavior."),
            Bool("supplyArrIntegrationEnabled", "SupplyArr integration enabled", true, "Enables SupplyArr carrier/supplier/customer reference behavior."),
            Bool("complianceCoreIntegrationEnabled", "Compliance Core integration enabled", true, "Enables Compliance Core signal consumption."),
            Bool("recordArrIntegrationEnabled", "RecordArr integration enabled", true, "Enables RecordArr document handoff behavior."),
            Bool("webhookSubscriptionsEnabled", "Webhook subscriptions enabled", true, "Enables outbound webhook subscription behavior."),
            Enum("integrationFailureBehavior", "Integration failure behavior", "queue", [Opt("retry", "Retry"), Opt("queue", "Queue"), Opt("warn", "Warn"), Opt("block_workflow", "Block workflow")], "Default behavior when integration calls fail."),
            Enum("externalIdMappingPolicy", "External ID mapping policy", "typed_mapping", [Opt("typed_mapping", "Typed mapping"), Opt("source_specific", "Source-specific"), Opt("read_only", "Read-only")], "External ID mapping policy."),
            Bool("lastSuccessfulSyncVisibility", "Last successful sync visibility", true, "Shows last successful sync status in admin health surfaces."),
        ]);

        AddGroup(fields, "overridesApprovals", "Overrides & Approvals", "Override reason, approval, audit, and post-trip review behavior.",
        [
            Bool("overrideReasonRequired", "Override reason required", true, "Requires reason for every override."),
            Multi("overrideCategories", "Override categories", ["readiness", "assignment", "timing"], [Opt("readiness", "Readiness"), Opt("assignment", "Assignment"), Opt("timing", "Timing"), Opt("rate", "Rate"), Opt("document", "Document"), Opt("emergency", "Emergency")], "Allowed override categories."),
            Enum("approvalRequiredBySeverity", "Approval required by severity", "high", [Opt("none", "None"), Opt("medium", "Medium and above"), Opt("high", "High and above"), Opt("critical", "Critical only")], "Severity threshold for approval."),
            Enum("approvalTimeoutBehavior", "Approval timeout behavior", "escalate", [Opt("hold", "Hold"), Opt("escalate", "Escalate"), Opt("auto_reject", "Auto-reject")], "Behavior when approvals time out."),
            Bool("captureApproverPersonId", "Capture approver personId", true, "Captures StaffArr personId for approvers."),
            Bool("captureOverrideSource", "Capture override source", true, "Captures where the override originated."),
            Bool("requirePostTripReviewForOverrides", "Require post-trip review for overrides", true, "Requires post-trip review for overridden trips."),
            Bool("showOverrideBannerOnTrip", "Show override banner on trip", true, "Shows override banner on affected trips."),
        ]);

        AddGroup(fields, "closeout", "Closeout", "Trip closeout checklist and auto-close behavior.",
        [
            Bool("requireAllStopsComplete", "Require all stops complete", true, "Requires all stops complete before closeout."),
            Bool("requireDocumentsComplete", "Require documents complete", true, "Requires configured documents complete before closeout."),
            Bool("requireExceptionsResolved", "Require exceptions resolved", true, "Requires exceptions resolved or formally suppressed before closeout."),
            Bool("requireAccessorialReview", "Require accessorial review", true, "Requires accessorial review before closeout."),
            Bool("requireDetentionReview", "Require detention review", true, "Requires detention review before closeout."),
            Bool("requireMileageConfirmation", "Require mileage confirmation", false, "Requires mileage confirmation before closeout."),
            Bool("requireDriverConfirmation", "Require driver confirmation", false, "Requires driver confirmation before closeout."),
            Bool("requireDispatcherReview", "Require dispatcher review", true, "Requires dispatcher review before closeout."),
            Bool("autoCloseEligibleTrips", "Auto-close eligible trips", false, "Auto-closes trips only when all configured gates pass."),
            Duration("closeoutGracePeriod", "Closeout grace period", 1440, "Grace period before closeout escalation."),
        ]);

        return fields;
    }

    private static void AddGroup(
        List<RoutArrTenantSettingDefinition> target,
        string groupKey,
        string label,
        string description,
        IReadOnlyList<PartialDefinition> fields)
    {
        var order = 0;
        foreach (var field in fields)
        {
            target.Add(new RoutArrTenantSettingDefinition(
                groupKey,
                label,
                description,
                field.SettingKey,
                field.Label,
                field.ValueKind,
                field.PlatformDefaultValue,
                field.HelpText,
                order++,
                field.Options,
                field.MinValue,
                field.MaxValue));
        }
    }

    private sealed record PartialDefinition(
        string SettingKey,
        string Label,
        RoutArrTenantSettingValueKind ValueKind,
        object? PlatformDefaultValue,
        string HelpText,
        IReadOnlyList<RoutArrSettingOptionResponse> Options,
        decimal? MinValue = null,
        decimal? MaxValue = null);

    private static PartialDefinition Bool(string key, string label, bool defaultValue, string help) =>
        new(key, label, RoutArrTenantSettingValueKind.Boolean, defaultValue, help, []);

    private static PartialDefinition Int(string key, string label, int defaultValue, string help, decimal? min = null, decimal? max = null) =>
        new(key, label, RoutArrTenantSettingValueKind.Integer, defaultValue, help, [], min, max);

    private static PartialDefinition Decimal(string key, string label, decimal defaultValue, string help, decimal? min = null, decimal? max = null) =>
        new(key, label, RoutArrTenantSettingValueKind.Decimal, defaultValue, help, [], min, max);

    private static PartialDefinition Text(string key, string label, string defaultValue, string help) =>
        new(key, label, RoutArrTenantSettingValueKind.Text, defaultValue, help, []);

    private static PartialDefinition Enum(string key, string label, string defaultValue, IReadOnlyList<RoutArrSettingOptionResponse> options, string help) =>
        new(key, label, RoutArrTenantSettingValueKind.Enum, defaultValue, help, options);

    private static PartialDefinition Time(string key, string label, string defaultValue, string help) =>
        new(key, label, RoutArrTenantSettingValueKind.Time, defaultValue, help, []);

    private static PartialDefinition Duration(string key, string label, int defaultValue, string help, decimal? min = 0, decimal? max = null) =>
        new(key, label, RoutArrTenantSettingValueKind.DurationMinutes, defaultValue, help, [], min, max);

    private static PartialDefinition Multi(string key, string label, IReadOnlyList<string> defaultValue, IReadOnlyList<RoutArrSettingOptionResponse> options, string help) =>
        new(key, label, RoutArrTenantSettingValueKind.MultiSelect, defaultValue, help, options);

    private static RoutArrSettingOptionResponse Opt(string value, string label) => new(value, label);

    private static IReadOnlyList<RoutArrSettingOptionResponse> TimeZones() =>
    [
        Opt("America/Chicago", "Central time"),
        Opt("America/New_York", "Eastern time"),
        Opt("America/Denver", "Mountain time"),
        Opt("America/Los_Angeles", "Pacific time"),
        Opt("UTC", "UTC"),
    ];

    private static IReadOnlyList<RoutArrSettingOptionResponse> WeekDays() =>
    [
        Opt("monday", "Monday"),
        Opt("tuesday", "Tuesday"),
        Opt("wednesday", "Wednesday"),
        Opt("thursday", "Thursday"),
        Opt("friday", "Friday"),
        Opt("saturday", "Saturday"),
        Opt("sunday", "Sunday"),
    ];

    private static IReadOnlyList<RoutArrSettingOptionResponse> TransportModes() =>
    [
        Opt("private_fleet", "Private fleet"),
        Opt("dedicated_carrier", "Dedicated carrier"),
        Opt("truckload", "Truckload"),
        Opt("ltl", "Less-than-truckload"),
        Opt("parcel", "Parcel"),
        Opt("intermodal", "Intermodal"),
        Opt("rail", "Rail"),
        Opt("drayage", "Drayage"),
        Opt("ocean", "Ocean"),
        Opt("air", "Air"),
        Opt("courier", "Courier"),
        Opt("shuttle", "Shuttle"),
        Opt("internal_transfer", "Internal transfer"),
    ];

    private static IReadOnlyList<RoutArrSettingOptionResponse> PriorityOptions() =>
    [
        Opt("low", "Low"),
        Opt("normal", "Normal"),
        Opt("high", "High"),
        Opt("expedite", "Expedite"),
        Opt("critical", "Critical"),
    ];

    private static IReadOnlyList<RoutArrSettingOptionResponse> AssignmentGuardOptions() =>
    [
        Opt("allow", "Allow"),
        Opt("warn", "Warn"),
        Opt("block", "Block"),
        Opt("require_override", "Require override"),
    ];

    private static IReadOnlyList<RoutArrSettingOptionResponse> ReviewGuardOptions() =>
    [
        Opt("off", "Off"),
        Opt("warn", "Warn"),
        Opt("block", "Block"),
        Opt("require_review", "Require review"),
    ];

    private static IReadOnlyList<RoutArrSettingOptionResponse> DemandFieldOptions() =>
    [
        Opt("sourceReference", "Source reference"),
        Opt("customer", "Customer reference"),
        Opt("origin", "Origin"),
        Opt("destination", "Destination"),
        Opt("requestedPickupWindow", "Requested pickup window"),
        Opt("requestedDeliveryWindow", "Requested delivery window"),
        Opt("transportMode", "Transportation mode"),
        Opt("serviceLevel", "Service level"),
        Opt("equipmentRequirement", "Equipment requirement"),
    ];

    private static IReadOnlyList<RoutArrSettingOptionResponse> TenderFieldOptions() =>
    [
        Opt("acceptedBy", "Accepted by"),
        Opt("acceptedAt", "Accepted at"),
        Opt("carrierReference", "Carrier reference"),
        Opt("rateConfirmation", "Rate confirmation"),
        Opt("insuranceSnapshot", "Insurance snapshot"),
    ];

    private static IReadOnlyList<RoutArrSettingOptionResponse> StopConfirmationFieldOptions() =>
    [
        Opt("timestamp", "Timestamp"),
        Opt("geofence", "Geofence"),
        Opt("proof", "Proof"),
        Opt("signature", "Signature"),
        Opt("photo", "Photo"),
        Opt("reason", "Reason"),
    ];

    private static IReadOnlyList<RoutArrSettingOptionResponse> ExceptionFieldOptions() =>
    [
        Opt("owner", "Owner"),
        Opt("severity", "Severity"),
        Opt("affectedRecord", "Affected trip/stop/demand"),
        Opt("reason", "Reason"),
        Opt("resolutionNote", "Resolution note"),
        Opt("correctiveAction", "Corrective action"),
    ];

    private static IReadOnlyList<RoutArrSettingOptionResponse> NotificationRuleOptions() =>
    [
        Opt("late_eta", "Late ETA"),
        Opt("late_delivery", "Late delivery"),
        Opt("tracking_gap", "Tracking gap"),
        Opt("assignment_block", "Assignment block"),
        Opt("assignment_changed", "Assignment changed"),
        Opt("tender_expiring", "Tender expiring"),
        Opt("eta_change", "ETA change"),
        Opt("arrival", "Arrival"),
        Opt("asset_breakdown", "Asset breakdown"),
        Opt("document_missing", "Document missing"),
    ];

    private static IReadOnlyList<RoutArrSettingOptionResponse> EvidenceOptions() =>
    [
        Opt("event", "Operational event"),
        Opt("document", "Document"),
        Opt("photo", "Photo"),
        Opt("signature", "Signature"),
        Opt("approval", "Approval"),
        Opt("rate_snapshot", "Rate snapshot"),
    ];

    private static IReadOnlyList<RoutArrSettingOptionResponse> DocumentTypeOptions() =>
    [
        Opt("bill_of_lading", "Bill of lading"),
        Opt("proof_of_delivery", "Proof of delivery"),
        Opt("packing_slip", "Packing slip"),
        Opt("scale_ticket", "Scale ticket"),
        Opt("temperature_log", "Temperature log"),
        Opt("rate_confirmation", "Rate confirmation"),
        Opt("lumper_receipt", "Lumper receipt"),
        Opt("accessorial_evidence", "Accessorial evidence"),
    ];
}
