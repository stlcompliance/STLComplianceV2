using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrTrainingAssignmentEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_training_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    QualificationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    QualificationName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrIncidentRemediationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignmentReason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    BlockerPublicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_assignments_trainarr_staffarr_incident_re~",
                        column: x => x.StaffarrIncidentRemediationId,
                        principalTable: "trainarr_staffarr_incident_remediations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trainarr_training_assignments_trainarr_training_definitions~",
                        column: x => x.TrainingDefinitionId,
                        principalTable: "trainarr_training_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignments_StaffarrIncidentRemediationId",
                table: "trainarr_training_assignments",
                column: "StaffarrIncidentRemediationId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignments_TenantId",
                table: "trainarr_training_assignments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignments_TenantId_StaffarrIncidentReme~",
                table: "trainarr_training_assignments",
                columns: new[] { "TenantId", "StaffarrIncidentRemediationId" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignments_TenantId_StaffarrPersonId_Cre~",
                table: "trainarr_training_assignments",
                columns: new[] { "TenantId", "StaffarrPersonId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignments_TrainingDefinitionId",
                table: "trainarr_training_assignments",
                column: "TrainingDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definitions_TenantId",
                table: "trainarr_training_definitions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definitions_TenantId_DefinitionKey",
                table: "trainarr_training_definitions",
                columns: new[] { "TenantId", "DefinitionKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_training_assignments");

            migrationBuilder.DropTable(
                name: "trainarr_training_definitions");
        }
    }
}
