using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffArrTimekeeping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_clock_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceDeviceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceDeviceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EventType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EventTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CapturedTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Timezone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    GeoPoint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SiteRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LocationRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SourceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EnteredByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    AnomalyFlagsCsv = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ImmutableAuditHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_clock_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_labor_allocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocationPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AllocationMinutes = table.Column<int>(type: "integer", nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CostObjectType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CostObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LegalEntityRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SiteRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DepartmentRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CustomerRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    OrderRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AssetRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    WorkOrderRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TripRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RouteRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    WarehouseTaskRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TrainingSessionRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    QualityCaseRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ProjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    GlDimensionSnapshot = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_labor_allocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_labor_evidence_inbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SuggestedPayCodeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    Timezone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SiteRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LocationRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LegalEntityRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CostObjectRefsJson = table.Column<string>(type: "text", nullable: false),
                    Confidence = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    EmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TimeEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    TimesheetPeriodId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConflictDetected = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_labor_evidence_inbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_pay_codes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CountsTowardWorkedHours = table.Column<bool>(type: "boolean", nullable: false),
                    CountsTowardOvertimeBase = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresAllocation = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresReason = table.Column<bool>(type: "boolean", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_pay_codes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_pay_policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    JurisdictionRefs = table.Column<string>(type: "text", nullable: false),
                    RoundingPolicy = table.Column<string>(type: "text", nullable: true),
                    MealBreakPolicy = table.Column<string>(type: "text", nullable: true),
                    RestBreakPolicy = table.Column<string>(type: "text", nullable: true),
                    OvertimePolicy = table.Column<string>(type: "text", nullable: true),
                    DoubleTimePolicy = table.Column<string>(type: "text", nullable: true),
                    HolidayPolicy = table.Column<string>(type: "text", nullable: true),
                    ShiftDifferentialPolicy = table.Column<string>(type: "text", nullable: true),
                    TravelTimePolicy = table.Column<string>(type: "text", nullable: true),
                    StandbyCalloutPolicy = table.Column<string>(type: "text", nullable: true),
                    ApprovalPolicy = table.Column<string>(type: "text", nullable: true),
                    CorrectionPolicy = table.Column<string>(type: "text", nullable: true),
                    AttestationPolicy = table.Column<string>(type: "text", nullable: true),
                    ComplianceRulepackRefs = table.Column<string>(type: "text", nullable: true),
                    EffectiveStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_pay_policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_time_attestations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimesheetPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttestationType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StatementText = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Response = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    AttestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AttestedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDeviceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_time_attestations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_time_corrections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReasonText = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    OldSnapshot = table.Column<string>(type: "text", nullable: false),
                    NewSnapshot = table.Column<string>(type: "text", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ApprovedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_time_corrections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_time_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TimesheetPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    PayCodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayPolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Classification = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SourceConfidence = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PayrollLockStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_time_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_time_exceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimesheetPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TimeEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Severity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ExceptionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ResolutionStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ResolvedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_time_exceptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_timekeeping_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DefaultLegalEntityRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DefaultSiteRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DefaultDepartmentRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DefaultPositionRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DefaultSupervisorPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayPolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayrollEligibilityStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TimeEntryMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OvertimeEligible = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresMealBreakAttestation = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresEndOfShiftAttestation = table.Column<bool>(type: "boolean", nullable: false),
                    AllowMobileClock = table.Column<bool>(type: "boolean", nullable: false),
                    AllowKioskClock = table.Column<bool>(type: "boolean", nullable: false),
                    AllowManualCorrections = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultLaborAllocationTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    EffectiveStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_timekeeping_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_timesheet_periods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollCalendarRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PeriodStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayrollReadyAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TotalWorkedMinutes = table.Column<int>(type: "integer", nullable: false),
                    TotalPaidMinutes = table.Column<int>(type: "integer", nullable: false),
                    TotalUnpaidMinutes = table.Column<int>(type: "integer", nullable: false),
                    OvertimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    ExceptionCount = table.Column<int>(type: "integer", nullable: false),
                    AttestationStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_timesheet_periods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_work_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Timezone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PrimarySourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PrimarySourceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SiteRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LocationRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SupervisorPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    AnomalyFlagsCsv = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CalculatedDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    PaidDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    UnpaidBreakMinutes = table.Column<int>(type: "integer", nullable: false),
                    RequiresReview = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_work_sessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_clock_events_TenantId_PersonId_EventTimestamp",
                table: "staffarr_clock_events",
                columns: new[] { "TenantId", "PersonId", "EventTimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_labor_allocations_TenantId_TimeEntryId",
                table: "staffarr_labor_allocations",
                columns: new[] { "TenantId", "TimeEntryId" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_labor_evidence_inbox_TenantId_IdempotencyKey",
                table: "staffarr_labor_evidence_inbox",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_labor_evidence_inbox_TenantId_PersonId_EmittedAt",
                table: "staffarr_labor_evidence_inbox",
                columns: new[] { "TenantId", "PersonId", "EmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_pay_codes_TenantId_Code",
                table: "staffarr_pay_codes",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_pay_policies_TenantId_Name",
                table: "staffarr_pay_policies",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_time_attestations_TenantId_TimesheetPeriodId_Attes~",
                table: "staffarr_time_attestations",
                columns: new[] { "TenantId", "TimesheetPeriodId", "AttestationType" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_time_corrections_TenantId_PersonId_CreatedAt",
                table: "staffarr_time_corrections",
                columns: new[] { "TenantId", "PersonId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_time_entries_TenantId_PersonId_EntryDate",
                table: "staffarr_time_entries",
                columns: new[] { "TenantId", "PersonId", "EntryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_time_entries_TenantId_TimesheetPeriodId",
                table: "staffarr_time_entries",
                columns: new[] { "TenantId", "TimesheetPeriodId" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_time_exceptions_TenantId_TimesheetPeriodId_Resolut~",
                table: "staffarr_time_exceptions",
                columns: new[] { "TenantId", "TimesheetPeriodId", "ResolutionStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_timekeeping_profiles_TenantId_PersonId",
                table: "staffarr_timekeeping_profiles",
                columns: new[] { "TenantId", "PersonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_timekeeping_profiles_TenantId_WorkerNumber",
                table: "staffarr_timekeeping_profiles",
                columns: new[] { "TenantId", "WorkerNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_timesheet_periods_TenantId_PersonId_PeriodStartDat~",
                table: "staffarr_timesheet_periods",
                columns: new[] { "TenantId", "PersonId", "PeriodStartDate", "PeriodEndDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_timesheet_periods_TenantId_Status_PayrollReadyAt",
                table: "staffarr_timesheet_periods",
                columns: new[] { "TenantId", "Status", "PayrollReadyAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_work_sessions_TenantId_PersonId_SessionDate",
                table: "staffarr_work_sessions",
                columns: new[] { "TenantId", "PersonId", "SessionDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_clock_events");

            migrationBuilder.DropTable(
                name: "staffarr_labor_allocations");

            migrationBuilder.DropTable(
                name: "staffarr_labor_evidence_inbox");

            migrationBuilder.DropTable(
                name: "staffarr_pay_codes");

            migrationBuilder.DropTable(
                name: "staffarr_pay_policies");

            migrationBuilder.DropTable(
                name: "staffarr_time_attestations");

            migrationBuilder.DropTable(
                name: "staffarr_time_corrections");

            migrationBuilder.DropTable(
                name: "staffarr_time_entries");

            migrationBuilder.DropTable(
                name: "staffarr_time_exceptions");

            migrationBuilder.DropTable(
                name: "staffarr_timekeeping_profiles");

            migrationBuilder.DropTable(
                name: "staffarr_timesheet_periods");

            migrationBuilder.DropTable(
                name: "staffarr_work_sessions");
        }
    }
}
