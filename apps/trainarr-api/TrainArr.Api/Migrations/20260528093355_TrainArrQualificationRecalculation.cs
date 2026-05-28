using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrQualificationRecalculation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_qualification_recalculation_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationIssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CheckOutcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_qualification_recalculation_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_qualification_recalculation_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationIssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    RulePackKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PreviousOutcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_qualification_recalculation_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_qualification_recalculation_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StalenessHours = table.Column<int>(type: "integer", nullable: false),
                    AutoSuspendOnBlock = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_qualification_recalculation_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_recalculation_runs_TenantId",
                table: "trainarr_qualification_recalculation_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_recalculation_runs_TenantId_Processe~",
                table: "trainarr_qualification_recalculation_runs",
                columns: new[] { "TenantId", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_recalculation_runs_TenantId_Qualific~",
                table: "trainarr_qualification_recalculation_runs",
                columns: new[] { "TenantId", "QualificationIssueId" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_recalculation_states_TenantId",
                table: "trainarr_qualification_recalculation_states",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_recalculation_states_TenantId_Comput~",
                table: "trainarr_qualification_recalculation_states",
                columns: new[] { "TenantId", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_recalculation_states_TenantId_Qualif~",
                table: "trainarr_qualification_recalculation_states",
                columns: new[] { "TenantId", "QualificationIssueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_qualification_recalculation_settings_Tenant~",
                table: "trainarr_tenant_qualification_recalculation_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_qualification_recalculation_runs");

            migrationBuilder.DropTable(
                name: "trainarr_qualification_recalculation_states");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_qualification_recalculation_settings");
        }
    }
}
