using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrRecertificationAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceQualificationIssueId",
                table: "trainarr_training_assignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "trainarr_recertification_assignment_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationIssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_recertification_assignment_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_recertification_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LeadDays = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_recertification_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignments_TenantId_SourceQualificationI~",
                table: "trainarr_training_assignments",
                columns: new[] { "TenantId", "SourceQualificationIssueId" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_recertification_assignment_runs_TenantId",
                table: "trainarr_recertification_assignment_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_recertification_assignment_runs_TenantId_Processed~",
                table: "trainarr_recertification_assignment_runs",
                columns: new[] { "TenantId", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_recertification_assignment_runs_TenantId_Qualifica~",
                table: "trainarr_recertification_assignment_runs",
                columns: new[] { "TenantId", "QualificationIssueId", "Outcome" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_recertification_settings_TenantId",
                table: "trainarr_tenant_recertification_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_recertification_assignment_runs");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_recertification_settings");

            migrationBuilder.DropIndex(
                name: "IX_trainarr_training_assignments_TenantId_SourceQualificationI~",
                table: "trainarr_training_assignments");

            migrationBuilder.DropColumn(
                name: "SourceQualificationIssueId",
                table: "trainarr_training_assignments");
        }
    }
}
