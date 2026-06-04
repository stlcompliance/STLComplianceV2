using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddContainmentActionsAndDispositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assurarr_containment_actions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    NonconformanceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ActionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AssignedPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedTeamRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SourceProductActionRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationRequired = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_containment_actions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_dispositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    NonconformanceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DispositionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DecisionByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecisionAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Rationale = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RequiredActions = table.Column<string[]>(type: "text[]", nullable: false),
                    ExecutionProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ExecutionObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_dispositions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_containment_actions_TenantId",
                table: "assurarr_containment_actions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_containment_actions_TenantId_NonconformanceRef",
                table: "assurarr_containment_actions",
                columns: new[] { "TenantId", "NonconformanceRef" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_containment_actions_TenantId_Number",
                table: "assurarr_containment_actions",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_containment_actions_TenantId_Status",
                table: "assurarr_containment_actions",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_dispositions_TenantId",
                table: "assurarr_dispositions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_dispositions_TenantId_NonconformanceRef",
                table: "assurarr_dispositions",
                columns: new[] { "TenantId", "NonconformanceRef" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_dispositions_TenantId_Number",
                table: "assurarr_dispositions",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_dispositions_TenantId_Status",
                table: "assurarr_dispositions",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assurarr_containment_actions");

            migrationBuilder.DropTable(
                name: "assurarr_dispositions");
        }
    }
}
