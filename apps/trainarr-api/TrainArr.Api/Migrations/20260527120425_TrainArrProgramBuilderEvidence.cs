using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrProgramBuilderEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_training_evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_evidence_trainarr_training_assignments_Tr~",
                        column: x => x.TrainingAssignmentId,
                        principalTable: "trainarr_training_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_programs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_programs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_program_definitions",
                columns: table => new
                {
                    TrainingProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_program_definitions", x => new { x.TrainingProgramId, x.TrainingDefinitionId });
                    table.ForeignKey(
                        name: "FK_trainarr_training_program_definitions_trainarr_training_def~",
                        column: x => x.TrainingDefinitionId,
                        principalTable: "trainarr_training_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trainarr_training_program_definitions_trainarr_training_pro~",
                        column: x => x.TrainingProgramId,
                        principalTable: "trainarr_training_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evidence_TenantId",
                table: "trainarr_training_evidence",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evidence_TenantId_TrainingAssignmentId_Cr~",
                table: "trainarr_training_evidence",
                columns: new[] { "TenantId", "TrainingAssignmentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evidence_TrainingAssignmentId",
                table: "trainarr_training_evidence",
                column: "TrainingAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_definitions_TrainingDefinitionId",
                table: "trainarr_training_program_definitions",
                column: "TrainingDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_definitions_TrainingProgramId_Sor~",
                table: "trainarr_training_program_definitions",
                columns: new[] { "TrainingProgramId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_programs_TenantId",
                table: "trainarr_training_programs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_programs_TenantId_ProgramKey",
                table: "trainarr_training_programs",
                columns: new[] { "TenantId", "ProgramKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_training_evidence");

            migrationBuilder.DropTable(
                name: "trainarr_training_program_definitions");

            migrationBuilder.DropTable(
                name: "trainarr_training_programs");
        }
    }
}
