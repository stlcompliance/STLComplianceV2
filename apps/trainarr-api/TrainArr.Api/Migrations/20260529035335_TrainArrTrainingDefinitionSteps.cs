using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrTrainingDefinitionSteps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_training_definition_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    StepType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ConfigJson = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_definition_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_definition_steps_trainarr_training_defini~",
                        column: x => x.TrainingDefinitionId,
                        principalTable: "trainarr_training_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_assignment_step_progress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingDefinitionStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    QuizScorePercent = table.Column<int>(type: "integer", nullable: true),
                    ResponseJson = table.Column<string>(type: "text", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_assignment_step_progress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_assignment_step_progress_trainarr_trainin~",
                        column: x => x.TrainingAssignmentId,
                        principalTable: "trainarr_training_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_trainarr_training_assignment_step_progress_trainarr_traini~1",
                        column: x => x.TrainingDefinitionStepId,
                        principalTable: "trainarr_training_definition_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_step_progress_TenantId",
                table: "trainarr_training_assignment_step_progress",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_step_progress_TenantId_Trainin~",
                table: "trainarr_training_assignment_step_progress",
                columns: new[] { "TenantId", "TrainingAssignmentId", "TrainingDefinitionStepId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_step_progress_TrainingAssignme~",
                table: "trainarr_training_assignment_step_progress",
                column: "TrainingAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_step_progress_TrainingDefiniti~",
                table: "trainarr_training_assignment_step_progress",
                column: "TrainingDefinitionStepId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_steps_TenantId",
                table: "trainarr_training_definition_steps",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_steps_TenantId_TrainingDefinit~",
                table: "trainarr_training_definition_steps",
                columns: new[] { "TenantId", "TrainingDefinitionId", "StepKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_steps_TrainingDefinitionId",
                table: "trainarr_training_definition_steps",
                column: "TrainingDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_training_assignment_step_progress");

            migrationBuilder.DropTable(
                name: "trainarr_training_definition_steps");
        }
    }
}
