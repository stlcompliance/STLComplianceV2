using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffArrTenantSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MetadataJson",
                table: "staffarr_audit_events",
                type: "character varying(16384)",
                maxLength: 16384,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "staffarr_tenant_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayNameFormat = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PreferredNameEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    EmployeeNumberLabel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EmployeeNumberRequired = table.Column<bool>(type: "boolean", nullable: false),
                    EmployeeNumberUniquenessScope = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProfilePhotoEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ContactVisibilityMode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EmergencyContactEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PersonalAddressEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultPersonStatusOnCreate = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequireManagerBeforeActivation = table.Column<bool>(type: "boolean", nullable: false),
                    RequirePositionBeforeActivation = table.Column<bool>(type: "boolean", nullable: false),
                    RequireHomeLocationBeforeActivation = table.Column<bool>(type: "boolean", nullable: false),
                    AllowInactivePeopleToBeAssignedWork = table.Column<bool>(type: "boolean", nullable: false),
                    RehireMatchBehavior = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DeactivationReasonRequired = table.Column<bool>(type: "boolean", nullable: false),
                    AutoRemoveRolesOnDeactivation = table.Column<bool>(type: "boolean", nullable: false),
                    AutoEndTeamAssignmentsOnDeactivation = table.Column<bool>(type: "boolean", nullable: false),
                    OrgHierarchyMode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequireEveryPersonInOrgUnit = table.Column<bool>(type: "boolean", nullable: false),
                    RequireDepartmentUnderSite = table.Column<bool>(type: "boolean", nullable: false),
                    AllowMatrixMembership = table.Column<bool>(type: "boolean", nullable: false),
                    PrimaryAssignmentRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ManagerHierarchyRequired = table.Column<bool>(type: "boolean", nullable: false),
                    AllowSkipLevelManagers = table.Column<bool>(type: "boolean", nullable: false),
                    PreventCircularReporting = table.Column<bool>(type: "boolean", nullable: false),
                    LocationHierarchyMode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequireLocationCode = table.Column<bool>(type: "boolean", nullable: false),
                    LocationCodeUniquenessScope = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AllowOperationalLocations = table.Column<bool>(type: "boolean", nullable: false),
                    AllowAddressableBinsShelves = table.Column<bool>(type: "boolean", nullable: false),
                    AllowMobileLocations = table.Column<bool>(type: "boolean", nullable: false),
                    RequireParentLocationExceptRoot = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedLocationAssignmentBehavior = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RoleAssignmentApprovalRequired = table.Column<bool>(type: "boolean", nullable: false),
                    AllowSelfServiceRoleRequests = table.Column<bool>(type: "boolean", nullable: false),
                    RoleExpirationEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultRoleGrantDurationDays = table.Column<int>(type: "integer", nullable: true),
                    RequireAssignmentReason = table.Column<bool>(type: "boolean", nullable: false),
                    PermissionReviewCadence = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AutoRemoveRolesOnInactivePerson = table.Column<bool>(type: "boolean", nullable: false),
                    AllowDirectPermissions = table.Column<bool>(type: "boolean", nullable: false),
                    PreferRolesOverDirectPermissions = table.Column<bool>(type: "boolean", nullable: false),
                    SiteScopedRoleAssignmentsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    TeamMembershipMode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequireTeamLead = table.Column<bool>(type: "boolean", nullable: false),
                    AllowTemporaryAssignments = table.Column<bool>(type: "boolean", nullable: false),
                    TemporaryAssignmentMaxDurationDays = table.Column<int>(type: "integer", nullable: true),
                    AssignmentEffectiveDatingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    HistoricalAssignmentVisibilityMode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AllowOpenPositions = table.Column<bool>(type: "boolean", nullable: false),
                    IncidentIntakeEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RequireIncidentCategory = table.Column<bool>(type: "boolean", nullable: false),
                    RequireInvolvedPerson = table.Column<bool>(type: "boolean", nullable: false),
                    ManagerNotificationMode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TrainArrRoutingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RetrainingRecommendationThreshold = table.Column<int>(type: "integer", nullable: true),
                    IncidentVisibilityMode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClosureApprovalRequired = table.Column<bool>(type: "boolean", nullable: false),
                    RequiredProfileSectionsCsv = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OptionalProfileSectionsCsv = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CustomProfileFieldsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    FieldVisibilityByRoleEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    FieldEditabilityByRoleEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    FieldReviewRequired = table.Column<bool>(type: "boolean", nullable: false),
                    FieldHistoryEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyManagerOnNewPerson = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnManagerChange = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnRoleGrantRemoval = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyBeforeRoleExpiration = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnInactiveAssignmentConflict = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewRemindersEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DigestFrequency = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AuditProfileChanges = table.Column<bool>(type: "boolean", nullable: false),
                    AuditRoleChanges = table.Column<bool>(type: "boolean", nullable: false),
                    AuditOrgLocationChanges = table.Column<bool>(type: "boolean", nullable: false),
                    RequireChangeReasonForSensitiveEdits = table.Column<bool>(type: "boolean", nullable: false),
                    SoftArchiveOnly = table.Column<bool>(type: "boolean", nullable: false),
                    RecordRetentionHintDays = table.Column<int>(type: "integer", nullable: true),
                    ExportEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    BulkImportEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    BulkImportReviewRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ExposePeopleReferenceApi = table.Column<bool>(type: "boolean", nullable: false),
                    ExposeLocationReferenceApi = table.Column<bool>(type: "boolean", nullable: false),
                    ExposeOrgUnitReferenceApi = table.Column<bool>(type: "boolean", nullable: false),
                    PublishPersonLifecycleEvents = table.Column<bool>(type: "boolean", nullable: false),
                    PublishOrgLocationEvents = table.Column<bool>(type: "boolean", nullable: false),
                    AllowProductOriginatedPersonProposals = table.Column<bool>(type: "boolean", nullable: false),
                    RequireReviewForProductOriginatedProposals = table.Column<bool>(type: "boolean", nullable: false),
                    SnapshotLabelPolicy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_tenant_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_tenant_settings_TenantId",
                table: "staffarr_tenant_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_tenant_settings");

            migrationBuilder.DropColumn(
                name: "MetadataJson",
                table: "staffarr_audit_events");
        }
    }
}
