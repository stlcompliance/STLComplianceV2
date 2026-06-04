using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCapaActionsAndVerificationPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assurarr_capa_actions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AssignedPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedTeamRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SourceProductActionRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TargetProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationRequired = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerifiedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    BlockerRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_capa_actions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_verification_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    VerificationType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SuccessCriteria = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SampleSize = table.Column<int>(type: "integer", nullable: true),
                    ObservationPeriodDays = table.Column<int>(type: "integer", nullable: true),
                    RequiredEvidenceTypes = table.Column<string[]>(type: "text[]", nullable: false),
                    ResponsiblePersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlannedVerificationAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_verification_plans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_actions_TenantId",
                table: "assurarr_capa_actions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_actions_TenantId_CapaId",
                table: "assurarr_capa_actions",
                columns: new[] { "TenantId", "CapaId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_actions_TenantId_Number",
                table: "assurarr_capa_actions",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_actions_TenantId_Status",
                table: "assurarr_capa_actions",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_verification_plans_TenantId",
                table: "assurarr_verification_plans",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_verification_plans_TenantId_CapaId",
                table: "assurarr_verification_plans",
                columns: new[] { "TenantId", "CapaId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_verification_plans_TenantId_Number",
                table: "assurarr_verification_plans",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_verification_plans_TenantId_Status",
                table: "assurarr_verification_plans",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assurarr_capa_actions");

            migrationBuilder.DropTable(
                name: "assurarr_verification_plans");
        }
    }
}
