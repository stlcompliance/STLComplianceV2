using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEffectivenessVerifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "EffectivenessVerificationRefs",
                table: "assurarr_capas",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.CreateTable(
                name: "assurarr_effectiveness_verifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerificationPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PerformedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResultSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    MetricResults = table.Column<string[]>(type: "text[]", nullable: false),
                    RecurrenceFound = table.Column<bool>(type: "boolean", nullable: false),
                    FollowUpRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ReopenedCapaRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_effectiveness_verifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_effectiveness_verifications_TenantId",
                table: "assurarr_effectiveness_verifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_effectiveness_verifications_TenantId_CapaId",
                table: "assurarr_effectiveness_verifications",
                columns: new[] { "TenantId", "CapaId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_effectiveness_verifications_TenantId_Number",
                table: "assurarr_effectiveness_verifications",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_effectiveness_verifications_TenantId_Status",
                table: "assurarr_effectiveness_verifications",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assurarr_effectiveness_verifications");

            migrationBuilder.DropColumn(
                name: "EffectivenessVerificationRefs",
                table: "assurarr_capas");
        }
    }
}
