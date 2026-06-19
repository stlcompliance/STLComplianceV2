using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruitingAndApplicationBridge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessLevel",
                table: "staffarr_personnel_documents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RestrictedData",
                table: "staffarr_personnel_documents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RetentionCategory",
                table: "staffarr_personnel_documents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CurrentEmploymentAction",
                table: "staffarr_people",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CurrentEmploymentActionAt",
                table: "staffarr_people",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EligibleForRehire",
                table: "staffarr_people",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "FlsaStatus",
                table: "staffarr_people",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeaveStatus",
                table: "staffarr_people",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionNumber",
                table: "staffarr_people",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkerCategory",
                table: "staffarr_people",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedCandidateId",
                table: "staffarr_employment_application_submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "staffarr_availability_blocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    AvailabilityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DayOfWeekMaskCsv = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StartLocalTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndLocalTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Timezone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EffectiveStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_availability_blocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_availability_blocks_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_benefit_beneficiaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Relationship = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AllocationPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    DesignationType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EffectiveStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_benefit_beneficiaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_benefit_beneficiaries_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_benefit_dependents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Relationship = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    IsStudent = table.Column<bool>(type: "boolean", nullable: false),
                    IsDisabled = table.Column<bool>(type: "boolean", nullable: false),
                    CoverageStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CoverageStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CoverageEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_benefit_dependents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_benefit_dependents_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_benefit_enrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    BenefitType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PlanName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BenefitClass = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CoverageLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EligibilityStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EnrollmentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CarrierExportStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CarrierMemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CarrierGroupId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EffectiveStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OpenEnrollmentYear = table.Column<DateOnly>(type: "date", nullable: true),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_benefit_enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_benefit_enrollments_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_compensation_change_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReasonText = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    OldSnapshot = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    NewSnapshot = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    RequestedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_compensation_change_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_compensation_change_requests_staffarr_people_Perso~",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_compensation_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayBasis = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PayGrade = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PayBand = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StepProgression = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    BaseRate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    AnnualSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    OvertimeEligible = table.Column<bool>(type: "boolean", nullable: false),
                    ShiftDifferentialEligible = table.Column<bool>(type: "boolean", nullable: false),
                    BonusEligible = table.Column<bool>(type: "boolean", nullable: false),
                    AllowanceEligible = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EffectiveStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_compensation_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_compensation_profiles_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_leave_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Timezone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsIntermittent = table.Column<bool>(type: "boolean", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequestedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApprovedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Reason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    PayrollLockStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_leave_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_leave_requests_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_performance_improvement_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TargetDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CheckInCadence = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NextCheckInAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ManagerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    HrOwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Summary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Expectations = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    SuccessCriteria = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_performance_improvement_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_performance_improvement_plans_staffarr_people_Pers~",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_performance_review_cycles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    CycleName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CycleType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SelfReviewDueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ManagerReviewDueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ManagerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    SelfReviewCompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ManagerReviewCompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OverallRating = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PromotionReady = table.Column<bool>(type: "boolean", nullable: false),
                    SuccessionReady = table.Column<bool>(type: "boolean", nullable: false),
                    NextCheckInAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Summary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    DevelopmentPlan = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_performance_review_cycles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_performance_review_cycles_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_recruiting_requisitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequisitionNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    JobCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    JobFamily = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DepartmentRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SiteRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LocationRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HiringManagerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecruiterPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    HeadcountRequested = table.Column<int>(type: "integer", nullable: false),
                    FilledCount = table.Column<int>(type: "integer", nullable: false),
                    OpenDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TargetStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_recruiting_requisitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_recruiting_requisitions_staffarr_people_HiringMana~",
                        column: x => x.HiringManagerPersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staffarr_recruiting_requisitions_staffarr_people_RecruiterP~",
                        column: x => x.RecruiterPersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "staffarr_attendance_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Severity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    PointValue = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RelatedLeaveRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedTimesheetPeriodId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_attendance_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_attendance_events_staffarr_leave_requests_RelatedL~",
                        column: x => x.RelatedLeaveRequestId,
                        principalTable: "staffarr_leave_requests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staffarr_attendance_events_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_attendance_events_staffarr_timesheet_periods_Relat~",
                        column: x => x.RelatedTimesheetPeriodId,
                        principalTable: "staffarr_timesheet_periods",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "staffarr_performance_competency_assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceReviewCycleId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompetencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CompetencyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExpectedLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CurrentLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Rating = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    AssessedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_performance_competency_assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_performance_competency_assessments_staffarr_people~",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_performance_competency_assessments_staffarr_perfor~",
                        column: x => x.PerformanceReviewCycleId,
                        principalTable: "staffarr_performance_review_cycles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "staffarr_performance_feedback_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceReviewCycleId = table.Column<Guid>(type: "uuid", nullable: true),
                    FeedbackType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Visibility = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    Sentiment = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    AuthorPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_performance_feedback_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_performance_feedback_entries_staffarr_people_Perso~",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_performance_feedback_entries_staffarr_performance_~",
                        column: x => x.PerformanceReviewCycleId,
                        principalTable: "staffarr_performance_review_cycles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "staffarr_performance_goals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceReviewCycleId = table.Column<Guid>(type: "uuid", nullable: true),
                    GoalTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GoalType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Priority = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ProgressPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TargetDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    SuccessMetric = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Summary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ResultSummary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_performance_goals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_performance_goals_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staffarr_performance_goals_staffarr_performance_review_cycl~",
                        column: x => x.PerformanceReviewCycleId,
                        principalTable: "staffarr_performance_review_cycles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "staffarr_recruiting_candidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecruitingRequisitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmploymentApplicationSubmissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    CandidateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CandidateEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    CandidatePhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Stage = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BackgroundCheckStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DrugScreenStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PhysicalStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    OfferStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_recruiting_candidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_recruiting_candidates_staffarr_employment_applicat~",
                        column: x => x.EmploymentApplicationSubmissionId,
                        principalTable: "staffarr_employment_application_submissions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staffarr_recruiting_candidates_staffarr_people_PersonId",
                        column: x => x.PersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staffarr_recruiting_candidates_staffarr_recruiting_requisit~",
                        column: x => x.RecruitingRequisitionId,
                        principalTable: "staffarr_recruiting_requisitions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "staffarr_recruiting_interview_stages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecruitingCandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    StageName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    InterviewerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    Recommendation = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_recruiting_interview_stages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_recruiting_interview_stages_staffarr_people_Interv~",
                        column: x => x.InterviewerPersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staffarr_recruiting_interview_stages_staffarr_recruiting_ca~",
                        column: x => x.RecruitingCandidateId,
                        principalTable: "staffarr_recruiting_candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_recruiting_offers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecruitingCandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PayBasis = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AnnualSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    HourlyRate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeclinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SourceProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_recruiting_offers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staffarr_recruiting_offers_staffarr_people_ApprovedByPerson~",
                        column: x => x.ApprovedByPersonId,
                        principalTable: "staffarr_people",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staffarr_recruiting_offers_staffarr_recruiting_candidates_R~",
                        column: x => x.RecruitingCandidateId,
                        principalTable: "staffarr_recruiting_candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_employment_application_submissions_CreatedCandidat~",
                table: "staffarr_employment_application_submissions",
                column: "CreatedCandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_attendance_events_PersonId",
                table: "staffarr_attendance_events",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_attendance_events_RelatedLeaveRequestId",
                table: "staffarr_attendance_events",
                column: "RelatedLeaveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_attendance_events_RelatedTimesheetPeriodId",
                table: "staffarr_attendance_events",
                column: "RelatedTimesheetPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_attendance_events_TenantId_PersonId_EventType_Stat~",
                table: "staffarr_attendance_events",
                columns: new[] { "TenantId", "PersonId", "EventType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_attendance_events_TenantId_PersonId_OccurredAt",
                table: "staffarr_attendance_events",
                columns: new[] { "TenantId", "PersonId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_availability_blocks_PersonId",
                table: "staffarr_availability_blocks",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_availability_blocks_TenantId_PersonId_EffectiveSta~",
                table: "staffarr_availability_blocks",
                columns: new[] { "TenantId", "PersonId", "EffectiveStartDate", "EffectiveEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_availability_blocks_TenantId_PersonId_Status",
                table: "staffarr_availability_blocks",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_benefit_beneficiaries_PersonId",
                table: "staffarr_benefit_beneficiaries",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_benefit_beneficiaries_TenantId_PersonId_Status",
                table: "staffarr_benefit_beneficiaries",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_benefit_dependents_PersonId",
                table: "staffarr_benefit_dependents",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_benefit_dependents_TenantId_PersonId_Relationship",
                table: "staffarr_benefit_dependents",
                columns: new[] { "TenantId", "PersonId", "Relationship" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_benefit_enrollments_PersonId",
                table: "staffarr_benefit_enrollments",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_benefit_enrollments_TenantId_PersonId_BenefitType_~",
                table: "staffarr_benefit_enrollments",
                columns: new[] { "TenantId", "PersonId", "BenefitType", "EnrollmentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_benefit_enrollments_TenantId_PersonId_EnrollmentSt~",
                table: "staffarr_benefit_enrollments",
                columns: new[] { "TenantId", "PersonId", "EnrollmentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_compensation_change_requests_PersonId",
                table: "staffarr_compensation_change_requests",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_compensation_change_requests_TenantId_PersonId_Req~",
                table: "staffarr_compensation_change_requests",
                columns: new[] { "TenantId", "PersonId", "RequestType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_compensation_change_requests_TenantId_PersonId_Sta~",
                table: "staffarr_compensation_change_requests",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_compensation_profiles_PersonId",
                table: "staffarr_compensation_profiles",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_compensation_profiles_TenantId_PersonId_EffectiveS~",
                table: "staffarr_compensation_profiles",
                columns: new[] { "TenantId", "PersonId", "EffectiveStartDate", "EffectiveEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_compensation_profiles_TenantId_PersonId_Status",
                table: "staffarr_compensation_profiles",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_leave_requests_PersonId",
                table: "staffarr_leave_requests",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_leave_requests_TenantId_PersonId_StartDate_EndDate",
                table: "staffarr_leave_requests",
                columns: new[] { "TenantId", "PersonId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_leave_requests_TenantId_PersonId_Status",
                table: "staffarr_leave_requests",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_competency_assessments_PerformanceRevi~",
                table: "staffarr_performance_competency_assessments",
                column: "PerformanceReviewCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_competency_assessments_PersonId",
                table: "staffarr_performance_competency_assessments",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_competency_assessments_TenantId_Perso~1",
                table: "staffarr_performance_competency_assessments",
                columns: new[] { "TenantId", "PersonId", "PerformanceReviewCycleId", "CompetencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_competency_assessments_TenantId_Person~",
                table: "staffarr_performance_competency_assessments",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_feedback_entries_PerformanceReviewCycl~",
                table: "staffarr_performance_feedback_entries",
                column: "PerformanceReviewCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_feedback_entries_PersonId",
                table: "staffarr_performance_feedback_entries",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_feedback_entries_TenantId_PersonId_Cre~",
                table: "staffarr_performance_feedback_entries",
                columns: new[] { "TenantId", "PersonId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_feedback_entries_TenantId_PersonId_Per~",
                table: "staffarr_performance_feedback_entries",
                columns: new[] { "TenantId", "PersonId", "PerformanceReviewCycleId" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_goals_PerformanceReviewCycleId",
                table: "staffarr_performance_goals",
                column: "PerformanceReviewCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_goals_PersonId",
                table: "staffarr_performance_goals",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_goals_TenantId_PersonId_PerformanceRev~",
                table: "staffarr_performance_goals",
                columns: new[] { "TenantId", "PersonId", "PerformanceReviewCycleId" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_goals_TenantId_PersonId_Status",
                table: "staffarr_performance_goals",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_improvement_plans_PersonId",
                table: "staffarr_performance_improvement_plans",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_improvement_plans_TenantId_PersonId_S~1",
                table: "staffarr_performance_improvement_plans",
                columns: new[] { "TenantId", "PersonId", "StartDate", "TargetDate" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_improvement_plans_TenantId_PersonId_St~",
                table: "staffarr_performance_improvement_plans",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_review_cycles_PersonId",
                table: "staffarr_performance_review_cycles",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_review_cycles_TenantId_PersonId_StartD~",
                table: "staffarr_performance_review_cycles",
                columns: new[] { "TenantId", "PersonId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_performance_review_cycles_TenantId_PersonId_Status",
                table: "staffarr_performance_review_cycles",
                columns: new[] { "TenantId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_recruiting_candidates_EmploymentApplicationSubmiss~",
                table: "staffarr_recruiting_candidates",
                column: "EmploymentApplicationSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_recruiting_candidates_PersonId",
                table: "staffarr_recruiting_candidates",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_recruiting_candidates_RecruitingRequisitionId",
                table: "staffarr_recruiting_candidates",
                column: "RecruitingRequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_recruiting_candidates_TenantId_RecruitingRequisiti~",
                table: "staffarr_recruiting_candidates",
                columns: new[] { "TenantId", "RecruitingRequisitionId", "Stage" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_recruiting_interview_stages_InterviewerPersonId",
                table: "staffarr_recruiting_interview_stages",
                column: "InterviewerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_recruiting_interview_stages_RecruitingCandidateId",
                table: "staffarr_recruiting_interview_stages",
                column: "RecruitingCandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_recruiting_interview_stages_TenantId_RecruitingCan~",
                table: "staffarr_recruiting_interview_stages",
                columns: new[] { "TenantId", "RecruitingCandidateId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_recruiting_offers_ApprovedByPersonId",
                table: "staffarr_recruiting_offers",
                column: "ApprovedByPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_recruiting_offers_RecruitingCandidateId",
                table: "staffarr_recruiting_offers",
                column: "RecruitingCandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_recruiting_offers_TenantId_RecruitingCandidateId_S~",
                table: "staffarr_recruiting_offers",
                columns: new[] { "TenantId", "RecruitingCandidateId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_recruiting_requisitions_HiringManagerPersonId",
                table: "staffarr_recruiting_requisitions",
                column: "HiringManagerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_recruiting_requisitions_RecruiterPersonId",
                table: "staffarr_recruiting_requisitions",
                column: "RecruiterPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_recruiting_requisitions_TenantId_RequisitionNumber",
                table: "staffarr_recruiting_requisitions",
                columns: new[] { "TenantId", "RequisitionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_recruiting_requisitions_TenantId_Status_CreatedAt",
                table: "staffarr_recruiting_requisitions",
                columns: new[] { "TenantId", "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_attendance_events");

            migrationBuilder.DropTable(
                name: "staffarr_availability_blocks");

            migrationBuilder.DropTable(
                name: "staffarr_benefit_beneficiaries");

            migrationBuilder.DropTable(
                name: "staffarr_benefit_dependents");

            migrationBuilder.DropTable(
                name: "staffarr_benefit_enrollments");

            migrationBuilder.DropTable(
                name: "staffarr_compensation_change_requests");

            migrationBuilder.DropTable(
                name: "staffarr_compensation_profiles");

            migrationBuilder.DropTable(
                name: "staffarr_performance_competency_assessments");

            migrationBuilder.DropTable(
                name: "staffarr_performance_feedback_entries");

            migrationBuilder.DropTable(
                name: "staffarr_performance_goals");

            migrationBuilder.DropTable(
                name: "staffarr_performance_improvement_plans");

            migrationBuilder.DropTable(
                name: "staffarr_recruiting_interview_stages");

            migrationBuilder.DropTable(
                name: "staffarr_recruiting_offers");

            migrationBuilder.DropTable(
                name: "staffarr_leave_requests");

            migrationBuilder.DropTable(
                name: "staffarr_performance_review_cycles");

            migrationBuilder.DropTable(
                name: "staffarr_recruiting_candidates");

            migrationBuilder.DropTable(
                name: "staffarr_recruiting_requisitions");

            migrationBuilder.DropIndex(
                name: "IX_staffarr_employment_application_submissions_CreatedCandidat~",
                table: "staffarr_employment_application_submissions");

            migrationBuilder.DropColumn(
                name: "AccessLevel",
                table: "staffarr_personnel_documents");

            migrationBuilder.DropColumn(
                name: "RestrictedData",
                table: "staffarr_personnel_documents");

            migrationBuilder.DropColumn(
                name: "RetentionCategory",
                table: "staffarr_personnel_documents");

            migrationBuilder.DropColumn(
                name: "CurrentEmploymentAction",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "CurrentEmploymentActionAt",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "EligibleForRehire",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "FlsaStatus",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "LeaveStatus",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "PositionNumber",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "WorkerCategory",
                table: "staffarr_people");

            migrationBuilder.DropColumn(
                name: "CreatedCandidateId",
                table: "staffarr_employment_application_submissions");
        }
    }
}
