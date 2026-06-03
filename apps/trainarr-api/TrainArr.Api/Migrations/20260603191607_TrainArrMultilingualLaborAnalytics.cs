using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrMultilingualLaborAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_trainarr_training_program_content_references_TenantId_Trai~1",
                table: "trainarr_training_program_content_references");

            migrationBuilder.AddColumn<string>(
                name: "LocaleTag",
                table: "trainarr_training_program_content_references",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "trainarr_training_assignment_labor_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    LaborTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    HoursWorked = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostPerHour = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    LoggedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LoggedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_assignment_labor_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_assignment_labor_entries_trainarr_trainin~",
                        column: x => x.TrainingAssignmentId,
                        principalTable: "trainarr_training_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_content_references_TenantId_Trai~1",
                table: "trainarr_training_program_content_references",
                columns: new[] { "TenantId", "TrainingProgramId", "ContentType", "ReferenceValue", "LocaleTag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_labor_entries_TenantId",
                table: "trainarr_training_assignment_labor_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_labor_entries_TenantId_Trainin~",
                table: "trainarr_training_assignment_labor_entries",
                columns: new[] { "TenantId", "TrainingAssignmentId", "LoggedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_labor_entries_TrainingAssignme~",
                table: "trainarr_training_assignment_labor_entries",
                column: "TrainingAssignmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_training_assignment_labor_entries");

            migrationBuilder.DropIndex(
                name: "IX_trainarr_training_program_content_references_TenantId_Trai~1",
                table: "trainarr_training_program_content_references");

            migrationBuilder.DropColumn(
                name: "LocaleTag",
                table: "trainarr_training_program_content_references");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_content_references_TenantId_Trai~1",
                table: "trainarr_training_program_content_references",
                columns: new[] { "TenantId", "TrainingProgramId", "ContentType", "ReferenceValue" },
                unique: true);
        }
    }
}
