using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrProgramVersionsTrainingMatrix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_training_matrix_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicabilityKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ApplicabilityLabel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TrainingProgramId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequirementLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_matrix_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_matrix_entries_trainarr_training_definiti~",
                        column: x => x.TrainingDefinitionId,
                        principalTable: "trainarr_training_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trainarr_training_matrix_entries_trainarr_training_programs~",
                        column: x => x.TrainingProgramId,
                        principalTable: "trainarr_training_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_program_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_program_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_program_versions_trainarr_training_progra~",
                        column: x => x.TrainingProgramId,
                        principalTable: "trainarr_training_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_program_version_definitions",
                columns: table => new
                {
                    TrainingProgramVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_program_version_definitions", x => new { x.TrainingProgramVersionId, x.TrainingDefinitionId });
                    table.ForeignKey(
                        name: "FK_trainarr_training_program_version_definitions_trainarr_trai~",
                        column: x => x.TrainingDefinitionId,
                        principalTable: "trainarr_training_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trainarr_training_program_version_definitions_trainarr_tra~1",
                        column: x => x.TrainingProgramVersionId,
                        principalTable: "trainarr_training_program_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_matrix_entries_TenantId",
                table: "trainarr_training_matrix_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_matrix_entries_TenantId_ApplicabilityKey_~",
                table: "trainarr_training_matrix_entries",
                columns: new[] { "TenantId", "ApplicabilityKey", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_matrix_entries_TrainingDefinitionId",
                table: "trainarr_training_matrix_entries",
                column: "TrainingDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_matrix_entries_TrainingProgramId",
                table: "trainarr_training_matrix_entries",
                column: "TrainingProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_version_definitions_TrainingDefin~",
                table: "trainarr_training_program_version_definitions",
                column: "TrainingDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_version_definitions_TrainingProgr~",
                table: "trainarr_training_program_version_definitions",
                columns: new[] { "TrainingProgramVersionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_versions_TenantId",
                table: "trainarr_training_program_versions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_versions_TenantId_TrainingProgram~",
                table: "trainarr_training_program_versions",
                columns: new[] { "TenantId", "TrainingProgramId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_versions_TrainingProgramId",
                table: "trainarr_training_program_versions",
                column: "TrainingProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_training_matrix_entries");

            migrationBuilder.DropTable(
                name: "trainarr_training_program_version_definitions");

            migrationBuilder.DropTable(
                name: "trainarr_training_program_versions");
        }
    }
}
